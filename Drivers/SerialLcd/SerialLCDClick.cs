/*
 * Sparkfun Serial LCD driver
 * 
 * Version 1.0 :
 *  - Initial revision coded by Stephen Cardinale
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


using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using MBN.Exceptions;

namespace MBN.Modules
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    ///     A MBN driver class to use a 2x16 or 4x20 Serial LCD manufactured by Parallax or any Serial LCD.
    ///     See http://www.parallax.com/product/27976
    ///     or http://www.parallax.com/product/27979
    /// <para><b>Pins used :</b> Tx</para>
    /// </summary>
    public class SerialLCD
    {
        #region CTOR

        /// <summary>
        ///     Default constructor. Creates a new instance of the LCD driver.
        /// </summary>
        /// <param name="socket">"The MBN Board Socket that the Serial LCD is plugged into. </param>
        /// <param name="baudRate">The Baud Rate that the Serial LCD is set to mommunicate at.</param>
        /// <param name="parity">Parity, typically N or None.</param>
        /// <param name="dataBits">DataBits, typically 8.</param>
        /// <param name="stopBits">StopBits, typically 1.</param>
        /// <param name="size">Display size, either 2x16 or 4x20.</param>
        public SerialLCD(Hardware.Socket socket, Baud baudRate, Parity parity, int dataBits, StopBits stopBits, DisplaySize size)
        {
            try
            {
                Hardware.CheckPins(socket, socket.Tx);
                _serialLcd = new SerialPort(socket.ComPort, (int) baudRate, parity, dataBits, stopBits);
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

        private void InitializeDisplay()
        {
            if (_serialLcd.IsOpen)
            {
                Thread.Sleep(100); // Pause 100 mSec to allow the display to startup per Parallax specification.
                SetBacklight(false);
                DisplayOff();
                ClearScreen();
                SetCursor(0, 0);
                SetupCustomCharacters();
                DisplayOn(DisplayMode.NoCursor);
                SetBacklight(true);
            }
            else
            {
                throw new SystemException("Error connecting to LCD, please check hardware pin assignments");
            }
        }

        private void SetupCustomCharacters()
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

        #endregion

        #region ENUMS

        /// <summary>
        ///     Baudrate enumeration, selectable from 2400 to 19200
        /// </summary>
        public enum Baud
        {
            /// <summary>
            ///     Baudrate 2400
            /// </summary>
            BaudRate2400 = 2400,

            /// <summary>
            ///     Baudrate 9600
            /// </summary>
            BaudRate9600 = 9600,

            /// <summary>
            ///     Baudrate 19200
            /// </summary>
            BaudRate19200 = 19200
        }

        /// <summary>
        ///     Enumeration for predefined custom charactes.
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
        ///     Display mode enumeration (blinking cursor, no cursor, etc.
        /// </summary>
        public enum DisplayMode : byte
        {
            /// <summary>
            ///     Display is on cursor not blinking.
            /// </summary>
            NoCursor = DSPLAYON_NOCURSOR,

            /// <summary>
            ///     Display is on, cursor is off with the last character blinking.
            /// </summary>
            CursorOffCharacterBlink = DISPLAYON_CURSOROFF_CHARBLINK,

            /// <summary>
            ///     Display is on, cursor is on character not blinking
            /// </summary>
            CursorOnNoBlink = DISPLAYON_CURSORON_NOBLINK,

            /// <summary>
            ///     Display is on, cursor is on with last character blinking.
            /// </summary>
            CursorBlinkCharacterBlink = DISPLAYON_CURSORBLINK_CHARBLINK
        }

        /// <summary>
        ///     LCD Character Display size 2x16 or 4x20
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

        #region Backlight

        private const byte BACKLIGHT_ON = 17; //Turn LCD Backlight on.
        private const byte BACKLIGHT_OFF = 18; //Turn LCD Backlight off.

        #endregion

        #region Display Functions

        private const byte CLEAR_DISP = 12; //Clear the display see FORMFEED above. Users must pause 5mSec after this command.
        private const byte DISPLAY_OFF = 21; //Turns the display off.
        private const byte DSPLAYON_NOCURSOR = 22; //Turn the display on, with cursor off and no blink
        private const byte DISPLAYON_CURSOROFF_CHARBLINK = 23; //Turn the display on, with cursor off and character blink.
        private const byte DISPLAYON_CURSORON_NOBLINK = 24; //Turn the display on, with cursor on and no blink (Default)
        private const byte DISPLAYON_CURSORBLINK_CHARBLINK = 25; //Turn the display on, with cursor on and character blink

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
        private readonly byte[] _fiveBars = {0, 0, 0, 31, 31, 31, 31, 31};
        private readonly byte[] _fourBars = {0, 0, 0, 0, 31, 31, 31, 31};
        private readonly byte[] _oneBar = {0, 0, 0, 0, 0, 0, 0, 31};
        private readonly byte[] _sevenBars = {31, 31, 31, 31, 31, 31, 31, 31};
        private readonly byte[] _sixBars = {0, 31, 31, 31, 31, 31, 31, 31};
        private readonly byte[] _threeBars = {0, 0, 0, 0, 0, 31, 31, 31};
        private readonly byte[] _twoBars = {0, 0, 0, 0, 0, 0, 31, 31};

        #endregion

        #endregion

        #region Properties

        /// <summary>
        ///     Property used to determine if the speaker of the LCD is currently playing a tone.
        /// </summary>
        public bool PlayingWarningTone { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Turns off the Display.
        /// </summary>
        public void DisplayOff()
        {
            _serialLcd.WriteByte(DISPLAY_OFF);
        }

        /// <summary>
        ///     Turns on the display.
        /// </summary>
        /// <param name="mode"></param>
        public void DisplayOn(DisplayMode mode)
        {
            _serialLcd.WriteByte((byte) (mode));
        }

        /// <summary>
        ///     Displays the passed string at the current cursor location.
        /// </summary>
        /// <param name="s">Thge string to display.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Print(string s)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(s);
            if (buffer == null) throw new ArgumentNullException("buffer");
            _serialLcd.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        ///     Displays the passed string at the specified row and column.
        /// </summary>
        /// <param name="s">Thge string to display.</param>
        /// <param name="row">The row to display the passed string.</param>
        /// <param name="col">The column to display the passed string.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Print(string s, byte row, byte col)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(s);
            if (buffer == null) throw new ArgumentNullException("buffer");
            SetCursor(row, col);
            _serialLcd.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        ///     Displays a single byte character at the current cursor location
        /// </summary>
        /// <param name="b">The byte bvalue of the character to display.</param>
        public void PutC(byte b)
        {
            _serialLcd.WriteByte(b);
        }

        /// <summary>
        ///     Displays a single byte character at the specified row and column.
        /// </summary>
        /// <param name="b">The byte bvalue of the character to display.</param>
        /// <param name="row">The row in which to display the passed byte.</param>
        /// <param name="col">The column in which to display the passed byte.</param>
        public void PutC(byte b, byte row, byte col)
        {
            SetCursor(row, col);
            _serialLcd.WriteByte(b);
        }

        /// <summary>
        ///     Clears the display.
        /// </summary>
        public void ClearScreen()
        {
            _serialLcd.WriteByte(CLEAR_DISP);
            Thread.Sleep(5);
        }

        /// <summary>
        ///     Moves the cursor insertion point down one row. If at the bottom row, it will wrap to the top row.
        /// </summary>
        public void FormFeed()
        {
            _serialLcd.WriteByte(FORM_FEED);
            Thread.Sleep(5); // Wait 5 seconds per Sparkfun datasheet.
        }

        /// <summary>
        ///     Sets the cursor to the Home position (Row = 0, Column = 0).
        /// </summary>
        public void CursorHome()
        {
            _serialLcd.WriteByte(CURSOR_HOME);
        }

        /// <summary>
        ///     Sets the cursor positio to the specified row and column.
        /// </summary>
        /// <param name="row">The Row in which to set the cursor position to.</param>
        /// <param name="col">The Column in which to set the cursor position to.</param>
        /// <exception cref="ArgumentException"></exception>
        public void SetCursor(byte row, byte col)
        {
            if (_displaySize == DisplaySize.Size2X16)
            {
                if (row > 1 || col > 15)
                {
                    throw new ArgumentException("Row and Column are Zero Based, Row (0 - 1) and Column (0 - 15). The row can not be larger than 3 and/or the column can not be larger than 15.");
                }
            }
            else
            {
                if (row > 3 || col > 19)
                {
                    throw new ArgumentException("Row and Column are Zero Based, Row (0 - 3) and Column (0 - 19). The row can not be larger than 3 and/or the column can not be larger than 19.");
                }
            }
            var rowOffset = new byte[] {0, 20, 40, 60};
            _serialLcd.WriteByte((byte) (SET_CURSOR + col + rowOffset[row]));
        }

        /// <summary>
        ///     Sets the cursor left one position in the current row.
        ///     If the cursor is at the Home Position (1, 15) it will wrap to the Home Position (0, 0).
        /// </summary>
        public void CursorLeft()
        {
            _serialLcd.WriteByte(CURSOR_LEFT);
        }

        /// <summary>
        ///     Sets the cursor position one position to the right in the current row.
        ///     If the cursor is at the last position in the last row (1, 15 or 3, 19) it will wrap to the Home Position (0, 0).
        ///     If the cursor position is in the last position of rows 0, 1, 2 it will wrap to the first position of the next row.
        /// </summary>
        public void CursorRight()
        {
            _serialLcd.WriteByte(CURSOR_LEFT);
        }

        /// <summary>
        ///     Sets the Backlight on of off.
        /// </summary>
        /// <param name="toggle">True for On, False for Off.</param>
        public void SetBacklight(bool toggle)
        {
            _serialLcd.WriteByte(toggle ? BACKLIGHT_ON : BACKLIGHT_OFF);
        }

        /// <summary>
        ///     Plays a simple and annoying warning tone.
        /// </summary>
        public void PlayWarningTone()
        {
            PlayingWarningTone = true;
            _serialLcd.WriteByte(QUARTERNOTE);
           _serialLcd.WriteByte(SEVENTHOCTAVE);
           _serialLcd.WriteByte(NOTE_GSHARP);
           _serialLcd.WriteByte(WHOLENOTE);
           _serialLcd.WriteByte(NOTE_PAUSE);
           PlayingWarningTone = false;
        }

        /// <summary>
        /// Stops the (annoying) warning tone.
        /// </summary>
        public void StopWarningTone()
        {
            if (!PlayingWarningTone) return;
            PlayingWarningTone = false;
        }
        #endregion
    }
}