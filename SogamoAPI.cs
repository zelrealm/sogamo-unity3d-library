using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using System.Timers;
using SogamoMiniJSON;
using SogamoPlistUtil;
using System.ComponentModel;
using System.Collections.Generic;

public sealed class SogamoAPI
{
	// Singleton Instance
	private static SogamoAPI instance;	
	
	// Constants
	private string SESSIONS_DATA_FILE_NAME = "sogamo_sessions.xml";
	private string API_DEFINITIONS_FILE_NAME = "sogamo_api_definitions.plist";		
	private static string AUTHENTICATION_SERVER_URL = "http://auth.sogamo.com";
	private static string BATCH_SUFFIX = "batch";	
	private int SESSION_TIME_OUT_PERIOD = 43200;
	
	// Class Variables
	
	private bool hasSessionStarted = false;
	
	private string sessionDataFilePath;		
	private string apiDefinitionsFilePath;
	private int platformId;
	
	private SogamoAPIDefinitions apiDefinitions;
	private SogamoSession currentSession;
	private List<SogamoSession> allSessions;
	
	private Dictionary<string, object> playerDict;
	private string playerId;
	private string apiKey;
	
	private static int MIN_FLUSH_INTERVAL = 0;
	private static int MAX_FLUSH_INTERVAL = 3600;
	
	private Timer flushTimer;
	private DateTime flushTimerStopTime;
	
	private int flushInterval = 0;
	public int FlushInterval {
		get { return this.flushInterval; }
		set { this.SetFlushInterval(value); }
	}
	
	public delegate void SogamoSuggestionResponseEventHandler(SogamoSuggestionResponseEventArgs e);
	   
   	private SogamoAPI()
   	{
		this.sessionDataFilePath = Application.persistentDataPath + Path.DirectorySeparatorChar + SESSIONS_DATA_FILE_NAME;		
		this.apiDefinitionsFilePath = Application.dataPath + Path.DirectorySeparatorChar + "Plugins" + 
			Path.DirectorySeparatorChar + "Sogamo" + Path.DirectorySeparatorChar + "Resources" + 
				Path.DirectorySeparatorChar + API_DEFINITIONS_FILE_NAME;	
		this.flushTimerStopTime = default(DateTime);
		this.platformId = this.GetPlatformId();
		
		try {
			this.apiDefinitions = new SogamoAPIDefinitions(apiDefinitionsFilePath);
			Dictionary<string, object> sessionsData = LoadSessionsData(this.sessionDataFilePath);
			this.ParseSessionsData(sessionsData);
		} catch (Exception exception) {
			SogamoAPI.Log(LogLevel.ERROR, exception.ToString());
		}			
   	}

   	public static SogamoAPI Instance
   	{
		get 
      	{
			if (instance == null) {
				instance = new SogamoAPI();
			}
			return instance; 
      	}
   	}
		
	#region Start / End Session
	public void StartSession(string apiKey)
	{
		string defaultPlayerId = SystemInfo.deviceUniqueIdentifier;
		this.StartSession(apiKey, defaultPlayerId, null);
	}
	
	public void StartSession(string apiKey, string playerId, Dictionary<string, object> playerDict)
	{
		this.apiKey = apiKey;
		this.playerId = playerId;
		this.playerDict = playerDict == null ? new Dictionary<string, object>() : playerDict;		

		this.ValidateStartSession();
			
		BackgroundWorker backgroundWorker = new BackgroundWorker();
		backgroundWorker.DoWork += (sender, e) => 
		{
			this.GetNewSessionIfNeeded();
			ConvertOfflineSessions(this.allSessions, this.apiKey, this.playerId);			
		};
		backgroundWorker.RunWorkerCompleted += (sender, e) => 
		{
			if (e.Error != null) {
				SogamoAPI.Log(LogLevel.ERROR, "(BACKGROUND) " + e.Error);
			}
			this.hasSessionStarted = true;
		};
		backgroundWorker.RunWorkerAsync();		
	}
	
