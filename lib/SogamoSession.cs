using System;
using SogamoMiniJSON;
using DateTimeExtensions;
using System.Collections;
using System.Collections.Generic;

public class SogamoSession
{
	private string sessionId;
	public string SessionId {
		get { return this.sessionId; }
		set { this.sessionId = value; }
	}
	
	private string playerId;
	public string PlayerId {
		get { return this.playerId; }
		set { this.playerId = value; }
	}
	
	private string logCollectorURL;
	public string LogCollectorURL {
		get { return this.logCollectorURL; }
		set { this.logCollectorURL = value; }
	}
	
	private string suggestionServerURL;
	public string SuggestionServerURL {
		get { return this.suggestionServerURL; }
		set { this.suggestionServerURL = value; }
	}
	
	private DateTime startDate;
	public DateTime StartDate {
		get { return this.startDate; }
		set { this.startDate = value; }
	}
	
	private bool isOfflineSession;
	public bool IsOfflineSession {
		get { return this.isOfflineSession; }
		set { this.isOfflineSession = value; }
	}
	
	private int gameId;
	public int GameId {
		get { return this.gameId; }
		set { this.gameId = value; }
	}
	
	private List<SogamoEvent> events;
	public List<SogamoEvent> Events {
		get { return this.events; }
		set { this.events = value; }
	}
	
	#region Constructor	
	public SogamoSession (string sessionId, string playerId, int gameId, string logCollectorURL, string suggestionServerURL, bool isOfflineSession)
	{
		this.sessionId = sessionId;
		this.playerId = playerId;
		this.gameId = gameId;
		this.logCollectorURL = logCollectorURL;
		this.suggestionServerURL = suggestionServerURL;
		this.isOfflineSession = isOfflineSession;
		this.startDate = DateTime.Now;
		this.events = new List<SogamoEvent>();
		
		this.Validate();
	}
	#endregion	
	
	private static string SESSION_ID_KEY = "sessionId";
	private static string PLAYER_ID_KEY = "playerId";
	private static string LOG_COLLECTOR_URL_KEY = "logCollectorURL";
	private static string SUGGESTION_SERVER_URL_KEY = "suggestionServerURL";
	private static string GAME_ID_KEY = "gameId";
	private static string START_DATE_KEY = "startDate";
	private static string IS_OFFLINE_SESSION_KEY = "isOfflineSession";
	private static string EVENTS_KEY = "events";
	
