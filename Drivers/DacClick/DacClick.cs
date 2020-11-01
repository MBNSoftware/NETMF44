/*
 * DAC Click board driver
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
using System.Reflection;
using MBN.Enums;
using MBN.Exceptions;
using Microsoft.SPOT.Hardware;
using MBN.Extensions;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the DAC Click board driver
    /// <para><b>Pins used :</b> Miso, Mosi, Cs, Sck, Rst</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///    static DacClick _dac;
    ///
    ///    public static void Main()
    ///    {
    ///        _dac = new DacClick(Hardware.SocketOne);
    ///
    ///        // Set DAC to 80 mV (Gain x1)
    ///        _dac.Output = 100;
    ///        Thread.Sleep(2000);
    ///
    ///        // Set DAC to 800 mV (Gain x1)
    ///        _dac.Output = 1000;
    ///        Thread.Sleep(2000);
    ///
    ///        // Set DAC to 1600 mV (Gain x2)
    ///        _dac.Gain = DacClick.Gains.X2;
    ///        _dac.Output = 1000;
    ///        Thread.Sleep(2000);
    ///
    ///        // Shutdown DAC power
    ///        _dac.PowerMode = PowerModes.Off;
    ///
    ///         Thread.Sleep(Timeout.Infinite);
    ///    }
    ///}
    /// </code>
    /// </example>
    public class DacClick : IDriver
    {
        /// <summary>
        /// List of available gains for the DAC
        /// </summary>
        public enum Gains
        {
            /// <summary>
            /// Gain x1
            /// </summary>
            X1, 
            /// <summary>
            /// Gain x2
            /// </summary>
            X2
        };               // Gain values

        private readonly SPI.Configuration _spiConfig;                // SPI configuration
        private Byte _controlBits;                           // Control bits for gain, shutdown mode and buffer
        private UInt16 _outputValue;                         // Internal variable to hold output value
        private PowerModes _powerMode;                             // Is DAC in active mode or not
        private Boolean _isBuffered;                         // Buffered mode
        private Gains _gain;                                // Gain mode

        /// <summary>
        /// Initializes a new instance of the <see cref="DacClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the BarGraph Click board is plugged on MikroBus.Net</param>
        /// <exception cref="System.InvalidOperationException">Thrown if some pins are already in use by another board on the same socket</exception>
        public DacClick(Hardware.Socket socket)
        {
            try
            {
                // Checks if needed SPI pins are available
                Hardware.CheckPins(socket, socket.Miso, socket.Mosi, socket.Cs, socket.Sck, socket.Rst);

                // Initialize SPI
                _spiConfig = new SPI.Configuration(socket.Cs, false, 15, 10, false, true, 20000, socket.SpiModule);
                if (Hardware.SPIBus == null) { Hardware.SPIBus = new SPI(_spiConfig); }

                _controlBits = 0x30;     // Unbuffered, Gain 1x, active mode
                Output = 0;             // Clean output value
                _powerMode = PowerModes.On;         // Active
                _isBuffered = false;     // Unbuffered
                _gain = Gains.X1;       // Gain 1x
            }
            // Catch only the PinInUse exception, so that program will halt on other exceptions
            // Send it directly to caller
            catch (PinInUseException) { throw new PinInUseException(); }
        }

        /// <summary>
        /// Gets or sets the output value of DAC.
        /// </summary>
        /// <value>
        /// The output from 0 to 4095 which will give 0 to 3.3 V
        /// </value>
        /// <example>
        /// <code language="C#">
        ///     _dac.Output = 1000;
        /// </code>
        /// </example>
        public UInt16 Output
        {
            get { return _powerMode == PowerModes.Off ? (UInt16)0 : _outputValue; }
            set
            {
                _outputValue = value > (UInt16)4095 ? (UInt16)4095 : value;
                var high = (Byte)((_outputValue >> 8) & 0x0F);
                high |= _controlBits;
                var low = (Byte)_outputValue;
                Hardware.SPIBus.Write(_spiConfig, new [] { high, low });
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Buffered mode is active.
        /// <para>See datasheet for help on buffered/unbuffered mode.</para>
        /// </summary>
        /// <value>
        ///   <c>true</c> if Buffered mode is active; otherwise, <c>false</c>.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///     _dac.Buffered = false;
        /// </code>
        /// </example>
        public Boolean Buffered
        {
            get { return _isBuffered; }
            set
            {
                _isBuffered = value;
                Bits.Set(ref _controlBits, _isBuffered ? "x1xx0000" : "x0xx0000");
                _outputValue = 0;
                Hardware.SPIBus.Write(_spiConfig, new Byte[] { _controlBits, 0x00 });
            }
        }

        /// <summary>
        /// Gets or sets the gain for output.
        /// </summary>
        /// <value>
        /// The gain value : 1x or 2x.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///     _dac.Gain = DacClick.Gains.X2;
        /// </code>
        /// </example>
        public Gains Gain
        {
            get { return _gain; }
            set
            {
                _gain = value;
                Bits.Set(ref _controlBits, _gain == Gains.X1 ? "xx1x0000" : "xx0x0000");
                _outputValue = 0;
                Hardware.SPIBus.Write(_spiConfig, new Byte[] { _controlBits, 0x00 });
            }
        }

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">Thrown if PowerModes.Low requested.</exception>
        /// <example>
        /// <code language="C#">
        ///     _dac.PowerMode = PowerModes.Off;
        /// </code>
        /// </example>
        public PowerModes PowerMode
        {
            get { return PowerModes.On; }
            set
            {
                if (value == PowerModes.Low) { throw new NotImplementedException("PowerModes.Low");}
                _powerMode = value;
                if (value == PowerModes.Off)
                {
                    Bits.Set(ref _controlBits, "xxx00000");
                    _outputValue = 0;
                    Hardware.SPIBus.Write(_spiConfig, new Byte[] {_controlBits, 0x00});
                }
                else
                {
                    Bits.Set(ref _controlBits, "xxx10000");
                    _outputValue = 0;
                    Hardware.SPIBus.Write(_spiConfig, new Byte[] { _controlBits, 0x00 });
                }
            }
        }

        /// <summary>
        /// Gets the driver version.
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///      Debug.Print ("Current driver version : "+_dac.DriverVersion);
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

