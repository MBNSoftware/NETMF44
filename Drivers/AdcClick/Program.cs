using System;
using System.Threading;
using MBN.Modules;
using Microsoft.SPOT;
using MBN;

namespace Examples
{
    public class Program
    {
        static AdcClick _adc;

        public static void Main()
        {
            try
            {
                // ADC Click board is plugged on socket #1 of the MikroBus.Net mainboard
                _adc = new AdcClick(Hardware.SocketOne);

                // Sets the range from 0 to 3300 (instead of 0-4095)
                _adc.SetScale(0, 3300);

                // Gets scaled measure for channel 0
                Debug.Print("Channel 0 scaled: " + _adc.GetChannel(0));


                // Gets the actual value of all channels. Result will be scaled to the default range 0-4095
                int[] all = _adc.GetAllChannels(false);
                Debug.Print("Channel 0 scaled : " + all[0]);
                Debug.Print("Channel 1 scaled : " + all[1]);
                Debug.Print("Channel 2 scaled : " + all[2]);
                Debug.Print("Channel 3 scaled : " + all[3]);

                // Gets last scaled measure for channel 0, not actual reading
                Debug.Print("Channel 0 was last measured at : " + _adc.GetLastValue(0));

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception ex)
            {
                Debug.Print("ERROR : " + ex.Message);
            }
        }
    }
}
