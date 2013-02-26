using System;
using System.Collections.Generic;

public class SogamoAuthenticationResponse
{
	private string sessionId;
	public string SessionId {
		get { return this.sessionId; }
	}
	
	private string playerId;
	public string PlayerId {
		get { return this.playerId; }
	}
	
	private int gameId = int.MinValue;
	public int GameId {
		get { return this.gameId; }
	}
	
	private string logCollectorURL;
	public string LogCollectorURL {
		get { return this.logCollectorURL; }
	}
	
	private string suggestionServerURL;
	public string SuggestionServerURL {
		get { return this.suggestionServerURL; }
	}
	
	private static string AUTHENTICATION_RESPONSE_SESSION_ID_KEY = "session_id";
	private static string AUTHENTICATION_RESPONSE_GAME_ID_KEY = "game_id";
	private static string AUTHENTICATION_RESPONSE_PLAYER_ID_KEY = "player_id";
	private static string AUTHENTICATION_RESPONSE_LOG_COLLECTOR_URL = "lc_url";
	private static string AUTHENTICATION_RESPONSE_SUGGESTION_SERVER_URL = "su_url";
	
	public SogamoAuthenticationResponse (string sessionId, int gameId, string playerId, string logCollectorURL, 
		string suggestionServerURL) 
	{
		this.sessionId = sessionId;
		this.gameId = gameId;
		this.playerId = playerId;
		this.logCollectorURL = logCollectorURL;
		this.suggestionServerURL = suggestionServerURL;
		
		this.Validate();
	}
	
	public static SogamoAuthenticationResponse ReadFromDictionary(Dictionary<string, object> authenticationResponseDict)
	{
		if (authenticationResponseDict == null) {
			throw new ArgumentNullException("Authentication Response Dictionary is null!");
		}
				
		object sessionIdObject = null;
		if (!authenticationResponseDict.TryGetValue(AUTHENTICATION_RESPONSE_SESSION_ID_KEY, out sessionIdObject)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "session_id param is missing from Dictionary!");
		}
		object gameIdObject = null;
		if (!authenticationResponseDict.TryGetValue(AUTHENTICATION_RESPONSE_GAME_ID_KEY, out gameIdObject)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "game_id param is missing from Dictionary!");			
		} else {
			if (gameIdObject is long) {
				long temp = (long)gameIdObject;
				gameIdObject = (int)temp;
			} 
		}					
		
		object playerIdObject = null;
		if (!authenticationResponseDict.TryGetValue(AUTHENTICATION_RESPONSE_PLAYER_ID_KEY, out playerIdObject)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "player_id param is missing from Dictionary!");
		}		
		object logCollectorURLObject = null;
		if (!authenticationResponseDict.TryGetValue(AUTHENTICATION_RESPONSE_LOG_COLLECTOR_URL, out logCollectorURLObject)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "lc_url param is missing from Dictionary!");
		}		
		object suggestionServerURLObject = null;
		if (!authenticationResponseDict.TryGetValue(AUTHENTICATION_RESPONSE_SUGGESTION_SERVER_URL, out suggestionServerURLObject)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "su_url param is missing from Dictionary!");
		}	
		
		return new SogamoAuthenticationResponse((string)sessionIdObject, (int)gameIdObject, (string)playerIdObject, 
			(string)logCollectorURLObject, (string)suggestionServerURLObject);
	}
	
	#region Validation
	private void Validate()
	{		
		if (string.IsNullOrEmpty(this.sessionId)) {
			throw new ArgumentNullException("Session ID param is null or empty!");
		}
		
		if (string.IsNullOrEmpty(this.playerId)) {
			throw new ArgumentNullException("Player ID param is null or empty!");
		}
		
		if (this.gameId == int.MinValue) {
			throw new ArgumentException("Game ID param is invalid!");
		}
		
		if (string.IsNullOrEmpty(this.logCollectorURL)) {
			throw new ArgumentNullException("Log Collector URL param is null or empty!");
		}
		
		if (string.IsNullOrEmpty(this.suggestionServerURL)) {
			throw new ArgumentNullException("Suggestion Server URL param is null or empty!");
		}
	}
	#endregion
}


