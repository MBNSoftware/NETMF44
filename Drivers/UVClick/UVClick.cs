/*
 * UV Click MBN Driver
 * 
 * Version 1.0 :
 *  - Initial release coded by Stephen Cardinale
 *
 * References needed :
 *   - Microsoft.SPOT.Hardware
 *   - Microsoft.SPOT.Native
 *   - MikroBus.Net
 *   - mscorlib
 *  
 * Copyright 2014 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System;
using System.Reflection;
using System.Threading;
using MBN.Enums;
using MBN.Exceptions;
using MBN.Extensions;
using Microsoft.SPOT.Hardware;

namespace MBN.Modules
{
    /// <summary>
    /// MicroBusNet Driver for the UV Click board by MikroElektronika.
    /// <para><b>The CurrentClick is a SPI Device</b></para>
    /// <para><b>Pins used :</b> Miso, Mosi, Cs, Sck, Rst</para>
    /// <para><b>References required:</b> MikroBus.Net, Microsoft.SPOT.Native, Microsoft.Spot.Hardware, mscorlib</para>
    /// </summary>
    /// <example>Example usage:
    /// <code language = "C#">
    /// using MBN;
    /// using MBN.Enums;
    /// using MBN.Modules;
    /// using Microsoft.SPOT;
    /// using System.Threading;
    /// namespace UVClickDemo
    /// {
    ///     public class Program
    ///     {
    ///         private static UVClick _uv;
    ///
    ///         public static void Main()
    ///         {
    ///             _uv = new UVClick(Hardware.SocketFour);
    ///
    ///             new Thread(Capture).Start();
    ///
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    ///
    ///         private static void Capture()
    ///         {
    ///             while (true)
    ///             {
    ///                 Debug.Print("V - " + _uv.ReadVoltage());
    ///                 Debug.Print("UV Intensity - " + _uv.ReadUVIntensity() + "\n");
    ///                 Thread.Sleep(1000);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// <code language = "VB">
    /// Option Explicit On
    /// Option Strict On
    ///
    /// Imports Microsoft.SPOT
    /// Imports MBN
    /// Imports MBN.Modules
    /// Imports System.Threading
    ///
    /// Namespace Examples
    ///
    ///     Public Module Module1
    ///
    ///         Dim _uv As UVClick
    ///
    ///         Sub Main()
    ///             _uv = new UVClick(Hardware.SocketFour)
    ///
    ///             Dim captureThread As Thread = New Thread(New ThreadStart(AddressOf Capture))
    ///             captureThread.Start()
    ///
    ///             Thread.Sleep(Timeout.Infinite)
    ///
    ///         End Sub
    ///
    ///         Private Sub Capture()
    ///             While (True)
    ///                 Debug.Print("V - " <![CDATA[&]]> _uv.ReadVoltage())
    ///                 Debug.Print("UV Intensity - " <![CDATA[&]]> _uv.ReadUVIntensity() <![CDATA[&]]> Microsoft.VisualBasic.Constants.vbCrLf)
    ///                 Thread.Sleep(1000)
    ///             End While
    ///         End Sub
    ///
    ///     End Module
    ///
    /// End Namespace
    /// </code>
    /// </example>
    public class UVClick : IDriver
    {

        #region Fields
        
        private readonly SPI.Configuration _spiConfig;
        private readonly OutputPort _enablePin;
        private int _numberOfSamples = 20; 

        #endregion

        #region CTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="UVClick"/> class.
        /// </summary>
        /// <param name="socket">The socket in which the UV Click board is inserted into.</param>
        /// <exception cref="PinInUseException">A <see cref="PinInUseException"/> will be thrown if the  Miso, Mosi, Sck, Cs, Rst pins are already in use in a stacked module arrangement.</exception>
        public UVClick(Hardware.Socket socket)
        {
            try
            {
                Hardware.CheckPins(socket, socket.Miso, socket.Mosi, socket.Cs, socket.Sck, socket.Rst);

                _spiConfig = new SPI.Configuration(socket.Cs, false, 0, 0, true, true, 10000, socket.SpiModule);

                if (Hardware.SPIBus == null)
                {
                    Hardware.SPIBus = new SPI(_spiConfig);
                }

                _enablePin = new OutputPort(socket.Rst, true);

            }
            // Catch only the PinInUse exception, so that program will halt on other exceptions
            // Send it directly to caller
            catch (PinInUseException ex)
            {
                throw new PinInUseException(ex.Message);
            }
        }

        #endregion

        #region Private Methods
        
        private int ReadAdc()
        {
            var average = 0;
            var buffer = new Byte[2]; // Array containing measure from the MCP3201 ADC
            for (var i = 0; i < _numberOfSamples; i++) // Read  n samples for smoothing.
            {
                Hardware.SPIBus.WriteRead(_spiConfig, new Byte[] { 0xFF, 0xFF }, buffer);
                var tempValue = ((0x1F & buffer[0]) << 7) | (buffer[1] >> 1);
                average += tempValue;
                Thread.Sleep(5);
            }
            return average / _numberOfSamples;
        }

        private static float MapFloat(float x, float inMin, float inMax, float outMin, float outMax)
        {
            return x < 0.99F ? 0.0F : (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads the UV Intensity in mW/cm^2. 
        /// </summary>
        /// <example>
        /// <code language = "C#">
        /// Debug.Print("UV Intensity - " + _uv.ReadUVIntensity());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("UV Intensity - " <![CDATA[&]]> _uv.ReadUVIntensity())
        /// </code>
        /// </example>
        public byte ReadUVIntensity()
        {
                return (byte)MapFloat(ReadVoltage(), 0.99F, 2.9F, 0.0F, 15.0F);
        }
        
        /// <summary>
        ///  Gets the raw voltage of the ML8511 Photo-diode is currently measuring when compared to the Reference Voltage of MCP3201(3.3V) on the UV Click.
        /// </summary>
        /// <example>Example usage:
        /// <code> language = "C#">
        /// Debug.Print("UV Volts - " + _uv.ReadVoltage() + "V");
        /// </code>
        /// <code language = "VB">
        ///  Debug.Print("UV Volts - " <![CDATA[&]]> _uc.ReadVoltage() <![CDATA[&]]> "V")
        /// </code>
        /// </example>
        public Single ReadVoltage()
        {
            return 3.3F * (ReadAdc() / 4095F);
        }

        /// <summary>
        /// Resets the UV Click.
        /// </summary>
        /// <param name="resetMode">The reset mode, see <see cref="ResetModes"/> for more information.</param>
        /// <remarks>
        /// This module has no Reset method, calling this method will throw an exception.
        /// </remarks>
        /// <exception cref="NotImplementedException">This module does not implement a reset method. Calling this method will throw a <see cref="NotImplementedException"/></exception>
        /// <example>None: This module does not support a Reset method.</example>
        public bool Reset(ResetModes resetMode)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Public Properties
        
        /// <summary>
        /// Gets the driver version.
        /// </summary>
        /// <value>
        /// The driver version see <see cref="Version"/>.
        /// </value>
        /// <example>Example usage to get the Driver Version in formation:
        /// <code language="C#">
        /// Debug.Print("Driver Version Info : " + _uv.DriverVersion);
        /// </code>
        /// <code language="VB">
        /// Debug.Print("Driver Version Info : " <![CDATA[&]]> _uv.DriverVersion)
        /// </code>
        /// </example>
        public Version DriverVersion
        {
            get { return Assembly.GetAssembly(GetType()).GetName().Version; }
        }

        /// <summary>
        /// The number of samples used when averaging the value read from the UV Click for smoothing out erroneous readings.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _uv.NumberOfSamples = 50;
        /// </code>
        /// <code language = "VB">
        /// _uv.NumberOfSamples = 20
        /// </code>
        /// </example>
        public int NumberOfSamples
        {
            get { return _numberOfSamples; }
            set { _numberOfSamples = value; }
        }

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <remarks>
        /// This module supports both <see cref="PowerModes.On"/> and <see cref="PowerModes.Off"/>, the SET accessor with <see cref="PowerModes.Low"/> will do nothing.
        /// </remarks>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _uv.PowerMode = PowerModes.Off;
        /// Debug.Print("Power Mode  - " + _uv.PowerMode);
        /// </code>
        /// <code language = "VB">
        /// _uv.PowerMode = PowerModes.Off
        /// Debug.Print("Power Mode  - " <![CDATA[&]]> _uv.PowerMode)
        /// </code>
        /// </example>
        public PowerModes PowerMode
        {
            get
            {
                return _enablePin.Read() ? PowerModes.On : PowerModes.Off;
            }
            set
            {
                if (value == PowerModes.Low) return;
                _enablePin.Write(value != PowerModes.Off);
            }
        }

        #endregion
    }
}

