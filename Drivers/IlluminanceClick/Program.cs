using System;
using System.Threading;
using MBN;
using MBN.Enums;
using MBN.Modules;
using Microsoft.SPOT;

namespace Examples
{
	public class Program
	{

		private static IlluminanceClick illuminance;

		public static void Main()
		{
			illuminance = new IlluminanceClick(Hardware.SocketFour, IlluminanceClick.I2CAddress.Primary, ClockRatesI2C.Clock400KHz)
			{
				IntegrationTime = IlluminanceClick.IntegrationTimePeriod._13MS,
				Gain = IlluminanceClick.GainControl.Low,
				AutoGainControl = true
			};

			illuminance.Initialize();


			/* Update these values depending on what you've set above! */
			Debug.Print("-------------------------------------");
			Debug.Print("           Gain: " + illuminance.Gain);
			Debug.Print("AutoGainControl: " + illuminance.AutoGainControl);
			Debug.Print("IntegrationTime: " + illuminance.IntegrationTime);
			Debug.Print("         ChipID: " + illuminance.ChipID);
			Debug.Print("-------------------------------------\n");


			while (true)
			{

				Debug.Print("FullSpectrum Light - " + illuminance.FullSpectrumLight);
				Debug.Print("Visible Light - " + illuminance.VisibleLight);
				Debug.Print("Infrared Light - " + illuminance.InfraredLight);
				Debug.Print("Lux - " + illuminance.ReadLux() + "\n");

				UInt16 visible;
				UInt16 ir;
				UInt16 fullSpectrum;
				illuminance.ReadLuminosity(out fullSpectrum, out visible, out ir);

				Debug.Print("-------------------------------------------");
				Debug.Print("The following readings with AutoGainControl");
				Debug.Print("-------------------------------------------");
				Debug.Print("AutoGain FullSpectrum Light - " + fullSpectrum);
				Debug.Print("     AutoGain Visible Light - " + visible);
				Debug.Print("    AutoGain Infrared Light - " + ir + "\n");

				Thread.Sleep(1000);

			}

		}
	}
}
