/*
 * IRThermo Click board driver
 * 
 * Version 1.0 :
 *  - Initial revision coded by Christophe Gerbier
 * 
 * References needed :
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Native
 *  MikroBus.Net
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
using MBN.Extensions;
using Microsoft.SPOT.Hardware;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the IRThermoClick driver
    /// <para><b>Pins used :</b> Scl, Sda</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    ///		public class Program
    ///    	{
    ///        private static IRThermoClick _ir;
    ///        
    ///        public static void Main()
    ///        {
    ///            _ir = new IRThermoClick(Hardware.SocketThree);
    ///
    ///            Debug.Print("Ambient temperature : " + _ir.ReadTemperature().ToString("F2"));
    ///            Debug.Print("Object temperature : " + _ir.ReadTemperature(TemperatureSources.Object).ToString("F2"));
    ///
    ///            Thread.Sleep(Timeout.Infinite);
    ///        }
    ///		}
    /// </code>
    /// </example>
    public class IRThermoClick : IDriver, ITemperature
    {
        private readonly I2CDevice.Configuration _config;                      // I²C configuration
        private TemperatureUnits _tempUnit = TemperatureUnits.Celsius;

        /// <summary>
        /// Initializes a new instance of the <see cref="IRThermoClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the IRThermo Click board is plugged on MikroBus.Net board</param>
        /// <param name="address">The address of the display. Default to 0x5A.</param>
        /// <param name="clockRateKHz">The clock rate of the I²C device. Default to ClockRatesI2C.Clock100KHz. <seealso cref="ClockRatesI2C"/></param>
        /// <exception cref="MBN.Exceptions.PinInUseException">Thrown if a pin is already in use by another driver.</exception>
        public IRThermoClick(Hardware.Socket socket, Byte address=0x5A, ClockRatesI2C clockRateKHz=ClockRatesI2C.Clock100KHz)
        {
            try
            {
                Hardware.CheckPinsI2C(socket, socket.Scl, socket.Sda);

                _config = new I2CDevice.Configuration(address, (Int32)clockRateKHz);

            }
            // Catch only the PinInUse exception, so that program will halt on other exceptions
            // Send it directly to caller
            catch (PinInUseException) { throw new PinInUseException(); }
        }

        /// <summary>
        /// Reads the temperature from the sensor.
        /// </summary>
        /// <param name="source">The source of the measurement : either "ambient" temperature or "object" temperature.</param>
        /// <returns>
        /// A single representing the temperature read from the source.
        /// </returns>
        /// <example>
        /// <code language="C#">
        ///     // Reads ambient temperature
        ///     Debug.Print ("Ambient temperature = "+_ir.ReadTemperature());
        /// 
        ///     // Reads object temperature
        ///     Debug.Print ("Object temperature = "+_ir.ReadTemperature(TemperatureSources.Object));
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            var result = new byte[3];

            var actions = new I2CDevice.I2CTransaction[2];
            actions[0] = I2CDevice.CreateWriteTransaction(new [] { source == TemperatureSources.Ambient ? (Byte)0x06 : (Byte)0x07 });
            actions[1] = I2CDevice.CreateReadTransaction(result);
            Hardware.I2CBus.Execute(_config, actions, 1000);

            RawData = (result[1] << 8) + result[0];
            var tempCelsius = (Single)((RawData*0.02) - 273.15);
            return _tempUnit == TemperatureUnits.Celsius ? tempCelsius : _tempUnit == TemperatureUnits.Fahrenheit ? (Single)((tempCelsius * (9.0 / 5)) + 32) : (Single)(tempCelsius + 273.15);
        }

        /// <summary>
        /// Gets or sets the temperature unit for the <seealso cref="ReadTemperature"/> method.
        /// <remarks><seealso cref="TemperatureUnits"/></remarks>
        /// </summary>
        /// <value>
        /// The temperature unit used.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///     // Set temperature unit to Fahrenheit
        ///     _ir.TemperatureUnit = TemperatureUnits.Farhenheit;
        /// </code>
        /// </example>
        public TemperatureUnits TemperatureUnit
        {
            get { return _tempUnit; }
            set { _tempUnit = value; }
        }

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">Thrown because this module has no power modes features.</exception>
        public PowerModes PowerMode
        {
            get { return PowerModes.On; }
            set { throw new NotImplementedException("PowerMode"); }
        }

        /// <summary>
        /// Gets the driver version.
        /// </summary>
        /// <value>
        /// The driver version.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///      Debug.Print ("Current driver version : "+_ir.DriverVersion);
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
        public Boolean Reset(ResetModes resetMode)
        {
            throw new NotImplementedException("Reset");
        }

        /// <summary>
        /// Gets the raw data associated with the temperature read.
        /// </summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example). 
        /// It's a pure number that has no physical meaning by itself. Please use <see cref="ReadTemperature"/> for readings with known units.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///      Debug.Print ("Raw data read from sensor : "+_ir.RawData);
        /// </code>
        /// </example>
        public int RawData { get ; private set; }
    }
}

