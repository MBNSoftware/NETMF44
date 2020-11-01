/*
 * Altitude Click board driver
 * 
 * Version 1.0 :
 *  - Initial revision coded by Stephen Cardinale
 *  - Not Implemented - Polling with Interrupt, FIFO Mode.
 *                    - Altitude, Pressure and Temperature Offsets. Using calculated Sea Level compensation instead.
 * Version 2.0 :
 *  - Implementation of the MikroBusNet Interface and new Namespaces requirements.
 *  - Cleaned up code and Refactoring.
 * 
 * References needed :
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Native
 *  MikroBus.Net
 *  mscorlib
 *  
 * Copyright 2014 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http:///www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using MBN.Enums;
using MBN.Exceptions;
using MBN.Extensions;
using Microsoft.SPOT.Hardware;
using System;
using System.Reflection;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace MBN.Modules
{
    /// <summary>
    /// a new instance of the <see cref="AltitudeClick"/> class.
    /// <para><b>This module is an I2C Device</b></para>
    /// <para><b>Pins used :</b> Sda, Scl</para>
    /// <para><b>References required:</b> MikroBus.Net, Microsoft.SPOT.Native, Microsoft.Spot.Hardware, mscorlib</para>
    /// </summary>
    /// <example>Example usage:
    /// <code language = "C#">
    ///  using MBN;
    ///  using MBN.Enums;
    ///  using MBN.Exceptions;
    ///  using MBN.Modules;
    ///  using System;
    ///  using Microsoft.SPOT;
    ///  using System.Threading;
    ///
    ///  namespace Examples
    ///  {
    ///     public class Program
    ///     {
    ///         private static AltitudeClick _altitude;
    ///         private static Boolean _deviceOk;
    ///
    ///         public static void Main()
    ///         {
    ///             var counter = 1;
    ///             _deviceOk = false;
    ///
    ///             while (!_deviceOk)
    ///             {
    ///                 try
    ///                 {
    ///                     _altitude = new AltitudeClick(Hardware.SocketOne, ClockRatesI2C.Clock100KHz) {OverSampling = Oss.Oss7, TemperatureUnit = TemperatureUnits.Fahrenheit};
    ///                     _altitude.MeasurementComplete += AltitudeAltitudeClickMeasurementComplete;
    ///                     Debug.Print("Initialization at " + " try number " + counter);
    ///                     _deviceOk = true;
    ///                 }
    ///                 catch (DeviceInitialisationException)
    ///                 {
    ///                     Debug.Print("Initialization failed, retrying...");
    ///                     if (counter <![CDATA[>]]>= 10) throw new DeviceInitialisationException("Altitude Click failed to initialize. Please check your Hardware.");
    ///                     counter++;
    ///                 }
    ///             }
    ///
    ///             while (true)
    ///             {
    ///                 _altitude.ReadSensor();
    ///
    ///                 /* Uncomment the following to use non-event driven methods for obtaining sensor data. */
    ///                 //var rawAltitude = _altitude.ReadAltitude(PressureCompensationModes.Uncompensated)
    ///                 //var compensatedAltitude = _altitude.ReadPressure(PressureCompensationModes.SeaLevelCompensated)
    ///                 //var rawPressure = _altitude.ReadPressure(PressureCompensationModes.Uncompensated)
    ///                 ///var compensatedPressure = _altitude.ReadPressure(PressureCompensationModes.SeaLevelCompensated)
    ///                 ///var temp = _altitude.ReadTemperature(TemperatureSources.Ambient)
    /// 
    ///                 //Debug.Print("Raw Altitude - " + rawAltitude + "  meters");
    ///                 //Debug.Print("Sea Level Compensated Altitude - " + compensatedAltitude + "  meters");
    ///                 //Debug.Print("Raw Pressure - " + rawPressure + " Pa");
    ///                 //Debug.Print("Seal Level Compensated Pressure - " + compensatedPressure + " Pa");
    ///                 //Debug.Print("Temperature - " + temp + (_altitude.TemperatureUnit == TemperatureUnits.Celsius ? "°C" : _altitude.TemperatureUnit == TemperatureUnits.Fahrenheit ? " °F" : "°K"));
    ///
    ///                 Thread.Sleep(5000);
    ///             }
    ///         }
    ///
    ///         static void AltitudeAltitudeClickMeasurementComplete(object sender, MBN.Events.MeasurementCompleteEventArgs e)
    ///         {
    ///             Debug.Print("Uncompensated Altitude - " + e.RawAltitude + "  meters");
    ///             Debug.Print("Sea Level Compensated Altitude - " + e.CompensatedAltitude + "  meters");
    ///             Debug.Print("Uncompensated Pressure - " + e.RawPressure + " Pa");
    ///             Debug.Print("Sea Level Compensated Pressure - " + e.CompensatedPressure + " Pa");
    ///             Debug.Print("Temperature - " + e.Temperature + (_altitude.TemperatureUnit == TemperatureUnits.Celsius ? "°C" : _altitude.TemperatureUnit == TemperatureUnits.Fahrenheit ? " °F" : "°K"));
    ///         }
    ///     }
    /// }
    /// </code>
    /// <code language = "VB">
    /// Option Explicit On
    /// Option Strict On
    ///
    /// Imports MBN
    /// Imports MBN.Enums
    /// Imports MBN.Exceptions
    /// Imports MBN.Modules
    /// Imports Microsoft.SPOT
    /// Imports System.Threading
    ///
    /// Namespace Examples
    ///
    ///     Public Module Module1
    ///
    ///         Private _altitude As AltitudeClick
    ///         Private _deviceOk As Boolean
    ///
    ///
    ///         Sub Main()
    ///             Dim counter = 1
    ///             _deviceOk = False
    ///
    ///             While Not _deviceOk
    ///                 Try
    ///                     _altitude = New AltitudeClick(Hardware.SocketOne, ClockRatesI2C.Clock100KHz)
    ///                     _altitude.OverSampling = Oss.Oss7
    ///                     _altitude.TemperatureUnit = TemperatureUnits.Fahrenheit
    ///                     AddHandler _altitude.MeasurementComplete, AddressOf AltitudeAltitudeClickMeasurementComplete
    ///                     Debug.Print("Initialization at " <![CDATA[&]]> " try number " <![CDATA[&]]> counter)
    ///                     _deviceOk = True
    ///                 Catch generatedExceptionName As DeviceInitialisationException
    ///                     Debug.Print("Initialization failed, retrying...")
    ///                     If counter <![CDATA[>]]>= 10 Then
    ///                         Throw New DeviceInitialisationException("Altitude Click failed to initialize. Please check your Hardware.")
    ///                     End If
    ///                     counter += 1
    ///                 End Try
    ///             End While
    ///
    ///             While True
    ///                 _altitude.ReadSensor()
    ///
    ///                 ' Uncomment the following to use non-event driven methods for obtaining sensor data. 
    ///                 'Dim rawAltitude = _altitude.ReadAltitude(PressureCompensationModes.Uncompensated)
    ///                 'Dim compensatedAltitude = _altitude.ReadPressure(PressureCompensationModes.SeaLevelCompensated)
    ///                 'Dim rawPressure = _altitude.ReadPressure(PressureCompensationModes.Uncompensated)
    ///                 'Dim compensatedPressure = _altitude.ReadPressure(PressureCompensationModes.SeaLevelCompensated)
    ///                 'Dim temp = _altitude.ReadTemperature(TemperatureSources.Ambient)
    ///
    ///                 'Debug.Print("Raw Altitude - " <![CDATA[&]]> rawAltitude <![CDATA[&]]> "  meters")
    ///                 'Debug.Print("Sea Level Compensated Altitude - " <![CDATA[&]]> compensatedAltitude <![CDATA[&]]> "  meters")
    ///                 'Debug.Print("Raw Pressure - " <![CDATA[&]]> rawPressure <![CDATA[&]]> " Pa")
    ///                 'Debug.Print("Seal Level Compensated Pressure - " <![CDATA[&]]> compensatedPressure <![CDATA[&]]> " Pa")
    ///                 'Debug.Print("Temperature - " <![CDATA[&]]> temp.ToString() <![CDATA[&]]> (If(_altitude.TemperatureUnit = TemperatureUnits.Celsius, "°F", If(_altitude.TemperatureUnit = TemperatureUnits.Fahrenheit, " °F", "°K"))) <![CDATA[&]]> Microsoft.VisualBasic.Constats.vbCrLf)
    ///
    ///                 Thread.Sleep(5000)
    ///             End While
    ///         End Sub
    ///
    ///         Private Sub AltitudeAltitudeClickMeasurementComplete(sender As Object, e As MBN.Events.MeasurementCompleteEventArgs)
    ///             Debug.Print("Uncompensated Altitude - " <![CDATA[&]]> e.RawAltitude <![CDATA[&]]> "  meters")
    ///             Debug.Print("Sea Level Compensated Altitude - " <![CDATA[&]]> e.CompensatedAltitude <![CDATA[&]]> "  meters")
    ///             Debug.Print("Uncompensated Pressure - " <![CDATA[&]]> e.RawPressure <![CDATA[&]]> " Pa")
    ///             Debug.Print("Sea Level Compensated Pressure - " <![CDATA[&]]> e.CompensatedPressure <![CDATA[&]]> " Pa")
    ///             Debug.Print("Temperature - " <![CDATA[&]]> e.Temperature.ToString() <![CDATA[&]]> (If(_altitude.TemperatureUnit = TemperatureUnits.Celsius, "°F", If(_altitude.TemperatureUnit = TemperatureUnits.Fahrenheit, " °F", "°K"))) <![CDATA[&]]> Microsoft.VisualBasic.Constats.vbCrLf)
    ///         End Sub
    ///
    ///     End Module
    ///
    /// End Namespace
    /// </code>
    /// </example>
    public class AltitudeClick : IDriver, IPressure, ITemperature
    {
        #region Constants
        
        // ReSharper disable InconsistentNaming
        private const Byte STATUSREG = 0x00;
        private const Byte CTRL_REG1 = 0x26;
        private const Byte CTRL_REG4 = 0x29;
        private const Byte PT_DATA_CFG = 0x13;
        //private const Byte BAR_IN_MSB = 0x14;
        //private const Byte BAR_IN_LSB = 0x15;
        private const Byte WHO_AM_I = 0x0C;
        //private const Byte DRDY = 0x12;
        private const Byte OUT_P_MSB = 0x01;
        private const Byte OUT_P_CSB = 0x02;
        private const Byte OUT_P_LSB = 0x03;
        private const Byte OUT_T_MSB = 0x04;
        private const Byte OUT_T_LSB = 0x05;
        // ReSharper restore InconsistentNaming 

        #endregion

        #region Fields

        private static I2CDevice.Configuration _i2CConfig; /// I²C configuration
        private const byte I2CAddress = 0xC0 >> 1;
        private static int _i2CTimeout;

        private const int MaxDataReadyAttempts = 512; /// How many times we loop waiting for data before giving up.

        private static Oss _oss = Oss.Oss0;
        private static TemperatureUnits _temperatureUnit = TemperatureUnits.Celsius;

        #endregion

        #region Enum
        
        /// <summary>
        ///     OverSampling Rate from least accurate and fastest response time (OSS0) to most accurate and slowest response time (OSS7) 
        /// </summary>
        public enum Oss : byte
        {
            /// <summary>
            /// 6 ms Time Between Data Samples - 1 sample
            /// </summary>
            Oss0 = 0,
            /// <summary>
            /// 10 ms Time Between Data Samples - 2 samples
            /// </summary>
            Oss1 = 1,
            /// <summary>
            /// 18 ms Time Between Data Samples - 4 samples
            /// </summary>
            Oss2 = 2,
            /// <summary>
            /// 34 ms Time Between Data Samples - 8 samples
            /// </summary>
            Oss3 = 3,
            /// <summary>
            /// 66 ms Time Between Data Samples - 16 samples
            /// </summary>
            Oss4 = 4,
            /// <summary>
            /// 130 ms Time Between Data Samples - 32 samples
            /// </summary>
            Oss5 = 5,
            /// <summary>
            /// 258 ms Time Between Data Samples - 64 samples
            /// </summary>
            Oss6 = 6,
            /// <summary>
            /// 512 ms Time Between Data Samples - 128 samples
            /// </summary>
            Oss7 = 7
        }

        #endregion

        #region CTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="AltitudeClick"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="Hardware.Socket"/> that the AltitudeClick is inserted into.</param>
        /// <param name="clockRateI2">The <see cref="MBN.Enums.ClockRatesI2C"/> for I2C communication.</param>
        /// <param name="i2CTimeout">The universal I2C Transaction Timeout value to wait for I2C transactions to complete.</param>
        /// <exception cref="DeviceInitialisationException">A DeviceInitialisationException will be thrown if the AltitudeClick does not complete its initialization properly.</exception>
        /// <exception cref="PinInUseException">A PinInUseException will be thrown if the I2C pins are being used for non-I2C function.</exception>
        public AltitudeClick(Hardware.Socket socket, ClockRatesI2C clockRateI2, int i2CTimeout = 1000)
        {
            try
            {
                // Checks if needed I²C pins are available
                Hardware.CheckPinsI2C(socket);

                // Create the driver's I²C configuration
                _i2CConfig = new I2CDevice.Configuration(I2CAddress, (int)clockRateI2);

                _i2CTimeout = i2CTimeout;

                // Check if it's the first time an I²C device is created
                if (Hardware.I2CBus == null)
                {
                    Hardware.I2CBus = new I2CDevice(_i2CConfig);
                }

                if (!Init())
                {
                    throw new DeviceInitialisationException("The AltitudeClick has failed to initialize");
                }
            }
            // Catch only the PinInUse exception, so that program will halt on other exceptions and send it directly to caller.
            catch (PinInUseException ex)
            {
                throw new PinInUseException(ex.Message);
            }
        }
 
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the driver version.
        /// </summary>
        /// <value>
        /// The driver version see <see cref="Version"/>.
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
        /// Gets or Sets the <see cref="Oss"/>. Datasheet calls for 128 but you can set it 
        /// from 1 to 128 samples. The higher the oversample rate the greater the accuracy and
        /// the longer time for conversion.
        /// <example>Example usage:
        /// <code language = "C#">
        /// _altitude.OverSampling = Oss.Oss7;
        /// </code>
        /// <code language = "VB">
        /// _altitude.OverSampling = Oss.Oss7
        /// </code>
        /// </example>
        /// </summary>
        public Oss OverSampling
        {
            get { return _oss; }
            set
            {
                _oss = value;
                SetOversampleRate((byte)value);
            }
        }

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <remarks>If the module has no power modes, then GET should always return PowerModes.ON while SET should throw a NotImplementedException.</remarks>
        /// <exception cref="NotImplementedException">This module has no power mode setting. If using the set accessor, it will throw an exception.</exception>
        /// <example>Example usage: None.</example>
        public PowerModes PowerMode
        {
            get
            {
                return PowerModes.On;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary> 
        /// Gets the raw data of the sensor. 
        /// </summary> 
        /// <value> 
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example) 
        /// </value>
        /// <exception cref="NotImplementedException">This module does not implement RawData, calling this method will throw a NotImplementedException</exception>.
        /// <example>Example usage: None.</example>
        public int RawData
        {
            get { throw new NotImplementedException(); }
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
        /// // Set temperature unit to Fahrenheit
        /// _altitude.TemperatureUnit = TemperatureUnits.Farhenheit;
        /// </code>
        /// <code language="VB">
        /// ' Set temperature unit to Fahrenheit
        /// _altitude.TemperatureUnit = TemperatureUnits.Farhenheit
        /// </code>
        /// </example>
        public TemperatureUnits TemperatureUnit
        {
            get { return _temperatureUnit; }
            set { _temperatureUnit = value; }
        }

        /// <summary>
        ///     Returns a <see cref="System.String"/> of "AltitudeClick or Unknown" if the AltitudeClick is found by reading register 0xC0 and compares the vale read to 0xC4 (manufacturer specific ID). 
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("Who am I" + _altitude.WhoAmI);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Who am I" <![CDATA[&]]> _altitude.WhoAmI)
        /// </code>
        /// </example>
        public String WhoAmI
        {
            get { return ReadByte(WHO_AM_I) == 0xC4 ? "AltitudeClick" : "Unknown"; }
        }

        #endregion

        #region Private Methods

        private static bool Init()
        {
            var err = 0;

            SetRegister(CTRL_REG1, 0xB9); // Enable sensor, oversampling, altimeter mode 
            if (ReadByte(CTRL_REG1) != 0xB9) { err++; }

            SetRegister(CTRL_REG4, 0x80); // Data ready interrupt enabled
            if (ReadByte(CTRL_REG4) != 0x80) { err++; }

            SetRegister(PT_DATA_CFG, 0x07); // Enable both pressure and temp event flags 
            if (ReadByte(PT_DATA_CFG) != 0x07) { err++; }

            return err == 0;
        }

        private static void SetOversampleRate(byte sampleRate)
        {
            if (sampleRate > 7) sampleRate = 7; //OS cannot be larger than 0b.0111
            sampleRate <<= 3; // Align it for the CtrlReg1 register
            var tempSetting = ReadByte(CTRL_REG1); //Read current settings
            tempSetting &= 0xC7; // Clear out old OS bits
            tempSetting |= sampleRate; // Mask in new OS bits
            SetRegister(CTRL_REG1, tempSetting);
        }

        private static void ToggleOneShot()
        {
            ClearRegisterBit(CTRL_REG1, 0x02);  // Clear OST bit
            SetRegisterBit(CTRL_REG1, 0x02);    // Set the OST bit.

        }

        private static void SetModeStandby()
        {
            ClearRegisterBit(CTRL_REG1, 0x01);  // Clear SBYB bit for Standby mode
        }

        private static void SetModeActive()
        {
            SetRegisterBit(CTRL_REG1, 0x01);    // Set SBYB bit for Active mode
        }

        private static bool PressureDataReady()
        {
            return DataAvailable(0x04);
        }

        private static bool TemperatureDataReady()
        {
            return DataAvailable(0x02);
        }

        private static bool DataAvailable(byte mask)
        {
            var attempts = 0;
            while ((ReadByte(STATUSREG) & mask) == 0)
            {
                attempts++;
                if (attempts > MaxDataReadyAttempts)
                    return false; // Failed
                Thread.Sleep(1);
            }
            return true; // Success
        }

        private static void SetModeBarometer()
        {
            ClearRegisterBit(CTRL_REG1, 0x80);  // Clear ALT bit
        }

        private static void SetModeAltimeter()
        {
            SetRegisterBit(CTRL_REG1, 0x80);    // Set ALT bit
        }

        private static double CalculatePressureAsl(double pressure)
        {
            var seaLevelCompensation = 101325 * Math.Pow(((288 - 0.0065 * 143) / 288), 5.256);
            return 101325 + pressure - seaLevelCompensation;
        }

        private static byte ReadByte(Byte register)
        {
            var result = new Byte[1];

            var actions = new I2CDevice.I2CTransaction[2];
            actions[0] = I2CDevice.CreateWriteTransaction(new[] { register });
            actions[1] = I2CDevice.CreateReadTransaction(result);
            Hardware.I2CBus.Execute(_i2CConfig, actions, _i2CTimeout);

            return result[0];
        }

        private static void SetRegister(Byte register, Byte value)
        {
            var actions = new I2CDevice.I2CTransaction[1];
            actions[0] = I2CDevice.CreateWriteTransaction(new[] { register, value });
            Hardware.I2CBus.Execute(_i2CConfig, actions, _i2CTimeout);
        }

        private static void ClearRegisterBit(byte register, byte bitMask)
        {
            int temp = ReadByte(register);      // Read the current register value
            temp &= ~bitMask;                   // Clear the bit from the value
            SetRegister(register, (byte)temp);  // Write register value back
        }

        private static void SetRegisterBit(byte register, byte bitMask)
        {
            var temp = ReadByte(register);  // Read the current register value
            temp |= bitMask;                // Set the bit in the value
            SetRegister(register, temp);    // Write register value back
        }

        #endregion

        #region Public Methods
     
        /// <summary>
        /// This function reads the altitude and temperature  then returns the sensor data by raising the <see cref="MeasurementComplete"/> event. 
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// var rawAltitude = _altitude.ReadAltitude(PressureCompensationModes.Uncompensated);
        /// var compensatedAltitude = _altitude.ReadPressure(PressureCompensationModes.SeaLevelCompensated);
        /// Debug.Print("Raw Altitude - " + rawAltitude + "  meters");
        /// Debug.Print("Sea Level Compensated Altitude - " + compensatedAltitude + "  meters");
        /// </code>
        /// <code language = "VB">
        /// _altitude.OverSampling = Oss.Oss7
        /// </code>
        /// </example>
        public void ReadSensor()
        {
            var uA = ReadAltitude(PressureCompensationModes.Uncompensated);
            var cA = ReadAltitude();
            var uP = ReadPressure(PressureCompensationModes.Uncompensated);
            var cP = ReadPressure();
            var t = Utilities.Temperature.ConvertTo(TemperatureUnits.Celsius, _temperatureUnit, ReadTemperature());

            _measurementComplete(this, new MeasurementCompleteEventArgs(uA, cA, uP, cP, t));
        }

        /// <summary>
        /// Gets the Uncompensated or Sea Level Compensated Altitude from the AltitudeClick. 
        /// </summary>
        /// <param name="compensationMode">The compensation mode for Altitude Measurement. See <see cref="PressureCompensationModes"/> for more information.</param>
        /// <returns>Uncompensated or Sea Level Compensated Altitude in meters (m).</returns>
        /// <example>Example usage:
        /// <code language = "C#">
        /// var rawAltitude = _altitude.ReadAltitude(PressureCompensationModes.Uncompensated);
        /// var compensatedAltitude = _altitude.ReadPressure(PressureCompensationModes.SeaLevelCompensated);
        /// Debug.Print("Raw Altitude - " + rawAltitude + "  meters");
        /// Debug.Print("Sea Level Compensated Altitude - " + compensatedAltitude + "  meters");
        /// </code>
        /// <code language = "VB">
        /// Dim rawAltitude = _altitude.ReadAltitude(PressureCompensationModes.Uncompensated)
        /// Dim compensatedAltitude = _altitude.ReadPressure(PressureCompensationModes.SeaLevelCompensated)
        /// Debug.Print("Raw Altitude - " <![CDATA[&]]> rawAltitude <![CDATA[&]]> "  meters")
        /// Debug.Print("Sea Level Compensated Altitude - " <![CDATA[&]]> compensatedAltitude <![CDATA[&]]> "  meters")
        /// </code>
        /// </example>
        public float ReadAltitude(PressureCompensationModes compensationMode = PressureCompensationModes.SeaLevelCompensated)
        {
            var counter = 0;

            SetModeStandby(); // Sensor must be in StandBy Mode when changing read mode. 
            SetModeAltimeter(); // Set mode to Altimeter
            SetModeActive(); // Sensor must be active to take readings.
            ToggleOneShot(); // Force the sensor to take a new reading.

            while (!PressureDataReady())
            {
                if (counter++ == MaxDataReadyAttempts) return -float.MinValue;
                Thread.Sleep(1);
            } // Wait here until new data is available.

            // Read Altitude 
            int mAltitude = ReadByte(OUT_P_MSB);
            int cAltitude = ReadByte(OUT_P_CSB);
            var lAltitude = (ReadByte(OUT_P_LSB) >> 4) / 16.0;
            var altitude = ((mAltitude << 8) | cAltitude) + lAltitude;

            if (compensationMode == PressureCompensationModes.Uncompensated) return (float) altitude;

            var tempPressure = ReadPressure(PressureCompensationModes.Uncompensated);
            if (Math.Abs(tempPressure - (-999.999)) < 0) return float.MinValue; 
            var seaLevelCompensation = 101325 * Math.Pow(((288 - 0.0065 * 143) / 288), 5.256);
            var tempValue = 101325 + tempPressure - seaLevelCompensation;
            return (float)(44330 * (1.0 - Math.Pow(tempValue / CalculatePressureAsl(tempValue), 0.1903)));

        }

        /// <summary>
        /// Gets the Uncompensated or Sea Level Compensated Pressure from the AltitudeClick. 
        /// </summary>
        /// <param name="compensationMode">The compensation mode for Pressure Measurement. See <see cref="PressureCompensationModes"/> for more information.</param>
        /// <returns>The uncompensated or sea level compensated pressure depending on the <para>"compensationMode"</para> parameter passed in Pascals (Pa).</returns>
        /// <example>Example usage:
        /// <code language = "C#">
        /// var rawPressure = _altitude.ReadPressure(PressureCompensationModes.Uncompensated);
        /// var compensatedPressure = _altitude.ReadPressure(PressureCompensationModes.SeaLevelCompensated);
        /// Debug.Print("Raw Pressure - " + PascalsToInches(rawPressure) + " Pa");
        /// Debug.Print("Seal Level Compensated Pressure - " + compensatedPressure + " Pa");
        /// </code>
        /// <code language = "VB">
        /// Dim rawPressure = _altitude.ReadPressure(PressureCompensationModes.Uncompensated)
        /// Dim compensatedPressure = _altitude.ReadPressure(PressureCompensationModes.SeaLevelCompensated)
        /// Debug.Print("Raw Pressure - " <![CDATA[&]]> PascalsToInches(rawPressure) <![CDATA[&]]> " Pa")
        /// Debug.Print("Seal Level Compensated Pressure - " <![CDATA[&]]> compensatedPressure <![CDATA[&]]> " Pa")
        /// </code>
        /// </example>
        public float ReadPressure(PressureCompensationModes compensationMode = PressureCompensationModes.SeaLevelCompensated)
        {
            var counter = 0;
            SetModeStandby(); // Sensor must be in StandBy Mode when changing read mode. 
            SetModeBarometer(); // Set mode to Barometer.
            SetModeActive(); // Sensor must be active to take reading.
            ToggleOneShot(); // Toggle One shot to force sensor to take reading.

            while (!PressureDataReady())
            {
                Thread.Sleep(1);
                if (counter++ == MaxDataReadyAttempts) return float.MinValue;
            } // Wait here until Pressure data is available or return -999.999 if times out.

            // read the Pressure 
            Int32 mPressure = ReadByte(OUT_P_MSB);
            var cPressure = ReadByte(OUT_P_CSB);
            var lPressure = ReadByte(OUT_P_LSB);

            // Calculate the pressure
            mPressure <<= 16;
            mPressure |= cPressure << 8;
            mPressure |= lPressure;
            var pressure = mPressure / 64;
            
            if (compensationMode == PressureCompensationModes.Uncompensated) return pressure;

            var seaLevelCompensation = 101325 * Math.Pow(((288 - 0.0065 * 143) / 288), 5.256);
            return (float) (101325 + pressure - seaLevelCompensation);
        }

        /// <summary>
        /// Reads the temperature in Celsius (°C). 
        /// </summary>
        /// <param name="source">The temperature source to measure. See <see cref="TemperatureSources"/> for more information.</param>
        /// <returns>Temperature in Celsius (C) if successful, otherwise -999.999</returns>
        /// <example>Example usage:
        /// <code language = "C#">
        /// var temp = _altitude.ReadTemperature(TemperatureSources.Ambient);
        /// Debug.Print("Temperature - " + temp + " °C\n");
        /// </code>
        /// <code language = "VB">
        /// Dim temp = _altitude.ReadTemperature(TemperatureSources.Ambient)
        /// Debug.Print("Temperature - " <![CDATA[&]]> temp <![CDATA[&]]> " <![CDATA[&]]> Microsoft.VisualBasic.Constants.vbCrLf)
        /// </code>
        /// </example>
        public float ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (source == TemperatureSources.Object) throw new NotImplementedException("The AltitudeClick does not provide Object temperature measurement. Use TemperatureSources.Ambient for Temperature measurement.");

            var counter = 0;
            // We do not have to change modes to read temperature.
            ToggleOneShot();
            while (!TemperatureDataReady())
            {
                if (counter++ == MaxDataReadyAttempts) return float.MinValue;
                Thread.Sleep(1);
            } // Wait here until new data is available.

            // read Temperature 
            var mTemp = ReadByte(OUT_T_MSB);
            var lTemp = (ReadByte(OUT_T_LSB) >> 4) / 16.0;

            var temperature = (float) (mTemp + lTemp);
            return Utilities.Temperature.ConvertTo(TemperatureUnits.Celsius, _temperatureUnit, temperature);
        }

        /// <summary>
        ///     Resets the USBUART Click.
        /// </summary>
        /// <param name="resetMode">The reset mode, see <see cref="ResetModes"/> for more information.</param>
        /// <remarks>
        /// This module has no Reset method, calling this method will throw an exception.
        /// </remarks>
        /// <exception cref="NotImplementedException">This module does not implement a reset method. Calling this method will throw a <see cref="NotImplementedException"/></exception>
        /// <example>None: This module does not support a Reset method.</example>
        public bool Reset(ResetModes resetMode)
        {
            throw new NotImplementedException("This module does not implement a Reset method.");
        }

        #endregion

        #region Events
        
        /// <summary>
        /// This event is raised when the AltitudeClick completes a measurement, see <see cref="MeasurementComplete"/>.
        /// </summary>
        public event MeasurementCompleteEventHandler MeasurementComplete;

        private void _measurementComplete(object sender, MeasurementCompleteEventArgs args)
        {
            MeasurementCompleteEventHandler handler = MeasurementComplete;

            if (handler == null) return;

            handler(sender, args);
        }

        /// <summary>
        ///  Represents the delegate that is used for the <see cref="AltitudeClick.MeasurementComplete"/> event.
        /// </summary>
        /// <param name="sender">The AltitudeClck that raised the event.</param>
        /// <param name="e">The <see cref="MeasurementCompleteEventArgs"/> representing the Altitude data.</param>
        public delegate void MeasurementCompleteEventHandler(object sender, MeasurementCompleteEventArgs e);

        /// <summary>
        /// A class holding the sensor data for he <see cref="MeasurementComplete"/> Event.
        /// </summary>
        public class MeasurementCompleteEventArgs
        {
            /// <summary>
            /// A class holding the sensor data as read from the AltitudeClick.
            /// </summary>
            /// <param name="rawAltitude">The raw altitude in meters (m)</param>
            /// <param name="compensatedAltitude">The sea level compensated altitude in meters (m)</param>
            /// <param name="rawPressure">The raw pressure in Pascals (Pa)</param>
            /// <param name="compensatedPressure">The sea level compensated pressure in Pascals (Pa)</param>
            /// <param name="temperature">The temperature in the unit specified by <see cref="TemperatureUnit"/> property.</param>
            public MeasurementCompleteEventArgs(float rawAltitude, float compensatedAltitude, float rawPressure, float compensatedPressure, float temperature)
            {
                RawAltitude = rawAltitude;
                CompensatedAltitude = compensatedAltitude;
                RawPressure = rawPressure;
                CompensatedPressure = compensatedPressure;
                Temperature = temperature;
            }

            /// <summary>
            /// The raw altitude in meters (m)
            /// </summary>
            public double RawAltitude { get; private set; }
            /// <summary>
            /// The sea level compensated altitude in Pascals (Pa)
            /// </summary>
            public float CompensatedAltitude { get; private set; }
            /// <summary>
            /// The raw pressure in Pascals (Pa)
            /// </summary>
            public double RawPressure { get; private set; }
            /// <summary>
            /// The sea level compensated pressure in Pascals (Pa)
            /// </summary>
            public float CompensatedPressure { get; private set; }
            /// <summary>
            /// The temperature in the unit specified by <see cref="TemperatureUnit"/> property.
            /// </summary>
            public double Temperature { get; private set; }
        }

        #endregion

    }
}