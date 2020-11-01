/*
 * SpeakUpClick driver skeleton generated on 12/5/2014 8:35:49 PM
 * 
 * Initial revision coded by Christophe Gerbier
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
 * 
 */

using System;
using System.IO.Ports;
using System.Reflection;
using MBN.Enums;
using MBN.Exceptions;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the SpeakUpClick driver
    /// <para><b>Pins used :</b> Tx, Rx</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// using System.Threading;
    /// using MBN;
    /// using MBN.Modules;
    /// using Microsoft.SPOT;
    /// 
    /// namespace TestApp
    /// {
    ///     public class Program
    ///     {
    ///         private static SpeakUpClick _speakUp;
    ///
    ///         public static void Main()
    ///         {
    ///             try
    ///             {
    ///                 _speakUp = new SpeakUpClick(Hardware.SocketTwo);
    ///             }
    ///             catch (PinInUseException)
    ///             {
    ///                 Debug.Print("Error initializing SpeakUp Click board.");
    ///             }
    ///
    ///             _speakUp.SpeakDetected += _speakUp_SpeakDetected;
    ///             _speakUp.Listening = true;
    ///
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    ///
    ///         private static void _speakUp_SpeakDetected(object sender, SpeakUpClick.SpeakUpEventArgs e)
    ///         {
    ///             Debug.Print("SpeakUp has received an order, index : " + e.Command);
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public partial class SpeakUpClick : IDriver
    {
        private static SerialPort _sp;
        private static Boolean _listening;

        /// <summary>
        /// Occurs when a pre-recorded order has been recognized.
        /// </summary>
        /// <example>
        /// <code language="C#">
        /// using System.Threading;
        /// using MBN;
        /// using MBN.Modules;
        /// using Microsoft.SPOT;
        /// 
        /// namespace TestApp
        /// {
        ///     public class Program
        ///     {
        ///         private static SpeakUpClick _speakUp;
        ///
        ///         public static void Main()
        ///         {
        ///             try
        ///             {
        ///                 _speakUp = new SpeakUpClick(Hardware.SocketTwo);
        ///             }
        ///             catch (PinInUseException)
        ///             {
        ///                 Debug.Print("Error initializing SpeakUp Click board.");
        ///             }
        ///
        ///             _speakUp.SpeakDetected += _speakUp_SpeakDetected;
        ///             _speakUp.Listening = true;
        ///
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        ///
        ///         private static void _speakUp_SpeakDetected(object sender, SpeakUpClick.SpeakUpEventArgs e)
        ///         {
        ///             Debug.Print("SpeakUp has received an order, index : " + e.Command);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public event SpeakUpEventHandler SpeakDetected = delegate { };

        /// <summary>
        /// Initializes a new instance of the <see cref="SpeakUpClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the SpeakUpClick module is plugged on MikroBus.Net board</param>
        public SpeakUpClick(Hardware.Socket socket)
        {
            try
            {
                Hardware.CheckPins(socket, socket.Tx, socket.Rx);
                _sp = new SerialPort(Hardware.SocketTwo.ComPort, 115200, Parity.None, 8, StopBits.One);
                
            }
            // Catch only the PinInUse exception, so that program will halt on other exceptions
            // Send it directly to the caller
            catch (PinInUseException) { throw new PinInUseException(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SpeakUpClick"/> is listening to voice commands.
        /// </summary>
        /// <value>
        ///   <c>true</c> if listening; otherwise, <c>false</c>.
        /// </value>
        /// <example>
        /// <code language="C#">
        /// using System.Threading;
        /// using MBN;
        /// using MBN.Modules;
        /// using Microsoft.SPOT;
        /// 
        /// namespace TestApp
        /// {
        ///     public class Program
        ///     {
        ///         private static SpeakUpClick _speakUp;
        ///
        ///         public static void Main()
        ///         {
        ///             try
        ///             {
        ///                 _speakUp = new SpeakUpClick(Hardware.SocketTwo);
        ///             }
        ///             catch (PinInUseException)
        ///             {
        ///                 Debug.Print("Error initializing SpeakUp Click board.");
        ///             }
        ///
        ///             _speakUp.SpeakDetected += _speakUp_SpeakDetected;
        ///             _speakUp.Listening = true;
        ///
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        ///
        ///         private static void _speakUp_SpeakDetected(object sender, SpeakUpClick.SpeakUpEventArgs e)
        ///         {
        ///             Debug.Print("SpeakUp has received an order, index : " + e.Command);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public Boolean Listening
        {
            get { return _listening; }
            set
            {
                if (value == _listening) { return; }
                if (value)
                {
                    _sp.DataReceived += sp_DataReceived;
                    _sp.Open();
                }
                else
                {
                    _sp.DataReceived -= sp_DataReceived;
                    _sp.Close();
                }
                _listening = value;
            }
        }

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var nb = _sp.BytesToRead;
            var buf = new Byte[nb];

            _sp.Read(buf, 0, nb);

            var speakEvent = SpeakDetected;
            speakEvent(this, new SpeakUpEventArgs(buf[0]));
        }

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <example> This sample shows how to use the PowerMode property.
        /// <code language="C#">
        ///             _SpeakUpClick.PowerMode = PowerModes.Off;
        /// </code>
        /// </example>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">Thrown if the property is set and the module doesn't support power modes.</exception>
        public PowerModes PowerMode
        {
            get { return PowerModes.On; }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the driver version.
        /// </summary>
        /// <example> This sample shows how to use the DriverVersion property.
        /// <code language="C#">
        ///             Debug.Print ("Current driver version : "+_SpeakUpClick.DriverVersion);
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
        /// <exception cref="NotImplementedException"></exception>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException">Thrown because this module has no reset feature.</exception>
        public bool Reset(ResetModes resetMode)
        {
            throw new NotImplementedException();
        }
    }
}

