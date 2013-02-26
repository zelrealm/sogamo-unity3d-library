using System;
using System.Collections;
using System.Collections.Generic;

public class SogamoEvent  
{
	private string eventName;
	public string EventName {
		get { return this.eventName; }
	}
	
	private string eventIndex;
	public string EventIndex {
		get { return this.eventIndex; }
	}
	
	private Dictionary<string, object> eventParams;
	public Dictionary<string, object> EventParams {
		get { return this.eventParams; }
	}
	
	#region Constructor	
	public SogamoEvent(string eventName, string eventIndex, Dictionary<string, object> eventParams)
	{
		this.eventName = eventName;
		this.eventIndex = eventIndex;
		this.eventParams = eventParams;
		
		this.Validate();
	}	
	#endregion
	
	private static string EVENT_NAME_KEY = "eventName";
	private static string EVENT_INDEX_KEY = "eventIndex";
	private static string EVENT_PARAMS_KEY = "eventParams";
	
	#region Read / Write to Dictionary	
	public static SogamoEvent ReadFromDictionary(Dictionary<string, object> sogamoEventDict)
	{			
		object eventNameObject;
		if (!sogamoEventDict.TryGetValue(EVENT_NAME_KEY, out eventNameObject)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "eventName param is missing from Dictionary!");
		}
		object eventIndexObject;
		if (!sogamoEventDict.TryGetValue(EVENT_INDEX_KEY, out eventIndexObject)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "eventIndex param is missing from Dictionary!");
		}
		object eventParamsObject;
		if (!sogamoEventDict.TryGetValue(EVENT_PARAMS_KEY, out eventParamsObject)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "eventParams param is missing from Dictionary!");
		}
		
		return new SogamoEvent((string)eventNameObject, (string)eventIndexObject, (Dictionary<string, object>)eventParamsObject);					
	}
	
	public Dictionary<string, object> WriteToDictionary()
	{		
		Dictionary <string, object> outputDictionary = new Dictionary<string, object>()
		{
			{EVENT_NAME_KEY, this.eventName},
			{EVENT_INDEX_KEY, this.eventIndex},
			{EVENT_PARAMS_KEY, this.eventParams},
		};
		
		return outputDictionary;
	}	
	#endregion
	
	#region Validation
	private void Validate() 
	{				
		if (string.IsNullOrEmpty(this.eventIndex)) {
			throw new ArgumentNullException("Event Index param is null or empty!");
		}
		
		if (this.eventParams == null) {
			throw new ArgumentNullException("Event Params is null!");
		}
	}
	#endregion
}
