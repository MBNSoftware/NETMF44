using System.Threading;
using MBN.Modules;
using Microsoft.SPOT;
using MBN;

namespace Examples
{
    public class Program
    {
#if DALMATIAN
        static StatusLed Led;
#endif
        static DigiPotClick _dp;

        public static void Main()
        {
            Debug.Print("Start!");
#if DALMATIAN
            Led = new StatusLed();
            Led.SetBlueLed(true); Thread.Sleep(200); Led.SetBlueLed(false); 
#endif
            _dp = new DigiPotClick(Hardware.SocketOne, 200);

            // WARNING THIS IS TEST CODE THAT DOES NOT EXIT TO DISPATCHER - NO EVENTS FIRED!!!
            // WARNING THIS IS TEST CODE THAT DOES NOT EXIT TO DISPATCHER - NO EVENTS FIRED!!!
            // WARNING THIS IS TEST CODE THAT DOES NOT EXIT TO DISPATCHER - NO EVENTS FIRED!!!
            while (true)
            {
#if DALMATIAN
                Led.SetRedLed(true); Thread.Sleep(200); Led.SetRedLed(false); 
#endif
                Debug.Print("Pos1");
                _dp.Resistance = 50;
#if DALMATIAN
                Led.SetRedLed(true); Thread.Sleep(500); Led.SetRedLed(false); Thread.Sleep(100);
#endif
                Thread.Sleep(1000);
                Debug.Print("Pos2");
                _dp.Resistance = 100;
#if DALMATIAN
                Led.SetGreenLed(true); Thread.Sleep(500); Led.SetGreenLed(false); Thread.Sleep(100);
#endif
                Debug.Print("Pos3");
                _dp.Resistance = 150;
#if DALMATIAN
                Led.SetBlueLed(true); Thread.Sleep(500); Led.SetBlueLed(false); Thread.Sleep(100);
#endif
                Debug.Print("Pos4");
                _dp.Resistance = 200;
#if DALMATIAN
                Led.SetRedLed(true); Thread.Sleep(500); Led.SetRedLed(false); Thread.Sleep(100);
#endif
                Debug.Print("Pos5");
                _dp.Resistance = 250;
#if DALMATIAN
                Led.SetGreenLed(true); Thread.Sleep(500); Led.SetGreenLed(false); Thread.Sleep(100);
#endif
                Debug.Print("Pos6");
#if DALMATIAN
                Led.SetRedLed(true); Thread.Sleep(500); Led.SetRedLed(false); Thread.Sleep(100);
#endif

                Thread.Sleep(1000);
            }
// ReSharper disable once FunctionNeverReturns
        }
    }
}
