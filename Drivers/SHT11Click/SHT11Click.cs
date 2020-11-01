/* Version 1.0
 *  - Initial version coded by Stephen Cardinale
 *  - Not implemented - NO_RELOAD_FROM_OTP_MASK and HEATER_MASK
 *  Version 2.0
 *  - Complete re-write and refactoring.
 *  - Bug Fix - Corrected a bug that would report incorrect return value on ResetModes.Soft
 *  - Implementation of the MikroBusNet Interface requirements.
 *  - Implemented NoReloadFromOTP - no reload of calibration data from One Time Programmable Memory before measurements.
 *  - Implemented enabling the On-Board IC Heater Capabilities useful for testing sensor diagnostics.
 *  - Implemented EndOfBattery property for low voltage detection.
 *  - Implemented Alarm functionality that is raised through the TemperatureHumidityMeasured Event and Low and High Temperature and Humidity Alarm Threshold Properties. 
 *  
 *  Version 2.0 :
 *  - Integration of the new namespaces and new organization.
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
using System;
using Microsoft.SPOT.Hardware;
using System.Reflection;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the MikroE Thermo Click board driver. This driver will also work for the SHT10, SHT11 and SHT15 in a NETMF design (non-click).
    /// <para><b>Pins used :</b> Scl, Sda  WARNING : this is NOT a real I²C module so it will prevent other real I²C modules from working!</para>
    /// <para><b>References required:</b> MikroBus.Net, Microsoft.SPOT.Native, Microsoft.Spot.Hardware, mscorlib</para>
    /// </summary>
    /// <example>Example usage:
    /// <code language = "C#">
    /// using System;
    /// using System.Threading;
    /// using MBN;
    /// using MBN.Enums;
    /// using MBN.Exceptions;
    /// using MBN.Extensions;
    /// using MBN.Modules;
    /// using MBN.Utilities;
    /// using Microsoft.SPOT;
    ///
    /// namespace Examples
    /// {
    ///     public class Program
    ///     {
    ///         private static SHT11Click _sht11;
    ///
    ///         public static void Main()
    ///         {
    ///             try
    ///             {
    ///                 _sht11 = new SHT11Click(Hardware.SocketFour);
    ///                 _sht11.TemperatureHumidityMeasured += TemperatureHumidityMeasured;
    ///             }
    ///
    ///             catch (PinInUseException ex) // Some pins are already used by another driver
    ///             {
    ///                 Debug.Print("Some pins are in use while creating instances : " + ex.Message);
    ///                 Debug.Print("Stack trace : " + ex.StackTrace);
    ///             }
    ///
    ///             catch (DeviceInitialisationException ex)
    ///             {
    ///                 Debug.Print("Exception during device initialization : " + ex.Message);
    ///             }
    ///
    ///             catch (Exception ex) // Other exception from NETMF core
    ///             {
    ///                 Debug.Print("Exception while creating instances : " + ex.Message);
    ///             }
    ///
    ///             _sht11.SensorResolution = SHT11Click.Resolution.High;
    ///
    ///             Debug.Print("Setting AlarmThreshold to trigger High Temperature and High Humidity Alarms.");
    ///             _sht11.AlarmThresholds = new AlarmThresholds(-40, -39, 0, 1);
    ///
    ///             Thread.Sleep(5000);
    ///
    ///             new Thread(Capture).Start();
    ///
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    ///
    ///
    ///         static void TemperatureHumidityMeasured(object sender, SHT11Click.TemperatureHumidityEventArgs e)
    ///         {
    ///
    ///             string alarmsPresent = "SHT11Alarms Present : ";
    ///
    ///             if (e.SHT11Alarms.ContainsFlag(SHT11Click.SHT11Alarms.NoAlarm)) alarmsPresent += "No Alarms present";
    ///             if ((e.SHT11Alarms.ContainsFlag(SHT11Click.SHT11Alarms.TemperatureLow))) alarmsPresent += "Low Temperature";
    ///             if ((e.SHT11Alarms.ContainsFlag(SHT11Click.SHT11Alarms.TemperatureHigh))) alarmsPresent += "High Temperature";
    ///             if ((e.SHT11Alarms.ContainsFlag(SHT11Click.SHT11Alarms.HumidityLow))) alarmsPresent += ", Low Humidity";
    ///             if ((e.SHT11Alarms.ContainsFlag(SHT11Click.SHT11Alarms.HumidityHigh))) alarmsPresent += ", High Humidity";
    ///
    ///             Debug.Print("Temperature - " + e.Temerature.ToString("f2") + " °C");
    ///             Debug.Print("Humidity - " + e.Humidity.ToString("f2") + " %RH");
    ///             Debug.Print("ReadRaw (Temperature) - " + (_sht11 as ITemperature).RawData);
    ///             Debug.Print("ReadRaw (Humidity) - " + (_sht11 as ITemperature).RawData);
    ///             Debug.Print("Dew Point - " + Humidity.CalculateDewPoint(e.Temerature, e.Humidity).ToString("f2") + " °C");
    ///             Debug.Print(alarmsPresent);
    ///             Debug.Print("---------------------------------------------------\n");
    ///         }
    ///
    ///         private static void Capture()
    ///         {
    ///             while (true)
    ///             {
    ///                 _sht11.ReadTemperatureHumidity();
    ///                 Thread.Sleep(2000);
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
    /// Imports MBN.Exceptions
    /// Imports MBN.Extensions
    /// Imports MBN.Modules
    /// Imports Microsoft.SPOT
    /// Imports System
    /// Imports System.Threading
    ///
    /// Namespace Examples
    ///
    ///     Public Module Module1
    ///
    ///         Dim WithEvents _sht11 As SHT11Click
    ///
    ///         Sub Main()
    ///             Try
    ///                 _sht11 = New SHT11Click(Hardware.SocketFour)
    ///
    ///             Catch ex As PinInUseException
    ///                 ' Some pins are already used by another driver
    ///                 Debug.Print("Some pins are in use while creating instances : " <![CDATA[&]]> ex.Message)
    ///                 Debug.Print("Stack trace : " <![CDATA[&]]> ex.StackTrace)
    ///
    ///             Catch ex As DeviceInitialisationException
    ///                 Debug.Print("Exception during device initialization : " <![CDATA[&]]> ex.Message)
    ///
    ///             Catch ex As Exception
    ///                 ' Other exception from NETMF core
    ///                 Debug.Print("Exception while creating instances : " <![CDATA[&]]> ex.Message)
    ///             End Try
    ///
    ///             _sht11.SensorResolution = SHT11Click.Resolution.High
    ///
    ///             Debug.Print("Setting AlarmThreshold to trigger High Temperature and High Humidity Alarms.")
    ///             _sht11.AlarmThresholds = New AlarmThresholds(-40, -39, 0, 1)
    ///
    ///             Thread.Sleep(5000)
    ///
    ///             Dim captureThread As Thread = New Thread(New ThreadStart(AddressOf Capture))
    ///             captureThread.Start()
    ///
    ///             Thread.Sleep(Timeout.Infinite)
    ///
    ///         End Sub
    ///
    ///         Private Sub Capture()
    ///             While True
    ///                 _sht11.ReadTemperatureHumidity()
    ///                 Thread.Sleep(2000)
    ///             End While
    ///         End Sub
    ///
    ///         Private Sub _sht11_TemperatureHumidityMeasured(sender As Object, e As SHT11Click.TemperatureHumidityEventArgs) Handles _sht11.TemperatureHumidityMeasured
    ///             Dim alarmsPresent As String = "SHT11Alarms Present : "
    ///
    ///             If e.Sht11Alarms.ContainsFlag(SHT11Click.SHT11Alarms.NoAlarm) Then
    ///                 alarmsPresent += "No Alarms present"
    ///             End If
    ///             If (e.Sht11Alarms.ContainsFlag(SHT11Click.SHT11Alarms.TemperatureLow)) Then
    ///                 alarmsPresent += "Low Temperature"
    ///             End If
    ///             If (e.Sht11Alarms.ContainsFlag(SHT11Click.SHT11Alarms.TemperatureHigh)) Then
    ///                 alarmsPresent += "High Temperature"
    ///             End If
    ///             If (e.Sht11Alarms.ContainsFlag(SHT11Click.SHT11Alarms.HumidityLow)) Then
    ///                 alarmsPresent += ", Low Humidity"
    ///             End If
    ///             If (e.Sht11Alarms.ContainsFlag(SHT11Click.SHT11Alarms.HumidityHigh)) Then
    ///                 alarmsPresent += ", High Humidity"
    ///             End If
    ///
    ///             Debug.Print("Temperature - " <![CDATA[&]]> e.Temerature.ToString("f2") <![CDATA[&]]> " °C")
    ///             Debug.Print("Humidity - " <![CDATA[&]]> e.Humidity.ToString("f2") <![CDATA[&]]> " %RH")
    ///             Debug.Print("ReadRaw (Temperature) - " <![CDATA[&]]> TryCast(_sht11, ITemperature).RawData.ToString())
    ///             Debug.Print("ReadRaw (Humidity) - " + TryCast(_sht11, ITemperature).RawData.ToString())
    ///             Debug.Print("Dew Point - " <![CDATA[&]]> MBN.Utilities.Humidity.CalculateDewPoint(e.Temerature, e.Humidity).ToString("f2") <![CDATA[&]]> " °C")
    ///             Debug.Print(alarmsPresent)
    ///             Debug.Print("---------------------------------------------------")
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///         End Sub
    ///     End Module
    ///
    /// End Namespace 
    /// </code>
    /// </example>
// ReSharper disable once InconsistentNaming
    public partial class SHT11Click : IDriver, ITemperature, IHumidity
    {
        #region Constants

        private const byte ADDR = 0x00;

        // If the SHT11 click board is modified to use 5V (POW SEL Jumper), change the following line to "private const float VDD = 5.0F".
        private const float VDD = 3.3F;

        #endregion

        #region Fields ...

        private static OutputPort _clkPort; // output port for clock
        private static TristatePort _dataPort; // tristate port for input/output for data 

        // This timer is used to turn off the Heater in case the no default duration is set in the call to the SetHeater method. Defaults to 15 minutes.
        private static Timer _timer;
        private static bool _reading;

        #endregion

        #region ENUMs

        /// <summary>
        ///     The measurement resolution for the SHT11 IC. The conversion times are 20/80/320 ms for a 8/12/14 bit.
        /// </summary>
        public enum Resolution
        {
            /// <summary>
            ///     High resolution 14 bit Temperature and 12 bit Humidity. With a conversion time of 320 ms for temperature and 80 ms for humidity. Accuracy is 0.01°C for temperature and 0.05 % Relative Humidity. 
            /// </summary>
            High = 0x00,

            /// <summary>
            ///     Low resolution 12 bit Temperature and 8 bit Humidity. With a conversion time of 80 ms for temperature and 20 ms for humidity. Accuracy is 0.04°C for temperature and 0.4 % Relative Humidity. 
            /// </summary>
            Low = 0x01
        }

        /// <summary>
        ///     Enumeration to control the state of the on-board heater on the SHT11 IC.
        /// </summary>
        public enum HeaterStatus
        {
            /// <summary>
            ///     Internal IC Heater circuitry is turned off. 
            /// </summary>
            Off,

            /// <summary>
            ///     Internal IC Heater circuitry is turned on. 
            /// </summary>
            On
        }

        internal enum Commands
        {
            MeasureTemperature = 0x03,
            MeasureRelativeHumidity = 0x05,
            ReadStatusRegister = 0x07,
            WriteStatusRegister = 0x06,
            SoftReset = 0x1E
        }

        #endregion

        #region CTOR

        /// <summary>
        ///     Default Constructor.
        /// </summary>
        /// <param name="socket">The <see cref="Hardware.Socket"/> that the SHT11 Click is inserted in.</param>
        public SHT11Click(Hardware.Socket socket)
        {
            try
            {
                Hardware.CheckPins(socket, socket.Sda, socket.Scl);

                _dataPort = new TristatePort(socket.Sda, false, true, Port.ResistorMode.PullDown) {Active = true}; 
                _clkPort = new OutputPort(socket.Scl, false);

                // Reset the SHT11 click and throw DeviceInitialisationException on failure. This will also reset the SHT11 Click to Factory Default Settings in the StatusRegister.
                if (!Reset(ResetModes.Soft)) throw new DeviceInitialisationException("Failed to initialize the SHT11 Click.");

                _timer = new Timer(HeaterControlDelegate, null, -1, -1);

                AlarmThresholds = new AlarmThresholds(-40F, 123.8F, 0F, 100F);
            }
                // Catch only the PinInUse exception, so that program will halt on other exceptions and send it directly to caller.
            catch (PinInUseException ex) { throw new PinInUseException(ex.Message); }
        }

        #endregion

        #region Private Methods

        private void HeaterControlDelegate(object state)
        {
            while (_reading) // We can not write to the StatusRegister while an active conversion is going on.
            {
                Thread.Sleep(10);
            }
            SetHeater(HeaterStatus.Off);
        }

        private static bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        // Execute transmission start sequence for commands
        private static void StartTransmission()
        {
            if (!_dataPort.Active) _dataPort.Active = true;

            // transmission start sequence
            _dataPort.Write(true); 
            _clkPort.Write(true);
            _dataPort.Write(false);
            _clkPort.Write(false);
            _clkPort.Write(true);
            _dataPort.Write(true);
            _clkPort.Write(false);
        }

        // Sends a command to the sensor
        private static bool SendCommand(Commands command)
        {
            var send = (byte) (ADDR | command);

            if (!_dataPort.Active) _dataPort.Active = true;

            // transmission start sequence
            StartTransmission();

            return WriteByte(send);
        }

        // Read a byte from sensor
        private static byte ReadByte(bool sendAck)
        {
            byte resp = 0x00;

            if (_dataPort.Active) _dataPort.Active = false;

            // read bits from MSB
            for (int i = 7; i >= 0; i--)
            {
                _clkPort.Write(true);
                resp |= (_dataPort.Read()) ? (byte) (1 << i) : (byte) 0;
                _clkPort.Write(false);
            }

            // set data port as output
            _dataPort.Active = true;
            // send or not ACK
            _dataPort.Write(!sendAck);
            _clkPort.Write(true);
            _clkPort.Write(false);
            _dataPort.Write(true);

            return resp;
        }

        // Write a byte to the sensor
        private static bool WriteByte(byte data)
        {
            if (!_dataPort.Active) _dataPort.Active = true;

            // shift out bits from MSB
            for (int i = 7; i >= 0; i--)
            {
                var b = (byte) (data & (1 << i));

                _dataPort.Write(b != 0);
                _clkPort.Write(true);
                _clkPort.Write(false);
            }

            // set data port as input
            _dataPort.Active = false;
            // wait for ACK (data pin LOW in input)
            _clkPort.Write(true);
            bool ack = _dataPort.Read();
            _clkPort.Write(false);

            return ack;
        }

        private static int ReadRaw(Commands command)
        {
            _reading = true;
            SendCommand(command);

            if (_dataPort.Active) _dataPort.Active = false;

            // while sensor is measuring temp, waiting for LOW on data pin (measure finished)
            while (_dataPort.Read()) Thread.Sleep(10);

            var resp = 0;
            resp |= ReadByte(true);

            resp = resp << 8;
            resp |= ReadByte(false);

            _reading = false;

            return resp;
        }

        // Read the status register, returns then content of status register
        private static byte ReadStatusRegister()
        {
            SendCommand(Commands.ReadStatusRegister);
            byte status = ReadByte(false);

            return status;
        }

        //  Write the status register
        private static void WriteStatusRegister(byte status)
        {
            SendCommand(Commands.WriteStatusRegister);
            WriteByte(status);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or Sets the <see cref="AlarmThresholds"/> which are for alarm triggering.
        /// </summary>
        /// <remarks>This Property is only used when calling the <see cref="ReadTemperatureHumidity"/> method and is returned on the <see cref="TemperatureHumidityMeasured"/> Event.</remarks>
        /// <example>Example usage:
        /// <code language="C#">
        /// _sht11Click.AlarmThresholds = new AlarmThresholds(20F, 40F), 25, 50);
        /// </code>
        /// <code language="VB">
        /// _sht11Click.AlarmThresholds = new AlarmThresholds(20F, 40F), 25, 50)
        /// </code>
        /// </example>
        public AlarmThresholds AlarmThresholds { get; set; }

        /// <summary>
        /// Gets the driver version.
        /// </summary>
        /// <value>
        /// The driver version see <see cref="Version"/>.
        /// </value>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.Print("Driver Version Info : " + _sht11Click.DriverVersion);
        /// </code>
        /// <code language="VB">
        /// Debug.Print("Driver Version Info : " <![CDATA[&]]> _sht11Click.DriverVersion)
        /// </code>
        /// </example>
        public Version DriverVersion
        {
            get { return Assembly.GetAssembly(GetType()).GetName().Version; }
        }

        /// <summary>
        /// Gets the End of Battery (low voltage detection) status of the SHT11 IC. Detects and notifies of VDD voltages below 2.47V. Accuracy is ±0.05V.
        /// </summary>
        /// <returns>True is VDD is below 2.47, otherwise false.</returns>
        /// <remarks>The EndOfBattery Bit in the StatusRegister is only updated after an Temperature or humidity measurement is complete. This method is included for completeness of the driver. Usage is not practical as the SHT11 Click is powered by the MBN main board and will be supplied a constant 3.3V VDD. If the SHTclick is powered remotely, this might come in handy.</remarks>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.Print("End Of Battery : " + _sht11Click.EndOfBattery);
        /// </code>
        /// <code language="VB">
        /// Debug.Print("End Of Battery : " <![CDATA[&]]> _sht11Click.EndOfBattery);
        /// </code>
        /// </example>
        public bool EndOfBattery
        {
            get { return IsBitSet(ReadStatusRegister(), 6); }
        }

        /// <summary>
        /// Sets the NoReloadFromOTP (One-time Programmable Memory) setting.
        /// </summary>
        /// <value>If set to true, deactivates reloading the calibration data before each measurement.</value>
        /// <remarks>For the SHT11 IC, the default is to uploaded the calibration data to the register before each measurement. This may be deactivated for reducing measurement time by about 10ms.</remarks>
        /// <exception cref="Exception">An <see cref="Exception"/> will be thrown is there is an error setting the NoReloadFromOTP bit to the StatusRegister of the SHT11 IC to the new value.</exception>
        /// <example>Example usage:
        /// <code language="C#">
        /// _sht11Click.NoReloadFromOTP = true;
        ///  Debug.Print("NoReloadFromOTP : " + _sht11Click.NoReloadFromOTP);
        /// </code>
        /// <code language="VB">
        /// _sht11Click.NoReloadFromOTP = true
        /// Debug.Print("NoReloadFromOTP : " <![CDATA[&]]> _sht11Click.NoReloadFromOTP);
        /// </code>
        /// </example>
        public bool NoReloadFromOTP
        {
            get { return IsBitSet(ReadStatusRegister(), 1); }
            set
            {
                var status = ReadStatusRegister();
                WriteStatusRegister(value ? Bits.Set(status, 1, true) : Bits.Set(status, 1, false));

                if ((value & !IsBitSet(ReadStatusRegister(), 1)) || (!value & IsBitSet(ReadStatusRegister(), 1)))
                {
                    throw new Exception("Error setting the NoReloadFromOTP bit in the StatusRegister.");
                }
            }
        }

        /// <summary>
        ///  Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <remarks>
        /// This module has no Power Modes, the GET accessor will always return PowerModes.On. See <see cref="PowerModes"/>, while the SET accessor will do nothing.
        /// </remarks>
        /// <exception cref="NotImplementedException"></exception>
        /// <example>None: This sensor does not support PowerMode.</example>
        public PowerModes PowerMode
        {
            get { return PowerModes.On; }
            set {  }
        }

        /// <summary>
        /// Gets the raw data of the humidity value.
        /// </summary>
        /// <value>
        /// The raw humidity data as read from the SHT11 Click.
        /// </value>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.Print("RawData (Humidity) - " + (_sht11Click as IHumidity).RawData);
        /// </code>
        /// <code language="VB">
        /// Debug.Print("RawData (Humidity) - " <![CDATA[&]]> (_sht11Click as IHumidity).RawData)
        /// </code>
        /// </example>
        int IHumidity.RawData
        {
            get { return ReadRaw(Commands.MeasureRelativeHumidity); }
        }

        /// <summary>
        /// Gets the raw data of the temperature value from the SHT11 Click.
        /// </summary>
        /// <value>
        /// The raw temperature data as read from the SHT11 Click.
        /// </value>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.Print("RawData (Temperature) - " + (_sht11Click as ITemperature).RawData);
        /// </code>
        /// <code language="VB">
        /// Debug.Print("RawData (Temperature) - " <![CDATA[&]]> (_sht11Click as ITemperature).RawData)
        /// </code>
        /// </example>
        int ITemperature.RawData
        {
            get { return ReadRaw(Commands.MeasureTemperature); }
        }

        /// <summary>
        /// Sets or Gets the <see cref="Resolution"/> of the SHT11 Click for measuring accuracy.
        /// </summary>
        /// <remarks>See <see cref="Resolution"/> for an explanation of this methodology and usage.</remarks>
        /// <value>Resolution either Low (12bit Temperature/8bit Humidity) or High (14bit Temperature/12bit Humidity). See <see cref="Resolution"/> for more information.</value>
        /// <exception cref="Exception">An <see cref="Exception"/> will be thrown is there is an error setting the resolution to the StatusRegister of the SHT11 IC to the new value.</exception>
        /// <example>Example usage:
        /// <code language="C#">
        /// _sht11Click.SensorResolution = SHT11Click.Resolution.Low;
        ///  Debug.Print("Sensor Resolution after changing resolution (0 = High, 1 = Low) : " + _sht11Click.SensorResolution);
        /// </code>
        /// <code language="VB">
        ///  _sht11Click.SensorResolution = SHT11Click.Resolution.Low
        ///  Debug.Print("Sensor Resolution after changing resolution (0 = High, 1 = Low) : " <![CDATA[&]]> _sht11Click.SensorResolution)
        /// </code>
        /// </example>
        public Resolution SensorResolution
        {
            get { return IsBitSet(ReadStatusRegister(), 0) ? Resolution.Low : Resolution.High; }
            set
            {
                var status = ReadStatusRegister();
                WriteStatusRegister(value == Resolution.Low ? Bits.Set(status, 0, true) : Bits.Set(status, 0, false));

                // Re-read the StatusRegister for verification.
                status = ReadStatusRegister();

                if ((value == Resolution.Low & !IsBitSet(status, 0)) || (value == Resolution.High & IsBitSet(status, 0)))
                {
                    throw new Exception("Error setting the resolution bit in the StatusRegister.");
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the Heater setting of the SHT11 Click.
        /// </summary>
        /// <returns>The current Heater status <see cref="HeaterStatus"/>. See <see cref="SetHeater"/> for an explanation of this methodology and usage.</returns>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.Print("Heater is -  " + _sht11Click.GetHeaterStatus() == SHT11Click.HeaterStatus.On ? "On" : "Off");
        /// </code>
        /// <code language="VB">
        /// Debug.Print(If("Heater is -  " <![CDATA[&]]> _sht11Click.GetHeaterStatus() = SHT11Click.HeaterStatus.[On], "On", "Off"))
        /// </code>
        /// </example>
        public HeaterStatus GetHeaterStatus()
        {
            return IsBitSet(ReadStatusRegister(), 2) ? HeaterStatus.On : HeaterStatus.Off;
        }

        /// <summary>
        /// Reads the (temperature compensated) humidity value from the SHT11 Click.
        /// </summary>
        /// <param name="source">The <see cref="HumidityMeasurementModes"/> to measure. The SHT11Click only supports <see cref="HumidityMeasurementModes.Relative"/> measurement.</param>
        /// <returns>
        /// A float representing the (temperature compensated) relative humidity as read from the sensor, in percentage (%).
        /// </returns>
        /// <exception cref="NotImplementedException">Will be raised if sources is <see cref="HumidityMeasurementModes.Absolute"/>.</exception>
        /// <example>Example usage:
        /// <code language="C#">
        ///     Debug.Print("Humidity - " + _sht11Click.ReadHumidity(HumiditySources.Relative));
        /// </code>
        /// <code language="VB">
        ///     Debug.Print("Humidity - " <![CDATA[&]]> _sht11Click.ReadHumidity(HumiditySources.Relative))
        /// </code>
        /// </example>
        public float ReadHumidity(HumidityMeasurementModes source = HumidityMeasurementModes.Relative)
        {
            if (source == HumidityMeasurementModes.Absolute) throw new NotImplementedException("The SHT11 Click does not provide Absolute Humidity measurement. Use HumiditySources.Relative for Humidity measurement.");

            // First read the Temperature to be used for temperature compensation.
            var temp = ReadTemperature();

            byte status = ReadStatusRegister();

            var sOrh = ReadRaw(Commands.MeasureRelativeHumidity);

            if (IsBitSet(status, 0)) //Low Resolution
            {
                var rHLinear = -2.0468F + 0.5872F*sOrh + -4.0845E-4F*sOrh*sOrh;
                return (temp - 25.0F)*(0.01F + 0.00128F*sOrh) + rHLinear;
            }

            var rHlinear = -2.0468F + 0.0367F*sOrh + -1.5955E-6F*sOrh*sOrh;
            return (temp - 25.0F)*(0.01F + 0.00008F*sOrh) + rHlinear;
        }

        /// <summary>
        /// Reads the temperature from the SHT11 Click.
        /// </summary>
        /// <param name="source">The <see cref="TemperatureSources"/> to measure. The SHT11Click only supports <see cref="TemperatureSources.Ambient"/> measurement.</param>>
        /// <returns>
        /// A float representing the current temperature read from the SHT11 Click in °C.
        /// </returns>
        /// <exception cref="NotImplementedException">Will be raised if sources is <see cref="TemperatureSources.Object"/>.</exception>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.Print("Temperature - " + _sht11Click.ReadTemperature(TemperatureSources.Ambient));
        /// </code>
        /// <code language="VB">
        /// Debug.Print("Temperature - " <![CDATA[&]]> _sht11Click.ReadTemperature(TemperatureSources.Ambient))
        /// </code>
        /// </example>
        public float ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (source == TemperatureSources.Object) throw new NotImplementedException("The SHT11 Click does not provide Object temperature measurement. Use TemperatureSources.Ambient for Humidity measurement.");

            // determine d2 based on resolution
            var status = ReadStatusRegister();
            var d2 = IsBitSet(status, 0) ? 0.04F : 0.01F;

            // Determine D1 based on VDD
            // The following formula (based on the slope of data in Table 8 of the DataSheet) is used to convert digital readout (sot) to actual temperature value.
            const float d1 = -0.28000000000000113f*VDD + -38.699999999999996f;

            return d1 + d2 * ReadRaw(Commands.MeasureTemperature);
        }

        /// <summary>
        /// Resets the SHT11 Click.
        /// </summary>
        /// <para>Soft reset, resets the interface, clears the status register to default values.</para>
        /// <para>Hard reset, If communication with the device is lost the following signal sequence will reset the serial interface while preserving the Status Registers.</para>
        /// <param name="resetMode">The reset mode either hard or soft. See <see cref="ResetModes"/> for more information.</param>
        /// <returns>True if the Reset was successful, otherwise false.</returns>
        /// <example>Example usage:
        /// <code language="C#">
        ///    while (true)
        ///    {
        ///        _sht11Click.ReadTemperatureHumidity();
        ///       Thread.Sleep(2000);
        ///    }
        /// </code>
        /// <code language="VB">
        ///     While True
        ///	        _sht11Click.ReadTemperatureHumidity()
        ///	        Thread.Sleep(2000)
        ///     End While
        /// </code>
        /// </example>
        public bool Reset(ResetModes resetMode)
        {
            if (resetMode == ResetModes.Soft)
            {
                var ack = SendCommand(Commands.SoftReset);

                // Wait a minimum 11 ms per datasheet
                Thread.Sleep(20);

                return !ack; //True on error, False on success
            }

            if (!_dataPort.Active) _dataPort.Active = true; // set data port as output

            _dataPort.Write(false);

            // Toggle clock signal 9 times. See datasheet.
            for (int i = 0; i < 9; i++)
            {
                _clkPort.Write(true);
                _clkPort.Write(false);
            }
            _dataPort.Write(true);

            return true;
        }

        /// <summary>
        /// Turns On or Off the internal heating element of the SHT11 IC.
        /// </summary>
        /// <param name="on">A Boolean indicating whether to turn On or Off the on-board heater.</param>
        /// <param name="duration">A Timespan to turn on the on-board heater.</param>
        /// <returns>True is successful, otherwise false.</returns>
        /// <remarks>
        /// The heater may increase the temperature of the sensor by 5 – 10°C beyond ambient temperature.
        /// For example the heater can be helpful for functional analysis: Humidity and temperature readings before and after applying the heater are compared.
        /// Temperature shall increase while relative humidity decreases at the same time. The Dew point shall remain the same.
        /// Caution: The temperature reading will display the temperature of the heated sensor element and not ambient temperature.
        /// Furthermore, the sensor is not qualified for continuous application of the heater. Use with caution.
        /// Therefore, if the duration is not specified, it will default to 15 minutes and automatically turn off the on-board heater. 
        /// </remarks>
        /// <example>Example usage:
        /// <code language="C#">
        /// _sht11Click.SetHeater(SHT11Click.HeaterStatus.On, new TimeSpan(0, 0, 0, 15));
        /// </code>
        /// <code language="VB">
        /// _sht11Click.SetHeater(SHT11Click.HeaterStatus.On, new TimeSpan(0, 0, 0, 15))
        /// </code>
        /// </example>
        public bool SetHeater(HeaterStatus on, TimeSpan duration = default(TimeSpan))
        {
            var status = ReadStatusRegister();
            WriteStatusRegister(on == HeaterStatus.On ? Bits.Set(status, 2, true) : Bits.Set(status, 2, false));

            if (on == HeaterStatus.On)
            {
                _timer.Change(duration == default(TimeSpan) ? new TimeSpan(0, 15, 0) : duration, duration == default(TimeSpan) ? new TimeSpan(0, 15, 0) : duration); //Default is 15 minutes.
            }
            else
            {
                _timer.Change(-1, -1); // Change the Timer due time and period.
            }

            return IsBitSet(ReadStatusRegister(), 2);
        }

        #endregion

        #region Event Handler

        /// <summary>
        ///     The event that is raised at completion of the <see cref="ReadTemperatureHumidity"/> method. 
        /// </summary>
        public event TemperatureMeasuredEventHandler TemperatureHumidityMeasured = delegate { };

        /// <summary>
        ///     Reads the Temperature and Humidity and associated <see cref="SHT11Alarms"/> from the SHT11 Click board and returns the values on the <see cref="TemperatureHumidityMeasured"/> Event.
        /// </summary>
        /// <returns><see cref="TemperatureHumidityMeasured"/> Event.</returns>
        public void ReadTemperatureHumidity()
        {
            var temperature = ReadTemperature();
            var humidity = ReadHumidity();
            var tempEvent = TemperatureHumidityMeasured;
            var alarmsPresent = CalculateAlarms(temperature, humidity);
            tempEvent(this, new TemperatureHumidityEventArgs(temperature, humidity, alarmsPresent));
        }

        private SHT11Alarms CalculateAlarms(float temperature, float humidity)
        {
            if (IsBetween(temperature, AlarmThresholds.LowTemperatureThreshold, AlarmThresholds.HighTemperatureThreshold) && IsBetween(humidity, AlarmThresholds.LowHumidityThreshold, AlarmThresholds.HighHumidityThreshold)) return SHT11Alarms.NoAlarm;
            var alarms = (SHT11Alarms)0;
            if (temperature < AlarmThresholds.LowTemperatureThreshold) alarms |= SHT11Alarms.TemperatureLow;
            if (temperature > AlarmThresholds.HighTemperatureThreshold) alarms |= SHT11Alarms.TemperatureHigh;
            if (humidity < AlarmThresholds.LowHumidityThreshold) alarms |= SHT11Alarms.HumidityLow;
            if (humidity > AlarmThresholds.HighHumidityThreshold) alarms |= SHT11Alarms.HumidityHigh;

            return alarms;
        }

        private bool IsBetween(float testNumber, float lowerRange, float upperRange)
        {
            return testNumber <= upperRange && testNumber >= lowerRange;
        }

        #endregion
    }
}