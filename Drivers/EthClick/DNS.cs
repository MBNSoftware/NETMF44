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
using System.Text;
using System.Threading;
using Microsoft.SPOT.Hardware;

namespace MBN.Modules
{
	/// <summary>
	/// The Domain Name Service class used for the lookup of a Domain Name.
	/// </summary>
	public static class DNS
	{

		#region Fields
		
		private const int CacheMax = 5;

		private static readonly byte[] Scratch =
		{
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x45, 0x00, 0x00, 0x38, 0x78,
			0xc0, 0x00, 0x00, 0x80, 0x11, 0x3d, 0x50, 0xc0, 0xa8, 0x01, 0x56, 0xc0, 0xa8, 0x01, 0xfe, 0xc7, 0x47, 0x00, 0x35,
			0x00, 0x24, 0x9f, 0xc9, 0x00, 0x02, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
		};

		private static readonly object LockObj = new Object();
		private static readonly AutoResetEvent DnsWaitHandle = new AutoResetEvent(false);
		private static string _syncDnsLookupQuery = string.Empty;
		private static byte[] _syncDnsLookupResult;

		private static readonly ArrayList DnsCache = new ArrayList(); 

		#endregion

		#region Internal Events

		internal delegate void DnsLookupReceivedEventHandler(string domainName, byte[] ipAddress);

		internal static event DnsLookupReceivedEventHandler OnDnsLookupEvent;
		
		#endregion

		#region Public Methods
		
