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
	internal static class ARP
	{
		#region Fields
		
		private static readonly byte[] Scratch =
		{
			0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x08, 0x06, 0x00, 0x01, 0x08, 0x00, 0x06,
			0x04, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
			0x00, 0x00, 0x00, 0x00
		}; 

		private static readonly byte[] Request = {0x01};
		private static readonly byte[] Reply = {0x02};

		#endregion

		#region Internal Methods

		internal static void HandlePacket(byte[] payload)
		{
			if (payload[21] == 0x01 &&
			    ((payload[32] == 0xff && payload[33] == 0xff && payload[34] == 0xff && payload[35] == 0xff && payload[36] == 0xff &&
			      payload[37] == 0xff) ||
			     (payload[32] == 0x00 && payload[33] == 0x00 && payload[34] == 0x00 && payload[35] == 0x00 && payload[36] == 0x00 &&
			      payload[37] == 0x00) ||
				 (payload[32] == EthClick._macAddress[0] && payload[33] == EthClick._macAddress[1] &&
				  payload[34] == EthClick._macAddress[2] && payload[35] == EthClick._macAddress[3] &&
				  payload[36] == EthClick._macAddress[4] && payload[37] == EthClick._macAddress[5])))
			{
				SendARP_Reply(Utility.ExtractRangeFromArray(payload, 6, 6), Utility.ExtractRangeFromArray(payload, 28, 4));
			}
			else if (payload[21] == 0x02)
			{
				if (EthClick._defaultGateway != null && payload[28] == EthClick._defaultGateway[0] && payload[29] == EthClick._defaultGateway[1] &&
				    payload[30] == EthClick._defaultGateway[2] && payload[31] == EthClick._defaultGateway[3])
				{
					if (EthClick._gatewayMac == null && EthClick._dhcpDisabled && EthClick._ip != null)
					{
						EthClick._gatewayMac = Utility.ExtractRangeFromArray(payload, 22, 6);
						EthClick.StartupHold.Set(); 
					}
					else
					{
						EthClick._gatewayMac = Utility.ExtractRangeFromArray(payload, 22, 6);
					}
				}
			}
		}

		internal static void SendARP_Gratuitus()
		{
			Scratch.Overwrite(0, EthClick.BroadcastMac);
			Scratch.Overwrite(6, EthClick._macAddress);
			Scratch.Overwrite(21, Request);
			Scratch.Overwrite(22, EthClick._macAddress);
			Scratch.Overwrite(28, EthClick._ip ?? new byte[] {0x00, 0x00, 0x00, 0x00});
			Scratch.Overwrite(32, EthClick.BlankMac);
			Scratch.Overwrite(38, EthClick._ip ?? new byte[] {0x00, 0x00, 0x00, 0x00});

			EthClick.SendFrame(Scratch);
		}

		internal static void SendARP_Probe(byte[] ipAddressToQuery)
		{
			if (ipAddressToQuery == null) return;

			Scratch.Overwrite(0, EthClick.BroadcastMac);
			Scratch.Overwrite(6, EthClick._macAddress);
			Scratch.Overwrite(21, Request);
			Scratch.Overwrite(22, EthClick._macAddress);
			Scratch.Overwrite(28, EthClick._ip ?? new byte[] {0x00, 0x00, 0x00, 0x00});
			Scratch.Overwrite(32, EthClick.BroadcastMac);
			Scratch.Overwrite(38, ipAddressToQuery);

			EthClick.SendFrame(Scratch);
		}
		
		#endregion
	
		#region Private Methods
		
		private static void SendARP_Reply(byte[] destinationMac, byte[] destinationIp)
		{
			Scratch.Overwrite(0, destinationMac);
			Scratch.Overwrite(6, EthClick._macAddress);
			Scratch.Overwrite(21, Reply);
			Scratch.Overwrite(22, EthClick._macAddress);
			Scratch.Overwrite(28, EthClick._ip ?? new byte[] {0x00, 0x00, 0x00, 0x00});
			Scratch.Overwrite(32, destinationMac);
			Scratch.Overwrite(38, destinationIp);

			EthClick.SendFrame(Scratch);
		}

		#endregion	

	}
}