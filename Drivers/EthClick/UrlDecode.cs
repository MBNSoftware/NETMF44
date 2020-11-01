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
	/// A class of HTTP Utility methods.
	/// </summary>
	public class HttpUtility
	{
		#region Public Methods
		
		/// <summary>
		/// Decodes a URL encoded query string into a human readable string.
		/// </summary>
		/// <param name="encodedString">URL Encoded string, you know with the %20 instead of spaces and stuff like that.</param>
		/// <param name="replacePlus">Replaces the Plus sign "+" with a space character if set to true.</param>
		/// <returns>The Human readable URL string.</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		/// string url = "http://services.odata.org/V2/(S(fhsma0h0orzoqo55jjrw1wyq))/OData/OData.svc/";
	    /// var encodedURL = HttpUtility.UrlEncode(url);
	    /// Debug.Print(HttpUtility.UrlDecode(encodedURL));
		/// </code>
		/// <code language = "VB">
		/// Dim url as String = "http://services.odata.org/V2/(S(fhsma0h0orzoqo55jjrw1wyq))/OData/OData.svc/"
	    /// Dim encodedURL As String = HttpUtility.UrlEncode(url)
	    /// Debug.Print(HttpUtility.UrlDecode(encodedURL))
		/// </code>
		/// </example>
		public static string UrlDecode(string encodedString, bool replacePlus = true)
		{
			var outStr = string.Empty;

			var i = 0;
			while (i < encodedString.Length)
			{
				switch (encodedString[i])
				{
					case '+':
						outStr += (replacePlus ? ' ' : encodedString[i]);
						break;
					case '%':
						outStr += Convert.ToChar((ushort)((HexToInt(encodedString[i + 1]) * 16) + HexToInt(encodedString[i + 2])));
						i += 2;
						break;
					default:
						outStr += encodedString[i];
						break;
				}
				i++;
			}
			return outStr;
		}

		/// <summary>
		/// Encodes an plain URL string and returns the encoded string
		/// </summary>
		/// <param name="plainString">Plain URL string. Replaces spaces with %20</param>
		/// <param name="encodePeriod">If false, it does not encode the period.</param>
		/// <returns>The URL encoded string</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		/// string url = "http://services.odata.org/V2/(S(fhsma0h0orzoqo55jjrw1wyq))/OData/OData.svc/";
		/// var encodedURL = HttpUtility.UrlEncode(url);
		/// Debug.Print(HttpUtility.UrlDecode(encodedURL));
		/// </code>
		/// <code language = "VB">
		/// Dim url as String = "http://services.odata.org/V2/(S(fhsma0h0orzoqo55jjrw1wyq))/OData/OData.svc/"
		/// Dim encodedURL As String = HttpUtility.UrlEncode(url)
		/// Debug.Print(HttpUtility.UrlDecode(encodedURL))
		/// </code>
		/// </example>
		public static string UrlEncode(string plainString, bool encodePeriod = true)
		{
			var outStr = string.Empty;
			var i = 0;

			while (i < plainString.Length)
			{
				var charCode = (int)plainString[i];

				if (charCode == 32) outStr += "+";
				else if (charCode == 46 && encodePeriod == false) outStr += ".";
				else if ((charCode >= 65 && charCode <= 90) || (charCode >= 97 && charCode <= 122) || (charCode >= 48 && charCode <= 57)) outStr += plainString[i];
				else if (charCode == 36 || charCode == 40 || charCode == 41 || charCode == 47 || charCode == 92) outStr += plainString[i];
				else outStr += "%" + charCode.ToString("X");

				i++;
			}

			return outStr;
		}

		#endregion

		#region Private Methods
	
		private static int HexToInt(char ch)
		{
			return
				(ch >= '0' && ch <= '9')
					? ch - '0'
					: (ch >= 'a' && ch <= 'f')
						? ch - 'a' + 10
						: (ch >= 'A' && ch <= 'F')
							? ch - 'A' + 10
							: -1;
		} 

		#endregion
	
	}
}