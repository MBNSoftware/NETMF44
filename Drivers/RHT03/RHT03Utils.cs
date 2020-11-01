/*
 * BMP180 Sensor Driver for MikroBusNet
 * 
 * - Based on the Bosch BMP180 Absolute Pressure Sensor.
 * - This driver will also work for the Bosch BMP085 Absolute Pressure Sensor.
 * Version 1.0 -Initial version coded by Stephen Cardinale
 *  - Never released.
 * Version 2.0 
 *  * - Implementation of MBN Interface and Namespaces requirements.
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

using MBN.Enums;

// ReSharper disable once CheckNamespace
namespace MBN.Utilities
{
    /// <summary>
    /// A Utility class with methods relating to Temperature conversion. 
    /// </summary>
    public abstract class Temperature
    {
        /// <summary>
        /// A utility method to convert from on temperature scale to another.
        /// </summary>
        /// <param name="fromUnit">Convert from this scale <see cref="TemperatureUnits"/>.</param>
        /// <param name="toUnit">Convert to this scale <see cref="TemperatureUnits"/>.</param>
        /// <param name="temperature">The temperature to convert.</param>
        /// <returns>The temperature converted to the selected scale.</returns>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("Temperature - " + Temperature.ConvertTo(TemperatureUnits.Celsius, TemperatureUnits.Fahrenheit, sensorData.Temperature).ToString("f2") + "°F");
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Temperature - " <![CDATA[&]]> Temperature.ConvertTo(TemperatureUnits.Celsius, TemperatureUnits.Fahrenheit, sensorData.Temperature).ToString("f2") <![CDATA[&]]> "°F")
        /// </code>
        /// </example>
        public static float ConvertTo(TemperatureUnits fromUnit, TemperatureUnits toUnit, float temperature)
        {
            switch (fromUnit)
            {
                case TemperatureUnits.Kelvin:
                {
                    switch (toUnit)
                    {
                        case TemperatureUnits.Celsius:
                            return temperature - 273.15F;
                        case TemperatureUnits.Fahrenheit:
                            return ((temperature - 32)*5/9) - 273.15F;
                        case TemperatureUnits.Kelvin:
                            return temperature;
                    }
                    break;
                }
                case TemperatureUnits.Celsius:
                {
                    switch (toUnit)
                    {
                        case TemperatureUnits.Celsius:
                            return temperature;
                        case TemperatureUnits.Fahrenheit:
                            return (temperature*9/5) + 32;
                        case TemperatureUnits.Kelvin:
                            return temperature + 273.15F;
                    }
                    break;
                }
                case TemperatureUnits.Fahrenheit:
                {
                    switch (toUnit)
                    {
                        case TemperatureUnits.Celsius:
                            return (temperature - 32)*5/9;
                        case TemperatureUnits.Fahrenheit:
                            return temperature;
                        case TemperatureUnits.Kelvin:
                            return (float) (((temperature - 32)/1.8000) + 273.15);
                    }
                    break;
                }
            }
            return float.MinValue;
        }
    }
}