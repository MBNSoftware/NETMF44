/*
 * RTC Click board driver
 * 
 * Version 1.0 :
 *  - Initial revision coded by Stephen Cardinale
 *  
 *  - Not implemented - Alarm Function, Timer and Event functionality, Writing/Reading to available User SRAM.
 *                    - Note: The PCF8583 only has a four (4) year clock.
 *                      In order to correctly read he DateTime of the RTC,
 *                      The current base year is stored in SRAM at address 0x10.
 *                      If implementing SRAM functionality, please reserve this register
 *                      or adjust the storage location accordingly.
 * Version 1.1 :
 *  - Implementation of the MikroBusNet Interface requirements.
 *  - Bug Fix - Changed all I2C Read/Write methods to use the Hardware.I2CBus.Execute extension method instead of Hardware.I2CBus.Execute method.
 *  - Implemented Clock Mode Alarm functionality (Daily, Weekday and Dated Alarms).
 *  - Implemented utilization of Free CMOS Ram for user storage.
 *  
 *  - Not implemented - Timer and Event Mode functionality.
 *  
 *  Version 2.0 :
 *  - Integration of the new namespaces and new organization.
 * 
 * * References needed :
 *   - Microsoft.SPOT.Hardware
 *   - Microsoft.SPOT.Native
 *   - MikroBusNet
 *   - mscorlib
 *  
 * Copyright 2014 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System;

// ReSharper disable once CheckNamespace
namespace MBN.Extensions
{
    internal static class ByteExtensionMethods
    {
        internal static byte FromBcd(this byte value)
        {
            var lo = value & 0x0f;
            var hi = (value & 0xf0) >> 4;
            return (byte) (hi * 10 + lo);
        }

        internal static byte ToBcd(this byte value)
        {
            return (byte) ((value / 10 << 4) + value % 10);
        }

        public static int BcdToInt(byte bcd)
        {
            return (bcd & 0x0F) + ((bcd & 0xF0) >> 4) * 10;
        }

        internal static byte Constrain(this byte value, byte min, byte max)
        {
            return value >= max ? max : value <= min ? min : value;
        }

        internal static bool IsBitSet(this byte value, byte pos)
        {
            return (value & (1 << pos)) != 0;
        }
    }

    /// <summary>
    ///     DateTime Extension Methods
    /// </summary>
    public static class DateTimeExtensionMethods
    {
        /// <summary>
        ///     An extension method to determine if the year is a Leap Year.
        /// </summary>
        /// <param name="dt">The <see cref="DateTime"/> to check.</param>
        /// <returns>True is the DateTime passes is a Leap Year or otherwise false.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <example>Example usage:
        /// <code language = "C#">
        /// var thisDate = new DateTime(2016, 2, 28, 23, 59, 50);
        /// Debug.Print("Is Leap Year" + LDate.IsLeapYear());
        /// </code>
        /// <code language = "VB">
        /// Dim thisDate = New DateTime(2016, 2, 28, 23, 59, 50)
	    /// Debug.Print("Is Leap Year" <![CDATA[&]]> LDate.IsLeapYear())
        /// </code>
        /// </example>
        public static bool IsLeapYear(this DateTime dt)
        {
            if (dt.Year < 1 || dt.Year > 9999) throw new ArgumentOutOfRangeException("dt");
            return dt.Year % 4 == 0 && (dt.Year % 100 != 0 || dt.Year % 400 == 0);
        }

    }
}