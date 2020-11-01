using System.Threading;
using MBN;
using MBN.Modules;
using Microsoft.SPOT;

namespace TestApp
{
    public class Program
    {
        static ThunderClick _thunder;

        public static void Main()
        {
            // Create the instance
            _thunder = new ThunderClick(Hardware.SocketOne);

            // Subscribe to events
            _thunder.LightningDetected += TH_LightningDetected;
            _thunder.DisturbanceDetected += TH_DisturbanceDetected;
            _thunder.NoiseDetected += TH_NoiseDetected;

            // Some information
            Debug.Print("Continuous input noise level " + _thunder.ContinuousInputNoiseLevel + " µV rms");

            // Start interrupt scanning
            _thunder.StartIRQ();

            Thread.Sleep(Timeout.Infinite);
        }

        static void TH_NoiseDetected(object sender, EventArgs e)
        {
            Debug.Print("Noise detected");
        }

        static void TH_DisturbanceDetected(object sender, EventArgs e)
        {
            Debug.Print("Disturbance detected");
        }

        static void TH_LightningDetected(object sender, ThunderClick.LightningEventArgs e)
        {
            Debug.Print("Lightning detected at " + e.Distance + " km, energy : " + e.Energy);
        }
    }
}