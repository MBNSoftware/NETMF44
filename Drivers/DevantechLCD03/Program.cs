using MBN;
using System.Threading;
using MBN.Modules;

namespace Examples
{
    public class Program
    {
        private static DevantechLcd03 _lcd;

        public static void Main()
        {
            _lcd = new DevantechLcd03(Hardware.SocketOne, 0xC8 >> 1)
            {
                BackLight = true,
                Cursor = DevantechLcd03.Cursors.Hide
            };
            _lcd.ClearScreen();
            _lcd.Write(1, 1, "Hello world !");
            _lcd.Write(1, 3, "Version " + _lcd.DriverVersion);

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
