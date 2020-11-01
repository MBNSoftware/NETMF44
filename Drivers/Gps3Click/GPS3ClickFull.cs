/*
 * GPS3 Click driver for Mikrobus.Net
 * 
 * Version 1.0 :
 *  - Initial version coded by Stephen Cardinale.
 *  - Not Implemented - LOCUS Logging Functionality.
 * 
 * References needed :
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Hardware.SerialPort
 *  Microsoft.SPOT.Native
 *  MikroBusNet
 *  mscorlib
 *  
 * Source for SimpleSerial taken from IggMoe's SimpleSerial Class https://www.ghielectronics.com/community/codeshare/entry/644
 *  
 * Copyright 2014 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 * press or implied. See the License for the specific language governing permissions and limitations under the License.
 * 
 */

#region Using

using System;
using System.Collections;
using System.Diagnostics;
using System.IO.Ports;
using System.Reflection;
using System.Text;
using System.Threading;
using MBN.Enums;
using MBN.Exceptions;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Math = System.Math;

#endregion

namespace MBN.Modules
{
    /// <summary>
    ///     Main class for the GPS3Click driver.
    ///     <para><b>Pins used :</b> Rx, Tx, Rst</para>
    ///     <para>The GPS3 click is a UART module and does not use Hardware Flow Control.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// This driver supports all features of the GPS3 Click.
    /// As it is quite large, you may want to use the driver with basic GPS support. This driver is also located on the Downloads Page.
    /// Alternatively, you can adopt this driver to suit your particular needs.
    /// </para>
    /// <para>
    /// VCC should be connected to the VBAT pads to maintain the Volatile Memory (VM) in the GPS module.
    /// If it is not connected, the module will always perform a Full Cold Start once it is powered on because the navigation data and time stored in the VM will be lost,
    /// and this will result in longer Time To First Fix (TTFF). In addition, any changes to the parameter settings through command, such as changing baud rate are also stored in the VM,
    /// these configuration settings will be lost and be reverted back to the default firmware value if VBACKUP is not connected
    /// If VBACKUP is connected and the previous data stored in the VM are valid, the TTFF time will be drastically reduced (hot-start or warm-start, depending on the length of time between each power-on).
    /// Things like RTC almanac/ephemeris and last known position as well as some other proprietary data are stored inside the VM.
    /// Typically you should see the GPS module perform a hot start if it is powered on within 2 hours of being turned off. Assuming VBACKUP is connected, of course.
    /// The GPS module will perform warm-start if it is powered on after more than 2 hours.
    /// If the ephemeris/almanac is determined to be unusable, such as when the receiver is moved across great distances or the ephemeris/almanac simply expires, then the receiver will perform Cold Start.
    /// Typically you will see the module perform cold start if it has been off for several days.
    /// </para>
    /// </remarks>
    /// <example>Example Usage:
    /// <code language = "C#">
    /// </code>
    /// <code language = "VB">
    /// </code>
    /// </example>
    public partial class GPS3Click : IDriver
    {
        #region Constants

        private const String EOL = "\r\n";

        private const String CMD_TEST_PACKET = "$PMTK000*32";

        private const String CMD_SET_POSITION_FIX_INTERVAL = "$PMTK220,"; 
        private const String CMD_SET_POSITION_FIX_CONTROL = "$PMTK300,"; 
        private const String CMD_QUERY_POSITION_FIX_CONTROL = "$PMTK400";

        private const String CMD_NMEA_SENTENCES_OFF = "$PMTK314,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0";
        private const String CMD_SET_NEMA_SENTENCES = "$PMTK314,";
        private const String CMD_QUERY_NMEA_SENTENCES = "$PMTK414";

        private const String CMD_QUERY_FIRMWARE_INFO = "$PMTK605";

        private const String CMD_SET_BAUDRATE = "$PMTK251,";

        private const string CMD_SET_AIC = "$PMTK286,";

        private const String CMD_SET_DEE = "$PMTK223,";

        private const String CMD_SET_EASY = "$PMTK869,1,";
        private const String CMD_QUERY_EASY_STATUS = "$PMTK869,0";

        private const String CMD_SET_STATIC_NAVIGATION_THRESHOLD = "$PMTK386,"; 
        private const String CMD_QUERY_STATIC_NAVIGATION = "$PMTK447"; 

        private const String CMD_QUERY_DPGS_MODE = "$PMTK401";
        private const String CMD_SET_DPGS = "$PMTK301,";

        private const String CMD_QUERY_SBAS = "$PMTK413";
        private const String CMD_SET_SBAS = "$PMTK313,";

        private const String CMD_HOT_START = "$PMTK101*32";
        private const String CMD_WARM_START = "$PMTK102*31";
        private const String CMD_COLD_START = "$PMTK103*30";
        private const String CMD_FULL_COLD_START = "$PMTK104*37";
        private const String CMD_STANDBY = "$PMTK161,0";

        private const String CMD_SET_PERIODIC_MODE = "$PMTK225,";
        private const String CMD_CANCEL_PERIODIC_MODE = "$PMTK225,0";
        
        private const String CMD_SET_HDOP_THRESHOLD = "$PMTK356,";
        private const String CMD_QUERY_HDOP_THRESHOLD = "$PMTK357";
        
        private const String CMD_SET_PMTKLSC = "$PMTK875,1,";
        private const String CMD_QUERY_PMTKLSC = "$PMTK875,0";
        
        #endregion

        #region Fields

        private static SimpleSerial _serial;

        private readonly OutputPort _resetPin;

        private static readonly AutoResetEvent _autoEvent = new AutoResetEvent(false);
        private const UInt16 _autoResetWaitTime = 500;
        private static Boolean _deviceReady;
        private static Boolean _waitingForRestart;
        private const UInt16 _waitForResponse = 2000;

        private static string _gpsFirmwareInfo = String.Empty;
       
        private AntennaStatus _antenna = AntennaStatus.Open;
        private BaudRates _baudRate = BaudRates.Baud_9600;

        private static String _waitMessage = String.Empty;

        private static NMEASubscriptions _subscriptions = new NMEASubscriptions();
        private static String _nmeaOutputString = String.Empty;
        
        private TimeSpan lastPositionFixTime;
        private static Boolean changingBuadRate;

        private static readonly Queue satelliteQueue = new Queue();
        private static readonly Satellite[] svTemp = new Satellite[4];

        private static Boolean _easyEnabled;
        private static Boolean _aicEnabled;
        private static Double _staticNavigationThreshold;
        private static UInt16 _positionFixInterval = 200;
        private static UInt16 _positionFixControlRate = 1000;
        private static Boolean _dpgsEnabled = true;
        private static Boolean _sbasEnabled = true;
        private static Boolean _pmtkSLCEnabled;
        private static Double _hdopThreshold;

        private static GSVData _lastGsvData = new GSVData();
        private static GSAData _lastGsaData = new GSAData();
        private static GGAData _lastGgaData = new GGAData();
        private static GLLData _lastGllData = new GLLData();
        private static RMCData _lastRmcData = new RMCData();
        private static VTGData _lastVTGData = new VTGData();
        
        #endregion

        #region ENUMS

        /// <summary>
        /// The available Restart types for the GPS3 Click.
        /// </summary>
        public enum RestartType
        {
            /// <summary>
            /// Commands the GPS3 Click to perform a Hot Start and use all available data in non-volatile storage upon restart.
            /// Normally hot start means the GPS module was powered down less than 3 hours (RTC must be alive) and its ephemeris
            /// is still valid. As there is no need for downloading ephemeris, it’s the fastest startup method.
            /// Assumes that the external VBat pads are connected to a proper battery source.
            /// </summary>
            Hot = 0,
            /// <summary>
            /// Commands the GPS3 click to perform a Warm Start.
            /// Warm start means the GPS module has  approximate information of time, position and
            /// coarse data on satellite positions. But it needs to download  ephemeris until it can get a fix.
            /// Using this message will force the GPS warm restarted without using the ephemeris data in non-volatile storage.
            /// </summary>
            Warm = 1,
            /// <summary>
            /// Commands the GPS3 Click to perform a Cold Start.
            /// A Cold Start will force the GPS to restart without using any prior location information, including time, position, almanacs and ephemeris data.
            /// </summary>
            Cold = 2,
            /// <summary>
            /// Commands the GPS3 Click to perform a Full Cold Start.
            /// A Full Cold Start is essentially a Cold restart, but additionally clears system and user configurations at re-start.
            /// That is, reset the GPS3 Click to the factory status. Full cold start means the GPS3 Click has no information on last location.
            /// It needs to search the full time and frequency space, and also all possible satellite numbers before it can get a fix.
            /// </summary>
            FullCold = 3
        }

        /// <summary>
        ///     The current state of the External Antenna circuitry.
        /// </summary>
        public enum AntennaStatus
        {
            /// <summary>
            ///     External active antenna is connected and the module will use external active antenna.
            /// </summary>
            Ok,

            /// <summary>
            ///     External antenna is in open-circuit state (not connected) and the internal antenna is used at this time.
            /// </summary>
            Open,

            /// <summary>
            ///     External Antenna is detected and short circuited. The internal antenna is being used.
            /// </summary>
            Shorted
        }

        /// <summary>
        ///     Supported Baud Rates of the GPS3 Click
        /// </summary>
        public enum BaudRates
        {
            /// <summary>
            ///     BaudRate 4800
            /// </summary>
            Baud_4800 = 4800,

            /// <summary>
            ///     BaudRate 9600. Default upon Reset or initial Power On.
            /// </summary>
            Baud_9600 = 9600,

            // ToD0 - Need to check out 14400 NetMF does not support 14400 Baud? 
            /// <summary>
            ///     BaudRate 14,400
            /// </summary>
            Baud_14400 = 14400,

            /// <summary>
            ///     BaudRate 19200
            /// </summary>
            Baud_19200 = 19200,

            /// <summary>
            ///     Default BaudRate 38400
            /// </summary>
            Baud_38400 = 38400,

            /// <summary>
            ///     BaudRate 57600
            /// </summary>
            Baud_57600 = 57600,

            /// <summary>
            ///     BaudRate 115200
            /// </summary>
            Baud_115200 = 115200,
        }

        /// <summary>
        ///     Position Fix acquisition type.
        /// </summary>
        public enum PositioningModes
        {
            /// <summary>
            ///     No Fix
            /// </summary>
            NoFix,

            /// <summary>
            ///     AutonomousFix GNSS fix
            /// </summary>
            AutonomousFix,

            /// <summary>
            ///     DifferentialFix GNSS fix
            /// </summary>
            DifferentialFix
        }

        /// <summary>
        /// The GPS Navigation Type, either Global Navigation Satellite System (GNSS) based or Differential Global Positioning System (DGPS) based. 
        /// </summary>
        public enum FixType
        {
            /// <summary>
            /// Invalid Status returned.
            /// </summary>
            Invalid = 0,
            /// <summary>
            /// Global Navigation Satellite System (Autonomous)
            /// </summary>
            GNSSFix = 1,
            /// <summary>
            /// Differential Global Positioning System (Differential)
            /// </summary>
            DGPSFix = 2,
        }

        /// <summary>
        ///     GNSS Fix Mode Manual (forced 3D) or Auto 2D/3D.
        /// </summary>
        public enum GNSSFixMode
        {
            /// <summary>
            ///     Manual, forced to switch to 3D mode
            /// </summary>
            Manual,

            /// <summary>
            ///     Allowed to automatically switch between 2D/3D modes
            /// </summary>
            Auto,
        }

        /// <summary>
        ///     GPS GNSS Fix Status
        /// </summary>
        public enum GNSSFixStatus
        {
            /// <summary>
            ///     No fix
            /// </summary>
            NoFix,

            /// <summary>
            ///     2D fix
            /// </summary>
            Fix2D,

            /// <summary>
            ///     3D fix
            /// </summary>
            Fix3D
        }

        /// <summary>
        /// Power Savings Modes applicable for the GPS3 Click
        /// </summary>
        public enum PeriodicMode
        {
            /// <summary>
            /// Normal Mode  - Full On Mode with no Power Savings.
            /// </summary>
            Normal = 0, 
            /// <summary>
            /// Periodically cycles between Full On Mode and Sleep Mode while backing up the data to non-volatile ram.  
            /// </summary>
            Backup = 1,
            /// <summary>
            /// Periodically cycles between Full On Mode and Sleep Mode while data is not backed up to non-volatile ram.  
            /// </summary>
            Standby = 2,
            /// <summary>
            /// Sets the GPS3 Click to perpetual backup mode. All data is written to non-volatile ram.  
            /// </summary>
            Perpetual = 4,
        }

        /// <summary>
        /// Power Savings Modes applicable for the GPS3 Click
        /// </summary>
        public enum AlwaysLocateMode
        {
            /// <summary>
            /// Normal Mode  - Full On Mode with no Power Savings.
            /// </summary>
            Normal = 0,
            /// <summary>
            /// Sets the GPS3 click to automatically switch between full on mode and AlwaysLocate™ standby mode.
            /// </summary>
            Standby = 8,
            /// <summary>
            /// Sets the GPS3 click to automatically switch between full on mode and AlwaysLocate™ backup mode.
            /// </summary>
            Backup = 9
        }

        #endregion

        #region CTOR

        /// <summary>
        ///     Initializes a new instance of the <see cref="GPS3Click" /> class.
        /// </summary>
        /// <param name="socket">The socket on which the GPS3Click module is plugged on MikroBus.Net board</param>
        /// <param name="baudRate">The BaudRate that the GPS is set to and not your desired BaudRate.</param>
        /// <remarks>This constructor will instantiate the GPS3 Click with no NMEA Sentence Subscriptions. You will have to select what NMEA Sentences that you want to subscribe to with the <see cref="SetNMEASubscriptions(NMEASubscriptions)"/> method.</remarks>
        public GPS3Click(Hardware.Socket socket, BaudRates baudRate = BaudRates.Baud_9600) 
        {
            try
            {
                Hardware.CheckPins(socket, socket.Rx, socket.Tx, socket.Rst);

                _baudRate = baudRate;

                _resetPin = new OutputPort(socket.Rst, true); // Reset circuit is active low
               
                _serial = new SimpleSerial(socket.ComPort, (int)baudRate) { ReadTimeout = 0 };
                
                if (!_serial.IsOpen) _serial.Open();
                _serial.Flush();
                _serial.DiscardInBuffer();
                _serial.DiscardOutBuffer();
                _serial.DataReceived += _serial_DataReceived;
                _serial.ErrorReceived += _serial_ErrorReceived;

                InternalReset();

                _serial.Write("$PMTK000*32" + EOL + EOL); // Can't use SendCommand here as the device has not been initialized yet. Note - If you are going to use the Write method, you need to send two EOLs.
               
                // Check if device is connected and communicating. 
                _autoEvent.Reset();
                _autoEvent.WaitOne(_autoResetWaitTime, true);
                if (!_deviceReady) throw new DeviceInitialisationException("Error initializing the GPS3 with " + _baudRate + " , try another BaudRate setting.");

                // Turn Off NMEA Subscriptions - User will have to subscribe to those wanted. Default upon Full Cold Start and initial Power On is all sentences on which may not be desired.
                SendCommand(CMD_NMEA_SENTENCES_OFF);
                
                // Populate the member info variables
                if (!InitializeMembers()) throw new DeviceInitialisationException("Error retrieving default parameters of the GPS3.");

                lastPositionFixTime = TimeSpan.MinValue;

                new Thread(ProcessMessageQueue).Start();
            }
            //Catch only the PinInUse exception, so that program will halt on other exceptions send it directly to the caller
            catch (PinInUseException ex)
            {
                throw new PinInUseException(ex.Message);
            }
        }
        
        #endregion

        #region Properties

        /// <summary>
        ///     Gets the Baud Rate of the UART Interface of the GPS3 Click.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("GPS3 Click Baud Rate is " + gps.BaudRate);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("GPS3 Click Baud Rate is " <![CDATA[&]]> gps.BaudRate.ToString())
        /// </code>
        /// </example>
        public BaudRates BaudRate
        {
            get { return _baudRate; }
        }

        /// <summary>
        /// Gets the last RMC Data received from the GPS3 Click.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Last RMC Data");
        /// Debug.Print(gps.LastRMCData.ToString());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Last RMC Data")
        /// Debug.Print(gps.LastRMCData.ToString())
        /// </code>
        /// </example>
        public RMCData LastRMCData
        {
            get { return _lastRmcData; }
        }

        /// <summary>
        /// Gets the last VTG Data received from the GPS3 Click.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Last VTGData");
        /// Debug.Print(gps.LastVTGData.ToString());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Last VTG Data");
        /// Debug.Print(gps.LastVTGData.ToString());
        /// </code>
        /// </example>
        public VTGData LastVTGData
        {
            get { return _lastVTGData; }
        }

        /// <summary>
        /// Gets the last GGA Data received from the GPS3 Click.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Last GGAData");
        /// Debug.Print(gps.LastGGAData.ToString());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Last GGA Data");
        /// Debug.Print(gps.LastGGAData.ToString());
        /// </code>
        /// </example>
        public GGAData LastGGAData
        {
            get { return _lastGgaData; }
        }

        /// <summary>
        /// Gets the last GSA Data received from the GPS3 Click.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Last GSAData");
        /// Debug.Print(gps.LastGSAData.ToString());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Last GSA Data");
        /// Debug.Print(gps.LastGSAData.ToString());
        /// </code>
        /// </example>
        public GSAData LastGSAData
        {
            get { return _lastGsaData; }
        }

        /// <summary>
        /// Gets the last GSV Data received from the GPS3 Click.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Last GSVData");
        /// Debug.Print(gps.LastGSVData.ToString());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Last VTG Data");
        /// Debug.Print(gps.LastVTGData.ToString());
        /// </code>
        /// </example>
        public GSVData LastGSVData
        {
            get { return _lastGsvData; }
        }

        /// <summary>
        /// Gets the last GLL Data received from the GPS3 Click.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Last GLLData");
        /// Debug.Print(gps.LastGLLGData.ToString());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Last GLL Data");
        /// Debug.Print(gps.LastGLLData.ToString());
        /// </code>
        /// </example>
        public GLLData LastGLLData
        {
            get { return _lastGllData; }
        }
        
