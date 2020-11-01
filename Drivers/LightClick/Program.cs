using MBN;
using MBN.Modules;
using Microsoft.SPOT;
using System.Threading;

namespace Examples
{
    public class Program
    {
        static LightClick _lightClick;

        public static void Main()
        {
            _lightClick = new LightClick(Hardware.SocketOne) {NumberOfSamples = 50};

            // Sets the range from 0 to 3300 (instead of 0-4095)
            _lightClick.SetScale(0, 3300);

            while (true)
            {
                Debug.Print("Light(Scaled) - " + _lightClick.Read(true));
                Debug.Print("Light(Unscaled) - " + _lightClick.Read(false));
                Debug.Print("Light Percent - " + _lightClick.ReadLightPercentage() + "%");
                Debug.Print("Light Volts - " + _lightClick.ReadVoltage() + "V\n");
                Thread.Sleep(3000);
            }
        }
    }
}
