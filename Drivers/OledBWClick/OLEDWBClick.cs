/*
 * OLED W/B Click board driver.
 *  - This driver can be used for the OLED W or OLED B Click boards by MikroE.
 * Version 1.0 :
 *  - Initial version coded by Stephen Cardinale
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

using MBN.Enums;
using MBN.Extensions;
using MBN.Exceptions;
using Microsoft.SPOT.Hardware;
using System;
using System.Reflection;
using System.Threading;
using Math = System.Math;

namespace MBN.Modules
{

	/// <summary>
	/// A MikroBusNet Driver for the MikroE Eth Click.
	/// <para><b>This module is an SPI Device</b></para>
	/// <para><b>Pins used :</b> Miso, Mosi, Sck, Cs, Rst and Pwm</para>
	/// <para><b>References required:</b> MikroBus.Net, Microsoft.SPOT.Native, Microsoft.Spot.Hardware, mscorlib</para>
	/// </summary>
	/// <example>Example usage:
	/// <code language = "C#">
	/// using MBN;
	/// using MBN.Enums;
	/// using MBN.Modules;
	/// using Bitmap = MBN.Modules.Bitmap;
	/// using System.Threading;
	///
	/// namespace OLEDClickTestApp
	/// {
	/// 	public class Program
	/// 	{
	/// 		private static OLEDWBClick _oled;
	///
	/// 		public static void Main()
	/// 		{
	/// 			_oled = new OLEDWBClick(Hardware.SocketThree) { AutoRefresh = true };
	///
	/// 			// Draw a bitmap to the OLED Display from an embedded resource.
	/// 			var logo = Bitmap.Parse(Resources.GetString(Resources.StringResources.mbn_logo_white));
	/// 			_oled.DrawBitmap(logo.BitmapDataBytes, 0, 0, logo.Width, logo.Height, OLEDWBClick.Colors.White);
	///
	/// 			Thread.Sleep(Timeout.Infinite);
	/// 		}
	///		}
	/// }
	/// </code>
	/// <code language = "VB">
	/// Option Explicit On
	/// Option Strict On
	///
	/// Imports MBN.Modules
	/// Imports MBN
	/// Imports System.Threading
	///
	/// Namespace MFConsoleApplication1
	///
	/// 	Public Module Module1
	///
	/// 	Dim WithEvents _oled As OLEDWBClick
	///
	/// 		Sub Main()
	/// 			_oled = New OLEDWBClick(Hardware.SocketThree)
	/// 			_oled.AutoRefresh = True
	///
	/// 			' Draw a bitmap to the OLED Display from an embedded resource.
	/// 			Dim logo As Bitmap = Bitmap.Parse(Resources.GetString(Resources.StringResources.mbn_logo_white))
	/// 			_oled.DrawBitmap(logo.BitmapDataBytes, 0, 0, logo.Width, logo.Height, OLEDWBClick.Colors.White)
	///
	/// 			Thread.Sleep(Timeout.Infinite)
	/// 		End Sub
	/// 	End Module
	/// End Namespace
	/// </code>
	/// </example>
	public class OLEDWBClick : IDriver
    {

		#region Constants

		private const byte SSD1306_LCDWIDTH = 96;
		private const byte SSD1306_LCDHEIGHT = 40;

		private const byte SSD1306_SETCONTRAST = 0x81;
		private const byte SSD1306_DISPLAYALLON_RESUME = 0xA4;
		private const byte SSD1306_DISPLAYALLON = 0xA5;
		private const byte SSD1306_NORMALDISPLAY = 0xA6;
		private const byte SSD1306_INVERTDISPLAY = 0xA7;
		private const byte SSD1306_DISPLAYOFF = 0xAE;
		private const byte SSD1306_DISPLAYON = 0xAF;

		private const byte SSD1306_SETDISPLAYOFFSET = 0xD3;
		private const byte SSD1306_SETCOMPINS = 0xDA;

		private const byte SSD1306_SETVCOMDETECT = 0xDB;

		private const byte SSD1306_SETDISPLAYCLOCKDIV = 0xD5;
		private const byte SSD1306_SETPRECHARGE = 0xD9;

		private const byte SSD1306_SETMULTIPLEX = 0xA8;

		private const byte SSD1306_SETLOWCOLUMN = 0x00;
		private const byte SSD1306_SETHIGHCOLUMN = 0x10;

		private const byte SSD1306_SETSTARTLINE = 0x40;

		private const byte SSD1306_MEMORYMODE = 0x20;

		private const byte SSD1306_COMSCANINC = 0xC0;
		private const byte SSD1306_COMSCANDEC = 0xC8;

		private const byte SSD1306_SEGREMAP = 0xA0;

		private const byte SSD1306_CHARGEPUMP = 0x8D;

		private const byte SSD1306_EXTERNALVCC = 0x1;
		private const byte SSD1306_SWITCHCAPVCC = 0x2;

		private const byte SSD1306_ACTIVATE_FADE_BLINK = 0x23;
		private const byte SSD1306_SET_ZOOM_IN = 0xD6;

		private const int SSD1306_ACTIVATE_SCROLL = 0x2F;
		private const int SSD1306_DEACTIVATE_SCROLL = 0x2E;
		private const int SSD1306_SET_VERTICAL_SCROLL_AREA = 0xA3;
		private const int SSD1306_RIGHT_HORIZONTAL_SCROLL = 0x26;
		private const int SSD1306_LEFT_HORIZONTAL_SCROLL = 0x27;
		private const int SSD1306_VERTICAL_AND_RIGHT_HORIZONTAL_SCROLL = 0x29;
		private const int SSD1306_VERTICAL_AND_LEFT_HORIZONTAL_SCROLL = 0x2A;

		#endregion

		#region Font

		static readonly byte[] Font5x7 =
		{
			0x00, 0x00, 0x00, 0x00, 0x00, // SPACE
			0x00, 0x00, 0x5F, 0x00, 0x00, // !
			0x00, 0x03, 0x00, 0x03, 0x00, // "
			0x14, 0x3E, 0x14, 0x3E, 0x14, // #
			0x24, 0x2A, 0x7F, 0x2A, 0x12, // $
			0x43, 0x33, 0x08, 0x66, 0x61, // % 
			0x36, 0x49, 0x55, 0x22, 0x50, // &
			0x00, 0x05, 0x03, 0x00, 0x00, // '
			0x00, 0x1C, 0x22, 0x41, 0x00, // (
			0x00, 0x41, 0x22, 0x1C, 0x00, // )
			0x14, 0x08, 0x3E, 0x08, 0x14, // *
			0x08, 0x08, 0x3E, 0x08, 0x08, // + 
			0x00, 0x50, 0x30, 0x00, 0x00, // , 
			0x08, 0x08, 0x08, 0x08, 0x08, // -
			0x00, 0x60, 0x60, 0x00, 0x00, // . 
			0x20, 0x10, 0x08, 0x04, 0x02, // / 
			0x3E, 0x51, 0x49, 0x45, 0x3E, // 0 
			0x00, 0x04, 0x02, 0x7F, 0x00, // 1
			0x42, 0x61, 0x51, 0x49, 0x46, // 2 
			0x22, 0x41, 0x49, 0x49, 0x36, // 3
			0x18, 0x14, 0x12, 0x7F, 0x10, // 4
			0x27, 0x45, 0x45, 0x45, 0x39, // 5
			0x3E, 0x49, 0x49, 0x49, 0x32, // 6
			0x01, 0x01, 0x71, 0x09, 0x07, // 7 
			0x36, 0x49, 0x49, 0x49, 0x36, // 8 
			0x26, 0x49, 0x49, 0x49, 0x3E, // 9
			0x00, 0x36, 0x36, 0x00, 0x00, // :
			0x00, 0x56, 0x36, 0x00, 0x00, // ;
			0x08, 0x14, 0x22, 0x41, 0x00, // <
			0x14, 0x14, 0x14, 0x14, 0x14, // =
			0x00, 0x41, 0x22, 0x14, 0x08, // >
			0x02, 0x01, 0x51, 0x09, 0x06, // ?
			0x3E, 0x41, 0x59, 0x55, 0x5E, // @
			0x7E, 0x09, 0x09, 0x09, 0x7E, // A
			0x7F, 0x49, 0x49, 0x49, 0x36, // B
			0x3E, 0x41, 0x41, 0x41, 0x22, // C
			0x7F, 0x41, 0x41, 0x41, 0x3E, // D 
			0x7F, 0x49, 0x49, 0x49, 0x41, // E 
			0x7F, 0x09, 0x09, 0x09, 0x01, // F
			0x3E, 0x41, 0x41, 0x49, 0x3A, // G
			0x7F, 0x08, 0x08, 0x08, 0x7F, // H
			0x00, 0x41, 0x7F, 0x41, 0x00, // I 
			0x30, 0x40, 0x40, 0x40, 0x3F, // J
			0x7F, 0x08, 0x14, 0x22, 0x41, // K
			0x7F, 0x40, 0x40, 0x40, 0x40, // L
			0x7F, 0x02, 0x0C, 0x02, 0x7F, // M 
			0x7F, 0x02, 0x04, 0x08, 0x7F, // N
			0x3E, 0x41, 0x41, 0x41, 0x3E, // O
			0x7F, 0x09, 0x09, 0x09, 0x06, // P
			0x1E, 0x21, 0x21, 0x21, 0x5E, // Q 
			0x7F, 0x09, 0x09, 0x09, 0x76, // R 
			0x26, 0x49, 0x49, 0x49, 0x32, // S
			0x01, 0x01, 0x7F, 0x01, 0x01, // T 
			0x3F, 0x40, 0x40, 0x40, 0x3F, // U
			0x1F, 0x20, 0x40, 0x20, 0x1F, // V
			0x7F, 0x20, 0x10, 0x20, 0x7F, // W 
			0x41, 0x22, 0x1C, 0x22, 0x41, // X
			0x07, 0x08, 0x70, 0x08, 0x07, // Y
			0x61, 0x51, 0x49, 0x45, 0x43, // Z 
			0x00, 0x7F, 0x41, 0x00, 0x00, // [
			0x02, 0x04, 0x08, 0x10, 0x20, // slash 
			0x00, 0x00, 0x41, 0x7F, 0x00, // ] 
			0x04, 0x02, 0x01, 0x02, 0x04, // ^
			0x40, 0x40, 0x40, 0x40, 0x40, // _
			0x00, 0x01, 0x02, 0x04, 0x00, // `
			0x20, 0x54, 0x54, 0x54, 0x78, // a 
			0x7F, 0x44, 0x44, 0x44, 0x38, // b 
			0x38, 0x44, 0x44, 0x44, 0x44, // character
			0x38, 0x44, 0x44, 0x44, 0x7F, // d
			0x38, 0x54, 0x54, 0x54, 0x18, // e
			0x04, 0x04, 0x7E, 0x05, 0x05, // f
			0x08, 0x54, 0x54, 0x54, 0x3C, // g
			0x7F, 0x08, 0x04, 0x04, 0x78, // height
			0x00, 0x44, 0x7D, 0x40, 0x00, // i 
			0x20, 0x40, 0x44, 0x3D, 0x00, // j 
			0x7F, 0x10, 0x28, 0x44, 0x00, // k 
			0x00, 0x41, 0x7F, 0x40, 0x00, // l
			0x7C, 0x04, 0x78, 0x04, 0x78, // m 
			0x7C, 0x08, 0x04, 0x04, 0x78, // n
			0x38, 0x44, 0x44, 0x44, 0x38, // o 
			0x7C, 0x14, 0x14, 0x14, 0x08, // p 
			0x08, 0x14, 0x14, 0x14, 0x7C, // q 
			0x00, 0x7C, 0x08, 0x04, 0x04, // radius 
			0x48, 0x54, 0x54, 0x54, 0x20, // s 
			0x04, 0x04, 0x3F, 0x44, 0x44, // t
			0x3C, 0x40, 0x40, 0x20, 0x7C, // u
			0x1C, 0x20, 0x40, 0x20, 0x1C, // v 
			0x3C, 0x40, 0x30, 0x40, 0x3C, // width 
			0x44, 0x28, 0x10, 0x28, 0x44, // centerX
			0x0C, 0x50, 0x50, 0x50, 0x3C, // centerY 
			0x44, 0x64, 0x54, 0x4C, 0x44, // z 
			0x00, 0x08, 0x36, 0x41, 0x41, // { 
			0x00, 0x00, 0x7F, 0x00, 0x00, // |
			0x41, 0x41, 0x36, 0x08, 0x00, // } 
			0x02, 0x01, 0x02, 0x04, 0x02  // ~
		};

		#endregion
		
		#region ENUMS
		
		/// <summary>
		/// The Direction enumeration used for scrolling methods of the OLED Display.
		/// </summary>
		public enum ScrollDirection
		{
			/// <summary>
			/// Scrolling direction is to the left..
			/// </summary>
			Left = 0,
			/// <summary>
			/// Scrolling direction is to the right.
			/// </summary>
			Right = 1
		}

		/// <summary>
		/// The color enumeration used for rendering pixels to the OLED Display.
		/// </summary>
		public enum Colors
		{
			/// <summary>
			/// Color White - i.e. White pixels are drawn as white.
			/// </summary>
			White = 0,
			/// <summary>
			/// Color Black - i.e. Black pixels are drawn as black.
			/// </summary>
			Black = 1,
			/// <summary>
			/// Inverse Color - i.e. Pixels are rendered the opposite color of the pixel at the current location.
			/// </summary>
			Inverse
		}

		/// <summary>
		/// The Orientation used to draw on the OLED display. 
		/// </summary>
		public enum Orientation
		{
			/// <summary>
			/// Normal orientation where the top of the screen is farthest away from the ribbon connector.
			/// </summary>
			Normal,
			/// <summary>
			/// Flipped orientation (Landscape - 180°) where the top of the screen is closest to the ribbon connector.
			/// </summary>
			Flipped
		}

		#endregion

		#region Fields

		private readonly OutputPort _dcPin;
		private readonly OutputPort _resetPin;

		private static bool _autoRefresh;
		private static PowerModes _powerMode = PowerModes.On;
		private Orientation _displayOrientation;
		private static byte _contrastLevel = 0x7F;
		private bool _displayOn;

		private static byte _displayOffset;

		private static readonly byte[] spiBuffer = new byte[1];
		private readonly SPI.Configuration _spiConfig;

		private static readonly byte[] displayBuffer = new byte[480];
		private static byte[] _font = Font5x7;

		#endregion

		#region CTOR

		/// <summary>
		/// Initializes a new instance of the <see cref="OLEDWBClick"/> class.
		/// </summary>
		/// <param name="socket">The socket on which the OLEDW Click or OLEDB Click board is inserted on MikroBus.Net</param>
		/// <exception cref="PinInUseException">If some pins are already used by another driver, then the exception is thrown.</exception>
		public OLEDWBClick(Hardware.Socket socket)
		{
			try
			{
				Hardware.CheckPins(socket, socket.Miso, socket.Cs, socket.Sck, socket.Rst, socket.Pwm);

				_spiConfig = new SPI.Configuration(socket.Cs, false, 0, 0, true, true, 40000, socket.SpiModule);

				if (Hardware.SPIBus == null)
				{
					Hardware.SPIBus = new SPI(_spiConfig);
				}

				_resetPin = new OutputPort(socket.Rst, false);
				_dcPin = new OutputPort(socket.Pwm, false);

				Init();

				DisplayOn = false;
				DisplayOrientation = Orientation.Normal;
				ClearDisplay(false);
				DisplayOn = true;
			}
			catch(PinInUseException ex)
			{
				throw new PinInUseException(ex.Message);
			}
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the AutoRefresh property. If true, rendering will automatically be done after each drawing method.
		/// </summary>
		/// <remarks>If composing a display screen by multiple graphics methods, disable the <see cref="AutoRefresh"/> property and then call the <see cref="Refresh"/> method to update the display. This will reduce screen flickering and decrease display time between graphic methods.</remarks>
		public bool AutoRefresh
		{
			get { return _autoRefresh; }
			set { _autoRefresh = value; }
		}

		/// <summary>
		/// Gets or Sets the OLED Display Contrast Level.
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		/// for (byte x = 255; x == 0; x--)
		/// {
		/// 	_oled.Contrast(x);
		/// 	Thread.Sleep(20);
		/// }
		/// Thread.Sleep(2000);
		/// for (byte x = 0; x == 255; x++)
		/// {
		///		_oled.Contrast(x);
		/// 	Thread.Sleep(20);
		/// }
		/// </code>
		/// <code language = "VB">
		/// For i As Integer = 255 To 0 
		/// 	_oled.Contrast(i)
		/// 	Thread.Sleep(20)
		/// Next
		/// Thread.Sleep(2000)
		/// For i As Integer = 0 To 255 
		/// 	_oled.Contrast(i)
		/// 	Thread.Sleep(20)
		/// Next
		/// </code>
		/// </example>
		public byte Contrast
		{
			get { return _contrastLevel; }
			set
			{
				WriteCommand(SSD1306_SETCONTRAST);
				WriteCommand((byte) (value + 0x01)); // contrast step 1 to 256}
				_contrastLevel = value;
			}
		}

		/// <summary>
		/// Gets or Sets the OLED Display Screen whether the screen is On or Off. 
		/// </summary>
		/// <remarks>This property only controls whether the OLED Display is on or off. It does not affect the internal display buffer or off-screen buffer used for rendering graphics to the display.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.DisplayOn = false;
		/// Thread.Sleep(2000);
		/// _oled.DisplayOn = true;
		/// </code>
		/// <code language = "VB">
		/// _oled.DisplayOn = False
		/// Thread.Sleep(2000)
		/// _oled.DisplayOn = True
		/// </code>
		/// </example>
		public bool DisplayOn
		{
			get { return _displayOn; }
			set
			{
				WriteCommand(value ? SSD1306_DISPLAYON : SSD1306_DISPLAYOFF);
				_displayOn = value;
			}
		}

		/// <summary>
		///     Gets the driver version.
		/// </summary>
		/// <value>
		///     The driver version see <see cref="Version"/>.
		/// </value>
		/// <example>Example usage:
		/// <code language="C#">
		/// Debug.Print("Driver Version Info : " + _oled.DriverVersion);
		/// </code>
		/// <code language="VB">
		/// Debug.Print("Driver Version Info : " <![CDATA[&]]> _oled.DriverVersion)
		/// </code>
		/// </example>
		public Version DriverVersion
		{
			get { return Assembly.GetAssembly(GetType()).GetName().Version; }
		}

		/// <summary>
		/// Gets or sets the Bitmap Font used for drawing text.
		/// </summary>
		/// <remarks>The default Font is 5x7 containing only ASSCI Characters 32 through 176. Non-printable and extended ASCII Characters are not supported by the drawing methods.</remarks>
		public byte[] Font
		{
			get { return _font; }
			set { _font = value; }
		}

		/// <summary>
		/// Gets or sets the OLED Display Orientation.
		/// </summary>
		/// <remarks>When changing the DisplayyOrientation, a <see cref="Refresh"/> will be required to re-draw the OLED Display. Otherwise the test will be drawn backwards (right to left).
		/// If the <see cref="AutoRefresh"/> property is set to true, this will be done automatically.
		/// </remarks>
		public Orientation DisplayOrientation
		{
			get { return _displayOrientation; }
			set
			{
				WriteCommand((byte)(value == Orientation.Flipped ? 0xC0 : 0xC8));
				WriteCommand((byte)(value == Orientation.Flipped ? SSD1306_SEGREMAP : SSD1306_SEGREMAP | 0x01));
				_displayOffset = (byte)(value == Orientation.Flipped ? 0x60 : 0x00);

				_displayOrientation = value;
				if (_autoRefresh) Refresh();
			}
		}

		/// <summary>
		/// Gets or sets the power mode.
		/// </summary>
		/// <value>
		/// The current power mode of the module.
		/// </value>
		/// <exception cref="NotSupportedException">A <see cref="NotSupportedException"/> will be thrown if attempting to set the PowerMode to <see cref="PowerModes.Low"/>.</exception>
		/// <example>None: This sensor does not support PowerMode.</example>
		public PowerModes PowerMode
		{
			get { return _powerMode; }
			set
			{
				if (value == PowerModes.Low) throw new NotSupportedException("This module does not support Low Power Mode.");

				if (_powerMode == PowerModes.On && value == PowerModes.Off)
				{
					WriteCommand(SSD1306_DISPLAYOFF);
					WriteCommand(SSD1306_CHARGEPUMP);
					WriteCommand(0x10);
					Thread.Sleep(100);
				}
				else if (_powerMode == PowerModes.Off && value == PowerModes.On)
				{
					WriteCommand(SSD1306_CHARGEPUMP);
					WriteCommand(0x14);
					Thread.Sleep(100);
					WriteCommand(SSD1306_DISPLAYON);
				}
			_powerMode = value;
			}
		}

		#endregion

		#region Private Methods
		
		private void Init()
		{
			_resetPin.Write(true);
			Thread.Sleep(10);
			_resetPin.Write(false);
			Thread.Sleep(10);
			_resetPin.Write(true);

			WriteCommand(SSD1306_DISPLAYOFF); // Turn off OLED panel

			WriteCommand(SSD1306_SETDISPLAYCLOCKDIV); // Set display clock divide ratio/oscillator frequency
			WriteCommand(0x80); // Set the divide ratio

			WriteCommand(SSD1306_SETMULTIPLEX); // Set the multiplex ratio(1 to 64)
			WriteCommand(0x26); // 38/64 duty cycle

			WriteCommand(SSD1306_SETDISPLAYOFFSET); // Set the display offset
			WriteCommand(0x00); // No offset

			WriteCommand(SSD1306_CHARGEPUMP); // Set the Charge Pump enable/disable
			WriteCommand(0x14); //--set(0x10) disable

			WriteCommand(SSD1306_SETSTARTLINE); // Set the start line address

			WriteCommand(SSD1306_NORMALDISPLAY); // Normal display

			WriteCommand(SSD1306_DISPLAYALLON_RESUME); // Disable Entire Display On

/* Added for testing */
			WriteCommand(SSD1306_MEMORYMODE); 
			WriteCommand((0x00));    
