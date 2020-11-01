/*
 * USB UART Click Driver
 * 
 * Version 1.0 :
 *  - Initial version coded by Stephen Cardinale
 *  Version 2.0 :
 *  - Implementation of the MikroBusNet Interface and Namespace requirements.
 *  - Added event for USB Cable Disconnection notification
 *  - Added event for USB Sleep/suspend notification.
 * 
 * References needed :
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Hardware.SerialPort
 *  Microsoft.SPOT.Native
 *  Microsoft.SPOT.IO
 *  MBN
 *  mscorlib
 *  
 * Source for SimpleSerial taken from IggMoe's SimpleSerial Class https://www.ghielectronics.com/community/codeshare/entry/644
 *  
 * Copyright 2014 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System;
using System.IO.Ports;
using System.Reflection;
using MBN.Enums;
using MBN.Exceptions;
using Microsoft.SPOT.Hardware;

// ReSharper disable once CheckNamespace
namespace MBN.Modules
{
    /// <summary>
    /// A MikroBusNet driver for the MikroE USB UART Click board.
    /// <para><b>This module is a Generic Device</b></para>
    /// <para><b>Pins used :</b>Tx, Rx, Rst, Int, Pwm and Cs</para>
    /// <para><b>References required:</b> MikroBus.Net, Microsoft.SPOT.Native, Microsoft.Spot.Hardware, mscorlib</para>
    /// </summary>
    /// <remarks>
    /// Required References - Microsoft.SPOT.Hardware,  Microsoft.SPOT.Hardware.SerialPort, Microsoft.SPOT.Native, Microsoft.SPOT.IO, mscorlib
    /// </remarks>
    /// <example>
    /// <code language = "C#">
    /// using MBN;
    /// using System.IO.Ports;
    /// using MBN.Enums;
    /// using Microsoft.SPOT;
    /// using System.Threading;
    ///
    /// namespace USB_UART_TestApp
    /// {
    ///    public class Program
    ///    {
    ///       private static USBUARTClick _usbUart;
    ///
    ///       public static void Main()
    ///       {
    ///         _usbUart= new USBUARTClick(Hardware.SocketFour, BaudRate.Baud9600, Handshake.None) {GlitchFilterTime = new TimeSpan(0, 0, 0, 0, 50)};
    ///
    ///        _usbUart.DataReceived += USBUartUSBUARTDataReceived;
    ///        _usbUart.CableConnectionChanged += USBUartUSBUARTCableConnectionChanged;
    ///        _usbUart.SleepSuspend += USBUARTUartUSBUARTSleepSuspend;
    ///
    ///        Debug.Print("USB is connected ? " + _usbUart.USBCableConnected);
    ///        Debug.Print("USB sleeping ? " + _usbUart.USBSuspended);
    ///
    ///        Thread.Sleep(Timeout.Infinite);
    ///        }
    ///
    ///        static void USBUARTUartUSBUARTSleepSuspend(object sender, bool isSleeping, DateTime eventTime)
    ///        {
    ///           Debug.Print("USB " + (isSleeping ? "Suspend/Sleep" : "Wake") + " event occurred at " + eventTime);
    ///        }
    ///
    ///        static void USBUartUSBUARTCableConnectionChanged(object sender, bool cableConnected, DateTime eventTime)
    ///        {
    ///            Debug.Print("Cable " + (cableConnected ? "connection" : "dis-connection") + " event occurred at " + eventTime);
    ///        }
    ///
    ///        static void USBUartUSBUARTDataReceived(object sender, string message, DateTime eventTime)
    ///        {
    ///            // Echo back to sender
    ///            _usbUart.SendData("Received your message of - \"" + message + "\" at " + eventTime);
    ///            Debug.Print(message);
    ///        }
    ///     }
    /// }
    /// </code>
    /// <code language = "VB">
    /// Option Explicit On
    /// Option Strict On
    ///
    /// Imports MBN
    /// Imports System.IO.Ports
    /// Imports MBN.Enums
    /// Imports Microsoft.SPOT
    /// Imports System.Threading
    ///
    /// Namespace MBN_Basic_VB_Application1
    ///
    ///    Public Module Module1
    ///
    ///       Dim _usbUart As USBUARTClick ' or Dim WithEvents _usbUart As USBUARTClick
    ///
    ///        Sub Main()
    ///           _usbUART = New USBUARTClick(Hardware.SocketFour, BaudRate.Baud921600, Handshake.None)
    /// 
    ///            _usbUART.GlitchFilterTime = New TimeSpan(0, 0, 0, 0, 50)
    /// 
    ///            AddHandler _usbUART.DataReceived, AddressOf usbUart_USBUARTDataReceived
    ///            AddHandler _usbUART.CableConnectionChanged, AddressOf _usbUart_USBUARTCableConnectionChanged
    ///            AddHandler _usbUART.SleepSuspend, AddressOf usbUart_USBUARTSleepSuspend
    /// 
    ///            Debug.Print("USB is connected ? " <![CDATA[&]]> _usbUart.USBCableConnected)
    ///            Debug.Print("USB sleeping ? " <![CDATA[&]]> _usbUart.USBSuspended)
    /// 
    ///            Thread.Sleep(Timeout.Infinite)
    ///        End Sub
    ///
    ///        Private Sub _usbUart_USBUARTDataReceived(message As String) Handles _usbUart.DataReceived
    ///            ' Echo back to sender
    ///            _usbUART.SendData("Received your message - " <![CDATA[&]]> message <![CDATA[&]]> " at " <![CDATA[&]]> eventTime)
    ///            Debug.Print(message)
    ///        End Sub
    /// 
    ///        Private Sub usbUart_USBUARTSleepSuspend(sender As Object, isSleeping As Boolean, eventTime As Date)
    ///            Debug.Print("USB " <![CDATA[&]]> If(isSleeping, "Suspend/Sleep", "Wake") <![CDATA[&]]> " event occurred at " <![CDATA[&]]> eventTime)
    ///        End Sub
    ///
    ///        Private Sub _usbUart_USBUARTCableConnectionChanged(sender As Object, cableConnected As Boolean, eventTime As Date)
    ///            Debug.Print("Cable " <![CDATA[&]]> If(cableConnected, "connection", "dis-connection") <![CDATA[&]]> " event occurred at " <![CDATA[&]]> eventTime)
    ///        End Sub
    ///    End Module
    ///
    /// End Namespace
    /// </code>
    /// </example>
// ReSharper disable once InconsistentNaming
    public partial class USBUARTClick: IDriver
    {

        #region Fields

        private readonly SimpleSerial _serial;
        private string[] _dataIn;

        private static InterruptPort _sleepPin;
        private static InterruptPort _powerPin;

        #endregion

        #region ENUMS
        
        /// <summary>
        ///     Enumeration for BaudRate selection of the USBUARTClick
        /// </summary>
        public enum BaudRate
        {
            /// <summary>
            ///     BaudRate 2400
            /// </summary>
            Baud2400 = 2400,

            /// <summary>
            ///     BaudRate 4800
            /// </summary>
            Baud4800 = 4800,

            /// <summary>
            ///     BaudRate 9600
            /// </summary>
            Baud9600 = 9600,

            /// <summary>
            ///     BaudRate 14,400
            /// </summary>
            Baud14400 = 14400,

            /// <summary>
            ///     BaudRate 19200
            /// </summary>
            Baud19200 = 19200,

            /// <summary>
            ///     BaudRate 28,800
            /// </summary>
            Baud28800 = 28800,
            /// <summary>
            ///     Default BaudRate 38400
            /// </summary>
            Baud38400 = 38400,

            /// <summary>
            ///     BaudRate 57600
            /// </summary>
            Baud57600 = 57600,

            /// <summary>
            ///     BaudRate 115200
            /// </summary>
            Baud115200 = 115200,

            /// <summary>
            ///     BaudRate 230400
            /// </summary>
            Baud230400 = 230400,

            /// <summary>
            ///     BaudRate 460800
            /// </summary>
            Baud460800 = 460800,

            /// <summary>
            ///     BaudRate 921600
            /// </summary>
            Baud921600 = 921600
        }

        #endregion

        #region CTOR
        
        /// <summary>
        ///     Default constructor
        /// </summary>
        /// <param name="socket">The socket in which the USB UART click board is inserted into.</param>
        /// <param name="baudRate">Baud Rate enumeration of usable baud rates (ones that actually work), see <see cref="System.IO.Ports.SerialPort.BaudRate"/></param>
        /// <param name="handshake">Optional - Handshake, defaults to None, see <see cref="System.IO.Ports.Handshake"/>. <see cref="System.IO.Ports.Handshake.RequestToSend "/> is not functional and will be set to <see cref="Handshake.None"/>.</param>
        /// <exception cref="PinInUseException">A <see cref="PinInUseException"/> will be thrown if the Tx, Rx, Rst, Int, Cs, Pwm are used in a stacked module arrangement.</exception>
        public USBUARTClick(Hardware.Socket socket, BaudRate baudRate, Handshake handshake)
        {
            try
            {
                // Rst Pin is connected to FT232RL CTS Pin, Int Pin is connected to FT232RL RTS Pin, Pwm Pin is connected to FT232RL CBUS3 Pin (USB Power), Cs Pin is connected to FT232RL CBUS4 Pin (USB Sleep/Suspend)
                Hardware.CheckPins(socket, socket.Tx, socket.Rx, socket.Rst, socket.Int, socket.Cs, socket.Pwm);
                _serial = new SimpleSerial(socket.ComPort, (int) baudRate) {Handshake = handshake == Handshake.RequestToSend ? Handshake.None : handshake};

                _serial.DataReceived += _serial_DataReceived;
                _serial.ErrorReceived += _serial_ErrorReceived;
                _serial.Open();

                _powerPin = new InterruptPort(socket.Pwm, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);
                _sleepPin = new InterruptPort(socket.Cs, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);
                
                _powerPin.OnInterrupt += powerPin_OnInterrupt;
                _sleepPin.OnInterrupt += sleepPin_OnInterrupt;

                _powerPin.EnableInterrupt();
                _sleepPin.EnableInterrupt();

            }
            catch (PinInUseException ex)
            {
                throw new PinInUseException(ex.Message);
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Use this property to adjust the CPU.GlitchFilterTime in the case where the <see cref="SleepSuspend"/> event is firing multiple times for one occurrence.
        /// </summary>
        /// <remarks>If not subscribing to the <see cref="SleepSuspend"/> event you can safely ignore this property. The default NETMF value is 20 mSec.</remarks>
        /// <remarks>A setting of <see cref="TimeSpan"/> 50 mSec typically corrects multiple firing. Do not set this to a very large value as some events might not be triggered, such as rapidly pressing a button.</remarks>
        /// <example>Example code on how to set the GlitchFilter Property
        /// <code language = "C#">
        /// _usbUart.GlitchFilterTime = new TimeSpan(0, 0, 0, 0, 50);
        ///  </code>
        /// <code language = "VB">
        /// _usbUart.GlitchFilterTime = New TimeSpan(0, 0, 0, 0, 50)
        ///  </code>
        /// </example>
        public TimeSpan GlitchFilterTime
        {
            get { return Cpu.GlitchFilterTime; }
            set { Cpu.GlitchFilterTime = value; }
        }
		
        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <returns cref="NotImplementedException">Calling this method will throw a <see cref="NotImplementedException"/>.</returns>
        /// <remarks>
        /// This module does not use Power Modes, the GET accessor will always return PowerModes.On. See <see cref="PowerModes"/>, while the SET accessor will do nothing.
        /// </remarks>
        /// <example>None: This sensor does not support PowerMode.</example>
        public PowerModes PowerMode { get { return PowerModes.Off; } set { } }

        /// <summary>
        /// Gets the driver version.
        /// </summary>
        /// <value>
        /// The driver version see <see cref="Version"/>.
        /// </value>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.Print("Driver Version Info : " + _usbUart.DriverVersion);
        /// </code>
        /// <code language="VB">
        /// Debug.Print("Driver Version Info : " <![CDATA[&]]> _usbUart.DriverVersion)
        /// </code>
        /// </example>
        public Version DriverVersion
        {
            get { return Assembly.GetAssembly(GetType()).GetName().Version; }
        }

        /// <summary>
        /// Gets the status of the USB Cable connection to USB UART Click.
        /// </summary>
        /// <returns>True is a USB cable is connected to the USB port of the USBUART Click or otherwise false.</returns>
        /// <returns>This property is useful to determine if a cable or PC is attached to the click. For example, you would not want to send data to an attached device (PC) if it is not connected to the click or a buffer overflow might happen.</returns>
        /// <example>
        /// <code language = "C#">
        /// Debug.Print("USB is connected ? " + _usbUart.USBCableConnected);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("USB is connected ? " <![CDATA[&]]> _usbUart.USBCableConnected)
        /// </code>
        /// </example>
        public bool USBCableConnected
        {
            get { return !_powerPin.Read(); }
        }

        /// <summary>
        /// Gets the Sleep/Suspend status of the USB connection to the USBUART Click.
        /// </summary>
        /// <returns>True is a USB connection is in the Sleep/Suspend mode or otherwise false.</returns>
        /// <returns>This property is useful to determine if the USB connection is Sleeping or Suspended. For example, you would not want to send data to an attached device (PC) if it is sleeping or suspended or a buffer overflow might happen.</returns>
        /// <example>
        /// <code language = "C#">
        /// Debug.Print("USB sleeping ? " + _usbUart.USBSuspended);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("USB sleeping ? " <![CDATA[&]]> _usbUart.USBSuspended);
        /// </code>
        /// </example>
        public bool USBSuspended
        {
            get
            {
                return !_sleepPin.Read();
            }
        }

        #endregion 

        #region Public Methods

        /// <summary>
        /// Send a <see cref="System.String"/> of data to the USB client connected to the USBUART click.
        /// </summary>
        /// <param name="data">String data to send.</param>
        /// <example>
        /// <code language = "C#">
        /// _usbUart.SendData("Received your message - " + message);
        /// </code>
        /// <code language = "VB">
        /// _usbUart.SendData("Received your message - " <![CDATA[&]]> message)
        /// </code>
        /// </example>
        public void SendData(string data)
        {
            _serial.WriteLine(data);
        }

        /// <summary>
        /// Resets the USBUART Click.
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
        ///  Represents the delegate that is used for the <see cref="DataReceived"/> event.
        /// </summary>
        /// <param name="sender">The USBUART Click that raised the event.</param>
        /// <param name="message">The <see cref="System.String"/> data that was received.</param>
        /// <param name="eventTime">The time that the event occurred.</param>
        public delegate void DataReceivedHandler(object sender, string message, DateTime eventTime);

        /// <summary>
        /// Raised when the USBUART Click receives data.
        /// </summary>
        public event DataReceivedHandler DataReceived;

        private void _serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _dataIn = _serial.Deserialize();
            foreach (string t in _dataIn)
            {
                if (DataReceived != null) DataReceived(this, t, DateTime.Now);
            }
        }

        /// <summary>
        ///  Represents the delegate that is used for the <see cref="USBUARTClick.ErrorReceived"/> event.
        /// </summary>
        /// <param name="sender">The USBUART Click that raised the event.</param>
        /// <param name="e">The <see cref="System.IO.Ports.SerialErrorReceivedEventArgs"/></param>
        /// <param name="eventTime">The time that the event occurred.</param>
        public delegate void ErrorReceivedHandler(object sender, SerialErrorReceivedEventArgs e, DateTime eventTime);

        /// <summary>
        /// Raised when the USBUART Click receives an Error in data transmission.
        /// </summary>
        public event ErrorReceivedHandler ErrorReceived;

        private void _serial_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            ErrorReceivedHandler handler = ErrorReceived;

            if (handler == null) return;

            handler(this, e, DateTime.Now);
        }

        /// <summary>
        /// Represents the delegate that is used for the <see cref="CableConnectionChanged"/> event. 
        /// </summary>
        /// <param name="sender">The USBUART Click that raised the event.</param>
        /// <param name="cableConnected">Cable connection parameter.</param>
        /// <param name="eventTime">The time that the event occurred.</param>
        public delegate void CableConnectionEventHandler(object sender, bool cableConnected, DateTime eventTime);

        /// <summary>
        /// Raised when the USB Cable connection to the USBUART has changed.
        /// </summary>
        public event CableConnectionEventHandler CableConnectionChanged;

        private void _cableConnectionChanged(bool cableConnected, DateTime eventTime)
        {
            CableConnectionEventHandler handler = CableConnectionChanged;

            if (handler == null) return;

            handler(this, cableConnected, eventTime);
        }

        /// <summary>
        /// Represents the delegate that is used for the <see cref="SleepSuspend"/> event. 
        /// </summary>
        /// <param name="sender">The USBUART Click that raised the event.</param>
        /// <param name="isSleeping">Sleep or suspend parameter.</param>
        /// <param name="eventTime">The time that the event occurred.</param>
        public delegate void SleepEventHandler(object sender, bool isSleeping, DateTime eventTime);

        /// <summary>
        /// Raised when the USB connection enters Sleep or Suspend mode.
        /// </summary>
        public event SleepEventHandler SleepSuspend;

        private void _usbSleep(bool isSleeping, DateTime eventTime)
        {
            SleepEventHandler handler = SleepSuspend;

            if (handler == null) return;

            handler(this, isSleeping, eventTime);
        }

        void sleepPin_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            _usbSleep(data2 != 1, time);
        }

        void powerPin_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            _cableConnectionChanged(data2 != 1, time);
        }

        #endregion

    }
}