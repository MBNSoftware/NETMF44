/*
 * Led8x8 R Click board driver
 * 
 *  Version 2.0
 *  - Conformance to the new namespaces and organization
 *  
 *  Version 1.1 :
 *  - Use of SPI extension methods for thread safety
 *  - Some renamings to please reSharper :-)
 *  
 * Version 1.0 :
 *  - Initial revision coded by Christophe Gerbier
 * 
 * References needed :
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Hardware.PWM
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

using System.Reflection;
using MBN.Enums;
using Microsoft.SPOT.Hardware;
using System;
using MBN.Extensions;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the Led8x8 R Click board driver
    /// <para><b>Pins used :</b> Miso, Mosi, Cs, Sck</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///    static Led8X8Click _leds;
    ///
    ///    public static void Main()
    ///    {
    ///        _leds = new Led8X8Click(Hardware.SocketOne) {Brightness = 1};
    ///        _leds.Display(true);
    ///        _leds.Clear();
    ///
    ///        DemoPixels();
    ///
    ///        Thread.Sleep(1000);
    ///        _leds.Display(false);
    ///
    ///        Thread.Sleep(Timeout.Infinite);
    ///    }
    /// 
    ///    private static void DemoPixels()
    ///    {
    ///        for (Byte i = 1; i &lt; 9; i++)
    ///        {
    ///            for (Byte j = 1; j &lt; 9; j++)
    ///            {
    ///                _leds.SetPixel(i, j, true, true);
    ///                Thread.Sleep(50);
    ///                _leds.SetPixel(i, j, false, true);
    ///            }
    ///        }
    ///
    ///        for (Byte i = 1; i &lt; 9; i++)
    ///        {
    ///            for (Byte j = 1; j &lt; 9; j++)
    ///            {
    ///                _leds.SetPixel(j, i, true, true);
    ///                Thread.Sleep(50);
    ///                _leds.SetPixel(j, i, false, true);
    ///            }
    ///        }
    ///    }
    /// }
    /// </code>
    /// </example>
    public partial class Led8X8Click : IDriver
    {
        private readonly SPI.Configuration _spiConfig;                // SPI configuration
        private Byte _brightness;                           // Brightness level
        private Byte[] _screen;                              // Internal buffer to hold display

        /// <summary>
        /// Initializes a new instance of the <see cref="Led8X8Click"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the Led 8x8 Click board is plugged on MikroBus.Net</param>
        /// <param name="initialState">Initial state of display : enabled (true) or disabled (false). Defaults to Enabled (true).</param>
        /// <exception cref="System.InvalidOperationException">Thrown if some pins are already in use by another board on the same socket</exception>
        public Led8X8Click(Hardware.Socket socket, Boolean initialState = true)
        {
            // Checks if needed SPI pins are available
            Hardware.CheckPins(socket, socket.Miso, socket.Mosi, socket.Cs, socket.Sck);

            // Initialize SPI
            _spiConfig = new SPI.Configuration(socket.Cs, false, 50, 0, false, true, 2000, socket.SpiModule);
            if (Hardware.SPIBus == null) { Hardware.SPIBus = new SPI(_spiConfig); }

            Init(initialState);
        }

        private void Init(Boolean initialState)
        {
            Hardware.SPIBus.Write(_spiConfig, new Byte[] { Registers.DISPLAY_TEST, 0x00 });        // Normal mode
            Hardware.SPIBus.Write(_spiConfig, new Byte[] { Registers.NO_OP, 0xFF });               // No op
            Hardware.SPIBus.Write(_spiConfig, new Byte[] { Registers.DECODE_MODE, 0x00 });         // No decoding
            Hardware.SPIBus.Write(_spiConfig, new Byte[] { Registers.INTENSITY, 0x01 });           // Brightness
            Hardware.SPIBus.Write(_spiConfig, new Byte[] { Registers.SCAN_LIMIT, 0x07 });          // Display refresh
            Hardware.SPIBus.Write(_spiConfig, new [] { Registers.SHUTDOWN, initialState ? (Byte)0x01 : (Byte)0x00 });      // Turn display on/off
            _screen = new Byte[8];
        }

        /// <summary>
        /// Refreshes the display by transfering the internal buffer content to the chip.
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///      _led8x8.Refresh();
        /// </code>
        /// </example>
        public void Refresh()
        {
            for (Byte i = 0; i < 8; i++)
            {
                Hardware.SPIBus.Write(_spiConfig, new [] { (Byte)(i + 1), _screen[i] });
            }
        }

        /// <summary>
        /// Enables or disables the physical display
        /// </summary>
        /// <param name="state">If set to <c>true</c> then display is enabled, otherwise it is disabled.</param>
        /// <example>
        /// <code language="C#">
        ///     // Turns off display
        ///      _led8x8.Display(false);
        /// </code>
        /// </example>
        public void Display(Boolean state)
        {
            Hardware.SPIBus.Write(_spiConfig, new [] { Registers.SHUTDOWN, state ? (Byte)0x01 : (Byte)0x00 });      // Turn on display
        }

        /// <summary>
        /// Clears the leds buffer.
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///      _led8x8.Clear();
        /// </code>
        /// </example>
        public void Clear()
        {
            for (Byte i = 0; i < 8; i++)
            {
                _screen[i] = 0x00;
            }
            Refresh();
        }

        /// <summary>
        /// Draws a complete column on the display.
        /// </summary>
        /// <param name="numCol">The column number, from 1 to 8.</param>
        /// <param name="state">If set to <c>true</c>, then the column's leds are lit. Otherwise, leds are turned off.</param>
        /// <param name="doRefresh">If set to <c>true</c> then internal buffer is flushed to the chip.</param>
        /// <example>
        /// <code language="C#">
        ///     // Turns on column 2 with immediate refresh
        ///      _led8x8.DrawColumn(2, true, true);
        /// </code>
        /// </example>
        public void DrawColumn(Byte numCol, Boolean state, Boolean doRefresh = false)
        {
            _screen[numCol - 1] = state ? (Byte)0xFF : (Byte)0x00;
            if (doRefresh) { Refresh(); }
        }

        /// <summary>
        /// Draws a complete line (row) on the display.
        /// </summary>
        /// <param name="numRow">The row number, from 1 to 8.</param>
        /// <param name="state">If set to <c>true</c>, then the row's leds are lit. Otherwise, leds are turned off.</param>
        /// <param name="doRefresh">If set to <c>true</c> then internal buffer is flushed to the chip.</param>
        /// <example>
        /// <code language="C#">
        ///     // Turns on row 2 with immediate refresh
        ///      _led8x8.DrawRow(2, true, true);
        /// </code>
        /// </example>
        public void DrawRow(Byte numRow, Boolean state, Boolean doRefresh = false)
        {
            for (Byte i = 0; i < 8; i++)
            {
                if (state) { _screen[i] |= (Byte)(1 << (numRow - 1)); }
                else { _screen[i] &= (Byte)(0xFF - (1 << (numRow - 1))); }
            }
            if (doRefresh) { Refresh(); }
        }


        /// <summary>
        /// Sets one pixel on the display.
        /// </summary>
        /// <param name="row">The row in the range 1 to 8.</param>
        /// <param name="col">The column in the range 1 to 8.</param>
        /// <param name="state">If set to <c>true</c>, then the pixel's led is lit. Otherwise, led is turned off.</param>
        /// <param name="doRefresh">If set to <c>true</c> then internal buffer is flushed to the chip.</param>
        /// <example>
        /// <code language="C#">
        ///     // Turns on pixel at row 3, column 2 with immediate refresh
        ///      _led8x8.SetPixel(3, 2, true, true);
        /// 
        ///     // Turns off pixel at row 1, column 1 with immediate refresh
        ///      _led8x8.SetPixel(1, 1, false, true);
        /// </code>
        /// </example>
        public void SetPixel(Byte row, Byte col, Boolean state, Boolean doRefresh = false)
        {
            if (state) { _screen[col - 1] |= (Byte)(1 << (row - 1)); }
            else { _screen[col - 1] &= (Byte)(0xFF - (1 << (row - 1))); }
            if (doRefresh) { Refresh(); }
        }

        /// <summary>
        /// Sends a complete 8 bytes array to the internal buffer
        /// </summary>
        /// <param name="source">The source byte array.</param>
        /// <param name="doRefresh">If set to <c>true</c> then internal buffer is flushed to the chip.</param>
        /// <example>
        /// <code language="C#">
        ///     // Sends an array of byte representing a big "0" char
        ///      var _digits = new Byte[] { 0x00, 0x3E, 0x7F, 0x49, 0x45, 0x7F, 0x3E, 0x00 };
        ///     _led8x8.SendArray(_digits, true);
        /// </code>
        /// </example>
        public void SendArray(Byte[] source, Boolean doRefresh = false)
        {
            _screen = (Byte[])source.Clone();
            if (doRefresh) { Refresh(); }
        }

        /// <summary>
        /// Gets or sets the brightness of the leds.
        /// </summary>
        /// <value>
        /// The brightness level, in the range 0 to 15
        /// </value>
        /// <example>
        /// <code language="C#">
        ///     // Sets brightness to half value
        ///     _led8x8.Brightness = 7;
        /// </code>
        /// </example>
        public Byte Brightness
        {
            get { return _brightness; }
            set
            {
                _brightness = value > 15 ? (Byte)15 : value;
                Hardware.SPIBus.Write(_spiConfig, new [] { Registers.INTENSITY, _brightness });      // Sets brightness level
            }
        }

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">Thrown because this module has no power mode feature.</exception>
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
        /// <value>
        /// The driver version.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///      Debug.Print ("Current driver version : "+_led8x8.DriverVersion);
        /// </code>
        /// </example>
        public Version DriverVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        /// <summary>
        /// Resets the module
        /// </summary>
        /// <param name="resetMode">The reset mode :
        /// <para>SOFT reset : generally by sending a software command to the chip</para><para>HARD reset : generally by activating a special chip's pin</para></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException">Thrown because this module has no reset feature.</exception>
        public bool Reset(ResetModes resetMode)
        {
            throw new NotImplementedException("Reset");
        }
    }
}
