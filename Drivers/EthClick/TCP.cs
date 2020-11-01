/*
 * Eth Click board driver for MikroBus.Net
 * 
 * Version 1.0 :
 *  - Initial version coded by Stephen Cardinale
 *  
 *  - Based in part on mIP - the Managed TCP/IP Stack.
 *		Hosted on CodePlex: http://mip.codeplex.com
 *		mIP is free software licensed under the Apache License 2.0
 *		© Copyright 2012 ValkyrieTech, LLC
 * 
 * References needed :
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Native
 *  MikroBusNet
 *  mscorlib
 *  
 * Copyright 2014 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace MBN.Modules
{

	internal class TCP
	{
		#region Fields

		private const ushort ConnectionIdleLimit = 30; // Thirty seconds idle limit for stale connections.

		// ToDo - was public
        internal static Hashtable Connections
        {
            get { return _connections; }
        }

		#endregion

		#region Internal Methods

		// ToDo - Current limit for 10 concurrent connections?
		internal static readonly Hashtable _connections = new Hashtable(10);

		internal static void HandlePacket(byte[] payload)
		{
			// ReSharper disable InconsistentNaming
			bool SYN = (payload[47] & (1 << 1)) != 0;
			var ACK = (payload[47] & (1 << 4)) != 0;
			bool FIN = (payload[47] & (1 << 0)) != 0;
			bool PSH = (payload[47] & (1 << 3)) != 0;
			bool RST = (payload[47] & (1 << 2)) != 0;
			// ReSharper restore InconsistentNaming

			byte[] sourceIp = Utility.ExtractRangeFromArray(payload, 26, 4);
			byte[] sourcePort = Utility.ExtractRangeFromArray(payload, 34, 2);
			byte[] localPort = Utility.ExtractRangeFromArray(payload, 36, 2);
			ulong connectionId = GenerateConnectionId(payload);
			Connection con;
			uint packetSeqNumber = Utility.ExtractRangeFromArray(payload, 38, 4).ToInt();

			if (SYN && !ACK)
			{
				var connection = _connections[connectionId] as Connection;
				if (connection != null && (_connections.Contains(connectionId) && connection.IsOpen))
					_connections.Remove(connectionId);

				var keys = new ulong[_connections.Count];
				_connections.Keys.CopyTo(keys, 0);

				foreach (ulong key in keys)
				{
					con = _connections[key] as Connection;

					var connection1 = _connections[key] as Connection;
					if (connection1 != null &&
					    Utility.GetMachineTime().Subtract(connection1.LastActivity).Seconds > ConnectionIdleLimit)
					{
						con.IsClosing = true;
						con.SeqNumber++;
						con.SendAck(false, true);
						_connections.Remove(key);
					}
				}

				if (!_connections.Contains(connectionId)) _connections.Add(connectionId, new Connection());

				con = _connections[connectionId] as Connection;

				if (con != null)
				{
					con.RemoteIp = sourceIp;
					con.RemotePort = sourcePort.ToShort();
					con.RemoteMac = Utility.ExtractRangeFromArray(payload, 6, 6);
					con.LocalPort = localPort.ToShort();
					con.SeqNumber = packetSeqNumber + 1;
					con.StartSeqNumber = packetSeqNumber;
					con.AckNumber = 2380;
					con.WindowSize = 1024;

					con.ReadyForRequest = true;
					con.SendAck(true);

					con.AckNumber++;
					con.IsOpen = true;
				}
			}
			else if (_connections.Contains(connectionId) && (ACK || FIN || PSH || RST))
			{
				con = _connections[connectionId] as Connection;

				ushort totalLength = Utility.ExtractRangeFromArray(payload, 16, 2).ToShort();
				var ipHeaderLength = (ushort) ((payload[14] & 0x0f)*4);
				var tcpHeaderLength = (ushort) ((payload[26 + ipHeaderLength] >> 4)*4);

				if (totalLength + 14 > payload.Length)
				{
					#if TINYCLR_TRACE
					if (EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("Bad packet size detected?  " + totalLength + "/" + payload.Length);
					#endif
					return;
				}


				if (con != null)
				{
					con.SeqNumber += (uint) (totalLength - (tcpHeaderLength + ipHeaderLength));
					con.WindowSize -= (ushort) (totalLength - (tcpHeaderLength + ipHeaderLength));

					if (PSH)
					{
						con.SendAck();
					}
					else if (SYN)
					{
						con.SeqNumber = packetSeqNumber + 1;
						con.StartSeqNumber = packetSeqNumber;
						con.AckNumber++;
						con.SendAck();
						con.IsOpen = true;
						return;
					}
					else if ((FIN || RST) && ACK)
					{
						con.IsClosing = true;
						con.SeqNumber++;
						con.SendAck();
						_connections.Remove(connectionId);
						return;
					}
					else if (FIN)
					{
						con.IsClosing = true;
						con.SeqNumber++;
						con.SendAck(false, true);
						return;
					}
					else if (RST)
					{
						con.IsClosing = true;
						con.SeqNumber++;
						return;
					}
					else if (con.IsClosing) 
					{
						_connections.Remove(connectionId);
						return;
					}

					#if TINYCLR_TRACE 
					if (EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("Check for data");
					#endif

					if ((totalLength - (tcpHeaderLength + ipHeaderLength)) > 0)
					{
						byte[] segment = Utility.ExtractRangeFromArray(payload, (14 + ipHeaderLength + tcpHeaderLength),
							(totalLength - (tcpHeaderLength + ipHeaderLength)));

					#if TINYCLR_TRACE
								if (EthClick._verboseDebugging)
							System.Diagnostics.Debug.WriteLine("We got some data, PSN: " + packetSeqNumber + ", SSN: " + con.StartSeqNumber +
											", header delimiter: " + segment.Locate(HttpRequest.HeaderDelimiter));
					#endif	

						EthClick.FireTcpPacketEvent(segment, packetSeqNumber - con.StartSeqNumber, con);

						con.FireOnConnectionPacketReceived(new Packet(PacketType.TCP)
						{
							SequenceNumber = packetSeqNumber - con.StartSeqNumber,
							Content = segment,
							Socket = con
						});

						if (segment.Length < 10 ||
						    !(segment[0] == 0x47 && segment[1] == 0x45 && segment[2] == 0x54) &&
						    !(segment[0] == 0x50 && segment[1] == 0x4F && segment[2] == 0x53 && segment[3] == 0x54))
							return; // if it is not a get, then we won't handle it through the HTTP Request Handler

						if (con.ReadyForRequest)
						{
							byte[] lrc = Utility.ExtractRangeFromArray(payload, (30 + ipHeaderLength), 2);

							if (con.LastRequestChecksum.BytesEqual(lrc))
							{
								#if TINYCLR_TRACE
								if (EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("Retransmission of Request Ignored!");
								#endif	
							}
							else
							{
								con.LastRequestChecksum = lrc;
								con.ReadyForRequest = false;
								EthClick.FireHttpPacketEvent(segment, con);
							}
						}
					}
				}
			}
			else if ((FIN || RST) && ACK)
			{
				#if TINYCLR_TRACE 
				if (EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("Handling RST for a connection that no longer exists!!!!!!!!!"); 
				#endif

				con = new Connection
				{
					RemoteIp = sourceIp,
					RemotePort = sourcePort.ToShort(),
					RemoteMac = Utility.ExtractRangeFromArray(payload, 6, 6),
					LocalPort = localPort.ToShort(),
					SeqNumber = Utility.ExtractRangeFromArray(payload, 38, 4).ToInt(),
					AckNumber = Utility.ExtractRangeFromArray(payload, 42, 4).ToInt(),
					IsClosing = true
				};

				con.SendAck();
			}
		}

		internal static ulong GenerateConnectionId(byte[] packet)
		{
			return
				Utility.CombineArrays(
					Utility.CombineArrays(Utility.ExtractRangeFromArray(packet, 26, 4), Utility.ExtractRangeFromArray(packet, 34, 2)),
					Utility.ExtractRangeFromArray(packet, 36, 2)).ToLong();
		}

		#endregion
	}

	/// <summary>
	/// The class containing the Socket connection information and methods.
	/// </summary>
	public class Connection
	{

		#region Fields - Internal and Private

		private static readonly byte[] AckBase =
		{
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x08, 0x00, 0x45, 0x00, 0x00, 0x2c, 0x00, 0xda, 0x00, 0x00, 0xff, 0x06,
			0x37, 0x0a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x50,
			0x09, 0xac, 0x00, 0x00, 0x43, 0x80, 0x58, 0xae, 0x2d, 0x41, 0x60, 0x12,
			0x04, 0x00, 0x41, 0xf2, 0x00, 0x00, 0x02, 0x04, 0x02, 0x00, 0x00, 0x00
		};

		private readonly ManualResetEvent _connectionWaitHandle = new ManualResetEvent(false);

		private readonly byte[] _scratch =
		{
			0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x08, 0x00, 0x45, 0x00, 0x00, 0xa8, 0x00, 0xd8, 0x00, 0x00,
			0xff, 0x06, 0x36, 0x8b, 0xc0, 0xa8, 0x00, 0x00, 0xc, 0xa8, 0x00, 0x00,
			0x00, 0x50, 0x09, 0xac, 0x00, 0x00, 0x43, 0x81, 0x58, 0xae, 0x2e, 0x82,
			0x50, 0x10, 0x04, 0x00, 0xf4, 0xb6, 0x00, 0x00
		};

		internal uint AckNumber = 6527;
		internal TimeSpan LastActivity = Utility.GetMachineTime();
		internal byte[] LastRequestChecksum = new byte[2];

		/// <summary>
		/// The local port of the TCP connection. 
		/// </summary>
		public ushort LocalPort = ++EthClick.EphmeralPort >= 65535 ? (ushort)49152 : EthClick.EphmeralPort;

		internal bool ReadyForRequest;

		/// <summary>
		/// The IPAddress of the remote TCP connection
		/// </summary>
		public byte[] RemoteIp;

		internal byte[] RemoteMac = EthClick._gatewayMac;

		/// <summary>
		/// The remote port of the TCP connection.
		/// </summary>
		public ushort RemotePort = 80;

		internal uint SeqNumber;
		internal uint StartSeqNumber;
		internal ushort WindowSize = 1024;

		internal bool IsClosing;

		private bool _isOpen;

		#endregion

		#region Private Methods
		
		private void Open(int timeout = 3)
		{
			if (_isOpen) return;

			RemoteMac = EthClick._gatewayMac;

			TCP._connections.Add(Id, this);

			_connectionWaitHandle.Reset();

			SendAck(true, false, false);

			_connectionWaitHandle.WaitOne(timeout * 1000, true);
		}

		private ulong Id
		{
			get
			{
				return Utility.CombineArrays(Utility.CombineArrays(RemoteIp, RemotePort.ToBytes()), LocalPort.ToBytes()).ToLong();
			}
		}
		
		#endregion

		#region Internal Properties
		
		internal bool IsOpen
		{
			get { return _isOpen; }
			set
			{
				_isOpen = value;
				_connectionWaitHandle.Set();
			}
		}

		#endregion

		#region Internal Methods

		internal void FireOnConnectionPacketReceived(Packet packet)
		{
			if (OnConnectionPacketReceived != null) OnConnectionPacketReceived(packet);
		}

		internal void SendAsync(byte[] buffer, int offset, short size)
		{
			if (EthClick._gatewayMac == null || EthClick._ip == null || size <= 0 || IsClosing) return;

			if (!_isOpen) Open();

			#if TINYCLR_TRACE
			if (EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("Sending TCP Data Segment");
			#endif

			_scratch.Overwrite(0 + 2, RemoteMac);
			_scratch.Overwrite(6 + 2, EthClick._macAddress);
			_scratch.Overwrite(26 + 2, EthClick._ip);
			_scratch.Overwrite(30 + 2, RemoteIp);
			_scratch.Overwrite(34 + 2, LocalPort.ToBytes());
			_scratch.Overwrite(36 + 2, RemotePort.ToBytes());
			_scratch.Overwrite(38 + 2, AckNumber.ToBytes()); 
			_scratch.Overwrite(42 + 2, SeqNumber.ToBytes()); 
			_scratch.Overwrite(48 + 2, new byte[] {0x04, 0x00});
			_scratch[47 + 2] = 0x10; 
			_scratch.Overwrite(50 + 2, new byte[] {0x00, 0x00});
			_scratch.Overwrite(24 + 2, new byte[] {0x00, 0x00});

			buffer = Utility.CombineArrays(_scratch, 0, _scratch.Length, buffer, offset, size);
			buffer.Overwrite(16 + 2, ((ushort) ((buffer.Length - 2) - 14)).ToBytes()); 
			buffer.Overwrite(50 + 2, buffer.InternetChecksum(20 + size, 34 + 2, EthClick._ip, RemoteIp));
			buffer.Overwrite(24 + 2, buffer.InternetChecksum(20, 14 + 2)); 

			AckNumber += (uint) size;
			EthClick.SendFrame(buffer, 2);
		}

		internal void SendAck(bool synchronize = false, bool finish = false, bool ack = true)
		{
			if (EthClick._gatewayMac == null || EthClick._ip == null) return;

			AckBase.Overwrite(0, RemoteMac);
			AckBase.Overwrite(6, EthClick._macAddress);
			AckBase.Overwrite(26, EthClick._ip);
			AckBase.Overwrite(30, RemoteIp);
			AckBase.Overwrite(34, LocalPort.ToBytes());
			AckBase.Overwrite(36, RemotePort.ToBytes());
			AckBase.Overwrite(38, AckNumber.ToBytes()); 
			AckBase.Overwrite(42, SeqNumber.ToBytes()); 
			AckBase.Overwrite(48, WindowSize.ToBytes());

			if (synchronize)
			{
				AckBase.Overwrite(54, new byte[] {0x02, 0x04, 0x05, 0xb4});
				AckBase.Overwrite(16, new byte[] {0x00, 0x2c});    
				AckBase.Overwrite(46, new byte[] {0x60}); 
			}
			else
			{
				AckBase.Overwrite(54, new byte[] {0x00, 0x00, 0x00, 0x00}); 
				AckBase.Overwrite(16, new byte[] {0x00, 0x28}); 
				AckBase.Overwrite(46, new byte[] {0x50}); 
			}

			if (synchronize && ack)
			{
				AckBase[47] = 0x12;
				#if TINYCLR_TRACE
				if (EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("Sending TCP SYN + ACK");
				#endif
			}
			else if (finish && ack)
			{
				AckBase[47] = 0x11;
				#if TINYCLR_TRACE
				if (EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("Sending TCP FIN + ACK");
				#endif
			}
			else if (synchronize)
			{
				AckBase[47] = 0x02;
				#if TINYCLR_TRACE
				if (EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("Sending TCP SYN");
				#endif
			}
			else if (finish)
			{
				AckBase[47] = 0x01;
				#if TINYCLR_TRACE
				if (EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("Sending TCP FIN");
				#endif
			}
			else
			{
				AckBase[47] = 0x10;
				#if TINYCLR_TRACE
				if (EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("Sending TCP ACK");
				#endif
			}

			AckBase.Overwrite(50, new byte[] {0x00, 0x00}); 
			AckBase.Overwrite(50, AckBase.InternetChecksum(synchronize ? 24 : 20, 34, EthClick._ip, RemoteIp));

			AckBase.Overwrite(24, new byte[] {0x00, 0x00}); 
			AckBase.Overwrite(24, AckBase.InternetChecksum(20, 14)); 

			EthClick.SendFrame(AckBase);
		}

		#endregion	

		#region Events
		
		internal delegate void ConnectionPacketReceivedEventHandler(Packet packet);
		
		internal event ConnectionPacketReceivedEventHandler OnConnectionPacketReceived;

		#endregion
	}

	/// <summary>
	/// The HTTP Request Class and methods.
	/// </summary>
	public class HttpRequest
	{
		#region Fields
		
		internal static readonly byte[] HeaderDelimiter = { 0x0d, 0x0a, 0x0d, 0x0a };
		private static readonly byte[] CrLf = { 0x0D, 0x0A };
		private static readonly object SdLock = new object();
		private static readonly byte[] Chunk = new byte[1400];
		private readonly ManualResetEvent _responseWaitHandle = new ManualResetEvent(false);

		private Connection _con;
		private HttpResponse _responseToSend;
		private bool _omitContent;

		#endregion

		#region Internal Methods
		
		internal HttpRequest(byte[] tcpContent, Connection socket)
		{
			_con = socket;
			Headers = new Hashtable();

			if (!Utility.ExtractRangeFromArray(tcpContent, tcpContent.Length - 4, 4).BytesEqual(HeaderDelimiter))
			{
				
				#if TINYCLR_TRACE
				if (EthClick._verboseDebugging)
				{
					System.Diagnostics.Debug.WriteLine("This should never happen!");
				}
				#endif

				throw new Exception("Error processing TCP request.");
			}

			tcpContent.Overwrite(tcpContent.Length - 4, HeaderDelimiter);

			var delimiterLocation = tcpContent.Locate(HeaderDelimiter);
			var firstLineLocation = tcpContent.Locate(CrLf);

			if (firstLineLocation < 12) throw new Exception("Malformed HTTP Request.");

			var firstLine = new string(Encoding.UTF8.GetChars(tcpContent, 0, firstLineLocation));

			RequestType = firstLine.Split(' ')[0].Trim().ToUpper();
			Path = HttpUtility.UrlDecode(firstLine.Split(' ')[1].Trim(), false);

			var colonLocation = -1;
			var start = firstLineLocation;
			var malformed = false;

			for (var i = firstLineLocation; i <= delimiterLocation; i++)
			{
				if (tcpContent[i] == 0x3A && colonLocation == -1) colonLocation = i;
				if (tcpContent[i] > 0x7E || tcpContent[i] < 0x09) malformed = true;

				if (tcpContent[i] == 0x0D || tcpContent[i] == 0x0A)
				{
					if (colonLocation > start && !malformed)
					{
						try
						{
							Headers.Add(new string(Encoding.UTF8.GetChars(tcpContent, start, colonLocation - start)).Trim(), new string(Encoding.UTF8.GetChars(tcpContent, colonLocation + 1, i - colonLocation)).Trim());
						}
						catch 
						{
							#if TINYCLR_TRACE
							if (EthClick._verboseDebugging)
							{
								System.Diagnostics.Debug.WriteLine("Error processing TCP request.");
							}
							#endif
							#if !TINYCLR_TRACE
							throw new Exception("Error processing TCP request.");
							#endif
						}
					}

					colonLocation = -1;
					start = i + 1;
					malformed = false;
				}
			}

			Content = RequestType != "GET" ? new string(Encoding.UTF8.GetChars(tcpContent, delimiterLocation + 4, tcpContent.Length - (delimiterLocation + 4))) : string.Empty;

			Protocol = (firstLine.IndexOf("HTTP") > 5) ? firstLine.Split(' ')[2] : "HTTP/1.1";

			if (Headers.Contains("Host")) Host = Headers["Host"] as string;
		}

		#endregion
		
		#region Public Properties

		/// <summary>
		/// Additional HTTP Headers in "key: value" format, such as "Cache-Control: no-cache; charset=utf-8"
		/// </summary>
		public Hashtable Headers { get; private set; }

		/// <summary>
		///     This is the HTTP verb, such as GET, PUT, POST, etc.
		/// </summary>
		public string RequestType { get; internal set; }

		/// <summary>
		///     This is the path to the requested resource, usually a file name
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		///     The Protocol and version, such as HTTP/1.1
		/// </summary>
		public string Protocol { get; internal set; }

		/// <summary>
		///     The Requested Host Name
		/// </summary>
		public string Host { get; internal set; }

		/// <summary>
		///     The content of the request.  GET requests are usually empty, but others may have something in here...
		/// </summary>
		public string Content { get; internal set; }

		#endregion
		
		#region Private Methods
		
		private byte[] AssembleRequest()
		{
			var a = RequestType;
			a += " " + Path + " " + Protocol + "\r\nHost: ";
			a += Host + "\r\n";

			foreach (object aHeader in Headers.Keys)
				a += (string)aHeader + ": " + (string)Headers[aHeader] + "\r\n";

			a += "\r\n"; // Cache-Control: no-cache\r\n  //Accept-Charset: utf-8;\r\n

			if (Content != null && Content != string.Empty && RequestType == "POST") a += Content;

			return Encoding.UTF8.GetBytes(a);
		}

		private void _con_OnConnectionPacketReceived(Packet packet)
		{
			_responseToSend = new HttpResponse(packet.Content, _omitContent);
			_responseWaitHandle.Set();
		}

		#endregion
		
		#region Public Methods

		/// <summary>
		/// Sends an HTTP Request to the specified URL.  If no content is specified a GET is sent, otherwise a POST is sent.
		/// </summary>
		/// <param name="url">The URL address with path, such as <!--"http://www.msftncsi.com/ncsi.txt"--></param>
		/// <param name="content">Optional, if a content is specified, the HTTPRequest will be a POST or otherwise a Get.</param>
		/// <param name="connection">Optional, specify a connection object of the host you want to get the response from.
		/// </param>
 		/// <example>Example usage:
		/// <code language = "C#">See example WebserverProgram.cs in the driver download for example usage.
		/// </code>
		/// <code language = "VB"> None provided.
		/// </code>
		/// </example>
		public HttpRequest(string url, string content = null, Connection connection = null)
		{
			if (connection != null) _con = connection;

			url = (url.IndexOf("http://") >= 0) ? url.Substring(url.IndexOf("http://") + 7) : url;

			Host = url.Split('/')[0].Trim();
			Path = HttpUtility.UrlEncode(url.Substring(Host.Length).Trim(), false);
			if (Path == string.Empty) Path = "/";

			Protocol = "HTTP/1.1";
			Content = content;
			RequestType = "GET";

			Headers = new Hashtable();

			if (content != null && content != string.Empty)
			{

				RequestType = "POST";
				Headers.Add("Content-Length", content.Length.ToString());
			}
		}

		/// <summary>
		/// A synchronous call to send an HTTP Request and Receive a Response --  this call will block (until the timeout) while trying to get the result
		/// </summary>
		/// <param name="timeout">Time out in seconds</param>
		/// <param name="returnHeaderOnly">Set this to true if you don't need the content of the response which can be memory intensive depending on the content length.
		/// </param>
		/// <returns>And HttpResponse object or null if it timeout period has elapsed.</returns>
		/// <remarks>The default timeout period is five (5) seconds and returnHeaders is false.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">See example WebserverProgram.cs in the driver download for example usage.
		/// </code>
		/// <code language = "VB">None Provided.
		/// </code>
		/// </example>
		public HttpResponse Send(ushort timeout = 5, bool returnHeaderOnly = false)
		{
			_omitContent = returnHeaderOnly;

			if (_con == null)
			{
				var remoteIp = DNS.Lookup(Host);
				_con = new Connection {RemoteIp = remoteIp};
			}

			var r = AssembleRequest();

			_responseToSend = null;

			_con.OnConnectionPacketReceived += _con_OnConnectionPacketReceived;

			_responseWaitHandle.Reset(); 

			_con.SendAsync(r, 0, (short) r.Length);

			_responseWaitHandle.WaitOne(timeout * 1000, true);

			_con.OnConnectionPacketReceived -= _con_OnConnectionPacketReceived;

			return _responseToSend;
		}

		/// <summary>
		/// Send a 404 not found response
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		/// request.SendNotFound();
		/// </code>
		/// <code language = "VB">
		/// request.SendNotFound()
		/// </code>
		/// </example>
		public void SendNotFound()
		{
			const string body = "<html><head><title>Page Not Found</title></head><body>404 - Not Found</body></html>";

			var notFoundResponse = new HttpResponse(body, "text/html", "404 Not Found");
			notFoundResponse.Headers.Add("Date", DateTime.Now.ToUniversalTime() + " UTC");
			SendResponse(notFoundResponse);
		}

		/// <summary>
		/// Send a 418 I'm a teapot response. This is a joke.
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		/// request.SendImATeapot();
		/// </code>
		/// <code language = "VB">
		/// request.SendImATeapot()
		/// </code>
		/// </example>
		public void SendImATeapot()
		{
			const string body = "<html><head><title>Page Not Found</title></head><body>418 - I'm a teapot</body></html>";

			var notFoundResponse = new HttpResponse(body, "text/html", "418 I'm a teapot");
			notFoundResponse.Headers.Add("Date", DateTime.Now.ToUniversalTime() + " UTC");
			SendResponse(notFoundResponse);
		}

		/// <summary>
		/// Send the Http Response with automatic chunking to keep memory usage low.
		/// </summary>
		/// <param name="response">The Response object to send.</param>
		/// <param name="chunkSize">If this chunk size exceeds the size of the controller buffer size, bad things could happen. So lets send chunked. I would recommend a maximum of 1024 bytes.
		/// </param>
		/// <example>Example usage: See example WebserverProgram.cs in the driver download for example usage.
		/// <code language = "C#">
		/// </code>
		/// <code language = "VB"> None provided.
		/// </code>
		/// </example>
		public void SendResponse(HttpResponse response, int chunkSize = 512)
		{
			try
			{
				_con.SendAsync(response.HeaderSection, 0, (short) response.HeaderSection.Length);

				lock (SdLock)
				{
					response.Content.Position = 0;

					short bytesToSend;
					do
					{
						bytesToSend = (short) response.Content.Read(Chunk, 0, chunkSize);
						_con.SendAsync(Chunk, 0, bytesToSend);
					} while (bytesToSend > 0 && response.Content.Position <= response.Content.Length); 

					response.Content.Flush();
					response.Content.Close();
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Pre garbage collecting... Memory: " + Debug.GC(false));
				Debug.Print("Running GC due to an exception.");
				Debug.GC(true);
				System.Diagnostics.Debug.WriteLine("Post garbage collecting... Memory: " + Debug.GC(false));
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
			finally
			{
				_con.ReadyForRequest = true;
			}
		}
 
		#endregion	

	}

	/// <summary>
	/// The HTTP Response Class and methods.
	/// </summary>
	public class HttpResponse
	{
		#region Fields
		
		private readonly string _connection;
		private readonly string _contentType;

		/// <summary>
		/// Then HTTP Response body.
		/// </summary>
		public readonly string Message;

		/// <summary>
		/// A string containing both the status code number and the Message.  Such as "HTTP/1.1 200 OK" or "HTTP/1.1 404 Not Found" or "HTTP/1.1 418 I'm a teapot"
		/// </summary>
		public readonly string Status;

		/// <summary>
		/// Additional HTTP Headers in "key: value" format, such as "Cache-Control: no-cache; charset=utf-8"
		/// </summary>
		public readonly Hashtable Headers = new Hashtable();

		internal readonly Stream Content;

		#endregion

		#region Public Methods
		
		/// <summary>
		/// Creates a new Http Response object.
		/// </summary>
		/// <param name="body">Be aware of memory limitations here. Use small strings with this constructor.</param>
		/// <param name="contentType">The Content-Type - for example "text/html".</param>
		/// <param name="status">The Response Status Code, for example "200 OK"</param>
		/// <param name="connection">Typically set to "close".</param>
		/// <remarks>Use this method to send textural responses only.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		/// // See example WebserverProgram.cs in the driver download for example usage.
		/// </code>
		/// <code language = "VB">
		/// ' None provided.
		/// </code>
		/// </example>
		public HttpResponse(string body, string contentType = "text/html", string status = "200 OK", string connection = "close")
		{
			_contentType = contentType;
			_connection = connection;
			Status = status;

			Content = new MemoryStream(Encoding.UTF8.GetBytes(body));
		}

		/// <summary>
		///	Creates a new Http Response object.
		/// </summary>
		/// <param name="content">The HTTP Content as a <see cref="System.IO.Stream"/>.</param>
		/// <param name="contentType">The Content-Type - for example "text/html".</param>
		/// <param name="status">The Response Status Code, for example "200 OK"</param>
		/// <param name="connection">Typically set to "close".</param>
		/// <remarks>Use this method to send binary streams such as images or file streams.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		/// // See example WebserverProgram.cs in the driver download for example usage.
		/// </code>
		/// <code language = "VB">
		/// ' None provided.
		/// </code>
		/// </example>
		public HttpResponse(Stream content, string contentType = "text/html", string status = "200 OK", string connection = "close")
		{
			_contentType = contentType;
			_connection = connection;
			Status = status;

			Content = content;
		}

		#endregion
		
		#region Internal Methods
		
		internal HttpResponse(byte[] content, bool omitContent = false)
		{
			var contentStart = content.Locate(new byte[] {0x0d, 0x0a, 0x0d, 0x0a});
			var headerStart = content.Locate(new byte[] {0x0d, 0x0a});

			Message = string.Empty;

			Status = new string(Encoding.UTF8.GetChars(Utility.ExtractRangeFromArray(content, 0, headerStart)));
			if ((contentStart > (headerStart + 2)) && headerStart > 5)
			{
				var colonLocation = -1;
				var start = headerStart + 2;
				var malformed = false;

				for (var i = headerStart + 2; i <= contentStart; i++)
				{
					if (content[i] == 0x3A && colonLocation == -1) colonLocation = i;
					if (content[i] > 0x7E || content[i] < 0x09) malformed = true;

					if (content[i] == 0x0D || content[i] == 0x0A)
					{
						if (colonLocation > start && !malformed)
						{
							try
							{
								Headers.Add(new string(Encoding.UTF8.GetChars(content, start, colonLocation - start)).Trim(), new string(Encoding.UTF8.GetChars(content, colonLocation + 1, i - colonLocation)).Trim());
							}
							catch
							{
								#if TINYCLR_TRACE
								if (EthClick._verboseDebugging)
								{
									System.Diagnostics.Debug.WriteLine("Error while adding header to the Header HashTable");
								}
								#endif
								#if !TINYCLR_TRACE
								throw new Exception("Error while adding header to the Header HashTable");
								#endif
							}
						}

						colonLocation = -1;
						start = i + 1;
						malformed = false;
					}
				}

				if (!omitContent && contentStart + 4 < content.Length)
				{
					Message =
						new string(
							Encoding.UTF8.GetChars(Utility.ExtractRangeFromArray(content, contentStart + 4,
								content.Length - (contentStart + 4))));
				}
			}
		}

		internal byte[] HeaderSection
		{
			get
			{
				var header = "HTTP/1.1 " + Status +
				                (_contentType != null && _contentType != string.Empty
					                ? ("\r\nContent-Type: " + _contentType)
					                : string.Empty);

				foreach (string headerKey in Headers.Keys)
				{
					header += "\r\n" + headerKey + ": " + (Headers[headerKey] as string);
				}

				header += ((Content != null && Content.Length > 0) ? ("\r\nContent-Length: " + Content.Length) : string.Empty)
				          + (_connection != null && _connection != string.Empty ? ("\r\nConnection: " + _connection) : string.Empty)
				          + "\r\n\r\n";

				return Encoding.UTF8.GetBytes(header);
			}
		}
 
		#endregion
	
	}

}