		/// <summary>
		///  Does a DNS lookup.  This is a synchronous call, so it will block you program until the timeout or the response is received.
		///  If the timeout happens, the result returned will be a null.  Also, if an IP address is passed in, it will just be returned back to you, since no lookup is necessary.
		/// </summary>
		/// <param name="dnsName">The Domain Name that you want to look up. For example "bing.com"</param>
		/// <param name="timeout">How long to wait for a response from the DNS Server before giving up. Units are seconds.</param>
		/// <returns>The IP Address of the result, or null if a timeout period elapses.</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		/// var addressBytes = DNS.Lookup("www.google.com");
		///	Debug.Print("DNS Lookup: www.google.com - " + addressBytes.ToAddress());
		/// </code>
		/// <code language = "VB">
		///	var addressBytes = DNS.Lookup("www.google.com")
		///	Debug.Print("DNS Lookup: www.google.com - " <![CDATA[&]]> addressBytes.ToAddress())
		/// </code>
		/// </example>
		public static byte[] Lookup(string dnsName, short timeout = 3)
		{
			dnsName = dnsName.Trim();
			if (dnsName.Length == 0) return null;

			foreach (DnsAnswer anAnswer in DnsCache)
			{
				if (anAnswer.Name == dnsName && !anAnswer.IsExpired())
					return anAnswer.Value;
			}

			lock (LockObj)
			{
				_syncDnsLookupQuery = dnsName;
				_syncDnsLookupResult = dnsName.ToBytes();

				if (_syncDnsLookupResult == null)
				{
					LookupAsync(dnsName);

					if (!DnsWaitHandle.WaitOne(timeout * 1000, true) && EthClick._alternateDomainNameServer != null)
					{
						LookupAsync(dnsName, true);

						DnsWaitHandle.WaitOne(timeout * 1000, true);
			
						#if TINYCLR_TRACE
						if (EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("2 " + DateTime.Now);
						#endif
					}
				}

				if (_syncDnsLookupResult == null)
				{
					// Let's use even an expired DNS Entry!  
					foreach (DnsAnswer anAnswer in DnsCache)
					{
						if (anAnswer.Name == dnsName)
						{
							return anAnswer.Value;
						}
					}
					throw new Exception("Domain Name lookup for " + dnsName + " failed. ");
				}
				return _syncDnsLookupResult;
			}
		}
		
		#endregion
		
		#region Private Methods

		private static void RemoveExpiredAnswers()
		{
			for (int i = DnsCache.Count - 1; i >= 0 && DnsCache.Count >= CacheMax; i--)
			{
				var dnsAnswer = DnsCache[i] as DnsAnswer;
				if (dnsAnswer != null && dnsAnswer.IsExpired()) DnsCache.RemoveAt(i);
			}
		}

		private static DnsAnswer[] ParseAnswers(byte[] payload, int start, uint count)
		{
			#if TINYCLR_TRACE
			if (EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("DNS Answers");
			#endif

			var result = new DnsAnswer[count];
			int position = start;

			try
			{
				for (int i = 0; i < count; i++)
				{
					ushort currentSize = Utility.ExtractRangeFromArray(payload, position + 10, 2).ToShort();

					result[i] = new DnsAnswer
					{
						Type = Utility.ExtractRangeFromArray(payload, position + 2, 2).ToShort(),
						Expiration =
							PowerState.Uptime.Add(
								new TimeSpan(TimeSpan.TicksPerSecond * Utility.ExtractRangeFromArray(payload, position + 6, 4).ToInt())),
						Value = Utility.ExtractRangeFromArray(payload, position + 12, currentSize)
					};

					position += currentSize + 12;
				}
			}
			catch
			{
				return new DnsAnswer[0];
			}
			return result;
		}

		private static void LookupAsync(string dnsName, bool useSecondaryDnsServer = false)
		{
			if (EthClick._preferredDomainNameServer == null)
			{
				throw new Exception("Domain Name Server is not set. If you are using DHCP, you must wait until EthClick.PreferredDomainNameServer is populated before you attempt to make a DNS call.");
			}
			if (EthClick._gatewayMac == null)
			{
				throw new Exception("DefaultGateway MAC is not set. If you are using DHCP, you must wait until EthClick.GatewayMac is populated before you attempt to make a DNS call.");
			}

			Scratch.Overwrite(0, EthClick._gatewayMac);
			Scratch.Overwrite(6, EthClick._macAddress);
			Scratch.Overwrite(30, (useSecondaryDnsServer && EthClick._alternateDomainNameServer != null) ? EthClick._alternateDomainNameServer : EthClick._preferredDomainNameServer);
			Scratch.Overwrite(26, EthClick._ip);

			lock (LockObj)
			{
				Scratch.Overwrite(42, Extensions.GetRandomBytes(2));

				var result = Utility.CombineArrays(Scratch, Utility.CombineArrays(EncodeDnsName(dnsName), new byte[] { 0x00, 0x01, 0x00, 0x01 }));

				result.Overwrite(16, ((ushort)(result.Length - 14)).ToBytes());
				result.Overwrite(38, ((ushort)(result.Length - 34)).ToBytes());
				result.Overwrite(24, new byte[] { 0x00, 0x00 });
				result.Overwrite(24, result.InternetChecksum(20, 14));
				result.Overwrite(40, new byte[] { 0x00, 0x00 });

				EthClick.SendFrame(result);
			}
		}

		#endregion

		#region Internal Methods

		internal static byte[] EncodeDnsName(string name)
		{
			name = name.ToLower();
			string[] parts = name.Split('.');
			var result = new byte[name.Length + 2];
			int i = 0;

			foreach (string aPart in parts)
			{
				result[i++] = (byte) (aPart.Length);

				char[] chr = aPart.ToCharArray();
				foreach (char c in chr)
				{
					result[i++] = Encoding.UTF8.GetBytes(c.ToString())[0];
				}
			}
			return result;
		}

		internal static string DecodeDnsName(byte[] buffer, int start)
		{
			int i = start;
			var result = new byte[0];

			while (buffer[i] != 0x00)
			{
				if (i != start) result = Utility.CombineArrays(result, new byte[] {0x2E});
				result = Utility.CombineArrays(result, Utility.ExtractRangeFromArray(buffer, i + 1, buffer[i]));
				i = start + result.Length + 1;
			}

			return new string(Encoding.UTF8.GetChars(result));
		}

		internal static void HandlePacket(byte[] payload)
		{
            #if TINYCLR_TRACE
			if (EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("DNS Response");
			#endif

			if (OnDnsLookupEvent == null && _syncDnsLookupQuery == string.Empty) return;

			var ipHeaderLength = (ushort) ((payload[14] & 0x0f)*4);

			bool isResponse = (payload[24 + ipHeaderLength] & (1 << 7)) != 0;

			if (!isResponse) return;

			if (payload[26 + ipHeaderLength] != 0x00 || payload[27 + ipHeaderLength] != 0x01) return;
			if (payload[28 + ipHeaderLength] == 0x00 && payload[29 + ipHeaderLength] == 0x00) return;

			string name = DecodeDnsName(payload, 34 + ipHeaderLength);

			int startOfAnswers = 40 + ipHeaderLength + name.Length;
			ushort answerCount = Utility.ExtractRangeFromArray(payload, 48, 2).ToShort();

			DnsAnswer[] answers = ParseAnswers(payload, startOfAnswers, answerCount);

			foreach (DnsAnswer answer in answers)
			{
				if (answer.Type == 1)
				{
					answer.Name = name;

					if (DnsCache.Contains(answer)) DnsCache[DnsCache.IndexOf(answer)] = answer;
					else DnsCache.Add(answer);

					if (name == _syncDnsLookupQuery)
					{
						_syncDnsLookupResult = answer.Value;
						DnsWaitHandle.Set();
					}
					else
					{
						if(OnDnsLookupEvent != null) OnDnsLookupEvent.Invoke(name, answer.Value);
					}

					RemoveExpiredAnswers();

					return;
				}
			}
		}

		#endregion
	}

	internal class DnsAnswer
	{
		public string Name { get; set; }
		public ushort Type { get; set; } 
		public TimeSpan Expiration { private get; set; }
		public byte[] Value { get; set; }

		public override bool Equals(object answer)
		{
			var dnsAnswer = answer as DnsAnswer;
			return dnsAnswer != null && Name == dnsAnswer.Name;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		internal bool IsExpired()
		{
			return PowerState.Uptime > Expiration;
		}
	}
}