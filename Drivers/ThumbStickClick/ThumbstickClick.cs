/*
 * Thumbstick Click MBN Driver
 * 
 * Version 1.0 :
 *  - Initial release coded by Stephen Cardinale
 *
 * References needed :
 *   - Microsoft.SPOT.Hardware
 *   - Microsoft.SPOT.Native
 *   - MikroBus.Net
 *   - mscorlib
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
using MBN.Exceptions;
using MBN.Extensions;
using Microsoft.SPOT.Hardware;

namespace MBN.Modules
{
    /// <summary>
    /// MicroBusNet Driver for the Thumbstick Click board by MikroElektronika.
    /// <para><b>The Thumbstick Click is a SPI Device</b></para>
    /// <para><b>Pins used :</b> Miso, Mosi, Cs, Sck, Int</para>
    /// <para><b>References required:</b> MikroBus.Net, Microsoft.SPOT.Native, Microsoft.Spot.Hardware, mscorlib</para>
    /// </summary>
    /// <example>Example usage:
    /// <code language = "C#">
    /// using MBN;
    /// using MBN.Modules;
    /// using Microsoft.SPOT;
    /// using System.Threading;
    ///
    /// namespace ThumbstickClickTestApp
    /// {
    ///     public class Program
    ///     {
    ///
    ///         private static ThumbstickClick _thumb;
    ///
    ///         public static void Main()
    ///         {
    ///             _thumb = new ThumbstickClick(Hardware.SocketFour);
    ///
    ///             _thumb.ThumbstickOrientation = ThumbstickClick.Orientation.Rotate90Degrees;
    ///
    ///             _thumb.ThumbstickPressed += _thumb_ThumbstickPressed;
    ///             _thumb.ThumbstickReleased += _thumb_ThumbstickReleased;
    ///
    ///             _thumb.Calibrate();
    ///
    ///             while (true)
    ///             {
    ///                 var position = _thumb.GetPosition();
    ///                 Debug.Print("Current X: " + position.X.ToString("f2"));
    ///                 Debug.Print("Current Y: " + position.Y.ToString("f2"));
    ///
    ///                 Thread.Sleep(1000);
    ///             }
    ///
    ///             //Thread.Sleep(Timeout.Infinite);
    ///         }
    ///
    ///         static void _thumb_ThumbstickReleased(ThumbstickClick sender, ThumbstickClick.ButtonState state)
    ///         {
    ///             Debug.Print("Thumbstick Released");
    ///         }
    ///
    ///         static void _thumb_ThumbstickPressed(ThumbstickClick sender, ThumbstickClick.ButtonState state)
    ///         {
    ///             Debug.Print("Thumbstick Pressed");
    ///         }
    ///     }
    /// }
    /// </code>
    /// <code language = "VB">
    /// Option Explicit On
    /// Option Strict On
    ///
    /// Imports Microsoft.SPOT
    /// Imports MBN
    /// Imports MBN.Modules
    /// Imports System.Threading
    ///
    /// Namespace Examples
    ///
    ///     Public Module Module1
    ///
    ///         Dim WithEvents _thumb As ThumbstickClick
    ///
    ///         Sub Main()
    ///
    ///             _thumb = New ThumbstickClick(Hardware.SocketFour)
    ///
    ///             While True
    ///                 Dim position As ThumbstickClick.Position = _thumb.GetPosition()
    ///                 Debug.Print("Current X: " + position.X.ToString("f2"))
    ///                 Debug.Print("Current Y: " + position.Y.ToString("f2"))
    ///                 Thread.Sleep(1000)
    ///             End While
    ///
    ///         End Sub
    ///
    ///         Private Sub _thumb_ThumbstickPressed(sender As ThumbstickClick, state As ThumbstickClick.ButtonState) Handles _thumb.ThumbstickPressed
    ///             Debug.Print("Thumbstick Pressed")
    ///         End Sub
    ///
    ///         Private Sub _thumb_ThumbstickReleased(sender As ThumbstickClick, state As ThumbstickClick.ButtonState) Handles _thumb.ThumbstickReleased
    ///             Debug.Print("Thumbstick Released")
    ///         End Sub
    ///     End Module
    ///
    /// End Namespace
    /// </code>
    /// </example>
    public class ThumbstickClick : IDriver
    {
        #region Fields
        
        private readonly SPI.Configuration _spiConfig;
        private readonly InterruptPort _button;

        private float _offsetX;
        private float _offsetY; 

        #endregion

        #region ENUMS
        
        /// <summary>
        /// The orientation of the Thumbstick Click when inserted into a MikroBus.Net Mainboard.
        /// </summary>
        public enum Orientation
        {
            /// <summary>
            /// Default Orientation with the Ground Pins on the ThumbstickClick are closest to the bottom side of the module (closest to you as you are looking at the ThumbstickClick).
            /// </summary>
            RotateZeroDegrees,
            /// <summary>
            /// Orientation is rotated 90° with the Ground Pins on the ThumbstickClick are closest to the right side of the module (towards your right when as you are looking at the ThumbstickClick).
            /// </summary>
            Rotate90Degrees,
            /// <summary>
            /// Orientation is rotated 180° with the Ground Pins on the ThumbstickClick are closest to the top side of the module (furthest from you as you are looking at the ThumbstickClick).
            /// </summary>
            Rotate180Degrees,
            /// <summary>
            /// Orientation is rotated 270° with the Ground Pins on the ThumbstickClick are closest to the left side of the module (towards your left when as you are looking at the ThumbstickClick).
            /// </summary>
            Rotate270Degrees
        }

        #endregion

        #region CTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="ThumbstickClick"/> class.
        /// </summary>
        /// <param name="socket">The socket in which the Thumbstick Click board is inserted into.</param>
        /// <exception cref="PinInUseException">A <see cref="PinInUseException"/> will be thrown if the  Miso, Mosi, Sck, Cs, Int pins are already in use in a stacked module arrangement.</exception>
        public ThumbstickClick(Hardware.Socket socket)
        {
            try
            {
                // Checks if needed SPI pins are available
                Hardware.CheckPins(socket, socket.Miso, socket.Mosi, socket.Cs, socket.Sck, socket.Int);

                // Initialize SPI
                _spiConfig = new SPI.Configuration(socket.Cs, false, 50, 50, true, true, 1000, socket.SpiModule);
                if (Hardware.SPIBus == null) { Hardware.SPIBus = new SPI(_spiConfig); }

                _button = new InterruptPort(socket.Int, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);
                _button.OnInterrupt += _button_OnInterrupt;

                Calibrate();

                ThumbstickOrientation = Orientation.RotateZeroDegrees;

            }
            // Catch only the PinInUse exception, so that program will halt on other exceptions
            // Send it directly to caller
            catch (PinInUseException ex) { throw new PinInUseException(ex.Message); }
        }

        #endregion

        #region Private Methods
        
        private static float Constrain(float value, float min, float max)
        {
            return value >= max ? max : value <= min ? min : value;
        }

        private float ReadADC(ushort channel, bool singleEnded = true)
        {
            float adcTotal = 0;

            var readArray = new byte[2];

            ushort commandBits = 0xC0;

            commandBits |= (ushort)(channel << 3);
            commandBits = (ushort)(commandBits << 3);

            var lowByte = (byte)(commandBits & 0xFF);
            var highByte = (byte)(commandBits >> 8);

            byte[] writeArray = { highByte, lowByte };

            for (var i = 0; i < 10; i++)
            {
                Hardware.SPIBus.WriteRead(_spiConfig, writeArray, readArray, 1);
                adcTotal += (((readArray[0] & 0x0F) << 8) + readArray[1]);
            }
            return (adcTotal / 10) / 4095;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the driver version.
        /// </summary>
        /// <value>
        /// The driver version see <see cref="Version"/>.
        /// </value>
        /// <example>Example usage to get the Driver Version in formation:
        /// <code language="C#">
        /// Debug.Print("Driver Version Info : " + _uv.DriverVersion);
        /// </code>
        /// <code language="VB">
        /// Debug.Print("Driver Version Info : " <![CDATA[&]]> _uv.DriverVersion)
        /// </code>
        /// </example>
        public Version DriverVersion
        {
            get { return Assembly.GetAssembly(GetType()).GetName().Version; }
        }
        
        /// <summary>
        /// Gets whether or not the JoystickClick is pressed.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("Is ThumbstickClick Pressed? " + _thumb.IsPressed.ToString());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Is ThumbstickClick Pressed? " <![CDATA[&]]> _thumb.IsPressed.ToString())
        /// </code>
        /// </example>
        public bool IsPressed
        {
            get
            {
                return !_button.Read();
            }
        }

        /// <summary>
        /// The orientation of the Thumbstick Click when inserted into a MikroBusNet Mainboard.
        /// <para>In the default <see cref="Orientation.RotateZeroDegrees"/>, the Ground Pins on the Thumbstick are closest to you when looking at the module </para> 
        /// <para>Use the ThumbstickOrientation property to account for your arrangement.</para>
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _thumb.ThumbstickOrientation = ThumbstickClick.Orientation.Rotate90Degrees;
        /// </code>
        /// <code language = "VB">
        /// _thumb.ThumbstickOrientation = ThumbstickClick.Orientation.Rotate90Degrees
        /// </code>
        /// </example>
        public Orientation ThumbstickOrientation { get; set; }

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">This module has no power modes features.</exception>
        public PowerModes PowerMode
        {
            get { return Enums.PowerModes.On; }
            set { throw new NotImplementedException("PowerMode"); }
        }

        #endregion
        
        #region Public Methods

        /// <summary>
        /// Calibrates the joystick such that the current (x, y) position is interpreted as (0, 0).
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _thumb.Calibrate();
        /// </code>
        /// <code language = "VB">
        /// _thumb.Calibrate()
        /// </code>
        /// </example>
        public void Calibrate()
        {
            _offsetX = ReadADC(1) * 2 - 1;
            _offsetY = (1 - ReadADC(0)) * 2 - 1;
        }

        /// <summary>
        /// Gets position of the joystick.
        /// </summary>
        /// <returns>The current <see cref="Position"/> of the JoystickClick.</returns>
        /// <example>Example usage:
        /// <code language = "C#">
        /// var position = _thumb.GetPosition();
        /// Debug.Print("Current X: " + position.X.ToString("f2"));
        /// Debug.Print("Current Y: " + position.Y.ToString("f2"));
        /// </code>
        /// <code language = "VB">
        /// dim position = _thumb.GetPosition()
        /// Debug.Print("Current X: " <![CDATA[&]]> position.X.ToString("f2"))
        /// Debug.Print("Current Y: " <![CDATA[&]]> position.Y.ToString("f2"))
        /// </code>
        /// </example>
        public Position GetPosition()
        {
            switch (ThumbstickOrientation)
            {
                case Orientation.Rotate90Degrees:
                {
                    return new Position()
                    {
                        X = Constrain(-(((1 - ReadADC(0)) * 2 - 1) - _offsetY), -1.0F, 1.0F),
                        Y = Constrain(((ReadADC(1) * 2 - 1) - _offsetX), -1.0F, 1.0F)
                    };
                }
                case Orientation.Rotate270Degrees:
                {
                    return new Position()
                    {
                        X = Constrain((((1 - ReadADC(0)) * 2 - 1) - _offsetY), -1.0F, 1.0F),
                        Y = Constrain(-(ReadADC(1) * 2 - 1) - _offsetX, -1.0F, 1.0F)
                    };
                }
                case Orientation.RotateZeroDegrees:
                {
                    return new Position()
                    {
                        X = Constrain(((ReadADC(1) * 2 - 1) - _offsetX), -1.0F, 1.0F),
                        Y = Constrain((((1 - ReadADC(0)) * 2 - 1) - _offsetY), -1.0F, 1.0F)
                    };
                }
                case Orientation.Rotate180Degrees:
                {
                    return new Position()
                    {
                        X = Constrain(-((ReadADC(1) * 2 - 1) - _offsetX), -1.0F, 1.0F),
                        Y = Constrain(-(((1 - ReadADC(0)) * 2 - 1) - _offsetY), -1.0F, 1.0F) 
                    };
                }
                default:
                {
                    return new Position();
                }
            }
        }

        /// <summary>
        /// Resets the module
        /// </summary>
        /// <param name="resetMode">The reset mode :
        /// <para>SOFT reset : generally by sending a software command to the chip</para><para>HARD reset : generally by activating a special chip's pin</para></param>
        /// <returns>True if Reset has been acknowledged, false otherwise.</returns>
        /// <exception cref="System.NotImplementedException">This module has no Reset feature.</exception>
        /// <examples>None as this module does not implement a reset method. </examples>
        public bool Reset(Enums.ResetModes resetMode)
        {
            throw new NotImplementedException("Reset");
        }

        #endregion

        #region Position Structure
        
        /// <summary>
        /// Structure that contains the X and Y position of the joystick from -1.0 to 1.0 (0.0 means centered).
        /// </summary>
        public struct Position
        {
            /// <summary>
            /// The X coordinate of the Thumbstick from -1.0 to 1.0 (0.0 means centered).
            /// </summary>
            public float X { get; set; }

            /// <summary>
            /// The Y coordinate of the Thumbstick from -1.0 to 1.0 (0.0 means centered).
            /// </summary>
            public float Y { get; set; }
        }

        #endregion
        
        #region Events

        /// <summary>
        /// Represents the delegate that is used to handle the <see cref="ThumbstickClick.ThumbstickReleased"/> and <see cref="ThumbstickClick.ThumbstickPressed"/> events.
        /// </summary>
        /// <param name="sender">The <see cref="ThumbstickClick"/> object that raised the event.</param>
        /// <param name="state">The state of the ThummbstickClick.</param>
        public delegate void JoystickEventHandler(ThumbstickClick sender, ButtonState state);

        /// <summary>
        /// Raised when the ThummbstickClick is released.
        /// </summary>
        public event JoystickEventHandler ThumbstickReleased;

        /// <summary>
        /// Raised when the ThummbstickClick is pressed.
        /// </summary>
        public event JoystickEventHandler ThumbstickPressed;

        private JoystickEventHandler _onJoystickEvent;

        private void OnJoystickEvent(ThumbstickClick sender, ButtonState buttonState)
        {
            if (_onJoystickEvent == null) _onJoystickEvent = OnJoystickEvent;

            switch (buttonState)
            {
                case ButtonState.Pressed:
                    if (ThumbstickPressed != null)
                    {
                        ThumbstickPressed(sender, ButtonState.Pressed);
                    }
                    break;
                case ButtonState.Released:
                    if (ThumbstickReleased != null)
                    {
                        ThumbstickReleased(sender, ButtonState.Released);
                    }
                    break;
            }
        }

        void _button_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            OnJoystickEvent(this, data2 == 1 ? ButtonState.Released : ButtonState.Pressed);
        }

        /// <summary>
        /// Represents the button state of the <see cref="ThumbstickClick"/> object.
        /// </summary>
        public enum ButtonState
        {
            /// <summary>
            /// The state of Thumbstick is Pressed.
            /// </summary>
            Pressed = 0,
            /// <summary>
            /// The state of Thumbstick is Released.
            /// </summary>
            Released = 1
        } 

        #endregion
    }
}

