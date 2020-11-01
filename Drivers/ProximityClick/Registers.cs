/*
 * Proximity Click board driver
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
    public partial class ProximityClick
    {
        internal static class Registers
        {
            public const Byte Command = 0x80; 
            public const Byte ProductIDRevision = 0x81;
            public const Byte ProximityRate = 0x82;
            public const Byte IRLedCurrent = 0x83;
            public const Byte AmbientLightParameter = 0x84;
            public const Byte AmbientLightResult = 0x85;
            public const Byte ProximityMeasurementResult = 0x87;
            public const Byte ProximityModulator = 0x8F;
        }
    }
}
