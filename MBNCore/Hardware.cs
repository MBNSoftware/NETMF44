/* 
* MikroBus.Net main assembly 
* 
* Hardware class and StatusLed class 
* 
* Version 2.4 - 31/03/2016, Christophe Gerbier
* - Added F429Disco Carrier board type
* - Added the RemoveReservedPin() method
* 
* Version 2.2 - Christophe Gerbier
* - Added firmware SKU check to determine board's type
* - Board types enums changed to reflect this check and add more boards (with new Prime firmware, mainly)
*
* Version 2.1 - Christophe Gerbier
* - Removed the static Flash code and replaced it with the new Storage/OnboardFlash code
* - Removed the indicators about Flash chip used or unused
*
* Version 2.0 :
*  - Changed namespaces
*  - Added the public BoardType property
*  - Removed #if/#endif for Dalmatian v1 (a separate dedicated core has been created for it)
* 
* Version 1.1 : 
*  - Changed the I²C check pins routine to accomodate with non-I²C devices using SCL/SDA pins for non I²C operations
*  
* Version 1.01 : 
* - Revision to include the AnalogChannel to the socket definitions by Stephen Cardinale. 
* 
* Version 1.0 : 
* - Initial revision coded by Justin Wilson and Christophe Gerbier 
* 
* 
* References needed : 
* Microsoft.SPOT.Hardware 
* Microsoft.SPOT.Native 
* mscorlib 
* 
* Copyright 2014 MikroBus.Net 
* Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at 
* http://www.apache.org/licenses/LICENSE-2.0 
* Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
* either express or implied. See the License for the specific language governing permissions and limitations under the License. 
*/

using System;
using System.Collections;
using System.Threading;
using MBN.Enums;
using MBN.Exceptions;
using Microsoft.SPOT.Hardware;

namespace MBN
{
    /// <summary> 
    /// Main Hardware class for MikroBus.Net board 
    /// </summary> 
    public static class Hardware
    {
        /// <summary> 
        /// MikroBus socket description 
        /// </summary> 
        public class Socket
        {
            /// <summary> 
            /// Analog pin 
            /// </summary> 
            public Cpu.Pin An;

            /// <summary> 
            /// Analog Channel asociated with the socket 
            /// </summary> 
            public Cpu.AnalogChannel AnalogChannel;

            /// <summary> 
            /// UART COM port 
            /// </summary> 
            public String ComPort;

            /// <summary> 
            /// Chip Select pin for SPI 
            /// </summary> 
            public Cpu.Pin Cs;

            /// <summary> 
            /// Interrupt pin 
            /// </summary> 
            public Cpu.Pin Int;

            /// <summary> 
            /// SPI Master IN Slave OUT pin 
            /// </summary> 
            public Cpu.Pin Miso;

            /// <summary> 
            /// SPI Master OUT Slave IN pin 
            /// </summary> 
            public Cpu.Pin Mosi;

            /// <summary> 
            /// Socket's name 
            /// </summary> 
            public String Name;

            /// <summary> 
            /// PWM pin 
            /// </summary> 
            public Cpu.Pin Pwm;

            /// <summary> 
            /// PWM Channel 
            /// </summary> 
            public Cpu.PWMChannel PwmChannel;

            /// <summary> 
            /// Reset pin 
            /// </summary> 
            public Cpu.Pin Rst;

            /// <summary> 
            /// UART Receive pin 
            /// </summary> 
            public Cpu.Pin Rx;

            /// <summary> 
            /// SPI Clock line 
            /// </summary> 
            public Cpu.Pin Sck;

            /// <summary> 
            /// I²C Serial Clock 
            /// </summary> 
            public Cpu.Pin Scl;

            /// <summary> 
            /// I²C Serial Data 
            /// </summary> 
            public Cpu.Pin Sda;

            /// <summary> 
            /// SPI module 
            /// </summary> 
            public SPI.SPI_module SpiModule;

            /// <summary> 
            /// UART Transmit pin 
            /// </summary> 
            public Cpu.Pin Tx;

            /// <summary> 
            /// Analog Output Channel
            /// </summary> 
            public Cpu.AnalogOutputChannel AnalogOutChannel;
        }

