using System;
using System.Text;
using MBN.Modules;
using MBN;
using MBN.Exceptions;
using System.Threading;
using Microsoft.SPOT;

namespace Examples
{
    public class Program
    {
        private static FTDIClick _ftdi;

        public static void Main()
        {
            try
            {
                _ftdi = new FTDIClick(Hardware.SocketOne);

                _ftdi.DataReceived += _ftdi_DataReceived;
                _ftdi.Listening = true;

                _ftdi.SendData(Encoding.UTF8.GetBytes("Hello world !"));
            }
            catch (PinInUseException)
            {
                Debug.Print("Error accessing the FTDI Click board");
            }

            Thread.Sleep(Timeout.Infinite);
        }

        static void _ftdi_DataReceived(object sender, FTDIClick.DataReceivedEventArgs e)
        {
            Debug.Print("Data received (" + e.Count + " bytes) : " + new String(Encoding.UTF8.GetChars(e.Data)));
        }
    }
}
