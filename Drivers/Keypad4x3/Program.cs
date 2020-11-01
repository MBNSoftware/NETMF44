using MBN.Modules;
using Microsoft.SPOT;
using MBN;
using System.Threading;

namespace Examples
{
    public class Program
    {
        private static Keypad4X3 _keypad;

        public static void Main()
        {
            // Pins PE5..PE2 are connected to rows, PD7..PD3 to columns
            _keypad = new Keypad4X3(Pin.PE5, Pin.PE4, Pin.PE3, Pin.PE2, Pin.PD7, Pin.PD4, Pin.PD3);
            _keypad.KeyReleased += KeypadKeyReleased;
            _keypad.KeyPressed += KeypadKeyPressed;

            _keypad.StartScan();

            Thread.Sleep(Timeout.Infinite);
        }

        static void KeypadKeyPressed(object sender, Keypad4X3.KeyPressedEventArgs e)
        {
            Hardware.Led1.Write(true);
        }

        static void KeypadKeyReleased(object sender, Keypad4X3.KeyReleasedEventArgs e)
        {
            Debug.Print("Key : " + e.KeyChar);
            Hardware.Led1.Write(false);
        }
    }
}