        /// <summary> 
        /// Socket #1 on MikroBus.Net Board 
        /// </summary> 
        public static readonly Socket SocketOne = new Socket
        {
            An = Pin.PA6,
            Rst = Pin.PA2,
            Cs = Pin.PA3,
            Pwm = Pin.PE9,
            Int = Pin.PA1,
            Scl = Pin.PB6,
            Sda = Pin.PB7,
            Rx = Pin.PD9,
            Tx = Pin.PD8,
            Sck = Pin.PB3,
            Miso = Pin.PB4,
            Mosi = Pin.PB5,
            ComPort = "COM3",
            SpiModule = SPI.SPI_module.SPI1,
            PwmChannel = Cpu.PWMChannel.PWM_4,
            AnalogChannel = Cpu.AnalogChannel.ANALOG_2,
            AnalogOutChannel = Cpu.AnalogOutputChannel.ANALOG_OUTPUT_NONE,
            Name = "SocketOne"
        };

        /// <summary> 
        /// Socket #2 on MikroBus.Net Board 
        /// </summary> 
        public static readonly Socket SocketTwo = new Socket
        {
            An = Pin.PA4,
            Rst = Pin.PE1,
            Cs = Pin.PE0,
            Pwm = Pin.PD15,
            Int = Pin.PB9,
            Scl = Pin.PB6,
            Sda = Pin.PB7,
            Rx = Pin.PD6,
            Tx = Pin.PD5,
            Sck = Pin.PB3,
            Miso = Pin.PB4,
            Mosi = Pin.PB5,
            ComPort = "COM2",
            SpiModule = SPI.SPI_module.SPI1,
            PwmChannel = Cpu.PWMChannel.PWM_3,
            AnalogChannel = Cpu.AnalogChannel.ANALOG_0,
            AnalogOutChannel = Cpu.AnalogOutputChannel.ANALOG_OUTPUT_0,
            Name = "SocketTwo"
        };

        /// <summary> 
        /// Socket #3 on MikroBus.Net Board 
        /// </summary> 
        public static readonly Socket SocketThree = new Socket
        {
            An = Pin.PA7,
            Rst = Pin.PD12,
            Cs = Pin.PD11,
            Pwm = Pin.PD13,
            Int = Pin.PC8,
            Scl = Pin.PB6,
            Sda = Pin.PB7,
            Rx = Pin.PC7,
            Tx = Pin.PC6,
            Sck = Pin.PC10,
            Miso = Pin.PC11,
            Mosi = Pin.PC12,
            SpiModule = SPI.SPI_module.SPI3,
            ComPort = "COM6",
            PwmChannel = Cpu.PWMChannel.PWM_1,
            AnalogChannel = Cpu.AnalogChannel.ANALOG_3,
            AnalogOutChannel = Cpu.AnalogOutputChannel.ANALOG_OUTPUT_NONE,
            Name = "SocketThree"
        };

        /// <summary> 
        /// Socket #4 on MikroBus.Net Board 
        /// </summary> 
        public static readonly Socket SocketFour = new Socket
        {
            An = Pin.PA5,
            Rst = Pin.PD0,
            Cs = Pin.PD1,
            Pwm = Pin.PD14,
            Int = Pin.PA14,
            Scl = Pin.PB6,
            Sda = Pin.PB7,
            Rx = Pin.PA10,
            Tx = Pin.PA9,
            Sck = Pin.PC10,
            Miso = Pin.PC11,
            Mosi = Pin.PC12,
            SpiModule = SPI.SPI_module.SPI3,
            ComPort = "COM1",
            PwmChannel = Cpu.PWMChannel.PWM_2,
            AnalogChannel = Cpu.AnalogChannel.ANALOG_1,
            AnalogOutChannel = Cpu.AnalogOutputChannel.ANALOG_OUTPUT_1,
            Name = "SocketFour"
        };

        /// <summary> 
        /// Class needed to check used pins on sockets 
        /// </summary> 
        private class UsedPinsStruct
        {
            /// <summary> 
            /// The pin 
            /// </summary> 
            public Cpu.Pin Pin;

            /// <summary> 
            /// The socket 
            /// </summary> 
            public Socket Sock;
        }

        /// <summary> 
        /// Array containing the list of used pins per socket 
        /// </summary> 
        internal static ArrayList UsedPins;

        /// <summary> 
        /// The common I²C bus 
        /// </summary> 
        public static I2CDevice I2CBus;

        /// <summary> 
        /// The common SPI bus 
        /// </summary> 
        public static SPI SPIBus;

