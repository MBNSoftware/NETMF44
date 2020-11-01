/*
 * MikroBus.Net board RTC2 Click driver
 *
 * Version 2.1
 * - Fixed a bug in the WriteByte() method
 *
 * Version 2.0
 * - Implemented MBN Interface and Namespace requirements.
 *
 * Version 1.0 
 * - Initial version coded by Stephen Cardinale
 *
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
using MBN.Exceptions;
using MBN.Extensions;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using System.Reflection;

namespace MBN.Modules
{
// ReSharper disable once InconsistentNaming
    /// <summary>
    /// This is the MBN driver class for the <see cref="RTC2Click"/> board by MikroE.
    /// <para><b>This module is an I2C Device.</b></para>
    /// <para><b>Pins used :</b> Scl, Sda (I²C bus)</para>
    /// <para><b>References required:</b> MikroBus.Net, Microsoft.SPOT.Native, Microsoft.Spot.Hardware, mscorlib</para>
    /// </summary>
    /// <example>Example usage:
    /// <code language = "C#">
    /// using System;
    /// using System.Threading;
    /// using MBN;
    /// using MBN.Enums;
    /// using MBN.Modules;
    /// using Microsoft.SPOT;
    ///
    /// namespace Examples
    /// {
    ///     class Program2
    ///     {
    ///         private static RTC2Click _clock;
    ///
    ///         public static void Main2()
    ///         {
    ///             _clock = new RTC2Click(Hardware.SocketOne, ClockRatesI2C.Clock400KHz, 100);
    ///
    ///             _clock.SetClock(new DateTime(2014, 08, 24, 11, 59, 45), false);
    ///
    ///             while (true)
    ///             {
    ///                 var dt = _clock.GetDateTime();
    ///                 Debug.Print("Current RTC DateTime - " + dt + "\n");
    ///                 Thread.Sleep(1000);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// <code language = "VB">
    /// Option Explicit On
    /// Option Strict On
    ///
    /// Imports System
    /// Imports MBN.Enums
    /// Imports MBN
    /// Imports Microsoft.SPOT
    /// Imports MBN.Modules
    /// Imports System.Threading
    ///
    /// Namespace Examples
    ///     Public Module Module1
    ///
    ///         Dim WithEvents clock As RTC2Click
    ///
    ///         Sub Main()
    ///
    ///             clock = New RTC2Click(Hardware.SocketOne, ClockRatesI2C.Clock400KHz, 100)
    ///             clock.SetClock(New DateTime(2014, 8, 24, 11, 59, 45), False)
    ///
    ///             While (True)
    ///                 Dim dt = _clock.GetDateTime()
    ///                 Debug.Print("Current RTC DateTime - " <![CDATA[&]]> dt)
    ///                 Thread.Sleep(1000)
    ///             End While
    ///         End Sub
    ///     End Module
    /// End Namespace
    /// </code>
    /// </example>
    public partial class RTC2Click : IDriver
    {
        #region ENUMS 

        /// <summary>
        ///     Defines the logic level on the SQW pin when the frequency is disabled
        /// </summary>
        [Flags]
        public enum SqwOutputControl
        {
            /// <summary>
            ///     Disabled
            /// </summary>
            Off,

            /// <summary>
            ///     Enabled
            /// </summary>
            On
        };

        /// <summary>
        ///     Defines the frequency of the signal on the SQW interrupt pin on the clock when enabled
        /// </summary>
        [Flags]
        public enum SqwFrequency
        {
            /// <summary>
            ///     1 Hz output.
            /// </summary>
            Sqw1Hz,

            /// <summary>
            ///     4 Hz output
            /// </summary>
            Sqw_4KHz,

            /// <summary>
            ///     8 Hz output
            /// </summary>
            Sqw_8KHz,

            /// <summary>
            ///     32 Hz output
            /// </summary>
            Sqw32KHz,

            /// <summary>
            ///     No output
            /// </summary>
            SqwOff
        };

        #endregion

        #region Fields

        private static int _i2CTimeout;
        private static I2CDevice.Configuration _i2CConfiguration;

        #endregion

        #region CTOR

        /// <summary>
        /// Initializes a new instance of the RTC2click class.
        /// </summary>
        /// <param name="socket">The socket that the RTC2 is inserted into.</param>
        /// <param name="clockKHz">The speed of the I2C Clock. See <see cref="ClockRatesI2C"/>.</param>
        /// <param name="timeout">I2C Transaction timeout.</param>
        public RTC2Click(Hardware.Socket socket, ClockRatesI2C clockKHz, int timeout)
        {
            try
            {
                // Checks if needed I²C pins are available
                Hardware.CheckPinsI2C(socket);

                _i2CTimeout = timeout;
                _i2CConfiguration = new I2CDevice.Configuration(Registers.I2C_ADDRESS, (int) clockKHz);

                // Check if it's the first time an I²C device is created
                if (Hardware.I2CBus == null)
                {
                    Hardware.I2CBus = new I2CDevice(_i2CConfiguration);
                }
                SetSquareWave(SqwFrequency.SqwOff); // Turn off SQW output and set it to initial state off.
            }
            catch (PinInUseException ex)
            {
                throw new PinInUseException(ex.Message);
            }
        }

        #endregion

        #region Private Methods
 
        private static bool WriteByte(byte register, byte value)
        {
            var xActions = new I2CDevice.I2CTransaction[1];
            xActions[0] = I2CDevice.CreateWriteTransaction(new[] { register, value });
            return Hardware.I2CBus.Execute(_i2CConfiguration, xActions, _i2CTimeout) != 0;
        }
       
        private static bool WriteRegister(byte registerAddress, byte[] value)
        {
            var address = new byte[1];
            address[0] = registerAddress;
            var bytesToSend = new byte[address.Length + value.Length];
            Array.Copy(address, bytesToSend, address.Length);
            Array.Copy(value, 0, bytesToSend, address.Length, value.Length);
            var xActions = new I2CDevice.I2CTransaction[1];
            xActions[0] = I2CDevice.CreateWriteTransaction(bytesToSend);
            return Hardware.I2CBus.Execute(_i2CConfiguration, xActions, _i2CTimeout) != 0;
        }

        private static byte ReadByte(byte registerAddress)
        {
            var readBuffer = new byte[8];
            var xActions = new I2CDevice.I2CTransaction[2];
            xActions[0] = I2CDevice.CreateWriteTransaction(new[] { registerAddress });
            xActions[1] = I2CDevice.CreateReadTransaction(readBuffer);
            Hardware.I2CBus.Execute(_i2CConfiguration, xActions, _i2CTimeout);
            return readBuffer[0];
        }

        private static byte[] ReadRegister(byte registerAddress)
        {
            Hardware.I2CBus.Config = _i2CConfiguration;
            var readBuffer = new byte[8];
            var xActions = new I2CDevice.I2CTransaction[2];
            xActions[0] = I2CDevice.CreateWriteTransaction(new[] { registerAddress });
            xActions[1] = I2CDevice.CreateReadTransaction(readBuffer);
            Hardware.I2CBus.Execute(_i2CConfiguration, xActions, _i2CTimeout);
            return readBuffer;
        }

        private static int BcdToDec(int val)
        {
            return ((val / 16 * 10) + (val % 16));
        }

        private static byte DecToBcd(int val)
        {
            return (byte)((val / 10 * 16) + (val % 10));
        } 

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks to see if the Oscillator is turned on.
        /// </summary>
        /// <returns>True is the Oscillator is turned on, otherwise false.</returns>
        /// <example>Example usage:
        /// <code language = "C#">
        /// if (!_clock.IsOscillatorOn())
        /// {
        ///     //Make sure the clock is running
        ///     _clock.ClockHalt(false);
        /// }
        /// </code>
        /// <code language = "VB">
        /// If Not _clock.IsOscillatorOn() Then
        ///     'Make sure the clock is running
        ///     _clock.ClockHalt(False)
        /// End If
        /// </code>
        /// </example>
        public bool IsOscillatorOn()
        {
            return ReadByte(Registers.RTC_START_ADDRESS) >> 7 == 0;
        }

        /// <summary>
        ///  Halts or Resumes the time-keeping function on the RTC. Calling this function preserves the value of the seconds register.
        /// </summary>
        /// <param name="halt">
        /// When True, stops the RTC from incrementing time and turns off the Oscillator, when False turns on the Oscillator and resumes RTC incrementing time.
        /// </param>
        /// <example>Example usage:
        /// <code language = "C#">
        /// if (_clock.IsOscillatorOn())
        /// {
        ///     // Turn off the clock.
        ///     _clock.ClockHalt(true);
        /// }
        /// </code>
        /// <code language = "VB">
        /// If _clock.IsOscillatorOn() Then
        ///     ' Turn off the clock.
        ///     _clock.ClockHalt(True)
        /// End If
        /// </code>
        /// </example>
        public void ClockHalt(bool halt)
        {
            WriteByte(Registers.RTC_START_ADDRESS, Bits.Set((Registers.RTC_START_ADDRESS), 7, halt));
        }

        /// <summary>
        /// This method checks to see if the DateTime of the  Real Time Clock has been set since power up.
        /// </summary>
        /// <returns>True if the RTC has been set, otherwise false.</returns>
        /// <remarks>
        /// The RTC_SET_Bit is stored in NVRAM at location 0x38 and will be preserved  through a power cycle provided that VBAT is above the nominal power-fail trip point.
        /// </remarks>
        /// <example>
        /// <code language = "C#">
        ///  Debug.Print("RTC set? " + _clock.IsDateTimeSet());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("RTC set? " <![CDATA[&]]> _clock.IsDateTimeSet())
        /// </code>
        /// </example>
        public bool IsDateTimeSet()
        {
            return ReadByte(Registers.RTC_SET_BIT_ADDRESS) == 1;
        }

        /// <summary>
        /// Gets the DateTime from the RTC.
        /// </summary>
        /// <returns>The current DateTime of the RTC.</returns>
        /// <example>
        /// <code language = "C#">
        /// var dt = _clock.GetDateTime();
        /// Debug.Print("Current RTC DateTime - " + dt);
        /// </code>
        /// <code language = "VB">
        /// Dim dt  as DateTime = _clock.GetDateTime()
        /// Debug.Print("Current RTC DateTime - " <![CDATA[&]]> dt)
        /// </code>
        /// </example>
        public DateTime GetDateTime()
        {
            byte[] clockData = ReadRegister(Registers.RTC_START_ADDRESS);

            try
            {
                return new DateTime(
                BcdToDec(clockData[6]) + 2000, // year
                BcdToDec(clockData[5]), // month
                BcdToDec(clockData[4]), // day
                BcdToDec(clockData[2] & 0x3f), // hours over 24 hours
                BcdToDec(clockData[1]), // minutes
                BcdToDec(clockData[0] & 0x7f) // seconds
                );
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Sets the time on the clock using the DateTime object. Milliseconds are not used.
        /// </summary>
        /// <param name="dt">A DateTime object used to set the clock</param>
        /// <param name="preserveClockHaltState">
        /// If true, it will preserve the state of the ClockHalt (DH) when setting the DateTime. If the CH Bit was previously High it will remain high, otherwise it will reset the CH Bit to 0 (Oscillator enabled).
        /// </param>
        /// <example>
        /// <code language = "C#">
        /// _clock.SetClock(new DateTime(2014, 05, 25, 11, 59, 45), true);
        /// </code>
        /// <code language = "VB">
        /// _clock.SetClock(New DateTime(2014, 05, 25, 11, 59, 45), True)
        /// </code>
        /// </example>
        public bool SetClock(DateTime dt, bool preserveClockHaltState = false)
        {
            var oscOn = false;
            if (preserveClockHaltState)
            {
                oscOn = IsOscillatorOn();
            }

            var clockData = new[]
            {
                DecToBcd(dt.Second),
                DecToBcd(dt.Minute),
                DecToBcd(dt.Hour),
                DecToBcd((int) dt.DayOfWeek),
                DecToBcd(dt.Day),
                DecToBcd(dt.Month),
                DecToBcd(dt.Year - 2000)
            };

            var returnValue = WriteRegister(Registers.RTC_START_ADDRESS, clockData);

            if (returnValue == false)
            {
                WriteByte(Registers.RTC_SET_BIT_ADDRESS, 0); // Unset the RTC Set Bit in SRAM.
                Debug.Print("Failed to set RTC");
                return false;
            }

            if (preserveClockHaltState)
            {
                ClockHalt(!oscOn); // Needed as setting the DateTime at Address 0x00 will over write the Clock Halt Bit. 
            }
            WriteByte(Registers.RTC_SET_BIT_ADDRESS, 1);
            return true;
        }

        /// <summary>
        /// Enables or Disables the Square Wave Generation function of the RTC.
        /// </summary>
        /// <param name="frequency">The desired frequency or disabled</param>
        /// <param name="outputControl">Logical level of output pin when the frequency is disabled (zero by default)</param>
        /// <example>
        /// <code language = "C#">
        ///  _clock.SetSquareWave(RTC2Click.SqwFrequency.Sqw1Hz, RTC2Click.SqwOutputControl.On);
        /// </code>
        /// <code language = "VB">
        ///  _clock.SetSquareWave(RTC2Click.SqwFrequency.Sqw1Hz, RTC2Click.SqwOutputControl.On)
        /// </code>
        /// </example>
        public bool SetSquareWave(SqwFrequency frequency, SqwOutputControl outputControl = SqwOutputControl.Off)
        {
            var sqwCtrlReg = (byte)outputControl;
            sqwCtrlReg <<= 3;
            if (frequency != SqwFrequency.SqwOff) { sqwCtrlReg |= 1; }
            sqwCtrlReg <<= 4;
            sqwCtrlReg |= (byte)frequency;
            return WriteByte(Registers.SQUARE_WAVE_CTRL_REGISTER_ADDRESS, sqwCtrlReg);
        }

        /// <summary>
        /// Writes to the RTC user RAM registers as a block
        /// </summary>
        /// <param name="buffer">A byte buffer of size USER_RAM_SIZE</param>
        /// <example>
        /// <code language = "C#">
        /// string text = "There are 48 available bytes in USER SRAM.";
        /// var userSram = new byte[RTC2Click.Registers.USER_RAM_SIZE];
        ///
        /// // Copy the string to the ram buffer
        /// for (byte b = 0; b <![CDATA[<]]> text.Length; b++)
        /// {
        ///     userSram[b] = (byte) text[b];
        /// }
        /// // Write it to the RAM in the clock
        /// _clock.WriteUSerSram(userSram);
        /// </code>
        /// <code language = "VB">
        /// Dim text As String = "There are 48 available bytes in USER SRAM."
        /// Dim userSram = New Byte(RTC2Click.Registers.USER_RAM_SIZE - 1) {}
        ///
        /// ' Copy the string to the ram buffer
        /// For b As Byte = 0 To CByte(text.Length - 1)
        ///	    userSram(b) = CByte(Strings.AscW(text(b)))
        /// Next
        /// 
        /// ' Write it to the RAM in the clock
        /// _clock.WriteUSerSram(userSram)
        /// </code>
        /// </example>
        public bool WriteUSerSram(byte[] buffer)
        {
            if (buffer.Length != Registers.USER_RAM_SIZE) { throw new ArgumentOutOfRangeException("buffer", "Invalid buffer length"); }
            var trxBuffer = new byte[sizeof(byte) + Registers.USER_RAM_SIZE];
            trxBuffer[0] = Registers.USER_RAM_START_ADDRESS;
            buffer.CopyTo(trxBuffer, 1);
            return WriteRegister(Registers.USER_RAM_START_ADDRESS, trxBuffer);
        }

        /// <summary>
        /// Writes a to SRAM at the specified address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <param name="value">The byte to write.</param>
        /// <returns>True if successful, otherwise false.</returns>
        /// <example>
        /// <code language = "C#">
        /// for (byte b = RTC2Click.Registers.RTC_START_ADDRESS; b <![CDATA[<]]>= RTC2Click.Registers.USER_RAM_END_ADDRESS; b++)
        /// {
        ///    _clock.WriteUserSramAddress(b, b);
        /// }
        /// </code>
        /// <code language = "VB">
        /// For b As Byte = RTC2Click.Registers.RTC_START_ADDRESS To RTC2Click.Registers.USER_RAM_END_ADDRESS
        ///    _clock.WriteUserSramAddress(b, b)
        /// Next
        /// </code>
        /// </example>
        public bool WriteUserSramAddress(byte address, byte value)
        {
            if (address > Registers.USER_RAM_END_ADDRESS) { throw new ArgumentOutOfRangeException("address", "Invalid register address"); }
            return WriteByte(address, value);
        }

        /// <summary>
        /// Reads the clock's user RAM registers as a block.
        /// </summary>
        /// <returns>A byte array of size USER_RAM_SIZE containing the user RAM data</returns>
        /// <example>
        /// <code language = "C#">
        /// userSram = _clock.ReadUserSram();
        ///  
        /// for (byte I = 0; I <![CDATA[<]]> userSram.Length; I++)
        /// {
        ///    text += (char) userSram[I];
        /// }
        ///
        /// if (text != null) Debug.Print("Reading from USER SRAM: " + text + "\n");
        /// </code>
        /// <code language = "VB">
        /// Dim userSram = _clock.ReadUserSram()
        ///
        /// For I As Byte = 0 To CByte(userSram.Length - 1)
        ///    text += CChar(Strings.ChrW(userSram(I)))
        /// Next
        ///
        /// If text IsNot Nothing Then
        ///     Debug.Print("Reading from USER SRAM: " <![CDATA[&]]> text)
        /// End If
        /// </code>
        /// </example>
        public byte[] ReadUserSram()
        {
            var data = new byte[Registers.USER_RAM_SIZE];

            var transaction = new I2CDevice.I2CTransaction[2];
            transaction[0] = I2CDevice.CreateWriteTransaction(new[] { Registers.USER_RAM_START_ADDRESS });
            transaction[1] = I2CDevice.CreateReadTransaction(data);
            Hardware.I2CBus.Execute(_i2CConfiguration, transaction, _i2CTimeout);
            return data;
        }

        /// <summary>
        /// Reads User RAM  at the specified register address.
        /// </summary>
        /// <param name="address">Register address between 0x08 and 0x38 (48 Bytes)</param>
        /// <returns>The value of the byte read at the specified address</returns>
        /// <example>
        /// <code language = "C#">
        /// for (byte b = RTC2Click.Registers.USER_RAM_START_ADDRESS; b <![CDATA[<]]>= RTC2Click.Registers.USER_RAM_END_ADDRESS; b++)
        /// {
        ///     Debug.Print(b + ": " + _clock.ReadUserSramAddress(b));
        /// }
        /// </code>
        /// <code language = "VB">
        /// For b As Byte = RTC2Click.Registers.USER_RAM_START_ADDRESS To RTC2Click.Registers.USER_RAM_END_ADDRESS
        ///     Debug.Print(b <![CDATA[&]]> ": " <![CDATA[&]]> _clock.ReadUserSramAddress(b))
        /// Next
        /// </code>
        /// </example>
        public byte ReadUserSramAddress(byte address)
        {
            if (address > Registers.USER_RAM_END_ADDRESS) { throw new ArgumentOutOfRangeException("address", "Invalid register address"); }
            return ReadByte(address);
        }

        /// <summary>
        /// Resets the RTC2Click.
        /// </summary>
        /// <param name="resetMode">The reset mode, see <see cref="ResetModes"/> for more information.</param>
        /// <returns cref="NotImplementedException">Calling this method will throw a <see cref="NotImplementedException"/>.</returns>
        /// <remarks>
        /// This module has no Reset method, calling this method will throw a <see cref="NotImplementedException"/>.
        /// </remarks>
        /// <exception cref="NotImplementedException"></exception>
        /// <example>None: This sensor does not support a Reset method.</example>
        public bool Reset(ResetModes resetMode)
        {
            throw new NotImplementedException();
        }
     
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <returns cref="NotImplementedException">Calling this method will throw a <see cref="NotImplementedException"/>.</returns>
        /// <remarks>
        /// This module does not use Power Modes, the GET accessor will always return PowerModes.On. See <see cref="PowerModes"/>, while the SET accessor will do nothing.
        /// </remarks>
        /// <example>None: This sensor does not support PowerMode.</example>
        public PowerModes PowerMode
        {
            get { return PowerModes.On; }
            set { }
        }

        /// <summary>
        ///     Gets the driver version.
        /// </summary>
        /// <value>
        ///     The driver version see <see cref="Version"/>.
        /// </value>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.Print("Driver Version Info : " + _clock1.DriverVersion);
        /// </code>
        /// <code language="VB">
        /// Debug.Print("Driver Version Info : " <![CDATA[&]]> _clock1.DriverVersion)
        /// </code>
        /// </example>
        public Version DriverVersion
        {
            get { return Assembly.GetAssembly(GetType()).GetName().Version; }
        } 

        #endregion

    }
}