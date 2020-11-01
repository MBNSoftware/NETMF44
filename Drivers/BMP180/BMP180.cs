/*
 * BMP180 Module Driver for MikroBusNet
 * 
 * - Based on the Bosch BMP180 Absolute Pressure Sensor.
 * - This driver will also work for the Bosch BMP085 Absolute Pressure Sensor.
 * Version 1.0 -Initial version coded by Stephen Cardinale
 *  - Never released.
 * Version 2.0 
 *  * - Implementation of MBN Interface and Namespaces requirements.
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
    /// A MikroBusNet Driver for a Bosch BMP180 module. This driver will also work for the Bosch BMP085 Sensor.
    /// <para><b>This module is an I2C Device</b></para>
    /// <para><b>Pins used :</b> Scl, Sda</para>
    /// <para><b>References required:</b> MikroBus.Net, Microsoft.SPOT.Native, Microsoft.Spot.Hardware, mscorlib</para>
    /// </summary>
    /// <example>
    /// <code language = "C#">
    /// using MBN;
    /// using MBN.Enums;
    /// using MBN.Modules;
    /// using Microsoft.SPOT;
    /// using System.Threading;
    /// 
    /// namespace Examples
    /// {
    ///     public class Program
    ///     {
    ///         private static BMP180 _bmp180;
    ///
    ///         public static void Main()
    ///         {
    ///             _bmp180 = new BMP180(Hardware.SocketThree, I2CBusSpeed.ClockKHz400, 1000, Hardware.SocketThree.An) {OverSamplingSetting = OverSamplingSetting.UltraHighResolution};
    ///             _bmp180.MeasurementComplete += _bmp180_MeasurementComplete;
    ///
    ///             Debug.Print("BMP180 Demo");
    ///             Debug.Print("Driver Version Info - " + _bmp180.DriverVersion);
    ///             Debug.Print("Is a BMP180 connected? " + _bmp180.IsConnected());
    ///             Debug.Print("BMP180 Sensor OSS is - " + _bmp180.OverSamplingSetting + "\n");
    ///
    ///             /* Use one of the following methods to read sensor */
    ///             new Thread(PollingwithEventsThread).Start();
    ///             //new Thread(DirectReadThread).Start();
    ///
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    ///
    ///         static void _bmp180_MeasurementComplete(object sender, SensorData sensorData)
    ///         {
    ///             Debug.Print("Temperature - " + sensorData.Temperature.ToString("f2") + (_bmp180.TemperatureUnit == TemperatureUnits.Celsius ? "°C" : _bmp180.TemperatureUnit == TemperatureUnits.Fahrenheit ? " °F" : "°K"));
    ///             Debug.Print("Pressure (Sea Level Compensated) - " + Pressure.ToInchesHg(sensorData.CompensatedPressure).ToString("f2") + " in Hg");
    ///             Debug.Print("Pressure (Raw) - " + Pressure.ToInchesHg(sensorData.RawPressure).ToString("f2") + " in Hg");
    ///             Debug.Print("Altitude - " + Altitude.ToFeet(sensorData.Altitude) + " feet" + "\n");
    ///             Debug.Print("SensorData ToString method - " + sensorData + "\n");
    ///         }
    ///
    ///         private static void DirectReadThread()
    ///         {
    ///             Debug.Print("____________Sensor Data using direct read methods____________");
    ///
    ///             while (true)
    ///             {
    ///                 Debug.Print("Temperature - " + _bmp180.ReadTemperature().ToString("f2") + (_bmp180.TemperatureUnit == TemperatureUnits.Celsius ? "°C" : _bmp180.TemperatureUnit == TemperatureUnits.Fahrenheit ? " °F" : "°K"));
    ///                 Debug.Print("Temperature RawData - " +  (_bmp180 as ITemperature).RawData);
    ///                 Debug.Print("SLC Compensated Pressure - " + Pressure.ToInchesHg(_bmp180.ReadPressure()).ToString("f1") + " in Hg");
    ///                 Debug.Print("Uncompensated Pressure - " + Pressure.ToInchesHg(_bmp180.ReadPressure(PressureCompensationModes.Uncompensated)).ToString("f1") + " in Hg");
    ///                 Debug.Print("Pressure RawData - " + (_bmp180 as IPressure).RawData);
    ///                 Debug.Print("Altitude - " + Altitude.ToFeet(_bmp180.ReadAltitude()) + " feet");
    ///                 Debug.Print("\n");
    ///                 Thread.Sleep(5000);
    ///             }
    ///         }
    ///
    ///         private static void PollingwithEventsThread()
    ///         {
    ///             Debug.Print("____________Sensor Data - Event Driven____________");
    ///
    ///             while (true)
    ///             {
    ///                 _bmp180.ReadSensor();
    ///                 Thread.Sleep(5000);
    ///             }
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
    /// Imports MBN.Interfaces
    /// Imports MBN.Modules
    /// Imports MBN.Utilities
    /// Imports Microsoft.SPOT
    /// Imports System.Threading
    ///
    /// Namespace Examples
    ///
    ///     Public Module Module1
    ///
    ///         Dim _bmp180 As BMP180
    ///
    ///         Sub Main()
    ///
    ///             _bmp180 = New BMP180(Hardware.SocketThree, ClockRatesI2C.Clock400KHz, 1000, Hardware.SocketThree.An)
    ///
    ///             _bmp180.OverSamplingSetting = BMP180.Oss.UltraHighResolution
    ///
    ///             AddHandler _bmp180.MeasurementComplete, AddressOf MeasurementComplete
    ///
    ///             Debug.Print("BMP180 Demo")
    ///             Debug.Print("Driver Version Info - " <![CDATA[&]]> _bmp180.DriverVersion.ToString())
    ///             Debug.Print("Is a BMP180 connected? " <![CDATA[&]]> _bmp180.IsConnected())
    ///             Debug.Print("BMP180 Sensor OSS is - " <![CDATA[&]]> _bmp180.OverSamplingSetting <![CDATA[&]]> Microsoft.VisualBasic.Constants.vbCrLf)
    ///
    ///             ' Use one of the following methods to read sensor 
    ///             Dim pollingThread As New Thread(New ThreadStart(AddressOf PollingThreadDelegate))
    ///             pollingThread.Start()
    ///
    ///             ' Dim directReadThread As New Thread(New ThreadStart(AddressOf DirectReadThreadDelegate))
    ///             ' pollingThread.Start()
    ///
    ///             Thread.Sleep(Timeout.Infinite)
    ///
    ///         End Sub
    ///
    ///         Private Sub DirectReadThreadDelegate()
    ///             Debug.Print("____________Sensor Data using direct read methods____________")
    ///
    ///            While True
    ///                 Debug.Print("Temperature - " <![CDATA[&]]> _bmp180.ReadTemperature().ToString("f2") <![CDATA[&]]> (If(_bmp180.TemperatureUnit = TemperatureUnits.Celsius, "°C", If(_bmp180.TemperatureUnit = TemperatureUnits.Fahrenheit, " °F", "°K"))))
    ///                 Debug.Print("Temperature RawData - " <![CDATA[&]]> DirectCast(_bmp180, ITemperature).RawData)
    ///                 Debug.Print("SLC Compensated Pressure - " <![CDATA[&]]> Pressure.ToInchesHg(_bmp180.ReadPressure()).ToString("f1") <![CDATA[&]]> " in Hg")
    ///                 Debug.Print("Uncompensated Pressure - " <![CDATA[&]]> Pressure.ToInchesHg(_bmp180.ReadPressure(PressureCompensationModes.Uncompensated)).ToString("f1") <![CDATA[&]]> " in Hg")
    ///                 Debug.Print("Pressure RawData - " <![CDATA[&]]> DirectCast(_bmp180, IPressure).RawData)
    ///                 Debug.Print("Altitude - " <![CDATA[&]]> Altitude.ToFeet(_bmp180.ReadAltitude()) <![CDATA[&]]> " feet")
    ///                 Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///                 Thread.Sleep(5000)
    ///             End While
    ///         End Sub
    ///
    ///         Private Sub PollingThreadDelegate()
    ///
    ///             Debug.Print("____________Sensor Data - Event Driven____________")
    ///    
    ///             While (True)
    ///                 _bmp180.ReadSensor()
    ///                 Thread.Sleep(5000)
    ///             End While
    ///
    ///         End Sub
    ///
    ///         Private Sub MeasurementComplete(ByVal sender As Object, ByVal sensorData As BMP180.SensorData)
    ///             Debug.Print("Temperature - " <![CDATA[&]]> sensorData.Temperature.ToString("f2") <![CDATA[&]]> (If(_bmp180.TemperatureUnit = TemperatureUnits.Celsius, "°C", If(_bmp180.TemperatureUnit = TemperatureUnits.Fahrenheit, " °F", "°K"))))
    ///             Debug.Print("Pressure (Sea Level Compensated) - " <![CDATA[&]]> Pressure.ToInchesHg(sensordata.CompensatedPressure).ToString("f2") <![CDATA[&]]> " in Hg")
    ///             Debug.Print("Pressure (Sea Level Compensated) - " <![CDATA[&]]> Pressure.ToHectoPascals(sensordata.CompensatedPressure).ToString("f1") <![CDATA[&]]> " hPa")
    ///             Debug.Print("Pressure (Raw) - " <![CDATA[&]]> Pressure.ToInchesHg(sensordata.RawPressure).ToString("f2") <![CDATA[&]]> " in Hg")
    ///             Debug.Print("Pressure (Raw) - " <![CDATA[&]]> Pressure.ToHectoPascals(sensordata.RawPressure).ToString("f1") <![CDATA[&]]> " hPa")
    ///             Debug.Print("Altitude - " <![CDATA[&]]> Altitude.ToFeet(sensordata.Altitude).ToString("f0") <![CDATA[&]]> " feet")
    ///             Debug.Print("Altitude - " <![CDATA[&]]> sensordata.Altitude.ToString("f0") <![CDATA[&]]> " meters")
    ///             Debug.Print("SensorData ToString method - " <![CDATA[&]]> sensordata.ToString() <![CDATA[&]]> Microsoft.VisualBasic.Constants.vbCrLf)
    ///         End Sub
    ///
    ///     End Module
    ///
    /// End Namespace
    /// </code>
    /// </example>
