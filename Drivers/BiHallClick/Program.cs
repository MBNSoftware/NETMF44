using System.Threading;
using MBN.Modules;
using MBN;
using Microsoft.SPOT;

namespace Examples
{
    public class Program
    {
        private static BiHallClick _biHall;

        public static void Main()
        {
            Debug.Print("Started");

            _biHall = new BiHallClick(Hardware.SocketFour);
            _biHall.SwitchStateChanged += BiHallSwitchStateChanged;

            Thread.Sleep(Timeout.Infinite);
        }

        static void BiHallSwitchStateChanged(object sender, bool switchState)
        {
            Debug.Print("Switch state - " + switchState);
        }
    }
}
