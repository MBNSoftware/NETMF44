﻿/* FM Click Driver for MikrobusNet 
 * Version 1.0 :
 *  - Initial version coded by Stephen Cardinale
 *  - This version was never publicly released.
 * Version 2.0
 *  - Implementation of new MBN Namespaces and Interface requirements.
 *  2BDun - Future Versions
 *  - Implement Stereo Indicator on GPIO1
 *  - Implement RDS Indicator on GPIO2
 *  - Implement Mute Indicator on GPIO3
 *  - Implement Extended Volume Attenuation
 *  - Implement DC clamping LOut and ROut during Low Power Modes to limit/minimize noise.
 *  - Implement SMute (SMute)
 *  - Implement more robust RDS capability.
 *  - Implement many other remaining features of the IC.
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
using System.Reflection;
using MBN.Enums;
using System.Threading;
using MBN.Extensions;
using Microsoft.SPOT.Hardware;
using MBN.Exceptions;

namespace MBN.Modules
{
    /// <summary>
    /// An FM Radio Click driver for MikroBusNet
    /// <para><b>This module is an I2C Device</b></para>
    /// <para><b>Pins used :</b> Scl, Sda</para>
    /// <para><b>References required:</b> MikroBus.Net, Microsoft.SPOT.Native, Microsoft.Spot.Hardware, mscorlib</para>
    /// </summary>
    /// <example>
    /// <code language = "C#">
    /// using System.Threading;
    /// using MBN;
    /// using MBN.Enums;
    /// using MBN.Modules;
    /// using Microsoft.SPOT;
    ///
    /// namespace Examples
    /// {
    ///     public class Program
    ///     {
    ///         private static FMClick _fm;
    ///
    ///         public static void Main()
    ///         {
    ///             _fm = new FMClick(Hardware.SocketFour, ClockRatesI2C.Clock400KHz, 100);
    ///
    ///             /* If you are outside the USA or Australia, you must change the Channel Spacing and Radio Band from the default of Spacing.UsaAustralia and Band.UsaEurope
    ///              *  In Europe - use Spacing.EuropeJapan and Band.UsaEurope.
    ///              *  In Japan - use Spacing.EuropeJapan and Band.Japan or JapanWide.
    ///              *  No other configurations are available.
    ///
    ///                 if (_fm.ChannelSpacing != FMClick.Spacing.UsaAustralia || _fm.RadioBand != FMClick.Band.UsaEurope)
    ///                 {
    ///                     _fm.SetRadioConfiguration(FMClick.Spacing.EuropeJapan, FMClick.Band.UsaEurope);
    ///                 }
    ///              */
    ///
    ///             //_fm.Volume = 1;
    ///             //_fm.Station = 93.3;
    ///
    ///             _fm.Volume = 7;
    ///             _fm.Station = 93.3;
    ///
    ///             _fm.RadioTextChanged +=_fm_RadioTextChanged;
    ///
    ///             new Thread(Capture).Start();
    ///
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    ///
    ///         private static void _fm_RadioTextChanged(FMClick sender, string newradiotext)
    ///         {
    ///             Debug.Print("RDS Text received - " + newradiotext);
    ///         }
    ///
    ///         private static void Capture()
    ///         {
    ///             while (true)
    ///             {
    ///                 var rssi = _fm.RSSI;
    ///                 var rssiPercentage = (double)(rssi) / 75 * 100;
    ///
    ///                 Debug.Print("RSSI: " + _fm.RSSI + " of 75 (" + rssiPercentage.ToString("f0") + "%)");
    ///                 Debug.Print("Stereo: " + _fm.IsStereo);
    ///                 Debug.Print("Is Muted ? " + _fm.Mute);
    ///                 Debug.Print("ChannelSpacing? " + _fm.ChannelSpacing);
    ///                 Debug.Print("RadioBand? " + _fm.RadioBand);
    ///                 Debug.Print("Station ? " + _fm.Station.ToString("D1") + " Hz");
    ///                 Debug.Print("RDS: " + _fm.RadioText + "\n");
    ///                 Thread.Sleep(3000);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// <code language = "VB">
    /// Option Explicit On
    /// Option Strict On
    ///
    /// Imports MBN.Modules
    /// Imports MBN
    /// Imports MBN.Enums
    /// Imports System.Threading
    /// Imports Microsoft.SPOT
    ///
    /// Namespace Examples
    ///
    ///     Public Module Module1
    ///
    ///         Dim WithEvents _fm As FMClick
    ///
    ///         Sub Main()
    ///             _fm = New FMClick(Hardware.SocketFour, ClockRatesI2C.Clock400KHz, 100)
    ///
    ///             ' If you are outside the USA or Australia, you must change the Channel Spacing and Radio Band from the default of Spacing.UsaAustralia and Band.UsaEurope
    ///             ' In Europe - use Spacing.EuropeJapan and Band.UsaEurope.
    ///             ' In Japan - use Spacing.EuropeJapan and Band.Japan or JapanWide.
    ///             ' No other configurations are available.
    ///             ' Example for Europe: _fm.SetRadioConfiguration(FMClick.Spacing.EuropeJapan, FMClick.Band.UsaEurope)
    ///
    ///             _fm.Volume = 7
    ///             _fm.Station = 93.3
    ///
    ///             Dim capturethread As New Thread(New ThreadStart(AddressOf Capture))
    ///             capturethread.Start()
    ///
    ///             Thread.Sleep(Timeout.Infinite)
    ///         End Sub
    ///
    ///         Private Sub Capture()
    ///             While True
    ///                 Dim rssi = _fm.RSSI
    ///                 Dim rssiPercentage = CDbl(rssi) / 75 * 100
    ///
    ///                 Debug.Print("RSSI: " <![CDATA[&]]> _fm.RSSI <![CDATA[&]]> " of 75 (" <![CDATA[&]]> rssiPercentage.ToString("f0") <![CDATA[&]]> "%)")
    ///                 Debug.Print("Stereo: " <![CDATA[&]]> _fm.IsStereo)
    ///                 Debug.Print("Is Muted ? " <![CDATA[&]]> _fm.Mute)
    ///                 Debug.Print("ChannelSpacing? " <![CDATA[&]]> _fm.ChannelSpacing)
    ///                 Debug.Print("RadioBand? " <![CDATA[&]]> _fm.RadioBand)
    ///                 Debug.Print("Station ? " <![CDATA[&]]> _fm.Station.ToString("D1") <![CDATA[&]]> " Hz")
    ///                 Debug.Print("RDS: " <![CDATA[&]]> _fm.RadioText <![CDATA[&]]> Microsoft.VisualBasic.Constants.vbCrLf)
    ///                 Thread.Sleep(3000)
    ///             End While
    ///         End Sub
    ///     End Module
    /// End Namespace
    /// </code>
    /// </example>
