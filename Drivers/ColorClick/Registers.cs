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

namespace MBN.Modules
{
    public partial class ColorClick
    {
        /// <summary>
        /// Internal chip registers used by the driver
        /// </summary>
        internal static class Registers
        {
// ReSharper disable InconsistentNaming
            /// <summary>
            /// Enables states and interrupts
            /// </summary>
            public const Byte ENABLE = 0x80;
            /// <summary>
            /// RGBC ADC time
            /// </summary>
            public const Byte ATIME = 0x81;
            /// <summary>
            /// Wait time
            /// </summary>
            public const Byte WTIME = 0x83;
            /// <summary>
            /// Configuration
            /// </summary>
            public const Byte CONFIG = 0x8D;
            /// <summary>
            /// Gain control register
            /// </summary>
            public const Byte CONTROL = 0x8F;
            /// <summary>
            /// Device ID
            /// </summary>
            public const Byte ID = 0x92;
            /// <summary>
            /// Device status
            /// </summary>
            public const Byte STATUS = 0x93;
            /// <summary>
            /// Clear ADC register
            /// </summary>
            public const Byte CDATA = 0x94;
            /// <summary>
            /// Red ADC register
            /// </summary>
            public const Byte RDATA = 0x96;
            /// <summary>
            /// Green ADC register
            /// </summary>
            public const Byte GDATA = 0x98;
            /// <summary>
            /// Blue ADC register
            /// </summary>
            public const Byte BDATA = 0x9A;
// ReSharper restore InconsistentNaming
        }
    }
}
