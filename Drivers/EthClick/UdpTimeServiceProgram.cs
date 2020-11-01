using MBN;
using MBN.Modules;
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;

namespace NetworkingExample
{
	public class UDPTimeServiceProgram
	{
		private static EthClick _eth;

		public static void Main()
		{
			/* Use this for Static IP and No DHCP */
			//_eth.Name = "Quail";
			//_eth.MacAddress = _eth.GenerateUniqueMacAddress("Quail");
			//_eth.IPAddress = "192.168.1.95".ToBytes();
			//_eth.DefaultGateway = "192.168.1.1".ToBytes();
			//_eth.PreferredDomainNameServer = "8.8.8.8".ToBytes(); // Google DNS Servers
			//_eth.AlternateDomainNameServer = "8.8.8.4".ToBytes(); // Google DNS Servers
			//_eth.DHCPDisabled = true;

			_eth = new EthClick(Hardware.SocketOne);

			_eth.Start(_eth.GenerateUniqueMacAddress("MikroBusNet"), "MikroBusNet");

			while (true)
			{
				if (_eth.ConnectedToInternet)
				{
					Debug.Print("Connected to Internet");
					Thread.Sleep(500);
					break;
				}
				Debug.Print("Waiting on Internet connection");
			}

			// Listen for UDP messages sent to activated ports
			EthClick.OnUdpPacketReceived += Adapter_OnUdpPacketReceived;

			// Activate the NTP (date/time) port 123
			_eth.ListenToPort(123);

			// Create a NTP (date/time) Request Message
			var msg = new byte[48];
			msg[0] = 0x1B;

			var ipAddress = "165.193.126.229".ToBytes(); // IPAddress for NIST Time Server in Weehawken, NJ USA see http://tf.nist.gov/tf-cgi/servers.cgi

			// Let's get the UTC time from a time server using a UDP Message
			UDP.SendUdpMessage(msg, ipAddress, 123, 123);

			Thread.Sleep(Timeout.Infinite);
		}

		private static void Adapter_OnUdpPacketReceived(Packet packet)
		{
			if (packet.Socket.RemotePort == 123)
			{
				var transitTime = Utility.ExtractRangeFromArray(packet.Content, 40, 8);
				Debug.Print("Current UTC Date/Time is " + transitTime.ToDateTime());
			}
		}
	}

	public static class Extensions
	{
		/// <summary>
		/// Convert an 8-byte array from NTP format to .NET DateTime.  
		/// </summary>
		/// <param name="ntpTime">NTP format 8-byte array containing date and time</param>
		/// <returns>A Standard .NET DateTime</returns>
		public static DateTime ToDateTime(this byte[] ntpTime)
		{
			Microsoft.SPOT.Debug.Assert(ntpTime.Length == 8, "The passed array is too short to be a valid NTP DateTime.");

			ulong intpart = 0;
			ulong fractpart = 0;

			for (int i = 0; i <= 3; i++)
				intpart = (intpart << 8) | ntpTime[i];

			for (int i = 4; i <= 7; i++)
				fractpart = (fractpart << 8) | ntpTime[i];

			ulong milliseconds = (intpart * 1000 + (fractpart * 1000) / 0x100000000L);

			var timeSince1900 = TimeSpan.FromTicks((long)milliseconds * TimeSpan.TicksPerMillisecond);
			return new DateTime(1900, 1, 1).Add(timeSince1900);
		}
	}
}