/*
 * Wifi driver skeleton generated on 12/10/2014 10:18:19 PM
 * 
 * Event namespace implementation
 * 
 * Initial revision coded by Christophe
 * 
 * References needed :  (change according to your driver's needs)
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Native
 *  MikroBusNet
 *  mscorlib
 *  
 * Copyright 2014 Christophe
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 * 
 * Follow the TODO tasks to complete the driver code and remove this line
 */
using System;

//TODO: review the XML comments so that they are applicable to your driver

namespace MBN.Modules
{
    public partial class Wifi
    {
        //TODO: implement events here if needed
        /// <summary>
        /// Delegate for the DemoEventFired event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DemoEventEventArgs"/> instance containing the event data.</param>
        public delegate void DemoEventEventHandler(object sender, DemoEventEventArgs e);

        /// <summary>
        /// Class holding arguments for the DemoEventFired event.
        /// </summary>
        public class DemoEventEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DemoEventEventArgs"/> class.
            /// </summary>
            /// <param name="eventArg1">First argument of the event.</param>
            /// <param name="eventArg2">Second argument of the event.</param>
            /// <param name="eventArg3">Third argument of the event.</param>
            public DemoEventEventArgs(Byte eventArg1, Boolean eventArg2, Boolean eventArg3)
            {
                EventArg1 = eventArg1;
                EventArg2 = eventArg2;
                EventArg3 = eventArg3;
            }
            /// <summary>
            /// Gets the first argument of the event
            /// </summary>
            /// <value>
            /// A value representing something useful for this event
            /// </value>
            public Byte EventArg1 { get; private set; }
            /// <summary>
            /// Gets the second argument of the event
            /// </summary>
            /// <value>
            /// A value representing something useful for this event
            /// </value>
            public Boolean EventArg2 { get; private set; }
            /// <summary>
            /// Gets the third argument of the event
            /// </summary>
            /// <value>
            /// A value representing something useful for this event
            /// </value>
            public Boolean EventArg3 { get; private set; }
        }
    }
}