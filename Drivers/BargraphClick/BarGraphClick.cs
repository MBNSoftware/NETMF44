/*
 * BarGraph Click board driver
 * 
 *  Version 2.0 :
 *  - Integration of the new namespaces and new organization.
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
    /// Main class for the MikroE BarGraph Click board driver
    /// <para><b>Pins used :</b> Miso, Mosi, Cs, Sck, Pwm, Int</para>
    /// </summary>
    /// <example> This sample shows a basic usage of the BarGraph module.
    /// <code language="C#">
    /// using System;
    /// using System.Threading;
    /// using MBN;
    /// using MBN.Modules;
    ///
    /// namespace Example
    /// {
    ///     public class Program
    ///     {
    ///         static BarGraph _bar;
    ///         
    ///         public static void Main()
    ///         {
    ///             // BarGraph Click board is plugged on socket #2 of the MikroBus.Net mainboard
    ///             _bar = new BarGraphClick(Hardware.SocketTwo);
    ///
    ///             _bar.Brightness = 0.2;
    ///             for (int j = 0; j &lt; 3; j++)
    ///             {
    ///                 for (UInt16 i = 0; i &lt;= 10; i++)
    ///                 {
    ///                     _bar.Bars(i, Fill);
    ///                     Thread.Sleep(50);
    ///                 }
    ///                 for (UInt16 i = 0; i &lt;= 10; i++)
    ///                 {
    ///                     _bar.Bars((UInt16)(10 - i), Fill);
    ///                     Thread.Sleep(50);
    ///                 }
    ///             }
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public class BarGraphClick : IDriver
    {
        private readonly SPI.Configuration _spiConfig;                // SPI configuration
        private Double _pwmLevel;                            // Brightness level
        private readonly PWM _pwm;                                    // Brightness control

        /// <summary>
        /// Initializes a new instance of the <see cref="BarGraphClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the BarGraph Click board is plugged on MikroBus.Net</param>
        /// <param name="initialBrightness">Initial brightness in the range 0.0 (no display) to 1.0 (full brightness)</param>
        /// <exception cref="System.InvalidOperationException">Thrown if some pins are already in use by another board on the same socket</exception>
        public BarGraphClick(Hardware.Socket socket, Double initialBrightness = 1.0)
        {
            // Checks if needed SPI pins are available
            Hardware.CheckPins(socket, socket.Miso, socket.Mosi, socket.Cs, socket.Sck, socket.Pwm, socket.Rst);

            // Initialize SPI
            _spiConfig = new SPI.Configuration(socket.Cs, false, 50, 0, false, true, 25000, socket.SpiModule);
            if (Hardware.SPIBus == null) { Hardware.SPIBus = new SPI(_spiConfig); }

            // Sets initial brightness
            _pwm = new PWM(socket.PwmChannel, 10000, initialBrightness, false);
            _pwm.Start();
            _pwmLevel = initialBrightness;
        }

        /// <summary>
        /// Gets or sets the brightness of the bargraph.
        /// </summary>
        /// <value>
        /// The brightness level, in the range 0.0 to 1.0
        /// </value>
        /// <example> This sample shows how to set the Brightness property.
        /// <code language="C#">
        ///             // BarGraph Click board is plugged on socket #2 of the MikroBus.Net mainboard
        ///             _bar = new BarGraphClick(Hardware.SocketTwo);
        ///
        ///             _bar.Brightness = 0.2;
        ///             _bar.Bars(5);
        /// </code>
        /// </example>
        public Double Brightness
        {
            get { return _pwmLevel; }
            set
            {
                _pwmLevel = (value > 1.0) || (value < 0.0) ? 1.0 : value;
                _pwm.DutyCycle = _pwmLevel;
                _pwm.Start();
            }
        }

        /// <summary>
        /// Displays the specified number of bars.
        /// </summary>
        /// <param name="nbBars">The number of bars to display (0 to 10).</param>
        /// <param name="fill">Boolean : true = fills preceding leds (default), false = light on the single led</param>
        /// <example> This sample shows how to call the Bars() method.
        /// <code language="C#">
        ///             // BarGraph Click board is plugged on socket #2 of the MikroBus.Net mainboard, with initial brightness set to half
        ///             _bar = new BarGraphClick(Hardware.SocketTwo { Brightness=0.5 });
        ///
        ///             _bar.Bars(5);           // Light on 5 bars
        /// 
        ///             Thread.Sleep(2000);     // Wait 2 sec
        /// 
        ///             _bar.Bars(6,false);     // Light on the 6th bar only
        /// </code>
        /// </example>
        public void Bars(UInt16 nbBars, Boolean fill = true)
        {
            Hardware.SPIBus.Write(_spiConfig, new [] { (UInt16)((1 << (fill ? nbBars : nbBars - 1)) - (fill ? 1 : 0)) });
        }

        /// <summary>
        /// Sends a bit mask to the chip.
        /// </summary>
        /// <param name="bars">10 bits mask to send.</param>
        /// <example> This sample shows how to call the SendMask() method.
        /// <code language="C#">
        ///         static BarGraph _bar;
        ///         static UInt16[] _fillMasks;
        ///         
        ///         public static void Main()
        ///         {
        ///             _fillMasks = new UInt16[10] { 512, 768, 896, 960, 992, 1008, 1016, 1020, 1022, 1023 };
        ///             // BarGraph Click board is plugged on socket #2 of the MikroBus.Net mainboard, with initial brightness set to half
        ///             _bar = new BarGraphClick(Hardware.SocketTwo { Brightness=0.5 });
        ///
        ///             for (int j = 0; j &lt; 3; j++)
        ///             {
        ///                 for (UInt16 i = 0; i &lt; 10; i++)
        ///                 {
        ///                     Leds.SendMask(_fillMasks[i]);
        ///                     Thread.Sleep(50);
        ///                 }
        ///                 for (UInt16 i = 0; i &lt; 10; i++)
        ///                 {
        ///                     Leds.SendMask(_fillMasks[9 - i]);
        ///                     Thread.Sleep(50);
        ///                 }
        ///             }
        ///         }
        /// </code>
        /// </example>
        public void SendMask(UInt16 bars)
        {
            Hardware.SPIBus.Write(_spiConfig, new [] { bars });
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
        /// <example> This sample shows how to use the DriverVersion property.
        /// <code language="C#">
        ///             Debug.Print ("Current driver version : "+_bar.DriverVersion);
        /// </code>
        /// </example>
        /// <value>
        /// The driver version.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        public Version DriverVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        /// <summary>
        /// Resets the module
        /// </summary>
        /// <param name="resetMode">The reset mode :
        /// <para>SOFT reset : generally by sending a software command to the chip</para><para>HARD reset : generally by activating a special chip's pin</para></param>
        /// <returns>True if Reset has been acknowledged, false otherwise.</returns>
        /// <exception cref="System.NotImplementedException">This module has no Reset feature.</exception>
        public bool Reset(ResetModes resetMode)
        {
            throw new NotImplementedException("Reset");
        }
    }
}
