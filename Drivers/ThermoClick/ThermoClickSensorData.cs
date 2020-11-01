/*
 * THERMO Click Driver for MikroBus.Net
 * 
 * Version 1.0 - using Software SPI through Bit-Banging
 *  - Initial version coded Stephen Cardinale
 *
 * Version 1.1
 *  - Revision to include Hardware SPI
 *
 * Version 2.0 :
 *  - General refactoring and code cleanup.
 *  - Removed Bit-Banging Code (software SPI).
 *  - Integration of the new name spaces and new organization.
 *
 * Version 3.0
 *  - Now correctly returns negative temperatures.
 * 
 * References Needed :
 * Microsoft.Spot.Hardware
 * Microsoft.spot.Native
 * MikroBusNet
 * mscorlib
 * 
 * Copyright 2015 Stephen Cardinale and MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License. 
 */

using System;

namespace MBN.Modules
{
    public partial class ThermoClick
    {
        /// <summary>
        /// A class to hold the Sensor Data as read from the ThermoClick.
        /// </summary>
        internal class ThermoClickSensorData
        {
            private readonly byte[] _sensorData = new byte[4];

            /// <summary>
            /// Initializes a new instance of the  <see cref="ThermoClickSensorData"/> class.
            /// </summary>
            /// <param name="sensorData">Must be 4 bytes in length</param>
            public ThermoClickSensorData(byte[] sensorData)
            {
                IsSensorDataValid = (_sensorData[0] & 0x07) > 0 || (_sensorData[2] & 0x01) != 0x01;
                Array.Copy(sensorData, _sensorData, _sensorData.Length);
            }

            /// <summary>
            /// True for if the data was pass was the correct length. Does not indicate if there were faults with the sensor.
            /// </summary>
            internal bool IsSensorDataValid { get; private set; }

            /// <summary>
            /// Thermocouple temperature in degrees Celsius.
            /// </summary>
            /// <returns>
            /// The temperature of the Thermocouple attached to the ThermoClick, or Negative Infinity if the ThermoClick detects an error.
            /// </returns>
            public Single ThermocoupleTemperature
            {
                get
                {
                    if (!IsSensorDataValid) return Single.MinValue;

                    var data = (_sensorData[0] << 8 | _sensorData[1]) >> 2;

                    if ((data & 0x2000) != 0x2000) return (data << 2) * 0.0625F;
                    data <<= 2;
                    data = ~data;
                    data &= 0x1FFF;
                    data += 1;
                    data *= -1;
                    return data * 0.0625F;
                }
            }

            /// <summary>
            ///  Gets the Cold Junction Temperature in degrees Celsius
            /// </summary>
            /// <returns>
            /// The temperature of the internal temperature of the MAX31855 IC on the ThermoClick, or Negative Infinity if the ThermoClick detects an error.
            /// </returns>
            /// <remarks>
            /// This could be useful for measuring ambient temperature inside an enclosure that the ThermoClick is mounted inside of.
            /// </remarks>
            internal Single ColdJunctionTemperature
            {
                get
                {
                    if (!IsSensorDataValid) return Single.MinValue;

                    var data = (_sensorData[2] << 8 | _sensorData[3]) >> 4;

                    if ((data & 0x800) != 0x800) return (data) * 0.0625F;
                    {
                        data = ~data;
                        data &= 0x7FF;
                        data += 1;
                        data *= -1;
                        return data * 0.0625F;
                    }
                }
            }
        }
    }
}