        /// <summary>
        ///     Gets the elapsed time since the last RMC position was received.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Tim since last valid RMC fix - " + gps.LastValidFixAge);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Tim since last valid RMC fix - " <![CDATA[&]]> gps.LastValidFixAge)
        /// </code>
        /// </example>
        public TimeSpan LastValidFixAge
        {
            get
            {
                if (lastPositionFixTime == TimeSpan.MinValue) return TimeSpan.MaxValue;
                return Utility.GetMachineTime() - lastPositionFixTime;
            }
        }

        /// <summary>
        ///     Gets the underlying Serial Port Instance of the driver of which you can manipulate based upon your needs.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// // Put the GPS3 Click in Standby Mode.
        /// var commandString = "$PMTK161,0*";
        /// commandString += gps.CalculateChecksum(commandString + "\r\n" + "\r\n");
        /// var commandBytes = Encoding.UTF8.GetBytes(commandString);
        /// gps.Serial.Write(commandBytes, 0, commandBytes.Length);
        /// </code>
        /// <code language = "VB">
        /// ' Put the GPS3 Click in Standby Mode.
        /// Dim commandString As String = "$PMTK161,0*"
        /// commandString += gps.CalculateChecksum(commandString <![CDATA[&]]> Microsoft.VisualBasic.Constants.vbCrLf <![CDATA[&]]> Microsoft.VisualBasic.Constants.vbCrLf)
        /// Dim commandBytes As Byte() = Encoding.UTF8.GetBytes(commandString)
        /// gps.Serial.Write(commandBytes, 0, commandBytes.Length)
        /// </code>
        /// </example>
        public SerialPort Serial
        {
            get { return _serial; }
        }

        /// <summary>
        ///     Gets the <see cref="AntennaStatus" /> of the External Antenna connection.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// var status = gps.Antenna;
        /// Debug.Print("Antenna is " + (status == GPS3Click.AntennaStatus.Ok ? "Open" : status == GPS3Click.AntennaStatus.Ok ? "Ok" : "Shorted"));
        /// </code>
        /// <code language = "VB">
        /// Dim status As GPS3Click.AntennaStatus = gps.Antenna
        /// Debug.Print("Antenna is " <![CDATA[&]]> If(status = GPS3Click.AntennaStatus.Ok, "Open", If(status = GPS3Click.AntennaStatus.Ok, "Ok", "Shorted")))
        /// </code>
        /// </example>
        public AntennaStatus Antenna
        {
            get { return _antenna; }
        }

        /// <summary>
        ///     Gets the <see cref="NMEASubscriptions" />  that are currently subscribed to.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("NMEASubscription Subscription Level - " + gps.Subscriptions.ToString());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("NMEASubscription Subscription Level - " <![CDATA[&]]> gps.Subscriptions.ToString())
        /// </code>
        /// </example>
        public NMEASubscriptions Subscriptions
        {
            get { return _subscriptions; }
        }

        /// <summary>
        ///     Gets a <see cref="System.String" /> containing the version information of the L80 IC on the GPS3 Click.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("GPS Firmware Information - " + gps.GPSFirmwareInfo);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("GPS Firmware Information - " <![CDATA[&]]> gps.GPSFirmwareInfo.ToString())
        /// </code>
        /// </example>
        public String GPSFirmwareInfo
        {
            get { return _gpsFirmwareInfo; }
        }

        /// <summary>
        ///     Gets the Position Fix Control Rate of the digital interface of the GPS3 click.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("GPS Fix Position Control Rate is " + gps.PositionFixControlRate);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("GPS Fix Position Control Rate is " <![CDATA[&]]> gps.PositionFixControlRate.ToString())
        /// </code>
        /// </example>
        public UInt16 PositionFixControlRate
        {
            get { return _positionFixControlRate; }
        }

        /// <summary>
        /// Gets the current Position Fix Interval.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("GPS Fix Position Interval " + gps.PositionFixInterval);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("GPS Fix Position Interval " <![CDATA[`&]]> gps.PositionFixInterval)
        /// </code>
        /// </example>
        public UInt16 PositionFixInterval
        {
            get { return _positionFixInterval; }
        }

        /// <summary>
        /// Gets the current status of Easy™ (Embedded Assist System)
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Is Easy enabled? " + gps.EasyEnabled);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Is Easy enabled? " <![CDATA[&]]> gps.EasyEnabled.ToString())
        /// </code>
        /// </example>
        public Boolean EasyEnabled
        {
            get { return _easyEnabled; }
        }

