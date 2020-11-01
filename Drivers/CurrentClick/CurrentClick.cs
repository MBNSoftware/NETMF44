/*
 * Current Click Driver
 * 
 *  Version 1.0 :
 *  
 * Version 1.0 :
 *  - Initial revision coded by Stephen Cardinale
 * Version 2.0 :
 *  - Implementation of the MikroBusNet Interface and new Namespaces requirements.

 * * References needed :
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

// ReSharper disable once CheckNamespace
namespace MBN.Modules
{
    /// <summary>
    /// MicroBusNet Driver for the CurrentClick board by MikroElektronika.
    /// <para><b>The CurrentClick is a SPI Device</b></para>
    /// <para><b>Pins used :</b> Miso, Mosi, Cs, Sck</para>
    /// <para><b>References required:</b> MikroBus.Net, Microsoft.SPOT.Native, Microsoft.Spot.Hardware, mscorlib</para>
    /// </summary>
    /// <remarks>
    /// <para>Use the following resistor table for sizing of the Shunt Resistor.</para>
    /// <conceptualLink target="fc971667-c1d0-4b99-8ac3-11750a5e203c" />
    /// </remarks>
    /// <example>
    /// <code language = "C#">
    /// using MBN;
    /// using MBN.Modules;
    /// using Microsoft.SPOT;
    /// using System.Threading;
    /// namespace Examples
    /// {
    ///     public class Program
    ///     {
    ///         private static CurrentClick _current;
    ///
    ///         public static void Main()
    ///         {
    ///             _current = new CurrentClick(Hardware.SocketThree, CurrentClick.ShuntResistor.SR_CUSTOM, 0.2f) {NumberOfSamples = 50};
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
    ///                 Debug.Print(_current.ReadCurrent() + "mA");
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
    /// Imports MBN
    /// Imports MBN.Enums
    /// Imports MBN.Modules
    /// Imports Microsoft.SPOT
    /// Imports System.Threading
    ///
    /// Namespace Examples
    ///
    ///    Public Module Module1
    ///
    ///         Private _current As CurrentClick
    ///
    ///         Sub Main()
    ///
    ///             _current = new CurrentClick(Hardware.SocketThree, CurrentClick.ShuntResistor.SR_CUSTOM, 0.2f)
    /// 
    ///            _current.NumberOfSamples = 20
    ///
    ///             Dim captureThread As New Thread(New ThreadStart(AddressOf Capture))
    ///             captureThread.Start()
    ///
    ///             Thread.Sleep(Timeout.Infinite)
    ///         End Sub
    ///
    ///         Private Sub Capture()
    ///             While True
    ///                 Debug.Print(_current.ReadCurrent() <![CDATA[&]]> "mA")
    ///                 Thread.Sleep(1000)
    ///             End While
    ///         End Sub
    ///
    ///     End Module
    ///
    /// End Namespace
    /// </code>
    /// </example>>
    public class CurrentClick : IDriver
    {
        #region Fields

        private static ShuntResistor _shuntResistorResistorValue;
        private static float _userdefinedShuntResistorValue = 1;
        private readonly Byte[] _buffer = new Byte[2]; // Array containing measure from the CurrentClick
        private readonly SPI.Configuration _spiConfig; // SPI configuration 

        #endregion

        #region Enum

        /// <summary>
        ///     The value in Ohms of the ShuntResistor Resistor inserted into the rShunt Terminal Block of the Current click.
        /// </summary>
        public enum ShuntResistor
        {
            /// <summary>
            ///     User defined ShuntResistor resistor.
            /// </summary>
            // ReSharper disable InconsistentNaming
            SR_CUSTOM = 0,

            /// <summary>
            ///     The resistor with a Resistance of 0.05 Ohms 1%.
            /// </summary>
            SR0_05 = 5,

            /// <summary>
            ///     The resistor with a Resistance of 0.25 Ohms 1%.
            /// </summary>
            SR0_2 = 20,

            /// <summary>
            ///     The resistor with a Resistance of 1 Ohms 1%.
            /// </summary>
            SR1 = 100,

            /// <summary>
            ///     The resistor with a Resistance of 10 Ohms 1%.
            /// </summary>
            SR10 = 1000
            // ReSharper restore InconsistentNaming
        }

        #endregion}
      
        #region CTOR

        /// <summary>
        ///     Default constructor for the Current Click board.
        /// </summary>
        /// <param name="socket">The MBN Socket that the Current click is inserted into.</param>
        /// <param name="shuntResistorResistorValue">The value in Ohms of the resistor in the ShuntResistor Terminal Block</param>
        /// <param name="userDefinedShuntResistor">
        ///     Optional User Defined value of the ShuntResistor Resistor.
        ///     To use a User Defined ShuntResistor Resistor, set the value of the <see cref="ShuntResistor"/> to SR_CUSTOM
        /// </param>
        public CurrentClick(Hardware.Socket socket, ShuntResistor shuntResistorResistorValue, float userDefinedShuntResistor = 1f)
        {
            try
            {
                Hardware.CheckPins(socket, socket.Miso, socket.Mosi, socket.Cs, socket.Sck);

                _spiConfig = new SPI.Configuration(socket.Cs, false, 0, 0, true, true, 1000, socket.SpiModule);

                if (Hardware.SPIBus == null)
                {
                    Hardware.SPIBus = new SPI(_spiConfig);
                }

                if (shuntResistorResistorValue == ShuntResistor.SR_CUSTOM)
                {
                    _userdefinedShuntResistorValue = userDefinedShuntResistor;
                }
                else
                {
                    _shuntResistorResistorValue = shuntResistorResistorValue;
                }
            }
                // Catch only the PinInUse exception, so that program will halt on other exceptions
                // Send it directly to caller
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
        ///     The number of samples used when averaging the value read from the CurrentClick for smoothing out erroneous readings.
        /// </summary>
        /// <example>
        /// <code language = "C#">
        /// _current.NumberOfSamples = 50;
        /// </code>
        /// <code language = "VB">
        /// _current.NumberOfSamples = 20
        /// </code>
        /// </example>
        public int NumberOfSamples { get; set; }

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

        #endregion

        #region Public Methods

        /// <summary>
        ///     Reads the Current (mA).
        /// </summary>
        /// <returns>The current (mA) applied to the IN(+) and OUT(-) pins of the first screw terminal.</returns>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print(_current.ReadCurrent() + "mA");
        /// </code>
        /// <code language = "VB">
        /// Debug.Print(_current.ReadCurrent() <![CDATA[&]]> "mA")
        /// </code>
        /// </example>
        public float ReadCurrent()
        {
            float adc = ReadAdc();

            if ((adc > 100) && (adc < 4095)) // If the ADC reads less than 100 it is probably noise.
            {
                return (float) (adc*.025/(_shuntResistorResistorValue != ShuntResistor.SR_CUSTOM ? (float) _shuntResistorResistorValue/100 : _userdefinedShuntResistorValue)); // Calculate current value in mA
            }
            return 0;
        }

        /// <summary>
        ///     Resets the CurrentClick Click.
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

        #region Private Methods

        private float ReadAdc()
        {
            int average = 0;
            for (int i = 0; i < NumberOfSamples; i++) // Read  n samples for smoothing.
            {
                Hardware.SPIBus.WriteRead(_spiConfig, new Byte[] {0x00, 0x00}, _buffer);
                int adcRead = ((0x1F & _buffer[0]) << 7) | (_buffer[1] >> 1);
                average += adcRead; 
                Thread.Sleep(5);
            }
            return (float) average / NumberOfSamples;
        }

        #endregion

    }
}
