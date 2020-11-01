using MBN;
using MBN.Enums;
using MBN.Modules;
using Microsoft.SPOT;
using System.Threading;

namespace UVClickDemo
{
    public class Program
    {
        private static UVClick _uv;

        public static void Main()
        {
            _uv = new UVClick(Hardware.SocketFour);

            new Thread(Capture).Start();

            Thread.Sleep(Timeout.Infinite);
        }

        private static void Capture()
        {
            while (true)
            {
                Debug.Print("V - " + _uv.ReadVoltage());
                Debug.Print("UV Intensity - " + _uv.ReadUVIntensity() + "\n");
                Thread.Sleep(1000);
            }
        }


    }
}
