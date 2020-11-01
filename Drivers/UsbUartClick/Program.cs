using System;
using System.IO.Ports;
using MBN.Modules;
using Microsoft.SPOT;
using MBN;
using System.Threading;

namespace Examples
{
    public class Program
    {
        private static USBUARTClick _usbUart;

        public static void Main()
        {

            _usbUart = new USBUARTClick(Hardware.SocketFour, USBUARTClick.BaudRate.Baud921600, Handshake.None) { GlitchFilterTime = new TimeSpan(0, 0, 0, 0, 50) };

            _usbUart.DataReceived += USBUartUSBUARTDataReceived;
            _usbUart.CableConnectionChanged += USBUartUSBUARTCableConnectionChanged;
            _usbUart.SleepSuspend += USBUARTUartUSBUARTSleepSuspend;

            Debug.Print("USB is connected ? " + _usbUart.USBCableConnected);
            Debug.Print("USB sleeping ? " + _usbUart.USBSuspended);

            Thread.Sleep(Timeout.Infinite);
        }

        static void USBUARTUartUSBUARTSleepSuspend(object sender, bool isSleeping, DateTime eventTime)
        {
            Debug.Print("USB " + (isSleeping ? "Suspend/Sleep" : "Wake") + " event occurred at " + eventTime);
        }

        static void USBUartUSBUARTCableConnectionChanged(object sender, bool cableConnected, DateTime eventTime)
        {
            Debug.Print("Cable " + (cableConnected ? "connection" : "dis-connection") + " event occurred at " + eventTime);
        }

        static void USBUartUSBUARTDataReceived(object sender, string message, DateTime eventTime)
        {
            // Echo back to sender
            _usbUart.SendData("Received your message of - \"" + message + "\" at " + eventTime);
            Debug.Print(message);
        }
    }
}
