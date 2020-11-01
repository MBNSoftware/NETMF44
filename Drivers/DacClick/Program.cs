using System.Threading;
using MBN.Modules;
using MBN;
using MBN.Enums;

namespace Examples
{
    public class Program
    {
        static DacClick _dac;

        public static void Main()
        {
            _dac = new DacClick(Hardware.SocketOne);

            // Set DAC to 80 mV (Gain x1)
            _dac.Output = 100;
            Thread.Sleep(2000);

            // Set DAC to 800 mV (Gain x1)
            _dac.Output = 1000;
            Thread.Sleep(2000);

            // Set DAC to 1600 mV (Gain x2)
            _dac.Gain = DacClick.Gains.X2;
            _dac.Output = 1000;
            Thread.Sleep(2000);

            // Shutdown DAC power
            _dac.PowerMode = PowerModes.Off;
            Thread.Sleep(2000);

            // Wake up DAC
            _dac.PowerMode = PowerModes.On;
            Thread.Sleep(2000);

            // Set DAC to 2400 mV (Gain x1)
            _dac.Gain = DacClick.Gains.X1;
            _dac.Output = 3000;
            Thread.Sleep(2000);

            // Shut it down
            _dac.PowerMode = PowerModes.Off;

            Thread.Sleep(Timeout.Infinite);
        }
    }
}

