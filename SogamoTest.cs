using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class SogamoTest : MonoBehaviour {
	
	private System.Diagnostics.Stopwatch stopWatch;

	// Use this for initialization
	void Start () {
		stopWatch = new System.Diagnostics.Stopwatch();
		
		Dictionary<string, object> testEventDict = new Dictionary<string, object>()
		{
			{"eventName", "session"},
			{"eventIndex", "login_datetime"},
			{"eventParams", new Dictionary<string, object>()
				{
					{"session_id", "1234"},
					{"game_id", 1024},
					{"player_id", "8304460"},
					{"last_active_datetime", DateTime.Now},
					{"login_datetime", DateTime.Now}
				}
			}
		};
		Dictionary<string, object> testEventDict2 = new Dictionary<string, object>()
		{
			{"eventName", "session"},
			{"eventIndex", "login_datetime"},
			{"eventParams", new Dictionary<string, object>()
				{
					{"session_id", "1235"},
					{"game_id", 1024},
					{"player_id", "8304460"},
					{"last_active_datetime", DateTime.Now},
					{"login_datetime", DateTime.Now}
				}
			}
		};		
		
		Dictionary<string, object> testSessionDict = new Dictionary<string, object>()
		{
			{"sessionId", "1234"},
			{"playerId", "8304460"},
			{"logCollectorURL", "sogamo-data-collector.herokuapp.com/"},
			{"suggestionServerURL", "sogamo-x10.herokuapp.com/"},
			{"gameId", 1024},
			{"startDate", DateTime.Now},
			{"isOfflineSession", true},
			{"events", new List<object>(){ (object)testEventDict }}
		};
		
		Dictionary<string, object> testSessionDict2 = new Dictionary<string, object>()
		{
			{"sessionId", "1235"},
			{"playerId", "8304460"},
			{"logCollectorURL", "sogamo-data-collector.herokuapp.com/"},
			{"suggestionServerURL", "sogamo-x10.herokuapp.com/"},
			{"gameId", 1024},
			{"startDate", DateTime.Now},
			{"isOfflineSession", false},
			{"events", new List<object>(){ (object)testEventDict2 }}
		};		
		
		SogamoSession testSession = SogamoSession.ReadFromDictionary(testSessionDict);
		SogamoSession testSession2 = SogamoSession.ReadFromDictionary(testSessionDict2);
		
		this.StartTimer();
		bool writeSessionTestResult = (testSession.WriteToDictionary() != null);
		this.StopTimer();		
		Debug.Log("Test 1 - SogamoSession Read/Write:\nPassed (" + stopWatch.ElapsedMilliseconds + "ms): " 
			+ writeSessionTestResult);
		
		this.StartTimer();
		bool convertEventsToJsonListTestResult = (testSession.ConvertEventsToJSONList() != null);
		this.StopTimer();		
		Debug.Log("Test 2 - SogamoSession Convert To JSON:\nPassed (" + stopWatch.ElapsedMilliseconds + "ms): " + 
			convertEventsToJsonListTestResult);
		
		string apiDefinitionsFileName = "sogamo_api_definitions.plist";
		string apiDefinitionaFilePath = Application.dataPath + Path.DirectorySeparatorChar + "Plugins" + 
			Path.DirectorySeparatorChar + "Sogamo" + Path.DirectorySeparatorChar + "Resources" + 
				Path.DirectorySeparatorChar + apiDefinitionsFileName;	
		
		this.StartTimer();
		SogamoAPIDefinitions apiDefinitions = new SogamoAPIDefinitions(apiDefinitionaFilePath);
		this.StopTimer();
		Debug.Log("Test 3 - Parsing API Definitions Plist:\nPassed (" + stopWatch.ElapsedMilliseconds + "ms): " 
			+ (apiDefinitions != null));		
						
		Dictionary<string, object> testEventDict3 = new Dictionary<string, object>()
		{
			{"eventName", "session"},
			{"eventIndex", "login_datetime"},
			{"eventParams", new Dictionary<string, object>()
				{
					{"gameId", 2},
					{"player_id", "200"},
					{"session_id", "1234"},
					{"last_active_datetime", DateTime.Now},
					{"login_datetime", DateTime.Now}
				}
			}
		};		
				
		SogamoEvent testEvent = SogamoEvent.ReadFromDictionary(testEventDict3);	
		this.StartTimer();
		bool testEventValid = apiDefinitions.Definitions[testEvent.EventName].ValidateEvent(testEvent);
		this.StopTimer();
		Debug.Log("Test 4 - SogamoEvent Validation Test\nPassed (" + stopWatch.ElapsedMilliseconds + "ms): " 
			+ testEventValid);
		
		Dictionary<string, object> testAuthenticationResponseDict = new Dictionary<string, object>()
		{
			{"session_id", "1234"},
			{"game_id", 200},
			{"player_id", "8304460"},
			{"lc_url", "http://lc_url"},
			{"su_url", "http://su_url"}
		};
		
		this.StartTimer();
		SogamoAuthenticationResponse authenticationResponse = SogamoAuthenticationResponse.ReadFromDictionary(testAuthenticationResponseDict);
		this.StopTimer();
		bool authenticationResponseValid = authenticationResponse != null;
		Debug.Log("Test 5 - SogamoAuthenticationResponse Validation Test\nPassed (" + stopWatch.ElapsedMilliseconds + "ms): " 
			+ authenticationResponseValid);
				
		string sessionId = "4f38af3614434a03af915278b5fc2913";
		string playerId = "8304460";
		this.StartTimer();
		bool authenticationTestResult = SogamoAPI.TestAuthentication(sessionId, playerId);
		this.StopTimer();
		Debug.Log("Test 6 - Authentication Test\nPassed (" + stopWatch.ElapsedMilliseconds + "ms): " + authenticationTestResult);
		
		string sessionsTestDataFilePath = Application.persistentDataPath + Path.DirectorySeparatorChar + "sogamo_sessions_test.xml";		
		List<SogamoSession> sessions = new List<SogamoSession>();
		sessions.Add(testSession);
		sessions.Add(testSession2);
		
		this.StartTimer();
		bool saveSessionsTestResult = SogamoAPI.TestSaveSessions(sessions, sessionsTestDataFilePath);
		this.StopTimer();
		Debug.Log("Test 7 - Sessions Persistence (Saving) Test\n Passed (" + stopWatch.ElapsedMilliseconds + "ms): " 
			+ saveSessionsTestResult);
		
		this.StartTimer();
		bool loadSessionsTestResult = SogamoAPI.TestLoadSessions(sessionsTestDataFilePath);
		this.StopTimer();			
		Debug.Log("Test 8 - Sessions Persistence (Loading) Test\n Passed (" + stopWatch.ElapsedMilliseconds + "ms): " 
			+ loadSessionsTestResult);
		
		this.StartTimer();
		bool offlineConvertionTestResult = SogamoAPI.TestOfflineSessionConversion(sessions, sessionId, playerId);
		this.StopTimer();		
		Debug.Log("Test 9 - Converting Offline Sessions Test\n Passed (" + stopWatch.ElapsedMilliseconds + "ms) : " 
			+ offlineConvertionTestResult);	
		
		this.StartTimer();
		bool flushTestResult = SogamoAPI.TestFlush(sessions);
		this.StopTimer();		
		Debug.Log("Test 10 - Flush Test\nPassed (" + stopWatch.ElapsedMilliseconds + "ms): " + flushTestResult);		
		
		Dictionary<string, object> testEventParams = new Dictionary<string, object>()
		{
			{"inviteId", 1024},
			{"respondedPlayerId", "1024"},
			{"responseDatetime", DateTime.Now},
			{"respondedPlayerStatus", "accepted"},
		};
		SogamoAPI.Instance.TrackEvent("inviteResponse", testEventParams);		
		
		this.StartTimer();
		SogamoAPI.Instance.StartSession(sessionId, playerId, null);		
		this.StopTimer();		
		Debug.Log("Test 11 - Starting a Session. Duration: " + stopWatch.ElapsedMilliseconds + "ms");
				
		// Sleep this thead for 2000ms to allow time for the StartSession to finish executing in the background
		System.Threading.Thread.Sleep(2000);
		
	
		this.StartTimer();
		SogamoAPI.Instance.CloseSession();		
		this.StopTimer();		
		Debug.Log("Test 12 - Closing a Session. Duration: " + stopWatch.ElapsedMilliseconds + "ms");		
		
		this.StartTimer();
		bool suggestionTestResult = SogamoAPI.TestSuggestion(sessionId, "1024", "buy", 
			"sogamo-x10.herokuapp.com");
		this.StopTimer();
		Debug.Log("Test 13 - Suggestion Test\nPassed (" + stopWatch.ElapsedMilliseconds + "ms): " + suggestionTestResult);
		
		this.StartTimer();
		SogamoAPI.SogamoSuggestionResponseEventHandler responseHandler = (eventArgs) => 
		{
			bool suggestionTestAsyncResult = (eventArgs.Suggestion != null);
			Debug.Log("Test 14 - Suggestion (Async) Test\nPassed: " + suggestionTestAsyncResult);
		};		
		SogamoAPI.TestSuggestionAsync(sessionId, "1024", "buy", "sogamo-x10.herokuapp.com", 
			responseHandler);
		this.StopTimer();
		Debug.Log("Test 14 - Suggestion (Async) Test\nCompleted (" + stopWatch.ElapsedMilliseconds + "ms)");
		
//		SogamoAPI.Instance.FlushInterval = 10;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	#region Convenience methods
	private void StartTimer()
	{
		stopWatch.Reset();
		stopWatch.Start();
	}
	
	private void StopTimer()
	{
		stopWatch.Stop();
	}
	
	public static string PrintDictionary(object dictionaryObject)
	{
		Dictionary<string, object> dictionary = (Dictionary<string, object>) dictionaryObject;
		return PrintDictionary(dictionary);
	}
	
	public static string PrintDictionary(Dictionary<string, object> dictionary)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, object> item in dictionary) {
			if (item.Value is Dictionary<string, object>) {
				stringBuilder.Append(item.Key + " : " + item.Value + "\n");
				stringBuilder.Append("	" + PrintDictionary((Dictionary<string, object>)item.Value) + "\n");
			} else {
				stringBuilder.Append(item.Key + " : " + item.Value + "\n");
			}			
		}
		
		return stringBuilder.ToString();
	}
	
	public static string PrintList(List<string> list)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string item in list) {
			stringBuilder.Append(item + "\n");
		}
		
		return stringBuilder.ToString();
	}
	
	public string PrintDefinitions(Dictionary<string, SogamoEventDefinition> definitions)
	{
		StringBuilder stringBuilder = new StringBuilder();
						
		foreach (KeyValuePair<string, SogamoEventDefinition> item in definitions) {
			stringBuilder.Append(item.Key + " : " + item.Value.EventIndex + "\n");
			stringBuilder.Append("	" + PrintParameters(item.Value.Parameters) + "\n");
		}
		
		return stringBuilder.ToString();
	}
	
	public string PrintParameters(Dictionary<string, SogamoEventDefinition.Parameter> parameters)
	{
		StringBuilder stringBuilder = new StringBuilder();
		
		foreach (KeyValuePair<string, SogamoEventDefinition.Parameter> parameterPair in parameters) {
			stringBuilder.AppendFormat("Name: {0}, Required: {1}, Type: {2}\n", parameterPair.Value.Name, 
				parameterPair.Value.Required, parameterPair.Value.Type);
		}
		
		return stringBuilder.ToString();
	}
	#endregion
}
