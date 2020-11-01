/* Version 1.0 :
 *  - Initial version coded by Stephen Cardinale
 *  - Not implemented - NO_RELOAD_FROM_OTP_MASK and HEATER_MASK
 *  Version 1.1 :
 *  - Complete re-write and refactoring.
 *  - Bug Fix - Corrected a bug that would report incorrect return value on ResetModes.Soft
 *  - Implementation of the MikroBusNet Interface requirements.
 *  - Implemented NoReloadFromOTP - no reload of calibration data from One Time Programmable Memory before measurements.
 *  - Implemented enabling the On-Board IC Heater Capabilities useful for testing sensor diagnostics.
 *  - Implemented EndOfBattery property for low voltage detection.
 *  - Implemented Alarm functionality that is raised through the TemperatureHumidityMeasured Event and Low and High Temperature and Humidity Alarm Threshold Properties. 
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
using S = System;

namespace MBN.Modules
{
    /// <summary>
    ///     A class holding the values of various alarm setting parameters.
    /// </summary>
    public class SHT11AlarmThresholds
    {
        /// <summary>
        ///     Low temperature threshold
        /// </summary>
        public float LowTemperatureThreshold { get; set; }

        /// <summary>
        ///     High temperature threshold
        /// </summary>
        public float HighTemperatureThreshold  { get; set; }
        
        /// <summary>
        ///     Low humidity threshold
        /// </summary>
        public float LowHumidityThreshold  { get; set; }

        /// <summary>
        /// High humidity threshold
        /// </summary>
        /// <returns></returns>
        public float HighHumidityThreshold  { get; set; }

        /// <summary>
        ///     Default constructor of the AlarmSetting Class
        /// </summary>
        /// <param name="lowTemperatureThreshold">The low temperature setting.</param>
        /// <param name="highTemperatureThreshold">The high temperature setting.</param>
        /// <param name="lowHumidityThreshold">the low humidity setting.</param>
        /// <param name="highHumidityThreshold">the low humidity setting.</param>
        public SHT11AlarmThresholds(float lowTemperatureThreshold, float highTemperatureThreshold, float lowHumidityThreshold, float highHumidityThreshold)
        {
            // Make sure the alarm setting is within the device's range (-40°C to 123.8°C)
            LowTemperatureThreshold = lowTemperatureThreshold < -40F ? -40F : lowTemperatureThreshold > 123.8F ? 123.8F : lowTemperatureThreshold;
            HighTemperatureThreshold = highTemperatureThreshold < -40F ? -40F : highTemperatureThreshold > 123.8F ? 123.8F : highTemperatureThreshold;
            LowHumidityThreshold = lowHumidityThreshold < 0F ? 0F : lowHumidityThreshold > 100F ? 100F : lowHumidityThreshold;
            HighHumidityThreshold = highHumidityThreshold < 0F ? 0F : highHumidityThreshold > 100F ? 100F : highHumidityThreshold;

            // Verify the Low Temperature alarm setting is not greater than or equal to the High Temperature Alarm setting, if so throw an exception.
            if (lowTemperatureThreshold >= highTemperatureThreshold)
            {
                throw new ArgumentException("Low Temperature Alarm setting must not be greater than or equal to the High Temperature Alarm setting.", "lowTemperatureThreshold");
            }
            
            // Verify the Low Temperature alarm setting is not greater than or equal to the High Temperature Alarm setting, if so throw an exception.
            if (lowHumidityThreshold >= highHumidityThreshold)
            {
                throw new ArgumentException("Low Temperature Alarm setting must not be greater than or equal to the High Temperature Alarm setting.", "lowHumidityThreshold");
            }
        }
    }
}