using MBN.Modules;
using Microsoft.SPOT;
using MBN;
using System.Threading;
using MBN.Enums;

namespace Examples
{
    public class Program
    {
        static ThermoClick _thermoClick;

        public static void Main()
        {
            _thermoClick = new ThermoClick(Hardware.SocketOne);
            _thermoClick.SensorFault += ThermoClickSensorFault;
            _thermoClick.TemperatureUnit = TemperatureUnits.Celsius;
            
            while (true)
            {
                Debug.Print("TC Temp "+ _thermoClick.ReadTemperature(TemperatureSources.Object).ToString("F2") + " °C");
                Debug.Print("CJ Temp " + _thermoClick.ReadTemperature().ToString("F6") + " °C");
                Thread.Sleep(3000);
            }
        }

        static void ThermoClickSensorFault(object sender, ThermoClick.FaultType e)
        {
            var err = string.Empty;
            switch (e)
            {
                case ThermoClick.FaultType.OpenFault:
                    err = "Thermocouple is Open.";
                    break;
                case ThermoClick.FaultType.ShortToGround:
                    err = "Thermocouple is shorted to Ground.";
                    break;
                case ThermoClick.FaultType.ShortToVcc:
                    err = "Thermocouple is shorted to VCC.";
                    break;
            }
            Debug.Print(err);
        }
    }
}