	#region Read / Write to Dictionary	
	public static SogamoSession ReadFromDictionary(Dictionary<string, object> sogamoSessionDict)
	{			
		object sessionId;
		if (!sogamoSessionDict.TryGetValue(SESSION_ID_KEY, out sessionId)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "sessionId param is missing from Dictionary!");
		}
		object playerId;
		if (!sogamoSessionDict.TryGetValue(PLAYER_ID_KEY, out playerId)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "playerId param is missing from Dictionary!");
		}		
		object logCollectorURL;
		if (!sogamoSessionDict.TryGetValue(LOG_COLLECTOR_URL_KEY, out logCollectorURL)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "logCollectorURL param is missing from Dictionary!");
		}		
		object suggestionServerURL;
		if (!sogamoSessionDict.TryGetValue(SUGGESTION_SERVER_URL_KEY, out suggestionServerURL)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "suggestionServerURL param is missing from Dictionary!");
		}
		object gameId;
		if (!sogamoSessionDict.TryGetValue(GAME_ID_KEY, out gameId)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "gameId param is missing from Dictionary!");
		}		
		object startDate;
		if (!sogamoSessionDict.TryGetValue(START_DATE_KEY, out startDate)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "startDate param is missing from Dictionary!");
		}		
		object isOfflineSession;
		if (!sogamoSessionDict.TryGetValue(IS_OFFLINE_SESSION_KEY, out isOfflineSession)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "isOfflineSession param is missing from Dictionary!");
		}
		object eventsObject;
		if (!sogamoSessionDict.TryGetValue(EVENTS_KEY, out eventsObject)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "events param is missing from Dictionary!");
		}		
		
		SogamoSession loadedSession = new SogamoSession((string)sessionId, (string)playerId, (int)gameId, 
			(string)logCollectorURL, (string)suggestionServerURL, (bool)isOfflineSession);		
		
		if (eventsObject != null) {
			// Convert the list of event dictionaries into proper SogamoEvent objects
			List<SogamoEvent>eventArray = new List<SogamoEvent>();
			List<object> eventObjectsList = (List<object>)eventsObject;
			foreach (object eventObject in eventObjectsList) {
				Dictionary<string, object> eventDict = (Dictionary<string, object>)eventObject;
				eventArray.Add(SogamoEvent.ReadFromDictionary(eventDict));
			}			

			loadedSession.events = eventArray;
			loadedSession.startDate = (DateTime)startDate;
			
			return loadedSession;
		} else {
			return null;
		}		
	}
	
	public Dictionary<string, object> WriteToDictionary()
	{
		List<object> eventDictionaries = new List<object>();
		foreach (SogamoEvent sogamoEvent in this.events) {
			eventDictionaries.Add(sogamoEvent.WriteToDictionary());
		}
		
		Dictionary <string, object> outputDictionary = new Dictionary<string, object>()
		{
			{SESSION_ID_KEY, this.sessionId},
			{PLAYER_ID_KEY, this.playerId},
			{LOG_COLLECTOR_URL_KEY, this.logCollectorURL},
			{SUGGESTION_SERVER_URL_KEY, this.suggestionServerURL},
			{GAME_ID_KEY, this.gameId},
			{START_DATE_KEY, this.startDate},
			{IS_OFFLINE_SESSION_KEY, this.isOfflineSession},
			{EVENTS_KEY, eventDictionaries}
		};
		
		return outputDictionary;		
	}	
	#endregion
	
	#region Validation
	private void Validate()
	{
		if (string.IsNullOrEmpty(this.sessionId)) {
			throw new ArgumentNullException("Session ID param is null or empty!");
		}
		
		if (string.IsNullOrEmpty(this.playerId)) {
			throw new ArgumentNullException("Player ID param is null or empty!");
		}
		
		if (string.IsNullOrEmpty(this.logCollectorURL)) {
			throw new ArgumentNullException("Log Collector URL param is null or empty!");
		}		
		
		if (string.IsNullOrEmpty(this.suggestionServerURL)) {
			throw new ArgumentNullException("Suggestion Server URL param is null or empty!");
		}		
		
		if (this.startDate == default(DateTime)) {
			throw new ArgumentNullException("Start Date param is invalid!");
		}
		
		if (this.gameId == int.MinValue) {
			throw new ArgumentException("Game ID param is invalid!");
		}
		
		if (this.events == null) {
			throw new ArgumentNullException("Events param is null!");
		}		
	}
	#endregion
	
	#region Convert to JSON
	public List<string> ConvertEventsToJSONList()
	{
		List<string> outputList = new List<string>();
		foreach (SogamoEvent sogamoEvent in this.events) {
			string eventJSONString = ConvertEventToJSONString(sogamoEvent);
			outputList.Add(eventJSONString);
		}
		
		return outputList;
	}
	
	private static string JSON_ACTION_KEY = "action";
	
	private string ConvertEventToJSONString(SogamoEvent sogamoEvent)
	{
		Dictionary<string, object> jsonDict = new Dictionary<string, object>();
		string actionValue = string.Format("{0}.{1}.{2}" ,this.gameId, sogamoEvent.EventName, sogamoEvent.EventIndex);
		jsonDict[JSON_ACTION_KEY] = actionValue;
		
		foreach (KeyValuePair<string, object> item in sogamoEvent.EventParams) {
			jsonDict[item.Key] = ConvertParamToString(item.Value);
		}
		
		return Json.Serialize(jsonDict);
	}
	
	private string ConvertParamToString(object paramObject)
	{			
		string paramString = null;
		
		if (paramObject is string) {
			paramString = (string)paramObject;
		} else if (paramObject is DateTime) {
			DateTime paramDateTime = (DateTime)paramObject;
			paramString = paramDateTime.ToUnixTimestamp().ToString();
		} else {
			paramString = paramObject.ToString();
		}
		
		return paramString;
	}
	#endregion
	
}

namespace DateTimeExtensions
{
    //Extension methods must be defined in a static class 
    public static class DateTimeExtension
	{
		// Extension to convert a DateTime to Unix timestamp
		public static long ToUnixTimestamp(this DateTime dateTime)
		{
			return (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1).ToUniversalTime()).TotalSeconds;			
		}
		
	}
}

