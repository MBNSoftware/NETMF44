using MBN;
using MBN.Enums;
using Microsoft.SPOT;
using MBN.Modules;
using System.Threading;

namespace Examples
{
    public class Program
    {
        private static IRThermoClick _ir;

        public static void Main()
        {
            _ir = new IRThermoClick(Hardware.SocketThree);

            Debug.Print("Ambient temperature : " + _ir.ReadTemperature().ToString("F2"));
            Debug.Print("Object temperature : " + _ir.ReadTemperature(TemperatureSources.Object).ToString("F2"));

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