/* */

			WriteCommand(SSD1306_SEGREMAP | 0x01); // Set segment re-map 128 to 0

			WriteCommand(SSD1306_COMSCANDEC); // Set COM Output Scan Direction 64 to 0

			WriteCommand(SSD1306_SETCOMPINS); // Set the Com Pins hardware configuration
			WriteCommand(0x10);

			WriteCommand(SSD1306_SETCONTRAST); // Set the contrast control register
			WriteCommand(0x7F); // Default contrast.

			WriteCommand(SSD1306_SETPRECHARGE); // Set the pre-charge period
			WriteCommand(0xF1);

			WriteCommand(SSD1306_SETVCOMDETECT); // Set VCOMH
			WriteCommand(0x40);

			WriteCommand(SSD1306_SETLOWCOLUMN);
			WriteCommand(0x00);

			//WriteCommand(SSD1306_DISPLAYON); // Turn on the OLED panel
		}

		private static void InternalDrawChar(int x, int line, char character)
		{

			if ((x < 0) || (x >= SSD1306_LCDWIDTH)) throw new ArgumentException("The X parameter cannot be less than 0 or greater than the 96.", "x");
			if ((line < 0) || (line > 4)) throw new ArgumentException("The line parameter cannot be less than 0 or greater than the 4.", "line"); ;
			if ((character < 32) || (character > 176)) throw new ArgumentException("Invalid character parameter. Only ASCII Characters 32 through 176 are supported.", "character");

			for (int i = 0; i < 5; i++)
			{
				displayBuffer[x + (line * SSD1306_LCDWIDTH)] = _font[((character - 32) * 5) + i];
				x++;
			}
		}
		
		private static void InternalSetPixel(int x, int y, Colors color)
		{
			//if ((x < 0) || (x >= SSD1306_LCDWIDTH)) throw new ArgumentException("The X parameter cannot be less than 0 or greater than the 96", "x");
			//if ((y < 0) || (y >= SSD1306_LCDHEIGHT)) throw new ArgumentException("The Y parameter cannot be less than 0 or greater than the 40", "y"); ;

			if (y / 8 > 4) return; // Ran out of lines.

			switch (color)
			{
				case Colors.White:
				{
					displayBuffer[x + (y / 8) * SSD1306_LCDWIDTH] |= (byte)(1 << (y % 8));
					break;
				}
				case Colors.Black:
				{
					displayBuffer[x + (y / 8) * SSD1306_LCDWIDTH] &= (byte)~(1 << (y % 8));
					break;
				}
				case Colors.Inverse:
				{
					displayBuffer[x + (y / 8) * SSD1306_LCDWIDTH] ^= (byte)(1 << (y % 8));
					break;
				}
			}
		}

		private void SetColumnAddress(int columnAddress)
		{
			WriteCommand((byte)((0x10 | (columnAddress >> 4)) + 0x02));
			WriteCommand((byte)(0x0f & columnAddress));
		}

		private void SetPageAddress(int pageAddress)
		{
			pageAddress = 0xb0 | pageAddress;
			WriteCommand((byte)pageAddress);
		}
		
		private static void Swap(ref int a, ref int b)
		{
			var t = a; a = b; b = t;
		}

		private void Write(byte data)
		{
			spiBuffer[0] = data;
			Hardware.SPIBus.Write(_spiConfig, spiBuffer);
		}

		private void WriteCommand(byte command)
		{
			_dcPin.Write(false);
			Write(command);
		}

		private void WriteData(byte data)
		{
			_dcPin.Write(true);
			Write(data);
		} 

		#endregion

		#region Public Methods

		/// <summary>
		/// Causes the OLED Display to blink at the specified interval.
		/// </summary>
		/// <param name="blink">If true, it will cause the OLED display to blink at the specified interval, otherwise set to false to deactivate blinking.</param>
		/// <param name="delayBetweenTransitions">Optional - The delay as number of frames (0 to 127) to delay, 0x00 = 8 frames, 0x01 = 16 frames to 0x7F = 128 frames. Default value is 0x00 or 8 frames.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.Blink(true, 0x00); // Turn on blinking.
		/// Thread.sleep(10000); // Wait for 10 seconds.
		/// _oled.Blink(false); // Turn off blinking.
		/// </code>
		/// <code language = "VB">
		/// _oled.Blink(True, <![CDATA[&]]>H0) ' Turn on blinking.
		/// Thread.sleep(10000) ' Wait for 10 seconds.
		/// _oled.Blink(False) ' Turn off blinking.
		/// </code>
		/// </example>
		public void Blink(bool blink, byte delayBetweenTransitions = 0x00)
		{
			if (delayBetweenTransitions > 127) delayBetweenTransitions = 127;
			WriteCommand(SSD1306_ACTIVATE_FADE_BLINK);
			WriteCommand((byte)(blink ? 0x30 | delayBetweenTransitions : 0x00));
		}

		/// <summary>
		///  Clears OLED display.
		/// </summary>
		/// <param name="preserveBuffer">If true, the off screen displayBuffer used for rendering the screen will be preserved and the display will be cleared. If false, both the screen displayBuffer and display will be cleared.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.ClearDisplay(false); // Clears the OLED Display and clears the internal off screen buffer.
		/// _oled.ClearDisplay(true); // Clears the OLED Display and preserves the internal off screen buffer.
		/// </code>
		/// <code language = "VB">
		/// _oled.ClearDisplay(False) ' Clears the OLED Display and clears the internal off screen buffer.
		/// _oled.ClearDisplay(True) ' Clears the OLED Display and preserves the internal off screen buffer.
		/// </code>
		/// </example>
		public void ClearDisplay(bool preserveBuffer)
		{
			if (!preserveBuffer)
			{
				for (int i = 0; i < 480; i++)
				{
					displayBuffer[i] = 0;
				}
			}

			for (int y = 0; y < 5; y++)
			{
				SetPageAddress(y);
				SetColumnAddress(0x00);
				for (int x = 0; x <= 480; x++)
				{
					WriteData(0x00);
				}
			}
		}

		/// <summary>
		/// Renders a full screen Bitmap directly to the OLED display at location 0, 0.
		/// </summary>
		/// <remarks>This is useful to render a bitmapBytes that will occupy the entire OLED display such as a Splash Screen.</remarks>
		/// <param name="bitmapBytes">The byte representation of the  monochrome bitmapBytes.</param>
		/// <exception cref="ArgumentException">An <see cref="ArgumentException"/> will be thrown if the bitmap dimensions are not 96x40.</exception>
		/// <example>Example usage:
		/// <code language = "C#">
		/// byte[] logo = {
		/// 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F,
		/// 0x3F, 0x1F, 0x1F, 0x3F, 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
		/// 0x7F, 0x3F, 0x1F, 0x1F, 0x3F, 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
		/// 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x3F,
		/// 0x3F, 0xFF, 0xFF, 0x3F, 0x3F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
		/// 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
		/// 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFC, 0xF8,
		/// 0xF0, 0x00, 0x00, 0xF0, 0xF8, 0xFC, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFC,
		/// 0xF8, 0xF0, 0x00, 0x00, 0xF0, 0xF8, 0xFC, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
		/// 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xF9, 0xF9, 0x01, 0x01, 0xF9, 0xF9, 0x01, 0x03, 0xFF, 0xFF, 0x03,
		/// 0x03, 0xFF, 0xFF, 0x00, 0x00, 0xCF, 0x87, 0x03, 0x31, 0x79, 0xFF, 0xFF, 0x00, 0x00, 0xF9, 0xF1,
		/// 0xF3, 0xFF, 0x87, 0x03, 0x79, 0x79, 0x79, 0x03, 0x87, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
		/// 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x07, 0x07, 0xC7, 0xC7, 0xC7, 0xC7,
		/// 0xC7, 0xC0, 0xC0, 0xC7, 0xC7, 0xC7, 0xC7, 0xC7, 0xC7, 0xC7, 0x07, 0x07, 0xC7, 0xC7, 0xC7, 0xC7,
		/// 0xC7, 0xC7, 0xC0, 0xC0, 0xC7, 0xC7, 0xC7, 0xC7, 0xC7, 0x07, 0x07, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
		/// 0xFF, 0xFF, 0xFF, 0x1E, 0x1E, 0x1F, 0x9F, 0x9E, 0x9E, 0x9F, 0x1F, 0x1E, 0x3E, 0x7F, 0xFF, 0xFE,
		/// 0xFE, 0x1F, 0x1F, 0xFE, 0xFE, 0xFF, 0xFF, 0xFF, 0xFE, 0x1E, 0x1F, 0xFF, 0xFE, 0x7E, 0x3F, 0x1F,
		/// 0x1F, 0x9F, 0x9F, 0x9F, 0x9E, 0x9E, 0x9E, 0x9F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
		/// 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x9F, 0x0F, 0x07, 0x00, 0x00, 0x07, 0x0F, 0x9F, 0xFF,
		/// 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x9F, 0x0F, 0x07, 0x00, 0x00, 0x07, 0x0F, 0x9F, 0xFF,
		/// 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x9F, 0x0F, 0x07, 0x00, 0x00, 0x07, 0x0F, 0x9F, 0xFF, 0xFF,
		/// 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0xF3, 0xF3, 0xF3, 0xF3, 0x63, 0x00, 0x00, 0x1C, 0xFF, 0xFF,
		/// 0xFF, 0x00, 0x00, 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F, 0x00, 0x00, 0xFF, 0xFF, 0xF8, 0xF0, 0xF0,
		/// 0xF3, 0xE3, 0xE7, 0xE7, 0xE7, 0x67, 0x07, 0x07, 0x0F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
		/// 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE, 0xFC, 0xFC, 0xFE, 0xFF, 0xFF, 0xFF,
		/// 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE, 0xFC, 0xFC, 0xFE, 0xFF, 0xFF, 0xFF,
		/// 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE, 0xFC, 0xFC, 0xFE, 0xFF, 0xFF, 0xFF, 0xFF,
		/// 0xFF, 0xFF, 0xFF, 0xFC, 0xFC, 0xFC, 0xFC, 0xFC, 0xFC, 0xFC, 0xFC, 0xFC, 0xFE, 0xFF, 0xFF, 0xFF,
		/// 0xFF, 0xFE, 0xFE, 0xFC, 0xFC, 0xFC, 0xFC, 0xFC, 0xFC, 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFC, 0xFC,
		/// 0xFC, 0xFC, 0xFC, 0xFC, 0xFC, 0xFC, 0xFC, 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
		/// };
		/// _oled.DrawBitmap(logo);
		/// </code>
		/// <code language = "VB">
		/// Dim logo As BytE() = {<![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>H7F, <![CDATA[&]]>H3F, <![CDATA[&]]>H1F, _
		/// <![CDATA[&]]>H1F, <![CDATA[&]]>H3F, <![CDATA[&]]>H7F, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>H7F, <![CDATA[&]]>H3F, <![CDATA[&]]>H1F, <![CDATA[&]]>H1F, _
		/// <![CDATA[&]]>H3F, <![CDATA[&]]>H7F, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>H3F, <![CDATA[&]]>H3F, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>H3F, <![CDATA[&]]>H3F, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFC, <![CDATA[&]]>HF8, <![CDATA[&]]>HF0, <![CDATA[&]]>H0, _
		/// <![CDATA[&]]>H0, <![CDATA[&]]>HF0, <![CDATA[&]]>HF8, <![CDATA[&]]>HFC, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFC, <![CDATA[&]]>HF8, <![CDATA[&]]>HF0, <![CDATA[&]]>H0, <![CDATA[&]]>H0, _
		/// <![CDATA[&]]>HF0, <![CDATA[&]]>HF8, <![CDATA[&]]>HFC, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>H0, <![CDATA[&]]>H0, <![CDATA[&]]>HF9, _
		/// <![CDATA[&]]>HF9, <![CDATA[&]]>H1, <![CDATA[&]]>H1, <![CDATA[&]]>HF9, <![CDATA[&]]>HF9, <![CDATA[&]]>H1, _
		/// <![CDATA[&]]>H3, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>H3, <![CDATA[&]]>H3, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>H0, <![CDATA[&]]>H0, <![CDATA[&]]>HCF, <![CDATA[&]]>H87, <![CDATA[&]]>H3, _
		/// <![CDATA[&]]>H31, <![CDATA[&]]>H79, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>H0, <![CDATA[&]]>H0, _
		/// <![CDATA[&]]>HF9, <![CDATA[&]]>HF1, <![CDATA[&]]>HF3, <![CDATA[&]]>HFF, <![CDATA[&]]>H87, <![CDATA[&]]>H3, _
		/// <![CDATA[&]]>H79, <![CDATA[&]]>H79, <![CDATA[&]]>H79, <![CDATA[&]]>H3, <![CDATA[&]]>H87, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>H7, <![CDATA[&]]>H7, _
		/// <![CDATA[&]]>HC7, <![CDATA[&]]>HC7, <![CDATA[&]]>HC7, <![CDATA[&]]>HC7, <![CDATA[&]]>HC7, <![CDATA[&]]>HC0, _
		/// <![CDATA[&]]>HC0, <![CDATA[&]]>HC7, <![CDATA[&]]>HC7, <![CDATA[&]]>HC7, <![CDATA[&]]>HC7, <![CDATA[&]]>HC7, _
		/// <![CDATA[&]]>HC7, <![CDATA[&]]>HC7, <![CDATA[&]]>H7, <![CDATA[&]]>H7, <![CDATA[&]]>HC7, <![CDATA[&]]>HC7, _
		/// <![CDATA[&]]>HC7, <![CDATA[&]]>HC7, <![CDATA[&]]>HC7, <![CDATA[&]]>HC7, <![CDATA[&]]>HC0, <![CDATA[&]]>HC0, _
		/// <![CDATA[&]]>HC7, <![CDATA[&]]>HC7, <![CDATA[&]]>HC7, <![CDATA[&]]>HC7, <![CDATA[&]]>HC7, <![CDATA[&]]>H7, _
		/// <![CDATA[&]]>H7, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>H1E, <![CDATA[&]]>H1E, <![CDATA[&]]>H1F, _
		/// <![CDATA[&]]>H9F, <![CDATA[&]]>H9E, <![CDATA[&]]>H9E, <![CDATA[&]]>H9F, <![CDATA[&]]>H1F, <![CDATA[&]]>H1E, _
		/// <![CDATA[&]]>H3E, <![CDATA[&]]>H7F, <![CDATA[&]]>HFF, <![CDATA[&]]>HFE, <![CDATA[&]]>HFE, <![CDATA[&]]>H1F, _
		/// <![CDATA[&]]>H1F, <![CDATA[&]]>HFE, <![CDATA[&]]>HFE, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFE, <![CDATA[&]]>H1E, <![CDATA[&]]>H1F, <![CDATA[&]]>HFF, <![CDATA[&]]>HFE, <![CDATA[&]]>H7E, _
		/// <![CDATA[&]]>H3F, <![CDATA[&]]>H1F, <![CDATA[&]]>H1F, <![CDATA[&]]>H9F, <![CDATA[&]]>H9F, <![CDATA[&]]>H9F, _
		/// <![CDATA[&]]>H9E, <![CDATA[&]]>H9E, <![CDATA[&]]>H9E, <![CDATA[&]]>H9F, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>H9F, <![CDATA[&]]>HF, <![CDATA[&]]>H7, <![CDATA[&]]>H0, <![CDATA[&]]>H0, _
		/// <![CDATA[&]]>H7, <![CDATA[&]]>HF, <![CDATA[&]]>H9F, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>H9F, _
		/// <![CDATA[&]]>HF, <![CDATA[&]]>H7, <![CDATA[&]]>H0, <![CDATA[&]]>H0, <![CDATA[&]]>H7, <![CDATA[&]]>HF, _
		/// <![CDATA[&]]>H9F, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>H9F, <![CDATA[&]]>HF, <![CDATA[&]]>H7, <![CDATA[&]]>H0, _
		/// <![CDATA[&]]>H0, <![CDATA[&]]>H7, <![CDATA[&]]>HF, <![CDATA[&]]>H9F, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>H0, <![CDATA[&]]>H0, <![CDATA[&]]>H0, _
		/// <![CDATA[&]]>HF3, <![CDATA[&]]>HF3, <![CDATA[&]]>HF3, <![CDATA[&]]>HF3, <![CDATA[&]]>H63, <![CDATA[&]]>H0, _
		/// <![CDATA[&]]>H0, <![CDATA[&]]>H1C, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>H0, _
		/// <![CDATA[&]]>H0, <![CDATA[&]]>H7F, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>H7F, <![CDATA[&]]>H0, <![CDATA[&]]>H0, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HF8, _
		/// <![CDATA[&]]>HF0, <![CDATA[&]]>HF0, <![CDATA[&]]>HF3, <![CDATA[&]]>HE3, <![CDATA[&]]>HE7, <![CDATA[&]]>HE7, _
		/// <![CDATA[&]]>HE7, <![CDATA[&]]>H67, <![CDATA[&]]>H7, <![CDATA[&]]>H7, <![CDATA[&]]>HF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFE, <![CDATA[&]]>HFC, <![CDATA[&]]>HFC, _
		/// <![CDATA[&]]>HFE, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFE, <![CDATA[&]]>HFC, <![CDATA[&]]>HFC, <![CDATA[&]]>HFE, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFE, <![CDATA[&]]>HFC, _
		/// <![CDATA[&]]>HFC, <![CDATA[&]]>HFE, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFC, <![CDATA[&]]>HFC, <![CDATA[&]]>HFC, _
		/// <![CDATA[&]]>HFC, <![CDATA[&]]>HFC, <![CDATA[&]]>HFC, <![CDATA[&]]>HFC, <![CDATA[&]]>HFC, <![CDATA[&]]>HFC, _
		/// <![CDATA[&]]>HFE, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFE, _
		/// <![CDATA[&]]>HFE, <![CDATA[&]]>HFC, <![CDATA[&]]>HFC, <![CDATA[&]]>HFC, <![CDATA[&]]>HFC, <![CDATA[&]]>HFC, _
		/// <![CDATA[&]]>HFC, <![CDATA[&]]>HFE, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFC, <![CDATA[&]]>HFC, <![CDATA[&]]>HFC, <![CDATA[&]]>HFC, <![CDATA[&]]>HFC, <![CDATA[&]]>HFC, _
		/// <![CDATA[&]]>HFC, <![CDATA[&]]>HFC, <![CDATA[&]]>HFC, <![CDATA[&]]>HFE, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, _
		/// <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF, <![CDATA[&]]>HFF}
		/// _oled.DrawBitmap(logo)
		/// </code>
		/// </example>
		public void DrawBitmap(byte[] bitmapBytes)
		{
			if (bitmapBytes.Length != 480) throw new ArgumentException("Invalid bitmapBytes parameter. Bitmap size of 96 X 40 is required by this method.", "bitmapBytes");

			for (byte i = 0; i < 5; i++)
			{
				SetPageAddress(i);
				SetColumnAddress(_displayOffset);

				for (byte j = 0; j < SSD1306_LCDWIDTH; j++)
				{
					WriteData(bitmapBytes[i * SSD1306_LCDWIDTH + j]);
				}
			}
		}

		/// <summary>
		/// Draws a bitmap on the OLED Display using the internal display buffer.
		/// </summary>
		/// <param name="bitmap">The byte array of the bitmap data structure.</param>
		/// <param name="x">The x-coordinate in pixels of the upper-left corner in which to render the bitmap to the OLED Display.</param>
		/// <param name="y">The y-coordinate in pixels of the upper-left corner in which to render the bitmap to the OLED Display.</param>
		/// <param name="width">The width in pixels in which to render the bitmap to the OLED Display.</param>
		/// <param name="height">The width in pixels in which to render the bitmap to the OLED Display.</param>
		/// <param name="color">The <see cref="Colors"/> in which to render the individual pixels on the OLED Display. </param>
		/// <exception cref="ArgumentException">An <see cref="ArgumentException"/> will be thrown if the bitmap dimensions are greater than 96x40 or if attempting to render the bitmap outside the OLED's physical space.</exception>
		/// <exception cref="NullReferenceException">An <see cref="NullReferenceException"/> will be thrown if the bitmap bytes are null or empty.</exception>
		/// <example>Example usage:
		/// <code language = "C#">
		/// byte[] image_16x16 = {
		/// 	0x00, 0x00, 0x00, 0x18, 0x24, 0x02, 0x02, 0x02, 0x82, 0x86, 0xCC, 0x78, 0x00, 0x00, 0x00, 0x00,
		/// 	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x67, 0x67, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
		/// };
		/// _oled.DrawBitmap(image_16x16, 16, 10, 10, 16, OLEDWBClick.Colors.Inverse);
		/// 
		///  // or from an embedded resource.
		/// var logo = Bitmap.Parse(Resources.GetString(Resources.StringResources._16x16));
		/// _oled.DrawBitmap(logo.BitmapDataBytes, 0, 0, logo.Width, logo.Height, OLEDWBClick.Colors.Inverse);
		///</code>
		/// <code languagE = "VB">
		/// Dim image_16x16 As Byte() = {<![CDATA[&]]>H0, <![CDATA[&]]>H0, <![CDATA[&]]>H0, <![CDATA[&]]>H18, <![CDATA[&]]>H24, <![CDATA[&]]>H2, _
		/// 	<![CDATA[&]]>H2, <![CDATA[&]]>H2, <![CDATA[&]]>H82, <![CDATA[&]]>H86, <![CDATA[&]]>HCC, <![CDATA[&]]>H78, _
		/// 	<![CDATA[&]]>H0, <![CDATA[&]]>H0, <![CDATA[&]]>H0, <![CDATA[&]]>H0, <![CDATA[&]]>H0, <![CDATA[&]]>H0, _
		/// 	<![CDATA[&]]>H0, <![CDATA[&]]>H0, <![CDATA[&]]>H0, <![CDATA[&]]>H0, <![CDATA[&]]>H0, <![CDATA[&]]>H67, _
		/// 	<![CDATA[&]]>H67, <![CDATA[&]]>H0, <![CDATA[&]]>H0, <![CDATA[&]]>H0, <![CDATA[&]]>H0, <![CDATA[&]]>H0, _
		/// 	<![CDATA[&]]>H0, <![CDATA[&]]>H0}
		///
		/// _oled.DrawBitmap(image_16x16, 16, 10, 10, 16, OLEDWBClick.Colors.Inverse)
		/// 
		/// ' or from an embedded resource.
		/// Dim logo  as Byte() = Bitmap.Parse(Resources.GetString(Resources.StringResources._16x16))
		/// _oled.DrawBitmap(logo.BitmapDataBytes, 0, 0, logo.Width, logo.Height, OLEDWBClick.Colors.Inverse)
		/// </code>
		/// </example>
		public void DrawBitmap(byte[] bitmap, int x, int y, int width, int height, Colors color)
		{
			// X and Y parameters are verified in the InternalSetPixel method.
			if (bitmap.Length > 480) throw new ArgumentException("Invalid bitmapBytes parameter. Maximum bitmapBytes size is 96 centerX 40.", "bitmap");
			if ((bitmap.GetHashCode() == 0) || bitmap.Length == 0) throw new NullReferenceException("Invalid bitmapBytes parameter. Bitmap bytes cannot be null.");
			if ((width < 1) || (width > SSD1306_LCDWIDTH)) throw new ArgumentException("Width parameter cannot be less than 1 or greater than the 96", "width");
			if ((height < 1) || (height > SSD1306_LCDHEIGHT)) throw new ArgumentException("Height parameter cannot be less than 1 or greater than the 40", "height"); 
			
			for (int j = 0; j < height; j++)
			{
				for (int i = 0; i < width; i++)
				{
					if ((bitmap[i + (j / 8) * width] & (1 << (j % 8))) != 0)
					{
						InternalSetPixel(x + i, y + j, color);
					}
				}
			}

			if (_autoRefresh) Refresh();
		}

		/// <summary>
		///  Draws a <see cref="System.Char"/> at the specified coordinates on the OLED Display.
		/// </summary>
		/// <param name="x">The x-coordinate in pixels of the upper-left corner in which to render the character to the OLED Display. Valid values are 0 to 69.</param>
		/// <param name="line">The line or row in which to draw the character. Valid values are rows 0 to 4.</param>
		/// <param name="character">The <see cref="System.Char"/> to draw. Only ASCII Printable Characters (32 - 176) are supported.</param>
		/// <remarks>All text drawing methods automatically render the text in the inverse color of the display area. The default bitmap font character is 6 pixels wide.</remarks>
		/// <exception cref="ArgumentException">An <see cref="ArgumentException"/> will be thrown if an invalid char is passed or if attempting to render outside the OLED's physical size.</exception>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.DrawChar(0, 0, 'A');
		/// _oled.DrawChar(0, 1, 'B');
		/// _oled.DrawChar(0, 2, 'C');
		/// _oled.DrawChar(0, 3, 'D');
		/// _oled.DrawChar(0, 4, 'E');
		/// _oled.Refresh();
		/// </code>
		/// <code language = "VB">
		/// _oled.DrawChar(0, 0, "A"c)
		/// _oled.DrawChar(0, 1, "B"c)
		/// _oled.DrawChar(0, 2, "C"c)
		/// _oled.DrawChar(0, 3, "D"c)
		/// _oled.DrawChar(0, 4, "E"c)
		/// _oled.Refresh()
		/// </code>
		/// </example>
		public void DrawChar(int x, int line, char character)
		{
			// Error checking is done in InternalDrawChar method.
			InternalDrawChar(x, line, character);

			if (_autoRefresh) Refresh();
		}

		/// <summary>
		/// Draws a circle on the OLED display.
		/// </summary>
		/// <param name="centerX">The x-coordinate in pixels of the center of the circle.</param>
		/// <param name="centerY">The y-coordinate in pixels of the center of the circle.</param>
		/// <param name="radius">The radius of the circle.</param>
		/// <param name="color">The <see cref="Colors"/> in which to render the circle on the OLED Display. </param>
		/// <exception cref="ArgumentException">An <see cref="ArgumentException"/> will be thrown if attempting to render outside the OLED's physical size.</exception>
 		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.DrawCircle(20, 20, 10, OLEDWBClick.Colors.Inverse);
		/// _oled.Refresh();
		/// </code>
		/// <code language = "VB">
		/// _oled.DrawCircle(20, 20, 10, OLEDWBClick.Colors.Inverse)
		/// _oled.Refresh()
		/// </code>
		/// </example>
		public void DrawCircle(int centerX, int centerY, int radius, Colors color)
		{
			// The X and Y parameters are checked in the InternalSetPixel method.
			if ((radius < 1) || (radius > SSD1306_LCDHEIGHT)) throw new ArgumentException("Width parameter cannot be less than 1 or greater than the 40", "radius");
			if (((centerY - (radius / 2)) < 0) || ((centerY + (radius / 2) > SSD1306_LCDHEIGHT))) throw new ArgumentException("The combination of radius and the centerY parameter will result in rendering outside the view port. Please check your parameters again.", "radius");
			if (((centerX - (radius / 2)) < 0) || ((centerX + (radius / 2) > SSD1306_LCDWIDTH))) throw new ArgumentException("The combination of radius and the centerX parameter will result in rendering outside the view port. Please check your parameters again.", "radius"); 

			int f = 1 - radius;
			int ddF_x = 1;
			int ddF_y = -2 * radius;
			int i = 0;
			int i1 = radius;

			InternalSetPixel(centerX, centerY + radius, color);
			InternalSetPixel(centerX, centerY - radius, color);
			InternalSetPixel(centerX + radius, centerY, color);
			InternalSetPixel(centerX - radius, centerY, color);

			while (i < i1)
			{
				if (f >= 0)
				{
					i1--;
					ddF_y += 2;
					f += ddF_y;
				}
				i++;
				ddF_x += 2;
				f += ddF_x;

				InternalSetPixel(centerX + i, centerY + i1, color);
				InternalSetPixel(centerX - i, centerY + i1, color);
				InternalSetPixel(centerX + i, centerY - i1, color);
				InternalSetPixel(centerX - i, centerY - i1, color);

				InternalSetPixel(centerX + i1, centerY + i, color);
				InternalSetPixel(centerX - i1, centerY + i, color);
				InternalSetPixel(centerX + i1, centerY - i, color);
				InternalSetPixel(centerX - i1, centerY - i, color);
			}

			if (_autoRefresh) Refresh();
		}

		/// <summary>
		/// Draws a filled circle on the OLED Display.
		/// </summary>
		/// <param name="centerX">The x-coordinate in pixels of the center of the circle.</param>
		/// <param name="centerY">The y-coordinate in pixels of the center of the circle.</param>
		/// <param name="radius">The radius of the circle.</param>
		/// <param name="color">The <see cref="Colors"/> in which to render the circle on the OLED Display.</param>
		/// <exception cref="ArgumentException">An <see cref="ArgumentException"/> will be thrown if attempting to render outside the OLED's physical size.</exception>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.DrawFilledCircle(20, 20, 10, OLEDWBClick.Colors.Inverse);
		/// _oled.Refresh();
		/// </code>
		/// <code language = "VB">
		/// _oled.DrawFilledCircle(20, 20, 10, OLEDWBClick.Colors.Inverse)
		/// _oled.Refresh()
		/// </code>
		/// </example>
		public void DrawFilledCircle(int centerX, int centerY, int radius, Colors color)
		{
			// The X and Y parameters are checked in the InternalSetPixel method.
			if ((radius < 1) || (radius > SSD1306_LCDHEIGHT)) throw new ArgumentException("Width parameter cannot be less than 1 or greater than the 40", "radius");
			if (((centerY - (radius / 2)) < 0) || ((centerY + (radius / 2) > SSD1306_LCDHEIGHT))) throw new ArgumentException("The combination of radius and the centerY parameter will result in rendering outside the view port. Please check your parameters again.", "radius");
			if (((centerX - (radius / 2)) < 0) || ((centerX + (radius / 2) > SSD1306_LCDWIDTH))) throw new ArgumentException("The combination of radius and the centerX parameter will result in rendering outside the view port. Please check your parameters again.", "radius"); 
			
			int f = 1 - radius;
			int ddF_x = 1;
			int ddF_y = -2 * radius;
			int i1 = 0;
			int i2 = radius;

			for (int i = centerY - radius; i <= centerY + radius; i++)
			{
				InternalSetPixel(centerX, i, color);
			}

			while (i1 < i2)
			{
				if (f >= 0)
				{
					i2--;
					ddF_y += 2;
					f += ddF_y;
				}
				i1++;
				ddF_x += 2;
				f += ddF_x;

				for (int i = centerY - i2; i <= centerY + i2; i++)
				{
					InternalSetPixel(centerX + i1, i, color);
					InternalSetPixel(centerX - i1, i, color);
				}
				for (int i = centerY - i1; i <= centerY + i1; i++)
				{
					InternalSetPixel(centerX + i2, i, color);
					InternalSetPixel(centerX - i2, i, color);
				}
			}

			if (_autoRefresh) Refresh();
		}

		/// <summary>
		/// Draws a filled rectangle on the OLED Display.
		/// </summary>
		/// <param name="x">The x-coordinate in pixels of the upper-left corner in which to render the rectangle to the OLED Display.</param>
		/// <param name="y">The y-coordinate in pixels of the upper-left corner in which to render the rectangle to the OLED Display.</param>
		/// <param name="width">The width in pixels in which to render the rectangle to the OLED Display.</param>
		/// <param name="height">The width in pixels in which to render the rectangle to the OLED Display.</param>
		/// <param name="color">The <see cref="Colors"/> in which to render the individual pixels on the OLED Display.</param>
		/// <exception cref="ArgumentException">An <see cref="ArgumentException"/> will be thrown if the rectangle dimensions are greater than 96x40 or if attempting to render the bitmap outside the OLED's physical space.</exception>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.DrawFilledRectangle(0, 0, 96, 40, OLEDWBClick.Colors.White);
		/// _oled.DrawFilledRectangle(10, 10, 76, 20, OLEDWBClick.Colors.Inverse);
		/// _oled.Refresh();
		/// </code>
		/// <code language = "VB">
		/// _oled.DrawFilledRectangle(0, 0, 96, 40, OLEDWBClick.Colors.White)
		/// _oled.DrawFilledRectangle(10, 10, 76, 20, OLEDWBClick.Colors.Inverse)
		/// _oled.Refresh()
		/// </code>
		/// </example>
		public void DrawFilledRectangle(int x, int y, int width, int height, Colors color)
		{
			// The X and Y parameters are checked in the InternalSetPixel method.
			if ((width < 1) || (width > SSD1306_LCDWIDTH)) throw new ArgumentException("Width parameter cannot be less than 1 or greater than the 96", "width");
			if ((height < 1) || (height > SSD1306_LCDHEIGHT)) throw new ArgumentException("Height parameter cannot be less than 1 or greater than the 40", "height"); 

			for (int i = x; i < x + width; i++)
			{
				for (int j = y; j < y + height; j++)
				{
					InternalSetPixel(i, j, color);
				}
			}

			if (_autoRefresh) Refresh();
		}

		/// <summary>
		/// Draws a line
		/// </summary>
		/// <param name="x0">The X starting point in pixels in which to render the line to the OKED Display.</param>
		/// <param name="y0">The Y starting point in pixels in which to render the line to the OKED Display.</param>
		/// <param name="x1">The X ending point in pixels in which to render the line to the OKED Display.</param>
		/// <param name="y1">The Y ending point in pixels in which to render the line to the OKED Display.</param>
		/// <param name="color">The <see cref="Colors"/> in which to render the circle on the OLED Display.</param>
		/// <exception cref="ArgumentException">An <see cref="ArgumentException"/> will be thrown if attempting to render outside the OLED's physical size.</exception>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.DrawLine(0, 0, 96, 40, OLEDWBClick.Colors.Inverse); // Draws a diagonal line from Top-Left to Bottom Right.
		/// _oled.Refresh();
		/// </code>
		/// <code language = "VB">
		/// _oled.DrawLine(0, 0, 96, 40, OLEDWBClick.Colors.Inverse) ' Draws a diagonal line from Top-Left to Bottom Right.
		/// _oled.Refresh()
		/// </code>
		/// </example>
		public void DrawLine(int x0, int y0, int x1, int y1, Colors color)
		{
			if ((x0 < 0) || (x0 > SSD1306_LCDWIDTH)) throw new ArgumentException("The X0 parameter cannot be less than 0 or greater than the 95", "x0");
			if ((y0 < 0) || (y0 > SSD1306_LCDHEIGHT)) throw new ArgumentException("The Y0 parameter cannot be less than 0 or greater than the 39", "y0"); ;
			if ((x1 < 0) || (x1 > SSD1306_LCDWIDTH)) throw new ArgumentException("X1 parameter cannot be less than 0 or greater than the 95", "x1");
			if ((y1 < 0) || (y1 > SSD1306_LCDHEIGHT)) throw new ArgumentException("Y1 parameter cannot be less than 0 or greater than the 39", "y1"); 

            var step = (Math.Abs(y1 - y0) > Math.Abs(x1 - x0)) ? 1 : 0;
          
            if (step != 0)
			{
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
            }

            if (x0 > x1)
			{
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }

			var dx = x1 - x0;
            var dy = Math.Abs(y1 - y0);

            var err = dx / 2;

			var ystep = y0 < y1 ? 1 : -1;

			for (; x0<x1; x0++)
			{
                if (step != 0) 
				{
					InternalSetPixel(y0, x0, color);
                }
                else
				{
                    InternalSetPixel(x0, y0, color);
                }
            
                err -= dy;
            
                if (err < 0)
				{
                    y0 += ystep;
                    err += dx;
                }
            }

			if (_autoRefresh) Refresh();
		}

		/// <summary>
		/// Draws a rectangle on the OLED Display.
		/// </summary>
		/// <param name="x">The x-coordinate in pixels of the upper-left corner in which to render the rectangle to the OLED Display.</param>
		/// <param name="y">The y-coordinate in pixels of the upper-left corner in which to render the rectangle to the OLED Display.</param>
		/// <param name="width">The width in pixels in which to render the rectangle to the OLED Display.</param>
		/// <param name="height">The width in pixels in which to render the rectangle to the OLED Display.</param>
		/// <param name="color">The <see cref="Colors"/> in which to render the individual pixels on the OLED Display.</param>
		/// <exception cref="ArgumentException">An <see cref="ArgumentException"/> will be thrown if the rectangle dimensions are greater than 96x40 or if attempting to render the bitmap outside the OLED's physical space.</exception>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.DrawRectangle(0, 0, 96, 40, OLEDWBClick.Colors.White);
		/// _oled.DrawRectangle(10, 10, 76, 20, OLEDWBClick.Colors.White);
		/// _oled.Refresh();
		/// </code>
		/// <code language = "VB">
		/// _oled.DrawRectangle(0, 0, 96, 40, OLEDWBClick.Colors.White)
		/// _oled.DrawRectangle(10, 10, 76, 20, OLEDWBClick.Colors.White)
		/// _oled.Refresh()
		/// </code>
		/// </example>
		public void DrawRectangle(int x, int y, int width, int height, Colors color)
		{
			// The centerX and centerY parameters are checked in the InternalSetPixel method.
			if ((width < 1) || (width > SSD1306_LCDWIDTH)) throw new ArgumentException("Width parameter cannot be less than 1 or greater than the 96", "width");
			if ((height < 1) || (height > SSD1306_LCDHEIGHT)) throw new ArgumentException("Height parameter cannot be less than 1 or greater than the 40", "height"); 

			for (int i = x; i < x + width; i++)
			{
				InternalSetPixel(i, y, color);
				InternalSetPixel(i, y + height - 1, color);
			}

			for (int i = y; i < y + height; i++)
			{
				InternalSetPixel(x, i, color);
				InternalSetPixel(x + width - 1, i, color);
			}

			if (_autoRefresh) Refresh();
		}

		/// <summary>
		/// Draws a text string on the OLED Display.
		/// </summary>
		/// <param name="x">The horizontal position in which to start drawing the text. Valid values are 0 to 69.</param>
		/// <param name="line">The line or row in which to draw the character. Valid values are rows 0 to 4.</param>
		/// <param name="text">The <see cref="System.String"/> to draw on the OLED Display.</param>
		/// <remarks>If the text is longer than 39 characters it will automatically overflow to the next line. If the text reaches the end position of the last row, it will be truncated.</remarks>
		/// <remarks>All text drawing methods automatically render the text in the inverse color of the display area.</remarks>
		/// <exception cref="ArgumentNullException">An <see cref="ArgumentNullException"/> will be thrown if a null or empty string is passed to this method.</exception>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.DrawText(0, 0, "MikroBusNet is really cool. Get it at www.mikrobusnet.org");
		/// _oled.Refresh();
		/// </code>
		/// <code language = "VB">
		/// _oled.DrawText(0, 0, "MikroBusNet is really cool. Get it at www.mikrobusnet.org")
		/// _oled.Refresh();
		/// </code>
		/// </example>
		public void DrawText(int x, int line, string text)
		{
			// X and Line parameters are checked in the InternalDrawChar method.
			if (text == null || text == string.Empty) throw new ArgumentNullException("text", "Text parameter cannot be null or empty.");

			var charArray = text.ToCharArray(0, text.Length);

			foreach (var ch in charArray)
			{
				InternalDrawChar(x, line, ch);
				x += 6; // Each character is 6 pixels wide
				if (x + 6 >= SSD1306_LCDWIDTH)
				{
					x = 0; // ran out of this line, wrap to next line.
					line++;
				}
				if (line == 5) break; // We ran out of OLED of lines.
			}

			if (_autoRefresh) Refresh();
		}

		/// <summary>
		/// Causes the OLED Display to fade to black at the specified interval.
		/// </summary>
		/// <param name="fade">If true, it will cause the OLED display to fade out at the specified interval, otherwise set to false to de-activate fading.</param>
		/// <param name="delayBetweenTransitions">Optional, the delay as number of frames (0 to 127) to delay, 0x00 = 8 frames, 0x01 = 16 frames to 0x7F = 128 frames.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.Fade(true, 0x00); // Turn on fading.
		/// Thread.Sleep(10000); // Wait for 10 seconds.
		/// _oled.Fade(false); // Turn off fading.
		/// </code>
		/// <code language = "VB">
		/// _oled.Fade(True, <![CDATA[&]]>H0) ' Turn on fading.
		/// Thread.sleep(10000) ' Wait for 10 seconds.
		/// _oled.Fade(False) ' Turn off fading.
		/// </code>
		/// </example>
		public void Fade(bool fade, byte delayBetweenTransitions = 0x00)
		{
			if (delayBetweenTransitions > 127) delayBetweenTransitions = 127;

			WriteCommand(SSD1306_ACTIVATE_FADE_BLINK);
			WriteCommand((byte)(fade ? 0x28 | delayBetweenTransitions : 0x00));
		}

		/// <summary>
		/// Inverts the colors of the OLED Display.
		/// </summary>
		/// <param name="invert">If true, inverts the OLED display colors. Black pixels are white and white pixels are black. If false, all pixels are rendered normally.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.InvertColor(true);
		/// Thread.Sleep(10000);
		/// _oled.InvertColor(false);
		/// </code>
		/// <code language = "VB">
		/// _oled.InvertColor(True)
		/// Thread.Sleep(10000)
		/// _oled.InvertColor(False)
		/// </code>
		/// </example>
		public void InvertColor(bool invert)
		{
			WriteCommand((invert ? SSD1306_INVERTDISPLAY : SSD1306_NORMALDISPLAY));
		}

		/// <summary>
		/// Refreshes the display displayBuffer to the OLED display.
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		/// // Do some random drawing routing
		/// _oled.Refresh();
		/// </code>
		/// <code language = "VB">
		/// ' Do some random drawing routing
		/// _oled.Refresh()
		/// </code>
		/// </example>
		public void Refresh()
		{
			for (byte i = 0; i < 5; i++)
			{
				SetPageAddress(i);
				SetColumnAddress(_displayOffset);

				for (byte j = 0; j < SSD1306_LCDWIDTH; j++)
				{
					WriteData(displayBuffer[i * SSD1306_LCDWIDTH + j]);
				}
			}
		}

		/// <summary>
		/// Resets the OLED W/B Click.
		/// </summary>
		/// <param name="resetMode">The reset mode, see <see cref="ResetModes"/> for more information.</param>
		/// <exception cref="NotSupportedException">A <see cref="NotSupportedException"/> will be thrown if a <see cref="ResetModes.Soft"/> is attempted. </exception>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.Reset(ResetModes.Hard);
		/// </code>
		/// <code language = "VB">
		/// _oled.Reset(ResetModes.Hard)
		/// </code>
		/// </example>
		public bool Reset(ResetModes resetMode)
	    {
			if (resetMode == ResetModes.Soft) throw new NotSupportedException("This module does not support a soft reset mode. Only Hard Resets are supported.");

			_resetPin.Write(false);
			Init();

			if (DisplayOn) DisplayOn = true; 

		    return true;
	    }

		/// <summary>
		/// Draws a single pixel at the specified location.
		/// </summary>
		/// <param name="x">The x coordinate in pixels in which to render the individual pixel.</param>
		/// <param name="y">The y coordinate in pixels in which to render the individual pixel.</param>
		/// <param name="color">The <see cref="Colors"/> in which to render the individual pixel on the OLED Display.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.SetPixel(10 , 10, OLEDWBClick.Colors.Inverse);
		/// _oled.Refresh();
		/// </code>
		/// <code language = "VB">
		/// _oled.SetPixel(10 , 10, OLEDWBClick.Colors.Inverse);
		/// _oled.Refresh()
		/// </code>
		/// </example>
		public void SetPixel(int x, int y, Colors color)
		{
			// Error checking is done in the InternalSetPixel method.
			InternalSetPixel(x, y, color);
			if (_autoRefresh) Refresh();
		}

		/// <summary>
		/// Scrolls the OLED Display Diagonally.
		/// </summary>
		/// <param name="direction">The <see cref="ScrollDirection"/> used to scroll the OLED Display.</param>
		/// <param name="delayBetweenTransitions"></param>
		/// <param name="startLine">The start line (0-4) of the scrolling region.</param>
		/// <param name="endLine">The end line of the scrolling region.</param>
		/// <remarks>Interesting different effects can be achieved with various settings. Just experiment.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.ScrollDisplayDiagonal(OLEDWBClick.ScrollDirection.Right, 0, 0, 4);
		/// Thread.Sleep(10250);
		/// _oled.StopScrolling();
		/// _oled.ScrollDisplayDiagonal(OLEDWBClick.ScrollDirection.Left, 0, 0, 4);
		/// Thread.Sleep(10250);
		/// _oled.StopScrolling();
		/// </code>
		/// <code language = "VB">
		/// _oled.ScrollDisplayDiagonal(OLEDWBClick.ScrollDirection.Right, 0, 0, 4);
		/// Thread.Sleep(10250);
		/// _oled.StopScrolling();
		/// _oled.ScrollDisplayDiagonal(OLEDWBClick.ScrollDirection.Left, 0, 0, 4);
		/// Thread.Sleep(10250);
		/// _oled.StopScrolling();
		/// </code>
		/// </example>
		public void ScrollDisplayDiagonal(ScrollDirection direction, byte delayBetweenTransitions, byte startLine, byte endLine)
		{
			WriteCommand(SSD1306_DEACTIVATE_SCROLL); // 0x2E Deactivate scroll
			WriteCommand(SSD1306_SET_VERTICAL_SCROLL_AREA); // Set Vertical Scroll Area
			WriteCommand(0x00); // Set No. of rows in top fixed area
			WriteCommand(SSD1306_LCDHEIGHT); // Set No. of rows in scroll area
			WriteCommand((byte)(direction == ScrollDirection.Left ? SSD1306_VERTICAL_AND_LEFT_HORIZONTAL_SCROLL : SSD1306_VERTICAL_AND_RIGHT_HORIZONTAL_SCROLL));
			WriteCommand(0x00); // Dummy byte
			WriteCommand(startLine); // Define start page address
			WriteCommand(delayBetweenTransitions); // Set time interval between each scroll
			WriteCommand(endLine); // Define end page address
			WriteCommand(0x01); // Vertical scrolling offset
			WriteCommand(SSD1306_ACTIVATE_SCROLL); // Activate Scrolling
		}

		/// <summary>
		/// Scrolls the OLED Display Horizontally.
		/// </summary>
		/// <param name="direction">The <see cref="ScrollDirection"/> used to scroll the OLED Display.</param>
		/// <param name="delayBetweenTransitions"></param>
		/// <param name="startLine">The start line (0-4) of the scrolling region.</param>
		/// <param name="endLine">The end line of the scrolling region.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.ScrollDisplayHorizontal(OLEDWBClick.ScrollDirection.Right, 0, 0, 1); // Scrolls the first two rows to the right.
		/// Thread.Sleep(2000);
		/// _oled.StopScrolling();
		/// Thread.Sleep(2000);
		/// _oled.ScrollDisplayHorizontal(OLEDWBClick.ScrollDirection.Left, 0, 0, 1); // Scrolls the first two rows to the left.
		/// _oled.StopScrolling();
		/// </code>
		/// <code language = "VB">
		/// _oled.ScrollDisplayHorizontal(OLEDWBClick.ScrollDirection.Right, 0, 0, 1) ' Scrolls the first two rows to the right.
		/// Thread.Sleep(2000);
		/// _oled.StopScrolling();
		/// Thread.Sleep(2000);
		/// _oled.ScrollDisplayHorizontal(OLEDWBClick.ScrollDirection.Left, 0, 0, 1) ' Scrolls the first two rows to the left.
		/// _oled.StopScrolling();
		/// </code>
		/// </example>
		public void ScrollDisplayHorizontal(ScrollDirection direction, byte delayBetweenTransitions, byte startLine, byte endLine)
		{
			WriteCommand(SSD1306_DEACTIVATE_SCROLL); // 0x2E Deactivate scroll
			WriteCommand((byte)(direction == ScrollDirection.Left ? SSD1306_LEFT_HORIZONTAL_SCROLL : SSD1306_RIGHT_HORIZONTAL_SCROLL));
			WriteCommand(0x00); // dummy byte
			WriteCommand(startLine); // Define start page address
			WriteCommand(delayBetweenTransitions); // Set time interval between each scroll
			WriteCommand(endLine); // Define end page address
			WriteCommand(0x00); // dummy byte
			WriteCommand(0xFF); // dummy byte
			WriteCommand(SSD1306_ACTIVATE_SCROLL); // Activate Scrolling
		}

		/// <summary>
		/// Scrolls the OLED Display Vertically. Sort of like scrolling movie credits.
		/// </summary>
		/// <param name="delayBetweenTransitions"></param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.ScrollDisplayVertical(0x00);
		/// Thread.Sleep(10250);
		/// _oled.StopScrolling();
		/// </code>
		/// <code language = "VB">
		/// _oled.ScrollDisplayVertical(<![CDATA[&]]>H0)
		/// Thread.Sleep(10250)
		/// _oled.StopScrolling()
		/// </code>
		/// </example>
		public void ScrollDisplayVertical(byte delayBetweenTransitions)
		{
			WriteCommand(SSD1306_DEACTIVATE_SCROLL); // 0x2E Deactivate scroll
			WriteCommand(SSD1306_SET_VERTICAL_SCROLL_AREA); // Set Vertical Scroll Area
			WriteCommand(0x00); // Set No. of rows in top fixed area
			WriteCommand(SSD1306_LCDHEIGHT); // Set No. of rows in scroll area
			WriteCommand(SSD1306_VERTICAL_AND_RIGHT_HORIZONTAL_SCROLL); // Vertical and Right Horizontal Scroll
			WriteCommand(0x00); // Dummy byte
			WriteCommand(0x05); // Define start page address
			WriteCommand(delayBetweenTransitions); // Set time interval between each scroll
			WriteCommand(0x05); // Define end page address
			WriteCommand(0x01); // Vertical scrolling offset
			WriteCommand(SSD1306_ACTIVATE_SCROLL); // Activate scroll
		}

		/// <summary>
		/// Immediately stops any of the scrolling.
		/// </summary>
		public void StopScrolling()
		{
			WriteCommand(SSD1306_DEACTIVATE_SCROLL);
		}

		/// <summary>
		/// Zooms in the OLEd Display
		/// </summary>
		/// <param name="zoom">If true, the top area of the OLED Display will be zoomed in.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.Zoom(true);
		/// Thread.Sleep(2000);
		/// _oled.Zoom(false);
		/// </code>
		/// <code language = "VB">
		/// _oled.Zoom(True)
		/// Thread.Sleep(2000)
		/// _oled.Zoom(False)
		/// </code>
		/// </example>
		public void Zoom(bool zoom)
		{
			WriteCommand(SSD1306_SET_ZOOM_IN);
			WriteCommand((byte)(zoom ? 0x01 : 0x00));
		}

		#endregion

	}
}

