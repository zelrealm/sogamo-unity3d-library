using System;

public class SogamoSuggestionResponseEventArgs : EventArgs {

	private SogamoSuggestionResponse suggestionResponse;
	public SogamoSuggestionResponse SuggestionResponse {
		get { return suggestionResponse; }
	}
	
	private Exception error;
	public Exception Error {
		get { return error; }
	}
		
	public SogamoSuggestionResponseEventArgs(SogamoSuggestionResponse suggestionResponse, Exception error) : base() {
		this.suggestionResponse = suggestionResponse;
		this.error = error;
		
		this.Validate();
	}
	
	private void Validate()
	{
		if (this.SuggestionResponse == null && this.Error == null) {
			throw new ArgumentNullException("'SuggestionResponse' and 'Error' param cannot be both null!");
		}
		
		if (this.SuggestionResponse != null && this.Error != null) {
			throw new ArgumentException("Either 'SuggestionResponse' or 'Error' param must be null");
		}
	}
}
