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

namespace MBN.Modules
{
// ReSharper disable once InconsistentNaming
    public partial class FTDIClick
    {
        /// <summary>
        /// Delegate for the DataReceived event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DataReceivedEventArgs"/> instance containing the event data.</param>
        public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);

        /// <summary>
        /// Class holding arguments for the DataReceived event.
        /// </summary>
        public class DataReceivedEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DataReceivedEventArgs"/> class.
            /// </summary>
            /// <param name="data">First argument of the event.</param>
            /// <param name="count">Second argument of the event.</param>
            public DataReceivedEventArgs(Byte[] data, Int32 count)
            {
                Data = data;
                Count = count;
            }
            /// <summary>
            /// Gets the first argument of the event
            /// </summary>
            /// <value>
            /// A byte array representing bytes received on the serial port
            /// </value>
            public Byte[] Data { get; private set; }
            /// <summary>
            /// Gets the second argument of the event
            /// </summary>
            /// <value>
            /// A value representing the number of bytes received
            /// </value>
            public Int32 Count { get; private set; }
        }
    }
}