using MBN.Modules;
using Microsoft.SPOT;
using MBN;
using System.Threading;

namespace Examples
{
    public class Program
    {
        private static AccelClick _accel;

        public static void Main()
        {
            _accel = new AccelClick(Hardware.SocketOne);
            Debug.Print("Device ID : " + _accel.DeviceID);

            // Set the sensor to fixed resolution, updating every 10ms
            _accel.OutputResolution = AccelClick.OutputResolutions.FixedResoultion;
            _accel.UpdateDelay = 10;

            // Single tap/double tap enabled by default, so capture the associated events
            _accel.OnDoubleTap += Accel_OnDoubleTap;
            _accel.OnSingleTap += Accel_OnSingleTap;

            // Start polling
            _accel.Start();

            
            
            while (true)
            {
                // Prints the 3 axis acceleration values
                Debug.Print(_accel.CurrentData.ToString());

                Thread.Sleep(100);
            }

            //Thread.Sleep(Timeout.Infinite);
// ReSharper disable once FunctionNeverReturns
        }

        static void Accel_OnSingleTap(object sender, EventArgs e)
        {
            Hardware.Led2.Write(true);
            Thread.Sleep(20);
            Hardware.Led2.Write(false);
        }

        static void Accel_OnDoubleTap(object sender, EventArgs e)
        {
            Hardware.Led1.Write(true);
            Thread.Sleep(20);
            Hardware.Led1.Write(false);
        }
    }
}