        /// <summary>
        /// Gets the current status of DPGS (Differential Global Positioning System)
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Is DPGS enabled? " + gps.DPGSEnabled);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Is DPGSEnabled enabled? " <![CDATA[&]]> gps.DPGSEnabled.ToString())
        /// </code>
        /// </example>
        public Boolean DPGSEnabled
        {
            get { return _dpgsEnabled; }
        }

        /// <summary>
        /// Gets the current status of AIC (Active Interference Cancellation)
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Is AIC enabled? " + gps.AICEnabled);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Is AIC enabled? " <![CDATA[&]]> gps.AICEnabled.ToString())
        /// </code>
        /// </example>
        public Boolean AICEnabled
        {
            get { return _aicEnabled; }
        }

        /// <summary>
        /// Gets the current status of SBAS (Satellite-Based Augmentation System)
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Is SBAS enabled? " + gps.SBASEnabled);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Is SBAS enabled? " <![CDATA[&]]> gps.SBASEnabled.ToString())
        /// </code>
        /// </example>
        public Boolean SBASEnabled
        {
            get { return _sbasEnabled; }
        }

        /// <summary>
        /// Gets the current setting of the Status Navigation Threshold.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Static Navigation Threshold is " + gps.StaticNavigationThreshold);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Static Navigation Threshold is " <![CDATA[&]]> gps.StaticNavigationThreshold.ToString())
        /// </code>
        /// </example>
        public Double StaticNavigationThreshold
        {
            get { return _staticNavigationThreshold; }
        }

        /// <summary>
        /// Gets the current setting of the HDOP (Horizontal Dilution of Precision)
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("HDOP Threshold is " + gps.HDOPThreshold);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("HDOP Threshold is " <![CDATA[&]]> gps.HDOPThreshold.ToString())
        /// </code>
        /// </example>
        public Double HDOPThreshold
        {
            get { return _hdopThreshold; }
        }

        /// <summary>
        /// Gets the current status of LSC (Leap Second Change) output enabled.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Is LSC output enabled? " + gps.LSCEnabled);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Is LSC output enabled? " <![CDATA[&]]> gps.LSCEnabled.ToString())
        /// </code>
        /// </example>
        public Boolean LSCEnabled
        {
            get { return _pmtkSLCEnabled; }
        }

        /// <summary>
        ///     Gets or sets the power mode.
        /// </summary>
        /// <example>
        ///     None provided as this module does not support changing PowerModes directly. 
        /// </example>
        /// <exception cref="NotImplementedException">A NotImplementedException will be thrown if attempting to change the PowerMode of the GPS3 Click as the module doesn't support power modes.</exception>
        /// <remarks>This driver does not support changing the Power Mode by use of this property. Use either the <see cref="EnableStandbyMode"/> or <see cref="SetPeriodicMode"/> methods for managing Power to the GPS3 Click.</remarks>
        /// <example>Example Usage: None provided as this module does not support Power modes.
        /// </example>
        public PowerModes PowerMode
        {
            get { return PowerModes.On; }
            set
            {
                throw new NotImplementedException("Changing PowerMode is not supported, use either the EnableStandbyMode() or SetPeriodicMode methods for reduced power consumption.");
            }
        }

        /// <summary>
        ///     Gets the driver version.
        /// </summary>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.Print ("Current driver version : "+ _gps.DriverVersion);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Current driver version : " <![CDATA[&]]> _gps.DriverVersion.ToString())
        /// </code>
        /// </example>
        public Version DriverVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets the module
        /// </summary>
        /// <param name="resetMode">The <see cref="ResetModes"/>to use to reset the module </param>
        /// <exception cref="NotImplementedException">A NotImplementedException will be thrown if attempting to REset the GPS3 Click as the module doesn't support power modes.</exception>
        /// <example>Example Usage: None provided as this module does not support Reset methods. Use the <see cref="PerformRestart"/> as an alternative.
        /// </example>
        public Boolean Reset(ResetModes resetMode)
        {
            throw new NotSupportedException("Reset functionality is not supported by the GPS3 Click. To reset the GPS3 Click, use PerformHotStart, PreformWarmStart, PerformColdStart or PerformFullColdStart(FactoryReset) methods.");
        }

        /// <summary>
        ///     Sends a properly formatted NMEA Sentence (command) to the GPS3 Click appending the calculated checksum and the
        ///     EndOfLine marker "CRLF"
        /// </summary>
        /// <param name="command">
        ///     Then NMEA Command string to send, expected format is
        ///     "$PMTK314,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0"
        /// </param>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// //Send command to put GPS3 Click in Standby Low Power Mode.
        /// gps.SendCommand("$PMTK161,0");
        /// </code>
        /// <code language = "VB">
        /// 'Send command to put GPS3 Click in Standby Low Power Mode.
        /// gps.SendCommand("$PMTK161,0")
        /// </code>
        /// </example>
        public Boolean SendCommand(String command)
        {
            if (!_deviceReady) return false;
            _serial.WriteLine(command + "*" + CalculateChecksum(command) + EOL);
            return true;
        }

        /// <summary>
        ///     Validates NMEA string to verify if the checksum is correct, format of NEMA string is expected to be
        ///     "$PMTK001,869,3*37"
        /// </summary>
        /// <param name="nemaString">The NMEA string to verify.</param>
        /// <returns>Return true if the checksum is valid, or otherwise false.</returns>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("ChecksumValid? " + gps.IsValidChecksum("$PMTK001,869,3*37"));
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("ChecksumValid? " <![CDATA[&]]> gps.IsValidChecksum("$PMTK001,869,3*37"))
        /// </code>
        /// </example>
        [DebuggerStepThrough]
        public Boolean IsValidChecksum(String nemaString)
        {
            if (nemaString.IndexOf('*') <= -1) return false;
            nemaString = Replace(nemaString, " ", String.Empty);

            String valid_checksum = nemaString.Substring(nemaString.IndexOf('*') + 1, 2);
            nemaString = nemaString.Substring(1, nemaString.IndexOf('*') - 1);

            Int32 checksum = 0;

            for (Int32 i = 0; i < nemaString.Length; i++)
            {
                checksum ^= nemaString[i];
            }
            var returnValue = false;
            try
            {
                returnValue = checksum == Convert.ToInt32(valid_checksum, 16);
            }
            catch (ArgumentException)
            {
               // Sanity check
            }
            return returnValue;
        }

        /// <summary>
        ///     Calculates the XOR Checksum of a NEMA String, format of NEMA String is expected to be "$PMTK001,869,3*37"
        /// </summary>
        /// <param name="nemaString">The NMEA command string to calculate the checksum for.</param>
        /// <returns>Returns a <see cref="System.String" /> of the checksum.</returns>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// // Put the GPS3 Click in Standby Mode.
        /// var commandString = "$PMTK161,0*";
        /// commandString += gps.CalculateChecksum(commandString + "\r\n" + "\r\n");
        /// var commandBytes = Encoding.UTF8.GetBytes(commandString);
        /// gps.Serial.Write(commandBytes, 0, commandBytes.Length);
        /// </code>
        /// <code language = "VB">
        /// ' Put the GPS3 Click in Standby Mode.
        /// Dim commandString As String = "$PMTK161,0*"
        /// commandString += gps.CalculateChecksum(commandString + Microsoft.VisualBasic.Constants.vbCrLf + Microsoft.VisualBasic.Constants.vbCrLf)
        /// Dim commandBytes As Byte() = Encoding.UTF8.GetBytes(commandString)
        /// gps.Serial.Write(commandBytes, 0, commandBytes.Length)
        /// </code>
        /// </example>
        [DebuggerStepThrough]
        public String CalculateChecksum(String nemaString)
        {
            nemaString = Replace(nemaString, " ", String.Empty);
            nemaString = nemaString.Substring(nemaString.IndexOf('$') + 1);

            Int32 check = 0;
            for (Int32 c = 0; c < nemaString.Length; c++)
            {
                check ^= nemaString[c];
            }
            return check.ToString("X2");
        }

        /// <summary>
        /// Sets the GPS3 Click UART Interface BaudRate.
        /// </summary>
        /// <param name="baudRate">The new <see cref="BaudRates"/> to use for the GPS3 Click UART interface.</param>
        /// <remarks><para><b>
        /// <p/>If you elect to change the BaudRate of the GPS3 Click UART Interface, you will need to provide a mechanism to pass the correct BaudRate to the GPS3 Click Constructor upon a soft reset of the NetMF Device.
        /// <p/>The GPS3 click will retain the current BaudRate setting while under continuous power and through Hot Starts, Warm Starts and Cold Starts.
        /// <p/>A Full Cold Restart will set all User Settings to the Factory Default including the BaudRate to 9600 Baud.
        /// <p/>It is suggested that if you change the BaudRate programatically that you save the new BaudRate setting to non-volatile storage. The initialization routine of your program would then be to read the setting
        /// and pass this value to the constructor for the GPS3 click.
        /// <p/>Better yet is to not change the BaudRate as the GPS3 Click can successfully handle all communication at 10 Hertz while subscribing to all NMEA output at one sentence per fix. 
        /// </b></para></remarks>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Set BaudRate to 115200? " + gps.SetBaudRate(GPS3Click.BaudRates.Baud_115200)); 
        /// Debug.Print("New Baud Rate? " + gps.BaudRate);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Set BaudRate to 115200? " <![CDATA[&]]> gps.SetBaudRate(GPS3Click.BaudRates.Baud_115200))
        /// Debug.Print("New Baud Rate? " <![CDATA[&]]> gps.BaudRate)
        /// </code>
        /// </example>
        public Boolean SetBaudRate(BaudRates baudRate)
        {
            _serial.DataReceived -= _serial_DataReceived;
            _serial.ErrorReceived -= _serial_ErrorReceived;

            var subscriptions = Subscriptions;
            SendCommand(CMD_NMEA_SENTENCES_OFF);

            changingBuadRate = true;
            SendCommand(CMD_SET_BAUDRATE + baudRate);
            _serial.Close();
            Thread.Sleep(_waitForResponse);
            _serial.BaudRate = (Int32)baudRate;
            _serial.Open();
            Thread.Sleep(_waitForResponse);
            _serial.DataReceived += _serial_DataReceived;
            _serial.ErrorReceived += _serial_ErrorReceived;
            changingBuadRate = false;
            _deviceReady = false;
           
            //_serial.Write("$PMTK000*32" + EOL + EOL);
            var cmd = Encoding.UTF8.GetBytes("$PMTK000*32" + EOL + EOL);
            _serial.Write(cmd, 0, cmd.Length);

            _autoEvent.Reset();
            _autoEvent.WaitOne(_autoResetWaitTime, true);
            if (!_deviceReady) return false;
            _baudRate = baudRate;
            SetNMEASubscriptions(subscriptions);
            return true;
        }

        /// <summary>
        /// The GPS3 Click has the ability to output the following eight sentences: RMC, VTG, GGA, GSA, GSV, GLL, ZDA and CHN.
        /// Additionally TXT which is used to output information about the state of the External Antenna.. This cannot be subscribed to or tuened off.
        /// </summary>
        /// <param name="nmeaSubscriptions">The <see cref="NMEASubscriptions"/> that you want to subscribe to.</param>
        /// <returns>True is successful in subscription or otherwise false.</returns>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// var nmeaSuscription = new GPS3Click.NMEASubscriptions { RMCEnabled = true, RMCFixesPerSentence = 10, GSVEnabled = true, GSVFixesPerSentence = 10, GSAEnabled = true, GSAFixesPerSentence = 10, VTGEnabled = true, VTGFixesPerSentence = 10, GLLEnabled = true, GLLFixesPerSentence = 10, GGAEnabled = true, GGAFixesPerSentence = 10, CHNEnabled = false, CHNFixesPerSentence = 0, ZDAEnabled = false, ZDAFixesPerSentence = 0 };
        /// Debug.Print("NMEA Set? " + gps.SetNMEASubscriptions(nmeaSuscription));
        /// Debug.Print("NMEA Subscriptions? " + gps.QueryNMEASubscriptions());
        /// </code>
        /// <code language = "VB">
        /// Dim nmeaSuscription As GPS3Click.NMEASubscriptions = New GPS3Click.NMEASubscriptions(True, 10, True, 10, True, 10, True, 10, True, 10, True, 10, True, 10, True, 10)
        /// Debug.Print("NMEA Set? " <![CDATA[&]]> gps.SetNMEASubscriptions(nmeaSuscription))
        /// Debug.Print("NMEA Subscriptions? " <![CDATA[&]]> gps.QueryNMEASubscriptions().ToString)
        /// </code>
        /// </example>
        public Boolean SetNMEASubscriptions(NMEASubscriptions nmeaSubscriptions)
        {
            if (!WaitForResponse(CMD_SET_NEMA_SENTENCES, nmeaSubscriptions.ToString().Substring(9))) return false;
            _subscriptions = nmeaSubscriptions;
            return true;
        }

        /// <summary>
        /// Queries the GPS3 Click to report current subscription levels. 
        /// </summary>
        /// <returns>The current <see cref="NMEASubscriptions"/> that are currently active.</returns>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("NMEA Subscriptions? " + gps.QueryNMEASubscriptions());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("NMEA Subscriptions? " <![CDATA[&]]> gps.QueryNMEASubscriptions().ToString)
        /// </code>
        /// </example>
        public NMEASubscriptions QueryNMEASubscriptions()
        {
            return WaitForResponse(CMD_QUERY_NMEA_SENTENCES) ? NMEASubscriptions.FromNMEARespoonse(_nmeaOutputString) : new NMEASubscriptions();
        }

        /// <summary>
        /// Used to control the rate of position fixing activity (0.1, 1, 5 or 10Hz) of the GPS3 Click.
        /// <para>
        /// <list type="Bullet">
        /// <listitem>
        /// 01 Hz = 10000
        /// </listitem>
        /// <listitem>
        /// 1 Hz = 1000
        /// </listitem>
        /// <listitem>
        /// 5 Hz = 200
        /// </listitem>
        /// <listitem>
        /// 10Hz = 100
        /// </listitem>
        /// </list>
        /// Or any value between 100 and 10,000.
        /// </para>
        /// <param name="positionFixControlRate">The rate to set.</param>
        /// </summary>
        /// <remarks>
        /// Position fix control rate cannot be set higher than 5 Hertz if SBAS is enabled or SBAS will be automatically disabled.
        /// If you want to set the Position Fix control Rate to higher than 5 Hertz, you must first disable SBAS.
        /// </remarks>
        /// <returns>True if successful in changing the Position Fix Control or otherwise false.</returns>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Set PositionFix Control to 1000 returns " + gps.SetPositionFixControlRate(1000));
        /// Debug.Print("Position Fix Control Rate is now " + gps.QueryPositionFixControlRate());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Set PositionFix Control to 1000 returns " <![CDATA[&]]> gps.SetPositionFixControlRate(1000))
        /// Debug.Print("Position Fix Control Rate is now " <![CDATA[&]]> gps.QueryPositionFixControlRate())
        /// </code>
        /// </example>
        public Boolean SetPositionFixControlRate(UInt16 positionFixControlRate)
        {
            if (positionFixControlRate < 100) positionFixControlRate = 100;
            if (positionFixControlRate > 10000) positionFixControlRate = 10000;
            if ((positionFixControlRate < 200) && _sbasEnabled) return false;
            if (!WaitForResponse(CMD_SET_POSITION_FIX_CONTROL, positionFixControlRate + ",0,0,0,0")) return false;
            _positionFixControlRate = positionFixControlRate;
            return true;
        }
       
        /// <summary>
        /// Queries the GPS3 Click to report the current Position Fix Control Rate. 
        /// </summary>
        /// <returns>The current Position Fix Control Rate of the GPS3 Click.</returns>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Set PositionFix Control to 1000 returns " + gps.SetPositionFixControlRate(1000));
        /// Debug.Print("Position Fix Control Rate is now " + gps.QueryPositionFixControlRate());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Set PositionFix Control to 1000 returns " <![CDATA[&]]> gps.SetPositionFixControlRate(1000))
        /// Debug.Print("Position Fix Control Rate is now " <![CDATA[&]]> gps.QueryPositionFixControlRate())
        /// </code>
        /// </example>
        public UInt16 QueryPositionFixControlRate()
        {
            return WaitForResponse(CMD_QUERY_POSITION_FIX_CONTROL) ? _positionFixControlRate : UInt16.MinValue;
        }

        /// <summary>
        ///  Enables or disables Multi-tone AIC (Active Interference Cancellation).
        /// </summary>
        /// <param name="enable">Set to true to enable or false to disable.</param>
        /// <returns></returns>
        /// <returns>True if successful in changing the AIC value or otherwise false.</returns>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Enable AIC ?" + gps.EnableAIC(true));
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Enable AIC ?" <![CDATA[&]]> gps.EnableAIC(True))
        /// </code>
        /// </example>
        public Boolean EnableAIC(Boolean enable)
        {
            if (_aicEnabled && enable) return true;
            if (!WaitForResponse(CMD_SET_AIC, (enable ? "1" : "0"))) return false;
            _aicEnabled = enable;
            return true;
        }

        /// <summary>
        /// Enables or disables EASY™ setting.
        /// EASY™ technology works as embedded software which can accelerate TTFF (Time To First Fix) by predicting satellite navigation messages from received ephemeris.
        /// </summary>
        /// <param name="enable">Set to true to enable or false to disable.</param>
        /// <returns>True if successful in changing the EASY™ setting or otherwise false.</returns>
        /// <remarks>EASY™ technology works as embedded software which can accelerate TTFF (Time To First Fix) by predicting satellite navigation messages from received ephemeris. EASY™ is enabled by default.</remarks>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("EASY enabled ? " + gps.QueryEASYStatus());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("EASY enabled ? " <![CDATA[&]]> gps.QueryEASYStatus())
        /// </code>
        /// </example>
        public Boolean EnableEASY(Boolean enable)
        {
            if (!WaitForResponse(CMD_SET_EASY, (enable ? "1" : "0"))) return false;
            _easyEnabled = enable;
            return true;
        }

        /// <summary>
        /// Queries the GPS3 Click for the status of the EASY™ setting.
        /// </summary>
        /// <returns>True if EASY™ is enabled or otherwise false.</returns>
        /// <example>Example Usage:
        /// <code language = "C#"> 
        /// Debug.Print("EASY enabled ? " + gps.QueryEASYStatus());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("EASY enabled ? " <![CDATA[&]]> gps.QueryEASYStatus())
        /// </code>
        /// </example>
        public Boolean QueryEASYStatus()
        {
            return WaitForResponse(CMD_QUERY_EASY_STATUS) && _easyEnabled;
        }

        /// <summary>
        /// Enables or disables The GPS3 Click to use SBAS (Satellite Based  Augmentation Systems).
        /// <para>SBAS is a system that supports wide-area or regional augmentation through
        /// geostationary satellite broadcast messages. The geostationary satellite broadcast GPS integrity and
        /// correction data with the assistance of multiple ground stations which are located at accurately-surveyed points.</para>
        /// </summary>
        /// <param name="enable">Set to true to enable or false to disable.</param>
        /// <returns>True if successful in changing the SBAS setting or otherwise false.</returns>
        /// <remarks>The underlying MediaTek MT3329 chip set of the L80 IC on the GPS3 click does NOT support SBAS mode for fix-rates higher than 5 Hertz.
        /// For fix-rates equal to from 6 to 10 Hz,  the GPS3 Click receiver will not enter SBAS mode to avoid data loss due to long calculation times.</remarks>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Enable SBAS ?" + gps.EnableSBAS(true));
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Enable SBAS ?" + gps.EnableSBAS(True))
        /// </code>
        /// </example>
        public Boolean EnableSBAS(Boolean enable)
        {
            if (_positionFixControlRate < 200) return false;
            if (!WaitForResponse(CMD_SET_SBAS, (enable ? "1" : "0"))) return false;
            _sbasEnabled = enable;
            return true;
        }

        /// <summary>
        /// Queries the GPS3 Click for the status of the SBAS setting.
        /// </summary>
        /// <returns>True if SBAS is enabled or otherwise false.</returns>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("EASY enabled ? " + gps.QuerySBASStatus());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("EASY enabled ? " + gps.QuerySBASStatus());
        /// </code>
        /// </example>
        public Boolean QuerySBASStatus()
        {
            return WaitForResponse(CMD_QUERY_SBAS) && _sbasEnabled;
        }
        
        /// <summary>
        /// Sets the speed threshold for static navigation.
        /// Speeds below the threshold, will lock the Course-Over-Ground (COG) to the last reading and output speed will be zero.
        /// Set the threshold value to 0 to disable this function.
        /// </summary>
        /// <param name="threshold">The Speed Threshold. Valid values are between 0.0 and 2.0 (meters/second).</param>
        /// <returns>True is successfully setting the Navigation threshold or otherwise false.</returns>
        /// <remarks>This will also lock the Course-Over-Ground (COG) to the last reading.</remarks>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// gps.SetStaticNavigationThreshold(1.6);
        /// Debug.Print("Static Navigatin Threshold s " + gps.QueryStaticNavigationthreshold());
        /// </code>
        /// <code language = "VB">
        /// gps.SetStaticNavigationThreshold(1.6)
        /// Debug.Print("Static Navigatin Threshold s " <![CDATA[&]]> gps.QueryStaticNavigationthreshold())
        /// </code>
        /// </example>
        public Boolean SetStaticNavigationThreshold(Double threshold)
        {
            if (threshold < 0.0F) threshold = 0.0F;
            if (threshold > 2.0F) threshold = 2.0F;
            if (!WaitForResponse(CMD_SET_STATIC_NAVIGATION_THRESHOLD, threshold.ToString("F2"))) return false;
            _staticNavigationThreshold = threshold;
            return true;
        }

        /// <summary>
        /// Queries the GPS3 Click for the Static Navigation Threshold Setting.
        /// </summary>
        /// <returns>The Static Navigation Threshold setting.</returns>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// gps.SetStaticNavigationThreshold(1.6);
        /// Debug.Print("Static Navigation Threshold s " + gps.QueryStaticNavigationthreshold());
        /// </code>
        /// <code language = "VB">
        /// gps.SetStaticNavigationThreshold(1.6)
        /// Debug.Print("Static Navigation Threshold s " <![CDATA[&]]> gps.QueryStaticNavigationthreshold())
        /// </code>
        /// </example>
        public Double QueryStaticNavigationthreshold()
        {
            return WaitForResponse(CMD_QUERY_STATIC_NAVIGATION) ? _staticNavigationThreshold : Double.NaN;
        }

        /// <summary>
        /// This method is used to set the Differential Global Positioning System (DGPS) correction data source mode.
        /// </summary>
        /// <param name="enable">Set the enabled parameter to true to enable DPGS (WAAS -Wide Area Augmentation System) or false to disable DGPS.</param>
        /// <returns>Returns true is successful in changing the DPGS Mode or otherwise false.</returns>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Enable DPGS ?" + gps.EnableDPGS(true));
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Enable DPGS ?" <![CDATA[&]]> gps.EnableDPGS(True))
        /// </code>
        /// </example>
        public Boolean EnableDPGS(Boolean enable)
        {
            if (!WaitForResponse(CMD_SET_DPGS, (enable ? "2" : "0"))) return false;
            _dpgsEnabled = enable;
            return true;
        }

        /// <summary>
        /// Queries the GPS3 Click for the status of DPGS correction data.
        /// </summary>
        /// <returns>True if DPDS is enabled or otherwise false.</returns>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("DPGS enabled ? " + gps.QueryDPGSStatus());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("DPGS enabled ? " <![CDATA[&]]> gps.QueryDPGSStatus());
        /// </code>
        /// </example>
        public Boolean QueryDPGSStatus()
        {
            return (WaitForResponse(CMD_QUERY_DPGS_MODE) && _dpgsEnabled);
        }
        
        /// <summary>
        /// Queries the GPS3 Click for the firmware information of the underlying chip set.
        /// </summary>
        /// <returns>A string representing the firmware information.</returns>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("GPS3 Click Firmware Inf? " + gps.QueryFirmwareInfo());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("GPS3 Click Firmware Inf? " <![CDATA[&]]> gps.QueryFirmwareInfo())
        /// </code>
        /// </example>
        public String QueryFirmwareInfo()
        {
            return WaitForResponse(CMD_QUERY_FIRMWARE_INFO) ? _gpsFirmwareInfo : String.Empty;
        }

        /// <summary>
        /// Sets the Position Fix Interval (actual computation period interval) in milliseconds.
        /// </summary>
        /// <param name="interval">The position fix interval in milliseconds.</param>
        /// <returns>True if successful in setting the fix or otherwise false.</returns>
        /// <remarks>The minimum interval is 200 milliseconds.</remarks>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Set POS Fix Interval? " + gps.SetPositionFixInterval(1000));
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Set POS Fix Interval? " <![CDATA[&]]> gps.SetPositionFixInterval(1000))
        /// </code>
        /// </example>
        public Boolean SetPositionFixInterval(UInt16 interval)
        {
            if (interval < 200) interval = 200;
            if (!WaitForResponse(CMD_SET_POSITION_FIX_INTERVAL, interval.ToString("F0"))) return false;
            _positionFixInterval = interval;
            return true;
        }

        /// <summary>
        /// Sets the HDOP (Horizontal Dilution OF Precision) of the GPS3 Click.
        /// </summary>
        /// <param name="threshold">The HDOP threshold, If the HDOP value is larger than this threshold value,
        ///  the position will not be fixed.</param>
        /// <returns>True if successful in setting the DPGS or otherwise false.</returns>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Set HDOP Threshold? " + gps.SetHDOPThreshold(0.5));
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Set HDOP Threshold? " <![CDATA[&]]> gps.SetHDOPThreshold(0.5))
        /// </code>
        /// </example>
        public Boolean SetHDOPThreshold(Double threshold)
        {
            if (!WaitForResponse(CMD_SET_HDOP_THRESHOLD, threshold.ToString("F2"))) return false;
            _hdopThreshold = threshold;
            return true;
        }

        /// <summary>
        /// Queries the GPS3 click for the HDOP (Horizontal Dilution OF Precision) threshold.
        /// </summary>
        /// <returns>The HDOP setting of the GPS3 Click.</returns>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("HDOP Threshold? " + gps.QueryHDOPThreshold());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("HDOP Threshold? " <![CDATA[&]]> gps.QueryHDOPThreshold())
        /// </code>
        /// </example>
        public Double QueryHDOPThreshold()
        {
            return WaitForResponse(CMD_QUERY_HDOP_THRESHOLD) ? _hdopThreshold : Double.NaN;
        }

        /// <summary>
        /// Sets the GPPS3 Click to output LSC (Leap Second Change). 
        /// </summary>
        /// <param name="enable">Set the enable parameter to true to output LSC data or false to disable LSC output.</param>
        /// <returns>Returns true is successful in changing LSC output or otherwise false.</returns>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Enable Leap Second Change Messages ?" + gps.EnableLSCSentences(true));
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Enable Leap Second Change Messages ?" <![CDATA[&]]> gps.EnableLSCSentences(True))
        /// </code>
        /// </example>
        public Boolean EnableLSCSentences(Boolean enable)
        {
            if (!WaitForResponse(CMD_SET_PMTKLSC, (enable ? "1" : "0"))) return false;
            _pmtkSLCEnabled = enable;
            return true;
        }

        /// <summary>
        /// Gets the current status of LSC (Leap Second Change) output enabled.
        /// </summary>
        /// <returns>True if DPDS is enabled or otherwise false.</returns>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Leap Second Change Messages enabled ? " + gps.QueryLSCEnabled());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Leap Second Change Messages enabled ? " <![CDATA[&]]> gps.QueryLSCEnabled());
        /// </code>
        /// </example>
        public Boolean QueryLSCEnabled()
        {
            return WaitForResponse(CMD_QUERY_PMTKLSC) && _pmtkSLCEnabled;
        }

        /// <summary>
        /// Sets the GPS3 Click AlwaysLocateTM mode Backup or Standby.
        /// <para>AlwaysLocateTM standby mode supports the module to switch automatically between full on mode and standby mode.</para>
        /// <para>AlwaysLocateTM backup mode is similar to AlwaysLocateTM standby mode. The difference is that AlwaysLocateTM backup mode can switch between full on mode and backup mode automatically.</para>
        /// </summary>
        /// <param name="mode">The <see cref="AlwaysLocateMode"/></param>
        /// <example>Example usage: 
        /// <list type="definition">
        /// <item>
        /// <term> AlwaysLocate™ Standby</term>
        /// <description>SetAlwaysLocateMode(GPS3Click.AlwaysLocateMode.Standby);</description>
        /// </item>
        /// <item>
        /// <term>AlwaysLocate™ Backup</term>
        /// <description>SetPeriodicMode(GPS3Click.AlwaysLocateMode.Backup);</description>
        /// </item>
        /// <item>
        /// <term>Exit Periodic modes</term>
        /// <description>SetPeriodicMode(GPS3Click.AlwaysLocateMode.Normal);</description>
        /// </item>
        /// </list>
        /// <code language = "C#">
        /// Debug.Print("Set Always Locate Mode to Standby Mode? " + gps.SetAlwaysLocateMode(GPS3Click.AlwaysLocateMode.Standby));
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Set Always Locate Mode to Standby Mode? " <![CDATA[&]]> gps.SetAlwaysLocateMode(GPS3Click.AlwaysLocateMode.Standby))
        /// </code>
        /// </example>
        public Boolean SetAlwaysLocateMode(AlwaysLocateMode mode)
        {
            CancelPeriodicMode(); // This is an ugly hack to work around the GPS3 not always responding with the same Ack Packets all of the time.
            return WaitForResponse(CMD_SET_PERIODIC_MODE, mode.ToString());
        }

        /// <summary>
        /// Sets the GPS3 Click receivers' Periodic Power Saving Mode Settings.
        /// <para>In RUN stage, the GPS receiver measures and calculates positions.</para>
        /// <para>In SLEEP stage, the GPS receiver may enter two different power saving modes. One is "Periodic Standby Mode" and another is "Periodic Backup Mode".</para>
        /// <para>Due to hardware limitation, the  maximum power down duration (SLEEP) is 2407 seconds.</para>
        /// <para>
        /// If the configured “SLEEP” interval is larger than 2047 seconds, GPS firmware will automatically extend the interval by software
        /// method. However, GPS system will be powered on for the interval extension and powered down again after the extension is done.
        /// </para>
        /// </summary>
        /// <param name="mode">Sets operation mode of power saving.</param>
        /// <param name="firstRunTime">The duration in milliseconds to fix (or attempt to fix) before switching from running mode to a minimum power sleep mode.
        ///     <para>0 to Disable</para>
        ///     <para>1000 or greater to Enable</para>
        ///     <para>Valid Range is 1000 to 518,400,000</para>
        /// </param>
        /// <param name="firstSleepTime">The interval in milliseconds to come out of a minimum power sleep mode and start running in order to get a new position fix.
        ///     <para>Valid Range is 1000 to 518,400,000</para>
        /// </param>
        /// <param name="secondRunTime">The duration in milliseconds to fix (or attempt to fix) before switching from running mode to a minimum power sleep mode.
        ///     <para>0 to Disable</para>
        ///     <para>1000 or greater to Enable</para>
        ///     <para>Valid Range is 1000 to 518400000</para>
        /// </param>
        /// <param name="secondSleepTime">The interval in milliseconds to come out of a minimum power sleep mode and start running in order to get a new position fix.
        ///     <para>Valid Range is 1000 to 518,400,000</para>
        /// </param>
        /// <remarks>The unit of run time or sleep time is milliseconds, the second run time should be larger than first run time for non-zero value.</remarks>
        /// <returns>True if successful in setting the Periodic Mode or otherwise false.</returns>
        /// <example>Example usage of how to enter Periodic Modes
        /// <list type="definition">
        /// <item>
        /// <term>Periodic Backup Mode</term>
        /// <description>SetPeriodicMode(GPS3Click.PeriodicMode.Backup,25,180000,60000);</description>
        /// </item>
        /// <item>
        /// <term>Periodic Standby Mode</term>
        /// <description>SetPeriodicMode(GPS3Click.PeriodicMode.Standby,3000,12000,18000,72000);</description>
        /// </item>
        /// <item>
        /// <term>Perpetual Backup Mode</term>
        /// <description>SetPeriodicMode(GPS3Click.PeriodicMode.Perpetual);</description>
        /// </item>
        /// <item>
        /// <term>AlwaysLocate™ Standby</term>
        /// <description>SetPeriodicMode(GPS3Click.PeriodicMode.Standby);</description>
        /// </item>
        /// <item>
        /// <term>AlwaysLocate™ Backup</term>
        /// <description>SetPeriodicMode(GPS3Click.PeriodicMode.Backup);</description>
        /// </item>
        /// <item>
        /// <term>Exit Periodic Modes</term>
        /// <description>SetPeriodicMode(GPS3Click.PeriodicMode.Normal);</description>
        /// </item>
        /// </list>
        /// <code language = "C#">
        /// Debug.Print("Entering Periodic Mode - Backup? " + gps.SetPeriodicMode(GPS3Click.PeriodicMode.Backup, 25, 5000));
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Entering Periodic Mode - Backup? " <![CDATA[&]]> gps.SetPeriodicMode(GPS3Click.PeriodicMode.Backup, 25, 5000))
        /// </code>
        /// </example>
        public bool SetPeriodicMode(PeriodicMode mode, UInt32 firstRunTime = 0, UInt32 firstSleepTime = 0, UInt32 secondRunTime = 0, UInt32 secondSleepTime = 0)
        {
            CancelPeriodicMode();

            if (IsBetween(firstRunTime, 0, 1000)) firstRunTime = 0;
            if (firstRunTime > 518400000) firstRunTime = 518400000;
            if (firstSleepTime < 1000) firstSleepTime = 1000;
            if (firstSleepTime > 518400000) firstSleepTime = 518400000;

            if (IsBetween(secondRunTime, 0, 1000)) secondRunTime = 0;
            if (secondSleepTime > 518400000) secondSleepTime = 518400000;
            if (firstSleepTime < 1000) firstSleepTime = 1000;
            if (firstSleepTime > 518400000) firstSleepTime = 518400000;

            if (firstRunTime > secondRunTime) secondRunTime += 1000; // Adding 1000 ms to ensure Second Runtime is larger than First Runtime.

            if (firstRunTime == 0) mode = PeriodicMode.Normal;

            return  WaitForResponse(CMD_SET_PERIODIC_MODE, mode + "," + firstRunTime + "," + firstSleepTime + "," + "," + secondRunTime + "," + secondSleepTime);
        }

        /// <summary>
        /// Cancels the GPS3 Click PeriodicMode functionality. 
        /// </summary>
        /// <returns>True if successful in canceling the PeriodicMode or otherwise false.</returns>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// gps.CancelPeriodicMode();
        /// </code>
        /// <code language = "VB">
        /// gps.CancelPeriodicMode()
        /// </code>
        /// </example>
        public Boolean CancelPeriodicMode()
        {
            return WaitForResponse(CMD_CANCEL_PERIODIC_MODE);
        }

        /// <summary>
        /// Sets the DEE (Dynamic Ephemeris Extension) time.
        /// </summary>
        /// <param name="sv">Satellite Value - Default value is 1 (Range: 1 to 4)</param>
        /// <param name="signalToNoiseRatio">SignalToNoise Ratio (SNR) Default Value is 30 (Range: 25 to 30)</param>
        /// <param name="extensionThreshold">The Extension Threshold  is the time (milliSeconds) to extend the amount of time the receiver is on with set values
        /// used by the AlwaysLocate™ feature. Default is 180000 ms (Range: 40000 to 180000)</param>
        /// <param name="extensionGap">Extension gap limits the frequency between Dynamic Ephemeris Extension (DEE) intervals. Default value is 60000 ms (Range: 0 to 3600000)</param>
        /// <returns>True is successful in setting the DEE or otherwise false.</returns>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Set DEE? " + gps.SetDEE(4, 26, 160000, 80000));
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Set DEE? " <![CDATA[&]]> gps.SetDEE(4, 26, 160000, 80000))
        /// </code>
        /// </example>
        public Boolean SetDEE(UInt16 sv, UInt16 signalToNoiseRatio, UInt32 extensionThreshold, UInt32 extensionGap)
        {
            if (sv > 4) sv = 4;

            if (signalToNoiseRatio < 25) signalToNoiseRatio = 25;
            if (signalToNoiseRatio > 30) signalToNoiseRatio = 30;

            if (extensionThreshold < 4000) extensionThreshold = 4000;
            if (extensionThreshold > 180000) extensionThreshold = 180000;

            if (extensionGap > 3600000) extensionGap = 3600000; 

            return WaitForResponse(CMD_SET_PERIODIC_MODE, sv + "," + signalToNoiseRatio + "," + extensionThreshold + "," + extensionGap);
        }

        /// <summary>
        /// Sets the GPS3 Click to Low Power Standby Mode.
        /// </summary>
        /// <returns>True if successful in setting the GPS3 click to Low Power Standby Mode or otherwise false.</returns>
        /// <remarks>
        /// Standby mode is a low-power mode. In standby mode, the internal core and I/O power domain are still
        /// active, but RF and TCXO are powered off, the module stops satellites search and navigation. UART is still
        /// accessible like PMTK commands or any other data, but there is no NMEA messages output.
        /// To cancel Standby Mode, re-issue the EnableStandbyMode or any other command to the GPS3 Click.
        /// </remarks>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// Debug.Print("Entering standby mode ?" + gps.EnableStandbyMode());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Entering standby mode ?" <![CDATA[&]]> gps.EnableStandbyMode())
        /// </code>
        /// </example>
        public Boolean EnableStandbyMode()
        {
            return WaitForResponse(CMD_STANDBY);
        }

        /// <summary>
        /// Performs a restart of the GPS3 Click
        /// </summary>
        /// <param name="restartType">the type of restart to perform. See the <see cref="RestartType"/> enumeration for more information.</param>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// gps.PerformRestart(GPS3Click.RestartType.Warm);
        /// </code>
        /// <code language = "VB">
        /// gps.PerformRestart(GPS3Click.RestartType.Warm)
        /// </code>
        /// </example>
        public void PerformRestart(RestartType restartType)
        {
            var type = restartType == RestartType.Hot
                ? CMD_HOT_START
                : restartType == RestartType.Warm
                    ? CMD_WARM_START
                    : restartType == RestartType.Cold ? CMD_COLD_START : CMD_FULL_COLD_START;

            _deviceReady = false;

            var cmd = Encoding.UTF8.GetBytes(type + EOL + EOL);
            _serial.Write(cmd, 0, cmd.Length);

            _serial.DataReceived += _serial_DataReceived;
            while (!_waitingForRestart)
            {
                if (_waitingForRestart) break;
                if (Debugger.IsAttached) Debug.Print("Waiting on GPS3 Click to finish Restart");
                Thread.Sleep(100);
            }

            while (true)
            {
                if (_deviceReady) break;
                var cmd2 = Encoding.UTF8.GetBytes(CMD_TEST_PACKET + EOL + EOL);
                _serial.Write(cmd2, 0, cmd2.Length);

                if (Debugger.IsAttached) Debug.Print("Waiting on GPS3 Click ready status");
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Starts the GPS3 Click driver to process NMEA Messages.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        ///  gps.StartGPS();
        /// </code>
        /// <code language = "VB">
        ///  gps.StartGPS()
        /// </code>
        /// </example>
        public void StartGPS()
        {
            SendCommand(CMD_TEST_PACKET);
            processQueueMessages = true;
        }
        
        /// <summary>
        /// Stops the GPS3 Click driver to process NMEA Messages.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        ///  gps.StopGPS();
        /// </code>
        /// <code language = "VB">
        ///  gps.StopGPS()
        /// </code>
        /// </example>
        public void StopGPS()
        {
            EnableStandbyMode();
            processQueueMessages = false;
        }

        #endregion

        #region Private Methods

        private void InternalReset()
        {
            _resetPin.Write(false);
            Thread.Sleep(15); // 10ms required per L80 Hardware Design v 1.2 Datasheet
            _resetPin.Write(true);
            Thread.Sleep(1000);
        }

        private Boolean InitializeMembers()
        {
            UInt16 err = 0;
            if (QueryFirmwareInfo() == String.Empty) err++;
            if (!QueryEASYStatus()) err++;
            if (!QueryDPGSStatus()) err++;
            if (Math.Abs(QueryHDOPThreshold()) > 0.0) err++;
            if (QueryNMEASubscriptions() != new NMEASubscriptions()) err++;
            if (QueryPositionFixControlRate() != 0) err++;
            if (!QuerySBASStatus()) err++;
            if (Math.Abs(QueryStaticNavigationthreshold()) > 0.0) err++;
            if (!QueryLSCEnabled()) err++;
            if (!SetAlwaysLocateMode(AlwaysLocateMode.Normal)) err++;
            return err != 0;
        }
       
        [DebuggerStepThrough]
        private Boolean WaitForResponse(String command, String commandParameters = null)
        {
            _autoEvent.Reset();
            SendCommand(command + commandParameters);
            _autoEvent.WaitOne(_autoResetWaitTime, true);
            return _waitMessage == command;
        }

        [DebuggerStepThrough]
        private static bool IsBetween(UInt32 testNumber, UInt32 lowerRange, UInt32 upperRange)
        {
            return testNumber <= upperRange && testNumber >= lowerRange;
        }
       
        [DebuggerStepThrough]
        private static String Replace(String Source, String ToFind, String ReplaceWith)
        {
            Int32 startIndex = 0;
            if (Source == String.Empty || Source == null || (ToFind == String.Empty || ToFind == null)) return Source;
            while (true)
            {
                Int32 length = Source.IndexOf(ToFind, startIndex);
                if (length >= 0)
                {
                    Source = length <= 0
                        ? ReplaceWith + Source.Substring(length + ToFind.Length)
                        : Source.Substring(0, length) + ReplaceWith + Source.Substring(length + ToFind.Length);
                    startIndex = length + ReplaceWith.Length;
                }
                else
                {
                    break;
                }
            }
            return Source;
        }

        [DebuggerStepThrough]
        private static Boolean EndsWith(string src, string target)
        {
            if (target.Length > src.Length) return false;
            var num = src.Length - target.Length;
            bool endWith = true;
            for (var i = 0; i < target.Length; i++)
            {
                if (target[i] != src[i + num])
                {
                    endWith = false;
                }
            }
            return endWith;
        }

        [DebuggerStepThrough]
        private static DateTime ParseDateTime(String utcTimeString, String utcDateString)
        {
            double timeRawDouble = Double.Parse(utcTimeString);
            var timeRaw = (Int32) timeRawDouble;
            int hours = timeRaw/10000;
            int minutes = (timeRaw/100)%100;
            int seconds = timeRaw%100;
            var milliseconds = (Int32) ((timeRawDouble - timeRaw)*1000.0);
            int dateRaw = Int32.Parse(utcDateString);
            int day = dateRaw/10000;
            int month = (dateRaw/100)%100;
            int year = 2000 + (dateRaw%100);
            return new DateTime(year, month, day, hours, minutes, seconds, milliseconds);
        }

        [DebuggerStepThrough]
        private static Time ParseTime(String utcTimeString)
        {
            var timeRawDouble = Double.Parse(utcTimeString);
            var timeRaw = (Int32)timeRawDouble;
            Int32 hours = timeRaw / 10000;
            Int32 minutes = (timeRaw / 100) % 100;
            Int32 seconds = timeRaw % 100;
            var milliseconds = (Int32)((timeRawDouble - timeRaw) * 1000.0);
            return new Time(hours, minutes, seconds, milliseconds);
        }

        [DebuggerStepThrough]
        private static Date ParseDate(String utcDateString)
        {
            var dateRaw = Int32.Parse(utcDateString);
            var day = dateRaw / 10000;
            var month = (dateRaw / 100) % 100;
            var year = 2000 + (dateRaw % 100);
            return new Date(year, month, day);
        }

        [DebuggerStepThrough]
        private static Double ParseLatitude(String latitudeString, String hemisphereString)
        {
            double latitudeRaw = Double.Parse(latitudeString);
            int latitudeDegreesRaw = ((Int32) latitudeRaw)/100;
            Double latitudeMinutesRaw = latitudeRaw - (latitudeDegreesRaw*100);
            double latitude = latitudeDegreesRaw + (latitudeMinutesRaw/60.0);
            if (hemisphereString == "S") latitude = -latitude;
            return latitude;
        }

        [DebuggerStepThrough]
        private static Double ParseLongitude(String longitudeString, String hemisphereString)
        {
            double longitudeRaw = Double.Parse(longitudeString);
            int longitudeDegreesRaw = ((Int32) longitudeRaw)/100;
            Double longitudeMinutesRaw = longitudeRaw - (longitudeDegreesRaw*100);
            double longitude = longitudeDegreesRaw + (longitudeMinutesRaw/60.0);
            if (hemisphereString == "W") longitude = -longitude;
            return longitude;
        }

        private static readonly Queue messageQueue = new Queue();
        private Boolean processQueueMessages = false;

        private void ProcessMessageQueue()
        {
            while (true)
            {
                if (messageQueue.Count == 0) continue;
                if (!processQueueMessages) continue;
                if (!_deviceReady) continue;

                ThreadStart threadStart;

                var sentence = (String)messageQueue.Dequeue();
                if (sentence == null || !IsValidChecksum(sentence)) continue;
                sentence = sentence.Substring(0, sentence.Length - 3);


                switch (sentence.Substring(0, 6))
                {
                    case "$GPRMC":
                    {
                        threadStart = () => ParseRMCSentence(sentence);
                        break;
                    }
                    case "$GPTXT":
                    {
                        threadStart = () => ParseTXTSentence(sentence);
                        break;
                    }
                    case "$GPGGA":
                    {
                        threadStart = () => ParseGGASentence(sentence);
                        break;
                    }
                    case "$GPGSA":
                    {
                        threadStart = () => ParseGSASentence(sentence);
                        break;
                    }
                    case "$GPVTG":
                    {
                        threadStart = () => ParseVTGSentence(sentence);
                        break;
                    }
                    case "$GPGSV":
                    {
                        threadStart = () => ParseGSVSentence(sentence);
                        break;
                    }
                    case "$GPGLL":
                    {
                        threadStart = () => ParseGLLSentence(sentence);
                        break;
                    }
                    default:
                    {
                        continue;
                    }
                }

                if (threadStart.Method != null)
                {
                    var thread = new Thread(threadStart) { Priority = ThreadPriority.Normal };
                    thread.Start();
                }

                Thread.Sleep(20);
            }
        }
        
        private void _serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (changingBuadRate) // We are changing BaudRate here so expect a few bad characters.
            {
                _serial.DiscardInBuffer();
                return;
            }

            var gpsDataIn = _serial.Deserialize();

            foreach (String gpsSentence in gpsDataIn)
            {
                if (gpsSentence == null || !IsValidChecksum(gpsSentence)) return;

                String sentence = gpsSentence.Substring(0, gpsSentence.Length - 3);

                if (sentence.Substring(0, 5) == "$PMTK")
                {
                    var pmtkThreadStart = new ThreadStart(() => ParsePMTKSentence(sentence));
                    var pmtkThread = new Thread(pmtkThreadStart) { Priority = ThreadPriority.Highest };
                    pmtkThread.Start();
                }

                messageQueue.Enqueue(gpsSentence);

                OnNMEASentenceReceived(this, sentence);
            }
        }

        private void ParseTXTSentence(String txtSentence)
        {
            string[] tokens = txtSentence.Split(',');
            if (tokens[4].Substring(0, 9) == "ANTSTATUS")
            {
                var s = tokens[4].Substring(10, tokens[4].Length - 10);
                var antennaStatus = s == "OK" ? AntennaStatus.Ok : s == "OPEN" ? AntennaStatus.Open : AntennaStatus.Shorted;
                if (_antenna == antennaStatus) return;
                _antenna = antennaStatus;
                OnAntennaStatusChanged(this, antennaStatus);
            }
        }

        private void ParseRMCSentence(String rmcSentence)
        {
            string[] tokens = rmcSentence.Split(',');

            var latitude = ParseLatitude(tokens[3], tokens[4]);
            var longitude = ParseLongitude(tokens[5], tokens[6]);

            var gpsUtcDateTime = ParseDateTime(tokens[1], tokens[9]);
            
            var rmcData = new RMCData
            {
                TimeUTC = ParseTime(tokens[1]),
                DataValid = tokens[2]== "A",
                DateUtc = ParseDate(tokens[9]),
                COG = (tokens[8] != "") ? Double.Parse(tokens[8]) : 0,
                Latitude = (tokens[4] == "S" ? Math.Abs(latitude) : latitude),
                LatitudeHemisphere = tokens[4],
                Longitude = (tokens[6] == "W") ? Math.Abs(longitude) : longitude,
                LongitudeHemisphere = tokens[6],
                Speed = (tokens[7] != "") ? Double.Parse(tokens[7]) : 0,
                PositioningMode = tokens[12] == "D" ? PositioningModes.DifferentialFix : tokens[12] == "A" ? PositioningModes.AutonomousFix : PositioningModes.NoFix
            };

            var accel = (_lastRmcData.Speed - rmcData.Speed) * 0.51444444 / (gpsUtcDateTime.Ticks - lastPositionFixTime.Ticks) * 0.0000001;
            var gForce = accel / 9.80665;

            if ((Math.Abs(_lastRmcData.Latitude - rmcData.Latitude) > 0.0) || Math.Abs(_lastRmcData.Longitude - rmcData.Longitude) > 0.0) OnRMCCoordinatesUpdated(this, rmcData.Latitude, rmcData.Longitude, gpsUtcDateTime);
            if (Math.Abs(_lastRmcData.Speed - rmcData.Speed) > 0.0) OnRMCSpeedChanged(this, rmcData.Speed, accel, gForce);
            if (Math.Abs(_lastRmcData.COG - rmcData.COG) > 0.0) OnRMCCourseChanged(this, rmcData.COG, gpsUtcDateTime);
            if (_lastRmcData.PositioningMode != rmcData.PositioningMode) OnRMCPositioningModeChanged(this, rmcData.PositioningMode);

            _lastRmcData = rmcData;
            OnRMCSenetenceReceived(this, _lastRmcData);
            
            lastPositionFixTime = Utility.GetMachineTime();
        }

        private void ParseGSVSentence(String gsvSentence)
        {
            var tokens = gsvSentence.Split(',');

            Satellite satId1 = (tokens.Length > 5)
                ? new Satellite((ushort) (tokens[4] != String.Empty ? UInt16.Parse(tokens[4]) : 0),
                    (ushort) (tokens[5] != String.Empty ? UInt16.Parse(tokens[5]) : 0),
                    (ushort) (tokens[6] != String.Empty ? UInt16.Parse(tokens[6]) : 0),
                    (ushort) ((tokens[7] != String.Empty) ? UInt16.Parse(tokens[7]) : 0))
                : new Satellite();

            Satellite satId2 = (tokens.Length > 9)
                ? new Satellite((ushort) (tokens[8] != String.Empty ? UInt16.Parse(tokens[8]) : 0),
                    (ushort) (tokens[9] != String.Empty ? UInt16.Parse(tokens[9]) : 0),
                    (ushort) (tokens[10] != String.Empty ? UInt16.Parse(tokens[10]) : 0),
                    (ushort) ((tokens[11] != String.Empty) ? UInt16.Parse(tokens[11]) : 0))
                : new Satellite();

            Satellite satId3 = (tokens.Length > 13)
                ? new Satellite((ushort) (tokens[12] != String.Empty ? UInt16.Parse(tokens[12]) : 0),
                    (ushort) (tokens[13] != String.Empty ? UInt16.Parse(tokens[13]) : 0),
                    (ushort) (tokens[14] != String.Empty ? UInt16.Parse(tokens[14]) : 0),
                    (ushort) ((tokens[15] != String.Empty) ? UInt16.Parse(tokens[15]) : 0))
                : new Satellite();

            Satellite satId4 = (tokens.Length > 17)
                ? new Satellite((ushort) (tokens[16] != String.Empty ? UInt16.Parse(tokens[16]) : 0),
                    (ushort) (tokens[17] != String.Empty ? UInt16.Parse(tokens[17]) : 0),
                    (ushort) (tokens[18] != String.Empty ? UInt16.Parse(tokens[18]) : 0),
                    (ushort) ((tokens[19] != String.Empty) ? UInt16.Parse(tokens[19]) : 0))
                : new Satellite();
            
            var newGSVData = new GSVData
            {
                SatellitesInView = UInt16.Parse(tokens[3]),
                NumberOfMessages = Int32.Parse(tokens[1]),
                SequenceNumber = Int32.Parse(tokens[2]),
                SatelliteID1 = satId1,
                SatelliteID2 = satId2,
                SatelliteID3 = satId3,
                SatelliteID4 = satId4
            };

            _lastGsvData = newGSVData;
            OnGSVSenetenceReceived(this, _lastGsvData);

            svTemp[0] = newGSVData.SatelliteID1;
            svTemp[1] = newGSVData.SatelliteID2;
            svTemp[2] = newGSVData.SatelliteID3;
            svTemp[3] = newGSVData.SatelliteID4;


            foreach (var satellite in svTemp)
            {
                if (satellite.PRNNumber != 0) satelliteQueue.Enqueue(satellite);
            }

            if (newGSVData.SatellitesInView != satelliteQueue.Count) return;

            var satellites = new Satellite[satelliteQueue.Count];
            satelliteQueue.CopyTo(satellites, 0);
            OnSatellitesInViewChanged(this, satellites);
            satelliteQueue.Clear();
        }

        private void ParseGSASentence(String gsaSentence)
        {
            var tokens = gsaSentence.Split(',');

            UInt16 satelliteCount = 0;

            for (var i = 3; i < 15; i++)
            {
                if (tokens[i] != String.Empty) satelliteCount += 1;
                else break;
            }

            var _satellitesIds = new UInt16[satelliteCount];

            for (var i = 0; i < satelliteCount; i++) _satellitesIds[i] = UInt16.Parse(tokens[i + 3]);

            var newGSAData = new GSAData
            {
                FixStatus = tokens[2] == "1" ? GNSSFixStatus.NoFix : tokens[2] == "2" ? GNSSFixStatus.Fix2D : GNSSFixStatus.Fix3D,
                HDOP = Double.Parse(tokens[16]),
                PDOP = Double.Parse(tokens[15]),
                VDOP = Double.Parse(tokens[17]),
                Satellites = _satellitesIds,
                Mode = tokens[1] == "M" ? GNSSFixMode.Manual : GNSSFixMode.Auto,
            };

            if (_lastGsaData.FixStatus != newGSAData.FixStatus) OnGNSSFixStatusChanged(this, newGSAData.FixStatus);
            if (_lastGsaData.Mode != newGSAData.Mode) OnGNSSFixModeChanged(this, newGSAData.Mode);

            if (_lastGsaData.Satellites != null)
            {
                if (_lastGsaData.GetHashCode() != newGSAData.GetHashCode())
                {
                    OnSatellitesUsedChanged(this, _satellitesIds);
                }
            }

            if (_lastGsaData != newGSAData)
            {
                _lastGsaData = newGSAData;
                OnGSASenetenceReceived(this, newGSAData);
            }
        }

        private void ParseGGASentence(String ggaSentence)
        {
            var tokens = ggaSentence.Split(',');

            var newGGAData = new GGAData
            {
                TimeUTC = ParseTime(tokens[1]),
                Latitude = ParseLatitude(tokens[2], tokens[3]),
                LatitudeHemisphere = tokens[3],
                Longitude =  ParseLongitude(tokens[4], tokens[5]),
                LongitudeHemisphere = tokens[5],
                Fix = tokens[6] == "0" ? FixType.Invalid : tokens[2] == "1" ? FixType.GNSSFix : FixType.DGPSFix,
                Altitude = Double.Parse(tokens[9]),
                NumberSV =  UInt16.Parse(tokens[7]),
                HDOP = Double.Parse(tokens[8]),
                GeoIDSeparation =  Double.Parse(tokens[11]),
                DGPSStationID = tokens[13] != String.Empty ? Int32.Parse(tokens[13]) : 0,
                DGPSAge = tokens[13] != String.Empty ? Int32.Parse(tokens[13]) : 0,
            };

            if ((Math.Abs(_lastGgaData.Latitude - newGGAData.Latitude) > 0.0) || Math.Abs(_lastGgaData.Longitude - newGGAData.Longitude) > 0.0) OnGGACoordinatesChanged(this, newGGAData.Latitude, newGGAData.Longitude, newGGAData.TimeUTC);
            if (_lastGgaData.Fix != newGGAData.Fix) OnFixStatusChanged(this,  newGGAData.Fix);
            if (Math.Abs(_lastGgaData.DGPSStationID - newGGAData.DGPSStationID) > 0) OnDPGSStationIDchanged(this, _lastGgaData.DGPSStationID, newGGAData.DGPSStationID);
            if (Math.Abs(_lastGgaData.Altitude - newGGAData.Altitude) > 0.0) OnAltitudeChanged(this, newGGAData.Altitude);
            if (_lastGgaData.NumberSV != newGGAData.NumberSV) OnNumberSatellitesUsedChanged(this, newGGAData.NumberSV);

            _lastGgaData = newGGAData;
            OnGGASentenceReceived(this, _lastGgaData);
        }

        private void ParseGLLSentence(String gllSentence)
        {
            var tokens = gllSentence.Split(',');

            var newGLLData = new GLLData
            {
                Latitude = ParseLatitude(tokens[1], tokens[2]),
                LatitudeHemisphere = tokens[2],
                Longitude =  ParseLongitude(tokens[3], tokens[4]),
                LongitudeHemisphere = tokens[4],
                UTCTime = ParseTime(tokens[5]),
                DataValid = tokens[6] == "A",
                PositioningMode = tokens[7] == "D" ? PositioningModes.DifferentialFix : tokens[7] == "A" ? PositioningModes.AutonomousFix : PositioningModes.NoFix
            };

            if ((Math.Abs(_lastGllData.Latitude - newGLLData.Latitude) > 0.0) || Math.Abs(_lastGllData.Longitude - newGLLData.Longitude) > 0.0) OnGLLCoordinatesChanged(this, newGLLData.Latitude, newGLLData.Longitude, newGLLData.UTCTime);
            if (_lastGllData.PositioningMode != newGLLData.PositioningMode) OnGLLPositioningModeChanged(this, newGLLData.PositioningMode);

            _lastGllData = newGLLData;
            OnGLLSenetenceReceived(this, _lastGllData);
            Debug.Print(gllSentence);
        }

        private void ParseVTGSentence(String vtgSentence)
        {
            var tokens = vtgSentence.Split(',');

            var newVTGData = new VTGData
            {
                PositioningMode = tokens[9] == "D" ? PositioningModes.DifferentialFix : tokens[9] == "A" ? PositioningModes.AutonomousFix : PositioningModes.NoFix,
                COG = (tokens[1] != "") ? Double.Parse(tokens[1]) : 0,
                SpeedKnots = (tokens[5] != "") ? Double.Parse(tokens[5]) : 0,
                SpeedKPH = (tokens[7] != "") ? Double.Parse(tokens[7]) : 0
            };

            if ((Math.Abs(_lastVTGData.SpeedKnots - newVTGData.SpeedKnots) > 0) || (Math.Abs(_lastVTGData.SpeedKPH - newVTGData.SpeedKPH) > 0.0)) OnVTGSpeedChanged(this, newVTGData.SpeedKnots, newVTGData.SpeedKPH);
            if (Math.Abs(_lastVTGData.COG - newVTGData.COG) > 0) OnVTGCourseChanged(this, newVTGData.COG);
            if (_lastVTGData.PositioningMode != newVTGData.PositioningMode) OnVTGPositioningModeChanged(this, newVTGData.PositioningMode);

            if (_lastVTGData != newVTGData)
            {
                _lastVTGData = newVTGData;
                OnVTGSenetenceReceived(this, _lastVTGData);
            }
        }

        private void ParsePMTKSentence(String pmtkSentence)
        {
            string[] tokens = pmtkSentence.Split(',');

            _waitMessage = String.Empty;

            switch (tokens[0])
            {
                case "$PMTK001":
                {
                    switch (tokens[1])
                    {
                        case "0": // Packet Type: Test Packet (Undocumented)
                        {
                            // Response to Test packet ($PMTK000*32) - $PMTK001,0,3*30 means Device Ready and communicating at the selected BaudRate.
                            _deviceReady = tokens[2] == "3";

                            _autoEvent.Set();
                            break;
                        }
                        case "161":
                        {
                            _waitMessage = tokens[2] == "3" ? CMD_STANDBY : String.Empty;
                            _autoEvent.Set();
                            break;
                        }
                        case "220": // Packet Type: 220 PMTK_SET_POS_FIX
                        {
                            _waitMessage = tokens[2] == "3" ? CMD_SET_POSITION_FIX_INTERVAL : String.Empty;
                            _autoEvent.Set();
                            break;
                        }
                        case "223": // Packet Type: 223 PMTK_SET_AL_DEE_CFG
                        {
                            _waitMessage = tokens[2] == "3" ? CMD_SET_DEE : String.Empty;
                            _autoEvent.Set();
                            break;
                        }
                        case "225": // Packet Type: 225 PMTK_SET_PERIODIC_MODE
                        {
                            _waitMessage = tokens[2] == "3" ? CMD_SET_PERIODIC_MODE : String.Empty;
                            _autoEvent.Set();
                            break;
                        }
                        case "286": // Packet Type: 286 PMTK_SET_AIC_ENABLED
                        {
                            _waitMessage = tokens[2] == "3" ? CMD_SET_AIC : String.Empty;
                            _autoEvent.Set();
                            break;
                        }
                        case "300": // Packet Type: 300 PMTK_API_SET_FIX_CTL
                        {
                            _waitMessage = tokens[2] == "3" ? CMD_SET_POSITION_FIX_CONTROL : String.Empty;
                            _autoEvent.Set();
                            break;
                        }
                        case "301": // Packet Type: 301 PMTK_API_SET_DGPS_MODE
                        {
                            _waitMessage = tokens[2] == "3" ? CMD_SET_DPGS : String.Empty;
                            _autoEvent.Set();
                            break;
                        }
                        case "313": // Packet Type: 313 PMTK_API_SET_SBAS_ENABLED
                        {
                            _waitMessage = tokens[2] == "3" ? CMD_SET_SBAS : String.Empty;
                            _autoEvent.Set();
                            break;
                        }
                        case "314": // Packet Type: 314 PMTK_API_SET_NMEA_OUTPUT
                        {
                            _waitMessage = tokens[2] == "3" ? CMD_SET_NEMA_SENTENCES : String.Empty;
                            _autoEvent.Set();
                            break;
                        }
                        case "386": // Packet Type: 386 PMTK_API_SET_STATIC_NAV_THD
                        {
                            _waitMessage = tokens[2] == "3" ? CMD_SET_STATIC_NAVIGATION_THRESHOLD : String.Empty;
                            _autoEvent.Set();
                            break;
                        }
                        case "869": // Packet Type: 869 PMTK_EASY_ENABLE
                        {
                            _waitMessage = tokens[2] == "3" ? CMD_SET_EASY : String.Empty;
                            _autoEvent.Set();
                            break;
                        }
                        case "875": // Packet Type: 875 PMTK_PMTKLSC_STN_OUTPUT
                        {
                            _waitMessage = tokens[2] == "3" ? CMD_SET_PMTKLSC : String.Empty;
                            _autoEvent.Set();
                            break;
                        }
                    }
                    break;
                }
                case "$PMTK010": // Packet Type: 010 PMTK_SYS_MSG
                {
                    _waitingForRestart = tokens[1] == "001" || tokens[1] == "002" || tokens[1] == "003";
                    break;
                }
                case "$PMTK011": // Packet Type: 011 PMTK_TXT_MSG
                {
                    _waitingForRestart = tokens[1] == "MTKGPS";
                    break;
                }
                case "$PMTK356": // Packet Type: 356 PMTK_API_SET_HDOP_THRESHOLD
                {
                    _waitMessage = EndsWith(tokens[1], "Set OK!") ? CMD_SET_HDOP_THRESHOLD : String.Empty;
                    _autoEvent.Set();
                    break;
                }
                case "$PMTK357": // Packet Type: 357 Get HDOP Threshold (Undocumented)
                {
                    _waitMessage = CMD_QUERY_HDOP_THRESHOLD;
                    _hdopThreshold = (tokens[1] != String.Empty) ? Double.Parse(tokens[1]) : Double.NaN;
                    _autoEvent.Set();
                    break;
                }
                case "$PMTK500": // Packet Type: 500 PMTK_DT_FIX_CTL
                {
                    _waitMessage = CMD_QUERY_POSITION_FIX_CONTROL;
                    _positionFixControlRate = UInt16.Parse(tokens[1]);
                    _autoEvent.Set();
                    break;
                }
                case "$PMTK501": // Packet Type: 501 PMTK_DT_DGPS_MODE
                {
                    _waitMessage = CMD_QUERY_DPGS_MODE;
                    _dpgsEnabled = tokens[1] == "2";
                    _autoEvent.Set();
                    break;
                }
                case "$PMTK513": // Packet Type: 513 PMTK_DT_SBAS_ENABLED
                {
                    _waitMessage = CMD_QUERY_SBAS;
                    _sbasEnabled = tokens[1] == "1";
                    _autoEvent.Set();
                    break;
                }
                case "$PMTK514": // Packet Type: 514 PMTK_DT_NMEA_OUTPUT
                {
                    _waitMessage = CMD_QUERY_NMEA_SENTENCES;
                    _nmeaOutputString = pmtkSentence;
                    _autoEvent.Set();
                    break;
                }
                case "$PMTK527": //  Packet Type: 527 PMTK_DT_Nav_Threshold (Undocumented)
                {
                    _waitMessage = CMD_QUERY_STATIC_NAVIGATION;
                    _staticNavigationThreshold = Double.Parse(tokens[1]);
                    _autoEvent.Set();
                    break;
                }
                case "$PMTK705": // Packet Type: 705 PMTK_DT_RELEASE
                {
                    String[] version = pmtkSentence.Split(',');
                    _gpsFirmwareInfo = "GPS Model: " + version[3] + ", Firmware Version: " + version[1] + ", Build: " +
                                       version[2] + ", SDK Version: " +
                                       (version[4] == String.Empty ? "N/A" : version[4]);

                    _waitMessage = CMD_QUERY_FIRMWARE_INFO;
                    _autoEvent.Set();
                    break;
                }
                case "$PMTK869": // Packet Type: 869 PMTK_EASY_ENABLE
                {
                    if (tokens[1] == "2")
                    {
                        _waitMessage = CMD_QUERY_EASY_STATUS;
                        _easyEnabled = tokens[2] == "1";
                        _autoEvent.Set();
                    }
                    break;
                }
                case "PMTK875": // Packet Type: 875 PMTK_PMTKLSC_STN_OUTPUT
                {
                    if (tokens[1] == "1")
                    {
                        _waitMessage = tokens[1] == "2" ? CMD_SET_PMTKLSC : String.Empty;
                        _pmtkSLCEnabled = tokens[2] == "1";
                        _autoEvent.Set();
                        break;
                    }
                    if (tokens[1] == "2")
                    {
                        _waitMessage = CMD_QUERY_PMTKLSC;
                        _pmtkSLCEnabled = tokens[2] == "1";
                        _autoEvent.Set();
                    }
                    break;
                }
            }

            OnPMTKSentenceReceived(this, pmtkSentence);
        }

        #endregion

        #region Events

        #region Antenna Status

        /// <summary>
        ///     The delegate used for the <see cref="AntennaStatusChanged" /> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="status">The <see cref="AntennaStatus" /> of the External Antenna.</param>
        public delegate void AntennaStatusChangedHandler(GPS3Click sender, AntennaStatus status);

        /// <summary>
        ///     The event that is raised when the status of the External Antenna changes.
        /// </summary>
        public event AntennaStatusChangedHandler AntennaStatusChanged;

        private void OnAntennaStatusChanged(GPS3Click sender, AntennaStatus status)
        {
            AntennaStatusChangedHandler handler = AntennaStatusChanged;
            if (handler == null) return;
            handler(sender, status);
        }

        #endregion

        #region MMEA Message Received Event

        /// <summary>
        ///     Represents the delegate that is used to handle the <see cref="GPS3Click.NMEASentenceReceived" /> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="nmeaSentence">The event arguments.</param>
        public delegate void NMEASentenceReceivedHandler(GPS3Click sender, String nmeaSentence);

        /// <summary>
        ///     The event that is raised when an NMEA sentence is received.
        /// <para><b>This is for advanced users who want to parse the NMEA sentences themselves.</b></para>  
        /// </summary>
        public event NMEASentenceReceivedHandler NMEASentenceReceived;

        private void OnNMEASentenceReceived(GPS3Click sender, String nmeaSentence)
        {
            NMEASentenceReceivedHandler handler = NMEASentenceReceived;
            if (handler == null) return;
            handler(sender, nmeaSentence);
        }

        #endregion

        #region PMTK Message Received Event

        /// <summary>
        /// The delegate used for the <see cref="PMTKSentenceReceived"/> event.
        /// </summary>
        /// <param name="sender">The GPS3 click that raised the event.</param>
        /// <param name="pmtkSentence">The PMTK GPS Sentence</param>
        public delegate void PMTKSentenceReceivedHandler(GPS3Click sender, String pmtkSentence);

        /// <summary>
        ///     Raised when a PMTK sentence is received.  
        /// <para><b>This is for advanced users who want to parse the NMEA sentences themselves.</b></para>  
        /// </summary>
        public event PMTKSentenceReceivedHandler PMTKSentenceReceived;

        private void OnPMTKSentenceReceived(GPS3Click sender, String pmtkSentence)
        {
            PMTKSentenceReceivedHandler handler = PMTKSentenceReceived;
            if (handler == null) return;
            handler(sender, pmtkSentence);
        }

        #endregion

        #region Error Event

        /// <summary>
        ///    The delegate used for the <see cref="GPS3Click.SerialError" /> event.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event.</param>
        /// <param name="errorDescription">The <see cref="System.String" /> containing the description of the error."</param>
        /// <param name="eventTime">The time that the event occurred.</param>
        public delegate void ErrorReceivedHandler(object sender, String errorDescription, DateTime eventTime);

        /// <summary>
        ///     The event that is raised when the GPS3 Click receives an Error in data transmission.
        /// </summary>
        public event ErrorReceivedHandler SerialError;

        private void _serial_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            String eventType;
            switch (e.EventType)
            {
                case System.IO.Ports.SerialError.Frame:
                    eventType = "Frame Error";
                    break;
                case System.IO.Ports.SerialError.Overrun:
                    eventType = "Overrun Error";
                    break;
                case System.IO.Ports.SerialError.RXOver:
                    eventType = "RX Overflow Error";
                    break;
                case System.IO.Ports.SerialError.RXParity:
                    eventType = "RX Parity Error";
                    break;
                case System.IO.Ports.SerialError.TXFull:
                    eventType = "TX Buffer Full";
                    break;
                default:
                    eventType = e.ToString();
                    break;
            }
            ErrorReceivedHandler handler = SerialError;
            if (handler == null) return;
            handler(this, eventType, DateTime.Now);
        }

        #endregion

        #region RMC Data Received Event

        /// <summary>
        ///     The event that is raised when valid RMC Data is received.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to RMC Sentences.</remarks>
        public event RMCSentenceReceivedHandler RMCSentenceReceived;

        /// <summary>
        ///     The delegate used for the <see cref="GPS3Click.RMCSentenceReceived" /> event.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event.</param>
        /// <param name="rmcData">The RMC data. See <see cref="RMCData"/></param>
        public delegate void RMCSentenceReceivedHandler(GPS3Click sender, RMCData rmcData);

        private void OnRMCSenetenceReceived(GPS3Click sender, RMCData rmcData)
        {
            RMCSentenceReceivedHandler handler = RMCSentenceReceived;
            if (handler == null) return;
            handler(sender, rmcData);
        }

        #endregion

        #region VTG Data Received Event

        /// <summary>
        ///     The event that is raised when valid VTG Data is received.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to VTG Sentences.</remarks>
        public event VTGSentenceReceivedHandler VTGSentenceReceived;

        /// <summary>
        ///     The delegate used for the <see cref="GPS3Click.VTGSentenceReceived" /> event.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event.</param>
        /// <param name="vtgData">The VTG data. See <see cref="VTGData"/></param>
        public delegate void VTGSentenceReceivedHandler(GPS3Click sender, VTGData vtgData);

        private void OnVTGSenetenceReceived(GPS3Click sender, VTGData vtgData)
        {
            var handler = VTGSentenceReceived;
            if (handler == null) return;
            handler(sender, vtgData);
        }

        #endregion

        #region GLL Data Received Event

        /// <summary>
        ///     The event that is raised when valid GLL Data is received.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to GLL Sentences.</remarks>
        public event GLLSentenceReceivedHandler GLLSentenceReceived;

        /// <summary>
        ///     The delegate used for the <see cref="GPS3Click.GLLSentenceReceived" /> event.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event.</param>
        /// <param name="gllData">The GLL data. See <see cref="GLLData"/></param>
        public delegate void GLLSentenceReceivedHandler(GPS3Click sender, GLLData gllData);

        private void OnGLLSenetenceReceived(GPS3Click sender, GLLData gllData)
        {
            var handler = GLLSentenceReceived;
            if (handler == null) return;
            handler(sender, gllData);
        }

        #endregion

        #region GGA Data Received Event

        /// <summary>
        ///     The event that is raised when valid GGA Data is received.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to GGA Sentences.</remarks>
        public event GGASentenceReceivedHandler GGASentenceReceived;

        /// <summary>
        ///     The delegate used for the <see cref="GPS3Click.GGASentenceReceived" /> event.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event.</param>
        /// <param name="ggaData">The GGA data. See <see cref="GGAData"/></param>
        public delegate void GGASentenceReceivedHandler(GPS3Click sender, GGAData ggaData);

        private void OnGGASentenceReceived(GPS3Click sender, GGAData ggaData)
        {
            var handler = GGASentenceReceived;
            if (handler == null) return;
            handler(sender, ggaData);
        }

        #endregion

        #region GSA Data Received Event

        /// <summary>
        ///     The event that is raised when valid GSA Data is received.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to GSA Sentences.</remarks>
        public event GSASentenceReceivedHandler GSASentenceReceived;

        /// <summary>
        ///     The delegate used for the <see cref="GPS3Click.GSASentenceReceived" /> event.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event.</param>
        /// <param name="gsaData">The GSA data. See <see cref="GSAData"/></param>
        public delegate void GSASentenceReceivedHandler(GPS3Click sender, GSAData gsaData);
        
        private void OnGSASenetenceReceived(GPS3Click sender, GSAData gsaData)
        {
            var handler = GSASentenceReceived;
            if (handler == null) return;
            handler(sender, gsaData);
        }

        #endregion

        #region GSV Data Received Event

        /// <summary>
        ///     The event that is raised when valid GSV Data is received.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to GSV Sentences.</remarks>
        public event GSVSentenceReceivedHandler GSVSentenceReceived;

        /// <summary>
        ///     The delegate used for the <see cref="GPS3Click.GSVSentenceReceived" /> event.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event.</param>
        /// <param name="gsvData">The GSV data. See <see cref="GSVData"/></param>
        public delegate void GSVSentenceReceivedHandler(GPS3Click sender, GSVData gsvData);

        private void OnGSVSenetenceReceived(GPS3Click sender, GSVData gsvData)
        {
            var handler = GSVSentenceReceived;
            if (handler == null) return;
            handler(sender, gsvData);
        }

        #endregion
        
        #region Altitude Changed Event
        
        /// <summary>
        /// The event that is raised when the Altitude changes.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to GGA Sentences.</remarks>
        public event AltitudeChangedHandler AltitudeChanged;

        /// <summary>
        ///     The delegate used for the <see cref="GPS3Click.AltitudeChanged" /> event.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event.</param>
        /// <param name="meters">The new Altitude in meters.</param>
        public delegate void AltitudeChangedHandler(GPS3Click sender, Double meters);

        private void OnAltitudeChanged(GPS3Click sender, Double meters)
        {
            AltitudeChangedHandler handler = AltitudeChanged;
            if (handler == null) return;
            handler(sender, meters);
        }

        #endregion

        #region RMC Coordinates Changed Event

        /// <summary>
        /// The delegate used for the <see cref="RMCPositioningModeChanged"/> event.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event</param>
        /// <param name="latitude">The latitude of the GPS Fix</param>
        /// <param name="longitude">The Longitude of the GPS Fix.</param>
        /// <param name="eventTime">The GPS UTC DateTime that the event took place.</param>
        public delegate void RMCCoordinatesUpdatedHandler(GPS3Click sender, Double latitude, Double longitude, DateTime eventTime);

        /// <summary>
        ///     The event that is raised when the Coordinates are changed.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to RMC Sentences.</remarks>
        public event RMCCoordinatesUpdatedHandler RMCCoordinatesChanged;

        private void OnRMCCoordinatesUpdated(GPS3Click sender, Double latitude, Double longitude, DateTime eventTime)
        {
            RMCCoordinatesUpdatedHandler handler = RMCCoordinatesChanged;
            if (handler == null) return;
            handler(sender, latitude, longitude, eventTime);
        }

        #endregion

        #region GLL Coordinates Changed Event

        /// <summary>
        ///     The event that is raised when the Coordinates have changed.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to GLL Sentences.</remarks>
        public event CoordinatesUpdatedHandler GLLCoordinatesChanged;

        private void OnGLLCoordinatesChanged(GPS3Click sender, Double latitude, Double longitude, Time eventTime)
        {
            CoordinatesUpdatedHandler handler = GLLCoordinatesChanged;
            if (handler == null) return;
            handler(sender, latitude, longitude, eventTime);
        }

        #endregion

        #region GGA Coordinates Changed Event

        /// <summary>
        ///     The event that is raised when the Coordinates have changed.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to GGA Sentences.</remarks>
        public event CoordinatesUpdatedHandler GGACoordinatesChanged;

        private void OnGGACoordinatesChanged(GPS3Click sender, Double latitude, Double longitude, Time eventTime)
        {
            CoordinatesUpdatedHandler handler = GGACoordinatesChanged;
            if (handler == null) return;
            handler(sender, latitude, longitude, eventTime);
        }

        #endregion

        #region RMC Course Changed Event

        /// <summary>
        /// The delegate used for the <see cref="RMCCourseChanged"/> event.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event</param>
        /// <param name="heading">The new heading in degrees.</param>
        /// <param name="eventTime">The GPS Time that the event took place.</param>
        public delegate void RMCCourseChangedHandler(GPS3Click sender, Double heading, DateTime eventTime);


        /// <summary>
        ///     The event that is raised when the Course (COG) has changed.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to RMC Sentences.</remarks>
        public event RMCCourseChangedHandler RMCCourseChanged;

        private void OnRMCCourseChanged(GPS3Click sender, Double newHeading, DateTime eventTime)
        {
            RMCCourseChangedHandler handler = RMCCourseChanged;
            if (handler == null) return;
            handler(sender, newHeading, eventTime);
        }

        #endregion

        #region VTG Course Changed Event

        /// <summary>
        ///     The event that is raised when the Course (COG) has changed.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to VTG Sentences.</remarks>
        public event VTGCourseChangedHandler VTGCourseChanged;

        /// <summary>
        /// The delegate used for the <see cref="VTGCourseChanged"/> event.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event</param>
        /// <param name="heading">The new heading in degrees.</param>
        public delegate void VTGCourseChangedHandler(GPS3Click sender, Double heading);


        private void OnVTGCourseChanged(GPS3Click sender, Double heading)
        {
            VTGCourseChangedHandler handler = VTGCourseChanged;
            if (handler == null) return;
            handler(sender, heading);
        }

        #endregion

        #region RMC Speed Changed Handler

        /// <summary>
        /// The delegate used for the <see cref="RMCSpeedChanged"/> event.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event</param>
        /// <param name="knots">The speed in knots. One Knot or Nautical Mile per Hour is equivalent to 1.151 mph.</param>
        /// <param name="acceleration">The acceleration in meters per second squared (m/s^2).</param>
        /// <param name="gForce">The gravitational force (G).</param>
        public delegate void RMCSpeedChangedHandler(GPS3Click sender, Double knots, Double acceleration, Double gForce);

        /// <summary>
        ///     The event that is raised when the Speed has changed.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to RMC Sentences.</remarks>
        public event RMCSpeedChangedHandler RMCSpeedChanged;

        private void OnRMCSpeedChanged(GPS3Click sender, Double knots, Double acceleration, Double gForce)
        {
            RMCSpeedChangedHandler handler = RMCSpeedChanged;
            if (handler == null) return;
            handler(sender, knots, acceleration, gForce);
        }

        #endregion  

        #region VTG Speed Changed Event

        /// <summary>
        /// The delegate used for the <see cref="VTGSpeedChanged"/> event.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event</param>
        /// <param name="knots">The speed in knots. One Knot or Nautical Mile per Hour is equivalent to 1.151 mph.</param>
        /// <param name="kph">The speed kilometers per hour.</param>
        public delegate void VTGSpeedChangedHandler(GPS3Click sender, Double knots, Double kph);
        
        /// <summary>
        ///     The event that is raised when the Speed has changed.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to VTG Sentences.</remarks>
        public event VTGSpeedChangedHandler VTGSpeedChanged;

        private void OnVTGSpeedChanged(GPS3Click sender, Double knots, Double kph)
        {
            VTGSpeedChangedHandler handler = VTGSpeedChanged;
            if (handler == null) return;
            handler(sender, knots, kph);
        }

        #endregion

        #region GNSS Fix Mode Change Event

        /// <summary>
        /// The delegate used for the <see cref="GLLCoordinatesChanged"/>, <see cref="GPS3Click.RMCCoordinatesChanged"/> and <see cref="GGACoordinatesChanged"/>events.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event</param>
        /// <param name="fixMode">The new <see cref="GNSSFixMode"/>.</param>
        public delegate void GNSSFixModeChangedHandler(GPS3Click sender, GNSSFixMode fixMode);
        
        /// <summary>
        ///     The event that is raised when the GNSS Fix Mode has changed.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to GSA Sentences.</remarks>
        public event GNSSFixModeChangedHandler GNSSFixModeChanged;

        private void OnGNSSFixModeChanged(GPS3Click sender, GNSSFixMode fixMode)
        {
            GNSSFixModeChangedHandler handler = GNSSFixModeChanged;
            if (handler == null) return;
            handler(sender, fixMode);
        }

        #endregion

        #region GGA Fix Type Changed Event

        /// <summary>
        /// The delegate used for the <see cref="GPS3Click.FixTypeChanged"/> event.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event</param>
        /// <param name="fixType">The new <see cref="FixType"/>.</param>
        public delegate void FixTypeChangedHandler(GPS3Click sender, FixType fixType);

        /// <summary>
        /// The event that is raised when the <see cref="FixType"/> has changed.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to GGA Sentences.</remarks>
        public event FixTypeChangedHandler FixTypeChanged;

        private void OnFixStatusChanged(GPS3Click sender, FixType e)
        {
            FixTypeChangedHandler handler = FixTypeChanged;
            if (handler == null) return;
            handler(sender, e);
        }

        #endregion

        #region GSA Fix Mode Changed Event

        /// <summary>
        /// The delegate used for the <see cref="GNSSFixStatusChanged"/> event.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event</param>
        /// <param name="fixStatus">The new <see cref="GNSSFixStatus"/>.</param>
        public delegate void GNSSFixStatusChangedHandler(GPS3Click sender, GNSSFixStatus fixStatus);

        /// <summary>
        /// The event that is raised when the <see cref="GNSSFixStatus"/> has changed.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to GSA Sentences.</remarks>
        public event GNSSFixStatusChangedHandler GNSSFixStatusChanged;

        private void OnGNSSFixStatusChanged(GPS3Click sender, GNSSFixStatus fixStatus)
        {
            GNSSFixStatusChangedHandler handler = GNSSFixStatusChanged;
            if (handler == null) return;
            handler(sender, fixStatus);
        }

        #endregion

        #region GLL Positioning Mode Changed Event
        
        /// <summary>
        /// The event that is raised when the <see cref="PositioningModes"/> has changed.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to GLL Sentences.</remarks>
        public event PositioningModeChangedHandler GLLPositioningModeChanged;

        private void OnGLLPositioningModeChanged(GPS3Click sender, PositioningModes positioningMode)
        {
            PositioningModeChangedHandler handler = GLLPositioningModeChanged;
            if (handler == null) return;
            handler(sender, positioningMode);
        }

        #endregion

        #region RMC Positioning Mode Changed Event
        
        /// <summary>
        /// The event that is raised when the <see cref="PositioningModes"/> has changed.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to RMC Sentences.</remarks>
        public event PositioningModeChangedHandler RMCPositioningModeChanged;

        private void OnRMCPositioningModeChanged(GPS3Click sender, PositioningModes positioningMode)
        {
            PositioningModeChangedHandler handler = RMCPositioningModeChanged;
            if (handler == null) return;
            handler(sender, positioningMode);
        }

        #endregion
        
        #region VTG Positioning Mode Changed Event

        /// <summary>
        /// The event that is raised when the <see cref="PositioningModes"/> has changed.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to VTG Sentences.</remarks>
        public event PositioningModeChangedHandler VTGPositioningModeChanged;

        /// <summary>
        /// The delegate used for the <see cref="VTGPositioningModeChanged"/> event.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event</param>
        /// <param name="e">The new <see cref="PositioningModes"/>.</param>
        public delegate void VTGModeChangedHandler(GPS3Click sender, PositioningModes e);

        private void OnVTGPositioningModeChanged(GPS3Click sender, PositioningModes positioningMode)
        {
            PositioningModeChangedHandler handler = VTGPositioningModeChanged;
            if (handler == null) return;
            handler(sender, positioningMode);
        }

        #endregion

        #region Satellites In View Changed Event

        /// <summary>
        /// The delegate used for the <see cref="SatellitesInViewChanged"/> event.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event</param>
        /// <param name="satellites">The new <see cref="PositioningModes"/>.</param>
        public delegate void SatellitesInViewChangedHandler(GPS3Click sender, Satellite[] satellites);

        /// <summary>
        /// The event that is raised when the Satellites in view has changed.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to GSV Sentences.</remarks>
        public event SatellitesInViewChangedHandler SatellitesInViewChanged;

        private void OnSatellitesInViewChanged(GPS3Click sender, Satellite[] satellites)
        {
            SatellitesInViewChangedHandler handler = SatellitesInViewChanged;
            if (handler == null) return;
            handler(sender, satellites);
        }

        #endregion

        #region Satellites In Use change Event
        
        /// <summary>
        /// The delegate used for the <see cref="SatellitesUsedChanged"/> event.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event</param>
        /// <param name="satellites">An array containing the ID's of the Satellites used.</param>
        public delegate void SatellitesUsedChangedHandler(GPS3Click sender, UInt16[] satellites);

        /// <summary>
        /// The event that is raised when the Satellites used has changed.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to GSA Sentences.</remarks>
        public event SatellitesUsedChangedHandler SatellitesUsedChanged;

        private void OnSatellitesUsedChanged(GPS3Click sender, UInt16[] satellites)
        {
            SatellitesUsedChangedHandler handler = SatellitesUsedChanged;
            if (handler == null) return;
            handler(sender, satellites);
        }

        #endregion

        #region Number of Satellites In Use Changed Event

        /// <summary>
        /// The delegate used for the <see cref="NumberSatellitesUsedChanged"/> event.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event</param>
        /// <param name="satellites">The count of Satellites currently in use.</param>
        public delegate void NumberSatellitesUsedChangedHandler(GPS3Click sender, UInt16 satellites);

        /// <summary>
        /// The event that is raised when the Satellites used has changed.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to GGA Sentences.</remarks>
        public event NumberSatellitesUsedChangedHandler NumberSatellitesUsedChanged;

        private void OnNumberSatellitesUsedChanged(GPS3Click sender, UInt16 count)
        {
            NumberSatellitesUsedChangedHandler handler = NumberSatellitesUsedChanged;
            if (handler == null) return;
            handler(sender, count);
        }

        #endregion

        #region DPGS Station ID Changed Event

        /// <summary>
        /// The delegate used for the <see cref="DPGSStationIDChanged"/> event.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event</param>
        /// <param name="oldStationID">The old  DGPS station ID.</param>
        /// <param name="newStationID">The new  DGPS station ID.</param>
        public delegate void DPGSStationIDChangedHandler(GPS3Click sender, Double oldStationID, Double newStationID);

        /// <summary>
        /// The event that is raised when the DGPS Station ID has changed.
        /// </summary>
        /// <remarks>This event will only fire when subscribing to GGA Sentences.</remarks>
        public event DPGSStationIDChangedHandler DPGSStationIDChanged;

        private void OnDPGSStationIDchanged(GPS3Click sender, Double oldStationID, Double newStationID)
        {
            DPGSStationIDChangedHandler handler = DPGSStationIDChanged;
            if (handler == null) return;
            handler(sender, oldStationID, newStationID);
        } 

        #endregion

        #region Non-specific Delegates

        /// <summary>
        /// The delegate used for the <see cref="GLLCoordinatesChanged"/>, <see cref="GPS3Click.RMCCoordinatesChanged"/> and <see cref="GGACoordinatesChanged"/> events.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event</param>
        /// <param name="positioningMode">The new <see cref="PositioningModes"/>.</param>
        public delegate void PositioningModeChangedHandler(GPS3Click sender, PositioningModes positioningMode);

        /// <summary>
        /// The delegate used for the <see cref="GLLPositioningModeChanged"/>, <see cref="RMCPositioningModeChanged"/> and <see cref="VTGPositioningModeChanged"/> events.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event</param>
        /// <param name="latitude">The latitude of the GPS Fix</param>
        /// <param name="longitude">The Longitude of the GPS Fix.</param>
        /// <param name="eventTime">The GPS Time that the event took place.</param>
        public delegate void CoordinatesUpdatedHandler(GPS3Click sender, Double latitude, Double longitude, Time eventTime);

        /// <summary>
        /// The delegate used for the <see cref="RMCCourseChanged"/> and <see cref="VTGCourseChanged"/> events.
        /// </summary>
        /// <param name="sender">The GPS3 Click that raised the event</param>
        /// <param name="heading">The new heading in degrees.</param>
        /// <param name="eventTime">The GPS Time that the event took place.</param>
        public delegate void CourseChangedHandler(GPS3Click sender, Double heading, Time eventTime);

        #endregion

        #endregion

        #region NMEA Message Classes

        /// <summary>
        ///     This class represents a GPS position data from a GPRMC sentence.
        ///     RMC, recommended minimum position data (including position, velocity, course and time).
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// </code>
        /// <code language = "VB">
        /// </code>
        /// </example>
        public class RMCData
        {
            /// <summary>
            ///     Time in format hhmmss.sss
            /// </summary>
            public Time TimeUTC { get; internal set; }

            /// <summary>
            ///     Valid data received. 
            /// </summary>
            public Boolean DataValid { get; internal set; }

            /// <summary>
            ///     The latitude.
            /// </summary>
            public Double Latitude { get; internal set; }

            /// <summary>
            ///     The North/South Hemisphere of the Latitude.
            /// </summary>
            public String LatitudeHemisphere { get; internal set; }

            /// <summary>
            ///     The longitude.
            /// </summary>
            public Double Longitude { get; internal set; }

            /// <summary>
            ///     The East/West Hemisphere of the Longitude.
            /// </summary>
            public String LongitudeHemisphere { get; internal set; }

            /// <summary>
            ///     SpeedKnotsRmc over the ground in knots.
            /// </summary>
            public Double Speed { get; internal set; }

            /// <summary>
            ///     CourseOverGround (COG) or heading in degrees.
            /// </summary>
            public Double COG { get; internal set; }

            /// <summary>
            ///     Date in format DDMMYYYY
            /// </summary>
            public Date DateUtc { get; internal set; }

            /// <summary>
            ///     Position Fix acquisition type.
            /// </summary>
            public PositioningModes PositioningMode { get; internal set; }

            /// <summary>
            ///     Provides a formatted string for RMC Sentence Data.
            /// </summary>
            /// <returns>The formatted string.</returns>
            public override String ToString()
            {
                return "Time: " + TimeUTC + ", Latitude: " + Latitude + " - " + LatitudeHemisphere + ", Longitude: " +
                       Longitude + " - " + LongitudeHemisphere + ", Speed(Knots): " + Speed + ", COG: " + COG +
                       ", Date: " + DateUtc + " , Positioning Mode: " +
                       (PositioningMode == PositioningModes.AutonomousFix
                           ? "Autonomous"
                           : PositioningMode == PositioningModes.DifferentialFix ? "Differential" : "NoFix");
            }
        }

        /// <summary>
        ///     This class represents a GPS information from GPGGA sentences.
        ///     GGA, global positioning system fix data, is the essential fix data which provides 3D location and accuracy data.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// </code>
        /// <code language = "VB">
        /// </code>
        /// </example>
        public class GGAData
        {
            /// <summary>
            ///     Time in format hhmmss.sss
            /// </summary>
            public Time TimeUTC { get; internal set; }

            /// <summary>
            ///     The latitude.
            /// </summary>
            public Double Latitude { get; internal set; }

            /// <summary>
            ///     The North/South Hemisphere of the Latitude.
            /// </summary>
            public String LatitudeHemisphere { get; internal set; }

            /// <summary>
            ///     The longitude.
            /// </summary>
            public Double Longitude { get; internal set; }

            /// <summary>
            ///     The East/West Hemisphere of the Longitude.
            /// </summary>
            public String LongitudeHemisphere { get; internal set; }

            /// <summary>
            ///     The Fix Status
            /// </summary>
            public FixType Fix { get; internal set; }

            /// <summary>
            ///     Number of satellites being used (0~12)
            /// </summary>
            public UInt16 NumberSV { get; internal set; }

            /// <summary>
            ///     Horizontal Dilution of Precision
            /// </summary>
            public Double HDOP { get; internal set; }

            /// <summary>
            ///     Altitude in meters according to WGS84 ellipsoid
            /// </summary>
            public Double Altitude { get; internal set; }

            /// <summary>
            ///     Separation Height of GeoID (mean sea level) above WGS84 ellipsoid, meter
            /// </summary>
            public Double GeoIDSeparation { get; internal set; }

            /// <summary>
            ///     Age of DGPS data in seconds, empty if DGPS is not used
            /// </summary>
            public Int32 DGPSAge { get; internal set; }

            /// <summary>
            ///     DGPS station ID, empty if DGPS is not used
            /// </summary>
            public Double DGPSStationID { get; internal set; }

            /// <summary>
            ///     Provides a formatted string for GGA Sentence Data.
            /// </summary>
            /// <returns>The formatted string.</returns>
            public override String ToString()
            {
                return "Time: " + TimeUTC + ", Latitude: " + Latitude + " - " + LatitudeHemisphere + ", Longitude: " +
                       Longitude + " - " + LongitudeHemisphere + ", " +
                       "Fix Status Mode: " +
                       (Fix == FixType.GNSSFix
                           ? "GNSS Fix"
                           : Fix == FixType.DGPSFix
                               ? "DPGS Fix"
                               : "Invalid" +
                                 ", Satellites In View: " + NumberSV + ", HDOP: " + HDOP + ", " + "GeoID Separation: " +
                                 GeoIDSeparation + ",  DGPS Age: " + DGPSAge +
                                 ", DPGS StationID: " + DGPSAge);
            }
        }

        /// <summary>
        ///     This class represents a GPS information from GPVTG sentences.
        ///     VTG,  Vehicle track made good and ground speed.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// </code>
        /// <code language = "VB">
        /// </code>
        /// </example>
        public class VTGData
        {
            /// <summary>
            ///     CourseOverGround (COG) or heading in degrees.
            /// </summary>
            public Double COG { get; internal set; }

            /// <summary>
            ///     SpeedKnotsRmc over ground in knots
            /// </summary>
            public Double SpeedKnots { get; internal set; }

            /// <summary>
            ///     SpeedKnotsRmc over ground in kilometers per hour.
            /// </summary>
            public Double SpeedKPH { get; internal set; }

            /// <summary>
            ///     Position Fix acquisition type.
            /// </summary>
            public PositioningModes PositioningMode { get; internal set; }

            /// <summary>
            ///     Provides a formatted string for VTG Sentence Data.
            /// </summary>
            /// <returns>The formatted string.</returns>
            public override String ToString()
            {
                return "Speed (Knots): " + SpeedKnots + ", Speed (km/h): " + SpeedKPH + ", Positioning Mode: " +
                       (PositioningMode == PositioningModes.AutonomousFix
                           ? "Autonomous"
                           : PositioningMode == PositioningModes.DifferentialFix ? "Differential" : "NoFix");
            }
        }

        /// <summary>
        ///     This class represents a GPS information from GPGSA sentences.
        ///     GSA, GNSS DOP and Active Satellites, provides details on the fix, including the numbers of the satellites
        ///     being used and the DOP. At most the first 12 satellite IDs are output.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// </code>
        /// <code language = "VB">
        /// </code>
        /// </example>
        public class GSAData
        {
            /// <summary>
            ///     The GNSS Fix Mode.
            /// </summary>
            public GNSSFixMode Mode { get; internal set; }

            /// <summary>
            ///     GNSS Fix Status
            /// </summary>
            public GNSSFixStatus FixStatus { get; internal set; }

            /// <summary>
            ///     An <see cref="System.Array"/> containing all of the Satellite ID for Satellite 1~12.
            /// </summary>
            public UInt16[] Satellites { get; internal set; }

            /// <summary>
            ///     Position Dilution Of Precision
            /// </summary>
            public Double PDOP { get; internal set; }

            /// <summary>
            ///     Horizontal Dilution Of Precision
            /// </summary>
            public Double HDOP { get; internal set; }

            /// <summary>
            ///     Vertical Dilution Of Precision
            /// </summary>
            public Double VDOP { get; internal set; }

            /// <summary>
            ///     Provides a formatted string for GSA Sentence Data.
            /// </summary>
            /// <returns>The formatted string.</returns>
            public override String ToString()
            {
                return "GNSS Fix Mode: " + (Mode == GNSSFixMode.Auto ? "Auto" : "Manual") + ", GNSS Fix Status: " + (FixStatus == GNSSFixStatus.Fix2D ? "2-D Fix" : FixStatus == GNSSFixStatus.Fix3D ? "3D Fix" : "No Fix") + ", PDOP: " + PDOP + ", HDOP: " + HDOP + ", VDOP: " + VDOP + ", Satellite Count: " + Satellites.Length;
            }

            /// <summary>
            /// Serves as a hash function for a particular type. 
            /// </summary>
            /// <returns>
            /// A hash code for the current object.
            /// </returns>
            public override int GetHashCode()
            {
                Int32 hash = 0;
                if (Satellites.Length == 0) hash = 0;
                if (Satellites.Length > 0) hash = Satellites[0];
                if (Satellites.Length > 1) hash *= Satellites[1];
                if (Satellites.Length > 2) hash *= Satellites[2];
                if (Satellites.Length > 3) hash *= Satellites[3];
                if (Satellites.Length > 4) hash *= Satellites[4];
                if (Satellites.Length > 5) hash *= Satellites[5];
                if (Satellites.Length > 6) hash *= Satellites[6];
                if (Satellites.Length > 7) hash *= Satellites[7];
                if (Satellites.Length > 8) hash *= Satellites[8];
                if (Satellites.Length > 9) hash *= Satellites[9];
                if (Satellites.Length > 10) hash *= Satellites[10];
                if (Satellites.Length > 11) hash *= Satellites[11];
                return hash;
            }
        }

        /// <summary>
        ///     This class represents a GPS information from GPGSA sentences.
        ///     GSV, GNSS Satellites in View. One GSV sentence can only provide data for at most 4 satellites, so
        ///     several sentences might be required for the full information. Since GSV includes satellites that are not
        ///     used as part of the solution, GSV sentence contains more satellites than GGA does.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// </code>
        /// <code language = "VB">
        /// </code>
        /// </example>
        public class GSVData
        {
            /// <summary>
            ///     Number of messages, total number of GPGSV messages being output (1-3)
            /// </summary>
            public Int32 NumberOfMessages { get; internal set; }

            /// <summary>
            ///     Sequence number of this entry (1~3)
            /// </summary>
            public Int32 SequenceNumber { get; internal set; }

            /// <summary>
            ///     Total satellites in view
            /// </summary>
            public Int32 SatellitesInView { get; internal set; }

            /// <summary>
            ///     Satellite ID1
            /// </summary>
            public Satellite SatelliteID1 { get; internal set; }

            /// <summary>
            ///     Satellite ID2
            /// </summary>
            public Satellite SatelliteID2 { get; internal set; }

            /// <summary>
            ///     Satellite ID3
            /// </summary>
            public Satellite SatelliteID3 { get; internal set; }

            /// <summary>
            ///     Satellite ID4
            /// </summary>
            public Satellite SatelliteID4 { get; internal set; }

            /// <summary>
            ///     Provides a formatted string for GSV Sentence Data.
            /// </summary>
            /// <returns>The formatted string.</returns>
            public override String ToString()
            {
                return "Number of Messages: " + NumberOfMessages + ", Sequence Number: " + SequenceNumber +
                       ", Satellites In View: " + SatellitesInView +
                       ", Satellite ID 1: " + SatelliteID1 + ", Satellite ID 2: " + SatelliteID2 +
                       ", Satellite ID 3: " + SatelliteID3 + ", Satellite ID 4: " + SatelliteID4;
            }
        }

        /// <summary>
        ///     This class represents a GPS information from GPGLL sentences.
        ///     GLL, Geographic Latitude and Longitude, contains position information, time of position fix and status.
        /// </summary>
        /// <example>Example Usage:
        /// <code language = "C#">
        /// </code>
        /// <code language = "VB">
        /// </code>
        /// </example>
        public class GLLData
        {
            /// <summary>
            ///     The latitude.
            /// </summary>
            public Double Latitude { get; internal set; }

            /// <summary>
            ///     The Hemisphere (N/S) of the Latitude.
            /// </summary>
            public String LatitudeHemisphere { get; internal set; }

            /// <summary>
            ///     The longitude.
            /// </summary>
            public Double Longitude { get; internal set; }

            /// <summary>
            ///     The Hemisphere (E/W) of the Longitude.
            /// </summary>
            public String LongitudeHemisphere { get; internal set; }

            /// <summary>
            ///     Time in format ‘hhmmss.sss’
            /// </summary>
            public Time UTCTime { get; internal set; }

            /// <summary>
            ///     Valid data received. 
            /// </summary>
            public Boolean DataValid { get; internal set; }

            /// <summary>
            ///     Position Fix acquisition type.
            /// </summary>
            public PositioningModes PositioningMode { get; internal set; }

            /// <summary>
            /// Provides a formatted string for GSV Sentence Data.
            /// </summary>
            public override String ToString()
            {
                return "Latitude: " + Latitude + ", Latitude Hemisphere: " + LatitudeHemisphere + ", Longitude: " +
                       Longitude + ", Longitude Hemisphere: " + LongitudeHemisphere + ", UTC Time: " + UTCTime +
                       ", Positioning Mode: " +
                       (PositioningMode == PositioningModes.AutonomousFix
                           ? "Autonomous"
                           : PositioningMode == PositioningModes.DifferentialFix ? "Differential" : "NoFix");
            }
        }

        #endregion

        #region NMEA Subscription class

        /// <summary>
        /// A class for configuring the NMEA Sentences Subscriptions.
        /// </summary>
        public class NMEASubscriptions
        {
            private bool _gllEnabled;
            private bool _rmcEnabled;
            private bool _vtgEnabled;
            private bool _ggaEnabled;
            private bool _gsaEnabled;
            private bool _gsvEnabled;
            private bool _zdaEnabled;
            private bool _chnEnabled;
            private ushort _gllFixesPerSentence;
            private ushort _rmcFixesPerSentence;
            private ushort _vtgFixesPerSentence;
            private ushort _ggaFixesPerSentence;
            private ushort _gsaFixesPerSentence;
            private ushort _gsvFixesPerSentence;
            private ushort _zdaFixesPerSentence;
            private ushort _chnFixesPerSentence;

            /// <summary>
            /// RMC (Recommended Minimum Position Data)
            /// </summary>
            public Boolean RMCEnabled
            {
                get { return _rmcEnabled; }
                set { _rmcEnabled = value; }
            }

            /// <summary>
            /// VTG (Track Made Good and Ground Speed)
            /// </summary>
            public Boolean VTGEnabled
            {
                get { return _vtgEnabled; }
                set { _vtgEnabled = value; }
            }

            /// <summary>
            /// GSV (GNSS Satellites in View)
            /// </summary>
            public Boolean GGAEnabled
            {
                get { return _ggaEnabled; }
                set { _ggaEnabled = value; }
            }

            /// <summary>
            /// GSA (GNSS DOP and Active Satellites)
            /// </summary>
            public Boolean GSAEnabled
            {
                get { return _gsaEnabled; }
                set { _gsaEnabled = value; }
            }

            
            /// <summary>
            /// GSV (GNSS Satellites in View)
            /// </summary>
            public Boolean GSVEnabled
            {
                get { return _gsvEnabled; }
                set { _gsvEnabled = value; }
            }

            /// <summary>
            /// GLL (Geographic Position – Latitude/Longitude)
            /// </summary>
            public Boolean GLLEnabled
            {
                get { return _gllEnabled; }
                set { _gllEnabled = value; }
            }

            /// <summary>
            /// ZDA (Time and Date)
            /// </summary>
            public Boolean ZDAEnabled
            {
                get { return _zdaEnabled; }
                set { _zdaEnabled = value; }
            }

            /// <summary>
            /// CHN Multi Channel Output data
            /// </summary>
            public Boolean CHNEnabled
            {
                get { return _chnEnabled; }
                set { _chnEnabled = value; }
            }

            /// <summary>
            /// RMC Fixes per output sentence.
            /// </summary>
            public UInt16 RMCFixesPerSentence
            {
                get { return _rmcFixesPerSentence; }
                set { _rmcFixesPerSentence = value; }
            }

            /// <summary>
            /// VTG Fixes per output sentence.
            /// </summary>
            public UInt16 VTGFixesPerSentence
            {
                get { return _vtgFixesPerSentence; }
                set { _vtgFixesPerSentence = value; }
            }

            /// <summary>
            /// GGA Fixes per output sentence.
            /// </summary>
            public UInt16 GGAFixesPerSentence
            {
                get { return _ggaFixesPerSentence; }
                set { _ggaFixesPerSentence = value; }
            }

            /// <summary>
            /// GSA Fixes per output sentence.
            /// </summary>
            public UInt16 GSAFixesPerSentence
            {
                get { return _gsaFixesPerSentence; }
                set { _gsaFixesPerSentence = value; }
            }

            /// <summary>
            /// GSV Fixes per output sentence.
            /// </summary>
            public UInt16 GSVFixesPerSentence
            {
                get { return _gsvFixesPerSentence; }
                set { _gsvFixesPerSentence = value; }
            }

            /// <summary>
            /// GLL Fixes per output sentence.
            /// </summary>
            public UInt16 GLLFixesPerSentence
            {
                get { return _gllFixesPerSentence; }
                set { _gllFixesPerSentence = value; }
            }

            /// <summary>
            /// ZDA Fixes per output sentence.
            /// </summary>
            public UInt16 ZDAFixesPerSentence
            {
                get { return _zdaFixesPerSentence; }
                set { _zdaFixesPerSentence = value; }
            }

            /// <summary>
            /// CHN Fixes per output sentence.
            /// </summary>
            public UInt16 CHNFixesPerSentence
            {
                get { return _chnFixesPerSentence; }
                set { _chnFixesPerSentence = value; }
            }

            /// <summary>
            /// Instantiates a new NMEA Subscriptions Class not subscribing to any NMEA sentence output. 
            /// </summary>
            /// <example>Example Usage:
            /// <code language = "C#">
            /// </code>
            /// <code language = "VB">
            /// </code>
            /// </example>
            public NMEASubscriptions() : this(false, 0, false, 0, false, 0, false, 0, false, 0, false, 0, false, 0, false, 0)
            {}

            /// <summary>
            /// Instantiates a new NMEA Subscriptions Class using the passed parameters. 
            /// </summary>
            /// <param name="gllEnabled">Set to true to enable GLL output as well as setting the gllFixesPerSentence to any value between 1 and 10. Setting to false will disable GLL output regardless of the gllFixesPerSentence setting.</param>
            /// <param name="gllFixesPerSentence">The number of GLL Fixes per output sentence. Valid values are 0 (disabled to 10 fixes per output sentence.</param>
            /// <param name="rmcEnabled">Set to true to enable RMC output as well as setting the rmcFixesPerSentence to any value between 1 and 10. Setting to false will disable RMC output regardless of the rmcFixesPerSentence setting.</param>
            /// <param name="rmcFixesPerSentence">The number of RMC Fixes per output sentence.</param>
            /// <param name="vtgEnabled">Set to true to enable VTG output as well as setting the vtgFixesPerSentence to any value between 1 and 10. Setting to false will disable VTG output regardless of the vtgFixesPerSentence setting.</param>
            /// <param name="vtgFixesPerSentence">The number of VTG Fixes per output sentence.</param>
            /// <param name="ggaEnabled">Set to true to enable GGA output as well as setting the ggaFixesPerSentence to any value between 1 and 10. Setting to false will disable GGA output regardless of the ggaFixesPerSentence setting.</param>
            /// <param name="ggaFixesPerSentence">The number of GGA Fixes per output sentence.</param>
            /// <param name="gsaEnabled">Set to true to enable GSA output as well as setting the gsaFixesPerSentence to any value between 1 and 10. Setting to false will disable GSA output regardless of the gsaFixesPerSentence setting.</param>
            /// <param name="gsaFixesPerSentence">The number of GSA Fixes per output sentence.</param>
            /// <param name="gsvEnabled">Set to true to enable GSV output as well as setting the gsvFixesPerSentence to any value between 1 and 10. Setting to false will disable GSV output regardless of the gsvFixesPerSentence setting.</param>
            /// <param name="gsvFixesPerSentence">The number of GSV Fixes per output sentence.</param>
            /// <param name="zdaEnabled">Set to true to enable ZDA output as well as setting the gsaFixesPerSentence to any value between 1 and 10. Setting to false will disable ZDA output regardless of the zdaFixesPerSentence setting.</param>
            /// <param name="zdaFixesPerSentence">The number of ZDA Fixes per output sentence.</param>
            /// <param name="chnEnabled">Set to true to enable CHN output as well as setting the chnFixesPerSentence to any value between 1 and 10. Setting to false will disable CHN output regardless of the chnFixesPerSentence setting.</param>
            /// <param name="chnFixesPerSentence">The number of CHN Fixes per output sentence.</param>
            /// <example>Example Usage:
            /// <code language = "C#">
            /// </code>
            /// <code language = "VB">
            /// </code>
            /// </example>
            public NMEASubscriptions(bool gllEnabled, ushort gllFixesPerSentence, bool rmcEnabled, ushort rmcFixesPerSentence, bool vtgEnabled, ushort vtgFixesPerSentence, bool ggaEnabled, ushort ggaFixesPerSentence, bool gsaEnabled, ushort gsaFixesPerSentence, bool gsvEnabled, ushort gsvFixesPerSentence, bool zdaEnabled, ushort zdaFixesPerSentence, bool chnEnabled, ushort chnFixesPerSentence)
            {
                var fixesPerSentence = new[] { gllFixesPerSentence, rmcFixesPerSentence, vtgFixesPerSentence, ggaFixesPerSentence, gsaFixesPerSentence, gsvFixesPerSentence, zdaFixesPerSentence, chnFixesPerSentence };

                for (Int16 x = 0; x == fixesPerSentence.Length - 1; x++)
                {
                    if (fixesPerSentence[x] > 10) fixesPerSentence[x] = 10;
                }
                _gllEnabled = gllEnabled;
                _gllFixesPerSentence = fixesPerSentence[0];
                _rmcEnabled = rmcEnabled;
                _rmcFixesPerSentence = fixesPerSentence[1];
                _vtgEnabled = vtgEnabled;
                _vtgFixesPerSentence = fixesPerSentence[2];
                _ggaEnabled = ggaEnabled;
                _ggaFixesPerSentence = fixesPerSentence[3];
                _gsaEnabled = gsaEnabled;
                _gsaFixesPerSentence = fixesPerSentence[4];
                _gsvEnabled = gsvEnabled;
                _gsvFixesPerSentence = fixesPerSentence[5];
                _zdaEnabled = zdaEnabled;
                _zdaFixesPerSentence = fixesPerSentence[6];
                _chnEnabled = chnEnabled;
                _chnFixesPerSentence = fixesPerSentence[7];
            }

            static internal NMEASubscriptions FromNMEARespoonse(String sentence)
            {
                var tokens = sentence.Split(',');
                var t = new NMEASubscriptions
                {
                    GLLEnabled = tokens[1] != "0",
                    GLLFixesPerSentence = UInt16.Parse(tokens[1]),
                    RMCEnabled = tokens[2] != "0",
                    RMCFixesPerSentence = UInt16.Parse(tokens[2]),
                    VTGEnabled = tokens[3] != "0",
                    VTGFixesPerSentence = UInt16.Parse(tokens[3]),
                    GGAEnabled = tokens[4] != "0",
                    GGAFixesPerSentence = UInt16.Parse(tokens[4]),
                    GSAEnabled = tokens[5] != "0",
                    GSAFixesPerSentence = UInt16.Parse(tokens[5]),
                    GSVEnabled = tokens[6] != "0",
                    GSVFixesPerSentence = UInt16.Parse(tokens[6]),
                    ZDAEnabled = tokens[7] != "0",
                    ZDAFixesPerSentence = UInt16.Parse(tokens[7]),
                    CHNEnabled = tokens[8] != "0",
                    CHNFixesPerSentence = UInt16.Parse(tokens[8])
                };
                return t;
            }

            /// <summary>
            /// Returns a string that represents the current NMEAsubscriptions class.
            /// </summary>
            /// <returns>
            /// A string that represents the current  NMEAsubscriptions class.
            /// </returns>
            public override string ToString()
            {
                var tempString = "$PMTK314,";
                tempString += (_gllEnabled ? _gllFixesPerSentence : 0) + ",";
                tempString += (_rmcEnabled ? _rmcFixesPerSentence : 0) + ",";
                tempString += (_vtgEnabled ? _vtgFixesPerSentence : 0) + ",";
                tempString += (_ggaEnabled ? _ggaFixesPerSentence : 0) + ",";
                tempString += (_gsaEnabled ? _gsaFixesPerSentence : 0) + ",";
                tempString += (_gsvEnabled ? _gsvFixesPerSentence : 0) + ",";
                tempString += "0,0,0,0,0,0,0,0,0,0,0,";
                tempString += (_zdaEnabled ? _zdaFixesPerSentence : 0) + ",";
                tempString += _chnEnabled ? _chnFixesPerSentence : 0;
                return tempString;
            }
        }

        #endregion

        #region Structures

        #region Date and Time Structures

        /// <summary>
        ///     A structure containing the Time information that is used for RMC and GLL UTCTime.
        /// </summary>
        public struct Time
        {
            /// <summary>
            ///     Default constructor for the Time structure.
            /// </summary>
            /// <param name="milliSeconds">The milliseconds of the <see cref="Time" /> Structure.</param>
            /// <param name="seconds">The seconds of the <see cref="Time" /> Structure.</param>
            /// <param name="minutes">The minutes of the <see cref="Time" /> Structure.</param>
            /// <param name="hours">The hours of the <see cref="Time" /> Structure.</param>
            public Time(Int32 hours, Int32 minutes, Int32 seconds, Int32 milliSeconds)
                : this()
            {
                Milliseconds = milliSeconds;
                Seconds = seconds;
                Minutes = minutes;
                Hours = hours;
            }

            /// <summary>
            ///     The  time in milliseconds.
            /// </summary>
            public Int32 Milliseconds { get; set; }

            /// <summary>
            ///     The  time in Seconds.
            /// </summary>
            public Int32 Seconds { get; set; }

            /// <summary>
            ///     The  time in Minutes.
            /// </summary>
            public Int32 Minutes { get; set; }

            /// <summary>
            ///     The  time in Hours.
            /// </summary>
            public Int32 Hours { get; set; }

            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            /// <returns>
            /// A string that represents the current object.
            /// </returns>
            /// <filterpriority>2</filterpriority>
            public override String ToString()
            {
                return Hours + ":" + Minutes + ":" + Seconds + "." + Milliseconds;
            }
        }

        /// <summary>
        ///     A structure containing the Date information that is used for RMC and GLL UTCTime.
        /// </summary>
        public struct Date
        {
            /// <summary>
            ///     Default constructor for the Date structure.
            /// </summary>
            /// <param name="day">The day of the <see cref="Date" /> Structure.</param>
            /// <param name="month">The month of the <see cref="Date" /> Structure.</param>
            /// <param name="year">The year of the <see cref="Date" /> Structure.</param>
            public Date(Int32 year, Int32 month, Int32 day)
                : this()
            {
                Year = year;
                Month = month;
                Day = day;
            }

            /// <summary>
            ///     The Days of the Date Structure.
            /// </summary>
            public Int32 Day { get; set; }

            /// <summary>
            ///     The  Month of the Date Structure.
            /// </summary>
            public Int32 Month { get; set; }

            /// <summary>
            ///     The  Year of the Date Structure.
            /// </summary>
            public Int32 Year { get; set; }

            /// <summary>
            /// Indicates whether this instance and a specified object are equal.
            /// </summary>
            /// <returns>
            /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. 
            /// </returns>
            /// <param name="obj">The object to compare with the current instance. </param><filterpriority>2</filterpriority>
            public override bool Equals(object obj)
            {
                return base.Equals(obj) && Month == ((Date)obj).Month && Equals(obj) && Day == ((Date)obj).Month && Equals(obj) && Year == ((Date)obj).Year;
            }

            /// <summary>
            /// Serves as a hash function for a particular type. 
            /// </summary>
            /// <returns>
            /// A hash code for the current object.
            /// </returns>
            /// <filterpriority>2</filterpriority>
            public override Int32 GetHashCode()
            {
                return Year * Month * Day;

            }

            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            /// <returns>
            /// A string that represents the current object.
            /// </returns>
            /// <filterpriority>2</filterpriority>
            public override String ToString()
            {
                return Month + "-" + Day + "-" + Year;
            }
        }

        #endregion

        #region Satellites Structure

        /// <summary>
        /// A structure containing necessary information to identify a Satellite.
        /// </summary>
        public struct Satellite
        {
            /// <summary>
            /// The heading of the horizon measured from true north and clockwise from the point where a vertical circle through a satellite intersects the horizon. 
            /// </summary>
            public UInt16 Azimuth;
            /// <summary>
            /// The height in heading above the horizon of the satellite position. 
            /// </summary>
            public UInt16 Elevation;
            /// <summary>
            /// The satellite's PRN Number (Pseudo-Random-Noise) used to identify a satellite.
            /// </summary>
            public UInt16 PRNNumber;
            /// <summary>
            /// Signal to Noise Ratio.
            /// </summary>
            public UInt16 SignalNoiseRatio;

            /// <summary>
            /// Instantiates a new Satellite structure.
            /// </summary>
            /// <param name="PRNNumber">The satellite's PRN Number (Pseudo-Random-Noise)</param>
            /// <param name="Elevation">The height in heading above the horizon of the satellite position.</param>
            /// <param name="Azimuth">The heading of the horizon measured from true north and clockwise from the point where a vertical circle through a satellite intersects the horizon. </param>
            /// <param name="SignalNoiseRatio">Signal to Noise Ratio.</param>
            public Satellite(UInt16 PRNNumber, UInt16 Elevation, UInt16 Azimuth, UInt16 SignalNoiseRatio)
            {
                this.PRNNumber = PRNNumber;
                this.Elevation = Elevation;
                this.Azimuth = Azimuth;
                this.SignalNoiseRatio = SignalNoiseRatio;
            }

            /// <summary>
            /// Returns a string that represents the current Satellite object.
            /// </summary>
            /// <returns>
            /// A string that represents the current Satellite object.
            /// </returns>
            public override String ToString()
            {
                return "Satellite PRN: " + PRNNumber + ", Elevation: " + Elevation + ", Azimuth: " + Azimuth + ", Signal To Noise Ratio: " + SignalNoiseRatio;
            }
        }

        #endregion

        #endregion

    }
}