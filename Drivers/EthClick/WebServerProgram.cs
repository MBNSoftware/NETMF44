using MBN;
using MBN.Modules;
using Microsoft.SPOT;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;
using NetworkingExample.Properties;

namespace NetworkingExample
{

	public class WebServerProgram
	{
		private static DateTime _serverStartTime;
		private static TimeSpan _elapsedTime;
		private static ulong _bytesServed;

		private static EthClick _eth;

		public static void Main()
		{

			_eth = new EthClick(Hardware.SocketOne);

			_eth.Start(_eth.GenerateUniqueMacAddress("MikroBusNet"), "MikroBusNet");


			_eth.Start(_eth.GenerateUniqueMacAddress("MikroBusNet"), "MikroBusNet");

			while (true)
			{
				if (_eth.ConnectedToInternet)
				{
					Debug.Print("Connected to Internet");
					break;
				}
				Debug.Print("Waiting on Internet connection");
			}

			EthClick.OnHttpPacketReceived += EthClick_OnHttpPacketReceived;
			EthClick.OnConnectionChanged += Adapter_OnConnectionChanged;
			EthClick.OnTcpPacketReceived += EthClick_OnTcpReceivedPacketEvent;
			EthClick.OnPingReceived += EthClick_OnPingReceived;	

			_eth.ListenToPort(80); // Listen on Port 80, the default web server port

			_serverStartTime = DateTime.Now;
			
			Thread.Sleep(Timeout.Infinite);

		}

		static void EthClick_OnPingReceived(byte[] originatingMac, byte[] originatingIp, byte[] id, byte[] seq)
		{
			Debug.Print("Ping received from " + originatingIp.ToAddress());
		}

		static void EthClick_OnTcpReceivedPacketEvent(Packet packet)
		{
			try
			{
			var content = new String(Encoding.UTF8.GetChars( packet.Content));
			Debug.Print("TCP Packet received - Packet Type - " + packet.PacketType + ", Sequence Number:  " + packet.SequenceNumber + ", Socket:  " + packet.Socket + " with the content of: " + content);
			}
			catch (Exception ex)
			{
				Debug.Print(ex.Message + " " + ex.InnerException);
			}
		}

		static void Adapter_OnConnectionChanged(bool status)
		{
			Debug.Print("Network connection has changed. It is now " + (status ? "connected" : "not connected."));
		}

		static void EthClick_OnHttpPacketReceived(HttpRequest request)
		{
			Debug.Print("Memory before sending response: " + Debug.GC(true));

			string uAgent = string.Empty;
			HttpResponse resp = null;
            Stream  ms = new MemoryStream();
			var pageToserve = string.Empty;

			foreach (DictionaryEntry entry in request.Headers)
			{
				if (entry.Key as string == "User-Agent")
				{
					string uAgentStr = entry.Value.ToString().ToLower();
					if (uAgentStr.IndexOf("ipad") > 0 || uAgentStr.IndexOf("iphone") > 0)
					{
						uAgent = "Mobile";
						Debug.Print("Found Mobile Browser");
					}
					else
					{
						uAgent = "Desktop";
						Debug.Print("Found Desktop Browser");
					}
				}
			}

			if (request.Path.ToLower() == "/mbn_logo.png")
			{
				ms = new MemoryStream(Resources.GetBytes(Resources.BinaryResources.MBNLogo));
				resp = uAgent.ToLower() == "desktop" ? new HttpResponse(ms, "image/png", "HTTP/1.1 200 OK", "close") : new HttpResponse(ms);
				_bytesServed += (ulong)(ms.Length);
				request.SendResponse(resp);
				return;
			}

			if (request.Path.ToLower() == "/quail.png")
			{
				ms = new MemoryStream(Resources.GetBytes(Resources.BinaryResources.Quail));
				
				resp = uAgent.ToLower() == "desktop" ? new HttpResponse(ms, "image/png", "HTTP/1.1 200 OK", "close") : new HttpResponse(ms);
				_bytesServed += (ulong)(ms.Length);
				request.SendResponse(resp);
				return;
			}

			if ((request.Path.ToLower() == "/") | (request.Path.ToLower() == "/index.html"))
			{
				pageToserve = Resources.GetString(Resources.StringResources.index);
			}
			else if (request.Path.ToLower() == "/serverstatusframe.html")
			{
				GetServerUpTime();
				pageToserve = Resources.GetString(Resources.StringResources.ServerStatusFrame);
				pageToserve = pageToserve.Replace("daysValue", _elapsedTime.Days.ToString());
				pageToserve = pageToserve.Replace("hoursValue", _elapsedTime.Hours.ToString());
				pageToserve = pageToserve.Replace("minutesValue", _elapsedTime.Minutes.ToString());
				pageToserve = pageToserve.Replace("secondsValue", _elapsedTime.Seconds.ToString());
				pageToserve = pageToserve.Replace("bytesServedValue", _bytesServed.ToString("N0"));
				pageToserve = pageToserve.Replace("mbServedValue", ConvertBytesToMegabytes(_bytesServed).ToString("N1"));
			}
			else if (request.Path.ToLower() == "/logoframe.html")
			{
				pageToserve = Resources.GetString(Resources.StringResources.LogoFrame);
			}
			else if (request.Path.ToLower() == "/linksframe.html")
			{
				pageToserve = Resources.GetString(Resources.StringResources.LinksFrame);
			}
			else
			{
				request.SendNotFound();
			}

			byte[] page = Encoding.UTF8.GetBytes(pageToserve);
			var s = new MemoryStream(page);

			_bytesServed += (ulong)(s.Length);

			if (uAgent == "Desktop")
			{
				resp = new HttpResponse(s, "text/html", "HTTP/1.1 200 OK");
			}
			else if (uAgent == "Mobile")
			{
				resp = new HttpResponse(s);
			}

			request.SendResponse(resp);

			s.Dispose();

			Debug.Print("Memory after sending response: " + Debug.GC(true));
		}

		private static void GetServerUpTime()
		{
			_elapsedTime = DateTime.Now - _serverStartTime;
		}

		private static double ConvertBytesToMegabytes(ulong bytes)
		{
			return (bytes / 1024f) / 1024f;
		}

	}

	/// <summary>
	/// General string extensions
	/// </summary>
	public static class StringExtensions
	{

		/// <summary>
		/// Replace all occurrences of the 'find' string with the 'replace' string.
		/// </summary>
		/// <param name="content">Original string to operate on</param>
		/// <param name="find">String to find within the original string</param>
		/// <param name="replace">String to be used in place of the find string</param>
		/// <returns>Final string after all instances have been replaced.</returns>
		public static string Replace(this string content, string find, string replace)
		{
			const int startFrom = 0;
			int findItemLength = find.Length;

			int firstFound = content.IndexOf(find, startFrom);
			var returning = new StringBuilder();

			string workingString = content;

			while ((firstFound = workingString.IndexOf(find, startFrom)) >= 0)
			{
				returning.Append(workingString.Substring(0, firstFound));
				returning.Append(replace);

				// the remaining part of the string.
				workingString = workingString.Substring(firstFound + findItemLength, workingString.Length - (firstFound + findItemLength));
			}

			returning.Append(workingString);

			return returning.ToString();

		}


	}

}
