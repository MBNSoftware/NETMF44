using MBN;
using MBN.Enums;
using MBN.Modules;
using Microsoft.SPOT;
using System.Threading;

namespace Examples
{
    public class Program
    {
        private static HTU21DClick _sensor;

        public static void Main()
        {
            _sensor = new HTU21DClick(Hardware.SocketTwo, ClockRatesI2C.Clock400KHz, 100)
            {
                MeasurementMode = HTU21DClick.ReadMode.Hold,
                Resolution = HTU21DClick.DeviceResolution.UltraHigh
            };

            while (true)
            {
                Debug.Print("Humidity    " + _sensor.ReadHumidity(HumidityMeasurementModes.Relative).ToString("n2") + " %RH");
                Debug.Print("Temperature " + _sensor.ReadTemperature(TemperatureSources.Ambient).ToString("n2") + " °C");
                Thread.Sleep(100);
            }
        }
    }
}