        internal static readonly object I2CLock, SpiLock;
        internal static BoardTypes BoardID;
        /// <summary>
        /// Tells whether the onboard flash memory is available or not.
        /// </summary>
        public static Boolean FlashAvailable;

        private static Boolean _calledByCheckI2C;

        /// <summary> 
        /// Onboard Led 1 
        /// </summary> 
        public static readonly OutputPort Led1;

        /// <summary> 
        /// Onboard Led 2 
        /// </summary> 
        public static readonly OutputPort Led2;

        /// <summary> 
        /// Onboard Led 3
        /// </summary> 
        public static readonly OutputPort Led3;

        // Constructor 
        static Hardware()
        {
            BoardID = GetBoardID();
            FlashAvailable = (BoardID == BoardTypes.QuailV2 || BoardID == BoardTypes.QuailV22);
            if (BoardType == BoardTypes.F429Carrier)
            {
                Led1 = new OutputPort(Pin.PG13, false);
                Led2 = new OutputPort(Pin.PG14, false);
            }
            else
            {
                Led1 = new OutputPort(Pin.PE15, false);
                Led2 = new OutputPort(Pin.PE10, false);
                Led3 = new OutputPort(Pin.PC3, false);
            }
            UsedPins = new ArrayList();
            SpiLock = new object();
            I2CLock = new object();
            Thread.Sleep(1000);     // Useful for I²C/SPI devices
        }

        /// <summary>
        /// Gets the type of the board.
        /// </summary>
        /// <value>
        /// The type of the board.
        /// </value>
        /// <seealso cref="BoardTypes"/>
        public static BoardTypes BoardType
        {
            get { return BoardID; }
        }

        private static BoardTypes GetBoardID()
        {
            BoardTypes boardType;
            if (SystemInfo.SystemID.SKU == 0)
            {
                var pc0 = new InputPort(Pin.PC0, false, Port.ResistorMode.Disabled).Read() ? (Byte) 1 : (Byte) 0;
                var pc1 = new InputPort(Pin.PC1, false, Port.ResistorMode.Disabled).Read() ? (Byte) 1 : (Byte) 0;
                var pc2 = new InputPort(Pin.PC2, false, Port.ResistorMode.Disabled).Read() ? (Byte) 1 : (Byte) 0;
                var id = pc0 + (pc1 << 1) + (pc2 << 2);

                switch (id)
                {
                    case 1:
                        boardType = BoardTypes.Tuatara;
                        break;
                    case 2:
                        boardType = BoardTypes.Dalmatian;
                        break;
                    case 3:
                        boardType = BoardTypes.QuailV2;
                        break;
                    default:
                        boardType = BoardTypes.Unknown;
                        break;
                }
            }
            else
            {
                switch (SystemInfo.SystemID.SKU)
                {
                    case 4020:
                        boardType = BoardTypes.QuailV22;
                        break;
                    case 5010:
                        boardType = BoardTypes.F429Carrier;
                        break;
                    default:
                        boardType = BoardTypes.Unknown;
                        break;
                }
            }
            return boardType;
        }

        /// <summary> 
        /// Checks if any pin needed by the driver is already in use by another driver 
        /// </summary> 
        /// <param name="socket">The specified socket.</param> 
        /// <param name="pins">The requested pins.</param> 
        /// <exception cref="InvalidOperationException">Thrown if a pin is already in use by another driver.</exception> 
        public static void CheckPins(Socket socket, params Cpu.Pin[] pins)
        {
            if (BoardType == BoardTypes.Dalmatian && socket == SocketThree)
            {
                throw new SocketUseException("Socket 3 unavailable on Dalmatian board.");
            }
            if (UsedPins.Count == 0)
            // First time a driver is created, so add its pins to the "Used pins" array immediately. No further checking necessary. 
            {
                for (var i = 0; i < pins.Length; i++)
                {
                    UsedPins.Add(new UsedPinsStruct { Pin = pins[i], Sock = socket });
                }
            }
            else
            {
                foreach (UsedPinsStruct ups in UsedPins)
                {
                    for (var i = 0; i < pins.Length; i++)
                    {
                        if (pins[i] == 0) { break; }
                        if (!_calledByCheckI2C)
                        // If we're coming from the I²C check method, no need to check for I²C pins as it was already done
                        {
                            // If an I²C device has already been created before this call, then 
                            // check that the current driver does not use SCL/SDA pins (without adding them to used pins, since SCL/SDA are shared) 
                            if (I2CBus != null)
                            {
                                if ((pins[i] == socket.Scl) || (pins[i] == socket.Sda))
                                {
                                    _calledByCheckI2C = false;
                                    throw new PinInUseException(socket.Name + ", pin " + ups.Pin + " already in use.");
                                }
                            }
                        }
                        // Check for a non-I²C driver 
                        if (ups.Sock == socket && pins[i] == ups.Pin)
                        {
                            _calledByCheckI2C = false;
                            // Since at least one pin is not available, the driver won't be able to work correctly. 
                            // So exit immediately and don't check any other pin 
                            throw new PinInUseException(socket.Name + ", pin " + ups.Pin + " already in use.");
                        }
                    }
                }
                // If we're here, then it means that none of the requested pins were already in use. So add them all to the UsedPins array. 
                for (var i = 0; i < pins.Length; i++)
                {
                    UsedPins.Add(new UsedPinsStruct { Pin = pins[i], Sock = socket });
                }
                _calledByCheckI2C = false;
            }
        }

