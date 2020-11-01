/*
 * OLED-C Click driver.
 * 
 * Initial version coded by Stephen Cardinale
 * 
 *  - ToDo - Add Methodology and Property/Method for Display Orientation.
 *  
 * References needed:
 *  <icrosoft.SPOT.Graphics
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Native
 *  MikroBusNet
 *  mscorlib
 *  
 * Copyright 2014 Stephen Cardinale
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System;
using MBN.Modules.Properties;

namespace MBN.Modules
{
	/// <summary>
	/// Manages font-related functionality.
	/// </summary>
	public static class FontManager
	{
		#region ENUMS
		
		/// <summary>
		/// A set of predefined Fonts of varying sizes (8-24 points).
		/// </summary>
		public enum FontName
		{
			/// <summary>
			/// Tahoma 7 point without Extended ASCII Characters
			/// </summary>
			TahomaReg7,

			/// <summary>
			/// Tahoma 7 point with Extended ASCII Characters
			/// </summary>
			TahomaExt7,

			/// <summary>
			/// Tahoma 12 point without Extended ASCII Characters
			/// </summary>
			TahomaReg12,

			/// <summary>
			/// Tahoma 12 point with Extended ASCII Characters
			/// </summary>
			TahomaExt12,

		}

		#endregion
		
		#region Public Methods
		
		/// <summary>
		/// Returns a MikroFont resource specified by a predefined font.
		/// </summary>
		/// <param name="font">The predefined font</param>
		/// <returns>A Font usable by the OLED-C Click driver.</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		/// private static readonly MikroFont _font1 = FontManager.GetFont(FontManager.FontName.TahomaReg7);
		/// </code>
		/// <code language = "VB">
		/// Private Shared ReadOnly _font1 As MikroFont = FontManager.GetFont(FontManager.FontName.TahomaReg7)
		/// </code>
		/// </example>
		public static MikroFont GetFont(FontName font)
		{
			switch (font)
			{
				case FontName.TahomaReg7:
					return new MikroFont(Resources.GetBytes(Resources.BinaryResources.tahoma_7));
				case FontName.TahomaExt7:
					return new MikroFont(Resources.GetBytes((Resources.BinaryResources.tahoma_7e)));
				case FontName.TahomaReg12:
					return new MikroFont(Resources.GetBytes((Resources.BinaryResources.tahoma_12)));
				case FontName.TahomaExt12:
					return new MikroFont(Resources.GetBytes((Resources.BinaryResources.tahoma_12e)));
				default:
					throw new ArgumentException("No such font exists.");
			}
		}

		#endregion
	}
}

