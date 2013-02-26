using System;
using System.Collections.Generic;

public class SogamoEventDefinition
{
	private string eventName;
	public string EventName {
		get { return this.eventName; }
	}
	
	private string eventIndex;
	public string EventIndex {
		get { return this.eventIndex; }
	}
	
	private Dictionary<string, Parameter> parameters;
	public Dictionary<string, Parameter> Parameters {
		get { return this.parameters; }
	}
	
	private List<string> requiredParams;
	public List<string> RequiredParams {
		get { return this.requiredParams; }
	}
	
	public SogamoEventDefinition (string eventName, string eventIndex, Dictionary<string, Parameter> parameters)
	{	
		this.eventName = eventName;
		this.eventIndex = eventIndex;
		this.parameters = parameters;
		this.requiredParams = this.CreateRequiredParamsList(this.parameters);
		
		this.Validate();
	}
	
	#region Create List of Required Params
	private List<string> CreateRequiredParamsList(Dictionary<string, Parameter> parameters)
	{
		if (parameters == null) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "Event Params is missing!");
		}
		
		List<string> requiredParams = new List<string>();
		
		foreach (KeyValuePair<string, Parameter> parameterPair in parameters) {
			if (parameterPair.Value.Required) {
				requiredParams.Add(parameterPair.Key);
			}
		}
		
		return requiredParams;
	}
	#endregion
	
	#region Read from Dictionary
	private static string DEFINITION_PARAMETERS_KEY = @"parameters";
	private static string DEFINITION_REQUIRED_KEY = @"required";	
	private static string DEFINITION_EVENT_INDEX_KEY = @"event_index";	
	private static string DEFINITION_TYPE_KEY = @"type";	
	
	public static SogamoEventDefinition ReadFromDictionary(string eventName, Dictionary<string, object> sogamoEventDefinitionDict)
	{
		string eventIndex = (string)sogamoEventDefinitionDict[DEFINITION_EVENT_INDEX_KEY];
		Dictionary<string, Parameter> eventParametersDict = new Dictionary<string, Parameter>();
		
		Dictionary<string, object> parametersDict = (Dictionary<string, object>)sogamoEventDefinitionDict[DEFINITION_PARAMETERS_KEY];		
		foreach (KeyValuePair<string, object> parameter in parametersDict) {
			Dictionary<string, object> parameterDict = (Dictionary<string, object>)parameter.Value;
			string parameterName = parameter.Key;
			bool required = (bool)parameterDict[DEFINITION_REQUIRED_KEY];
			string typeString = (string)parameterDict[DEFINITION_TYPE_KEY];
			eventParametersDict[parameterName] = new Parameter(parameterName, required, typeString);	
			
		}
		
		return new SogamoEventDefinition(eventName, eventIndex, eventParametersDict);
	}
	#endregion
	
	#region Validation
	// Validate self
	private void Validate() 
	{
		if (string.IsNullOrEmpty(this.eventName)) {
			throw new ArgumentNullException("Event Name param is null or empty!");
		}
		
		if (string.IsNullOrEmpty(this.eventIndex)) {
			throw new ArgumentNullException("Event Index param is null or empty!");
		}
		
		if (this.parameters == null) {
			throw new ArgumentNullException("Parameters is null!");
		}		
		
		if (this.requiredParams == null) {
			throw new ArgumentNullException("Required Params is null!");
		}
	}
	
	// Validate Event
	public bool ValidateEvent(SogamoEvent sogamoEvent)
	{		
		if (!this.eventName.Equals(sogamoEvent.EventName)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "Incorrect Event Definition being used for validation!");
			return false;			
		}
		
		bool result = true;
		
		// Check if the given event index matches the given event name
		if (!sogamoEvent.EventIndex.Equals(this.eventIndex)) {
			SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "Given Event Index " + sogamoEvent.EventIndex + " is invalid!");
			result = false;
			return result;
		}
				
		// Check each given parameter
		foreach (KeyValuePair<string, object> eventParam in sogamoEvent.EventParams) {
			if (!this.parameters.ContainsKey(eventParam.Key)) {
				SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "" + eventParam.Key + " is not a valid paramter!");
				result = false;
				return result;				
			}
			
			// Check whether the parameter value is of the correct type
			SogamoEventDefinition.Parameter eventDefintionParam = this.parameters[eventParam.Key];
			object eventParamValue = eventParam.Value;
			
			try {
				eventParamValue = Convert.ChangeType(eventParamValue, eventDefintionParam.Type);
			} catch (Exception exception) {
				if (exception is InvalidCastException || exception is FormatException) {
					SogamoAPI.Log(SogamoAPI.LogLevel.ERROR,"Value for parameter " + eventParam.Key + " is not the correct type! Should be " 
						+ eventDefintionParam.Type + ", but instead is " + eventParamValue.GetType());					
				} else {
					SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, exception.ToString());
				}
				result = false;
				return result;
			}
		}
		
		// Check if all required Params are present
		foreach (string requiredParam in this.requiredParams) {
			if (!sogamoEvent.EventParams.ContainsKey(requiredParam)) {
				SogamoAPI.Log(SogamoAPI.LogLevel.ERROR, "Required Parameter: '" + requiredParam + "' is missing!");
				result = false;
				return result;
			}
		}	
		return result;		
	}
	#endregion
	
	#region Parameter Nested Class
	public class Parameter
	{
		private string name;
		public string Name {
			get { return this.name; }
		}
		
		private bool required;
		public bool Required {
			get { return this.required; }
		}
		
		private Type type;
		public Type Type {
			get { return this.type; }
		}
		
		public Parameter(string name, bool required, string typeString)
		{
			this.name = name;
			this.required = required;
			this.type = this.MapPlistClassNameToType(typeString);
			
			this.Validate();
		}
		
		private Type MapPlistClassNameToType(string plistClassName)
		{
			Type mappedType = null;
			switch (plistClassName) {
				case "NSString":
					mappedType = typeof(string);
					break;
				case "NSDate":
					mappedType = typeof(DateTime);
					break;
				case "NSNumber":
					mappedType = typeof(double);
					break;		
			}
			
			return mappedType;
		}		
		
		private void Validate()
		{
			if (string.IsNullOrEmpty(this.name)) {
				throw new ArgumentNullException("Name param is null or empty!");
			}
			
			if (this.type == null) {
				throw new ArgumentNullException("Type param is null!");
			}			
		}
	}
	#endregion
}

