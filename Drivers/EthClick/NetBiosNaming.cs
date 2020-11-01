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
	internal static class NetBiosNaming
	{
		#region Fields
		
		private static readonly byte[] Scratch =
		{
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x45, 0x00, 0x00, 0x5a, 0x00,
			0x05, 0x00, 0x00, 0xff, 0x11, 0x37, 0x8d, 0xc0, 0xa8, 0x01, 0x5a, 0xc0, 0xa8, 0x01, 0x56, 0x00, 0x89, 0x00, 0x89,
			0x00, 0x46, 0x00, 0x00, 0xdd, 0x1a, 0x85, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x20, 0x45, 0x4f,
			0x45, 0x46, 0x46, 0x45, 0x45, 0x45, 0x46, 0x46, 0x45, 0x4a, 0x45, 0x4f, 0x45, 0x50, 0x43, 0x41, 0x43, 0x41, 0x43,
			0x41, 0x43, 0x41, 0x43, 0x41, 0x43, 0x41, 0x43, 0x41, 0x41, 0x41, 0x00, 0x00, 0x20, 0x00, 0x01, 0x00, 0x00, 0x0f,
			0x0f, 0x00, 0x06, 0x60, 0x00, 0xc0, 0xa8, 0x01, 0x5a
		};

		#endregion
		
		#region Internal Methods

		internal static void HandlePacket(byte[] payload)
		{
			if ((payload[44] >> 3) == 0) 
			{
				var nbName = new byte[32];

				Array.Copy(payload, 55, nbName, 0, 32);

				#if TINYCLR_TRACE
				if (EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("Netbios name query for: " + DecodeNetbiosName(nbName));
				#endif

				if (DecodeNetbiosName(nbName).Trim().ToLower() == EthClick._name ||
					DecodeNetbiosName(nbName).Trim().ToLower() == EthClick._name + ".local")
					SendNetbiosReply(nbName, Utility.ExtractRangeFromArray(payload, 6, 6),
						Utility.ExtractRangeFromArray(payload, 26, 4), Utility.ExtractRangeFromArray(payload, 42, 2));
			}
		}

		#endregion
	
		#region Private Methods
		
		private static void SendNetbiosReply(byte[] nbName, byte[] destinationMac, byte[] destinationIp, byte[] transactionId)
		{
			Scratch.Overwrite(0, destinationMac);
			Scratch.Overwrite(6, EthClick._macAddress);
			Scratch.Overwrite(26, EthClick._ip);
			Scratch.Overwrite(30, destinationIp);
			Scratch.Overwrite(42, transactionId);
			Scratch.Overwrite(55, nbName);
			Scratch.Overwrite(100, EthClick._ip);
			Scratch.Overwrite(24, new byte[] {0x00, 0x00});
			Scratch.Overwrite(24, Scratch.InternetChecksum(20, 14));

			EthClick.SendFrame(Scratch);
		}

		private static string DecodeNetbiosName(byte[] netBiosName)
		{
			var result = string.Empty;
			for (var i = 0; i < 15; i++)
			{
				byte b1 = netBiosName[i * 2];
				byte b2 = netBiosName[(i * 2) + 1];
				var c = (char) (((b1 - 65) << 4) | (b2 - 65));
				result += c;
			}

			return result;
		}
 
		#endregion
	
	}
}