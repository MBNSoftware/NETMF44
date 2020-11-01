/*
 * Pressure Click board driver
 * 
 *  Version 1.2 :
 *  - Added methods/properties to conform to new interfaces : IDriver and ITemperature
 *  
 *  Version 1.1 :
 *  - Use of I²C extension methods for thread safety
 *  - Added MikroE default I²C address/ClockRate in constructor
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

using System.Reflection;
using MBN.Enums;
using MBN.Exceptions;
using Microsoft.SPOT.Hardware;
using System;
using System.Threading;
using MBN.Extensions;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the Led8x8 R Click board driver
    /// <para><b>Pins used :</b> Scl, Sda</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///    static PressureClick _pres;
    ///    static Boolean _deviceOk;
    ///
    ///    public static void Main()
    ///    {
    ///        _deviceOk = false;
    ///        while (!_deviceOk)
    ///        {
    ///            try
    ///            {
    ///                _pres = new PressureClick(Hardware.SocketOne);
    ///                _deviceOk = true;
    ///            }
    ///            catch (DeviceInitialisationException)
    ///            {
    ///                Debug.Print("Init failed, retrying...");
    ///            }
    ///        }
    ///
    ///        while (true)
    ///        {
    ///            Debug.Print("Pression = " + _pres.ReadPressure() + " hPa");
    ///            Debug.Print("Temperature = " + _pres.ReadTemperature().ToString("F2") + "°");
    ///            Thread.Sleep(1000);
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class PressureClick : IDriver, ITemperature, IPressure
    {
        private readonly I2CDevice.Configuration _config;      // I²C configuration
        private Boolean _powered = true;
        private Byte _odr = 0x64;
        private TemperatureUnits _tempUnit = TemperatureUnits.Celsius;
        private Int16 _tempRawData;

        /// <summary>
        /// Initializes a new instance of the <see cref="PressureClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the Pressure Click board is plugged on MikroBus.Net</param>
        /// <param name="address">Address of the I²C device.</param>
        /// <param name="clockRateKHz">Clock rate of the Pressure Click board.</param>
        /// <exception cref="System.InvalidOperationException">Thrown if some pins are already in use by another board on the same socket</exception>
        public PressureClick(Hardware.Socket socket, Byte address = (0xBA >> 1), ClockRatesI2C clockRateKHz=ClockRatesI2C.Clock100KHz)
        {
            // Checks if needed I²C pins are available
            Hardware.CheckPinsI2C(socket);

            // Create the driver's I²C configuration
            _config = new I2CDevice.Configuration(address, (Int32)clockRateKHz);
            if (!Init())
            {
                throw new DeviceInitialisationException("Device failed to initialize");
            }
        }

        #region Private methods
        private Byte ReadByte(Byte register)
        {
            var result = new Byte[1];

            var actions = new I2CDevice.I2CTransaction[2];
            actions[0] = I2CDevice.CreateWriteTransaction(new [] { register });
            actions[1] = I2CDevice.CreateReadTransaction(result);
            Hardware.I2CBus.Execute(_config, actions, 1000);

            return result[0];
        }

        private Int32 ReadInteger(Byte register)
        {
            var low = ReadByte(register);
            var high = ReadByte((Byte)(register + 1));

            return (high << 8) + low;
        }

        private void SetRegister(Byte register, Byte value)
        {
            var actions = new I2CDevice.I2CTransaction[1];
            actions[0] = I2CDevice.CreateWriteTransaction(new [] { register, value });
            Hardware.I2CBus.Execute(_config, actions, 1000);
            Thread.Sleep(20);
        }

        private void SetRegisterBit(Byte register, Byte bitIndex, Boolean state)
        {
            var actions = new I2CDevice.I2CTransaction[1];
            actions[0] = I2CDevice.CreateWriteTransaction(new [] { register, Bits.Set(ReadByte(register), bitIndex, state) });
            Hardware.I2CBus.Execute(_config, actions, 1000);
            Thread.Sleep(20);
        }

        private Boolean Init()
        {
            Byte err = 0;
            SetRegister(Registers.RES_CONF, 0x78);
            if (ReadByte(Registers.RES_CONF) != 0x78) { err++; }
            SetRegister(Registers.CTRL_REG1, 0x64);
            if (ReadByte(Registers.CTRL_REG1) != 0x64) { err++; }
            SetRegister(Registers.CTRL_REG1, 0xE4);
            if (ReadByte(Registers.CTRL_REG1) != 0xE4) { err++; }

            return err == 0;
        }
        #endregion

        /// <summary>
        /// Gets the device identifier.
        /// </summary>
        /// <value>
        /// The device identifier.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///      Debug.Print ("Pressure sensor : "+_pres.DeviceID);
        /// </code>
        /// </example>
        public String DeviceID
        {
            get { return ReadByte(Registers.WHO_AM_I) == 0xBB ? "LPS331AP" : "Unknown"; }
        }

        /// <summary>
        /// Gets or sets the output data rate.
        /// </summary>
        /// <remarks>See the datasheet as this parameter sets both the temperature and pressure data rate at different frequencies.</remarks>
        /// <value>
        /// The output data rate.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///     // Sets the pressure datarate to 7Hz and temperature to 1Hz
        ///      _pres.OutputDataRate = 2;
        /// </code>
        /// </example>
        public Byte OutputDataRate
        {
            get { return _odr; }
            set
            {
                var valTmp = value > 7 ? (Byte)0 : value;
                _odr = valTmp;
                if ((valTmp == 7) & (ReadByte(Registers.RES_CONF) == 0x7A))         // 0x7A Config not allowed with ODR = 25Hz/25Hz
                {
                    SetRegister(Registers.RES_CONF, 0x64);                          // For ODR 25Hz/25Hz, the suggested configuration for RES_CONF is 6Ah.
                }
                var ctrlReg1 = ReadByte(Registers.CTRL_REG1);
                SetRegister(Registers.CTRL_REG1, (Byte)(ctrlReg1 & (0xFF & (valTmp << 4))));
            }
        }

        /// <summary>
        /// Reads the temperature from the sensor.
        /// </summary>
        /// <param name="source">The source for the measurement.<see cref="TemperatureSources"/></param>
        /// <returns>
        /// A single representing the temperature read from the source, degrees Celsius
        /// </returns>
        /// <example>
        /// <code language="C#">
        ///     // Reads the ambient temperature
        ///     Debug.Print ("Ambient temperature = "+_pres.ReadTemperature());
        /// </code>
        /// </example>
        public float ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            var rawValue = (Int16)ReadInteger(Registers.TEMP_OUT_L);
            _tempRawData = rawValue;
            float tempCelsius;

            if ((rawValue & 0x8000) == 0x8000)          // Sign bit is 1, so use two's complement and negate the result
            {
                tempCelsius = (float)(((-1 * ~rawValue + 1) / 480.0) + 42.5);
            }
            else
            {
                tempCelsius = (float)((rawValue / 480.0) + 42.5);
            }
            return _tempUnit == TemperatureUnits.Celsius ? tempCelsius : _tempUnit == TemperatureUnits.Fahrenheit ? (Single)((tempCelsius * (9.0 / 5)) + 32) : (Single)(tempCelsius + 273.15);
        }

        /// <summary>
        /// Gets the raw data of the temperature value.
        /// </summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <example>
        /// <code language="C#">
        ///      // Reads the temperature raw data
        ///      Debug.Print("Temperature raw data = " + (_pres as ITemperature).RawData);
        /// </code>
        /// </example>
        int ITemperature.RawData
        {
            get { return _tempRawData; }
        }

        /// <summary>
        /// Gets or sets the temperature unit.
        /// </summary>
        /// <value>
        /// The temperature unit from <see cref="TemperatureUnits"/>
        /// </value>
        /// <example>
        /// <code language="C#">
        ///      // Sets the temperature unit to Kelvin
        ///      _pres.TemperatureUnit = TemperatureUnits.Kelvin;
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
        /// <exception cref="System.NotImplementedException">Thrown when trying to use PowerModes.Low.</exception>
        /// <example>
        /// <code language="C#">
        ///      // Power off the module
        ///      _pres.PowerMode = PowerModes.Off;
        /// </code>
        /// </example>
        public PowerModes PowerMode
        {
            get { return _powered ? PowerModes.On : PowerModes.Off; }
            set
            {
                if (value == PowerModes.Low) { throw new NotImplementedException("PowerModes.Low"); }
                SetRegisterBit(Registers.CTRL_REG1, 7, value == PowerModes.On);
                _powered = value == PowerModes.On;
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
        ///      Debug.Print ("Current driver version : "+_pres.DriverVersion);
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
        /// Reads the pressure from the sensor.
        /// </summary>
        /// <param name="compensationMode">Indicates if the pressure reading returned by the sensor is see-level compensated or not.</param>
        /// <returns>
        /// A single representing the pressure read from the source, in hPa (hectoPascal)
        /// </returns>
        /// <example>
        /// <code language="C#">
        ///     // Reads the actual pressure, in SeaLevelCompensated mode (default)
        ///      Debug.Print ("Current pressure = "+_pres.ReadPressure()+" hPa");
        /// </code>
        /// </example>
        public Single ReadPressure(PressureCompensationModes compensationMode = PressureCompensationModes.SeaLevelCompensated)
        {
            Int32 high = ReadByte(Registers.PRESS_OUT_H);
            var mid = ReadByte(Registers.PRESS_OUT_L);
            var low = ReadByte(Registers.PRESS_OUT_XL);

            high <<= 8;
            high |= mid;
            high <<= 8;
            high |= low;

            return high >> 12;
        }

        /// <summary>
        /// Gets the raw data of the pressure value.
        /// </summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <exception cref="NotImplementedException">The module does not provide raw data for pressure.</exception>
        int IPressure.RawData
        {
            get { throw new NotImplementedException("IPressure.RawData"); }
        }
    }
}
