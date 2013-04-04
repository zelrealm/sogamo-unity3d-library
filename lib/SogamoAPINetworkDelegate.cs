using UnityEngine;
using System.Collections;

public class SogamoAPINetworkDelegate : MonoBehaviour {
	
	public delegate void SogamoAPIAuthenticationRequestCompleteHandler(string successfulResponseString, string errorString);	
	public delegate void SogamoAPIFlushRequestCompleteHandler(string successfulResponseString, string errorString);
	public delegate void SogamoAPISuggestionRequestCompleteHandler(string successfulResponseString, string errorString);
	
	// Use this for initialization
	void Start () 
	{
	
	}
	
	public void StartAuthenticationRequest(string authenticationURL, SogamoAPIAuthenticationRequestCompleteHandler handler)
	{
		StartCoroutine(AuthenticateRequest(authenticationURL, handler));
	}
	
	IEnumerator AuthenticateRequest(string authenticationURL, SogamoAPIAuthenticationRequestCompleteHandler handler)
	{
		WWW authenticationWWW = new WWW(authenticationURL);
		yield return authenticationWWW;
		handler(authenticationWWW.text, authenticationWWW.error);
	}
	
	public void StartFlush(string flushURL, SogamoAPIFlushRequestCompleteHandler handler)
	{
		StartCoroutine(Flush(flushURL, handler));
	}
	
	IEnumerator Flush(string flushURL, SogamoAPIFlushRequestCompleteHandler handler)
	{
		WWW flushWWW = new WWW(flushURL);
		yield return flushWWW;
		handler(flushWWW.text, flushWWW.error);
	}
	
	public void StartSuggestionRequest(string suggestionURL, SogamoAPISuggestionRequestCompleteHandler handler)
	{
		StartCoroutine(SuggestionRequest(suggestionURL, handler));
	}
	
	IEnumerator SuggestionRequest(string suggestionURL, SogamoAPISuggestionRequestCompleteHandler handler)
	{
		WWW suggestionWWW = new WWW(suggestionURL);
		yield return suggestionWWW;
		handler(suggestionWWW.text, suggestionWWW.error);
	}
}
