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

namespace MBN.Modules
{
	/// <summary>
	/// The class containing the TCP/UDP Packet information.
	/// </summary>
	public class Packet
    {
		/// <summary>
		/// Default constructor of the Packet class.
		/// </summary>
		/// <param name="type">See <see cref="PacketType"/></param>
		public Packet(PacketType type)
		{
			PacketType = type;
		}

		/// <summary>
		/// The Packet type. See <see cref="PacketType"/>
		/// </summary>
		public PacketType PacketType;

		/// <summary>
		/// The content of the packet.
		/// </summary>
		public byte[] Content { get; set; }

		/// <summary>
		/// The Sequence Number of the packet.
		/// </summary>
		public uint SequenceNumber { get; set; }

		/// <summary>
		/// The connecting Socket that sent the packet.
		/// </summary>
		public Connection Socket { get; set; }

    }

	/// <summary>
	/// Packet type enumeration. either TCP or UDP
	/// </summary>
	public enum PacketType
	{
		/// <summary>
		/// Packet Type TCP
		/// </summary>
		TCP,
		/// <summary>
		/// Packet type UDP
		/// </summary>
		UDP
	}

}
