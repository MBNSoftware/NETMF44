/*
 * Devantech LCD03 driver
 * 
 * Version 2.0 :
 *  - Conformance to the new namespaces and organization
 *  
 * Version 1.0 :
 *  - Initial revision coded by Christophe Gerbier
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
using System.Reflection;
using MBN.Enums;
using MBN.Exceptions;
using Microsoft.SPOT.Hardware;
using MBN.Extensions;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the Devantech LCD03 driver
    /// <para><b>Pins used :</b> Scl, Sda in I²C mode. Tx, Rx in UART mode.</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///     private static DevantechLcd03 _lcd;
    ///
    ///     public static void Main()
    ///     {
    ///         _lcd = new DevantechLcd03(Hardware.SocketOne, 0xC8 >> 1)
    ///         {
    ///             BackLight = true,
    ///             Cursor = DevantechLcd03.Cursors.Hide
    ///         };
    ///         _lcd.ClearScreen();
    ///         _lcd.Write(1, 1, "Hello world !");
    ///         _lcd.Write(1, 3, "Version " + _lcd.DriverVersion);
    ///     }
    /// }
    /// </code>
    /// </example>
    public class DevantechLcd03 : IDriver
    {
        /// <summary>
        /// List of cursor types available on the LCD
        /// </summary>
        public enum Cursors
        {
            /// <summary>
            /// Hides the cursor
            /// </summary>
            Hide,
            /// <summary>
            /// Cursor is a steady underline
            /// </summary>
            Underline,
            /// <summary>
            /// Cursor is a blinking block
            /// </summary>
            Blink
        };

        private readonly I2CDevice.Configuration _config;                      // I²C configuration
        private readonly Boolean _isUart;
        private readonly SerialPort _uart;
        private Cursors _cursor;
        private Boolean _backLight;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevantechLcd03"/> class using I²C communication
        /// </summary>
        /// <param name="socket">The socket on MBN board where the Lcd is connected</param>
        /// <param name="address">I²C address (7 bits) of the LCD. </param>
        public DevantechLcd03(Hardware.Socket socket, Byte address)     // I²C mode
        {
            try
            {
                // Checks if needed I²C pins are available
                Hardware.CheckPinsI2C(socket);
                // Create the driver's I²C configuration
                _config = new I2CDevice.Configuration(address, (Int32)ClockRatesI2C.Clock100KHz);

                _isUart = false;
                Init();
            }
            // Catch only the PinInUse exception, so that program will halt on other exceptions
            // Send it directly to caller
            catch (PinInUseException) { throw new PinInUseException(); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DevantechLcd03"/> class using serial communication (UART)
        /// </summary>
        /// <param name="socket">The socket on MBN board where the Lcd is connected</param>
        public DevantechLcd03(Hardware.Socket socket)       // UART mode
        {
            try
            {
                // Checks if needed UART pins are available
                Hardware.CheckPins(socket, socket.Tx, socket.Rx);

                _uart = new SerialPort(socket.ComPort, 9600, Parity.None, 8, StopBits.Two);
                _uart.Open();

                _isUart = true;
                Init();
            }
            // Catch only the PinInUse exception, so that program will halt on other exceptions
            // Send it directly to caller
            catch (PinInUseException) { throw new PinInUseException(); }
        }

        private void Init()
        {
            _backLight = false;
            _cursor = Cursors.Blink;
        }

        /// <summary>
        /// Gets or sets the cursor shape.
        /// </summary>
        /// <value>
        /// The actual cursor's shape
        /// </value>
        /// <example>
        /// <code language="C#">
        ///     _lcd.Cursor = DevantechLcd03.Cursors.Hide;
        /// </code>
        /// </example>
        public Cursors Cursor
        {
            get { return _cursor; }
            set
            {
                if (_isUart) { _uart.Write(new[] {(byte) (4 + value)}, 0, 1); }
                else
                {
                    Hardware.I2CBus.Execute(_config, new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(new[] { (Byte)0, (byte)(4 + value) }) }, 1000);
                }
                _cursor = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether back light is turned on or off.
        /// </summary>
        /// <value>
        ///   <c>true</c> if back light is on, otherwise <c>false</c>.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///     _lcd.BackLight = true;
        /// </code>
        /// </example>
        public Boolean BackLight
        {
            get { return _backLight; }
            set
            {
                if (_isUart) { _uart.Write(new [] {value ? (Byte) 19 : (Byte) 20}, 0, 1); }
                else
                {
                    Hardware.I2CBus.Execute(_config, new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(new[] { (Byte)0, value ? (Byte)19 : (Byte)20 }) }, 1000);
                }
                _backLight = value;
            }
        }

        /// <summary>
        /// Sets the cursor at specified coordinates
        /// </summary>
        /// <param name="x">The column (1 to 20)</param>
        /// <param name="y">The line (1 to 4)</param>
        /// <example>
        /// <code language="C#">
        ///     // Sets the cursor position on column 10 of line 2
        ///     _lcd.Cursor(10,2);
        /// </code>
        /// </example>
        public void SetCursor(byte x, byte y)
        {
            if (x <= 0 || x > 20 || y <= 0 || y > 4) { return; }
            if (_isUart) { _uart.Write(new byte[] { 3, y, x }, 0, 3); }
            else
            {
                Hardware.I2CBus.Execute(_config, new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(new byte[] { 0, 3, y, x }) }, 1000);
            }
        }

        /// <summary>
        /// Writes the specified text at the current cursor's position
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <example>
        /// <code language="C#">
        ///     // Write the "Hello world !" text at the current cursor's position
        ///     _lcd.Write("Hello world !");
        /// </code>
        /// </example>
        public void Write(string text)
        {
            if (_isUart)
            {
                byte[] str = System.Text.Encoding.UTF8.GetBytes(text);
                _uart.Write(str, 0, str.Length);
            }
            else
            {
                Hardware.I2CBus.Execute(_config, new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(System.Text.Encoding.UTF8.GetBytes((byte)0 + text)) }, 1000);
            }
        }

        /// <summary>
        /// Writes the specified text at (x,y) coordinates
        /// </summary>
        /// <param name="x">The column (1 to 20</param>
        /// <param name="y">The line (1 to 4)</param>
        /// <param name="text">The text to display</param>
        /// <example>
        /// <code language="C#">
        ///     // Write the "Hello world !" text on the first row, first column
        ///     _lcd.Write(1,1,"Hello world !");
        /// </code>
        /// </example>
        public void Write(byte x, byte y, string text)
        {
            if (x <= 0 || x > 20 || y <= 0 || y > 4) { return; }
            SetCursor(x, y);
            Write(text);
        }

        /// <summary>
        /// Clears the screen.
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///     _lcd.ClearScreen();
        /// </code>
        /// </example>
        public void ClearScreen()
        {
            if (_isUart) { _uart.Write(new byte[] { 12 }, 0, 1); }
            else
            {
                Hardware.I2CBus.Execute(_config, new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(new byte[] { 0, 12 }) }, 1000);
            }
        }

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">This module has no power modes features.</exception>
        public PowerModes PowerMode
        {
            get { return PowerModes.On; }
            set
            {
                throw new NotImplementedException("PowerMode");
            }
        }

        /// <summary>
        /// Gets the driver version.
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///      Debug.Print ("Current driver version : "+_lcd.DriverVersion);
        /// </code>
        /// </example>
        /// <value>
        /// The driver version.
        /// </value>
        public Version DriverVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        /// <summary>
        /// Herited from the IDriver interface but not used by this module.
        /// </summary>
        /// <param name="resetMode">The reset mode :
        /// <para>SOFT reset : generally by sending a software command to the chip</para><para>HARD reset : generally by activating a special chip's pin</para></param>
        /// <returns>True if Reset has been acknowledged, false otherwise.</returns>
        /// <exception cref="System.NotImplementedException">Thrown because this module has no Reset feature.</exception>
        public bool Reset(ResetModes resetMode)
        {
            throw new NotImplementedException("Reset");
        }
    }
}