        /// <summary> 
        /// Checks if I²C SCL/SDA pins are already in use by another non-I²C driver 
        /// </summary> 
        /// <param name="socket">The specified socket.</param> 
        /// <param name="pins">Extra pins (other than SCL/SDA) used by the driver</param> 
        /// <exception cref="InvalidOperationException">Thrown if SCL or SDA pin is already in use by another driver.</exception> 
        public static void CheckPinsI2C(Socket socket, params Cpu.Pin[] pins)
        {
            if (BoardType == BoardTypes.Dalmatian && socket == SocketThree)
            {
                throw new SocketUseException("Socket 3 unavailable on Dalmatian board.");
            }
            foreach (UsedPinsStruct ups in UsedPins)
            {
                if ((ups.Pin == socket.Sda || ups.Pin == socket.Scl) && (I2CBus == null))
                {
                    // One of the I²C pins is already used by another non-I²C driver 
                    throw new PinInUseException(socket.Name + ", pin " + ups.Pin + " already in use.");
                }
            }
            if (I2CBus == null)     // If it doesn't already exist, create the I²C bus with an empty configuration to physically reserve I²C pins
            {
                I2CBus = new I2CDevice(null);
                UsedPins.Add(new UsedPinsStruct { Pin = socket.Scl, Sock = socket });
                UsedPins.Add(new UsedPinsStruct { Pin = socket.Sda, Sock = socket });
            }
            //if (pins.Length <= 2) return; // if no other non-I²C pins, then no need to call the check method
            _calledByCheckI2C = true; // Set to true to ignore I²C pins check
            var count = -1;
            var newPins = new Cpu.Pin[10];
            // Remove the I²C pins for the "non-I²C" check that will follow
            foreach (var p in pins)
            {
                if ((p != socket.Scl) && (p != socket.Sda))
                {
                    newPins[++count] = p;
                }
            }
            CheckPins(socket, newPins); // Check other non-I²C pins 
        }

        /// <summary>
        /// Frees up the previously used pins for a driver non longer in use
        /// </summary>
        /// <param name="socket">The socket on which the driver was created</param>
        /// <param name="pins">The list of pins to release</param>
        /// <returns>True if all pins have been removed, False otherwise</returns>
        public static Boolean RemoveReservedPins(Socket socket, params Cpu.Pin[] pins)
        {
            var currentUsedPincount = UsedPins.Count;

            var enumerator = UsedPins.GetEnumerator();
            enumerator.Reset();

            foreach (var pin in pins)
            {
                while (enumerator.MoveNext())
                {
                    var currentPin = (UsedPinsStruct)enumerator.Current;

                    if (socket.Sck == pin) break; // Bus pin - do not remove.
                    if (socket.Mosi == pin) break; // Bus pin - do not remove.
                    if (socket.Miso == pin) break; // Bus pin - do not remove.
                    if (socket.Scl == pin) break; // Bus pin - do not remove.
                    if (socket.Sda == pin) break; // Bus pin - do not remove.
                    if (currentPin == null) break; // Already removed this one, so move on to next pin in the parameter array.
                    if (currentPin.Pin == pin) UsedPins.Remove(currentPin);
                }
                enumerator.Reset();
            }

            return currentUsedPincount - pins.Length == UsedPins.Count;
        }
    }
}