// ReSharper disable once InconsistentNaming
    public class FMClick : IDriver
    {
        #region Constants
        
// ReSharper disable InconsistentNaming
        //private const int REGISTER_DEVICEID = 0x00;
        //private const int REGISTER_CHIPID = 0x01;
        private const int REGISTER_POWERCFG = 0x02;
        private const int REGISTER_CHANNEL = 0x03;
        private const int REGISTER_SYSCONFIG1 = 0x04;
        private const int REGISTER_SYSCONFIG2 = 0x05;
        private const int REGISTER_TEST = 0x07;
        private const byte REGISTER_STATUSRSSI = 0x0A;
        private const int REGISTER_READCHAN = 0x0B;
        //private const byte REGISTER_RDSA = 0x0C;
        private const byte REGISTER_RDSB = 0x0D;
        private const byte REGISTER_RDSC = 0x0E;
        private const byte REGISTER_RDSD = 0x0F;

        //Register 0x02 - REGISTER_POWERCFG
        //private const byte BIT_SMUTE = 15;
        private const byte BIT_DMUTE = 14;
        //private const byte BIT_SKMODE = 10;
        private const byte BIT_SEEKUP = 9;
        private const byte BIT_SEEK = 8;

        //Register 0x03 - CHANNEL
        private const byte BIT_TUNE = 15;

        //Register 0x04 - SYSCONFIG1
        private const int BIT_RDS = 12;
        //private const byte BIT_DE = 11;

        //Register 0x05 - SYSCONFIG2
        //private const byte BIT_SPACE1 = 5;
        //private const byte BIT_SPACE0 = 4;

        //Register 0x0A - STATUSRSSI
        private const byte BIT_RDSR = 15;
        private const byte BIT_STC = 14;
        private const byte BIT_SFBL = 13;
        //private const byte BIT_AFCRL = 12;
        //private const byte BIT_RDSS = 11;
        private const byte BIT_STEREO = 8;

// ReSharper restore InconsistentNaming

        #endregion

        #region Fields

        private static int _baseChannel = 875;
        private string _currentRadioText = "RDS not available.";
        private static int _currentVolume;
        private static int _spacingDivisor = 2;
        private static Spacing _channelSpacing = Spacing.UsaAustralia;
        private static Band _radioBand = Band.UsaEurope;
        private Single _maxChannel;
        private Single _minChannel;

        private static int[] _shadowRegisters = new int[16];

        private static I2CDevice.Configuration _i2CConfiguration;
        private static int _i2CTimeOut;

        private static OutputPort _resetPin;

        #endregion

        #region ENUMS

        /// <summary>
        /// The radio frequency band.
        /// </summary>
        public enum Band
        {
            /// <summary>
            /// The band used in the United States and Europe (87.5-108MHz).
            /// </summary>
            UsaEurope,

            /// <summary>
            ///     The wide band used in the Japan (76-108MHz).
            /// </summary>
            JapanWide,

            /// <summary>
            /// The band used in Japan (76-90MHz).
            /// </summary>
            Japan
        }

        /// <summary>
        /// The enumeration that determines which direction to Seek when calling Seek(direction);
        /// </summary>
        public enum SeekDirection
        {
            /// <summary>
            /// Seeks for a higher station number.
            /// </summary>
            Forward,

            /// <summary>
            /// Seeks for a lower station number.
            /// </summary>
            Backward
        };

        /// <summary>
        ///     The radio channel Spacing.
        /// </summary>
        public enum Spacing
        {
            /// <summary>
            /// Spacing in USA and Australia (200KHz).
            /// </summary>
            UsaAustralia,

            /// <summary>
            ///     Spacing in Europe and Japan (100KHz).
            /// </summary>
            EuropeJapan
        }

        #endregion

        #region CTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="FMClick"/> class.
        /// </summary>
        /// <param name="socket">The socket that this module is plugged in to.</param>
        /// <param name="clockRateI2C">The <see cref="ClockRatesI2C"/> </param>
        /// <param name="i2CTimeout">The I2C Transaction Timeout Period before returning an error in I2C communication.</param>
        public FMClick(Hardware.Socket socket, ClockRatesI2C clockRateI2C, int i2CTimeout = 1000)
        {
            try
            {
                // Checks if needed I²C pins are available
                Hardware.CheckPinsI2C(socket, socket.Rst);

                _i2CTimeOut = i2CTimeout;

                // Create the driver's I²C configuration
                _i2CConfiguration = new I2CDevice.Configuration(0x10, (int)clockRateI2C);

                // Check if it's the first time an I²C device is created
                if (Hardware.I2CBus == null)
                {
                    Hardware.I2CBus = new I2CDevice(_i2CConfiguration);
                }

                _resetPin = new OutputPort(socket.Rst, false);

                PowerUp();

                SetRadioConfiguration(Spacing.UsaAustralia, Band.UsaEurope);

                //Start at the bottom of the radio band and at the lowest output volume.
                Station = MinChannel;
                Volume = MinVolume;

                new Thread(RdsWorkerThread).Start();
            }
            // Catch only the PinInUse exception, so that program will halt on other exceptions and send it directly to caller.
            catch (PinInUseException ex)
            {
                throw new PinInUseException(ex.Message);
            }
        }

        #endregion

        #region Private Methods

        private static void PowerUp()
        {
            if (_resetPin.Read() == false) _resetPin.Write(true);

            ReadRegisters();

            _shadowRegisters[REGISTER_TEST] = 0x8100; //Enable the oscillator and yes, that what SI calls this register.

            UpdateRegisters();

            Thread.Sleep(500); // Wait for clock to settle - from AN230 Datasheet

            ReadRegisters();
            _shadowRegisters[REGISTER_POWERCFG] = 0x4001; 
            _shadowRegisters[REGISTER_SYSCONFIG1] |= (1 << BIT_RDS);
            _shadowRegisters[REGISTER_SYSCONFIG2] &=  0xFFCF;
            _shadowRegisters[REGISTER_SYSCONFIG2] &= 0xFFCF;
            _shadowRegisters[REGISTER_SYSCONFIG2] &= 0xFF3F;
            _shadowRegisters[REGISTER_SYSCONFIG2] &= 0xFFF0;
            _shadowRegisters[REGISTER_SYSCONFIG2] |= 0x001F;

            UpdateRegisters();

            Thread.Sleep(110);
        }

        private static void ReadRegisters()
        {
            byte[] registers = Read(64);
            var newBuffer = new byte[32];
            Array.Copy(registers, 12, newBuffer, 0, 32);
            var pSi4703Registers = new int[16];
            for (int x = 0x00; x <= 0x0F; x++)
            {
                //Read in these 32 bytes
                pSi4703Registers[x] = newBuffer[2*x] << 8;
                pSi4703Registers[x] |= newBuffer[2*x + 1];
            }
            _shadowRegisters = pSi4703Registers;
        }

        private static void UpdateRegisters()
        {
            var bytesForUpdate = new byte[32];
            int i = 0;
            foreach (int word in _shadowRegisters)
            {
                int highByte = word >> 8;
                int lowByte = word & 0x00FF;
                bytesForUpdate[i] = (byte) highByte;
                i++;
                bytesForUpdate[i] = (byte) lowByte;
                i++;
            }

            var newBytesForUpdate = new byte[15];

            Array.Copy(bytesForUpdate, 4, newBytesForUpdate, 0, 14);

            Write(newBytesForUpdate);
        }

        private static byte[] Read(int responseLength)
        {
            var buffer = new byte[responseLength];
            var actions = new I2CDevice.I2CTransaction[1];
            actions[0] = I2CDevice.CreateReadTransaction(buffer);
            Hardware.I2CBus.Execute(_i2CConfiguration, actions, _i2CTimeOut);
            return buffer;
        }

        private static void Write(byte[] bytesToWrite)
        {
            var actions = new I2CDevice.I2CTransaction[1];
            actions[0] = I2CDevice.CreateWriteTransaction(bytesToWrite);
            Hardware.I2CBus.Execute(_i2CConfiguration, actions, _i2CTimeOut);
        }

        private static void SetDeviceVolume(ushort volume)
        {
            ReadRegisters();
            _shadowRegisters[REGISTER_SYSCONFIG2] &= 0xFFF0; //Clear Volume bits
            _shadowRegisters[REGISTER_SYSCONFIG2] |= volume; //Set Volume to lowest
            UpdateRegisters();
        }

        private static int GetDeviceChannel()
        {
            ReadRegisters();
            int channel = _shadowRegisters[REGISTER_READCHAN] & 0x03FF;
            return channel * _spacingDivisor + _baseChannel;
        }

        private static void SetDeviceChannel(int newChannel)
        {
            newChannel -= _baseChannel;
            newChannel /= _spacingDivisor;

            ReadRegisters();
            _shadowRegisters[REGISTER_CHANNEL] &= 0xFE00; //Clear out the Station bits
            _shadowRegisters[REGISTER_CHANNEL] |= (ushort) newChannel; //Mask in the new Station
            _shadowRegisters[REGISTER_CHANNEL] |= (1 << BIT_TUNE); //Set the TUNE bit to start
            UpdateRegisters();

            //Poll to see if STC is set
            while (true)
            {
                ReadRegisters();
                if ((_shadowRegisters[REGISTER_STATUSRSSI] & (1 << BIT_STC)) != 0) break; //Tuning complete!
            }

            //ReadRegisters();
            _shadowRegisters[REGISTER_CHANNEL] &= 0x7FFF; //Clear the tune after a tune has completed
            UpdateRegisters();

            //Wait for the si4703 to clear the STC as well
            while (true)
            {
                ReadRegisters();
                if ((_shadowRegisters[REGISTER_STATUSRSSI] & (1 << BIT_STC)) == 0) break; //Tuning complete!
            }
        }

        private static bool SeekDevice(SeekDirection direction)
        {
            ReadRegisters();

            //Set Seek mode wrap bit
            _shadowRegisters[REGISTER_POWERCFG] &= 0xFBFF;

            switch (direction)
            {
                case SeekDirection.Backward:
                    _shadowRegisters[REGISTER_POWERCFG] &= 0xFDFF; //Seek down is the default upon reset
                    break;
                default:
                    _shadowRegisters[REGISTER_POWERCFG] |= 1 << BIT_SEEKUP; //Set the bit to Seek up
                    break;
            }

            _shadowRegisters[REGISTER_POWERCFG] |= (1 << BIT_SEEK); //Start Seek

            UpdateRegisters();

            // Poll to see if STC is set
            while (true)
            {
                ReadRegisters();
                if ((_shadowRegisters[REGISTER_STATUSRSSI] & (1 << BIT_STC)) != 0) break;
            }

            ReadRegisters();

            int valueSfbl = _shadowRegisters[REGISTER_STATUSRSSI] & (1 << BIT_SFBL);
            _shadowRegisters[REGISTER_POWERCFG] &= 0xFEFF; //Clear the Seek bit after Seek has completed

            UpdateRegisters();

            //Wait for the si4703 to clear the STC as well
            while (true)
            {
                ReadRegisters();
                if ((_shadowRegisters[REGISTER_STATUSRSSI] & (1 << BIT_STC)) == 0) break;
            }

            return valueSfbl <= 0;
        }

        private static bool IsBitSet(int value, int pos)
        {
            return (value & (1 << pos)) != 0;
        }

        private void RdsWorkerThread()
        {

            const int radioTextGroupCode = 2;
            const int toggleFlagPosition = 5;
            const int charsPerSegment = 2;

            const int maxMessageLength = 64;
            const int maxSegments = 16;
            const int maxCharsPerGroup = 4;

            const int versionATextSegmentPerGroup = 2;
            const int versionBTextSegmentPerGroup = 1;

            var currentRadioText = new char[maxMessageLength];
            var isSegmentPresent = new bool[maxSegments];
            int endOfMessage = -1;
            int endSegmentAddress = -1;
            string lastMessage = "";
            int lastTextToggleFlag = -1;
            bool waitForNextMessage = false;

            while (true)
            {
                ReadRegisters();
                //var a = (ushort)_shadowRegisters[REGISTER_RDSA];
                var b = (ushort)_shadowRegisters[REGISTER_RDSB];
                var c = (ushort)_shadowRegisters[REGISTER_RDSC];
                var d = (ushort)_shadowRegisters[REGISTER_RDSD];
                bool ready = (_shadowRegisters[REGISTER_STATUSRSSI] & (1 << BIT_RDSR)) != 0;

                if (ready)
                {
                    //int programIdCode = a;
                    int groupTypeCode = b >> 12;
                    int versionCode = (b >> 11) & 0x1;
                    //int trafficIDCode = (b >> 10) & 0x1;
                    //int programTypeCode = (b >> 5) & 0x1F;

                    if (groupTypeCode == radioTextGroupCode)
                    {
                        int textToggleFlag = b & (1 << (toggleFlagPosition - 1));
                        if (textToggleFlag != lastTextToggleFlag)
                        {
                            currentRadioText = new char[maxMessageLength];
                            lastTextToggleFlag = textToggleFlag;
                            waitForNextMessage = false;
                        }
                        else if (waitForNextMessage)
                        {
                            continue;
                        }

                        int segmentAddress = (b & 0xF);
                        int textAddress ; // = -1;
                        isSegmentPresent[segmentAddress] = true;

                        if (versionCode == 0)
                        {
                            textAddress = segmentAddress * charsPerSegment * versionATextSegmentPerGroup;
                            currentRadioText[textAddress] = (char)(c >> 8);
                            currentRadioText[textAddress + 1] = (char)(c & 0xFF);
                            currentRadioText[textAddress + 2] = (char)(d >> 8);
                            currentRadioText[textAddress + 3] = (char)(d & 0xFF);
                        }
                        else
                        {
                            textAddress = segmentAddress * charsPerSegment * versionBTextSegmentPerGroup;
                            currentRadioText[textAddress] = (char)(d >> 8);
                            currentRadioText[textAddress + 1] = (char)(d & 0xFF);
                        }

                        if (endOfMessage == -1)
                        {
                            for (int i = 0; i < maxCharsPerGroup; ++i)
                            {
                                if (currentRadioText[textAddress + i] == 0xD)
                                {
                                    endOfMessage = textAddress + i;
                                    endSegmentAddress = segmentAddress;
                                }
                            }
                        }

                        if (endOfMessage == -1) continue;

                        bool complete = true;
                        for (int i = 0; i < endSegmentAddress; ++i)
                            if (!isSegmentPresent[i]) complete = false;

                        if (!complete) continue;

                        var message = new string(currentRadioText, 0, endOfMessage);
                        if (message == lastMessage)
                        {
                            _currentRadioText = message;
                            OnRadioTextChanged(this, message);
                            waitForNextMessage = true;

                            for (int i = 0; i < endSegmentAddress; ++i)
                                isSegmentPresent[i] = false;

                            endOfMessage = -1;
                            endSegmentAddress = -1;
                        }

                        lastMessage = message;
                    }
                    Thread.Sleep(35);
                }
                else
                {
                    Thread.Sleep(40);
                }
            }
        // ReSharper disable once FunctionNeverReturns
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the ChannelSpacing that the FMClick is programmed for.
        /// </summary>
        /// <value>See <see cref="Spacing"/> for more information.</value>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("Channel Spacing - " + _fm.ChannelSpacing);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Channel Spacing - " <![CDATA[&]]> _fm.ChannelSpacing)
        /// </code>
        /// </example>
        public Spacing ChannelSpacing
        {
            get { return _channelSpacing; }
        }

        /// <summary>
        /// Gets the driver version.
        /// </summary>
        /// <value>
        /// The driver version.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///      Debug.Print ("Current driver version : "+_keypad.DriverVersion);
        /// </code>
        /// </example>
        public Version DriverVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        /// <summary>
        /// Returns true if the currently tuned radio station is broadcasting in Stereo, otherwise false. 
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("Is Stereo: " + _fm.IsStereo);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Is Stereo: " <![CDATA[&]]> _fm.IsStereo)
        /// </code>
        /// </example>
        public bool IsStereo
        {
            get
            {
                ReadRegisters();
                return (_shadowRegisters[REGISTER_STATUSRSSI] & (1 << BIT_STEREO)) > 0;
            }
        }

        /// <summary>
        /// Gets the maximum channel the FMClick can be tuned to.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("Maximum Allowable Station - " + _fm.MaxChannel);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Maximum Allowable Station - " <![CDATA[&]]> _fm.MaxChannel)
        /// </code>
        /// </example>
        public Single MaxChannel
        {
            get { return _maxChannel; }
        }

        /// <summary>
        /// Gets the minimum channel the FMClick can be tuned to.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("Minimum Allowable Station - " + _fm.MinChannel);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Minimum Allowable Station - " <![CDATA[&]]> _fm.MinChannel)
        /// </code>
        /// </example>
        public Single MinChannel
        {
            get { return _minChannel; }
        }
        
        /// <summary>
        /// Gets or sets the Hardware Mute (silence) of the FMClick.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// if (!_fm.Mute) _fm.Mute = true;
        /// </code>
        /// <code language = "VB">
        /// If Not _fm.Mute Then
        ///	    _fm.Mute = True
        /// End If
        /// </code>
        /// </example>
        public bool Mute
        {
            get { return (_shadowRegisters[REGISTER_POWERCFG] & (1 << BIT_DMUTE)) == 0; }
            set
            {
                ReadRegisters();
                if (value)
                {
                    _shadowRegisters[REGISTER_POWERCFG] ^= (1 << BIT_DMUTE);
                }
                else
                {
                    _shadowRegisters[REGISTER_POWERCFG] |= (1 << BIT_DMUTE);
                }
                UpdateRegisters();
            }
        }

        /// <summary>
        /// Gets or sets the power mode of the FMClick.
        /// </summary>
        /// <value>
        /// The current <see cref="PowerModes"/> of the module.
        /// </value>
        /// <remarks>All settings are preserved when changing power modes.</remarks>
        /// <example>Example usage:
        /// <code language = "C#">
        ///  _fm.PowerMode = PowerModes.Low;
        /// </code>
        /// <code language = "VB">
        ///  _fm.PowerMode = PowerModes.Low
        /// </code>
        /// </example>
        public PowerModes PowerMode
        {
            get
            {
                ReadRegisters();
                if (IsBitSet(_shadowRegisters[REGISTER_POWERCFG], 0) && !IsBitSet(_shadowRegisters[REGISTER_POWERCFG], 6)) return PowerModes.On;
                if (IsBitSet(_shadowRegisters[REGISTER_POWERCFG], 0) && IsBitSet(_shadowRegisters[REGISTER_POWERCFG], 6)) return PowerModes.Low;
                return !IsBitSet(_shadowRegisters[REGISTER_POWERCFG], 15) ? PowerModes.Off : PowerModes.Off;
            }
            set
            {
                ReadRegisters();
                _shadowRegisters[REGISTER_POWERCFG] = value == PowerModes.Low ? 0x0041 : value == PowerModes.On ? 0x4001 : 0x0000;
                UpdateRegisters();
            }
        }

        /// <summary>
        /// Gets the RadioBand that the FMClick is programmed for.
        /// </summary>
        /// <value>See <see cref="Band"/> for more information.</value>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("Radio Band -  " + _fm.RadioBand);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Radio Band - " <![CDATA[&]]> _fm.RadioBand)
        /// </code>
        /// </example>
        public Band RadioBand
        {
            get { return _radioBand; }
        }

        /// <summary>
        /// Gets the current Radio Data System (RDS) Text. See <see href="http://en.wikipedia.org/wiki/Radio_Data_System">Wikipedia</see> for more information).
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("RDS/RBDS: " + _fm.RadioText);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("RDS/RBDS: " <![CDATA[&]]> _fm.RadioText)
        /// </code>
        /// </example>
        public string RadioText
        {
            get { return _currentRadioText; }
        }

        /// <summary>
        /// Returns the RSSI (Received Signal Strength Indicator) of the current station.
        /// </summary>
        /// <returns>Returns the RSSI from 0 to 75 (Full Scale ).</returns>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("RSSI: " + _fm.RSSI);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("RSSI: " <![CDATA[&]]> _fm.RSSI)
        /// </code>
        /// </example>
        public int RSSI
        {
            get
            {
                ReadRegisters();
                var rssi = (byte) (_shadowRegisters[REGISTER_STATUSRSSI] & 0x00FF); 
                return rssi;
            }
        }

        /// <summary>
        /// Gets or sets the Station (Channel) of the FMClick.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _fm.Station = 93.3;
        ///  Debug.Print("Current Station  - " + _fm.Station);
        /// </code>
        /// <code language = "VB">
        /// _fm.Station = 93.3;
        ///  Debug.Print("Current Station  - " <![CDATA[&]]> _fm.Station)
        /// </code>
        /// </example>
        public Double Station
        {
            get
            {
                return (GetDeviceChannel()) / 10.0;
            }
            set
            {
                if (value > _maxChannel || value < _minChannel) throw new ArgumentOutOfRangeException("value", "The Station provided was outside the allowable range.");
                SetDeviceChannel((int) (value * 10));
            }
        }

        /// <summary>
        /// Gets or sets the Volume of the FMClick.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _fm.Volume = FMClick.MaxVolume;
        /// // Or you can use 
        /// _fm.Volume = 7;
        /// </code>
        /// <code language = "VB">
        /// _fm.Volume = FMClick.MaxVolume;
        /// ' Or you can use
        /// _fm.Volume = 7
        /// </code>
        /// </example>
        public int Volume
        {
            get { return _currentVolume; }
            set
            {
                if (value > MaxVolume || value < MinVolume) return;
                _currentVolume = value;
                SetDeviceVolume((ushort) value);
            }
        }

        #endregion

        #region Public Constants

        /// <summary>
        ///     The Station returned by <see cref="Seek" /> when no Station is found during a seek or scan operation.
        /// </summary>
        public const float InvalidChannel = -1.0F;

        /// <summary>
        /// The minimum Volume the device can output.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _fm.Volume = FMClick.MinVolume; // Same effect as setting the Mute Property to true.
        /// </code>
        /// <code language = "VB">
        /// _fm.Volume = FMClick.MinVolume; ' Same effect as setting the Mute Property to True.
        /// </code>
        /// </example>
        public const int MinVolume = 0;

        /// <summary>
        /// The maximum Volume the device can output.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _fm.Volume = FMClick.MaxVolume;
        /// </code>
        /// <code language = "VB">
        /// _fm.Volume = FMClick.MaxVolume;
        /// </code>
        /// </example>
        public const int MaxVolume = 15;

        #endregion

        #region Public Methods

        /// <summary>
        /// Decreases the Volume by one.
        /// </summary>
        /// <remarks>Typically this method would be used by an external stimulus such as a key press of the KeyPad4x3.</remarks>
        /// <example>Example usage:
        /// <code language = "C#">
        /// static void _keypad_KeyPressed(object sender, Keypad4X3.KeyPressedEventArgs e)
        /// {
        ///     _fm.DecreaseVolume();
        /// }
        /// </code>
        /// <code language = "VB">
        /// Private Sub _keypad_KeyPressed(sender As Object, e As Keypad4X3.KeyPressedEventArgs) Handles _keypad.KeyPressed
        ///    _fm.DecreaseVolume()
        /// End Sub
        /// </code>
        /// </example>
        public void DecreaseVolume()
        {
            --Volume;
        }

        /// <summary>
        /// Increases the Volume by one.
        /// </summary>
        /// <remarks>Typically this method would be used by an external stimulus such as a key press of the KeyPad4x3.</remarks>
        /// <example>Example usage:
        /// <code language = "C#">
        /// static void _keypad_KeyPressed(object sender, Keypad4X3.KeyPressedEventArgs e)
        /// {
        ///     _fm.IncreaseVolume();
        /// }
        /// </code>
        /// <code language = "VB">
        /// Private Sub _keypad_KeyPressed(sender As Object, e As Keypad4X3.KeyPressedEventArgs) Handles _keypad.KeyPressed
        ///    _fm.IncreaseVolume()
        /// End Sub
        /// </code>
        /// </example>
        public void IncreaseVolume()
        {
            ++Volume;
        }

        /// <summary>
        /// Resets the FMClick
        /// </summary>
        /// <param name="resetMode">The type of <see cref="ResetModes"/> to perform.</param>
        /// <exception cref="NotImplementedException">A NotImplementedException will be thrown is <see cref="ResetModes.Soft"/> is passed to the Setter of this Property.</exception>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("Reset Successful? " +_fm.Reset(ResetModes.Hard));
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Reset Successful? " <![CDATA[&]]>_fm.Reset(ResetModes.Hard))
        /// </code>
        /// </example>
        public bool Reset(ResetModes resetMode)
        {
            if (resetMode == ResetModes.Soft) throw new NotImplementedException("ResetModes.Soft not implemented for this module. Use ResetModes.Hard to reset this module.");

            _resetPin.Write(false);
            Thread.Sleep(10);

            ReadRegisters();
            var success = _shadowRegisters[REGISTER_TEST] == 0x000;

            PowerUp();
            return success;
        }

        /// <summary>
        /// Tells the radio to Seek in the given direction until it finds a station.
        /// </summary>
        /// <param name="direction">The direction to <see cref="SeekDirection"/> the radio.</param>
        /// <remarks>It does wrap around when seeking.</remarks>
        /// <returns>The Station that was tuned to or <see cref="InvalidChannel" /> if no Station was found.</returns>
        /// <example>Example usage:
        /// <code language = "C#">
        /// static void _keypad_KeyPressed(object sender, Keypad4X3.KeyPressedEventArgs e)
        /// {
        ///      _fm.Seek(FMClick.SeekDirection.Forward);
        /// }
        /// </code>
        /// <code language = "VB">
        /// Private Sub _keypad_KeyPressed(sender As Object, e As Keypad4X3.KeyPressedEventArgs) Handles _keypad.KeyPressed
        ///     _fm.Seek(FMClick.SeekDirection.Forward);
        /// End Sub
        /// </code>
        /// </example>
        public double Seek(SeekDirection direction)
        {
            return SeekDevice(direction) ? Station : InvalidChannel;
        }

        /// <summary>
        /// Sets the Station Spacing and Band of the FMClick.
        /// </summary>
        /// <param name="spacing">The channel <see cref="Spacing"/> to use.</param>
        /// <param name="band">The radio <see cref="Band"/> to use.</param>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _fm.SetRadioConfiguration(FMClick.Spacing.EuropeJapan, FMClick.Band.UsaEurope);
        /// </code>
        /// <code language = "VB">
        /// _fm.SetRadioConfiguration(FMClick.Spacing.EuropeJapan, FMClick.Band.UsaEurope)
        /// </code>
        /// </example>
        public void SetRadioConfiguration(Spacing spacing, Band band)
        {
            ReadRegisters();

            switch (spacing)
            {
                case Spacing.UsaAustralia:
                {
                    _shadowRegisters[REGISTER_SYSCONFIG2] &= 0xFFCF;
                    _spacingDivisor = 2;
                    _channelSpacing = Spacing.UsaAustralia;
                    break;
                }
                case Spacing.EuropeJapan:
                {
                    _shadowRegisters[REGISTER_SYSCONFIG2] &= 0xFFDF;
                    _spacingDivisor = 1;
                    _channelSpacing = Spacing.EuropeJapan;
                    break;
                }
            }

            switch (band)
            {
                case Band.UsaEurope:
                {
                    _shadowRegisters[REGISTER_SYSCONFIG2] &= 0xFF3F;
                    _baseChannel = 875;
                    _minChannel = 87.5F;
                   _maxChannel = 107.5F;
                    _radioBand = Band.UsaEurope;
                    break;
                }
                case Band.JapanWide:
                {
                    _shadowRegisters[REGISTER_SYSCONFIG2] &= 0xFF7F;
                    _baseChannel = 760;
                    _minChannel = 76;
                    _maxChannel = 108;
                    _radioBand = Band.JapanWide;
                    break;
                }
                case Band.Japan:
                {
                    _shadowRegisters[REGISTER_SYSCONFIG2] &= 0xFFBF;
                    _baseChannel = 760;
                    _minChannel = 76;
                    _maxChannel = 90;
                    _radioBand = Band.Japan;
                    break;
                }
            }
            UpdateRegisters();
        }

        /// <summary>
        /// Toggles the Hardware Mute of the FMClick to On/Off.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// </code>
        /// <code language = "VB">
        /// </code>
        /// </example>
        public void ToggleMute()
        {
            ReadRegisters();
            _shadowRegisters[REGISTER_POWERCFG] ^= (1 << BIT_DMUTE); //Toggle Mute bit
            UpdateRegisters();
        }

        #endregion

        #region Events

        /// <summary>
        /// Represents the delegate that is used to handle the <see cref="RadioTextChanged"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="FMClick"/> that raised the event.</param>
        /// <param name="newRadioText">The new Radio Text.</param>
        public delegate void RadioTextChangedHandler(FMClick sender, string newRadioText);

        /// <summary>
        /// Raised when new Radio Text is available.
        /// </summary>
        public event RadioTextChangedHandler RadioTextChanged;

        private RadioTextChangedHandler _radioTextChangedHandler;

        private void OnRadioTextChanged(FMClick sender, string newRadioText)
        {
            if (_radioTextChangedHandler == null) _radioTextChangedHandler = OnRadioTextChanged;
            if (RadioTextChanged != null) RadioTextChanged(sender, newRadioText);
        }

        #endregion

    }
}