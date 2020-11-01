/*
 * Wifi driver skeleton generated on 12/10/2014 10:18:19 PM
 * 
 * Initial revision coded by Christophe
 * 
 * References needed :  (change according to your driver's needs)
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Native
 *  MikroBusNet
 *  mscorlib
 *  
 * Copyright 2014 Christophe
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 * 
 * Follow the TODO tasks to complete the driver code and remove this line
 */


using System;
using System.Collections;
using System.IO.Ports;
using System.Reflection;
using System.Text;
using System.Threading;
using MBN.Enums;
using MBN.Exceptions;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the Wifi driver
    /// <para><b>Pins used :</b> Miso, Mosi, Cs, Sck</para>
    /// </summary>
    public partial class Wifi : IDriver
    {
        private class ConnectionProfile
        {
            public readonly Byte Num;
            public String SSID { get; private set; }
            public SecurityModes SecurityMode { get; private set; }
            public Byte[] SecurityKey { get; private set; }
            public Byte Channel { get; private set; }
            public Byte KeyIndex { get; private set; }
            public Byte[] BinaryKey { get; set; }

            public ConnectionProfile(String pSSID, SecurityModes pSecurityMode, Byte[] pKey = null, Byte pKeyIndex = 0, Byte pChannel = 0)
            {
                if (_profilesCount == 2) { throw new InvalidOperationException(); }
                Num = _profilesCount;
                SSID = pSSID;
                SecurityMode = pSecurityMode;
                SecurityKey = pKey;
                KeyIndex = pKeyIndex;
                Channel = pChannel;

                _profiles[_profilesCount] = this;
                _profilesCount++;
            }
        }

        // This is a demo event fired by the driver. Implementation is in the Christophe.Events namespace below.
        /// <summary>
        /// Occurs when a demo event is detected.
        /// </summary>
        public event DemoEventEventHandler DemoEventFired = delegate { };

        private readonly SerialPort _sp;
        private readonly ArrayList _receivedData;
        private readonly Queue _Data;
        private readonly OutputPort _resetPort;
        private String _lastCmd;
        private Boolean _firstTime, _chipReady;
        private Boolean _buffering = true;
        private static Byte _profilesCount;
        private static ConnectionProfile[] _profiles;
        private Int32 _messageType;
        private Byte _eventType;
        private const NetworkModes NetworkMode = NetworkModes.Infrastructure; // No Ad-Hoc yet
        private object _lock;
        private Queue _commands;
        private Byte[] _cmd;
        private Boolean _piggyBack;
        private Int32 _defaultTimeOut=5000;
        private Int32 _cmdTimeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="Wifi"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the Wifi module is plugged on MikroBus.Net board</param>
        public Wifi(Hardware.Socket socket)
        {
            Hardware.CheckPins(socket, socket.Tx, socket.Rx, socket.Rst);

            //CurrentProfile = 0;
            _profilesCount = 0;
            _profiles = new ConnectionProfile[2];
            
            //_error = 0;
            //_nStat = 0;
            //Hibernating = false;
            _chipReady = false;
            _receivedData = new ArrayList();
            _firstTime = true;
            _commands = new Queue();
            _sp = new SerialPort(socket.ComPort, 115200, Parity.None, 8, StopBits.Two);
            _sp.Open();
            _sp.DataReceived += sp_DataReceived;

            _resetPort = new OutputPort(socket.Rst, true);
            Thread.Sleep(10);
            _resetPort.Write(false);
            Thread.Sleep(10);
            _resetPort.Write(true);
        }

        public void CreateProfile(String ssid, SecurityModes securityMode, Byte[] key = null, Byte keyIndex = 0, Byte channel = 0)
        {
            if ((key == null || key.Length == 0) && securityMode != SecurityModes.Open) { throw new ArgumentException("Invalid profile informations"); }
            var pf = new ConnectionProfile(ssid, securityMode, key, keyIndex, channel);
            if (_profilesCount == 1) { Reset(ResetModes.Hard); }
            Init(pf.Num);
        }

        /// <summary>
        /// This is a sample method used by the <see cref="Wifi"/> module. It raises an event.
        /// </summary>
        public void MethodWithEvent()
        {
            // A temporary event is used to avoid any race condition
            var tempEvent = DemoEventFired;
            tempEvent(this, new DemoEventEventArgs(1, false, true));
        }

        public Int32 DefaultTimeOut
        {
            get { return _defaultTimeOut; }
            set { _defaultTimeOut = value; }
        }

        public void SetNetworkMode(NetworkModes mode=NetworkModes.Infrastructure, Byte profile=0 )
        {
            _lastCmd = "SetNetworkMode";
            SendWait(new Byte[] { 0x55, 0xAA, 0x37, 0x00, 0x02, 0x00, (Byte)(profile + 1), mode == NetworkModes.Infrastructure ? (Byte)0x01 : (Byte)0x02, 0x45 });
        }

        public void SetProfileSSID(String ssid, Byte profile = 0)
        {
            _lastCmd = "SetProfileSSID";
            SendRawData(new Byte[] { 0x55, 0xAA, 0x39, 0x00, (Byte)(ssid.Length + 2), 0x00, (Byte)(profile + 1), (Byte)ssid.Length });
            SendRawData(Encoding.UTF8.GetBytes(ssid));
            SendWait(new Byte[] { 0x45 });
        }

        private void Init(Byte connectionProfile)
        {
            SetNetworkMode();   // Default values used : profile #0 and mode Infrastructure
            SetProfileSSID(_profiles[connectionProfile].SSID);
            /*
            SendRawData(new Byte[] { 0x55, 0xAA, 0x39, 0x00, (Byte)(_profiles[connectionProfile].SSID.Length + 2), 0x00, (Byte)(connectionProfile + 1), (Byte)_profiles[connectionProfile].SSID.Length });
            SendRawData(Encoding.UTF8.GetBytes(_profiles[connectionProfile].SSID));
            SendRawData(new Byte[] { 0x45 });
             */
            switch (_profiles[connectionProfile].SecurityMode)
            {
                case SecurityModes.Open:
                    SendRawData(new Byte[] { 0x55, 0xAA, 0x41, 0x00, 0x02, 0x00, (Byte)(connectionProfile + 1), 0x00, 0x45 });
                    break;
                case SecurityModes.WEP104:
                case SecurityModes.WEP40:
                    if (_profiles[connectionProfile].SecurityMode == SecurityModes.WEP40 && _profiles[connectionProfile].SecurityKey.Length != 5) { throw new ArgumentException(); }
                    if (_profiles[connectionProfile].SecurityMode == SecurityModes.WEP104 && _profiles[connectionProfile].SecurityKey.Length != 13) { throw new ArgumentException(); }
                    SendRawData(new Byte[] { 0x55, 0xAA, (Byte)(_profiles[connectionProfile].SecurityMode == SecurityModes.WEP40 ? 0x42 : 0x43), 0x00, (Byte)(_profiles[connectionProfile].SecurityMode == SecurityModes.WEP40 ? 0x18 : 0x38), 0x00, (Byte)(connectionProfile + 1), 0x00, _profiles[connectionProfile].KeyIndex, 0x00 });
                    SendRawData(_profiles[connectionProfile].SecurityKey);
                    SendRawData(_profiles[connectionProfile].SecurityKey);
                    SendRawData(_profiles[connectionProfile].SecurityKey);
                    SendRawData(_profiles[connectionProfile].SecurityKey);
                    SendRawData(new Byte[] { 0x45 });
                    break;
                case SecurityModes.WPA:
                case SecurityModes.WPA2:
                    if (_profiles[connectionProfile].BinaryKey != null && _profiles[connectionProfile].BinaryKey.Length > 0)
                    {
                        SendRawData(new Byte[] { 0x55, 0xAA, 0x44, 0x00, 0x24, 0x00, (Byte)(connectionProfile + 1), (Byte)(_profiles[connectionProfile].SecurityMode == SecurityModes.WPA ? 0x03 : 0x05), 0x00, 0x20 });
                        SendRawData(_profiles[connectionProfile].BinaryKey);
                    }
                    else
                    {
                        SendRawData(new Byte[] { 0x55, 0xAA, 0x44, 0x00, (Byte)(4 + _profiles[connectionProfile].SecurityKey.Length), 0x00, (Byte)(connectionProfile + 1), (Byte)(_profiles[connectionProfile].SecurityMode == SecurityModes.WPA ? 0x04 : 0x06), 0x00, (Byte)_profiles[connectionProfile].SecurityKey.Length });
                        SendRawData(_profiles[connectionProfile].SecurityKey);
                    }
                    SendRawData(new Byte[] { 0x45 });
                    break;
            }
            // User defined channel. When set to 0, it scans channels 1, 6 and 11 for Infrastructure and 6 for Ad-Hoc
            if (_profiles[connectionProfile].Channel != 0) { SendRawData(new Byte[] { 0x55, 0xAA, 0x3A, 0x00, 0x03, 0x00, 0x01, 0x00, _profiles[connectionProfile].Channel, 0x45 }); }
        }

        private void SendRawData(Byte[] data)
        {
            _sp.Write(data, 0, data.Length);
            Thread.Sleep(50);
        }

        private void SendWait(Byte[] data)
        {
            _cmdTimeout = 0;
            _piggyBack = false;
            _sp.Write(data, 0, data.Length);
            while (!_piggyBack)
            {
                Thread.Sleep(50);
                _cmdTimeout += 50;
                if (_cmdTimeout > _defaultTimeOut) { throw  new Exception("Timeout : "+_lastCmd);}
            }
        }

        private void ParseBuffer(ArrayList pBuffer)
        {
            var bArray = new Byte[pBuffer.Count];
            pBuffer.CopyTo(bArray);
            _messageType = (bArray[3] << 8) + bArray[2];
            switch (_messageType)
            {
                case 0: // ACK
                    Debug.Print("ACK");
                    break;
                case 0x8000: // ACK + PiggyBack
                    Debug.Print("ACK_PIGGYBACK, last command : "+_lastCmd);
                    _lastCmd = "";
                    _piggyBack = true;
                    break;
                case 1: // EVENT
                    _eventType = bArray[6];
                    switch (_eventType)
                    {
                            /*
                        case 8:  // Connection status changed
                            Connected = bArray[7] == 1;
                            if (bArray[7] == 2) { _profiles[CurrentProfile].BinaryKey = null; }
                            if (ConnectionStatusChanged != null) { ConnectionStatusChanged(this, new Events.ConnectionStatusChangedEventArgs(bArray[7], bArray[8])); }
                            break;
                        case 9: // Scan done
                            _nbNetworks = bArray[7];
                            _scanDone = true;
                            break;
                        case 16:  // IP address assigned by DHCP
                            GetNetworkStatus();
                            if (Connected && AddressAssigned != null) { AddressAssigned(this, new Events.AddressAssignedEventArgs(bArray)); }
                            break;
                        case 26:   // Ping response
                            if (PingResponse != null) { PingResponse(this, new Events.PingResponseEventArgs(bArray[7] == 0, bArray[8])); }
                            break;
                             */
                        case 27:  // STARTUP_EVENT
                            Debug.Print("StartupEvent");
                            _chipReady = true;
                            break;
                        case 255:  // ERROR_EVENT
                            Debug.Print("Erreur : " + bArray[8]);
                            //if (ErrorOccured != null) { ErrorOccured(this, new Events.ErrorEventArgs(bArray[8])); }
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                    break;
                    /*
                case 22:  // Scan results
                    var res = new ScanResults { BSSID = BitConverter.ToString(bArray, 6, 6), SSID = String.Empty };
                    for (Byte i = 0; i < bArray[12]; i++)
                    {
                        res.SSID = String.Concat(res.SSID, (Char)bArray[13 + i]);
                    }
                    res.BSSType = (NetworkModes)bArray[61];
                    res.APConfig = bArray[45];
                    res.BeaconPeriod = BytesToInt16(bArray[46], bArray[47]);
                    res.ATIMWindow = BytesToInt16(bArray[48], bArray[49]);
                    res.BasicRatesListLength = bArray[59];
                    res.BasicRates = new Byte[res.BasicRatesListLength];
                    for (Byte i = 0; i < res.BasicRatesListLength; i++) { res.BasicRates[i] = bArray[50 + i]; }
                    res.DTIMPeriod = bArray[60];
                    res.Channel = bArray[62];
                    _scanRes.Add(res);
                    if (_scanRes.Count == _nbNetworks)
                    {
                        if (NetwordScanDone != null) { NetwordScanDone(this, new Events.NetworkScannedEventArgs(_scanRes)); }
                    }
                    break;
                case 23:   // Create socket
                    if (bArray[6] <= 253 && SocketCreated != null)
                    {
                        SocketCreated(this, new Events.CreateSocketEventArgs(bArray[6]));
                    }
                    break;
                case 24:   // Bind socket
                    if (bArray[8] == 0 && SocketBound != null)
                    {
                        SocketBound(this, new Events.SocketBoundEventArgs((Int16)(bArray[6] + (bArray[7] << 8 & 0xFF00))));
                    }
                    break;
                case 25:   // Socket connect
                    _sockConnected = bArray[6] == 0x00;
                    if (SocketConnected != null) { SocketConnected(this, new Events.SocketConnectedEventArgs(bArray[6])); }
                    break;
                case 26:   // Socket listen
                    _sockConnected = bArray[6] == 0x00;
                    if (SocketConnected != null) { SocketConnected(this, new Events.SocketConnectedEventArgs(bArray[6])); }
                    break;
                case 27:   // Socket accept
                    if (bArray[6] != 0xFF)
                    {
                        SocketRemotePort = BytesToInt16(bArray[8], bArray[9]);
                        for (Byte i = 0; i < 4; i++) { SocketRemoteIP[i] = bArray[10 + i]; }
                        if (SocketConnected != null) { SocketConnected(this, new Events.SocketConnectedEventArgs(bArray[6])); }
                    }
                    break;
                case 28:   // Socket send response
                    if (SocketDataSent != null) { SocketDataSent(this, new Events.SocketDataSentEventArgs((Int16)(bArray[6] + (bArray[7] << 8 & 0xFF00)))); }
                    break;
                case 29:   // Socket receive response
                    Int16 bytesToRead = BytesToInt16(bArray[8], bArray[9]);
                    if (bytesToRead == 0) { break; }
                    var data = new Byte[bytesToRead];
                    for (int i = 0; i < bytesToRead; i++) { data[i] = bArray[10 + i]; }
                    if (SocketDataReceived != null)
                    {
                        SocketDataReceived(this, new Events.SocketDataReceivedEventArgs(bArray[6], bytesToRead, data));
                    }
                    break;
                case 48:   // Get network status
                    _nStat = bArray[61];
                    Connected = (_nStat == 1 || _nStat == 3);
                    if (NetworkStatus != null) { NetworkStatus(this, new Events.NetworkStatusEventArgs(bArray)); }
                    break;
                case 49:   // Get WPA/WPA2 binary key
                    _profiles[CurrentProfile].BinaryKey = new Byte[32];
                    for (int i = 6; i < bArray.Length - 1; i++) { _profiles[CurrentProfile].BinaryKey[i - 6] = bArray[i]; }
                    if (BinaryKeyCalculated != null) { BinaryKeyCalculated(this, new Events.BinaryKeyCalculatedEventArgs(_profiles[CurrentProfile].BinaryKey)); }

                    break;
                case 50:    // GPIO response
                    if (GPIOEvent != null) { GPIOEvent(this, new Events.GPIOEventArgs(bArray[6], bArray[7])); }
                    break;
                     */
                default:
                    Debug.Print("Message type : "+_messageType);
                    //throw new InvalidOperationException(_messageType.ToString());
                    break;
            }
            pBuffer.Clear();
        }

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var nb = _sp.BytesToRead;
            var buf = new Byte[nb];

            _sp.Read(buf, 0, nb);
            /*
            if (_lastCmd == "Receive")  // Receive event
            {
                if (_firstTime)
                {
                    _receivedData.Clear();
                    _firstTime = false;
                }
                for (Byte i = 0; i < nb; i++) { _receivedData.Add(buf[i]); }
                if (_receivedData.Count > 10)
                {
                    var bytesToRead = (Int16)((Byte)_receivedData[8] + ((Byte)_receivedData[9] << 8 & 0xFF00));
                    if (_receivedData.Count == bytesToRead + 11)
                    {
                        ParseBuffer(_receivedData);
                        _receivedData.Clear();
                        _lastCmd = "";
                        _firstTime = true;
                    }
                }
            }
            else
             */
            {
                var depart = 0;
                if (buf[0] == 0 && buf.Length == 2)
                {
                    if (buf[0] == 0 && buf[1] == 0x55)   // Incomplete Startup event sequence
                    {
                        _buffering = true;
                        _receivedData.Clear();
                        _receivedData.Add((Byte)0x55);
                        depart = 2;
                    }
                }
                if (buf[0] == 0 && buf.Length > 2)
                {
                    if (buf[0] == 0 && buf[1] == 0x55 && buf[2] == 0xAA)   // Startup event
                    {
                        _buffering = true;
                        _receivedData.Clear();
                        _receivedData.Add((Byte)0x55);
                        _receivedData.Add((Byte)0xAA);
                        depart = 3;
                    }
                }
                if (buf.Length > 1)
                {
                    if (buf[0] == 0x55 && buf[1] == 0xAA)  // Normal event
                    {
                        _buffering = true;
                        _receivedData.Clear();
                        _receivedData.Add((Byte)0x55);
                        _receivedData.Add((Byte)0xAA);
                        depart = 2;
                    }


                    for (var i = depart; i < nb; i++)
                    {
                        var c = buf[i];
                        if (_buffering && c != 0x45) { _receivedData.Add(c); }
                        if (c == 0x45 && _buffering)
                        {
                            _buffering = false;
                            _receivedData.Add(c);
                            ParseBuffer(_receivedData);
                            _receivedData.Clear();
                            _lastCmd = "";
                        }
                    }
                }
                else
                {
                    _buffering = true;
                    _receivedData.Clear();
                    _receivedData.Add(buf[0]);
                }
            }
        }

        #region IDriver implementation
        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <example> This sample shows how to use the PowerMode property.
        /// <code language="C#">
        ///             _Wifi.PowerMode = PowerModes.Off;
        /// </code>
        /// </example>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">Thrown if the property is set and the module doesn't support power modes.</exception>
        public PowerModes PowerMode
        {
            get { return PowerModes.On; }
            set
            {
                throw new NotImplementedException("PowerModes");
            }
        }

        /// <summary>
        /// Gets the driver version.
        /// </summary>
        /// <example> This sample shows how to use the DriverVersion property.
        /// <code language="C#">
        ///             Debug.Print ("Current driver version : "+_Wifi.DriverVersion);
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
            _chipReady = false;
            _resetPort.Write(false); 
            Thread.Sleep(200); 
            _resetPort.Write(true);
            do { Thread.Sleep(20); } while (!_chipReady);
            
            return true;
        }
        #endregion
    }
}

