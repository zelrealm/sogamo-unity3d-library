# Sogamo Analytics API Unity3D Plugin #

To integrate the Sogamo Analytics API Plugin first download the latest zip archive or clone the repo.

# Requirements #
1. Unity3d 3.5.5 or above

# Setup #
Adding the Sogamo Unity3D plugin to your project is just 2 easy steps:

1. Create a folder named 'Sogamo' inside your 'Plugins' folder. If your project does not have a 'Plugins' folder, just create one under your 'Assets' folder. 
2. Copy the contents from the zip archive / git repo into the newly created 'Sogamo' Folder. 
	
![Final Folder Structure][Final Folder Structure]

And that's it!

# Usage #
## Starting a Session ##

The first thing you need to do is to initialize a SogamoAPI session with your  API key.

###C\# ###
Start session with just API Key

	string apiKey = YOUR_API_KEY;
	SogamoAPI.Instance.StartSession(apiKey);
Start session with API Key, Player ID (can be either their Facebook ID or an internal Identifier if your app provides one) and Player Details
	
	string playerId = A_PLAYER_ID;
	Dictionary<string, object> userDetails = new Dictionary<string, object>()
	{
		{"username", "test_user"},
		{"firstname", "test"},
		{"lastname", "user"},
		{"email", "test_user@test.com"}
	};	
	SogamoAPI.Instance.StartSession(apiKey, playerId, userDetails);
	
### Javascript ###
Start session with just API Key

	var apiKey : String = YOUR_API_KEY;
	SogamoAPI.Instance.StartSession(apiKey);
Start session with API Key, Player ID (can be either their Facebook ID or an internal Identifier if your app provides one) and Player Details

	var playerId = A_PLAYER_ID;
	var userDetails : Dictionary.<String, Object> = new Dictionary.<String, Object>();
	userDetails["username"] = "test_user";
	userDetails["firstname"] = "test";
	userDetails["lastname"] = "user";
	userDetails["email"] = "test_user@test.com";
	
	SogamoAPI.Instance.StartSession(apiKey, playerId, userDetails);
	
## Tracking Events ##
After starting a session, you are ready to track events. This can be done with the following method:

###C\# ###

		Dictionary<string, object> testEventParams = new Dictionary<string, object>()
		{
			{"inviteId", 1024},
			{"respondedPlayerId", "1024"},
			{"responseDatetime", DateTime.Now},
			{"respondedPlayerStatus", "accepted"},
		};
		SogamoAPI.Instance.TrackEvent("inviteResponse", testEventParams);
		
Note: Event params are to be wrapped in a Dictionary<string, object>. Datetime parameters are to be represented as DateTime objects.

For a full list of the events that can be tracked, visit the [Sogamo website](http://www.sogamo.com)
		
### Javascript ###

		var testEventParams : Dictionary.<String, Object> = new Dictionary.<String, Object>();		
		testEventParams["inviteId"] = 1024;
		testEventParams["respondedPlayerId"] = "1024";
		testEventParams["responseDatetime"] = DateTime.Now;
		testEventParams["respondedPlayerStatus"] = "accepted";		
		SogamoAPI.Instance.TrackEvent("inviteResponse", testEventParams);

## Sending Data ##
Event Data is _flushed_ (i.e transmitted) to the Sogamo server at several points:

- When closing a session
- Whenever the periodic flush is triggered (if it is enabled)
- If the **Flush()** method is called

### Closing a Session ###
Note: You must close a session first if you intend to start a new session with a different API Key.

####C\# ####

	SogamoAPI.Instance.CloseSession();

#### Javascript ####

	SogamoAPI.Instance.CloseSession();
	
### Periodic Flush ###
When the periodic flush is enabled, SogamoAPI will flush accumulated event data at the specified intervals.

_Note: The value for the FlushInterval must be in seconds, and between 0 (the default) and 3600._

####C\# ####

	SogamoAPI.Instance.FlushInterval = 30; // Event Data will be flushed every 30s

#### Javascript ####

	SogamoAPI.Instance.FlushInterval = 30; // Event Data will be flushed every 30s
	
### Flush() ###
For those who prefer more direct control, use the Flush() method. 
**StartSession() must have been called and be allowed to complete before attempting to call Flush()**

_Note: This method runs asynchronously, so it will return immediately._

####C\# ####

	SogamoAPI.Instance.Flush();

#### Javascript ####

	SogamoAPI.Instance.Flush();
	
## Suggestions ##
There are two ways to request suggestions via the SogamoAPI.

### Synchronous Request ###
The GetSuggestion method is a synchronous call and will therefore block the main thread. This is not the recommended way to request suggestions, but it included here as it is the only option for javascript users. Refer to the 'Asynchronous Request' section below.

If the request is unsuccessful, the GetSuggestion() method will return null. **Always check if the return value is null before using it.**

####C\# ####

	string suggestionType = A_SUGGESTION_TYPE;
	SogamoSuggestionResponse suggestionResponse = SogamoAPI.Instance.GetSuggestion(suggestionType);
	if (suggestionResponse != null) {
		string suggestion = suggestionResponse.Suggestion;
	}

#### Javascript ####

	var suggestionType : String = A_SUGGESTION_TYPE;
	var suggestionResponse : SogamoSuggestionResponse = SogamoAPI.Instance.GetSuggestion(suggestionType);
	if (suggestionResponse != null) {
		var suggestion : String = suggestionResponse.Suggestion;
	}
	
### Asynchronous Request ###
The GetSuggestionAsync method is (as the name suggests) asynchronous and uses a delegate to handle the callback. 

If the asynchronous request is unsucessful, the Error property of eventArgs will contain the relevant Exception. If the request suceeds, the SuggestionResponse property will contain a valid SogamoSuggestionResponse.

**Always check if the request failed (by checking the Error property) before trying to access the SuggestionResponse property.**

_Note: This option is not available to javascript users._

####C\# ####

	string suggestionType = A_SUGGESTION_TYPE;
	SogamoAPI.SogamoSuggestionResponseEventHandler responseHandler = (eventArgs) => 
	{
		if (eventArgs.Error != null) {
			Debug.Log("Suggestion Request Error:" + eventArgs.Error);
			// Handle the failed request
		} else {
			string suggestion = eventArgs.SuggestionResponse.Suggestion;
			// Handle the successful request
		}		
	};	
		
	SogamoAPI.Instance.GetSuggestionAsync(suggestionType, responseHandler);

## Performance Implications ##

The Sogamo Unity3D plugin runs all of its major functions (Start / Closing a session, flushing data) on a background thread, so it does not affect the performance of your application.

[Final Folder Structure]:https://github.com/zelrealm/sogamo-unity3d-library/raw/master/images/Final%20Folder%20Structure.jpg "Final Folder Structure"
