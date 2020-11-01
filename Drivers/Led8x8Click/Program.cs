using System;
using System.Threading;
using MBN;
using MBN.Modules;

namespace Examples
{
    public class Program
    {
        static Led8X8Click _leds;
        static Byte[][] _digits, _invertedDigits;

        public static void Main()
        {
            InitDigits();

            _leds = new Led8X8Click(Hardware.SocketOne) {Brightness = 1};
            _leds.Display(true);

            _leds.Clear();
            Thread.Sleep(1000);

            DemoDigits();
            DemoPixels();
            DemoColumns();
            DemoLines();
            Demoboth();

            Thread.Sleep(1000);
            _leds.Display(false);

            Thread.Sleep(Timeout.Infinite);
        }

        private static void DemoDigits()
        {
            for (Byte i = 0; i < 10; i++)
            {
                _leds.SendArray(_digits[i], true);
                Thread.Sleep(100);
            }
            for (Byte i = 10; i > 0; i--)
            {
                _leds.SendArray(_invertedDigits[i - 1], true);
                Thread.Sleep(100);
            }
        }

        private static void InitDigits()
        {
            _digits = new Byte[10][];
            _digits[0] = new Byte[] { 0x00, 0x3E, 0x7F, 0x49, 0x45, 0x7F, 0x3E, 0x00 };
            _digits[1] = new Byte[] { 0x00, 0x40, 0x44, 0x7F, 0x7F, 0x40, 0x40, 0x00 };
            _digits[2] = new Byte[] { 0x00, 0x62, 0x73, 0x51, 0x49, 0x4F, 0x46, 0x00 };
            _digits[3] = new Byte[] { 0x00, 0x22, 0x63, 0x49, 0x49, 0x7F, 0x36, 0x00 };
            _digits[4] = new Byte[] { 0x00, 0x18, 0x18, 0x14, 0x16, 0x7F, 0x7F, 0x10 };
            _digits[5] = new Byte[] { 0x00, 0x27, 0x67, 0x45, 0x45, 0x7D, 0x39, 0x00 };
            _digits[6] = new Byte[] { 0x00, 0x3E, 0x7F, 0x49, 0x49, 0x7B, 0x32, 0x00 };
            _digits[7] = new Byte[] { 0x00, 0x03, 0x03, 0x79, 0x7D, 0x07, 0x03, 0x00 };
            _digits[8] = new Byte[] { 0x00, 0x36, 0x7F, 0x49, 0x49, 0x7F, 0x36, 0x00 };
            _digits[9] = new Byte[] { 0x00, 0x26, 0x6F, 0x49, 0x49, 0x7F, 0x3E, 0x00 };

            _invertedDigits = new Byte[10][];
            _invertedDigits[0] = new Byte[] { 0xFF, 0xC1, 0x80, 0xB6, 0xBA, 0x80, 0xC1, 0xFF };
            _invertedDigits[1] = new Byte[] { 0xFF, 0xBF, 0xBB, 0x80, 0x80, 0xBF, 0xBF, 0xFF };
            _invertedDigits[2] = new Byte[] { 0xFF, 0x9D, 0x8C, 0xAE, 0xB6, 0xB0, 0xB9, 0xFF };
            _invertedDigits[3] = new Byte[] { 0xFF, 0xDD, 0x9C, 0xB6, 0xB6, 0x80, 0xC9, 0xFF };
            _invertedDigits[4] = new Byte[] { 0xFF, 0xE7, 0xE7, 0xEB, 0xE9, 0x80, 0x80, 0xEF };
            _invertedDigits[5] = new Byte[] { 0xFF, 0xD8, 0x98, 0xBA, 0xBA, 0x82, 0xC6, 0xFF };
            _invertedDigits[6] = new Byte[] { 0xFF, 0xC1, 0x80, 0xB6, 0xB6, 0x84, 0xCD, 0xFF };
            _invertedDigits[7] = new Byte[] { 0xFF, 0xFC, 0xFC, 0x86, 0x82, 0xF8, 0xFC, 0xFF };
            _invertedDigits[8] = new Byte[] { 0xFF, 0xC9, 0x80, 0xB6, 0xB6, 0x80, 0xC9, 0xFF };
            _invertedDigits[9] = new Byte[] { 0xFF, 0xD9, 0x90, 0x86, 0x86, 0x80, 0xC1, 0xFF };
        }

        private static void DemoPixels()
        {
            for (Byte i = 1; i < 9; i++)
            {
                for (Byte j = 1; j < 9; j++)
                {
                    _leds.SetPixel(i, j, true, true);
                    Thread.Sleep(50);
                    _leds.SetPixel(i, j, false, true);
                }
            }

            for (Byte i = 1; i < 9; i++)
            {
                for (Byte j = 1; j < 9; j++)
                {
                    _leds.SetPixel(j, i, true, true);
                    Thread.Sleep(50);
                    _leds.SetPixel(j, i, false, true);
                }
            }
        }

        private static void DemoColumns()
        {
            for (int i = 0; i < 5; i++)
            {
                for (Byte col = 1; col <= 8; col++)
                {
                    _leds.DrawColumn(col, true, true);
                    Thread.Sleep(50);
                    _leds.DrawColumn(col, false, true);
                }

                for (Byte col = 8; col >= 1; col--)
                {
                    _leds.DrawColumn(col, true, true);
                    Thread.Sleep(50);
                    _leds.DrawColumn(col, false, true);
                }
            }
        }

        private static void DemoLines()
        {
            for (int i = 0; i < 5; i++)
            {
                for (Byte col = 1; col <= 8; col++)
                {
                    _leds.DrawRow(col, true, true);
                    Thread.Sleep(50);
                    _leds.DrawRow(col, false, true);
                }

                for (Byte col = 8; col >= 1; col--)
                {
                    _leds.DrawRow(col, true, true);
                    Thread.Sleep(50);
                    _leds.DrawRow(col, false, true);
                }
            }
        }

        private static void Demoboth()
        {
            for (var i = 0; i < 5; i++)
            {
                for (Byte col = 1; col <= 8; col++)
                {
                    _leds.DrawColumn(col, true);
                    _leds.DrawRow(col, true, true);
                    Thread.Sleep(50);
                    _leds.DrawColumn(col, false);
                    _leds.DrawRow(col, false, true);
                }

                for (Byte col = 8; col >= 1; col--)
                {
                    _leds.DrawColumn(col, true);
                    _leds.DrawRow(col, true, true);
                    Thread.Sleep(50);
                    _leds.DrawColumn(col, false);
                    _leds.DrawRow(col, false, true);
                }
            }
        }
    }
}
