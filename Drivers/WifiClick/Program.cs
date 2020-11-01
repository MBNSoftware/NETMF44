using System;
using System.Text;
using System.Threading;
using MBN.Modules;
using Microsoft.SPOT;
using MBN;

namespace Examples
{
    public class Program
    {
        static WifiClick _wifi;
        static Byte _handle1;
// ReSharper disable once NotAccessedField.Local
        static String _myIP;

        public static void Main()
        {
            _wifi = new WifiClick(Hardware.SocketTwo);
            _wifi.CreateProfile("baf", WifiClick.SecurityModes.WEP40, new Byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE });

            _wifi.AddressAssigned += wifi_AddressAssigned;
            _wifi.PingResponse += wifi_PingResponse;
            _wifi.ConnectionStatusChanged += wifi_ConnectionStatusChanged;
            _wifi.SocketConnected += wifi_SocketConnected;
            _wifi.SocketDataSent += wifi_SocketDataSent;
            _wifi.SocketDataReceived += wifi_SocketDataReceived;
            _wifi.NetworkStatus += wifi_NetworkStatus;
            _wifi.SocketCreated += wifi_SocketCreated;
            _wifi.SocketBound += wifi_SocketBound;
            _wifi.NetwordScanDone += wifi_NetwordScanDone;

            ConnectDHCP();   
            //wifi.ConnectStaticIP("192.168.1.4", "255.255.255.0", "192.168.0.235");
            TestSocket();

            Thread.Sleep(Timeout.Infinite);
        }

        private static void ConnectDHCP()
        {
            _wifi.ConnectDHCP();
            Thread.Sleep(2000);
            Debug.Print("LAN ping");
            _wifi.Ping("192.168.0.6");
            Thread.Sleep(5000);
            Debug.Print("WAN ping");
            _wifi.Ping("8.8.8.8");
        }

        private static void TestSocket()
        {
            _wifi.SocketCreate(WifiClick.SocketType.TCP);
            Debug.Print("Connecting to TCP server @192.168.0.6:4001");
            _wifi.SocketConnect(_handle1, 4001, "192.168.0.6");
            _wifi.StartReceiving(_handle1);
        }

        static void wifi_SocketDataReceived(object sender, WifiClick.Events.SocketDataReceivedEventArgs e)
        {
            Debug.Print("Message from server : " + new String(Encoding.UTF8.GetChars(e.Data)));

            _wifi.SocketSend(_handle1, Encoding.UTF8.GetBytes("Message received OK."));
        }

/*
        private static void TestGPIO()
        {
            Debug.Print("GPIO dans 1 sec");
            Thread.Sleep(1000);
            _wifi.GPIOWrite(1, true);
            _wifi.GPIOWrite(1, false);
            Debug.Print("GPIO 2 as input");
            _wifi.GPIORead(2);
            Thread.Sleep(1000);
            Debug.Print("GPIO 2 true");
            _wifi.GPIOWrite(2, true);
            _wifi.GPIORead(2);
            Thread.Sleep(1000);
            Debug.Print("GPIO 2 false");
            _wifi.GPIOWrite(2, false);
            _wifi.GPIORead(2);
        }
*/

/*
        static void wifi_GPIOEvent(object sender, WifiClick.Events.GPIOEventArgs e)
        {
            Debug.Print("GPIO1, index = " + e.Index.ToString() + ", value = " + e.Value.ToString());
        }

        static void GPIO1_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            Debug.Print("GPIO1, data1 = " + data1.ToString() + ", data2 = " + data2.ToString());
        }
*/

        static void wifi_NetwordScanDone(object sender, WifiClick.Events.NetworkScannedEventArgs e)
        {
            Debug.Print("Scan done");
            Debug.Print(e.ScanResult != null ? "Networks detected" : "No network detected");
        }

        static void wifi_SocketDataSent(object sender, WifiClick.Events.SocketDataSentEventArgs e)
        {
            Debug.Print("Socket data sent : " + e.BytesCount + " bytes sent");
        }

        static void wifi_SocketConnected(object sender, WifiClick.Events.SocketConnectedEventArgs e)
        {
            Debug.Print("Socket connected, status = " + (e.Status == 0 ? "connected" : "connecting"));
        }

        static void wifi_SocketBound(object sender, WifiClick.Events.SocketBoundEventArgs e)
        {
            Debug.Print("Socket bound to port " + e.Port);
        }

        static void wifi_SocketCreated(object sender, WifiClick.Events.CreateSocketEventArgs e)
        {
            _handle1 = e.Handle;
        }

        static void wifi_ConnectionStatusChanged(object sender, WifiClick.Events.ConnectionStatusChangedEventArgs e)
        {
            Debug.Print("Connection status changed : " + e.Status + ", " + e.Data);
        }

        static void wifi_PingResponse(object sender, WifiClick.Events.PingResponseEventArgs e)
        {
            Debug.Print("Ping answer : " + e.Ok + ", " + e.Time + " ms");
        }

        static void wifi_AddressAssigned(object sender, WifiClick.Events.AddressAssignedEventArgs e)
        {
            Debug.Print("IP address assigned : " + e);
            _myIP = e.ToString();
        }

        static void wifi_NetworkStatus(object sender, WifiClick.Events.NetworkStatusEventArgs e)
        {
            //if (e.Status == WifiClick.NetStatus.ConnectedDHCP || e.Status == WifiClick.NetStatus.ConnectedStaticIP)
            {
                Debug.Print("MAC = " + e.MAC);
                Debug.Print("IP = " + e.IP);
                Debug.Print("Mask = " + e.NetMask);
                Debug.Print("Gateway = " + e.Gateway);
                Debug.Print("Status = " + e.Status);
            }
        }
    }
}