	public void CloseSession()
	{
		if (!this.hasSessionStarted) {
			SogamoAPI.Log(LogLevel.ERROR, "StartSession() must have called first and allowed to finish before CloseSession() can be called!");
			return;
		}
		
		BackgroundWorker backgroundWorker = new BackgroundWorker();
		backgroundWorker.DoWork += (sender, e) => 
		{
			this.Flush();
			bool currentSessionExists = (this.currentSession != null);
			SaveSessionsData(this.allSessions, this.sessionDataFilePath, currentSessionExists);
		};
		backgroundWorker.RunWorkerCompleted += (sender, e) => 
		{
			if (e.Error != null) {
				SogamoAPI.Log(LogLevel.ERROR, "(BACKGROUND) " + e.Error);
			}
		};
		backgroundWorker.RunWorkerAsync();				
	}
	#endregion
	
	#region Set Flush Interval	
	private void SetFlushInterval(int flushInterval)
	{
		// Clamp the flush interval between the MIN and MAX values
	    if (flushInterval < MIN_FLUSH_INTERVAL) {
	        flushInterval = MIN_FLUSH_INTERVAL;
	    } else if (flushInterval > MAX_FLUSH_INTERVAL) {
	        flushInterval = MAX_FLUSH_INTERVAL;
	    }
		
	    // Only update if new value is different
	    if (flushInterval != this.flushInterval) {
	        this.flushInterval = flushInterval;
	        
	        if (this.flushInterval > 0) {
				// Cancel any pre-existing timers
				if (this.flushTimer != null) {
					this.flushTimer.Stop();
					this.flushTimer = null;
					this.flushTimerStopTime = DateTime.Now;										
				}
				
				// Create Periodic Timer
				this.flushTimer = new Timer();
				this.flushTimer.Interval = this.flushInterval * 1000; // Convert into ms
				this.flushTimer.AutoReset = true;
				this.flushTimer.Elapsed += new ElapsedEventHandler(FlushTimerEventHandler);
				this.flushTimer.Start();		
	        }
	    }		
	}	
	#endregion	
	
	#region Event Tracking
	public void TrackEvent(string eventName, Dictionary<string, object> eventParams)
	{
		this.PrivateTrackEvent(eventName, eventParams, this.currentSession);
	}
	#endregion
	
	#region Suggestions
	
	public void GetSuggestionAsync(string suggestionType, SogamoSuggestionResponseEventHandler handler)
	{
		SogamoAPI.GetSuggestionAsync(this.apiKey, this.playerId, this.currentSession.SuggestionServerURL, suggestionType, handler);
	}
	
	public SogamoSuggestionResponse GetSuggestion(string suggestionType)
	{
		return SogamoAPI.GetSuggestion(this.apiKey, this.playerId, suggestionType, this.currentSession.SuggestionServerURL);
	}

	private static void GetSuggestionAsync(string apiKey, string playerId, string suggestionType, 
		string suggestionServerURL, SogamoSuggestionResponseEventHandler handler)
	{
		BackgroundWorker backgroundWorker = new BackgroundWorker();
		backgroundWorker.DoWork += (sender, e) => 
		{
			SogamoSuggestionResponse suggestionResponse = SogamoAPI.GetSuggestion(apiKey, playerId, suggestionType, suggestionServerURL);
			e.Result = suggestionResponse;
		};
		backgroundWorker.RunWorkerCompleted += (sender, e) => 
		{
			if (e.Error != null) {
				SogamoAPI.Log(LogLevel.ERROR, "(BACKGROUND) " + e.Error);
			} else {
				if (e.Result != null) {
					SogamoSuggestionResponse suggestionResponse = e.Result as SogamoSuggestionResponse;
					handler(new SogamoSuggestionResponseEventArgs(suggestionResponse.Suggestion));
				}
			}
		};
		backgroundWorker.RunWorkerAsync();		
	}
	
	private static SogamoSuggestionResponse GetSuggestion(string apiKey, string playerId, string suggestionType, string suggestionServerURL) 
	{
		SogamoSuggestionResponse suggestionResponse = null;
		
		try {
			using (SogamoWebClient client = new SogamoWebClient()) 
			{
				string requestString = string.Format("http://{0}?apiKey={1}&playerId={2}&suggestionType={3}", 
					suggestionServerURL, apiKey, playerId, suggestionType);
				string responseString = client.DownloadString(requestString);
				// Check response string is not empty
				if (!string.IsNullOrEmpty(responseString)) {
					object jsonResponseObject = Json.Deserialize(responseString);
					// Check decided JSON format is as expected, a Dictionary<string, object>
					if (jsonResponseObject is Dictionary<string, object>) {
						// Wrap JSON response into a SogamoSuggesionResponse
						suggestionResponse = SogamoSuggestionResponse.ReadFromDictionary((Dictionary<string, object>)jsonResponseObject);
					}
				}			
			}
		} catch (Exception e) {
			// Failed Request
			SogamoAPI.Log(LogLevel.ERROR, "(Suggestion): " + e);							
		} 
		
		return suggestionResponse;
	}
	#endregion
	