// ReSharper disable once InconsistentNaming
    public class BMP180 : IDriver, ITemperature, IPressure
    {

        #region Fields

        #region Calibration Data Fields

        private static int _ac1;
        private static int _ac2;
        private static int _ac3;
        private static int _ac4;
        private static int _ac5;
        private static int _ac6;
        private static int _b1;
        private static int _b2;
        private static int _mb;
        private static int _mc;
        private static int _md;

        #endregion

        private static I2CDevice.Configuration _i2CConfiguration; // I²C configuration
        private static int _i2CTimeout;

        private static OutputPort _resetBusyPin;

        private static Oss _overSamplingSetting = Oss.Standard; // 0 = low precision & power to 3 = higher both
        private static TemperatureUnits _temperatureUnit = TemperatureUnits.Celsius;

        #endregion

        #region ENUMS

        /// <summary>
        ///     Enumeration of the Over Sampling Setting (OSS) of the BMP180 Module.
        /// </summary>
        public enum Oss : byte
        {
            /// <summary>
            ///     Least accurate and least power consumption.
            /// </summary>
            UltraLowPower = 0,

            /// <summary>
            ///     Standard mode
            /// </summary>
            Standard = 1,

            /// <summary>
            ///     High Resolution
            /// </summary>
            HighResolution = 2,

            /// <summary>
            ///     Most accurate and most power consumption.
            /// </summary>
            UltraHighResolution = 3
        } 

        #endregion

        #region CTOR

        /// <summary>
        ///     Default Constructor
        /// </summary>
        /// <param name="socket">The socket that the BMP180 Module is connected into.</param>
        /// <param name="clockRate">I2CBus clock speed, either standard or high speed bus.</param>
        /// <param name="i2CTimeout">I2C Transaction timeout.</param>
        /// <param name="resetBusyPin">The CPU.Pin used internally to test is the BMP180 Module has completed reset process.</param>
        public BMP180(Hardware.Socket socket, ClockRatesI2C clockRate, int i2CTimeout, Cpu.Pin resetBusyPin = Cpu.Pin.GPIO_NONE)
        {
            try
            {
                if (resetBusyPin == Cpu.Pin.GPIO_NONE) resetBusyPin = socket.An;
                // Checks if needed I²C pins are available
                Hardware.CheckPinsI2C(socket, resetBusyPin);

                // Create the driver's I²C configuration
                _i2CConfiguration = new I2CDevice.Configuration(0x77, (byte) clockRate);
                _i2CTimeout = i2CTimeout;

                // Check if it's the first time an I²C device is created
                if (Hardware.I2CBus == null)
                {
                    Hardware.I2CBus = new I2CDevice(_i2CConfiguration);
                }

                // Is the BMP180 there?
                if (!IsConnected()) throw new DeviceInitialisationException("BMP180 not found.");

                // Get Calibration Data
                if (!GetCalibrationData()) throw new DeviceInitialisationException("BMP180 GetCalibrationData failed.");

                _resetBusyPin = new OutputPort(resetBusyPin, false);
            }
                // Catch only the PinInUse exception, so that program will halt on other exceptions and send it directly to caller
            catch (PinInUseException ex)
            {
                throw new PinInUseException(ex.Message);
            }
        }

        #endregion

        #region Private Methods

        private static float CalculatePressureAsl(float pressure)
        {
            var seaLevelCompensation = (float) (101325 * Math.Pow(((288 - 0.0065 * 143) / 288), 5.256));
            return 101325 + pressure - seaLevelCompensation;
        }

        private static bool GetCalibrationData()
        {
            var rawData = ReadRegister(0xAA, 22);

            _ac1 = (short)(rawData[0] << 8) + rawData[1];
            _ac2 = (short)(rawData[2] << 8) + rawData[3];
            _ac3 = (short)(rawData[4] << 8) + rawData[5];
            _ac4 = (ushort)(rawData[6] << 8) + rawData[7];
            _ac5 = (ushort)(rawData[8] << 8) + rawData[9];
            _ac6 = (ushort)(rawData[10] << 8) + rawData[11];
            _b1 = (short)(rawData[12] << 8) + rawData[13];
            _b2 = (short)(rawData[14] << 8) + rawData[15];
            _mb = (short)(rawData[16] << 8) + rawData[17];
            _mc = (short)(rawData[18] << 8) + rawData[19];
            _md = (short)(rawData[20] << 8) + rawData[21];

            // If any of the 11 CalibrationData Words are either 0x00 or 0xFFFF the calibration data read has failed. 
            return (_ac1 != 0 || _ac1 != 0xFFFF) && (_ac2 != 0 || _ac2 != 0xFFFF) && (_ac3 != 0 | _ac3 != 0xFFFF) && (_ac4 != 0 | _ac4 != 0xFFFF) && (_ac5 != 0 || _ac5 != 0xFFFF) && (_ac6 != 0 || _ac6 != 0xFFFF) && (_b1 != 0 | _b1 != 0xFFFF) && (_b2 != 0 || _b2 != 0xFFFF) && (_mb != 0 || _mb != 0xFFFF) && (_mc != 0 | _mc != 0xFFFF) && (_md != 0 || _md != 0xFFFF);
        }

        private static byte ReadByte(byte register)
        {
            var result = new Byte[1];

            var actions = new I2CDevice.I2CTransaction[2];
            actions[0] = I2CDevice.CreateWriteTransaction(new[] {register});
            actions[1] = I2CDevice.CreateReadTransaction(result);

            Hardware.I2CBus.Execute(_i2CConfiguration, actions, _i2CTimeout);

            return result[0];
        }

        private static byte[] ReadRegister(byte register, byte bytesToRead)
        {
            var result = new Byte[bytesToRead];

            var actions = new I2CDevice.I2CTransaction[2];
            actions[0] = I2CDevice.CreateWriteTransaction(new[] {register});
            actions[1] = I2CDevice.CreateReadTransaction(result);

            Hardware.I2CBus.Execute(_i2CConfiguration, actions, _i2CTimeout);

            return result;
        }

        private static void WriteByte(byte register, byte data)
        {
            var actions = new I2CDevice.I2CTransaction[1];
            actions[0] = I2CDevice.CreateWriteTransaction(new[] {register, data});

            Hardware.I2CBus.Execute(_i2CConfiguration, actions, _i2CTimeout);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the driver version.
        /// </summary>
        /// <value>
        ///     The driver version see <see cref="Version"/>.
        /// </value>
        /// <example>Example usage to get the Driver Version in formation:
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
        ///     Sets or Gets the Over Sampling Setting (OSS) of the BMP180 Module see <see cref="Oss" />.
        /// </summary>
        /// <example>Example usage to set the Over Sampling Setting:
        /// <code language="C#">
        /// _bmp180.OverSamplingSetting = OverSamplingSetting.UltraHighResolution;
        /// </code>
        /// <code language="VB">
        /// _bmp180.OverSamplingSetting = OverSamplingSetting.UltraHighResolution
        /// </code>
        /// </example>
        /// <example>Example usage to get the Over Sampling Setting:
        /// <code language="C#">
        /// Debug.Print("BMP180 Module OSS is - " + _bmp180.OverSamplingSetting.ToString());
        /// </code>
        /// <code language="VB">
        /// Debug.Print("BMP180 Module OSS is - " <![CDATA[&]]> _bmp180.OverSamplingSetting.ToString())
        /// </code>
        /// </example>
        public Oss OverSamplingSetting
        {
            get { return _overSamplingSetting; }
            set { _overSamplingSetting = value; }
        }

        /// <summary>
        ///     Gets or sets the power mode.
        /// </summary>
        /// <value>
        ///     The current power mode of the module.
        /// </value>
        /// <returns cref="NotImplementedException">Calling this method will throw a <see cref="NotImplementedException"/>.</returns>
        /// <remarks>
        ///     This module does not use Power Modes, the GET accessor will always return PowerModes.On. See <see cref="PowerModes"/>, while the SET accessor will throw a <see cref="NotImplementedException"/>.
        /// </remarks>
        /// <exception cref="NotImplementedException"></exception>
        /// <example>None: This Module does not support PowerMode.</example>
        public PowerModes PowerMode
        {
            get { return PowerModes.On; }
            set { throw new NotImplementedException("Power Mode not implemented for this module"); }
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
        /// _bmp180.TemperatureUnit = TemperatureUnits.Farhenheit;
        /// </code>
        /// <code language="VB">
        /// ' Set temperature unit to Fahrenheit
        /// _bmp180.TemperatureUnit = TemperatureUnits.Farhenheit
        /// </code>
        /// </example>
        public TemperatureUnits TemperatureUnit
        {
            get { return _temperatureUnit; }
            set { _temperatureUnit = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method determines if a BMP180 module is connected.
        /// </summary>
        /// <returns>True if a BMP180 module is connected to the I2C Bus or otherwise false.</returns>
        /// <example>
        /// <code language = "C#">
        /// Debug.Print("Is a BMP180 connected? " + _bmp180.IsConnected());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Is a BMP180 connected? " <![CDATA[&]]> _bmp180.IsConnected())
        /// </code>
        /// </example>
        public bool IsConnected()
        {
            return ReadByte(0xD0) == 0x55;
        }

        /// <summary>
        /// The altitude as read from the BMP180 module. 
        /// </summary>
        /// <returns>The altitude in meters.</returns>
        /// <remarks>The altitude reading is a calculated value based on well established mathematical formulas.</remarks>
        /// <example>Example usage to read the altitude:
        /// <code language = "C#">
        /// Debug.Print("Altitude - " + _bmp180.ReadAltitude());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Altitude - " <![CDATA[&]]> _bmp180.ReadAltitude())
        /// </code>
        /// </example>
        public int ReadAltitude()
        {
            return (int)Math.Round((44330 * (1.0 - Math.Pow(ReadPressure(PressureCompensationModes.Uncompensated) / ReadPressure(), 0.1903))));
        }

        /// <summary>
        /// Return the pressure as read by the BMP180 module.
        /// </summary>
        /// <param name="compensationMode">The <see cref="PressureCompensationModes"/> of the BMP180 module.</param>
        /// <returns>The pressure as read from the BMP180 module, either uncompensated or Sea Level Compensated in Pascals (Pa)</returns>
        /// <remarks>One (1) Pascal (Pa) is equivalent to one (100) millibar (mBar).</remarks>
        /// <remarks>Standard meteorological reporting of Atmospheric pressure is Sea Level compensated.</remarks>
        /// <example>Example usage to read the Pressure:
        /// <code language = "C#">
        /// Debug.Print("Sea Level Compensated Pressure - " + _bmp180.ReadPressure() + " meters.");
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Sea Level Compensated Pressure - " <![CDATA[&]]> _bmp180.ReadPressure() <![CDATA[&]]> " meters.")
        /// </code>
        /// </example>
        public float ReadPressure(PressureCompensationModes compensationMode = PressureCompensationModes.SeaLevelCompensated)
        {
            var x = ((this as ITemperature).RawData - _ac6)*_ac5 >> 15;
            var y = (_mc << 11)/(x + _md);
            var b5 = x + y;
            var b6 = b5 - 4000;
            var x1 = (_b2*(b6*b6 >> 12)) >> 11;
            var x2 = _ac2*b6 >> 11;
            var x3 = x1 + x2;
            var b3 = (((_ac1*4 + x3) << (byte) _overSamplingSetting) + 2) >> 2;
            x1 = _ac3*b6 >> 13;
            x2 = (_b1*(b6*b6 >> 12)) >> 16;
            x3 = ((x1 + x2) + 2) >> 2;
            var b4 = (uint) (_ac4*(x3 + 32768) >> 15);
            var b7 = (uint) (((this as IPressure).RawData - b3)*(50000 >> (byte) _overSamplingSetting));
            var p = (int) ((b7 < 0x80000000) ? (b7*2)/b4 : (b7/b4)*2);
            x1 = (p >> 8)*(p >> 8);
            x1 = (x1*3038) >> 16;
            x2 = (-7357*p) >> 16;
            var pressure = p + ((x1 + x2 + 3791) >> 4);

            return compensationMode == PressureCompensationModes.Uncompensated ? pressure : CalculatePressureAsl(pressure);
        }

        /// <summary>
        /// Reads the BMP180 module and returns the <see cref="SensorData"/> (Temperature, Uncompensated Pressure, Sea Level Compensated Pressure and Altitude) by raising the <see cref="MeasurementComplete"/> event.
        /// </summary>
        /// <example>Example usage::
        /// <code language = "C#">
        /// while (true)
        /// {
        ///     _bmp180.ReadSensor();
        ///     Thread.Sleep(5000);
        ///    }
        /// </code>
        /// <code language = "VB">
        /// While True
        ///	    _bmp180.ReadSensor()
        ///	    Thread.Sleep(5000)
        /// End While
        /// </code>
        /// </example>
        public void ReadSensor()
        {
            while (_resetBusyPin.Read()) Thread.Sleep(10);

            // ReSharper disable JoinDeclarationAndInitializer
            int x1, x2, b5, b6, x3, b3, p, ut, up;
            uint b7, b4;

            // Start conversion
            WriteByte(0xF4, 0x2E);
            Thread.Sleep(5); // Wait 5 mSec to read the uncompensated temperature, minimum is 4.5 mSec per dataSheet.

            var utData = ReadRegister(0xF6, 2);

            ut = (utData[0] << 8) + utData[1];

            WriteByte(0xF4, (byte)(((byte)(_overSamplingSetting) << 6) + 0x34));

            //Per data sheet wait 4.5 mSec for OSS=0, 7.5 mSec for OSS=1, 13.5 mSec for OSS=2 and 25.5 mSec for OSS=3 
            Thread.Sleep(_overSamplingSetting == 0 ? 5 : (byte)_overSamplingSetting == 1 ? 8 : (byte)_overSamplingSetting == 2 ? 14 : 26);

            var upData = ReadRegister(0xF6, 3);

            up = ((upData[0] << 16) + (upData[1] << 8) + upData[2]) >> (8 - (byte)_overSamplingSetting);

            // Calculate compensated temperature.
            x1 = (ut - _ac6) * _ac5 >> 15;
            x2 = (_mc << 11) / (x1 + _md);
            b5 = x1 + x2;
            var t = ((float) ((b5 + 8) >> 4)) / 10;
            var temperature = Utilities.Temperature.ConvertTo(TemperatureUnits.Celsius, _temperatureUnit, t);

            //Calculate compensated pressure
            b6 = b5 - 4000;
            x1 = (_b2 * (b6 * b6 >> 12)) >> 11;
            x2 = _ac2 * b6 >> 11;
            x3 = x1 + x2;
            b3 = (((_ac1 * 4 + x3) << (byte) _overSamplingSetting) + 2) >> 2;
            x1 = _ac3 * b6 >> 13;
            x2 = (_b1 * (b6 * b6 >> 12)) >> 16;
            x3 = ((x1 + x2) + 2) >> 2;
            b4 = (uint) (_ac4 * (x3 + 32768) >> 15);
            b7 = (uint) ((up - b3) * (50000 >> (byte) _overSamplingSetting));
            p = (int) ((b7 < 0x80000000) ? (b7 * 2) / b4 : (b7 / b4) * 2);
            x1 = (p >> 8)*(p >> 8);
            x1 = (x1 * 3038) >> 16;
            x2 = (-7357 * p) >> 16;
            var pressureRaw = p + ((x1 + x2 + 3791) >> 4);
            var pressureCompensated = CalculatePressureAsl(pressureRaw);

            //Calculate Altitude
            var altitude = (int) (44330 * (1.0 - Math.Pow(pressureRaw/pressureCompensated, 0.1903)));

            // Raise the MeasurementComplete Event.
            if (MeasurementComplete != null) MeasurementComplete(this, new SensorData(temperature, pressureRaw, pressureCompensated, altitude));
        }

        /// <summary>
        /// Return the temperature as read by the BMP180 module.
        /// </summary>
        /// <param name="source">The <see cref="TemperatureSources.Ambient"/> to measure.</param>
        /// <returns>The temperature in the unit specified by the <see cref="TemperatureUnit"/> property.</returns>
        /// <exception cref="NotImplementedException">A <see cref="NotImplementedException"/> will be thrown the source parameter is <see cref="TemperatureSources.Object"/>. The BMP180 does not support Object temperature measurement.</exception>
        /// <example>Example usage to read the Temperature:
        /// <code language = "C#">
        /// Debug.Print("Temperature - " + _bmp180.ReadTemperature() + " °C");
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Temperature - " <![CDATA[&]]> _bmp180.ReadTemperature() <![CDATA[&]]> " °C")
        /// </code>
        /// </example>
        public float ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (source == TemperatureSources.Object) throw new NotImplementedException("The BMP180 module does not provide Object temperature measurement. Use TemperatureSources.Ambient for Temperature measurement.");

            int ut = (this as ITemperature).RawData;

            int x1 = (ut - _ac6) * _ac5 >> 15;
            int x2 = (_mc << 11) / (x1 + _md);
            int b5 = x1 + x2;

            var temperature = ((float)((b5 + 8) >> 4)) / 10;
            return Utilities.Temperature.ConvertTo(TemperatureUnits.Celsius, _temperatureUnit, temperature);
        }

        /// <summary>
        /// The raw pressure data as read from the BMP180 module.
        /// </summary>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.Print("ReadRaw (Pressure) - " + (_bmp180Sensor as IPressure).ReadRaw);
        /// </code>
        /// <code language="VB">
        /// Debug.Print("ReadRaw (Pressure) - " <![CDATA[&]]> (_bmp180Sensor as IPressure).ReadRaw)
        /// </code>
        /// </example>
        /// <remarks>Not particularly useful but provided if one wants to calculate temperature by their own implementation.</remarks>
        int IPressure.RawData
        {
            get
            {
                while (_resetBusyPin.Read()) Thread.Sleep(10);

                WriteByte(0xF4, (byte)(((byte)(_overSamplingSetting) << 6) + 0x34));

                //Per data sheet wait 4.5 mSec for OSS=0, 7.5 mSec for OSS=1, 13.5 mSec for OSS=2 and 25.5 mSec for OSS=3 
                Thread.Sleep(_overSamplingSetting == 0 ? 5 : (byte) _overSamplingSetting == 1 ? 8 : (byte) _overSamplingSetting == 2 ? 14 : 26);

                var upData = ReadRegister(0xF6, 3);

                return ((upData[0] << 16) + (upData[1] << 8) + upData[2]) >> (8 - (byte) _overSamplingSetting);
            }
        }

        /// <summary>
        /// The raw temperature data as read from the BMP180 module.
        /// </summary>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.Print("ReadRaw (Temperature) - " + (_bmp180Sensor as ITemperature).ReadRaw);
        /// </code>
        /// <code language="VB">
        /// Debug.Print("ReadRaw (Temperature) - " <![CDATA[&]]> (_bmp180Sensor as ITemperature).ReadRaw)
        /// </code>
        /// </example>
        /// <remarks>Not particularly useful but provided if one wants to calculate temperature by their own implementation.</remarks>
        int ITemperature.RawData
        {
            get
            {
                while (_resetBusyPin.Read()) Thread.Sleep(10);

                // Start the uncompensated temperature conversion (UT)
                WriteByte(0xF4, 0x2E);

                Thread.Sleep(5); // Wait 5 mSec to read the uncompensated temperature, minimum is 4.5 mSec per dataSheet.

                // Read Uncompensated Temperature
                var data = ReadRegister(0xF6, 2);

                return (data[0] << 8) + data[1];
            }
        }

        /// <summary>
        /// Resets the BMP180 module.
        /// </summary>
        /// <param name="resetMode">The reset mode, see <see cref="ResetModes"/> for more information.</param>
        /// <returns>True if the reset was successful  or otherwise false.</returns>
        /// <remarks>This module only supports <see cref="ResetModes.Soft"/> and will perform the same sequence as power on reset.</remarks>
        /// <exception cref="NotImplementedException">A <see cref="NotImplementedException"/> will be thrown is a <see cref="ResetModes.Hard"/> reset is attempted.</exception>
        /// <example>Example code to perform a Soft reset.
        /// <code language = "C#">
        /// _bmp180Sensor.Reset(ResetModes.Soft);
        /// </code>
        /// <code language = "VB">
        /// _bmp180Sensor.Reset(ResetModes.Soft)
        /// </code>
        /// </example>
        public bool Reset(ResetModes resetMode)
        {
            if (resetMode == ResetModes.Hard) throw new NotImplementedException("Hard Reset is not available for this module. Only Soft Reset is implemented.");

            _resetBusyPin.Write(true);

            //_isResetting = true; // We do not want to poll the sensor when it is resetting. Bad things could happen.

            WriteByte(0xE0, 0xB6);

            while (!IsConnected())
            {
                Thread.Sleep(5);
            }

            var returnValue = GetCalibrationData();

            //_isResetting = false;

            _resetBusyPin.Write(false);

            return returnValue;
        }

        #endregion

        #region Events

        /// <summary>
        ///     The MeasurementComplete Event Handler.
        /// </summary>
        public event MeasurementCompletetEventHandler MeasurementComplete;

        /// <summary>
        /// The event delegate method that is used by the <see cref="MeasurementComplete"/> event.
        /// </summary>
        /// <param name="sender">The BMP180 module that raised the event.</param>
        /// <param name="sensorData">The sensor data returned by the device.</param>
        public delegate void MeasurementCompletetEventHandler(object sender, SensorData sensorData);

        /// <summary>
        /// The class that represents the sensor data of the BMP Device - Temperature in °C, Uncompensated Pressure in Pascals (Pa), Sea Level Compensated (SLC) Pressure in Pascals (Pa) and Altitude in meters.
        /// </summary>
        public class SensorData
        {
            /// <summary>
            /// Returns the current temperature reading in °C.
            /// </summary>
            public float Temperature { get; private set; }

            /// <summary>
            /// Returns the current uncompensated Pressure in Pascals.
            /// </summary>
            public float RawPressure { get; private set; }

            /// <summary>
            /// Returns the current Sea Level compensated Pressure in Pascals.
            /// </summary>
            public float CompensatedPressure { get; private set; }

            /// <summary>
            /// Returns the Altitude in meters.
            /// </summary>
            public int Altitude { get; set; }

            /// <summary>
            /// This is an internal class used to return sensor data via the MeasurementComplete Event Handler.
            /// </summary>
            /// <param name="temperature">The temperature.</param>
            /// <param name="rawPressure">the raw pressure as read from the module in Pascals.</param>
            /// <param name="compensatedPressure">The compensated pressure in Pascals.</param>
            /// <param name="altitude">The Altitude in meters.</param>
            protected internal SensorData(float temperature, float rawPressure, float compensatedPressure, int altitude)
            {
                Temperature = temperature;
                RawPressure = rawPressure;
                CompensatedPressure = compensatedPressure;
                Altitude = altitude;
            }

            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            /// <returns>
            /// A string that represents the current object.
            /// </returns>
            public override string ToString()
            {
                var sensorDataString = "Temperature - " + Temperature + "  °C, Uncompensated Pressure - " + RawPressure + " Pa, Sea Level Compensated Pressure - " + CompensatedPressure + " Pa, Altitude  - " + Altitude + " meters";
                return sensorDataString;
            }

        }
        #endregion

    }
}