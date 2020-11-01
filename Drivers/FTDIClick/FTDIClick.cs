/*
 * FTDIClick driver 
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
    /// Main class for the FTDIClick driver
    /// <para><b>Pins used :</b> Tx, Rx</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// using System;
    /// using System.Text;
    /// using MBN.Modules;
    /// using MBN;
    /// using MBN.Exceptions;
    /// using System.Threading;
    /// using Microsoft.SPOT;
    /// 
    /// namespace Examples
    /// {
    ///     public class Program
    ///     {
    ///         private static FTDIClick _ftdi;
    /// 
    ///         public static void Main()
    ///         {
    ///             try
    ///             {
    ///                 _ftdi = new FTDIClick(Hardware.SocketOne);
    /// 
    ///                 _ftdi.DataReceived += _ftdi_DataReceived;
    ///                 _ftdi.Listening = true;
    /// 
    ///                 _ftdi.SendData(Encoding.UTF8.GetBytes("Hello world !"));
    ///             }
    ///             catch (PinInUseException)
    ///             {
    ///                 Debug.Print("Error accessing the FTDI Click board");
    ///             }
    /// 
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    /// 
    ///         static void _ftdi_DataReceived(object sender, FTDIClick.DataReceivedEventArgs e)
    ///         {
    ///             Debug.Print("Data received (" + e.Count + " bytes) : " + new String(Encoding.UTF8.GetChars(e.Data)));
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
// ReSharper disable once InconsistentNaming
    public partial class FTDIClick : IDriver
    {
        // This is a demo event fired by the driver. Implementation is in the Christophe.Events namespace below.
        /// <summary>
        /// Occurs when a demo event is detected.
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///         public static void Main()
        ///         {
        ///             try
        ///             {
        ///                 _ftdi = new FTDIClick(Hardware.SocketOne);
        /// 
        ///                 _ftdi.DataReceived += _ftdi_DataReceived;
        ///                 _ftdi.Listening = true;
        /// 
        ///                 _ftdi.SendData(Encoding.UTF8.GetBytes("Hello world !"));
        ///             }
        ///             catch (PinInUseException)
        ///             {
        ///                 Debug.Print("Error accessing the FTDI Click board");
        ///             }
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        /// 
        ///         static void _ftdi_DataReceived(object sender, FTDIClick.DataReceivedEventArgs e)
        ///         {
        ///             Debug.Print("Data received (" + e.Count + " bytes) : " + new String(Encoding.UTF8.GetChars(e.Data)));
        ///         }
        /// </code>
        /// </example>
        public event DataReceivedEventHandler DataReceived = delegate { };

        private readonly SerialPort _sp;
        private Boolean _listening;

        /// <summary>
        /// Initializes a new instance of the <see cref="FTDIClick" /> class.
        /// </summary>
        /// <param name="socket">The socket on which the FTDIClick module is plugged on MikroBus.Net board</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="parity">The parity.</param>
        /// <param name="dataBits">The data bits.</param>
        /// <param name="stopBits">The stop bits.</param>
        /// <exception cref="MBN.Exceptions.PinInUseException"></exception>
        public FTDIClick(Hardware.Socket socket, Int32 baudRate=9600, Parity parity=Parity.None, Int32 dataBits=8, StopBits stopBits=StopBits.One)
        {
            try
            {
                Hardware.CheckPins(socket, socket.Tx, socket.Rx);

                _sp = new SerialPort(socket.ComPort, baudRate, parity, dataBits, stopBits);
                _sp.Open();
            }
            // Catch only the PinInUse exception, so that program will halt on other exceptions
            // Send it directly to the caller
            catch (PinInUseException) { throw new PinInUseException(); }
        }

        private void _sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var nb = _sp.BytesToRead;
            var buf = new Byte[nb];

            _sp.Read(buf, 0, nb);
            var tempEvent = DataReceived;
            tempEvent(this, new DataReceivedEventArgs(buf, nb));
        }

        /// <summary>
        /// Sends data to the FTDI chip.
        /// </summary>
        /// <param name="data">Array of bytes containing data to send</param>
        /// <example>
        /// <code language="C#">
        ///         public static void Main()
        ///         {
        ///             try
        ///             {
        ///                 _ftdi = new FTDIClick(Hardware.SocketOne);
        /// 
        ///                 _ftdi.SendData(Encoding.UTF8.GetBytes("Hello world !"));
        ///             }
        ///             catch (PinInUseException)
        ///             {
        ///                 Debug.Print("Error accessing the FTDI Click board");
        ///             }
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        /// </code>
        /// </example>
        public void SendData (Byte[] data)
        {
            _sp.Write(data,0,data.Length);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FTDIClick"/> is listening on the serial port
        /// </summary>
        /// <value>
        ///   <c>true</c> if listening; otherwise, <c>false</c>.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///         public static void Main()
        ///         {
        ///             try
        ///             {
        ///                 _ftdi = new FTDIClick(Hardware.SocketOne);
        /// 
        ///                 _ftdi.DataReceived += _ftdi_DataReceived;
        ///                 _ftdi.Listening = true;
        /// 
        ///             }
        ///             catch (PinInUseException)
        ///             {
        ///                 Debug.Print("Error accessing the FTDI Click board");
        ///             }
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        /// 
        ///         static void _ftdi_DataReceived(object sender, FTDIClick.DataReceivedEventArgs e)
        ///         {
        ///             Debug.Print("Data received (" + e.Count + " bytes) : " + new String(Encoding.UTF8.GetChars(e.Data)));
        ///         }
        /// </code>
        /// </example>
        public Boolean Listening
        {
            get { return _listening; }
            set
            {
                if (value == _listening) { return; }
                if (value) { _sp.DataReceived += _sp_DataReceived; }
                else { _sp.DataReceived -= _sp_DataReceived; }
                _listening = value;
            }
        }

        

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">Thrown if the property is set and the module doesn't support power modes.</exception>
        public PowerModes PowerMode
        {
            get { return PowerModes.On; }
            set { throw new NotImplementedException("PowerModes"); }
        }

        /// <summary>
        /// Gets the driver version.
        /// </summary>
        /// <example> This sample shows how to use the DriverVersion property.
        /// <code language="C#">
        ///             Debug.Print ("Current driver version : "+_FTDIClick.DriverVersion);
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
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException">Thrown because this module has no reset feature.</exception>
        public bool Reset(ResetModes resetMode)
        {
            throw new NotImplementedException("Reset");
        }
    }
}

