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

namespace MBN.Modules
{
	/// <summary>
	/// A class of extension methods used by the EthClick driver class.
	/// </summary>
	public static class Extensions
    {
		#region Internal Methods

		internal static void Overwrite(this byte[] origin, int start, byte[] source)
		{
			foreach (var aByte in source) origin[start++] = aByte;
		}

		internal static int Locate(this byte[] buffer, byte[] pattern)
		{
			for (int i = 0; i < buffer.Length; i++)
			{
				if (pattern[0] == buffer[i] && buffer.Length - i >= pattern.Length)
				{
					var ismatch = true;
					for (var j = 1; j < pattern.Length && ismatch; j++)
					{
						if (buffer[i + j] != pattern[j])
						{
							ismatch = false;
						}
					}

					if (ismatch) return i;
				}
			}

			return -1;
		}

		internal static byte[] GetRandomBytes(short count)
		{
			var random = new Random();
			var buffer = new byte[count];
			random.NextBytes(buffer);

			return buffer;
		}

		internal static byte[] InternetChecksum(this byte[] buffer, int length = 0, int start = 0, byte[] sourceIp = null, byte[] destiantionIp = null, byte protocol = 0x06)
		{
			length = length == 0 ? buffer.Length : length;
			var i = start;
			UInt32 sum = 0;
			byte[] pseudoHeader = null;

			if (sourceIp != null && destiantionIp != null)
			{
				pseudoHeader = new byte[] { sourceIp[0], sourceIp[1], sourceIp[2], sourceIp[3], destiantionIp[0], destiantionIp[1], destiantionIp[2], destiantionIp[3], 0x00, protocol, (byte)(length >> 8), (byte)(length >> 0) };
				length += pseudoHeader.Length;
				i -= pseudoHeader.Length;
			}

			while (length > 1)
			{
				UInt32 data = 0;

				if (i < start)
				{
					if (pseudoHeader != null) data = ((UInt32)(pseudoHeader[(i - start) + pseudoHeader.Length]) << 8) | ((UInt32)(pseudoHeader[(i - start) + pseudoHeader.Length + 1]) & 0xFF);
				}
				else
				{
					data = ((UInt32)(buffer[i]) << 8) | ((UInt32)(buffer[i + 1]) & 0xFF);
				}

				sum += data;

				if ((sum & 0xFFFF0000) > 0)
				{
					sum = sum & 0xFFFF;
					sum += 1;
				}

				i += 2;
				length -= 2;
			}

			if (length > 0)
			{
				sum += (UInt32)(buffer[i] << 8);
				if ((sum & 0xFFFF0000) > 0)
				{
					sum = sum & 0xFFFF;
					sum += 1;
				}
			}

			sum = ~sum;
			sum = sum & 0xFFFF;

			var result = new byte[2];

			result[0] = (byte)(sum >> 8);
			result[1] = (byte)sum;

			return result;
		}

		internal static ulong ToLong(this byte[] array)
		{
			int pos = array.Length * 8;
			ulong result = 0;
			foreach (byte by in array)
			{
				pos -= 8;
				result |= (ulong)by << pos;
			}

			return result;
		}

		internal static uint ToInt(this byte[] array)
		{
			return (uint)ToLong(array);
		}

		internal static ushort ToShort(this byte[] array)
		{
			return (ushort)ToLong(array);
		}

		internal static byte[] ToBytes(this uint num)
		{
			return new[] { (byte)(num >> 24), (byte)(num >> 16), (byte)(num >> 8), (byte)(num >> 0) };
		} 

	    internal static bool BytesEqual(this byte[] array1, byte[] array2)
        {
            return array1.BytesEqual(0, array2, 0, array1.Length);
        }

		internal static byte[] ToBytes(this ushort num)
		{
			return new[] { (byte)(num >> 8), (byte)(num >> 0) };
		}

	    internal static string ToHexString(this byte[] buffer)
        {
			const string hexDigits = "0123456789ABCDEF";

			var chars = new char[buffer.Length * 2];

			for (short y = 0, x = 0; y < buffer.Length; ++y, ++x)
			{
				chars[x] = hexDigits[(buffer[y] & 0xF0) >> 4];
				chars[++x] = hexDigits[(buffer[y] & 0x0F)];
			}

			return new string(chars);
		}

		#endregion

		#region Private

		private static bool IsNumeric(this string value)
		{
			const string digits = "0123456789.-";

			var temp = Bytes2Chars(System.Text.Encoding.UTF8.GetBytes(value));
			foreach (var aChar in temp)
			{
				if (digits.IndexOf(aChar) == -1) return false;
			}
			return true;
		}

		private static char[] Bytes2Chars(byte[] input)
		{
			var output = new char[input.Length];
			for (var counter = 0; counter < input.Length; ++counter)
			{
				output[counter] = (char)input[counter];
			}
			return output;
		}

		private static bool BytesEqual(this byte[] array1, int start1, byte[] array2, int start2, int count)
		{
			bool result = true;
			for (int i = 0; i < count; i++)
			{
				if (array1[i + start1] != array2[i + start2])
				{
					result = false;
					break;
				}
			}
			return result;
		}

		#endregion

		#region Public

		/// <summary>
		/// Returns a standard dot separated IP Address, for example "192.168.1.255".
		/// </summary>
		/// <param name="array">The Byte Array containing the IPAddress to convert to a string.</param>
		/// <returns>The string representation of the IPAddress.</returns>
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
		public static string ToAddress(this byte[] array)
		{
			string result = string.Empty;

			if (array == null) return result;

			foreach (byte aByte in array)
			{
				result += ((uint)aByte) + ".";
			}
			return result.TrimEnd('.');
		}

	    /// <summary>
	    /// Returns the byte representation of an IP Address string such as "192.168.0.2"
	    /// </summary>
	    /// <param name="ipAddress">The IP Address string.</param>
	    /// <returns>The byte representation of the IP Address string.</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _eth.IPAddress = "192.168.1.95".ToBytes();
		/// </code>
		/// <code language = "VB">
		/// _eth.IPAddress = "192.168.1.95".ToBytes()
		/// </code>
		/// </example>
		public static byte[] ToBytes(this string ipAddress)
        {
            if (ipAddress.IndexOf('.') > 0)
            {
                var dnsParts = ipAddress.Split('.');

	            if (dnsParts.Length == 4 && dnsParts[3].IsNumeric() && dnsParts[2].IsNumeric() && dnsParts[1].IsNumeric() && dnsParts[0].IsNumeric())
	            {
		            return new[]
		            {
			            (byte) (ushort.Parse(dnsParts[0]) >> 0), (byte) (ushort.Parse(dnsParts[1]) >> 0),
			            (byte) (ushort.Parse(dnsParts[2]) >> 0), (byte) (ushort.Parse(dnsParts[3]) >> 0)
		            };
	            }
            }
            return null;
        }

		#endregion

    }
}
