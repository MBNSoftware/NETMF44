/*
 * Color Click board driver
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
    public partial class ColorClick
    {
        /// <summary>
        /// Public delegate for the RelayStateChanged event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="DataReadyEventArgs"/> instance containing the event data.</param>
        public delegate void DataReadyEventHandler(object sender, DataReadyEventArgs e);

        /// <summary>
        /// DataReady arguments
        /// </summary>
        public class DataReadyEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DataReadyEventArgs"/> class.
            /// </summary>
            /// <param name="red">The value of the Red channel.</param>
            /// <param name="green">The value of the Green channel.</param>
            /// <param name="blue">The value of the Blue channel.</param>
            /// <param name="clear">The value of the Clear channel.</param>
            public DataReadyEventArgs(Double red, Double green, Double blue, Double clear)
            {
                Clear = clear;
                Red = red;
                Green = green;
                Blue = blue;
            }

            /// <summary>
            /// Gets the Clear channel value.
            /// </summary>
            public Double Clear { get; private set; }

            /// <summary>
            /// Gets the Red channel value.
            /// </summary>
            public Double Red { get; private set; }

            /// <summary>
            /// Gets the Green channel value.
            /// </summary>
            public Double Green { get; private set; }

            /// <summary>
            /// Gets the Blue channel value.
            /// </summary>
            public Double Blue { get; private set; }
        }
    }
}