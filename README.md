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

	SogamoAPI.Instance.StartSession(API_KEY);
Start session with API Key, Player ID (can be either their Facebook ID or an internal Identifier if your app provides one) and Player Details
	
	Dictionary<string, object> userDetails = new Dictionary<string, object>()
	{
		{"username", "test_user"},
		{"firstname", "test"},
		{"lastname", "user"},
		{"email", "test_user@test.com"}
	};	
	SogamoAPI.Instance.StartSession(API_KEY, PLAYER_ID, userDetails);
	
### Javascript ###
Start session with just API Key

	SogamoAPI.Instance.StartSession(API_KEY);
Start session with API Key, Player ID (can be either their Facebook ID or an internal Identifier if your app provides one) and Player Details

	var userDetails : Dictionary.<String, Object> = new Dictionary.<String, Object>();
	userDetails["username"] = "test_user";
	userDetails["firstname"] = "test";
	userDetails["lastname"] = "user";
	userDetails["email"] = "test_user@test.com";
	SogamoAPI.Instance.StartSession(API_KEY, PLAYER_ID, userDetails);
	
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

## Closing a Session ##
Finally, to send the accumulated data to Sogamo, you need to close the session.

###C\# ###

	SogamoAPI.Instance.CloseSession();

### Javascript ###

	SogamoAPI.Instance.CloseSession();
	
## Performance Implications ##

The Sogamo Unity3D plugin runs all of its major functions on a background thread, so it does not affect the performance of your application.

[Final Folder Structure]:https://github.com/zelrealm/sogamo-unity3d-library/raw/master/images/Final%20Folder%20Structure.jpg "Final Folder Structure"
