/*
 * BiHall Click MikroBus.Net NetMF driver
 * 
 * Version 1.0 :
 *  - Initial release coded by Stephen Cardinale
 * 
 * References needed :
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Native
 *  MikroBus.Net
 *  mscorlib
 *  
 * Copyright 2014 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http:///www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System;
using System.Reflection;
using MBN.Enums;
using MBN.Exceptions;
using Microsoft.SPOT.Hardware;

namespace MBN.Modules
{
    /// <summary>
    /// a new instance of the <see cref="BiHallClick"/> class.
    /// <para><b>This module is a Generic Device.</b></para>
    /// <para><b>Pins used :</b>Int</para>
    /// <para><b>References required:</b> MikroBus.Net, Microsoft.SPOT.Native, Microsoft.Spot.Hardware, mscorlib</para>
    /// </summary>
    /// <example>Example usage:
    /// <code language = "C#">
    /// using System.Threading;
    /// using MBN.Modules;
    /// using MBN;
    /// using Microsoft.SPOT;
    ///
    /// namespace Examples
    /// {
    ///     public class Program
    ///     {
    ///         private static BiHallClick _biHall;
    ///
    ///         public static void Main()
    ///         {
    ///             Debug.Print("Started");
    ///
    ///             _biHall = new BiHallClick(Hardware.SocketFour);
    ///             _biHall.SwitchStateChanged += BiHallSwitchStateChanged;
    ///
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    ///
    ///         static void BiHallSwitchStateChanged(object sender, bool switchState)
    ///         {
    ///             Debug.Print("Switch state - " + switchState);
    ///         }
    ///     }
    /// }
    /// </code>
    /// <code language = "VB">
    /// Option Explicit On
    /// Option Strict On
    ///
    /// Imports Microsoft.SPOT
    /// Imports MBN.Modules
    /// Imports MBN
    ///
    /// Namespace Examples
    ///
    ///     Public Module Module1
    ///
    ///         Private WithEvents _biHall As BiHallClick
    ///
    ///         Sub Main()
    ///
    ///             _biHall = New BiHallClick(Hardware.SocketFour)
    ///
    ///         End Sub
    ///
    ///         Private Sub _biHall_SwitchStateChanged(sender As Object, swtichState As Boolean) Handles _biHall.SwitchStateChanged
    ///             Debug.Print("Switch state - " <![CDATA[&]]> swtichState.ToString())
    ///         End Sub
    ///     End Module
    ///
    /// End Namespace
    /// </code>
    /// </example>
    public class BiHallClick : IDriver
    {
        #region Fields

        private readonly InterruptPort _interrupt;

        #endregion

        #region CTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="BiHallClick"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="Hardware.Socket"/> that the AltitudeClick is inserted into.</param>
        /// <exception cref="PinInUseException">A PinInUseException will be thrown if the Int pin is being used by another driver on the same socket.</exception>
        public BiHallClick(Hardware.Socket socket)
        {
            try
            {
                Hardware.CheckPins(socket, socket.Int);

                _interrupt = new InterruptPort(socket.Int, true, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeBoth);
                _interrupt.OnInterrupt += _interrupt_OnInterrupt;

                _interrupt.EnableInterrupt();
            }
                // Catch only the PinInUse exception, so that program will halt on other exceptions
                // Send it directly to caller
            catch (PinInUseException ex)
            {
                throw new PinInUseException(ex.Message);
            }
        }

        #endregion

        #region Private Methods/Internal Interrupt Routines

        private void _interrupt_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            SwitchStatechangedEventHandler tempEvent = SwitchStateChanged;
            tempEvent(this, data2 == 1);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the state of the internal switch.
        /// </summary>
        /// <returns>True if the switch is closed (latched) or otherwise false.</returns>
        /// <example>
        ///     <code language="C#">
        /// Debug.Print("Switch state - " + _biHall.SwitchState);
        /// </code>
        ///     <code language="VB">
        /// Debug.Print("Switch state - " <![CDATA[&]]> _biHall.SwitchState)
        /// </code>
        /// </example>
        public bool SwitchState
        {
            get { return _interrupt.Read(); }
        }

        /// <summary>
        ///     Gets the driver version.
        /// </summary>
        /// <value>
        ///     The driver version.
        /// </value>
        /// <example>
        ///     <code language="C#">
        /// Debug.Print("Current driver version : "+ _biHall.DriverVersion);
        /// </code>
        ///     <code language="VB">
        /// Debug.Print("Current driver version : " <![CDATA[&]]> _biHall.DriverVersion)
        /// </code>
        /// </example>
        public Version DriverVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        /// <summary>
        ///     Gets or sets the power mode.
        /// </summary>
        /// <value>
        ///     The current power mode of the module.
        /// </value>
        /// <remarks>
        ///     This module doe not implement power modes, the GET will always return PowerModes.ON while the SET accessor
        ///     will throw a NotImplementedException.
        /// </remarks>
        /// <exception cref="NotImplementedException">
        ///     This module has no power mode setting. If using the set accessor, it will
        ///     throw an exception.
        /// </exception>
        /// <example>Example Usage: None - This module does not support PowerMode.</example>
        public PowerModes PowerMode
        {
            get { return PowerModes.On; }
            set { throw new NotImplementedException("Power modes not supported."); }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Resets the BiHall Click.
        /// </summary>
        /// <param name="resetMode">The reset mode, see <see cref="ResetModes" /> for more information.</param>
        /// <remarks>
        ///     This module has no Reset method, calling this method will throw an exception.
        /// </remarks>
        /// <exception cref="NotImplementedException">
        ///     This module does not implement a reset method. Calling this method will throw
        ///     a <see cref="NotImplementedException" />
        /// </exception>
        /// <example>Example Usage: None - This module does not support a Reset method.</example>
        public bool Reset(ResetModes resetMode)
        {
            throw new NotImplementedException("This module does not implement a Reset method.");
        }

        #endregion

        #region Events

        /// <summary>
        /// This is the delegate method used by the <see cref="SwitchStateChanged"/> Event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="swtichState"></param>
        public delegate void SwitchStatechangedEventHandler(object sender, bool swtichState);

        /// <summary>
        /// The event that is fired when the BiHall Click opens or closes the internal latched switch in the presence of either a North or South Pole Magnetic Field.  
        /// </summary>
        public event SwitchStatechangedEventHandler SwitchStateChanged = delegate { };

        #endregion
    }
}