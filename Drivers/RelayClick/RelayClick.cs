/*
 * Relay Click board driver
 * 
 * Version 2.0
 *  - Conformance to the new namespaces and organization
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
using MBN.Enums;
using Microsoft.SPOT.Hardware;
using System;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the MikroE Relay Click board driver
    /// <para><b>Pins used :</b> Pwm, Cs</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///     static RelayClick _relays;
    /// 
    ///     public static void Main()
    ///     {
    ///         try
    ///         {
    ///             // Relay Click board is plugged on socket #1 of the MikroBus.Net mainboard
    ///             // Relay 0 will be OFF and Relay 1 will be ON at startup
    ///             _relays = new RelayClick(Hardware.SocketTwo, relay1InitialState: true);
    /// 
    ///             // Register to the event generated when a relay state has been changed
    ///             _relays.RelayStateChanged += Relays_RelayStateChanged;
    /// 
    ///             // Sets relay 0 to ON using the SetRelay() method
    ///             _relays.SetRelay(0, true);
    ///             Thread.Sleep(2000);
    /// 
    ///             // Sets relay 0 to OFF using the Relay0 property
    ///             _relays.Relay0 = false;
    /// 
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    ///         catch (Exception ex)
    ///         {
    ///             Debug.Print("ERROR : " + ex.Message);
    ///         }
    ///     }
    /// 
    ///     static void Relays_RelayStateChanged(object sender, RelayClick.RelayStateChangedEventArgs e)
    ///     {
    ///         if (e.Relay == 0)   // Red led for relay 0
    ///         {
    ///             Debug.Print("Relay 0 state has changed");
    ///         }
    ///         else                // Green led for relay 1
    ///         {
    ///             Debug.Print("Relay 1 state has changed");
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public partial class RelayClick : IDriver
    {
        /// <summary>
        /// Occurs when the state of a relay has changed.
        /// </summary>
        public event RelayStateChangedEventHandler RelayStateChanged = delegate { };

        private readonly Boolean[] _states;
        private readonly OutputPort[] _relays;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayClick"/> class.
        /// </summary>
        /// <param name="socket">he socket on which the Relay Click board is plugged on MikroBus.Net</param>
        /// <param name="relay0InitialState">Sets the initial state of relay 0 : <c>true</c> = ON, <c>false</c> = OFF.</param>
        /// <param name="relay1InitialState">Sets the initial state of relay 1 : <c>true</c> = ON, <c>false</c> = OFF.</param>
        public RelayClick(Hardware.Socket socket, Boolean relay0InitialState = false, Boolean relay1InitialState = false)
        {
            // Check if needed pins are available
            Hardware.CheckPins(socket, socket.Pwm, socket.Cs);

            // Init the array containing the relays states
            _states = new [] { relay0InitialState, relay1InitialState };
            // Initialize hardware ports with requested initial states
            _relays = new [] { new OutputPort(socket.Pwm, relay0InitialState), new OutputPort(socket.Cs, relay1InitialState) };
        }

        /// <summary>
        /// Gets or sets a value indicating whether Relay0 will be ON or OFF
        /// </summary>
        /// <value>
        ///   <c>true</c> if Relay0 should be ON, otherwise <c>false</c>.
        /// </value>
        public Boolean Relay0
        {
            get { return GetRelay(0); }
            set { SetRelay(0, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Relay1 will be ON or OFF
        /// </summary>
        /// <value>
        ///   <c>true</c> if Relay1 should be ON, otherwise <c>false</c>.
        /// </value>
        public Boolean Relay1
        {
            get { return GetRelay(1); }
            set { SetRelay(1, value); }
        }

        /// <summary>
        /// Sets the state of a specified relay.
        /// </summary>
        /// <param name="relay">The relay : 0 or 1.</param>
        /// <param name="state">if set to <c>true</c>, relay will be ON otherwise it will be OFF.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown if the relay number is greater than 1.</exception>
        public void SetRelay(Byte relay, Boolean state)
        {
            if (relay > 1) { throw new IndexOutOfRangeException("relay"); }
            if (state != _states[relay])
            {
                _relays[relay].Write(state);
                _states[relay] = state;
                var relayEvent = RelayStateChanged;
                relayEvent(this, new RelayStateChangedEventArgs(relay, !state, state));
                Thread.Sleep(10);   // Max operate time, from datasheet
            }
        }

        /// <summary>
        /// Gets the state of a specified relay.
        /// </summary>
        /// <param name="relay">The relay : 0 or 1.</param>
        /// <returns>A <cref>Boolean</cref> indicating the state. <c>true</c> means "Relay ON", <c>false</c> means "Relay OFF".</returns>
        public Boolean GetRelay(Byte relay)
        {
            try
            {
                return _states[relay];
            }
            catch
            {
                throw new IndexOutOfRangeException();
            }
        }

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">This module does not have a power modes feature.</exception>
        public PowerModes PowerMode
        {
            get { return PowerModes.On; }
            set
            {
                throw new NotImplementedException("PowerMode");
            }
        }

        /// <summary>
        /// Gets the driver version.
        /// </summary>
        /// <value>
        /// The driver version.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <example>
        /// <code language="C#">
        ///      Debug.Print ("Current driver version : "+_relay.DriverVersion);
        /// </code>
        /// </example>
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
        /// <exception cref="System.NotImplementedException">This module does not offer a Reset feature.</exception>
        public bool Reset(ResetModes resetMode)
        {
            throw new NotImplementedException("Reset");
        }
    }
}
