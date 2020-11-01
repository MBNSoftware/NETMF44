using MBN;
using System;
using System.Threading;
using MBN.Modules;

namespace Examples
{
    public class Program
    {
        static BarGraphClick Leds;
        static UInt16[] _fillMasks, _indivMasks;

        public static void Main()
        {
            _fillMasks = new UInt16[] { 512, 768, 896, 960, 992, 1008, 1016, 1020, 1022, 1023 };
            _indivMasks = new UInt16[] { 1, 3, 7, 14, 28, 56, 112, 224, 448, 896 };

            Leds = new BarGraphClick(Hardware.SocketTwo);

            Demo(true);
            Demo(false);
            DemoRandom();
            DemoMask();

            Leds.Brightness = 1.0;
            Leds.Bars(5);

            Thread.Sleep(Timeout.Infinite);
        }

        private static void DemoBrightness()
        {
            for (int j = 0; j < 3; j++)
            {
                for (double i = 0; i < 0.5; i += 0.01)
                {
                    Leds.Brightness = i;
                    Thread.Sleep(25);
                }
                for (double i = 0.5; i > 0; i -= 0.01)
                {
                    Leds.Brightness = i;
                    Thread.Sleep(25);
                }
            }
        }

        private static void DemoMask()
        {
            for (var j = 0; j < 3; j++)
            {
                for (UInt16 i = 0; i < 10; i++)
                {
                    Leds.SendMask(_fillMasks[i]);
                    Thread.Sleep(50);
                }
                for (UInt16 i = 0; i < 10; i++)
                {
                    Leds.SendMask(_fillMasks[9 - i]);
                    Thread.Sleep(50);
                }
            }

            for (int j = 0; j < 3; j++)
            {
                for (UInt16 i = 0; i < 10; i++)
                {
                    Leds.SendMask(_indivMasks[i]);
                    Thread.Sleep(50);
                }
                for (UInt16 i = 0; i < 10; i++)
                {
                    Leds.SendMask(_indivMasks[9 - i]);
                    Thread.Sleep(50);
                }
            }
        }

        private static void DemoRandom()
        {
            Random Rnd = new Random(1023);
            for (int i = 0; i < 20; i++)
            {
                Leds.SendMask((UInt16)Rnd.Next());
                Thread.Sleep(100);
            }
        }

        private static void Demo(Boolean Fill)
        {
            for (int j = 0; j < 3; j++)
            {
                for (UInt16 i = 0; i <= 10; i++)
                {
                    Leds.Bars(i, Fill);
                    Thread.Sleep(50);
                }
                for (UInt16 i = 0; i <= 10; i++)
                {
                    Leds.Bars((UInt16)(10 - i), Fill);
                    Thread.Sleep(50);
                }
            }
        }
    }
}
