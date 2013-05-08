using System;
using System.Text;
using UnityEngine;
using System.Net.Sockets;

public class SogamoRequest : TcpClient
{	
	public static SogamoResponse PerformGETRequest(string host, string request)
	{
		// Remove any / from the host string
		host = host.TrimEnd('/');
		
		string method = "GET";
		StringBuilder completeRequestString = new StringBuilder();
		completeRequestString.AppendFormat("{0} {1} HTTP/1.1", method, request);
		completeRequestString.AppendFormat("\r\nHost: {0}", host);
		completeRequestString.Append("\r\nConnection: close");
		completeRequestString.Append("\r\n\r\n");
//		Debug.Log("Complete Request String: " + completeRequestString.ToString());
		
		using(SogamoRequest sogamoRequest = new SogamoRequest())
		{
			sogamoRequest.Connect(host, 80);
		
	        using (NetworkStream ns = sogamoRequest.GetStream())
		    {
		        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(ns))
		        {
		            using (System.IO.StreamReader sr = new System.IO.StreamReader(ns))
		            {
		                sw.Write(completeRequestString.ToString());
		                sw.Flush();
		                string rawResponseString = sr.ReadToEnd();
						return new SogamoResponse(rawResponseString);
		            }
		        }
		    }
		}		
	}
	
	public static SogamoResponse PerformPOSTRequest(string host, string endpoint, string postBody)
	{
		// Remove any / from the host string
		host = host.TrimEnd('/');		
		
		string method = "POST";
		StringBuilder completeRequestString = new StringBuilder();
		completeRequestString.AppendFormat("{0} {1} HTTP/1.1", method, endpoint);
		completeRequestString.AppendFormat("\r\nHost: {0}", host);
		completeRequestString.Append("\r\nConnection: close");
		completeRequestString.Append("\r\n\r\n");
		completeRequestString.Append(postBody);
//		Debug.Log("POST Request\n " + completeRequestString.ToString());
		
		using(SogamoRequest sogamoRequest = new SogamoRequest())
		{
			sogamoRequest.Connect(host, 80);
		
	        using (NetworkStream ns = sogamoRequest.GetStream())
		    {
		        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(ns))
		        {
		            using (System.IO.StreamReader sr = new System.IO.StreamReader(ns))
		            {
		                sw.Write(completeRequestString.ToString());
		                sw.Flush();
		                string rawResponseString = sr.ReadToEnd();
						return new SogamoResponse(rawResponseString);
		            }
		        }
		    }
		}			
	}
}

