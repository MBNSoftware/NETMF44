/* 
 *  Digipot Click board driver
 * 
 * Version 2.0 :
 *  - Conformance to the new namespaces and organization
 *  
 *  Version 1.1 :
 *  - Use of SPI extension methods for thread safety
 *  
 * Version 1.0 :
 *  - Initial revision coded by Niels Jakob Buch
 *  - Thanks to Christophe Gerbier for valuable help
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
 * Click board driver skeleton generated on 3/2/2014 2:33:52 PM
 *  
 * Datasheet to read here: http://ww1.microchip.com/downloads/en/DeviceDoc/22059b.pdf
 * Click board user guide: http://www.mikroe.com/downloads/get/1716/digipot_click_manual_v100.pdf
 * 
 * 
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
    /// Main class for the MikroE Digipot Click board driver
    /// <para><b>Pins used :</b> Miso, Mosi, Sck, Cs.</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///     static DigiPotClick _dp;
    ///
    ///     public static void Main()
    ///     {
    ///        _dp = new DigiPotClick(Hardware.SocketOne, 200);
    ///
    ///        Debug.Print("Pos1");
    ///        _dp.Resistance = 50;
    ///
    ///        Thread.Sleep(1000);
    ///        Debug.Print("Pos2");
    ///        _dp.Resistance = 100;
    /// 
    ///         Thread.Sleep (Timeout.Infinite);
	///	    }
    /// }
    /// </code>
    /// </example>
    public class DigiPotClick : IDriver
    {
        private readonly SPI.Configuration _spiConfig;                // SPI configuration
        private byte _currentResistance;                     // Resistance

        /// <summary>
        /// Main class for the MikroE Digipot Click board driver
        /// </summary>
        /// <param name="socket">The socket on which the Digipot Click board is plugged on MikroBus.Net</param>
        /// <param name="initialResistance">The initial resistance the Digipot should be initialized with.</param>
        /// <exception cref="System.InvalidOperationException">Thrown if some pins are already in use by another board on the same socket</exception>
        public DigiPotClick(Hardware.Socket socket, byte initialResistance)
        {
            try
            {
                // Checks if needed SPI pins are available
                Hardware.CheckPins(socket, socket.Cs, socket.Sck, socket.Mosi, socket.Miso);

                // Initialize SPI
                _spiConfig = new SPI.Configuration(
	                                  socket.Cs,        //   Cpu.Pin ChipSelect_Port,
	                                  false,            //   bool ChipSelect_ActiveState,
	                                  10,               //   uint ChipSelect_SetupTime,
	                                  20,               //   uint ChipSelect_HoldTime,
	                                  true,             //   bool Clock_IdleState,
	                                  false,            //   bool Clock_Edge,
	                                  10000,            //   uint Clock_RateKHz,
	                                  socket.SpiModule);
                if (Hardware.SPIBus == null) { Hardware.SPIBus = new SPI(_spiConfig); }

                _currentResistance = initialResistance;
                
            }
            // Catch only the PinInUse exception, so that program will halt on other exceptions
            // Send it directly to caller
            catch (PinInUseException) { throw new PinInUseException(); }
        }


        /// <summary>
        /// Gets or sets the resistance of the Digipot click.
        /// </summary>
        /// <value>
        /// The resistance between 0 and 255.
        /// 255 should be close to 8KOHM between the PA and PW pins.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///        _dp.Resistance = 50;
        /// </code>
        /// </example>
        public byte Resistance
        {
            get { return _currentResistance; }
            set
            {   
                value = (value > (byte)255) ? (byte)128 : value;
                _currentResistance = value;
                // Write the value to the chip using SPI
                Hardware.SPIBus.Write(_spiConfig, new [] { (byte)0x00, value, (byte)0x20, value });
#if DEBUG
                Debug.Print("Digipot: Current resistance:" + _currentResistance);
#endif                
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
        ///      Debug.Print ("Current driver version : "+_digipot.DriverVersion);
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

