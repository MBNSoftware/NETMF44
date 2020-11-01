using MBN.Modules;
using Microsoft.SPOT;
using MBN;
using MBN.Exceptions;
using System;
using System.Threading;

namespace Examples
{
    public class Program
    {
        static PressureClick _pres;
        static Boolean _deviceOk;

        public static void Main()
        {
            _deviceOk = false;
            while (!_deviceOk)
            {
                try
                {
// ReSharper disable RedundantArgumentDefaultValue
                    _pres = new PressureClick(Hardware.SocketOne, 0xBA >> 1, MBN.Enums.ClockRatesI2C.Clock100KHz);
// ReSharper restore RedundantArgumentDefaultValue
                    _deviceOk = true;
                }
                catch (DeviceInitialisationException)
                {
                    Debug.Print("Init failed, retrying...");
                }
            }

            while (true)
            {
                Debug.Print("Pression = " + _pres.ReadPressure() + " hPa");
                Debug.Print("Temperature = " + _pres.ReadTemperature().ToString("F2") + "°");
                Thread.Sleep(1000);
            }
        }
    }
}
