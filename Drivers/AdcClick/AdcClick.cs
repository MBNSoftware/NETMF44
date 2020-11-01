/*
 * ADC Click board driver
 * 
 *  Version 2.0 :
 *  - Integration of the new namespaces and new organization.
 *  
 *  Version 1.1 :
 *  - Use of SPI extension methods
 *  - some renaming to please reSharper :-)
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
using Microsoft.SPOT.Hardware;
using System;
using MBN.Extensions;

// ReSharper disable once CheckNamespace
namespace MBN.Modules
{
    /// <summary>
    /// Main class for the MikroE ADC Click board driver
    /// <para><b>Pins used :</b> Miso, Mosi, Cs, Sck</para>
    /// </summary>
    /// <example> This sample shows a basic use of the ADC Click board.
    /// <code language="C#">
    /// using System;
    /// using System.Threading;
    /// using Microsoft.SPOT;
    /// using MikroBusNet
    ///
    /// namespace Example
    /// {
    ///     public class Program
    ///     {
    ///         static AdcClick _adc;
    ///         
    ///         public static void Main()
    ///         {
    ///             // ADC Click board is plugged on socket #1 of the MikroBus.Net mainboard
    ///             _adc = new AdcClick(Hardware.SocketOne);
    ///
    ///             // Sets the range from 0 to 3300 (instead of 0-4095)
    ///             _adc.SetScale(0, 3300);
    /// 
    ///             // Gets last scaled measure for channel 0, not actual reading
    ///             Debug.Print("Channel 0 was last measured at : " + _adc.GetLastValue(0));
    /// 
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public class AdcClick : IDriver
    {
        /// <summary>
        /// List of associated channels for differential measure
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public enum ADCClickDifferentialModes
        {
            /// <summary>
            /// Channel 0 - Channel 1
            /// </summary>
            Ch0Ch1,
            /// <summary>
            /// Channel 1 - Channel 0
            /// </summary>
            Ch1Ch0,
            /// <summary>
            /// Channel 2 - Channel 3
            /// </summary>
            Ch2Ch3,
            /// <summary>
            /// Channel 3 - Channel 2
            /// </summary>
            Ch3Ch2
        };

        private const Int32 MinValue = 0, MaxValue = 4095;  // Min/Max values returned by ADC

        private readonly Int32[] _lastValues;                         // Last values read for each channel
        private Int32 _scaleLow, _scaleHigh;                  // Lower/Upper bound of scale
        private readonly SPI.Configuration _spiConfig;                // SPI configuration
        private readonly Byte[] _adcResult = new Byte[3];             // Array containing measure from the ADC
        private Int32 _result;                               // Result send back to caller

        /// <summary>
        /// Initializes a new instance of the <see cref="AdcClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the ADC Click board is plugged on MikroBus.Net</param>
        /// <exception cref="System.InvalidOperationException">If some pins are already used by another driver, then the exception is thrown.</exception>
        public AdcClick(Hardware.Socket socket)
        {
            // Checks if needed SPI pins are available
            Hardware.CheckPins(socket, socket.Miso, socket.Mosi, socket.Cs, socket.Sck);

            // Initialize SPI
            _spiConfig = new SPI.Configuration(socket.Cs, false, 50, 50, true, true, 1000, socket.SpiModule);
            if (Hardware.SPIBus == null) { Hardware.SPIBus = new SPI(_spiConfig); }

            _lastValues = new Int32[4];          // Empty array for last read values
            SetScale();       // No scaling by default
        }

        /// <summary>
        /// Sets the scale of the measure.
        /// <para>By default, the ADC Click board returns values between 0 and 4095. With this method, you can tell the driver
        /// to return values from -100 to 100 or from 0 to 1000, for example.</para>
        /// <para>If this method is not called, then the driver will be using a 0-4095 scale (similar to a SetScale(0,4095) command).</para>
        /// </summary>
        /// <param name="low">The lower bound of the scale</param>
        /// <param name="high">The upper bound of the scale</param>
        /// <exception cref="ArgumentOutOfRangeException">This exception is thrown if the lower bound is greater or equal to the upper bound or if 
        /// the difference between High andLow is greater than 4095, which is the max value returned by the ADC.</exception>
        /// <example> This sample shows how to call the SetScale() method.
        /// <code language="C#">
        ///             // Sets the range from 0 to 3300 (instead of 0-4095)
        ///             _adc.SetScale(0, 3300);
        /// </code>
        /// </example>
        public void SetScale(Int32 low = MinValue, Int32 high = MaxValue)
        {
// ReSharper disable once NotResolvedInText
            if (((high - low) > MaxValue) || (low >= high)) { throw new ArgumentOutOfRangeException("SetScale"); }
            _scaleLow = low;
            _scaleHigh = high;
        }

        /// <summary>
        /// Gets the last value read for a specified channel.
        /// <para>Warning : this is the value of the last measurment. It may not represent the actual real value given by the GetChannel() method.</para>
        /// </summary>
        /// <param name="channel">The channel (0-3)</param>
        /// <param name="scaled">If set to <c>true</c>, then the returned value will be scaled according to the scale provided to the SetScale() method, otherwise the return value will be a raw value
        /// in the default range of 0-4095.</param>
        /// <returns>The value measured by the ADC.</returns>
        /// <example> This sample shows how to use the GetLastValue() method.
        /// <code language="C#">
        ///             // Gets last scaled measure for channel 0, not real actual measure
        ///             Debug.Print("Channel 0 was last measured at : " + _adc.GetLastValue(0));
        /// </code>
        /// </example>
        public Int32 GetLastValue(Byte channel, Boolean scaled = true)
        {
            return Scale(_lastValues[channel]);
        }

        /// <summary>
        /// Reads all channels (0-3) on the ADC.
        /// </summary>
        /// <param name="scaled">If set to <c>true</c>, then the returned value will be scaled according to the scale provided to the SetScale() method, otherwise the return value will be a raw value
        /// in the default range of 0-4095.</param>
        /// <returns>An array containing the values measured by the ADC for all channels.</returns>
        /// <example> This sample shows how to use the GetAllChannels() method.
        /// <code language="C#">
        ///             // ADC Click board is plugged on socket #1 of the MikroBus.Net mainboard
        ///             _adc = new AdcClick(Hardware.SocketOne);
        ///
        ///             // Gets the actual value of all channels. Result will be scaled to the default range 0-4095
        ///             int[] all = _adc.GetAllChannels(false);
        ///             Debug.Print("Channel 0 scaled : " + all[0]);
        ///             Debug.Print("Channel 1 scaled : " + all[1]);
        ///             Debug.Print("Channel 2 scaled : " + all[2]);
        ///             Debug.Print("Channel 3 scaled : " + all[3]);
        /// </code>
        /// </example>
        public Int32[] GetAllChannels(Boolean scaled = true)
        {
            return new[] { GetChannel(0, scaled), GetChannel(1, scaled), GetChannel(2, scaled), GetChannel(3, scaled) };
        }

        /// <summary>
        /// Gets current measure for a specified channel.
        /// </summary>
        /// <param name="channel">The channel (0-3)</param>
        /// <param name="scaled">If set to <c>true</c>, then the returned value will be scaled according to the scale provided to the SetScale() method, otherwise the return value will be a raw value
        /// in the default range of 0-4095.</param>
        /// <returns>The value measured by the ADC.</returns>
        /// <example> This sample shows how to call the GetChannel() method.
        /// <code language="C#">
        ///             // Gets measure for channel 0 in the default range 0-4095
        ///             Debug.Print("Channel 0 scaled: " + _adc.GetChannel(0));
        /// </code>
        /// </example>
        public Int32 GetChannel(Byte channel, Boolean scaled = true)
        {
            Hardware.SPIBus.WriteRead(_spiConfig, new Byte[] { 0x06, (Byte)(channel << 6), 0x00 }, _adcResult, 1);
            _result = ((_adcResult[0] & 0x0F) << 8) + _adcResult[1];
            _lastValues[channel] = _result;
            return scaled ? Scale(_result) : _result;
        }

        private Int32 Scale(Int32 x)
        {
            return (((_scaleHigh - _scaleLow) * (x - MinValue)) / MaxValue) + _scaleLow;
        }

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">This module has no power modes features.</exception>
        public Enums.PowerModes PowerMode
        {
            get { return Enums.PowerModes.On; }
            set { throw new NotImplementedException("PowerMode"); }
        }

        /// <summary>
        /// Gets the driver version.
        /// </summary>
        /// <example> This sample shows how to use the DriverVersion property.
        /// <code language="C#">
        ///             Debug.Print ("Current driver version : "+_adc.DriverVersion);
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
        /// Resets the module
        /// </summary>
        /// <param name="resetMode">The reset mode :
        /// <para>SOFT reset : generally by sending a software command to the chip</para><para>HARD reset : generally by activating a special chip's pin</para></param>
        /// <returns>True if Reset has been acknowledged, false otherwise.</returns>
        /// <exception cref="System.NotImplementedException">This module has no Reset feature.</exception>
        public bool Reset(Enums.ResetModes resetMode)
        {
            throw new NotImplementedException("Reset");
        }
    }
}
