/*
 * Led8x8 R Click board driver
 * 
 * Version 1.0 :
 *  - Initial revision coded by Christophe Gerbier
 * 
 * References needed :
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Hardware.PWM
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
    public partial class Led8X8Click
    {
        internal static class Registers
        {
// ReSharper disable InconsistentNaming
            public const Byte DECODE_MODE = 0x09;
            public const Byte INTENSITY = 0x0A;
            public const Byte SCAN_LIMIT = 0x0B;
            public const Byte SHUTDOWN = 0x0C;
            public const Byte NO_OP = 0x00;
            public const Byte DISPLAY_TEST = 0x0F;
// ReSharper restore InconsistentNaming
        }
    }
}
