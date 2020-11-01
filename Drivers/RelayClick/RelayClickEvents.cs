/*
 * Relay Click board driver
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

using System;

// ReSharper disable once CheckNamespace
namespace MBN.Modules
{
    public partial class RelayClick
    {
        /// <summary>
        /// Public delegate for the RelayStateChanged event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="RelayStateChangedEventArgs"/> instance containing the event data.</param>
        public delegate void RelayStateChangedEventHandler(object sender, RelayStateChangedEventArgs e);

        /// <summary>
        /// RelayStateChanged arguments
        /// </summary>
        public class RelayStateChangedEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RelayStateChangedEventArgs"/> class.
            /// </summary>
            /// <param name="eRelay">The specified relay.</param>
            /// <param name="eOldState">The state of the relay before the event was thrown.</param>
            /// <param name="eNewState">The new state of the relay.</param>
            public RelayStateChangedEventArgs(Byte eRelay, Boolean eOldState, Boolean eNewState)
            {
                Relay = eRelay;
                OldState = eOldState;
                NewState = eNewState;
            }

            /// <summary>
            /// Gets the relay number.
            /// </summary>
            /// <value>
            /// The relay number.
            /// </value>
            public Byte Relay { get; private set; }

            /// <summary>
            /// Gets a value indicating the state of the relay before the event was thrown.
            /// </summary>
            /// <value>
            ///   <c>true</c> if relay was ON; otherwise, <c>false</c>.
            /// </value>
            public Boolean OldState { get; private set; }

            /// <summary>
            /// Gets a value indicating the new state of the relay.
            /// </summary>
            /// <value>
            ///   <c>true</c> if relay has been set to ON; otherwise, <c>false</c>.
            /// </value>
            public Boolean NewState { get; private set; }
        }
    }
}