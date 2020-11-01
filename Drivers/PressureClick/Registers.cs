/*
 * Pressure Click board driver
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
    public partial class PressureClick
    {
        internal static class Registers
        {
// ReSharper disable InconsistentNaming
            public const Byte RES_CONF = 0x10;
            public const Byte CTRL_REG1 = 0x20;
            public const Byte WHO_AM_I = 0x0F;
            public const Byte PRESS_OUT_L = 0x29;
            public const Byte PRESS_OUT_XL = 0x28;
            public const Byte PRESS_OUT_H = 0x2A;
            public const Byte TEMP_OUT_L = 0x2B;
            public const Byte TEMP_OUT_H = 0x2C;
// ReSharper restore InconsistentNaming
        }
    }
}
