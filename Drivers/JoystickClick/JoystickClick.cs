/*
 * JoystickClick driver skeleton generated on 28/10/2014 20:27:16
 * 
 * Initial revision coded by Christophe Gerbier
 * 
 * References needed :  (change according to your driver's needs)
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
using System.Reflection;
using System.Threading;
using MBN.Enums;
using MBN.Exceptions;
using MBN.Extensions;
using Microsoft.SPOT.Hardware;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the JoystickClick driver
    /// <para>The module is an I2C device. <b>Pins used :</b> Scl, Sda, Int, Rst, Cs</para>
    /// <example>
    /// <code language="C#">
    ///	using System;
    ///	using MBN.Enums;
    ///	using MBN.Modules;
    ///	using MBN;
    ///	using System.Threading;
    ///	using Microsoft.SPOT;
    ///	
    ///	namespace TestJoystick
    ///	{
    ///	    public class Program
    ///	    {
    ///			private static JoystickClick _joy;
    ///	
    ///	        public static void Main()
    ///	        {
    ///	            _joy = new JoystickClick(Hardware.SocketThree) { DeadZone = new SByte[] { 100, -100, 100, -100 }, TimeBase = 7 };
    ///	
    ///	            _joy.InterruptLine.OnInterrupt += InterruptLineOnInterruptLine;
    ///	            _joy.Button.OnInterrupt += Button_OnInterrupt;
    ///	
    ///	            Thread.Sleep(Timeout.Infinite);
    ///	        }
    ///	
    ///	        private static void Button_OnInterrupt(uint data1, uint data2, DateTime time)
    ///	        {
    ///	            Hardware.Led1.Write(data2 != 0);
    ///	        }
    ///	
    ///	        private static void InterruptLineOnInterruptLine(uint data1, uint data2, DateTime time)
    ///	        {
    ///	            var pos = _joy.GetKnobPosition();
    ///	        }
    ///	    }
    ///	}
    /// </code>
    /// </example>
    /// </summary>
    public partial class JoystickClick : IDriver
    {
        /// <summary>
        /// Structure containing actual knob position coordinates
        /// </summary>
        public struct KnobPosition
        {
            /// <summary>
            /// The X-axis value
            /// </summary>
            public Int32 X;
            /// <summary>
            /// The Y-axis value
            /// </summary>
            public Int32 Y;

            /// <summary>
            /// Initializes a new instance of the <see cref="KnobPosition"/> struct.
            /// </summary>
            /// <param name="posX">The x-axis position.</param>
            /// <param name="posY">The y-axis position.</param>
            public KnobPosition(Int32 posX, Int32 posY)
            {
                X = posX;
                Y = posY;
            }
        }

        /// <summary>
        /// The interrupt line used to signal that the knob position has passed the user-defined dead-zone.
        /// </summary>
        public InterruptPort InterruptLine;
        /// <summary>
        /// Used to signal that the joystick button has been pressed/released.
        /// </summary>
        public InterruptPort Button;

        private readonly OutputPort _reset;
        private readonly I2CDevice.Configuration _config;       // I²C configuration

        #region Private methods
        private Byte ReadRegister(Byte register)
        {
            var result = new Byte[1];

            Hardware.I2CBus.Execute(_config, new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(new[] { register }), I2CDevice.CreateReadTransaction(result) }, 1000);

            return result[0];
        }

        private SByte ReadSignedRegister(Byte register)
        {
            var result = new Byte[1];

            Hardware.I2CBus.Execute(_config, new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(new[] { register }), I2CDevice.CreateReadTransaction(result) }, 1000);

            return (SByte)result[0];
        }

        private void WriteRegister(Byte register, Byte data)
        {
            Hardware.I2CBus.Execute(_config, new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(new[] { register, data }) }, 1000);
        }

        private Boolean DataReady()
        {
            return (ReadRegister(Registers.CONTROL1) & 0x01) == 0x01;
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="JoystickClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the JoystickClick module is plugged on MikroBus.Net board</param>
        /// <param name="address">The address of the module.</param>
        /// <param name="clockRateKHz">The clock rate of the I²C device. <seealso cref="ClockRatesI2C"/></param>
        public JoystickClick(Hardware.Socket socket, Byte address = 0x40, ClockRatesI2C clockRateKHz = ClockRatesI2C.Clock100KHz)
        {
            try
            {
                // Checks if needed I²C pins are available.
                Hardware.CheckPinsI2C(socket, socket.Int, socket.Rst, socket.Cs);

                // Create the driver's I²C configuration
                _config = new I2CDevice.Configuration(address, (Int32)clockRateKHz);

                _reset = new OutputPort(socket.Rst, true);
                Reset(ResetModes.Hard);

                Sensitivity = 0x3F; // Max sensitivity
                Scaling = 0x09;     // 100% scaling

                Button = new InterruptPort(socket.Cs, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);
                Button.EnableInterrupt();

                PowerMode = PowerModes.On;      // Interrupt mode
                InterruptLine = new InterruptPort(socket.Int, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeLow);
                TimeBase = 3;
                ReadRegister(0x11);     // Don't care about the first data available
                InterruptLine.EnableInterrupt();

            }
            // Catch only the PinInUse exception, so that program will halt on other exceptions
            // Send it directly to the caller
            catch (PinInUseException) { throw new PinInUseException(); }
        }

        /// <summary>
        /// Gets the knob position.
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///             var pos = _joy.GetKnobPosition();
        /// </code>
        /// </example>
        /// <returns>A structure containing the actual coordinates of the knob.</returns>
        public KnobPosition GetKnobPosition()
        {
            if (PowerMode == PowerModes.Low) { while (!DataReady()) { Thread.Sleep(100); } }
            return new KnobPosition(ReadSignedRegister(Registers.X), ReadSignedRegister(Registers.Y));
        }

        /// <summary>
        /// Gets or sets the dead zone. In interrupt mode, no interrupt will be generated while the knob is inside this zone.
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///             _joy.DeadZone = new SByte[] { 100, -100, 100, -100 };
        /// </code>
        /// </example>
        /// <value>
        /// An array of signed-bytes containing the limits of the dead-zone. Xp stands for "X positive" and Xn for "X negative".
        /// </value>
        /// <exception cref="System.ArgumentException">Thrown if array size if not 4 SBytes.</exception>
        public SByte[] DeadZone
        {
            get
            {
                return new[]
                {
                    ReadSignedRegister(Registers.Xp), 
                    ReadSignedRegister(Registers.Xn), 
                    ReadSignedRegister(Registers.Yp),
                    ReadSignedRegister(Registers.Yn)
                };
            }
            set
            {
                if (value.Length != 4) { throw new ArgumentException(); }
                WriteRegister(Registers.Xp, (Byte)value[0]);
                WriteRegister(Registers.Xn, (Byte)value[1]);
                WriteRegister(Registers.Yp, (Byte)value[2]);
                WriteRegister(Registers.Yn, (Byte)value[3]);
            }
        }

        /// <summary>
        /// Gets or sets the time base used internally by the chip to poll the position. See datasheet for exact meaning of the value.
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///             _joy.TimeBase = 3;  // 100 ms internal polling
        /// </code>
        /// </example>
        /// <value>
        /// The time base.
        /// </value>
        public Byte TimeBase
        {
            get { return (Byte)((ReadRegister(Registers.CONTROL1) & 0x70) >> 4); }
            set
            {
                WriteRegister(Registers.CONTROL1, Bits.Set(ReadRegister(Registers.CONTROL1), "01110000", value > 7 ? (Byte)7 : value));
            }
        }

        /// <summary>
        /// Gets or sets the scaling used to fill the SByte space with actual knob position. Please refer to the datasheet for complete understanding of this value.
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///             _joy.Scaling = 0x09;        // Scale : 100%
        /// </code>
        /// </example>
        /// <value>
        /// The scaling factor.
        /// </value>
        public Byte Scaling
        {
            get { return ReadRegister(Registers.T_CTRL); }
            set
            {
                WriteRegister(Registers.T_CTRL, value > 79 ? (Byte)79 : value);
            }
        }

        /// <summary>
        /// Gets or sets the sensitivity of the chip to the magnets.
        /// <para>With MikroE Click board, it should be set to the max value 0x3F.</para>
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///             _joy.Sensitivity = 0x3F;
        /// </code>
        /// </example>
        /// <value>
        /// The sensitivity factor.
        /// </value>
        public Byte Sensitivity
        {
            get { return ReadRegister(Registers.AGC); }
            set
            {
                WriteRegister(Registers.AGC, value > 0x3F ? (Byte)0x3F : value);
            }
        }


        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <example> This sample shows how to use the PowerMode property.
        /// <code language="C#">
        ///             _JoystickClick.PowerMode = PowerModes.Off;
        /// </code>
        /// </example>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">Thrown if the property is set to PowerModes.Off, as the module doesn't support this mode.</exception>
        public PowerModes PowerMode
        {
            get { return (ReadRegister(Registers.CONTROL1) & 0x80) == 0x80 ? PowerModes.Low : PowerModes.On; }
            set
            {
                if (value == PowerModes.Off) { throw new NotImplementedException(); }
                WriteRegister(Registers.CONTROL1, Bits.Set(ReadRegister(Registers.CONTROL1), value == PowerModes.On ? "0XXX010X" : "1XXX100X"));
            }
        }

        /// <summary>
        /// Gets the driver version.
        /// </summary>
        /// <example> This sample shows how to use the DriverVersion property.
        /// <code language="C#">
        ///             Debug.Print ("Current driver version : "+_JoystickClick.DriverVersion);
        /// </code>
        /// </example>
        /// <value>
        /// The driver version.
        /// </value>
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
        public bool Reset(ResetModes resetMode)
        {
            if (resetMode == ResetModes.Hard)
            {
                _reset.Write(false);
                Thread.Sleep(10);
                _reset.Write(true);
            }
            else
            {
                WriteRegister(Registers.CONTROL1, 0x02);
            }
            Thread.Sleep(1000);
            do { Thread.Sleep(100); } while ((ReadRegister(Registers.CONTROL1) & 0xF0) != 0xF0);

            WriteRegister(Registers.CONTROL2, 0x84);

            return true;
        }
    }
}