	// Private Methods
	
	#region Validation
	private void ValidateStartSession()
	{
		if (string.IsNullOrEmpty(this.apiKey)) {
			throw new ArgumentNullException("API Key is null or empty!");
		}
		
		if (string.IsNullOrEmpty(this.playerId)) {
			throw new ArgumentNullException("Player ID is null or empty!");
		}	
		
		if (this.playerDict == null) {
			throw new ArgumentNullException("Player Dictionary is null!");
		}		
	}	
	#endregion
	
	#region Authentication
	private static SogamoAuthenticationResponse Authenticate(string apiKey, string playerId)
	{
		SogamoAuthenticationResponse authenticationResponse = null;
		
		try {
			using (SogamoWebClient client = new SogamoWebClient()) 
			{
				string requestString = 
					string.Format("{0}?apiKey={1}&playerId={2}", AUTHENTICATION_SERVER_URL, apiKey, playerId);
				string responseString = client.DownloadString(requestString);
				// Check response string is not empty
				if (!string.IsNullOrEmpty(responseString)) {
					object jsonResponseObject = Json.Deserialize(responseString);
					// Check decided JSON format is as expected, a Dictionary<string, object>
					if (jsonResponseObject is Dictionary<string, object>) {
						// Wrap JSON response into a SogamoAuthenticationResponse
						try {
							authenticationResponse = 
								SogamoAuthenticationResponse.ReadFromDictionary((Dictionary<string, object>)jsonResponseObject);
						} catch (Exception exception) {
							SogamoAPI.Log(LogLevel.ERROR, exception.ToString());
						}						
					}
				}
			}
		} catch (Exception e) {
			// Failed Request
			SogamoAPI.Log(LogLevel.ERROR, "(Authentication) " + e);							
		} 
		
		return authenticationResponse;
	}
	#endregion
	
	#region Track Events
	private void PrivateTrackEvent(string eventName, Dictionary<string, object> eventParams, SogamoSession session)
	{
		SogamoEvent newEvent = this.CreateEvent(eventName, eventParams, session);
		if (newEvent != null) {
			session.Events.Add(newEvent);
			SogamoAPI.Log(LogLevel.MESSAGE, "'" + newEvent.EventName + "' event successfully tracked!");
		} else {
			SogamoAPI.Log(LogLevel.ERROR, "Failed to track '" + eventName + "' event!");
		}
	}
	
	private SogamoEvent CreateEvent(string eventName, Dictionary<string, object> eventParams, SogamoSession session)
	{
		if (session == null) {
			SogamoAPI.Log(LogLevel.ERROR, "An event must have an associated session!");
			return null;
		}
		
		if (this.apiDefinitions == null) {
			SogamoAPI.Log(LogLevel.ERROR, "API Definitions are missing!");
			return null;			
		}
		
		// Check a definition exists for the given event name
		SogamoEventDefinition sogamoEventDefinition = this.apiDefinitions.Definitions[eventName];
		if (sogamoEventDefinition == null) {
			SogamoAPI.Log(LogLevel.ERROR, "No such event name exists!");
			return null;
		}		
				
		// Insert session-wide parameters (if necessary by checking required params list)		
		foreach (string requiredParam in sogamoEventDefinition.RequiredParams) {
			switch (requiredParam) {
				case "session_id":
				case "sessionId":
					eventParams[requiredParam] = session.SessionId;
					break;
				case "game_id":
				case "gameId":
					eventParams[requiredParam] = session.GameId;					
					break;
				case "player_id":
				case "playerId":
					eventParams[requiredParam] = session.PlayerId;
					break;
				case "login_datetime":
					eventParams[requiredParam] = session.StartDate;
					break;
				case "logDatetime":
				case "updatedDatetime":
				case "last_active_datetime":
					eventParams[requiredParam] = DateTime.Now;
					break;				
			}
		}
		
		string eventIndex = this.apiDefinitions.GetEventIndexForName(eventName);
		
		SogamoEvent sogamoEvent = null;
		
		try {
			sogamoEvent = new SogamoEvent(eventName, eventIndex, eventParams);		
		} catch (Exception exception) {
			SogamoAPI.Log(LogLevel.ERROR, exception.ToString());
		}
		
		// Validate event
		if (sogamoEvent != null && sogamoEventDefinition.ValidateEvent(sogamoEvent)) {
			return sogamoEvent;
		} else {
			return null;
		}
	}	
	#endregion
	
