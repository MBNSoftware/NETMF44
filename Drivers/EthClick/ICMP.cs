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
	internal static class ICMP
	{
		#region Fields
		
		private static readonly byte[] Scratch =
		{
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x45, 0x00, 0x00, 0x3c, 0x14,
			0xef, 0x00, 0x00, 0x80, 0x01, 0x53, 0xc4, 0xc0, 0xa8, 0x01, 0x56, 0x08, 0x08, 0x08, 0x08, 0x08, 0x00, 0x4d, 0x57,
			0x00, 0x01, 0x00, 0x04, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x6b, 0x6c, 0x6d, 0x6e, 0x6f,
			0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69
		};

		private static readonly byte[] Request = { 0x08 };
		private static readonly byte[] Reply = { 0x00 }; 

		#endregion

		#region Internal Methods

		internal static void HandlePacket(byte[] payload)
		{
			if (payload[34] == Request[0])
			{
				var destinationMac = Utility.ExtractRangeFromArray(payload, 6, 6);
				var destinationIp = Utility.ExtractRangeFromArray(payload, 26, 4);
				var id = Utility.ExtractRangeFromArray(payload, 38, 2);
				var sequence = Utility.ExtractRangeFromArray(payload, 40, 2);

				SendPING_Reply(destinationMac, destinationIp, id, sequence);

				EthClick.FirePingReceivedEvent(destinationMac, destinationIp, id, sequence);
			}
			else if (payload[34] == Reply[0])
			{
				#if TINYCLR_TRACE
				if (EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("Received Ping response.");
				#endif	
			}

			#if TINYCLR_TRACE
			if (EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("Ping received");
			#endif
		}
		
		#endregion

		#region Public Methods
		
		/// <summary>
		/// Send a PING Request to the specified IP Address.
		/// </summary>
		/// <param name="ipAddress">The byte encoded IPAddress.</param>
		public static void SendPingRequest(byte[] ipAddress)
		{
			if (EthClick._gatewayMac == null || EthClick._ip == null) return;

			Scratch.Overwrite(0, EthClick._gatewayMac);
			Scratch.Overwrite(6, EthClick._macAddress);
			Scratch.Overwrite(26, EthClick._ip);
			Scratch.Overwrite(30, EthClick._ip);
			Scratch.Overwrite(34, Request);
			Scratch.Overwrite(24, new byte[] { 0x00, 0x00 }); // clear header checksum, so that calculation excludes the checksum itself
			Scratch.Overwrite(24, Scratch.InternetChecksum(20, 14)); // header checksum

			EthClick.SendFrame(Scratch);
		}

		#endregion

		#region Private Methods
		
		private static void SendPING_Reply(byte[] destinationMac, byte[] destinationIp, byte[] id, byte[] seq)
		{
			if (EthClick._gatewayMac == null || EthClick._ip == null) return;

			#if TINYCLR_TRACE
			if(EthClick._verboseDebugging) System.Diagnostics.Debug.WriteLine("Sending Response to Ping request");
			#endif
			
			Scratch.Overwrite(0, destinationMac);
			Scratch.Overwrite(6, EthClick._macAddress);
			Scratch.Overwrite(26, EthClick._ip);
			Scratch.Overwrite(30, destinationIp);
			Scratch.Overwrite(34, Reply);
			Scratch.Overwrite(38, id);
			Scratch.Overwrite(40, seq);

			Scratch.Overwrite(36, new byte[] { 0x00, 0x00 });
			Scratch.Overwrite(36, Scratch.InternetChecksum(40, 34));

			Scratch.Overwrite(24, new byte[] { 0x00, 0x00 });
			Scratch.Overwrite(24, Scratch.InternetChecksum(20, 14));

			EthClick.SendFrame(Scratch);
		} 

		#endregion
	}
}