using System;
using SogamoPlistUtil;
using System.Collections.Generic;

public class SogamoAPIDefinitions
{
	private Dictionary<string, SogamoEventDefinition> definitions;
	public Dictionary<string, SogamoEventDefinition> Definitions {
		get { return this.definitions; }
	}
	
	private string version;
	public string Version {
		get { return this.version; }
	}
	
	public SogamoAPIDefinitions (string definitionsFilename)
	{
		LoadAPIDefinitionsData(definitionsFilename);
	}
	
	private static string DEFINITIONS_DATA_API_DEFINITIONS_KEY = @"api_definitions";
	private static string DEFINITIONS_VERSION_KEY = @"version";
	
	private void LoadAPIDefinitionsData(string definitionsFilePath)
	{
		if (string.IsNullOrEmpty(definitionsFilePath)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "API Definitions File Path is wrong!");
			return;			
		}
		
		object apiDefinitionsObject = Plist.readPlist(definitionsFilePath);
		// Check parsed plist object is in the expected format
		if (apiDefinitionsObject is Dictionary<string, object>) {
			Dictionary<string, object> apiDefinitionsDict = (Dictionary<string, object>)apiDefinitionsObject;			
			Dictionary<string, object> definitionsDict = (Dictionary<string, object>)apiDefinitionsDict[DEFINITIONS_DATA_API_DEFINITIONS_KEY];			

			Dictionary<string, SogamoEventDefinition> definitions = new Dictionary<string, SogamoEventDefinition>();
			
			// Iterate through each API and convert into a SogamoEventDefinition
			foreach (KeyValuePair<string, object> definitionDictPair in definitionsDict) {
				Dictionary<string, object> sogamoEventDefinitionDict = (Dictionary<string, object>)definitionDictPair.Value;
				definitions[definitionDictPair.Key] = SogamoEventDefinition.ReadFromDictionary(definitionDictPair.Key, sogamoEventDefinitionDict);
			}
						
			this.definitions = definitions;
			this.version = (string)apiDefinitionsDict[DEFINITIONS_VERSION_KEY];			
			SogamoAPI.Log(SogamoAPI.LogLevel.MESSAGE,"Successfully loaded API Definitions!");
		} else {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "API Definitions Plist is in invalid!");
			return;			
		}
	}
	
	public string GetEventIndexForName(string eventName)
	{
		if (this.definitions == null) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "API Definitions Data is missing!");
			return null;
		}
		
		string eventIndex = null;		
		if (this.definitions.ContainsKey(eventName)) {
			SogamoEventDefinition eventDefinition = this.definitions[eventName];
			eventIndex = eventDefinition.EventIndex;
		} else {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "No such event Name!");
		}
		
		return eventIndex;
	}
}

