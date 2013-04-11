using System;
using System.Net.Sockets;

public class SogamoRequest : TcpClient
{
	public static SogamoResponse PerformRequest(string host, string request)
	{
		using(SogamoRequest sogamoRequest = new SogamoRequest())
		{
			sogamoRequest.Connect(host, 80);
		
	        using (NetworkStream ns = sogamoRequest.GetStream())
		    {
		        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(ns))
		        {
		            using (System.IO.StreamReader sr = new System.IO.StreamReader(ns))
		            {
		                sw.Write(request);
		                sw.Flush();
		                string rawResponseString = sr.ReadToEnd();
						return new SogamoResponse(rawResponseString);
		            }
		        }
		    }
		}
	}
}

