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
	internal static class LLMNR 
	{
		#region Fields
		
		private static readonly byte[] Prefix =
		{
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08,
			0x00, 0x45, 0x00, 0x00, 0x52, 0x08, 0xfd, 0x00, 0x00, 0x80, 0x11, 0xad, 0xa5, 0xc0, 0xa8, 0x01, 0x52, 0xc0, 0xa8,
			0x01, 0x56, 0x14, 0xeb, 0xd5, 0x98, 0x00, 0x3e, 0xb9, 0xe0, 0x52, 0xcb, 0x80, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00,
			0x00, 0x00, 0x00
		};

		private static readonly object LockObj = new Object(); 

		#endregion

		#region Internal Methods
		
		internal static void HandlePacket(byte[] payload)
		{
			var ipHeaderLength = (ushort) ((payload[14] & 0x0f)*4);
			string name = DNS.DecodeDnsName(payload, 34 + ipHeaderLength); // Name from first Query

			bool isQuery = (payload[24 + ipHeaderLength] & (1 << 7)) == 0; // DNS Query ?
			if (!isQuery) return;

			bool isTypeA = payload[payload.Length - 3] == 0x01;
			if (!isTypeA) return;

			if (payload[10 + ipHeaderLength] != 0xe0 || payload[11 + ipHeaderLength] != 0x00 ||
			    payload[12 + ipHeaderLength] != 0x00 || payload[13 + ipHeaderLength] != 0xfc) return;


			if (name != EthClick._name + ".local" && name != EthClick._name) return;

			#if TINYCLR_TRACE
			if (EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("Local Name Request (LLMNR, Type A) for " + name);
			#endif

			SendLLMNRNameReply(name, Utility.ExtractRangeFromArray(payload, 42, 2), Utility.ExtractRangeFromArray(payload, 6, 6),
				Utility.ExtractRangeFromArray(payload, 26, 4), Utility.ExtractRangeFromArray(payload, 34, 2));
		}

		#endregion

		#region Private Methods

		private static void SendLLMNRNameReply(string name, byte[] tranId, byte[] destinationMac, byte[] destinationIp, byte[] destinationPort)
		{
			if (EthClick._ip == null || EthClick._name == string.Empty) return;

			lock (LockObj)
			{
				Prefix.Overwrite(0, destinationMac);
				Prefix.Overwrite(6, EthClick._macAddress);
				Prefix.Overwrite(26, EthClick._ip);
				Prefix.Overwrite(30, destinationIp);
				Prefix.Overwrite(36, destinationPort);
				Prefix.Overwrite(42, tranId);

				var suffix = new byte[name.Length * 2 + 22];
				byte[] byteName = Utility.CombineArrays(DNS.EncodeDnsName(name), new byte[] {0x00, 0x01, 0x00, 0x01});
				suffix.Overwrite(0, Utility.CombineArrays(byteName, byteName));
				suffix.Overwrite(suffix.Length - 7, new byte[] {0x1e});
				suffix.Overwrite(suffix.Length - 5, new byte[] {0x04});
				suffix.Overwrite(suffix.Length - 4, EthClick._ip);

				byte[] result = Utility.CombineArrays(Prefix, suffix);

				result.Overwrite(16, ((ushort) (result.Length - 14)).ToBytes());
				result.Overwrite(38, ((ushort) (result.Length - 34)).ToBytes());
				result.Overwrite(24, new byte[] {0x00, 0x00}); 
				result.Overwrite(24, result.InternetChecksum(20, 14));
				result.Overwrite(40, new byte[] {0x00, 0x00}); 

				EthClick.SendFrame(result);

				#if TINYCLR_TRACE
				if(EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("LLMNR Response sent");
				#endif
			}
		}
 
		#endregion	

	}
}