/*
 * Light Click board driver
 * 
 * Version 1.0 :
 *  - Initial revision coded by Stephen Cardinale
 *  Version 2.0 :
 *  - Updated to implement new Namespaces and Interface requirements.
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
using MBN.Extensions;
using System;
using System.Threading;
using Microsoft.SPOT.Hardware;

// ReSharper disable once CheckNamespace
namespace MBN.Modules
{
    /// <summary>
    /// Main class for the MikroE LightClick board
    /// <para><b>The LightClick is a SPI Device</b></para>
    /// <para><b>Pins used :</b> Miso, Mosi, Cs, Sck</para>
    /// <para><b>References required:</b> MikroBus.Net, LightClick, Microsoft.SPOT.Native, Microsoft.Spot.Hardware, mscorlib</para>
    /// </summary>
    /// <example>
    /// <code language = "C#">
    /// using MBN;
    /// using MBN.Modules;
    /// using Microsoft.SPOT;
    /// using System.Threading;
    ///
    /// namespace Examples
    /// {
    ///     public class Program
    ///     {
    ///         static LightClick _lightClick;
    ///
    ///         public static void Main()
    ///         {
    ///             _lightClick = new LightClick(Hardware.SocketOne) {NumberOfSamples = 50};
    ///
    ///             // Sets the range from 0 to 3300 (instead of 0-4095)
    ///             _lightClick.SetScale(0, 3300);
    ///
    ///             while (true)
    ///             {
    ///                 Debug.Print("Light(Scaled) - " + _lightClick.Read(true));
    ///                 Debug.Print("Light(Unscaled) - " + _lightClick.Read(false));
    ///                 Debug.Print("Light Percent - " + _lightClick.ReadLightPercentage() + "%");
    ///                 Debug.Print("Light Volts - " + _lightClick.ReadVoltage() + "V\n");
    ///                 Thread.Sleep(3000);
    ///             }
    ///         }
    ///    }
    /// }
    /// </code>
    /// <code language = "VB">
    /// Option Explicit On
    /// Option Strict On
    ///
    /// Imports Microsoft.SPOT
    /// Imports MBN
    /// Imports System.Threading
    /// Imports MBN.Modules
    ///
    /// Namespace Examples
    ///
    ///     Public Module Module1
    ///
    ///         Dim _lightClick As LightClick
    ///
    ///         Sub Main()
    ///
    ///             _lightClick = New LightClick(Hardware.SocketOne)
    ///
    ///             _lightClick.NumberOfSamples = 50
    ///
    ///             ' Sets the range from 0 to 3300 (instead of 0-4095)
    ///             _lightClick.SetScale(0, 3300)
    ///
    ///             While (True)
    ///                 Debug.Print("Light(Scaled) - " <![CDATA[&]]> _lightClick.Read(True))
    ///                 Debug.Print("Light(Unscaled) - " <![CDATA[&]]> _lightClick.Read(False))
    ///                 Debug.Print("Light Percent - " <![CDATA[&]]> _lightClick.ReadLightPercentage() <![CDATA[&]]> "%")
    ///                 Debug.Print("Light Volts - " <![CDATA[&]]> _lightClick.ReadVoltage() <![CDATA[&]]> "V" <![CDATA[&]]> Microsoft.VisualBasic.Constants.vbCrLf)
    ///                 Thread.Sleep(3000)
    ///             End While
    ///
    ///         End Sub
    ///
    ///     End Module
    ///
    /// End Namespace
    /// </code>
    /// </example>
    public class LightClick : IDriver
    {
        #region Fields

        private const int MinValue = 0, MaxValue = 4095; // Min and Max values returned by MCP3201 ADC
        private const Single ReferenceVoltage = 2.048f; // Reference Voltage from MAX6106, VOUT = 2.048V

        private readonly Byte[] _buffer = new Byte[2]; // Array containing measure from the MCP3201 ADC

        private readonly SPI.Configuration _spiConfig; // SPI configuration 
        private int _scaleHigh; // Upper bound of scale
        private int _scaleLow; // Upper bound of scale

        #endregion

        #region CTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="LightClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the Light Click board is inserted on MikroBus.Net</param>
        /// <exception cref="System.InvalidOperationException">If some pins are already used by another driver, then the exception is thrown.</exception>
        public LightClick(Hardware.Socket socket)
        {
            try
            {
                Hardware.CheckPins(socket, socket.Miso, socket.Cs, socket.Sck); //socket.Mosi is not used by MCP3201 ADC 

                // Change the SPI.Configuration parameters according to the chip datasheet
                _spiConfig = new SPI.Configuration(socket.Cs, false, 0, 0, true, true, 1000, socket.SpiModule);

                if (Hardware.SPIBus == null)
                {
                    Hardware.SPIBus = new SPI(_spiConfig);
                }

                SetScale(); // No scaling by default
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
        ///     Gets or sets the power mode.
        /// </summary>
        /// <value>
        ///     The current power mode of the module.
        /// </value>
        /// <returns cref="NotImplementedException">Calling this method will throw a <see cref="NotImplementedException"/>.</returns>
        /// <remarks>
        ///     This module does not use Power Modes, the GET accessor will always return PowerModes.On. See <see cref="PowerModes"/>, while the SET accessor will do nothing.
        /// </remarks>
        /// <example>None: This sensor does not support PowerMode.</example>
        public PowerModes PowerMode
        {
            get { return PowerModes.On; }
            set { }
        }

        /// <summary>
        ///     The number of samples used when averaging the value read from the MCP3201 ADC for smoothing out erroneous readings.
        /// </summary>
        /// <example>Example usage:
        /// <code> language = "C#">
        /// _lightClick.NumberOfSamples = 50;
        /// </code>
        /// <code language = "VB">
        /// _lightClick.NumberOfSamples = 50
        /// </code>
        /// </example>
        public int NumberOfSamples { get; set; }

        #endregion

        #region Private Methods

        private Int32 Scale(Int32 x)
        {
            return (((_scaleHigh - _scaleLow)*(x - MinValue))/MaxValue) + _scaleLow;
        }

        private int ReadAdc()
        {
            var average = 0;
            for (var i = 0; i < NumberOfSamples; i++) // Read  n samples for smoothing.
            {
                Hardware.SPIBus.WriteRead(_spiConfig, new Byte[] {0xFF, 0xFF}, _buffer);
                var tempValue = ((0x1F & _buffer[0]) << 7) | (_buffer[1] >> 1);
                average += tempValue;
                Thread.Sleep(5);
            }
            return average/NumberOfSamples;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets current light reading.
        /// </summary>
        /// <param name="scaled">
        /// If set to <c>true</c>, then the returned value will be scaled according to the scale provided to the SetScale() method, otherwise the return value will be a raw value in the default range of 0-4095.
        /// </param>
        /// <returns>The value measured by the LightClick.</returns>
        /// <example>Example usage:
        /// <code> language = "C#">
        /// Debug.Print("Light(Scaled) - " + _lightClick.Read(true));
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Light(Scaled) - " <![CDATA[&]]> _lightClick.Read(true))
        /// </code>
        /// </example>
        public int Read(Boolean scaled = true)
        {
            return scaled ? Scale(ReadAdc()) : ReadAdc();
        }

        /// <summary>
        /// Reads Light as a percentage of full scale.
        /// </summary>
        /// <returns>Light reading as a percentage of full scale.</returns>
        /// <example>Example usage:
        /// <code> language = "C#">
        /// Debug.Print("Light Percent - " + _lightClick.ReadLightPercentage() + "%");
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Light Percent - " <![CDATA[&]]> _lightClick.ReadLightPercentage() <![CDATA[&]]> "%")
        /// </code>
        /// </example>
        public Single ReadLightPercentage()
        {
            return (ReadAdc() / 4095f) * 100;
        }

        /// <summary>
        ///     The voltage of the PD15-22C/TR8 Photo-diode is currently measuring when compared to the Reference Voltage of
        ///     MAX6106(2.048V) on the Light Click.
        /// </summary>
        /// <returns> Returns the current voltage.</returns>
        /// <example>Example usage:
        /// <code> language = "C#">
        /// Debug.Print("Light Volts - " + _lightClick.ReadVoltage());
        /// </code>
        /// <code language = "VB">
        ///  Debug.Print("Light Volts - " <![CDATA[&]]> _lightClick.ReadVoltage() <![CDATA[&]]> "V")
        /// </code>
        /// </example>
        public Single ReadVoltage()
        {
            return ReferenceVoltage*(ReadAdc()/4095f);
        }

        /// <summary>
        ///     Resets the LightClick.
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

        /// <summary>
        /// Sets the scale of the measure.
        /// <para><b>By default, the LightClick returns values between 0 and 4095. With this method, you can tell the driver to return values from -100 to 100 or from 0 to 1000, for example.</b></para>
        /// <para><b>By default, the driver is using a 0-4095 scale, similar to a SetScale(0, 4095) function.</b></para>
        /// </summary>
        /// <param name="low">The lower bound of the scale</param>
        /// <param name="high">The upper bound of the scale</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     This exception is thrown if the lower bound is greater or equal to the upper bound or if
        ///     the difference between High andLow is greater than 4095, which is the maximum resolution of the LightClick.
        /// </exception>
        /// <example>
        /// <code> language = "C#">
        /// // Sets the range from 0 to 3300 (instead of 0-4095)
        /// _lightClick.SetScale(0, 3300);
        /// </code>
        /// <code language = "VB">
        /// ' Sets the range from 0 to 3300 (instead of 0-4095)
        /// _lightClick.SetScale(0, 3300)
        /// </code>
        /// </example>
        public void SetScale(Int32 low = MinValue, Int32 high = MaxValue)
        {
            if (((high - low) > MaxValue) || (low >= high))
            {
                throw new ArgumentOutOfRangeException("high");
            }
            _scaleLow = low;
            _scaleHigh = high;
        }

        #endregion
    }
}