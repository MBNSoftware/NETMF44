using MBN;
using MBN.Modules;
using Microsoft.SPOT;
using System.Threading;

namespace Examples
{
    public class Program
    {
        private static CurrentClick _current;

        public static void Main()
        {
            _current = new CurrentClick(Hardware.SocketThree, CurrentClick.ShuntResistor.SR_CUSTOM, 0.2f) {NumberOfSamples = 50};

            new Thread(Capture).Start();

            Thread.Sleep(Timeout.Infinite);
        }

        private static void Capture()
        {
            while (true)
            {
                Debug.Print(_current.ReadCurrent() + "mA");
                Thread.Sleep(1000);
            }
        }
    }
}