	#region Flush / Send Data
	
	private void FlushTimerEventHandler(object sender, ElapsedEventArgs e)
	{
		// If the flushTimerStopTime property has been set, ignore the event 
		if (this.flushTimerStopTime != default(DateTime) && this.flushTimerStopTime < e.SignalTime) {
			this.flushTimerStopTime = default(DateTime);
			return;
		}			
			
		try {			
			this.Flush();
			bool currentSessionExists = (this.currentSession != null);
			SaveSessionsData(this.allSessions, this.sessionDataFilePath, currentSessionExists);		
		} catch (Exception exception) {
			SogamoAPI.Log(LogLevel.ERROR, exception.ToString());
		}
	}
	
	private void Flush() {
		if (this.allSessions == null || this.allSessions.Count == 0) {
			SogamoAPI.Log(LogLevel.WARNING, "No Sessions Data to flush!");
			return;
		}		
		
		SogamoAPI.Flush(this.allSessions);
		
		// Re-insert the current session into the allSessions array if removed
		if (!this.allSessions.Contains(this.currentSession)) {
			// Clear the existing events array in the current session since it has already been flushed
			this.currentSession.Events.Clear();
			this.allSessions.Add(this.currentSession);
		}
	}
	
	private static void Flush(List<SogamoSession> sessions)
	{
		if (sessions == null || sessions.Count == 0) {
			SogamoAPI.Log(LogLevel.WARNING, "No Sessions Data to flush!");
			return;
		}
						
 		SogamoAPI.Log(LogLevel.MESSAGE, "Attemping to flush sessions data...");
 				
		List<SogamoSession> sessionsToRemove = new List<SogamoSession>();
		// Convert each session's event into an array of JSON strings
		foreach (SogamoSession session in sessions) {
			string logCollectorURL = session.LogCollectorURL;
			string flushURLString = string.Format("http://{0}/{1}?", logCollectorURL.TrimEnd(new char[]{'/'}), BATCH_SUFFIX);					
			List<string> jsonEvents = session.ConvertEventsToJSONList();
			
			// if jsonEvents is empty, mark it for removal and skip this loop iteration
			if (jsonEvents.Count == 0) {
				sessionsToRemove.Add(session);
				continue;
			}
			
			StringBuilder urlString = new StringBuilder();
			urlString.Append(flushURLString);
						
			// Add each event as a param to the url string
			for (int i = 0; i < jsonEvents.Count; i++) {				
				string encodedJSONEvent = string.Format("{0}={1}&", i, Uri.EscapeDataString(jsonEvents[i]));
				urlString.Append(encodedJSONEvent);
//				SogamoAPI.Log(LogLevel.MESSAGE ,jsonEvents[i]);
			}
			
			// Delete trailing & symbol
			urlString = urlString.Remove(urlString.Length-1, 1);			
//			SogamoAPI.Log("FINAL URL for Session : " + session.sessionId + " = " + urlString);
			
			// Attempt to send aggregated session data
			try {
				using (SogamoWebClient client = new SogamoWebClient()) 
				{
					client.DownloadString(urlString.ToString());
					sessionsToRemove.Add(session);
					SogamoAPI.Log(LogLevel.MESSAGE, "Session " + session.SessionId + " successfully sent!");
				}				
			} catch (Exception exception) {
				SogamoAPI.Log(LogLevel.ERROR, "(Flushing)" + exception);
				break;
			}
		}
		
		// After successfuly delivery, delete from the sessions array
		if (sessionsToRemove.Count > 0) {
			foreach (SogamoSession sessionToRemove in sessionsToRemove) {
				sessions.Remove(sessionToRemove);
			}
		}				
	}
	
	#endregion
	
	#region Session Creation / Renewal
	private bool HasCurrentSessionExpired()
	{
		if (this.currentSession == null) {
			SogamoAPI.Log(LogLevel.ERROR, "There is no current session");
			return true;
		}
		
		DateTime sessionExpiryDate = this.currentSession.StartDate.AddSeconds(SESSION_TIME_OUT_PERIOD);
		
		return DateTime.UtcNow > sessionExpiryDate.ToUniversalTime();
	}
	
