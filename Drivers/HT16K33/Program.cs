using Microsoft.SPOT;
using MBN;
using MBN.Modules;
using System.Threading;

namespace Examples
{
    public class Program
    {
        private static HT16K33 _disp;

        public static void Main()
        {
            _disp = new HT16K33(Hardware.SocketTwo) { Brightness = 1 };
            
            Debug.Print("Driver version : " + _disp.DriverVersion);
            TestDoubles();
            Thread.Sleep(2000);
            TestIntegers();

            Thread.Sleep(Timeout.Infinite);
        }

        private static void TestDoubles()
        {
            for (var i = 0.0; i < 200.0; i += 0.1)
            {
                _disp.Write(i.ToString("F1"));
                Thread.Sleep(5);
            }
            Thread.Sleep(1000);

            for (var i = 0.0; i < 20.0; i += 0.01)
            {
                _disp.Write(i.ToString("F2"));
                Thread.Sleep(5);
            }
            Thread.Sleep(1000);

            for (var i = 0.0; i < 2.0; i += 0.001)
            {
                _disp.Write(i.ToString("F3"));
                Thread.Sleep(5);
            }
            Thread.Sleep(2000);
            _disp.Display(false);
        }

        private static void TestIntegers()
        {
            for (int i = 0; i < 2000; i++)
            {
                _disp.Write(i, i > 999);
                Thread.Sleep(10);
            }
            _disp.Blink(HT16K33.BlinkModes.Blink2Hz);
            Thread.Sleep(6000);

            _disp.Display(true);
            _disp.ClearDisplay();

            for (int i = 0; i > -1000; i--)
            {
                _disp.Write(i);
                Thread.Sleep(10);
            }
            Thread.Sleep(2000);
            _disp.Display(false);
        }
    }
}
