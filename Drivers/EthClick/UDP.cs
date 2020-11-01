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

using Microsoft.SPOT.Hardware;

namespace MBN.Modules
{
	/// <summary>
	/// The UDP Class for sending UDP messages.
	/// </summary>
	public static class UDP
	{
		#region Fields

		private static readonly byte[] Scratch =
		{
			0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x50, 0xe5, 0x49, 0xe4, 0x34, 0x8d, 0x08, 0x00, 0x45, 0x00, 0x00, 0x1d, 0x05,
			0x01, 0x00, 0x00, 0x80, 0x11, 0x73, 0xcc, 0xc0, 0xa8, 0x01, 0x52, 0xff, 0xff, 0xff, 0xff, 0x06, 0xf2, 0x06, 0xf2,
			0x00, 0x00, 0x5d, 0x88
		};

		#endregion

		#region Internal Methods

		internal static void HandlePacket(byte[] payload)
		{

			var sourceIp = Utility.ExtractRangeFromArray(payload, 26, 4);
			var sourcePort = Utility.ExtractRangeFromArray(payload, 34, 2);
			var destinationPort = Utility.ExtractRangeFromArray(payload, 36, 2);

			var socket = new Connection
			{
				RemoteIp = sourceIp,
				RemotePort = sourcePort.ToShort(),
				LocalPort = destinationPort.ToShort()
			};

			var udpDataLength = (ushort) ((new[] {payload[38], payload[39]}).ToShort() - 8);

			if (udpDataLength > 0) EthClick.FireUdpPacketEvent(Utility.ExtractRangeFromArray(payload, 42, udpDataLength), socket);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Sends a UDP message.
		/// </summary>
		/// <param name="message">The byte encoded message to send.</param>
		/// <param name="destinationIp">The byte encoded Destination IPAddress.</param>
		/// <param name="destinationPort">The destination Port.</param>
		/// <param name="sourcePort">The source Port.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// namespace NetworkingExample
		/// {
		/// 	public class UdpProgram
		/// 	{
		/// 		private static EthClick _eth;
		///
		/// 		public static void Main()
		/// 		{
		/// 			/* Use this for Static IP and No DHCP */
		/// 			//_eth.Name = "Quail";
		/// 			//_eth.MacAddress = _eth.GenerateUniqueMacAddress("Quail");
		/// 			//_eth.IPAddress = "192.168.1.95".ToBytes();
		/// 			//_eth.DefaultGateway = "192.168.1.1".ToBytes();
		/// 			//_eth.PreferredDomainNameServer = "8.8.8.8".ToBytes(); // Google DNS Servers
		/// 			//_eth.AlternateDomainNameServer = "8.8.8.4".ToBytes(); // Google DNS Servers
		/// 			//_eth.DHCPDisabled = true;
		///
		/// 			_eth = new EthClick(Hardware.SocketOne);
		///
		/// 			_eth.Start(_eth.GenerateUniqueMacAddress("MikroBusNet"), "MikroBusNet");
		///
		/// 			while (true)
		/// 			{
		/// 				if (_eth.ConnectedToInternet)
		/// 				{
		/// 					Debug.Print("Connected to Internet");
		/// 					break;
		/// 				}
		/// 				Debug.Print("Waiting on Internet connection");
		/// 			}
		///
		/// 			// Listen for UDP messages sent to activated ports
		/// 			EthClick.OnUdpPacketReceived += Adapter_OnUdpPacketReceived;
		///
		/// 			// Activate the NTP (date/time) port 123
		/// 			_eth.ListenToPort(123);
		///
		/// 			// Create a NTP (date/time) Request Message
		/// 			var msg = new byte[48];
		/// 			msg[0] = 0x1B;
		///
		/// 			var ipAddress = "165.193.126.229".ToBytes(); // IPAddress for NIST Time Server in Weehawken, NJ USA see http://tf.nist.gov/tf-cgi/servers.cgi
		///
		/// 			// Let's get the UTC time from a time server using a UDP Message
		/// 			UDP.SendUdpMessage(msg, ipAddress, 123, 123);  
		///
		/// 			Thread.Sleep(Timeout.Infinite);
		/// 		}
		///
		/// 		private static void Adapter_OnUdpPacketReceived(Packet packet)
		/// 		{
		/// 			if (packet.Socket.RemotePort == 123)
		/// 			{
		/// 				var transitTime = Utility.ExtractRangeFromArray(packet.Content, 40, 8);
		/// 				Debug.Print("Current UTC Date/Time is " + transitTime.ToDateTime());
		/// 			}
		/// 		}
		/// 	}
		///
		/// 	public static class Extensions
		/// 	{
		/// 			/// <summary>
		/// 			/// Convert an 8-byte array from NTP format to .NET DateTime.  
		/// 			/// </summary>
		/// 			<![CDATA[///>]]>param name="ntpTime">NTP format 8-byte array containing date and time<![CDATA[<]]>/param>
		/// 			/// <returns>A Standard .NET DateTime</returns>
		/// 			public static DateTime ToDateTime(this byte[] ntpTime)
		/// 			{
		/// 				Microsoft.SPOT.Debug.Assert(ntpTime.Length == 8, "The passed array is too short to be a valid NTP DateTime.");
		///
		/// 				ulong intpart = 0;
		/// 				ulong fractpart = 0;
		///
		/// 				for (int i = 0; i <![CDATA[<]]>= 3; i++)
		/// 					intpart = (intpart <![CDATA[<<]]> 8) | ntpTime[i];
		///
		/// 				for (int i = 4; i <![CDATA[<]]>= 7; i++)
		/// 					fractpart = (fractpart <![CDATA[<<]]> 8) | ntpTime[i];
		///
		/// 				ulong milliseconds = (intpart*1000 + (fractpart*1000)/0x100000000L);
		///
		/// 				var timeSince1900 = TimeSpan.FromTicks((long) milliseconds*TimeSpan.TicksPerMillisecond);
		/// 				return new DateTime(1900, 1, 1).Add(timeSince1900);
		/// 			}
		/// 	}
		/// }
		/// </code>
		/// <code language = "VB">
		/// ' None provided as VS 2012 does not support bit-shifting with VB.Net which is necessary for this example.
		/// </code>
		/// </example>
		/// 
		public static void SendUdpMessage(byte[] message, byte[] destinationIp, ushort destinationPort, ushort sourcePort)
		{
			Scratch.Overwrite(0, EthClick._gatewayMac);
			Scratch.Overwrite(6, EthClick._macAddress);
			Scratch.Overwrite(30, destinationIp);
			Scratch.Overwrite(26, EthClick._ip);
			Scratch.Overwrite(34, sourcePort.ToBytes());
			Scratch.Overwrite(36, destinationPort.ToBytes());
			Scratch.Overwrite(38, ((ushort) (message.Length + 8)).ToBytes());
			Scratch.Overwrite(16, ((ushort) ((Scratch.Length + message.Length) - 14)).ToBytes());
			Scratch.Overwrite(24, new byte[] {0x00, 0x00});
			Scratch.Overwrite(24, Scratch.InternetChecksum(20, 14));
			Scratch.Overwrite(40, new byte[] {0x00, 0x00});

			EthClick.SendFrame(Utility.CombineArrays(Scratch, message));
		}

		#endregion
	}
}