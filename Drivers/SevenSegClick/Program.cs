using System.Threading;
using MBN;
using MBN.Modules;

namespace Examples
{
    public class Program
    {
        private static SevenSegClick _seven;

        private static readonly byte[] Spin1 = 
        {
            0x00, 0x04, 
            0x00, 0x40,
            0x00, 0x20,
            0x00, 0x10,
            0x10, 0x00,
            0x08, 0x00,
            0x02, 0x00,
            0x04, 0x00
        };

        private static readonly byte[] Spin2 = 
        {
            0x04, 0x04, 
            0x80, 0x80,
            0x10, 0x10,
            0x80, 0x80
        };

        private static readonly byte[] Spin3 = 
        {
            0x00, 0x60, 
            0x00, 0x0A,
            0x60, 0x00,
            0x0A, 0x00,
            0x60, 0x00,
            0x00, 0x0A
        };

        public static void Main()
        {
            _seven = new SevenSegClick(Hardware.SocketOne, 0.05);

            // Displays from 0 to 9.9
            // Trick : no float here, only bytes, the dot is added as soon as i > 9
            for (byte i = 0; i < 100; i++)
            {
                _seven.SendBytes(i < 10
                    ? new byte[] {_seven.GetDigit(i), 0x00}
                    : new [] {_seven.GetDigit((byte) (i%10)), (byte) (_seven.GetDigit((byte) (i/10)) + 1)});
                Thread.Sleep(75);
            }

            // Scrolls from A to Z
            _seven.SendBytes(new byte[] { 0xEE, 0x00 });
            Thread.Sleep(200);

            for (byte i = 66; i < 91; i++)
            {
                _seven.SendBytes(new [] { _seven.CharTable[i - 44], _seven.CharTable[i - 45] });
                Thread.Sleep(200);
            }

            // Displays all alpha chars on the right display, using the GetChar() method
            for (char i = 'A'; i <= 'Z'; i++)
            {
                _seven.SendBytes(new byte[] { _seven.GetChar(i), 0x00 });
                Thread.Sleep(200);
            }

            // Some fun, now !
            for (int j = 0; j < 10; j++)
            {
                for (byte i = 0; i < 16; i += 2)
                {
                    _seven.SendBytes(new [] { Spin1[i], Spin1[i + 1] });
                    Thread.Sleep(50);
                }
            }

            for (int j = 0; j < 6; j++)
            {
                for (byte i = 0; i < 8; i += 2)
                {
                    _seven.SendBytes(new [] { Spin2[i], Spin2[i + 1] });
                    Thread.Sleep(200);
                }
            }

            for (int j = 0; j < 6; j++)
            {
                for (byte i = 0; i < 12; i += 2)
                {
                    _seven.SendBytes(new [] { Spin3[i], Spin3[i + 1] });
                    Thread.Sleep(200);
                }
            }
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