	private void GetNewSessionIfNeeded()
	{
		// If there is an existing session, check to see if it is still valid
		if (this.currentSession != null) {
			if (this.HasCurrentSessionExpired()) {
				SogamoAPI.Log(LogLevel.MESSAGE, ": Current session has expired. Creating a new session...");
				SogamoAuthenticationResponse authenticationResponse = Authenticate(this.apiKey, this.playerId);
				if (authenticationResponse != null) {
					this.currentSession = this.CreateSession(authenticationResponse);	
				} else {
					this.currentSession = this.CreateOfflineSession();
				}
				
				this.playerDict["platform"] = this.platformId;
				this.PrivateTrackEvent("session", this.playerDict, this.currentSession);
				this.allSessions.Add(this.currentSession);
				
			} else {
				SogamoAPI.Log(LogLevel.MESSAGE, "Current session is still valid. No new session key required");
				Dictionary<string, object> playerDict = new Dictionary<string, object>();
				playerDict["platform"] = this.platformId;
				this.PrivateTrackEvent("session", playerDict, this.currentSession);
				SogamoAPI.Log(LogLevel.MESSAGE, "Current session has " + this.currentSession.Events.Count + " events");
			}
		} else {
			SogamoAPI.Log(LogLevel.MESSAGE, "No session detected. Creating a new one...");
			SogamoAuthenticationResponse authenticationResponse = Authenticate(this.apiKey, this.playerId);
			if (authenticationResponse != null) {
				this.currentSession = this.CreateSession(authenticationResponse);	
			} else {
				this.currentSession = this.CreateOfflineSession();
			}			
			
			this.playerDict["platform"] = this.platformId;
			this.PrivateTrackEvent("session", this.playerDict, this.currentSession);
			this.allSessions.Add(this.currentSession);
		}		
	}
	
	private SogamoSession CreateSession(SogamoAuthenticationResponse authenticationResponse)
	{
		string sessionId = authenticationResponse.SessionId;
		string playerId = authenticationResponse.PlayerId;
		int gameId = authenticationResponse.GameId;
		string logCollectorURL = authenticationResponse.LogCollectorURL;
		string suggestionServerURL = authenticationResponse.SuggestionServerURL;		
		bool isOfflineSession = false;
		
		SogamoSession newSession = new SogamoSession(sessionId, playerId, gameId, logCollectorURL, suggestionServerURL, isOfflineSession);
		SogamoAPI.Log(LogLevel.MESSAGE, "Generating Session with ID: " + sessionId);
		return newSession;
	}
	
	private SogamoSession CreateOfflineSession()
	{
		string sessionId = this.GenerateOfflineSessionId();
		string playerId = this.playerId;
		int gameId = int.MinValue;
		string logCollectorURL = "";
		string suggestionServerURL = "";
		bool isOfflineSession = true;		
		
		SogamoSession newSession = new SogamoSession(sessionId, playerId, gameId, logCollectorURL, suggestionServerURL, isOfflineSession);
		SogamoAPI.Log(LogLevel.MESSAGE, "Generating Offline Session with ID: " + sessionId);
		return newSession;
	}
	#endregion
	
	#region Session Persistence	
	
	private static string CURRENT_SESSION_EXISTS_KEY = "currentSessionExists";
	private static string SESSIONS_KEY = "sessions";
	
	private static Dictionary<string, object> LoadSessionsData(string sessionDataFilePath)
	{
//		List<SogamoSession> sessions = new List<SogamoSession>();		
		Dictionary<string, object> sessionsData = null;
		if (File.Exists(sessionDataFilePath)) {			
			object sessionsDataObject = Plist.readPlist(sessionDataFilePath);
			
			if (sessionsDataObject is Dictionary<string, object>) {
				sessionsData = (Dictionary<string, object>)sessionsDataObject;	
			}														
			SogamoAPI.Log(LogLevel.MESSAGE, "Successfully loaded Sessions Data");
		} else {
			SogamoAPI.Log(LogLevel.MESSAGE, "No saved sessions file found!");
		}
		
		return sessionsData;
	}
	
