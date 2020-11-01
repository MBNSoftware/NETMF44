/*
 * 7Seg Click board driver
 * 
 * Version 2.0
 *  - Conformance to the new namespaces and organization
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
using System.Reflection;
using MBN.Enums;
using Microsoft.SPOT.Hardware;
using MBN.Extensions;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the 7Seg board driver
    /// <para><b>Pins used :</b> Miso, Mosi, Cs, Sck, Pwm</para>
    /// </summary>
    public class SevenSegClick : IDriver
    {
        private readonly SPI.Configuration _spiConfig;                // SPI configuration
        private Double _pwmLevel;                            // Brightness level
        private readonly PWM _pwm;                           // Brightness control

        #region CharTable
        /// <summary>
        /// Contains binary values for digits and chars
        /// </summary>
        public byte[] CharTable = 
        {
            0x80, // '-'
            0x01, // '.'   Bit order :
            0x00, // '/'   (g)(f)(e)(d)(c)(a)(b)(dp)
            0x7E, // '0'
            0x0A, // '1'    _a_
            0xB6, // '2'  f|   |b
            0x9E, // '3'   |_g_|
            0xCA, // '4'  e|   |c
            0xDC, // '5'   |_d_|.dp
            0xFC, // '6'
            0x0E, // '7'
            0xFE, // '8'
            0xDE, // '9'
            0x00, // ':'
            0x00, // ';'
            0x00, // '<'
            0x00, // '='
            0x00, // '>'
            0x00, // '?'
            0x00, // '@'
            0xEE, // 'A'
            0xF8, // 'B'
            0x74, // 'C'
            0xBA, // 'D'
            0xF4, // 'E'
            0xE4, // 'F'
            0x7C, // 'G'
            0xEA, // 'H'
            0x0A, // 'I'
            0x3A, // 'J'
            0x00, // 'K'
            0x70, // 'L'
            0x00, // 'M'
            0x6E, // 'N'
            0x7E, // 'O'
            0xE6, // 'P'
            0xCE, // 'Q'
            0x64, // 'R'
            0xDC, // 'S'
            0xF0, // 'T'
            0x7A, // 'U'
            0x00, // 'V'
            0x00, // 'W'
            0x00, // 'X'
            0xDA, // 'Y'
            0x00, // 'Z'
            0x00, // '['
            0x00, // '/'
            0x00, // ']'
            0x00, // '^'
            0x10  // '_'
        };
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SevenSegClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the 7Seg Click board is plugged on the MBN board</param>
        /// <param name="initialBrightness">Initial brightness in the range 0.0 (no display) to 1.0 (full brightness)</param>
        /// <exception cref="System.InvalidOperationException">Thrown if some pins are already in use by another board on the same socket</exception>
        public SevenSegClick(Hardware.Socket socket, Double initialBrightness = 1.0)
        {
            Hardware.CheckPins(socket, socket.Miso, socket.Mosi, socket.Cs, socket.Sck, socket.Pwm);

            _spiConfig = new SPI.Configuration(socket.Cs, false, 15, 10, false, true, 20000, socket.SpiModule);
            if (Hardware.SPIBus == null) { Hardware.SPIBus = new SPI(_spiConfig); }

            // Sets initial brightness
            _pwm = new PWM(socket.PwmChannel, 10000, initialBrightness, false);
            _pwm.Start();
            _pwmLevel = initialBrightness;
        }

        /// <summary>
        /// Gets byte value representing the character in the char table
        /// </summary>
        /// <param name="character">The character.</param>
        /// <returns>A byte to send to the board</returns>
        /// <example> This sample shows how to call the GetChar() method.
        /// <code language="C#">
        /// using System.Threading;
        /// using MBN;
        /// using MBN.Modules;
        ///
        /// namespace Examples
        /// {
        ///     public class Program
        ///     {
        ///         private static SevenSegClick _seven;
        /// 
        ///         public static void Main()
        ///         {
        ///             _seven = new SevenSegClick(Hardware.SocketOne, 0.05);
        /// 
        ///             // Displays all alpha chars on the right display, using the GetChar() method
        ///             for (char i = 'A'; i &lt;= 'Z'; i++)
        ///             {
        ///                 _seven.SendBytes(new byte[] { _seven.GetChar(i), 0x00 });
        ///                 Thread.Sleep(200);
        ///             }
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public byte GetChar(char character)
        {
            return ((character >= '-') && (character <= '_')) ? CharTable[character - '-'] : (byte)0x00;
        }

        /// <summary>
        /// Gets byte value representing the digit in the char table
        /// </summary>
        /// <param name="digit">The digit.</param>
        /// <returns>A byte to send to the board</returns>
        /// <example> This sample shows how to call the GetDigit() method.
        /// <code language="C#">
        /// using System.Threading;
        /// using MBN;
        /// using MBN.Modules;
        ///
        /// namespace Examples
        /// {
        ///     public class Program
        ///     {
        ///         private static SevenSegClick _seven;
        /// 
        ///         public static void Main()
        ///         {
        ///             _seven = new SevenSegClick(Hardware.SocketOne, 0.05);
        /// 
        ///             // Displays from 0 to 9.9
        ///             // Trick : no float here, only bytes, the dot is added as soon as i > 9
        ///             for (byte i = 0; i &lt; 100; i++)
        ///             {
        ///                 _seven.SendBytes(i &lt; 10
        ///                     ? new byte[] {_seven.GetDigit(i), 0x00}
        ///                     : new [] {_seven.GetDigit((byte) (i%10)), (byte) (_seven.GetDigit((byte) (i/10)) + 1)});
        ///                 Thread.Sleep(75);
        ///             }
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public byte GetDigit(byte digit)
        {
            return digit <= 9 ? CharTable[digit + 3] : (Byte)0x00;
        }

        /// <summary>
        /// Gets or sets the brightness of the bargraph.
        /// </summary>
        /// <value>
        /// The brightness level, in the range 0.0 to 1.0
        /// </value>
        /// <example> This sample shows how to use the Brightness property.
        /// <code language="C#">
        /// using System.Threading;
        /// using MBN;
        /// using MBN.Modules;
        ///
        /// namespace Examples
        /// {
        ///     public class Program
        ///     {
        ///         private static SevenSegClick _seven;
        /// 
        ///         public static void Main()
        ///         {
        ///             _seven = new SevenSegClick(Hardware.SocketOne, 0.05);
        /// 
        ///             // Displays from 0 to 9.9
        ///             // Trick : no float here, only bytes, the dot is added as soon as i > 9
        ///             for (byte i = 0; i &lt; 100; i++)
        ///             {
        ///                 _seven.SendBytes(i &lt; 10
        ///                     ? new byte[] {_seven.GetDigit(i), 0x00}
        ///                     : new [] {_seven.GetDigit((byte) (i%10)), (byte) (_seven.GetDigit((byte) (i/10)) + 1)});
        ///                 Thread.Sleep(75);
        ///             }
        /// 
        ///             _seven.Brightness = 0.25;
        /// 
        ///             // Scrolls from A to Z
        ///             _seven.SendBytes(new byte[] { 0xEE, 0x00 });
        /// 
        ///             for (byte i = 66; i &lt; 91; i++)
        ///             {
        ///                 _seven.SendBytes(new [] { _seven.CharTable[i - 44], _seven.CharTable[i - 45] });
        ///                 Thread.Sleep(200);
        ///             }
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public Double Brightness
        {
            get { return _pwmLevel; }
            set
            {
                _pwmLevel = (value > 1.0) || (value < 0.0) ? 1.0 : value;
                _pwm.DutyCycle = _pwmLevel;
                _pwm.Start();
            }
        }

        /// <summary>
        /// Clears the display, without affecting brightness.
        /// </summary>
        /// <example> This sample shows how to call the Clear() method.
        /// <code language="C#">
        /// using System.Threading;
        /// using MBN;
        /// using MBN.Modules;
        ///
        /// namespace Examples
        /// {
        ///     public class Program
        ///     {
        ///         private static SevenSegClick _seven;
        /// 
        ///         public static void Main()
        ///         {
        ///             _seven = new SevenSegClick(Hardware.SocketOne, 0.05);
        /// 
        ///             // Displays from 0 to 9.9
        ///             // Trick : no float here, only bytes, the dot is added as soon as i > 9
        ///             for (byte i = 0; i &lt; 100; i++)
        ///             {
        ///                 _seven.SendBytes(i &lt; 10
        ///                     ? new byte[] {_seven.GetDigit(i), 0x00}
        ///                     : new [] {_seven.GetDigit((byte) (i%10)), (byte) (_seven.GetDigit((byte) (i/10)) + 1)});
        ///                 Thread.Sleep(75);
        ///             }
        /// 
        ///             _seven.Clear();
        /// 
        ///             // Scrolls from A to Z
        ///             _seven.SendBytes(new byte[] { 0xEE, 0x00 });
        /// 
        ///             for (byte i = 66; i &lt; 91; i++)
        ///             {
        ///                 _seven.SendBytes(new [] { _seven.CharTable[i - 44], _seven.CharTable[i - 45] });
        ///                 Thread.Sleep(200);
        ///             }
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public void Clear()
        {
            Hardware.SPIBus.Write(_spiConfig, new byte[] { 0x00, 0x00 });
        }

        /// <summary>
        /// Sends a 2 bytes array to the 7Seg.
        /// <para>First byte is left display, second byte is right display</para>
        /// </summary>
        /// <param name="tab">The array of bytes.</param>
        /// <example> This sample shows how to call the SendBytes() method.
        /// <code language="C#">
        /// using System.Threading;
        /// using MBN;
        /// using MBN.Modules;
        ///
        /// namespace Examples
        /// {
        ///     public class Program
        ///     {
        ///         private static SevenSegClick _seven;
        /// 
        ///         private static readonly byte[] Spin1 = 
        ///         {
        ///             0x00, 0x04, 
        ///             0x00, 0x40,
        ///             0x00, 0x20,
        ///             0x00, 0x10,
        ///             0x10, 0x00,
        ///             0x08, 0x00,
        ///             0x02, 0x00,
        ///             0x04, 0x00
        ///         };
        /// 
        ///         public static void Main()
        ///         {
        ///             _seven = new SevenSegClick(Hardware.SocketOne, 0.05);
        /// 
        ///             for (int j = 0; j &lt; 10; j++)
        ///             {
        ///                 for (byte i = 0; i &lt; 16; i += 2)
        ///                 {
        ///                     _seven.SendBytes(new [] { Spin1[i], Spin1[i + 1] });
        ///                     Thread.Sleep(50);
        ///                 }
        ///             }
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public void SendBytes(byte[] tab)
        {
            Hardware.SPIBus.Write(_spiConfig, tab);
        }

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">
        /// </exception>
        /// <remarks>
        /// If the module has no power modes, then GET should always return PowerModes.ON while SET should throw a NotImplementedException.
        /// </remarks>
        public PowerModes PowerMode
        {
            get { return PowerModes.On; }
            set
            {
                throw new NotImplementedException("PowerMode");
            }
        }

        /// <summary>
        /// Gets the driver version.
        /// </summary>
        /// <value>
        /// The driver version.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///      Debug.Print ("Current driver version : "+_seven.DriverVersion);
        /// </code>
        /// </example>
        public Version DriverVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        /// <summary>
        /// Resets the module
        /// </summary>
        /// <param name="resetMode">The reset mode :
        /// <para>SOFT reset : generally by sending a software command to the chip</para><para>HARD reset : generally by activating a special chip's pin</para></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException">This module has no Reset feature.</exception>
        public bool Reset(ResetModes resetMode)
        {
            throw new NotImplementedException("Reset");
        }
    }
}

