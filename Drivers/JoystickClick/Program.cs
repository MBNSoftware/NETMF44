using System;
using Microsoft.SPOT;
using MBN;
using MBN.Modules;
using System.Threading;

namespace Examples
{
    public class Program
    {
        private static JoystickClick _joy;

        public static void Main()
        {
            _joy = new JoystickClick(Hardware.SocketFour) {DeadZone = new SByte[] {100, -100, 100, -100}};

            Debug.Print("DZ = " + _joy.DeadZone[0] + "," + _joy.DeadZone[1] + "," + _joy.DeadZone[2] + "," +
                        _joy.DeadZone[3]);
            _joy.TimeBase = 7;

            _joy.InterruptLine.OnInterrupt += InterruptLineOnInterruptLine;
            _joy.Button.OnInterrupt += Button_OnInterrupt;

            Thread.Sleep(Timeout.Infinite);
        }

        private static void Button_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            Hardware.Led1.Write(data2 == 0);
        }

        private static void InterruptLineOnInterruptLine(uint data1, uint data2, DateTime time)
        {
            var pos = _joy.GetKnobPosition();
            Debug.Print("Interrupt (X,Y) : " + pos.X + "," + pos.Y);
        }
    }
}
