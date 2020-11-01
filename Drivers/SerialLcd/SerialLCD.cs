/*
 * Parallax Serial LCD driver
 * 
 * Version 1.0 :
 *  - Initial version coded by Stephen Cardinale
 *  Version 2.0 :
 *  - Integration of the new namespaces and new organization.
 * 
 * References needed :
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Hardware.SerialPort
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
using MBN.Exceptions;
using System;
using System.IO.Ports;
using System.Reflection;
using System.Text;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>
    /// This is the MBN driver class for the <see cref="SerialLCD"/> SerialLCD by Parallax.
    ///     See http://www.parallax.com/product/27976 or http://www.parallax.com/product/27979
    /// <para><b>This is a Generic Module.</b></para>
    /// <para><b>Pins used: Tx</b></para>
    /// <para><b>References required:</b> MikroBus.Net, Microsoft.SPOT.Native, Microsoft.Spot.Hardware, mscorlib</para>
    /// </summary>
    /// <example>Example usage:
    /// <code language = "C#">
    /// using MBN;
    /// using MBN.Modules;
    /// using MBN.Exceptions;
    /// using Microsoft.SPOT;
    /// using System.Threading;
    ///
    /// namespace Examples
    /// {
    ///     public class Program
    ///     {
    ///         private static SerialLCD _lcd;
    ///
    ///         public static void Main()
    ///         {
    ///             try
    ///             {
    ///                 _lcd = new SerialLCD(Hardware.SocketTwo, SerialLCD.Baud.BaudRate9600, SerialLCD.DisplaySize.Size4X20);
    ///                 _lcd.ClearScreen();
    ///                 _lcd.SetBacklight(true);
    ///             }
    ///
    ///             catch (PinInUseException ex)
    ///             {
    ///                 Debug.Print("Some pins are in use while creating instances : " + ex.Message + " Stack Trace " + ex.StackTrace);
    ///             }
    ///
    ///             DisplayMBNLogo(5000);
    ///
    ///             _lcd.Print("Hello MikroBus.Net", 0, 0);
    ///             _lcd.Print("VBat ", 2, 0);
    ///             _lcd.PutC((byte)SerialLCD.CustomCharacters.OneBar, 2, 5);
    ///             _lcd.PutC((byte)SerialLCD.CustomCharacters.TwoBars, 2, 6);
    ///             _lcd.PutC((byte)SerialLCD.CustomCharacters.ThreeBars, 2, 7);
    ///             _lcd.PutC((byte)SerialLCD.CustomCharacters.FourBars, 2, 8);
    ///             _lcd.PutC((byte)SerialLCD.CustomCharacters.FiveBars, 2, 9);
    ///             _lcd.PutC((byte)SerialLCD.CustomCharacters.SixBars, 2, 10);
    ///             _lcd.PutC((byte)SerialLCD.CustomCharacters.SevenBars, 2, 11);
    ///             _lcd.Print("Temperature 25.0°C", 3, 0);
    ///             _lcd.PutC((byte)SerialLCD.CustomCharacters.DegreesSymbol, 3, 16);
    ///
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    ///
    ///         private static void DisplayMBNLogo(int msDelay)
    ///         {
    ///             var oldCharacterSet = _lcd.CharacterSet;
    ///
    ///             _lcd.SetUserCharacter(SerialLCD.UserCharacters.UserCharacter1, new byte[] { 1, 6, 12, 24, 24, 12, 6, 1 });
    ///             _lcd.SetUserCharacter(SerialLCD.UserCharacters.UserCharacter2, new byte[] { 16, 12, 6, 3, 3, 6, 12, 16 });
    ///             _lcd.SetUserCharacter(SerialLCD.UserCharacters.UserCharacter3, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 });
    ///             _lcd.SetUserCharacter(SerialLCD.UserCharacters.UserCharacter4, new byte[] { 16, 16, 16, 16, 16, 16, 16, 16 });
    ///
    ///             _lcd.CharacterSet = SerialLCD.CharacterSets.User;
    ///
    ///             _lcd.PutC((byte)SerialLCD.UserCharacters.UserCharacter1, 0, 0);
    ///             _lcd.PutC((byte)SerialLCD.UserCharacters.UserCharacter2, 0, 1);
    ///             _lcd.PutC((byte)SerialLCD.UserCharacters.UserCharacter3, 1, 0);
    ///             _lcd.PutC((byte)SerialLCD.UserCharacters.UserCharacter4, 1, 1);
    ///
    ///             _lcd.Print("MikroBus.Net", 0, 4);
    ///             _lcd.Print("Where NetMF", 2, 4);
    ///             _lcd.Print("meets MikroBus",3, 4);
    ///             Thread.Sleep(msDelay);
    ///             _lcd.ClearScreen();
    ///             _lcd.CharacterSet = oldCharacterSet;
    ///         }
    ///     }
    /// }
    /// </code>
    /// <code language = "VB">
    /// Option Explicit On
    /// Option Strict On
    ///
    /// Imports MBN.Exceptions
    /// Imports Microsoft.SPOT
    /// Imports MBN.Modules
    /// Imports MBN
    /// Imports System.Threading
    ///
    /// Namespace Examples
    ///
    ///     Public Module Module1
    ///
    ///         Private WithEvents _lcd As SerialLCD
    ///
    ///         Sub Main()
    ///             Try
    ///                 _lcd = New SerialLCD(Hardware.SocketTwo, SerialLCD.Baud.BaudRate9600, SerialLCD.DisplaySize.Size4X20)
    ///                 _lcd.ClearScreen()
    ///                 _lcd.SetBacklight(True)
    ///             Catch ex As PinInUseException
    ///                 Debug.Print("Some pins are in use while creating instances : " <![CDATA[&]]> ex.Message <![CDATA[&]]> " Stack Trace " <![CDATA[&]]> ex.StackTrace)
    ///             End Try
    ///
    ///             DisplayMBNLogo(5000)
    ///
    ///             _lcd.Print("Hello MikroBus.Net", 0, 0)
    ///             _lcd.Print("VBat ", 2, 0)
    ///             _lcd.PutC(CByte(SerialLCD.CustomCharacters.OneBar), 2, 5)
    ///             _lcd.PutC(CByte(SerialLCD.CustomCharacters.TwoBars), 2, 6)
    ///             _lcd.PutC(CByte(SerialLCD.CustomCharacters.ThreeBars), 2, 7)
    ///             _lcd.PutC(CByte(SerialLCD.CustomCharacters.FourBars), 2, 8)
    ///             _lcd.PutC(CByte(SerialLCD.CustomCharacters.FiveBars), 2, 9)
    ///             _lcd.PutC(CByte(SerialLCD.CustomCharacters.SixBars), 2, 10)
    ///             _lcd.PutC(CByte(SerialLCD.CustomCharacters.SevenBars), 2, 11)
    ///             _lcd.Print("Temperature 25.0°C", 3, 0)
    ///             _lcd.PutC(CByte(SerialLCD.CustomCharacters.DegreesSymbol), 3, 16)
    ///
    ///             Thread.Sleep(Timeout.Infinite)
    ///         End Sub
    ///
    ///         Private Sub DisplayMBNLogo(msDelay As Integer)
    ///             Dim oldCharacterSet = _lcd.CharacterSet
    ///
    ///             _lcd.SetUserCharacter(SerialLCD.UserCharacters.UserCharacter1, New Byte() {1, 6, 12, 24, 24, 12, 6, 1})
    ///             _lcd.SetUserCharacter(SerialLCD.UserCharacters.UserCharacter2, New Byte() {16, 12, 6, 3, 3, 6, 12, 16})
    ///             _lcd.SetUserCharacter(SerialLCD.UserCharacters.UserCharacter3, New Byte() {1, 1, 1, 1, 1, 1, 1, 1})
    ///             _lcd.SetUserCharacter(SerialLCD.UserCharacters.UserCharacter4, New Byte() {16, 16, 16, 16, 16, 16, 16, 16})
    ///
    ///             _lcd.CharacterSet = SerialLCD.CharacterSets.User
    ///
    ///             _lcd.PutC(CByte(SerialLCD.UserCharacters.UserCharacter1), 0, 0)
    ///             _lcd.PutC(CByte(SerialLCD.UserCharacters.UserCharacter2), 0, 1)
    ///             _lcd.PutC(CByte(SerialLCD.UserCharacters.UserCharacter3), 1, 0)
    ///             _lcd.PutC(CByte(SerialLCD.UserCharacters.UserCharacter4), 1, 1)
    ///
    ///             _lcd.Print("MikroBus.Net", 0, 4)
    ///             _lcd.Print("Where NetMF", 2, 4)
    ///             _lcd.Print("meets MikroBus", 3, 4)
    ///
    ///             Thread.Sleep(msDelay)
    ///
    ///             _lcd.ClearScreen()
    ///
    ///             _lcd.CharacterSet = oldCharacterSet
    ///         End Sub
    ///
    ///     End Module
    ///
    /// End Namespace 
    /// </code>
    /// </example>
// ReSharper disable once InconsistentNaming
    public class SerialLCD : IDriver
    {
        #region CTOR

        /// <summary>
        /// Creates a new instance of the <see cref="SerialLCD"/> class.
        /// </summary>
        /// <param name="socket">"The MBN Board Socket that the SerialLCD is connected to.</param>
        /// <param name="baudRate">The Baud Rate that the Serial LCD is set to communicate at.</param>
        /// <param name="size">Display size, either 2x16 or 4x20.</param>
        public SerialLCD(Hardware.Socket socket, Baud baudRate, DisplaySize size)
        {
            try
            {
                Hardware.CheckPins(socket, socket.Tx);

                _serialLcd = new SerialPort(socket.ComPort, (int)baudRate, Parity.None, 8, StopBits.One);
                _displaySize = size;
                _serialLcd.Open();

                InitializeDisplay();
            }
            // Catch only the PinInUse exception, so that program will halt on other exceptions
            // Send it directly to caller
            catch (PinInUseException exception)
            {
                throw new PinInUseException(exception.Message);
            }
        }

        #endregion

        #region Private Methods

        private void InitializeDisplay()
        {
            if (_serialLcd.IsOpen)
            {
                Thread.Sleep(100); // Pause 100 mSec to allow the display to startup per Parallax specification.
                SetBacklight(false);
                DisplayOff();
                ClearScreen();
                SetCursorPosition(0, 0);
                SetupPredefinedCustomCharacters();
                DisplayOn(CursorStyle.NoCursor);
                SetBacklight(true);
            }
            else
            {
                throw new SystemException("Error connecting to LCD, please check hardware pin assignments");
            }
        }

        private void SetupPredefinedCustomCharacters()
        {
            _serialLcd.WriteByte(248);
            _serialLcd.Write(_degreeSymbol, 0, _degreeSymbol.Length);
            _serialLcd.WriteByte(249);
            _serialLcd.Write(_oneBar, 0, _oneBar.Length);
            _serialLcd.WriteByte(250);
            _serialLcd.Write(_twoBars, 0, _twoBars.Length);
            _serialLcd.WriteByte(251);
            _serialLcd.Write(_threeBars, 0, _threeBars.Length);
            _serialLcd.WriteByte(252);
            _serialLcd.Write(_fourBars, 0, _fourBars.Length);
            _serialLcd.WriteByte(253);
            _serialLcd.Write(_fiveBars, 0, _fiveBars.Length);
            _serialLcd.WriteByte(254);
            _serialLcd.Write(_sixBars, 0, _sixBars.Length);
            _serialLcd.WriteByte(255);
            _serialLcd.Write(_sevenBars, 0, _sevenBars.Length);
        }

        private void SetupUserDefinedCustomCharacters()
        {
            _serialLcd.WriteByte(248);
            _serialLcd.Write(_userChartacterOne, 0, _userChartacterOne.Length);
            _serialLcd.WriteByte(249);
            _serialLcd.Write(_userChartacterTwo, 0, _userChartacterTwo.Length);
            _serialLcd.WriteByte(250);
            _serialLcd.Write(_userChartacterThree, 0, _userChartacterOne.Length);
            _serialLcd.WriteByte(251);
            _serialLcd.Write(_userChartacterFour, 0, _userChartacterFour.Length);
            _serialLcd.WriteByte(252);
            _serialLcd.Write(_userChartacterFive, 0, _userChartacterFive.Length);
            _serialLcd.WriteByte(253);
            _serialLcd.Write(_userChartacterSix, 0, _userChartacterSix.Length);
            _serialLcd.WriteByte(254);
            _serialLcd.Write(_userChartacterSeven, 0, _userChartacterSeven.Length);
            _serialLcd.WriteByte(255);
            _serialLcd.Write(_userChartacterEight, 0, _userChartacterEight.Length);
        }

        private static string ReplaceNonPrintableCharacters(string s, char replaceWith)
        {
            var result = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                var c = s[i];
                var b = (byte)c;
                result.Append((b < 32) || (b > 127) ? replaceWith : c);
            }
            return result.ToString();
        }

        #endregion
        
        #region ENUMS

        /// <summary>
        /// Valid Baud Rates for the LCD, selectable from 2400 to 19200
        /// </summary>
        public enum Baud
        {
            /// <summary>
            ///     Baud Rate 2400
            /// </summary>
            BaudRate2400 = 2400,

            /// <summary>
            ///     Baud Rate 9600
            /// </summary>
            BaudRate9600 = 9600,

            /// <summary>
            ///     Baud Rate 19200
            /// </summary>
            BaudRate19200 = 19200
        }

        /// <summary>
        /// The Character Set to use for the eight (8) Custom Characters available to the SerialLCD.
        /// </summary>
        public enum CharacterSets : byte
        {
            /// <summary>
            /// Use the predefined Custom Characters. See <see cref="CustomCharacters"/> for usage.
            /// </summary>
            Predefined = 0,
            /// <summary>
            /// Use the User defined Custom Characters. See <see cref="UserCharacters"/> for usage.
            /// </summary>
            User
        }

        /// <summary>
        /// Enumeration for predefined custom characters.
        /// </summary>
        public enum CustomCharacters : byte
        {
            /// <summary>
            ///     A typical degrees (°) symbol
            /// </summary>
            DegreesSymbol = 0,

            /// <summary>
            ///     A single horizontal line which represents a typical battery strength indicator.
            /// </summary>
            OneBar = 1,

            /// <summary>
            ///     Two horizontal lines which represents a typical battery strength indicator.
            /// </summary>
            TwoBars = 2,

            /// <summary>
            ///     Three horizontal lines which represents a typical battery strength indicator.
            /// </summary>
            ThreeBars = 3,

            /// <summary>
            ///     Four horizontal lines which represents a typical battery strength indicator.
            /// </summary>
            FourBars = 4,

            /// <summary>
            ///     Five horizontal lines which represents a typical battery strength indicator.
            /// </summary>
            FiveBars = 5,

            /// <summary>
            ///     Six horizontal lines which represents a typical battery strength indicator.
            /// </summary>
            SixBars = 6,

            /// <summary>
            ///     Seven horizontal lines which represents a typical battery strength indicator.
            /// </summary>
            SevenBars = 7
        }

        /// <summary>
        /// Enumeration for user defined custom characters.
        /// </summary>
        public enum UserCharacters : byte
        {
            /// <summary>
            /// User Character 1
            /// </summary>
            UserCharacter1 = 0,
            /// <summary>
            /// User Character 2
            /// </summary>
            UserCharacter2 = 1,
            /// <summary>
            /// User Character 3
            /// </summary>
            UserCharacter3 = 2,
            /// <summary>
            /// User Character 4
            /// </summary>
            UserCharacter4 = 3,
            /// <summary>
            /// User Character 5
            /// </summary>
            UserCharacter5 = 4,
            /// <summary>
            /// User Character 6
            /// </summary>
            UserCharacter6 = 5,
            /// <summary>
            /// User Character 7
            /// </summary>
            UserCharacter7 = 6,
            /// <summary>
            /// User Character 8
            /// </summary>
            UserCharacter8 = 7
        }

        /// <summary>
        /// Display mode enumeration (blinking cursor, no cursor, etc.
        /// </summary>
        public enum CursorStyle : byte
        {
            /// <summary>
            ///     Display is on cursor not blinking.
            /// </summary>
            NoCursor = DSPLAYON_NOCURSOR,

            /// <summary>
            ///     Display is on, cursor is off with the last characters blinking.
            /// </summary>
            CursorOffCharacterBlink = DISPLAYON_CURSOROFF_CHARBLINK,

            /// <summary>
            ///     Display is on, cursor is on characters not blinking
            /// </summary>
            CursorOnNoBlink = DISPLAYON_CURSORON_NOBLINK,

            /// <summary>
            ///     Display is on, cursor is on with last characters blinking.
            /// </summary>
            CursorBlinkCharacterBlink = DISPLAYON_CURSORBLINK_CHARBLINK
        }

        /// <summary>
        /// LCD Character Display size 2x16 or 4x20
        /// </summary>
        public enum DisplaySize : byte
        {
            /// <summary>
            ///     Display with 2 rows and 16 columns
            /// </summary>
            Size2X16 = 0,

            /// <summary>
            ///     Display with 2 rows and 16 columns
            /// </summary>
            Size4X20 = 1
        }

        #endregion

        #region Fields

        private readonly DisplaySize _displaySize = DisplaySize.Size2X16; //defaults to 2x16
        private CharacterSets _characterSet = CharacterSets.Predefined;
        private readonly SerialPort _serialLcd;

        #region Cursor Movement

        // ReSharper disable InconsistentNaming
        //private const byte BACKSPACE = 8;           //Moves cursor one space back.
        private const byte CURSOR_LEFT = 8; //Moves cursor one space back.
        //private const byte CURSOR_RIGHT = 9;    	//Moves cursor one space right.
        //private const byte LINE_FEED = 11;      	//The cursor is moved down one line. If on last, wraps around to line 0. The horizontal position remains the same.
        private const byte FORM_FEED = 12; //Form Feed - The cursor is moved to position 0 on line 0 and the entire display is cleared. Users must pause 5mSec after this command.
        private const byte CURSOR_HOME = 13; //For the two line LCD model, if on line 0 the cursor is moved to position 0 on line 1. If on line 1, it wraps around to position 0 on line 0.
        private const byte SET_CURSOR = 128; //SET_CURSOR + X : Sets cursor position to X

        #endregion

        #region BackLight

        private const byte BACKLIGHT_ON = 17; //Turn LCD BackLight on.
        private const byte BACKLIGHT_OFF = 18; //Turn LCD BackLight off.

        #endregion

        #region Display Functions

        private const byte CLEAR_DISP = 12; //Clear the display see FORMFEED above. Users must pause 5mSec after this command.
        private const byte DISPLAY_OFF = 21; //Turns the display off.
        private const byte DSPLAYON_NOCURSOR = 22; //Turn the display on, with cursor off and no blink
        private const byte DISPLAYON_CURSOROFF_CHARBLINK = 23; //Turn the display on, with cursor off and characters blink.
        private const byte DISPLAYON_CURSORON_NOBLINK = 24; //Turn the display on, with cursor on and no blink (Default)
        private const byte DISPLAYON_CURSORBLINK_CHARBLINK = 25; //Turn the display on, with cursor on and characters blink

        #endregion

        #region Musical Constants

        private const byte QUARTERNOTE = 212; //Set note length to 1/4 note
        private const byte WHOLENOTE = 214; //Set note length to whole note (2 seconds)
        private const byte SEVENTHOCTAVE = 219; //Select the 7th scale (A = 3520 Hz)
        private const byte NOTE_GSHARP = 231; //Play note G# in selected octave.
        private const byte NOTE_PAUSE = 232; //Pause for current note length (no sound)
        // ReSharper restore InconsistentNaming

        #endregion

        #region Custom Characters

        //private readonly byte[] _degreeSymbol = { 6, 9, 9, 9, 6, 0, 0, 0 }; // This is a larger and rounded degree symbol
        private readonly byte[] _degreeSymbol = { 28, 20, 28, 0, 0, 0, 0, 0 };
        private readonly byte[] _fiveBars = { 0, 0, 0, 31, 31, 31, 31, 31 };
        private readonly byte[] _fourBars = { 0, 0, 0, 0, 31, 31, 31, 31 };
        private readonly byte[] _oneBar = { 0, 0, 0, 0, 0, 0, 0, 31 };
        private readonly byte[] _sevenBars = { 31, 31, 31, 31, 31, 31, 31, 31 };
        private readonly byte[] _sixBars = { 0, 31, 31, 31, 31, 31, 31, 31 };
        private readonly byte[] _threeBars = { 0, 0, 0, 0, 0, 31, 31, 31 };
        private readonly byte[] _twoBars = { 0, 0, 0, 0, 0, 0, 31, 31 };

        #endregion

        #region UserCharacters

        private byte[] _userChartacterOne = { 0, 0, 0, 0, 0, 0, 0, 0 };
        private byte[] _userChartacterTwo = { 0, 0, 0, 0, 0, 0, 0, 0 };
        private byte[] _userChartacterThree = { 0, 0, 0, 0, 0, 0, 0, 0 };
        private byte[] _userChartacterFour = { 0, 0, 0, 0, 0, 0, 0, 0 };
        private byte[] _userChartacterFive = { 0, 0, 0, 0, 0, 0, 0, 0 };
        private byte[] _userChartacterSix = { 0, 0, 0, 0, 0, 0, 0, 0 };
        private byte[] _userChartacterSeven = { 0, 0, 0, 0, 0, 0, 0, 0 };
        private byte[] _userChartacterEight = { 0, 0, 0, 0, 0, 0, 0, 0 };

        #endregion

        #endregion

        #region Public Properties

        /// <summary>
        /// The Character Set to use for the available eight (8) Custom Characters available to the SerialLCD. You can select either <see cref="CharacterSets.Predefined"/> or <see cref="CharacterSets.User"/>.
        /// </summary>
        /// <remarks>When changing Character Sets, any custom character previously drawn on the LCD will be over-written with the new character defined in the new character set.</remarks>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _lcd.CharacterSet = SerialLCD.CharacterSets.User;
        /// </code>
        /// <code language = "VB">
        /// _lcd.CharacterSet = SerialLCD.CharacterSets.User
        /// </code>
        /// </example>
        public CharacterSets CharacterSet
        {
            get { return _characterSet; }
            set
            {
                _characterSet = value;

                if (_characterSet == CharacterSets.User)
                {
                    SetupUserDefinedCustomCharacters();
                }
                else
                {
                    SetupPredefinedCustomCharacters();
                }
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
        /// Debug.Print("Driver Version Info : " + _clock1.DriverVersion);
        /// </code>
        /// <code language="VB">
        /// Debug.Print("Driver Version Info : " <![CDATA[&]]> _clock1.DriverVersion)
        /// </code>
        /// </example>
        public Version DriverVersion
        {
            get { return Assembly.GetAssembly(GetType()).GetName().Version; }
        }

        /// <summary>
        ///     Gets or sets the power mode.
        /// </summary>
        /// <value>
        ///     The current power mode of the module.
        /// </value>
        /// <returns cref="NotImplementedException">Calling this method will throw a <see cref="NotImplementedException"/>.</returns>
        /// <remarks>
        ///     This module does not use Power Modes, the GET accessor will always return PowerModes.On. See <see cref="PowerModes"/>, while the SET accessor will do nothing.
        /// </remarks>
        /// <exception cref="NotImplementedException"></exception>
        /// <example>None: This sensor does not support PowerMode.</example>
        public PowerModes PowerMode
        {
            get { return PowerModes.On; }
            set {  }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes one of the eight Custom Characters that are available to the SerialLCD.  
        /// </summary>
        /// <param name="characters">The <see cref="UserCharacters"/> to setup.</param>
        /// <param name="characterBytes">The byte pattern representing the 5x7 matrix.</param>
        /// <example>Example usage:
        /// <code language = "C#">
        ///  var bell = new Byte[] {4, 14, 14, 14, 31, 31, 4, 0};
        /// _lcd.SetUserCharacter(SerialLCD.UserCharacters.UserCharacter1, bell);
        /// // To display the bell characters
        ///  _lcd.PutC((byte)SerialLCD.UserCharacters.UserCharacter1, 0, 0);
        /// </code>
        /// <code language = "VB">
        /// Dim bell = New [Byte]() {4, 14, 14, 14, 31, 31, 4, 0}
        /// _lcd.SetUserCharacter(SerialLCD.UserCharacters.UserCharacter1, bell)
        /// ' To display the bell characters
        /// _lcd.PutC(CByte(SerialLCD.UserCharacters.UserCharacter1), 0, 0)
        /// </code>
        /// </example>
        public void SetUserCharacter(UserCharacters characters, byte[] characterBytes)
        {
            switch (characters)
            {
                case UserCharacters.UserCharacter1:
                    _userChartacterOne = characterBytes;
                    break;
                case UserCharacters.UserCharacter2:
                    _userChartacterTwo = characterBytes;
                    break;
                case UserCharacters.UserCharacter3:
                    _userChartacterThree = characterBytes;
                    break;
                case UserCharacters.UserCharacter4:
                    _userChartacterFour = characterBytes;
                    break;
                case UserCharacters.UserCharacter5:
                    _userChartacterFive = characterBytes;
                    break;
                case UserCharacters.UserCharacter6:
                    _userChartacterSix = characterBytes;
                    break;
                case UserCharacters.UserCharacter7:
                    _userChartacterSeven = characterBytes;
                    break;
                case UserCharacters.UserCharacter8:
                    _userChartacterEight = characterBytes;
                    break;
            }

        }

        /// <summary>
        /// Sets the cursor to one of the predefined <see cref="CursorStyle"/> types.
        /// </summary>
        /// <param name="style">The style to set to.</param>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _lcd.SetCursorStyle(SerialLCD.CursorStyle.CursorOffCharacterBlink);
        /// </code>
        /// <code language = "VB">
        /// _lcd.SetCursorStyle(SerialLCD.CursorStyle.CursorOffCharacterBlink)
        /// </code>
        /// </example>
        public void SetCursorStyle(CursorStyle style)
        {
            _serialLcd.WriteByte((byte)(style));
        }

        /// <summary>
        /// Turns off the Display.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _lcd.DisplayOff();
        /// </code>
        /// <code language = "VB">
        /// _lcd.DisplayOff();
        /// </code>
        /// </example>
        public void DisplayOff()
        {
            _serialLcd.WriteByte(DISPLAY_OFF);
        }

        /// <summary>
        /// Turns on the display.
        /// </summary>
        /// <param name="mode"></param>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _lcd.DisplayOn(SerialLCD.CursorStyle.NoCursor);
        /// </code>
        /// <code language = "VB">
        /// _lcd.DisplayOn(SerialLCD.CursorStyle.NoCursor)
        /// </code>
        /// </example>
        public void DisplayOn(CursorStyle mode)
        {
            _serialLcd.WriteByte((byte)(mode));
        }

        /// <summary>
        /// Displays the passed string at the current cursor location. Non printable characters are replaced with a space characters.
        /// </summary>
        /// <param name="s">The string to display.</param>
        /// <example>Example usage:
        /// <code language = "C#">
        ///  _lcd.Print("Hello MikroBus.Net");
        /// </code>
        /// <code language = "VB">
        ///  _lcd.Print("Hello MikroBus.Net");
        /// </code>
        /// </example>
        public void Print(string s)
        {
            var fixedString = ReplaceNonPrintableCharacters(s, ' ');
            byte[] buffer = Encoding.UTF8.GetBytes(fixedString);
            _serialLcd.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Displays the passed string at the specified row and column. Non printable characters are replaced with a space characters.
        /// </summary>
        /// <param name="s">The string to display.</param>
        /// <param name="row">The row to display the passed string.</param>
        /// <param name="col">The column to display the passed string.</param>
        /// <example>Example usage:
        /// <code language = "C#">
        ///  _lcd.Print("Hello MikroBus.Net", 0, 0);
        /// </code>
        /// <code>
        /// _lcd.Print("Hello MikroBus.Net", 0, 0)
        /// </code>
        /// </example>
        public void Print(string s, byte row, byte col)
        {
            var fixedString = ReplaceNonPrintableCharacters(s, ' ');
            byte[] buffer = Encoding.UTF8.GetBytes(fixedString);
            SetCursorPosition(row, col);
            _serialLcd.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Displays a single byte characters at the current cursor location
        /// </summary>
        /// <param name="b">The byte value of the characters to display.</param>
        /// <example>Example usage:
        /// <code language = "C#">
        ///  _lcd.PutC((byte)'C', 0, 0);
        /// // or
        /// _lcd.PutC((byte)SerialLCD.CustomCharacters.DegreesSymbol);
        /// </code>
        /// <code language = "VB">
        /// _lcd.PutC(CByte(Strings.AscW("C"c)), 0, 0)
        /// ' or
        /// _lcd.PutC(CByte(SerialLCD.CustomCharacters.DegreesSymbol))
        /// </code>
        /// </example>
        public void PutC(byte b)
        {
            _serialLcd.WriteByte(b);
        }

        /// <summary>
        /// Displays a single byte characters at the specified row and column.
        /// </summary>
        /// <param name="b">The byte value of the characters to display.</param>
        /// <param name="row">The row in which to display the passed byte.</param>
        /// <param name="col">The column in which to display the passed byte.</param>
        /// <example>Example usage:
        /// <code language = "C#">
        ///  _lcd.PutC((byte)'C', 0, 0);
        /// // or
        /// _lcd.PutC((byte)SerialLCD.CustomCharacters.DegreesSymbol, 0, 0);
        /// </code>
        /// <code language = "VB">
        /// _lcd.PutC(CByte(Strings.AscW("C"c)), 0, 0)
        /// ' or
        /// _lcd.PutC(CByte(SerialLCD.CustomCharacters.DegreesSymbol), 0, 0)
        /// </code>
        /// </example>
        public void PutC(byte b, byte row, byte col)
        {
            SetCursorPosition(row, col);
            _serialLcd.WriteByte(b);
        }

        /// <summary>
        /// Clears the display.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _lcd.ClearScreen();
        /// </code>
        /// <code language = "VB">
        /// _lcd.ClearScreen();
        /// </code>
        /// </example>
        public void ClearScreen()
        {
            _serialLcd.WriteByte(CLEAR_DISP);
            Thread.Sleep(5);
        }

        /// <summary>
        /// Moves the cursor insertion point down one row. If at the bottom row, it will wrap to the top row.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _lcd.FormFeed();
        /// </code>
        /// <code language = "VB">
        /// _lcd.FormFeed()
        /// </code>
        /// </example>
        public void FormFeed()
        {
            _serialLcd.WriteByte(FORM_FEED);
            Thread.Sleep(5); // Wait 5 seconds per Parallax datasheet.
        }

        /// <summary>
        /// Sets the cursor to the Home position (Row = 0, Column = 0).
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _lcd.CursorHome();
        /// </code>
        /// <code language = "VB">
        /// _lcd.CursorHome()
        /// </code>
        /// </example>
        public void CursorHome()
        {
            _serialLcd.WriteByte(CURSOR_HOME);
        }

        /// <summary>
        /// Sets the cursor position to the specified row and column.
        /// </summary>
        /// <param name="row">The Row in which to set the cursor position to.</param>
        /// <param name="col">The Column in which to set the cursor position to.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <example>Example usage:
        /// <code language = "C#">
        ///  _lcd.SetCursorPosition(1, 15);
        /// </code>
        /// <code language = "VB">
        ///  _lcd.SetCursorPosition(1, 15)
        /// </code>
        /// </example>
        public void SetCursorPosition(byte row, byte col)
        {
            if (_displaySize == DisplaySize.Size2X16)
            {
                if (row > 1 || col > 15)
                {
                    throw new ArgumentException("For 2x16 Displays, Rows and Columns are Zero Based, Row (0 - 1) and Column (0 - 15). The row can not be larger than 1 and/or the column can not be larger than 15.");
                }
            }
            else
            {
                if (row > 3 || col > 19)
                {
                    throw new ArgumentException("For 4x20 Displays, Rows and Columns are Zero Based, Row (0 - 3) and Column (0 - 19). The row can not be larger than 3 and/or the column can not be larger than 19.");
                }
            }
            var rowOffset = new byte[] { 0, 20, 40, 60 };
            _serialLcd.WriteByte((byte)(SET_CURSOR + col + rowOffset[row]));
        }

        /// <summary>
        /// Sets the cursor left one position in the current row. If the cursor is at position (1, 0) it will wrap to the Home Position (0, 15).
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _lcd.CursorLeft();
        /// </code>
        /// <code language = "VB">
        /// _lcd.CursorLeft()
        /// </code>
        /// </example>
        public void CursorLeft()
        {
            _serialLcd.WriteByte(CURSOR_LEFT);
        }

        /// <summary>
        /// Sets the cursor position one position to the right in the current row. If the cursor is at the last position in the last row (1, 15 or 3, 19) it will wrap to the Home Position (0, 0). If the cursor position is in the last position of rows 0, 1, 2 it will wrap to the first position of the next row.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _lcd.CursorRight();
        /// </code>
        /// <code language = "VB">
        /// _lcd.CursorRight()
        /// </code>
        /// </example>
        public void CursorRight()
        {
            _serialLcd.WriteByte(CURSOR_LEFT);
        }

        /// <summary>
        /// Sets the BackLight on of off.
        /// </summary>
        /// <param name="toggle">True for On, False for Off.</param>
        /// <example>Example usage:
        /// <code language = "C#">
        ///  _lcd.SetBacklight(true);
        /// </code>
        /// <code language = "VB">
        ///  _lcd.SetBacklight(True)
        /// </code>
        /// </example>
        public void SetBacklight(bool toggle)
        {
            _serialLcd.WriteByte(toggle ? BACKLIGHT_ON : BACKLIGHT_OFF);
        }

        /// <summary>
        /// Plays a simple and annoying warning tone.
        /// </summary>
        /// <example>
        /// <code language = "C#">
        /// var counter = 0;
        /// for (int x = 0; x <![CDATA[<]]>= 100 ; x++)
        /// {
        ///     if (x = 5) continue;
        ///    _lcd.PlayWarningTone();
        /// }
        /// </code>
        /// <code language = "VB">
        /// Dim counter as Integer = 0
        /// For x As Integer = 0 To 100
        ///     If counter = 5 Then Continue For
        /// 	_lcd.PlayWarningTone()
        ///     counter += 1
        /// Next
        /// </code>
        /// </example>
        public void PlayWarningTone()
        {
            _serialLcd.WriteByte(QUARTERNOTE);
            _serialLcd.WriteByte(SEVENTHOCTAVE);
            _serialLcd.WriteByte(NOTE_GSHARP);
            _serialLcd.WriteByte(WHOLENOTE);
            _serialLcd.WriteByte(NOTE_PAUSE);
        }

        /// <summary>
        ///     Resets the SerialLCD Module.
        /// </summary>
        /// <param name="resetMode">The reset mode, see <see cref="ResetModes"/> for more information.</param>
        /// <returns cref="NotImplementedException">Calling this method will throw a <see cref="NotImplementedException"/>.</returns>
        /// <remarks>
        ///     This module has no Reset method, calling this method will throw a <see cref="NotImplementedException"/>.
        /// </remarks>
        /// <exception cref="NotImplementedException"></exception>
        /// <example>None: This sensor does not support a Reset method.</example>
        public bool Reset(ResetModes resetMode)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}