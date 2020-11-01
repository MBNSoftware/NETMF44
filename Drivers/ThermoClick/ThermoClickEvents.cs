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

// ReSharper disable once CheckNamespace
namespace MBN.Modules
{
    public partial class ThermoClick
    {
        /// <summary>
        /// The <see cref="SensorFault"/> is raised when the ThermoClick senses a fault with the Thermocouple connection during a read operation.
        /// </summary>
        public event SensorFaultEventHandler SensorFault = delegate { };

        /// <summary>
        /// The delegate method used by the <see cref="SensorFault"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="ThermoClick"/> that raised the event</param>
        /// <param name="e">The <see cref="FaultType"/> that occurred.</param>
        public delegate void SensorFaultEventHandler(object sender, FaultType e);

        /// <summary>
        /// Enumeration for the type of fault.    
        /// </summary>
        public enum FaultType
        {
            /// <summary>
            ///     No Fault Detected
            /// </summary>
            NoFault = 0,

            /// <summary>
            ///     Thermocouple is shorted to VCC.
            /// </summary>
            ShortToVcc = 1,

            /// <summary>
            ///     Thermocouple is shorted to Ground.
            /// </summary>
            ShortToGround = 2,

            /// <summary>
            ///     Thermocouple is Open.
            /// </summary>
            OpenFault = 3
        }
    }
}