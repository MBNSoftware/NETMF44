using MBN.Modules;
using Microsoft.SPOT;
using MBN;
using System.Threading;

namespace Examples
{
    public class Program
    {
        static ProximityClick _prox;

        public static void Main()
        {
            _prox = new ProximityClick(Hardware.SocketTwo);                   // Proximity at address 0x70 on socket 2

            Debug.Print("Chip revision : " + _prox.ChipRevision);             // Get chip version and firmware revision

            // Set IR Led current to 200 mA  (20 x 10). 
            // Warning : different values of current will cause different readings for the same distance (see datasheet).
            _prox.IRLedCurrent = 20;     
            _prox.ProximityRate = 1;     // Set Proximity rate measurement to 3.9 measures/s

            Debug.Print("Ambient light : " + _prox.AmbientLight());           // Get ambient light value

            while (true)
            {
                Debug.Print("Proximity : " + _prox.Distance());               // Get proximity value
                Thread.Sleep(100);
            }
// ReSharper disable once FunctionNeverReturns
        }
    }
}