	private void ParseSessionsData(Dictionary<string, object> sessionsData)
	{
		if (sessionsData == null) {
			SogamoAPI.Log(LogLevel.WARNING, "No prior saved sessions!");
			this.allSessions = new List<SogamoSession>();
			return;
		}
		
		try {
			ValidateSessionsData(sessionsData);
			List<SogamoSession> sessions = new List<SogamoSession>();
			List<object> sessionObjects = sessionsData[SESSIONS_KEY] as List<object>;
			foreach (object sessionObject in sessionObjects) {
				SogamoSession session = SogamoSession.ReadFromDictionary((Dictionary<string, object>)sessionObject);
				if (session != null) sessions.Add(session);
			}
			
			this.allSessions = sessions;
			
			bool currentSessionExists = (bool)sessionsData[CURRENT_SESSION_EXISTS_KEY];
			if (currentSessionExists && this.allSessions.Count > 0) {
				this.currentSession = this.allSessions[this.allSessions.Count - 1];
			} else {
				this.currentSession = null;
			}					
		} catch (Exception exception) {
			SogamoAPI.Log(LogLevel.ERROR, "(Sessions Data Validation Error) " + exception.ToString());
		}		
	}
	
	private static void ValidateSessionsData(Dictionary<string, object> sessionsData)
	{
		object sessionsObject;
		if (!sessionsData.TryGetValue(SESSIONS_KEY, out sessionsObject)) {
			throw new Exception("'sessions' value is null!");
		}		
		if (!(sessionsObject is List<object>)) {
			throw new Exception("'sessions' value is wrong object type!");
		}				
		
		object currentSessionExistsObject;
		if (!sessionsData.TryGetValue(CURRENT_SESSION_EXISTS_KEY, out currentSessionExistsObject)) {
			throw new Exception("'currentSessionsExists' value is null!");
		}		
		if (!(currentSessionExistsObject is bool)) {
			throw new Exception("'currentSessionsExists' value is wrong object type!");
		}						
	}
	
	private static void SaveSessionsData(List<SogamoSession> sessions, string sessionDataFilePath, bool currentSessionExists)
	{
		if (sessions == null || sessions.Count == 0) {
			SogamoAPI.Log(LogLevel.ERROR, "No Sessions Data to save!");
			return;
		}
		
		List<object> sessionsList = new List<object>();
		foreach (SogamoSession session in sessions) {
			sessionsList.Add(session.WriteToDictionary());
		}
		
		Dictionary<string, object> sessionsData = new Dictionary<string, object>()
		{
			{SESSIONS_KEY, sessionsList},
			{CURRENT_SESSION_EXISTS_KEY, currentSessionExists}
		};
		
		Plist.writeXml(sessionsData, sessionDataFilePath);
//			SogamoAPI.Log(LogLevel.MESSAGE, "SessionsDataFilePath: " + this.sessionDataFilePath);
		SogamoAPI.Log(LogLevel.MESSAGE, "Sesssions Data saved successfully!");
	}
		
	#endregion
	
	#region Offline Sessions
	private static bool ConvertOfflineSessions(List<SogamoSession> sessions, string apiKey, string playerId)
	{
		if (sessions == null || sessions.Count == 0) {
			SogamoAPI.Log(LogLevel.WARNING, "Sessions must not be empty!");
			return false;
		}
		
		bool result = true;
		bool offlineSessionsExist = false;
		
		foreach (SogamoSession session in sessions) {
			if (session.IsOfflineSession) {
				offlineSessionsExist = true;
				SogamoAuthenticationResponse authenticationResponse = Authenticate(apiKey, playerId);
				if (authenticationResponse != null) {
					session.SessionId = authenticationResponse.SessionId;
					session.GameId = authenticationResponse.GameId;
					session.LogCollectorURL = authenticationResponse.LogCollectorURL;
					session.IsOfflineSession = false;
					
					// Update all the tracked events data
					foreach (SogamoEvent sogamoEvent in session.Events) {
						if (sogamoEvent.EventParams.ContainsKey("session_id")) {
							sogamoEvent.EventParams["session_id"] = session.SessionId;
						} else if (sogamoEvent.EventParams.ContainsKey("sessionId")) {
							sogamoEvent.EventParams["sessionId"] = session.SessionId;
						}
						
						if (sogamoEvent.EventParams.ContainsKey("game_id")) {
							sogamoEvent.EventParams["game_id"] = session.GameId;
						} else if (sogamoEvent.EventParams.ContainsKey("gameId")) {
							sogamoEvent.EventParams["gameId"] = session.GameId;
						}												
					}
					
					SogamoAPI.Log(LogLevel.MESSAGE, "Successfully converted an offline session");
				} else {
					SogamoAPI.Log(LogLevel.WARNING, "Attempt to convert an offline session failed");
					result = false;
					break;
				}
			}
		}
		
		if (offlineSessionsExist == false) {
			SogamoAPI.Log(LogLevel.WARNING, "No Offline sessions to convert!");
		}
		
		return result;
	}
	
