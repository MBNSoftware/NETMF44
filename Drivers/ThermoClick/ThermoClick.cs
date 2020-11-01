/*
 * THERMO Click Driver for MikroBus.Net
 * 
 * Version 1.0 - using Software SPI through Bit-Banging
 *  - Initial version coded Stephen Cardinale
 *
 * Version 1.1
 *  - Revision to include Hardware SPI
 *
 * Version 2.0 :
 *  - General refactoring and code cleanup.
 *  - Removed Bit-Banging Code (software SPI).
 *  - Integration of the new name spaces and new organization.
 *
 * Version 3.0
 *  - Now correctly returns negative temperatures.
 * 
 * References Needed :
 * Microsoft.Spot.Hardware
 * Microsoft.spot.Native
 * MikroBusNet
 * mscorlib
 * 
 * Copyright 2015 Stephen Cardinale and MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License. 
 */

using MBN.Enums;
using MBN.Exceptions;
using MBN.Extensions;
using Microsoft.SPOT.Hardware;
using System;
using System.Reflection;

namespace MBN.Modules
{
// ReSharper disable once InconsistentNaming
    /// <summary>
    /// Main class for the MikroE Thermo Click board driver
    /// <para><b>This module is a SPI Device.</b></para>
    /// <para><b>Pins used :</b> Miso, Mosi, Cs, Sck</para>
    /// <para><b>References required:</b> MikroBus.Net, Microsoft.SPOT.Native, Microsoft.Spot.Hardware, mscorlib</para>
    /// </summary>
    /// <example>Example usage:
    /// <code language = "C#">
    /// using MBN.Modules;
    /// using Microsoft.SPOT;
    /// using MBN;
    /// using System.Threading;
    /// using MBN.Enums;
    /// 
    /// namespace Examples
    /// {
    ///     public class Program
    ///     {
    ///         static ThermoClick _thermoClick;
    /// 
    ///         public static void Main()
    ///         {
    ///             _thermoClick = new ThermoClick(Hardware.SocketOne);
    ///             _thermoClick.SensorFault += ThermoClickSensorFault;
    ///             _thermoClick.TemperatureUnit = TemperatureUnits.Celsius;
    /// 
    ///             while (true)
    ///             {
    ///                 Debug.Print("Thermocouple Temperature - " + _thermoClick.ReadTemperature(TemperatureSources.Object) + " °C");
    ///                 Debug.Print("Cold Junction Temperature - " + _thermoClick.ReadTemperature() + " °C\n");
    ///                 Thread.Sleep(3000);
    ///             }
    ///         }
    /// 
    ///         static void ThermoClickSensorFault(object sender, ThermoClick.FaultType e)
    ///         {
    ///             var err = string.Empty;
    ///             switch (e)
    ///             {
    ///                 case ThermoClick.FaultType.OpenFault:
    ///                     err = "Thermocouple is Open.";
    ///                     break;
    ///                 case ThermoClick.FaultType.ShortToGround:
    ///                     err = "Thermocouple is shorted to Ground.";
    ///                     break;
    ///                 case ThermoClick.FaultType.ShortToVcc:
    ///                     err = "Thermocouple is shorted to VCC.";
    ///                     break;
    ///             }
    ///             Debug.Print(err);
    ///         }
    ///     }
    /// }
    /// </code>
    /// <code language = "VB">
    /// Option Explicit On
    /// Option Strict On
    ///
    /// Imports MBN.Enums
    /// Imports MBN
    /// Imports Microsoft.SPOT
    /// Imports MBN.Modules
    /// Imports System.Threading
    ///
    /// Namespace Examples
    ///
    ///     Public Module Module1
    ///
    ///         Dim WithEvents thermo As ThermoClick
    ///
    ///         Sub Main()
    ///
    ///             thermo = New ThermoClick(Hardware.SocketOne)
    ///             thermo.TemperatureUnit = TemperatureUnits.Celsius
    ///
    ///             While (True)
    ///                 Debug.Print("Thermocouple Temperature - " <![CDATA[&]]> thermo.ReadTemperature(TemperatureSources.Object) <![CDATA[&]> " °C")
    ///                 Debug.Print("Cold Junction Temperature - " <![CDATA[&]]> _thermo.ReadTemperature() <![CDATA[&]]> " °C")
    ///                 Thread.Sleep(3000)
    ///             End While
    ///
    ///         End Sub
    ///
    ///         Private Sub thermo_SensorFault(sender As Object, e As ThermoClick.FaultType) Handles thermo.SensorFault
    ///             Dim err As String = String.Empty
    ///             Select (e)
    ///                 Case ThermoClick.FaultType.ThermocoupleOpenFault
    ///                     err = "Thermocouple is Open."
    ///                 Case ThermoClick.FaultType.ThermocoupleShortToGround
    ///                     err = "Thermocouple is shorted to Ground."
    ///                 Case ThermoClick.FaultType.ThermocoupleShortToVcc
    ///                     err = "Thermocouple is shorted to VCC."
    ///             End Select
    ///             Debug.Print(err)
    ///         End Sub
    ///     End Module
    ///
    /// End Namespace
    /// </code>
    /// </example>
    public partial class ThermoClick : IDriver, ITemperature
    {
        #region Fields
        
        private TemperatureUnits _temperatureUnit = TemperatureUnits.Celsius;

        private const uint ClockRateKHz = 5000;
        private static SPI.Configuration spiConfiguration;  

        #endregion

