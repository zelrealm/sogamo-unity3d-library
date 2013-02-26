using System;
using System.Net;

public class SogamoReachability
{
	private static int TIMEOUT = 10; //10s
	
	public static bool IsUriReachable(Uri uri)
	{		
		HttpWebRequest request = (HttpWebRequest) WebRequest.Create(uri);
		request.Timeout = TIMEOUT * 1000;
		request.Method = "HEAD";
		try
		{
		    using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
		    {
				SogamoAPI.Log(SogamoAPI.LogLevel.MESSAGE, "Reachability status code: " + response.StatusCode);
		        return response.StatusCode == HttpStatusCode.OK;
		    }
		}
		catch (WebException exception)
		{
			if (exception.Status == WebExceptionStatus.Timeout) {
				return false;
			} else {
				return true;
			}
		}		
	}
}

