using System;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

public class SogamoResponse
{
	private int code;
	public int Code {
		get { return this.code; }
	}
	
	private Dictionary<string, string> headers;
	public Dictionary<string, string> Headers {
		get { return this.headers; }
	}
	
	private string responseString;
	public string ResponseString {
		get { return this.responseString; }
	}
	
	private string rawResponseString;
	public string RawResponseString {
		get { return this.rawResponseString; }
	}
	
	public SogamoResponse (string rawResponseString)
	{
		this.rawResponseString = rawResponseString;
		this.headers = new Dictionary<string, string>();
		this.ProcessResponse(rawResponseString);
	}
	
	private void ProcessResponse(string rawResponseString)
	{
		string[] responseLines = rawResponseString.Split(new string[] { "\r\n" }, StringSplitOptions.None);
		
		// Read Status-Line
		string statusLine = responseLines[0]; 
		this.code = Convert.ToInt32(statusLine.Split(new string[] {" "}, StringSplitOptions.None)[1]);
		int indexOfCRLFBreak = Array.IndexOf(responseLines, "");
		
		// Process Headers
		for (int i = 1; i < indexOfCRLFBreak; i++) {
			string header = responseLines[i];
			string[] headerElements = header.Split(new string[] {": "}, StringSplitOptions.None);
			this.headers.Add(headerElements[0], headerElements[1]);
		}
		
		// Process Response String (if any)
		int indexOfDataLine = indexOfCRLFBreak + 1;
		if (indexOfDataLine < responseLines.Length) {
			// Fix for a weird bug where the characters '9d' are inserted in the line above the actual Data
			if (responseLines[indexOfDataLine].Equals("9d")) {
				indexOfDataLine += 1;
				if (indexOfDataLine >= responseLines.Length) {
					throw new Exception("Response is invalid!");
				}
			}
			this.responseString = responseLines[indexOfDataLine];
		}
						
//		Debug.Log("Code: " + this.code);
//		Debug.Log("Headers: " + this.PrintHeaders());
//		Debug.Log("Response String" + this.responseString);
	}
	
	private string PrintHeaders()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, string> header in headers) {
			stringBuilder.AppendFormat("{0}: {1}\n", header.Key, header.Value);
		}
		
		return stringBuilder.ToString();
	}
}