        #region CTOR
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ThermoClick"/> class.
        /// </summary>
        /// <param name="socket">The socket of the MBN board that the THERMO is connected to.</param>
        public ThermoClick(Hardware.Socket socket)
        {
            try
            {
                Hardware.CheckPins(socket, socket.Miso, socket.Mosi, socket.Cs, socket.Sck);
                spiConfiguration = new SPI.Configuration(socket.Cs, false, 1, 1, false, true, ClockRateKHz, socket.SpiModule);

                if (Hardware.SPIBus == null)
                {
                    Hardware.SPIBus = new SPI(spiConfiguration);
                }
            }
            // Catch only the PinInUse exception, so that program will halt on other exceptions and send it directly to caller.
            catch (PinInUseException ex)
            {
                throw new PinInUseException(ex.Message);
            }
        }

        #endregion

        #region Private Methods    
    
        private ThermoClickSensorData ReadData()
        {
            var sensorBytes = new byte[4];

            Hardware.SPIBus.WriteRead(spiConfiguration, new Byte[] { 0xFF}, sensorBytes);

            // Check for sensor faults
            FaultType sensorFaults = CheckForSensorFaults(sensorBytes);
            var fault = FaultType.NoFault;

            if (sensorFaults == FaultType.NoFault) return new ThermoClickSensorData(sensorBytes);

            switch (sensorFaults)
            {
                case FaultType.OpenFault:
                    fault = FaultType.OpenFault;
                    break;
                case FaultType.ShortToGround:
                    fault = FaultType.ShortToGround;
                    break;
                case FaultType.ShortToVcc:
                    fault = FaultType.ShortToVcc;
                    break;
            }

            // If we have a sensor fault raise the SensorFault Event and return nothing.
            SensorFaultEventHandler tempEvent = SensorFault;
            if (tempEvent != null) tempEvent(this, fault);
            return new ThermoClickSensorData(new byte[] {0xFF, 0xFF, 0xFF, 0xFF});
        }
      
        private static FaultType CheckForSensorFaults(byte[] data)
        {
            var retVal = FaultType.NoFault;
            if ((data[1] & 0x01) == 0)
            {
                retVal = FaultType.NoFault;
            }
            else if ((data[3] & 0x04) != 0)
            {
                retVal = FaultType.ShortToVcc;
            }
            else if ((data[3] & 0x02) != 0)
            {
                retVal = FaultType.ShortToGround;
            }
            else if ((data[3] & 0x01) != 0)
            {
                retVal = FaultType.OpenFault;
            }
            return retVal;
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
        /// Debug.Print("Driver Version Info : " + _thermo.DriverVersion);
        /// </code>
        /// <code language="VB">
        /// Debug.Print("Driver Version Info : " <![CDATA[&]]> _thermo.DriverVersion.ToString())
        /// </code>
        /// </example>
        public Version DriverVersion
        {
            get { return Assembly.GetAssembly(GetType()).GetName().Version; }
        }

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <remarks>
        /// If the module has no power modes, then GET should always return PowerModes.ON while the SET Accessor will do nothing.
        /// </remarks>
        /// <example>None: PowerMode is not supported by this module.</example>
        public PowerModes PowerMode
        {
            get { return PowerModes.On; }
            set { }
        }

        /// <summary>
        /// Gets the raw data of the temperature value.
        /// </summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <example>None: RawData not implemented for this module.</example>
        public int RawData
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the temperature unit for the <seealso cref="ReadTemperature"/> method.
        /// </summary>
        /// <value>
        /// The temperature unit used.
        /// </value>
        /// <example>
        /// <code language="C#">
        /// // Set temperature unit to Fahrenheit
        /// _thermo.TemperatureUnit = TemperatureUnits.Farhenheit;
        /// </code>
        /// <code language="VB">
        /// ' Set temperature unit to Fahrenheit
        /// _thermo.TemperatureUnit = TemperatureUnits.Farhenheit
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
        /// Resets the module
        /// </summary>
        /// <param name="resetMode">The reset mode : 
        /// <para>SOFT reset : generally by sending a software command to the chip</para>
        /// <para>HARD reset : generally by activating a special chip's pin</para></param>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <example>None: Reset methods are not supported by this module.</example>
        public Boolean Reset(ResetModes resetMode)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads the temperature.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        /// A single representing the temperature read from the source, in the unit specified by the TemperatureUnit property.
        /// </returns>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("Thermocouple Temperature - " + _thermoClick.ReadTemperature(TemperatureSources.Object) + "°C");
        /// Debug.Print("Internal IC Temp - " + _thermoClick.ReadTemperature() + "°C");
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Thermocouple Temperature - " <![CDATA[&]]> _thermoClick.ReadTemperature(TemperatureSources.Object) <![CDATA[&]]> "°C");
        /// Debug.Print("Internal IC Temp - " <![CDATA[&]]> _thermoClick.ReadTemperature() <![CDATA[&]]> "°C");
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            var dataTmp = ReadData();

            if (source == TemperatureSources.Ambient)
            {
                return _temperatureUnit == TemperatureUnits.Celsius ? dataTmp.ColdJunctionTemperature : _temperatureUnit == TemperatureUnits.Fahrenheit ? (Single)((dataTmp.ColdJunctionTemperature * (9.0 / 5)) + 32) : (Single)(dataTmp.ColdJunctionTemperature + 273.15);
            }
            return _temperatureUnit == TemperatureUnits.Celsius ? dataTmp.ThermocoupleTemperature : _temperatureUnit == TemperatureUnits.Fahrenheit ? (Single)((dataTmp.ThermocoupleTemperature * (9.0 / 5)) + 32) : (Single)(dataTmp.ThermocoupleTemperature + 273.15);
        }

        #endregion

    }
}