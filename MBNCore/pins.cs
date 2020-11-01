/*
 * MikroBus.Net main assembly
 * 
 * Pins abstraction class
 * 
 * Version 2.4 : 31/03/2016 Christophe Gerbier
 *  - Added pins definitions for the F429Disco Carrier board (PF0 to PG15)
 *  
 * Version 1.0 :
 *  - Initial revision coded by Justin Wilson
 * 
 * References needed :
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Native
 *  mscorlib
 *  
 * Copyright 2014 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using Microsoft.SPOT.Hardware;

namespace MBN
{
    /// <summary>
    /// Base class of MirkoBus.Net pins assignments
    /// </summary>
    // ReSharper disable InconsistentNaming
    public class Pin
    {
        /// <summary>A value indicating that no GPIO pin is specified.</summary>
        public const Cpu.Pin GPIO_NONE = Cpu.Pin.GPIO_NONE;

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PA0 = (Cpu.Pin)(0 * 16 + 0);


        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PA1 = (Cpu.Pin)(0 * 16 + 1);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PA2 = (Cpu.Pin)(0 * 16 + 2);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PA3 = (Cpu.Pin)(0 * 16 + 3);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PA4 = (Cpu.Pin)(0 * 16 + 4);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PA5 = (Cpu.Pin)(0 * 16 + 5);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PA6 = (Cpu.Pin)(0 * 16 + 6);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PA7 = (Cpu.Pin)(0 * 16 + 7);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PA8 = (Cpu.Pin)(0 * 16 + 8);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PA9 = (Cpu.Pin)(0 * 16 + 9);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PA10 = (Cpu.Pin)(0 * 16 + 10);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PA11 = (Cpu.Pin)(0 * 16 + 11);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PA12 = (Cpu.Pin)(0 * 16 + 12);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PA13 = (Cpu.Pin)(0 * 16 + 13);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PA14 = (Cpu.Pin)(0 * 16 + 14);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PA15 = (Cpu.Pin)(0 * 16 + 15);


        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PB0 = (Cpu.Pin)(1 * 16 + 0);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PB1 = (Cpu.Pin)(1 * 16 + 1);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PB2 = (Cpu.Pin)(1 * 16 + 2);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PB3 = (Cpu.Pin)(1 * 16 + 3);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PB4 = (Cpu.Pin)(1 * 16 + 4);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PB5 = (Cpu.Pin)(1 * 16 + 5);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PB6 = (Cpu.Pin)(1 * 16 + 6);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PB7 = (Cpu.Pin)(1 * 16 + 7);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PB8 = (Cpu.Pin)(1 * 16 + 8);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PB9 = (Cpu.Pin)(1 * 16 + 9);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PB10 = (Cpu.Pin)(1 * 16 + 10);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PB11 = (Cpu.Pin)(1 * 16 + 11);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PB12 = (Cpu.Pin)(1 * 16 + 12);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PB13 = (Cpu.Pin)(1 * 16 + 13);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PB14 = (Cpu.Pin)(1 * 16 + 14);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PB15 = (Cpu.Pin)(1 * 16 + 15);



        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PC0 = (Cpu.Pin)(2 * 16 + 0);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PC1 = (Cpu.Pin)(2 * 16 + 1);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PC2 = (Cpu.Pin)(2 * 16 + 2);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PC3 = (Cpu.Pin)(2 * 16 + 3);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PC4 = (Cpu.Pin)(2 * 16 + 4);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PC5 = (Cpu.Pin)(2 * 16 + 5);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PC6 = (Cpu.Pin)(2 * 16 + 6);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PC7 = (Cpu.Pin)(2 * 16 + 7);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PC8 = (Cpu.Pin)(2 * 16 + 8);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PC9 = (Cpu.Pin)(2 * 16 + 9);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PC10 = (Cpu.Pin)(2 * 16 + 10);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PC11 = (Cpu.Pin)(2 * 16 + 11);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PC12 = (Cpu.Pin)(2 * 16 + 12);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PC13 = (Cpu.Pin)(2 * 16 + 13);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PC14 = (Cpu.Pin)(2 * 16 + 14);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PC15 = (Cpu.Pin)(2 * 16 + 15);



        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PD0 = (Cpu.Pin)(3 * 16 + 0);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PD1 = (Cpu.Pin)(3 * 16 + 1);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PD2 = (Cpu.Pin)(3 * 16 + 2);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PD3 = (Cpu.Pin)(3 * 16 + 3);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PD4 = (Cpu.Pin)(3 * 16 + 4);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PD5 = (Cpu.Pin)(3 * 16 + 5);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PD6 = (Cpu.Pin)(3 * 16 + 6);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PD7 = (Cpu.Pin)(3 * 16 + 7);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PD8 = (Cpu.Pin)(3 * 16 + 8);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PD9 = (Cpu.Pin)(3 * 16 + 9);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PD10 = (Cpu.Pin)(3 * 16 + 10);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PD11 = (Cpu.Pin)(3 * 16 + 11);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PD12 = (Cpu.Pin)(3 * 16 + 12);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PD13 = (Cpu.Pin)(3 * 16 + 13);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PD14 = (Cpu.Pin)(3 * 16 + 14);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PD15 = (Cpu.Pin)(3 * 16 + 15);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PE0 = (Cpu.Pin)(4 * 16 + 0);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PE1 = (Cpu.Pin)(4 * 16 + 1);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PE2 = (Cpu.Pin)(4 * 16 + 2);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PE3 = (Cpu.Pin)(4 * 16 + 3);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PE4 = (Cpu.Pin)(4 * 16 + 4);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PE5 = (Cpu.Pin)(4 * 16 + 5);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PE6 = (Cpu.Pin)(4 * 16 + 6);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PE7 = (Cpu.Pin)(4 * 16 + 7);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PE8 = (Cpu.Pin)(4 * 16 + 8);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PE9 = (Cpu.Pin)(4 * 16 + 9);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PE10 = (Cpu.Pin)(4 * 16 + 10);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PE11 = (Cpu.Pin)(4 * 16 + 11);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PE12 = (Cpu.Pin)(4 * 16 + 12);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PE13 = (Cpu.Pin)(4 * 16 + 13);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PE14 = (Cpu.Pin)(4 * 16 + 14);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PE15 = (Cpu.Pin)(4 * 16 + 15);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PF0 = (Cpu.Pin)(5 * 16 + 0);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PF1 = (Cpu.Pin)(5 * 16 + 1);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PF2 = (Cpu.Pin)(5 * 16 + 2);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PF3 = (Cpu.Pin)(5 * 16 + 3);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PF4 = (Cpu.Pin)(5 * 16 + 4);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PF5 = (Cpu.Pin)(5 * 16 + 5);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PF6 = (Cpu.Pin)(5 * 16 + 6);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PF7 = (Cpu.Pin)(5 * 16 + 7);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PF8 = (Cpu.Pin)(5 * 16 + 8);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PF9 = (Cpu.Pin)(5 * 16 + 9);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PF10 = (Cpu.Pin)(5 * 16 + 10);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PF11 = (Cpu.Pin)(5 * 16 + 11);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PF12 = (Cpu.Pin)(5 * 16 + 12);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PF13 = (Cpu.Pin)(5 * 16 + 13);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PF14 = (Cpu.Pin)(5 * 16 + 14);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PF15 = (Cpu.Pin)(5 * 16 + 15);


        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PG0 = (Cpu.Pin)(6 * 16 + 0);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PG1 = (Cpu.Pin)(6 * 16 + 1);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PG2 = (Cpu.Pin)(6 * 16 + 2);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PG3 = (Cpu.Pin)(6 * 16 + 3);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PG4 = (Cpu.Pin)(6 * 16 + 4);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PG5 = (Cpu.Pin)(6 * 16 + 5);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PG6 = (Cpu.Pin)(6 * 16 + 6);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PG7 = (Cpu.Pin)(6 * 16 + 7);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PG8 = (Cpu.Pin)(6 * 16 + 8);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PG9 = (Cpu.Pin)(6 * 16 + 9);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PG10 = (Cpu.Pin)(6 * 16 + 10);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PG11 = (Cpu.Pin)(6 * 16 + 11);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PG12 = (Cpu.Pin)(6 * 16 + 12);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PG13 = (Cpu.Pin)(6 * 16 + 13);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PG14 = (Cpu.Pin)(6 * 16 + 14);

        /// <summary>Digital I/O.</summary>
        public const Cpu.Pin PG15 = (Cpu.Pin)(6 * 16 + 15);


    }
    // ReSharper restore InconsistentNaming
}
