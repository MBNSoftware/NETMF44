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
using Microsoft.SPOT.Hardware;

namespace MBN.Modules
{
	internal static class MDNS
	{
		#region Fields
		
		private static readonly byte[] Prefix =
		{
			0x01, 0x00, 0x5e, 0x00, 0x00, 0xfb, 0x00, 0x0, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x45, 0x00, 0x00, 0x74, 0x01, 0xc5,
			0x00, 0x00, 0xff, 0x11, 0x16, 0xb0, 0xc0, 0xa8, 0x01, 0x00, 0xe0, 0x00, 0x00, 0xfb, 0x14, 0xe9, 0x14, 0xe9, 0x00,
			0x60, 0xa3, 0x75, 0x00, 0x00, 0x84, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01
		};

		private static readonly byte[] Suffix =
		{
			0x00, 0x01, 0x80, 0x01, 0x00, 0x00, 0x00, 0x78, 0x00, 0x04, 0xc0, 0xa8, 0x01, 0x00, 0xc0, 0x0c, 0x00, 0x2f, 0x80,
			0x01, 0x00, 0x00, 0x00, 0x78, 0x00, 0x05, 0xc0, 0x0c, 0x00, 0x01, 0x40
		}; 

		private static readonly object LockObj = new Object();

		#endregion

		#region Internal Methods

		internal static void HandlePacket(byte[] payload)
		{
			var ipHeaderLength = (ushort) ((payload[14] & 0x0f)*4);
			var name = DNS.DecodeDnsName(payload, 34 + ipHeaderLength);

            #if TINYCLR_TRACE
			if (EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("Local Name Request (MDNS) for " + name);
			#endif

			var isQuery = (payload[24 + ipHeaderLength] & (1 << 7)) == 0; 
			if (!isQuery) return;

			if (payload[10 + ipHeaderLength] != 0xe0 || payload[11 + ipHeaderLength] != 0x00 ||
			    payload[12 + ipHeaderLength] != 0x00 || payload[13 + ipHeaderLength] != 0xfb) return;

			if (name != EthClick._name + ".local") return; 
			
			SendMDNSNameReply();
		}

		#endregion

		#region Private Methods

		private static void SendMDNSNameReply()
		{
			if (EthClick._ip == null || EthClick._name == string.Empty) return;

			Prefix.Overwrite(0, new byte[] {0x01, 0x00, 0x5e, 0x00, 0x00, 0xfb});
			Prefix.Overwrite(6, EthClick._macAddress);
			Prefix.Overwrite(26, EthClick._ip); 

			lock (LockObj)
			{
				Suffix.Overwrite(10, EthClick._ip);

				var result = Utility.CombineArrays(Prefix, Utility.CombineArrays(DNS.EncodeDnsName(EthClick._name + ".local"), Suffix));

				result.Overwrite(16, ((ushort) (result.Length - 14)).ToBytes()); 
				result.Overwrite(38, ((ushort) (result.Length - 34)).ToBytes()); 
				result.Overwrite(24, new byte[] {0x00, 0x00}); 
				result.Overwrite(24, result.InternetChecksum(20, 14)); 
				result.Overwrite(40, new byte[] {0x00, 0x00});

				#if TINYCLR_TRACE
				if (EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("Sending MDNS name message!");
				#endif

				EthClick.SendFrame(result);
			}
		}

		#endregion
	}
}