/*
 * RTC2 Click board driver
 * 
 * Version 1.0 :
 *  - Initial version coded by Stephen Cardinale
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

// ReSharper disable once CheckNamespace

namespace MBN.Modules
{
// ReSharper disable once InconsistentNaming
    public partial class RTC2Click
    {
        /// <summary>
        /// Class holding the Register Address of the DS1307 IC used on the RTC2 Click Board.
        /// </summary>
        public class Registers
        {
            // ReSharper disable InconsistentNaming
            /// <summary>
            ///     The I2C address of the DS1307.
            /// </summary>
            internal const int I2C_ADDRESS = 0x68;

            /// <summary>
            ///     The start address of the RTC Register space.
            /// </summary>
            /// <value>0x00</value>
            public const byte RTC_START_ADDRESS = 0x00;

            /// <summary>
            ///     The end address of the RTC Register space.
            /// </summary>
            /// <value>0x06</value>
           public const byte RTC_END_ADDRESS = 0x06;

            /// <summary>
            ///     Square Wave Frequency Generator register address
            /// </summary>
            internal const byte SQUARE_WAVE_CTRL_REGISTER_ADDRESS = 0x07;

            /// <summary>
            ///     The start addresses of the User RAM registers
            /// </summary>
            /// <value>0x08</value>
            public const byte USER_RAM_START_ADDRESS = 0x08;

            /// <summary>
            /// The end addresses of the User RAM registers
            /// </summary>
            /// <value>0x38</value>
            public const byte USER_RAM_END_ADDRESS = 0x3F - 0x07; // Address 0x38 is for the RTC Set Register.

            /// <summary>
            ///     The start address space for the RTC_SET_Bit - Reserved.
            /// </summary>
            internal const byte RTC_SET_BIT_ADDRESS = USER_RAM_END_ADDRESS + 0x07;

            /// <summary>
            /// Total size of the user RAM block in Bytes
            /// </summary>
            /// <value>48</value>
            public const byte USER_RAM_SIZE = 56 - 8;
            // Actual user Ram size is 56, reserving one byte for the RTC Set Register.

            // ReSharper restore InconsistentNaming
        }
    }
}