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
using MBN.Modules;
using s = System;

// ReSharper disable once CheckNamespace
namespace MBN.Events
{
    /// <summary>
    ///     Represents the delegate that is used for the <see cref="RTCClick.AlarmTriggered"/> event.
    /// </summary>
    /// <param name="sender">The <see cref="RTCClick"/> that raises the event.</param>
    /// <param name="e">The <see cref="RTCClickAlarmEventArgs"/>.</param>
    public delegate void RTCClickAlarmEventHandler(object sender, RTCClickAlarmEventArgs e);

    /// <summary>
    ///     A class holding the values of the Alarm Event that triggered the Alarm (the alarm type and time of the alarm occurrence) see <see cref="RTCClick.AlarmTriggered"/> event.
    /// </summary>
    public class RTCClickAlarmEventArgs
    {
        internal RTCClickAlarmEventArgs(RTCClick.AlarmBehavior alarmType, DateTime alarmEventTime)
        {
            AlarmType = alarmType;
            AlarmEventEventTime = alarmEventTime;
        }

        /// <summary>
        ///     The property holding the <see cref="AlarmType"/> that triggered the alarm.
        /// </summary>
        public RTCClick.AlarmBehavior AlarmType { get; private set; }

        /// <summary>
        ///     The property holding the <see cref="DateTime"/> that the alarm event occurred.
        /// </summary>
        public DateTime AlarmEventEventTime { get; private set; }
    }
}