	private string GenerateOfflineSessionId()
	{
		Guid newGuid = Guid.NewGuid();
		return newGuid.ToString();
	}
	#endregion
	
	#region Determine Platform ID
	private int GetPlatformId()
	{
		int platformId = int.MinValue;
		switch (Application.platform) {
			case RuntimePlatform.OSXWebPlayer:
			case RuntimePlatform.WindowsWebPlayer:
				platformId = 1;			
				break;
			case RuntimePlatform.IPhonePlayer:
				platformId = 2;
				break;
			case RuntimePlatform.Android:
				platformId = 3;
				break;
			case RuntimePlatform.FlashPlayer:
				platformId = 4;
				break;
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.WindowsEditor:
				platformId = 5;
				break;
		}
		
		return platformId;
	}
	#endregion
		
	#region Logging
	
	public enum LogLevel 
	{
		MESSAGE,
		WARNING,
		ERROR
	}
	
	public static void Log(LogLevel level, string logString)
	{
		#if UNITY_EDITOR
		string combinedLog = string.Format("SOGAMO {0}: {1}", level.ToString(), logString);
    	Debug.Log(combinedLog);
  		#endif		
	}
		
	#endregion
	
	#region Testing
	
	public static bool TestSaveSessions(List<SogamoSession> sessions, string sessionTestDataFilePath)
	{
		bool result = false;
		try {
			SaveSessionsData(sessions, sessionTestDataFilePath, false);
			result = true;
		} catch (Exception exception) {
			SogamoAPI.Log(LogLevel.ERROR, exception.ToString());			
		}		
		
		return result;
	}
	
	public bool TestSaveSessions()
	{
		bool result = false;
		try {
			SaveSessionsData(this.allSessions, this.sessionDataFilePath, this.currentSession != null);
			result = true;
		} catch (Exception exception) {
			SogamoAPI.Log(LogLevel.ERROR, exception.ToString());			
		}		
		
		return result;		
	}
	
	public static bool TestLoadSessions(string sessionTestDataFilePath)
	{
		Dictionary<string, object> sessionsData = LoadSessionsData(sessionTestDataFilePath);
		bool result = false;
		try {
			ValidateSessionsData(sessionsData);
			result = true;
		} catch (Exception exception){
			SogamoAPI.Log(LogLevel.ERROR, exception.ToString());			
		}
		
		return result;
	}
	
	public static bool TestAuthentication(string apiKey, string playerId)
	{
		SogamoAuthenticationResponse authenticationResponse = Authenticate(apiKey, playerId);
		return (authenticationResponse != null);
	}
	
	public static bool TestOfflineSessionConversion(List<SogamoSession> sessions, string apiKey, string playerId)
	{
		bool result = false;
		
		try {
			result = ConvertOfflineSessions(sessions, apiKey, playerId);
		} catch (Exception exception) {
			SogamoAPI.Log(LogLevel.ERROR, exception.ToString());			
		}		
		
		return result;
	}
	
	public static bool TestFlush(List<SogamoSession> sessions)
	{
		SogamoAPI.Flush(sessions);
		return sessions.Count == 0;
	}
	
	public static bool TestSuggestion(string apiKey, string playerId, string suggestionType, string suggestionServerURL)
	{
		SogamoSuggestionResponse suggestionResponse = SogamoAPI.GetSuggestion(apiKey, playerId, suggestionType, suggestionServerURL);
		return suggestionResponse != null;
	}
	
	public static void TestSuggestionAsync(string apiKey, string playerId, string suggestionType, 
		string suggestionServerURL, SogamoSuggestionResponseEventHandler handler)
	{
		SogamoAPI.GetSuggestionAsync(apiKey, playerId, suggestionType, suggestionServerURL, handler);
	}
	
	#endregion
	
	#region WebClient Subclass
  	private class SogamoWebClient : WebClient
    {
		public SogamoWebClient()
        {

        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = 10 * 1000; // 10s
            return w;
        }
    }	
	#endregion
}

