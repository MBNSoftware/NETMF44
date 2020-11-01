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
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace MBN.Modules
{
	internal static class DHCP
	{
		#region Fields
		
		private const int TwoHoursInMilliseconds = 7200 * 1000;

		private static readonly byte[] Scratch =
		{
			0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x08,
			0x00, 0x45, 0x00, 0x01, 0x48, 0x00, 0x00, 0x00, 0x00, 0xff, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff,
			0xff, 0xff, 0x00, 0x44, 0x00, 0x43, 0x01, 0x34, 0x00, 0x00, 0x01, 0x01, 0x06, 0x00, 0x92, 0xff, 0xf9, 0xef, 0x00,
			0x0b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x10, 0x78, 0xd2, 0xd9, 0xd5, 0xef, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x63,
			0x82, 0x53, 0x63, 0x35, 0x01, 0x01, 0x37, 0x06, 0x01, 0x03, 0x06, 0x0f, 0x77, 0xfc, 0x39, 0x02, 0x02, 0xee, 0x3d,
			0x07, 0x01
		};

		private static readonly Timer RenewTimer = new Timer(RenewNow, null, Timeout.Infinite, Timeout.Infinite);

		private static readonly byte[] MagicCookie = { 0x63, 0x82, 0x53, 0x63 };
		private static byte[] _transactionId;

		internal static readonly byte[] Request = { 0x03 };
		internal static readonly byte[] Discover = { 0x01 };
		private static byte[] _pendingIpAddress; 

		#endregion

		#region Internal Methods
		
		internal static void HandlePacket(byte[] payload)
		{
			if (_transactionId == null || payload[46] != _transactionId[0] || payload[47] != _transactionId[1] ||
				payload[48] != _transactionId[2] || payload[49] != _transactionId[3]) return;

			var options = ParseOptions(payload);

			if (options.Contains("53"))
			{
				if (((byte[])(options["53"]))[0] == 0x02)
				{
					var ipHeaderLength = (ushort)((payload[14] & 0x0f) * 4);
					_pendingIpAddress = Utility.ExtractRangeFromArray(payload, ipHeaderLength + 38, 4);
					if (options.Contains("54")) EthClick._defaultGateway = (byte[])options["54"];
					if (options.Contains("6")) EthClick._preferredDomainNameServer = (byte[])options["6"];
					if (options.Contains("1")) EthClick._subnetMask = (byte[])options["1"]; 
					if (options.Contains("3")) EthClick._defaultGateway = (byte[])options["3"];
					if (options.Contains("58")) RenewTimer.Change((int)(((byte[])options["58"]).ToInt() * 1050), TwoHoursInMilliseconds);
					if (options.Contains("51")) RenewTimer.Change((int)(((byte[])options["51"]).ToInt() * 750), TwoHoursInMilliseconds);
					EthClick._gatewayMac = Utility.ExtractRangeFromArray(payload, 6, 6);

					SendMessage(Request);
				}
				else switch (((byte[])options["53"])[0])
					{
						case 0x05:
							if (options.Contains("54")) EthClick._defaultGateway = (byte[])options["54"];
							if (options.Contains("6")) EthClick._preferredDomainNameServer = (byte[])options["6"];
							if (options.Contains("1")) EthClick._subnetMask = (byte[])options["1"];
							if (options.Contains("3")) EthClick._defaultGateway = (byte[])options["3"];
							if (options.Contains("58")) RenewTimer.Change((int)(((byte[])options["58"]).ToInt() * 1050), TwoHoursInMilliseconds);
							if (options.Contains("51")) RenewTimer.Change((int)(((byte[])options["51"]).ToInt() * 750), TwoHoursInMilliseconds);
							EthClick._gatewayMac = Utility.ExtractRangeFromArray(payload, 6, 6);
							_transactionId = null;
							EthClick.IsRenewing = false;
							EthClick._ip = _pendingIpAddress ?? EthClick._ip;
							EthClick.StartupHold.Set();

							#if TINYCLR_TRACE
							if(EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("DHCP SUCCESS!  We have an IP Address - " + EthClick._ip.ToAddress() + "; DefaultGateway: " + EthClick._defaultGateway.ToAddress());
							#endif

							ARP.SendARP_Probe(EthClick._defaultGateway);
							break;
						case 0x06:
							_transactionId = null;
							EthClick.IsRenewing = false;
							EthClick._ip = null;
							EthClick._defaultGateway = null;
							EthClick._gatewayMac = null;
							break;
					}
			}
		}

		internal static void SendMessage(byte[] packetType)
		{
			lock (MagicCookie)
			{
				Scratch.Overwrite(0, EthClick.BroadcastMac);
				Scratch.Overwrite(6, EthClick._macAddress);
				Scratch.Overwrite(30, EthClick.BroadcastIpAddress);
				Scratch.Overwrite(54, EthClick.BlankIpAddress);
				Scratch.Overwrite(70, EthClick._macAddress);
				Scratch.Overwrite(284, packetType);

				var options = new byte[13 + (EthClick._name == string.Empty ? 0 : EthClick._name.Length + 2)];
				options.Overwrite(0, EthClick._macAddress);

				_transactionId = _transactionId ?? Extensions.GetRandomBytes(4);

				if (packetType == Discover)
				{
					_pendingIpAddress = null;
					options.Overwrite(6, new byte[] { 0x33, 0x04, 0x00, 0x76, 0xa7, 0x00 });
				}
				else if (packetType == Request && _pendingIpAddress != null && EthClick._defaultGateway != null)
				{
					options.Overwrite(6, new byte[] { 0x32, 0x04 });
					options.Overwrite(8, _pendingIpAddress);
					options.Overwrite(12, new byte[] { 0x36, 0x04 });
					options.Overwrite(14, EthClick._defaultGateway);

					if (EthClick._gatewayMac != null && EthClick._defaultGateway != null && EthClick._ip != null)
					{
						Scratch.Overwrite(54, EthClick._ip);
					}
				}
				else
				{
					#if TINYCLR_TRACE
					if (EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("Odd DHCP situation... should we be concerned?");
					#endif
					return;
				}

				if (EthClick._name != string.Empty)
				{
					options.Overwrite(options.Length - (EthClick._name.Length + 3), new byte[] { 0x0c });
					options.Overwrite(options.Length - (EthClick._name.Length + 2), DNS.EncodeDnsName(EthClick._name));
				}

				options.Overwrite(options.Length - 1, new byte[] { 0xFF });

				Scratch.Overwrite(46, _transactionId);

				var result = Utility.CombineArrays(Scratch, options);

				result.Overwrite(16, ((ushort)(result.Length - 14)).ToBytes());
				result.Overwrite(38, ((ushort)(result.Length - 34)).ToBytes());
				result.Overwrite(24, new byte[] { 0x00, 0x00 });
				result.Overwrite(24, result.InternetChecksum(20, 14));
				result.Overwrite(40, new byte[] { 0x00, 0x00 });

				EthClick.SendFrame(result);
			}
		}

		#endregion
		
		#region Private Methods
		
		private static void RenewNow(object o)
		{
			RenewTimer.Change(Timeout.Infinite, TwoHoursInMilliseconds);
			_transactionId = _transactionId ?? Extensions.GetRandomBytes(4);
			EthClick.IsRenewing = true;
		}

		private static Hashtable ParseOptions(byte[] payload)
		{
			var result = new Hashtable();
			var current = payload.Locate(MagicCookie) + MagicCookie.Length;
			Debug.Print("current - " + current);

			try
			{
				while (payload[current] != 0xff)
				{
					var currentOption = payload[current++];
					var currentSize = payload[current++];

					if (!result.Contains(currentOption.ToString()))
					{
						result.Add(currentOption.ToString(), Utility.ExtractRangeFromArray(payload, current, currentSize));
					}
					current += currentSize;
				}
			}
			catch (Exception ex)
			{
				#if TINYCLR_TRACE
				if (EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("Parsing Error occurred while parsing DHCP Packet."); 
				#endif
				throw new Exception("Parsing Error occurred while parsing the DHCP Packet Message - " + ex.Message);
			}

			return result;
		}

		#endregion
	}
}