using System;
using System.Collections.Generic;

public class SogamoSuggestionResponse
{			
	private int gameId = int.MinValue;
	public int GameId {
		get { return gameId; }
	}
	
	private string playerId;
	public string PlayerId {
		get { return playerId; }
	}
	
	private string suggestionType;
	public string SuggestionType {
		get { return suggestionType; }
	}
	
	private string suggestion;	
	public string Suggestion {
		get { return suggestion; }
	}
	
	public SogamoSuggestionResponse (int gameId, string playerId, string suggestionType, string suggestion)
	{
		this.gameId = gameId;
		this.playerId = playerId;
		this.suggestionType = suggestionType;
		this.suggestion = suggestion;
		
		this.Validate();
	}
	
	private static string SUGGESTION_RESPONSE_GAME_ID_KEY = "game_id";
	private static string SUGGESTION_RESPONSE_PLAYER_ID_KEY = "player_id";
	private static string SUGGESTION_RESPONSE_SUGGESTION_TYPE_KEY = "suggestion_type";
	private static string SUGGESTION_RESPONSE_SUGGESTION_KEY = "suggestion";
	
	public static SogamoSuggestionResponse ReadFromDictionary(Dictionary<string, object> suggestionResponseDict)
	{
		if (suggestionResponseDict == null) {
			throw new ArgumentException("Suggestion Response Dictionary is null!");
		}
		
		object gameIdObject = null;
		if (!suggestionResponseDict.TryGetValue(SUGGESTION_RESPONSE_GAME_ID_KEY, out gameIdObject)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "game_id param is missing from Dictionary!");
		} else {
			if (gameIdObject is long) {
				long temp = (long)gameIdObject;
				gameIdObject = (int)temp;
			}			
		}	
		
		object playerIdObject = null;
		if (!suggestionResponseDict.TryGetValue(SUGGESTION_RESPONSE_PLAYER_ID_KEY, out playerIdObject)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "player_id param is missing from Dictionary!");
		}	
		
		object suggestionTypeObject = null;
		if (!suggestionResponseDict.TryGetValue(SUGGESTION_RESPONSE_SUGGESTION_TYPE_KEY, out suggestionTypeObject)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "suggestion_type param is missing from Dictionary!");
		}
		
		object suggestionObject = null;
		if (!suggestionResponseDict.TryGetValue(SUGGESTION_RESPONSE_SUGGESTION_KEY, out suggestionObject)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "suggestion param is missing from Dictionary!");
		}
		
		return new SogamoSuggestionResponse((int)gameIdObject, (string)playerIdObject, (string)suggestionTypeObject, 
			(string)suggestionObject);
	}
	
	#region Validation
	private void Validate()
	{		
		if (this.gameId == int.MinValue) {
			throw new ArgumentException("Game ID param is invalid!");
		}		
		
		if (string.IsNullOrEmpty(this.playerId)) {
			throw new ArgumentNullException("Player ID param is null or empty!");
		}		
		
		if (string.IsNullOrEmpty(this.suggestionType)) {
			throw new ArgumentNullException("Suggestion Type param is null or empty!");
		}		
		
		if (string.IsNullOrEmpty(this.suggestion)) {
			throw new ArgumentNullException("Suggestion param is null or empty!");
		}		
	}
	#endregion
}

