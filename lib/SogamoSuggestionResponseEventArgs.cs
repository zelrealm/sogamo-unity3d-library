using System;

public class SogamoSuggestionResponseEventArgs : EventArgs {
	private string suggestion;
	public string Suggestion {
		get { return suggestion; }
	}
	
	public SogamoSuggestionResponseEventArgs(string suggestion) : base() {
		this.suggestion = suggestion;
		
		this.Validate();
	}
	
	private void Validate()
	{
		if (string.IsNullOrEmpty(this.suggestion)) {
			throw new ArgumentNullException("Suggestion param is null or empty!");
		}
	}
}
