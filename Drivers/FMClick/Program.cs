using System.Threading;
using MBN;
using MBN.Enums;
using MBN.Modules;
using Microsoft.SPOT;

namespace Examples
{
    public class Program
    {
        private static FMClick _fm;

        public static void Main()
        {
            _fm = new FMClick(Hardware.SocketFour, ClockRatesI2C.Clock400KHz, 100);

            /* If you are outside the USA or Australia, you must change the Channel Spacing and Radio Band from the default of Spacing.UsaAustralia and Band.UsaEurope
             *  In Europe - use Spacing.EuropeJapan and Band.UsaEurope.
             *  In Japan - use Spacing.EuropeJapan and Band.Japan or JapanWide.
             *  No other configurations are available.

                if (_fm.ChannelSpacing != FMClick.Spacing.UsaAustralia || _fm.RadioBand != FMClick.Band.UsaEurope)
                {
                    _fm.SetRadioConfiguration(FMClick.Spacing.EuropeJapan, FMClick.Band.UsaEurope);
                }
             */

            //_fm.Volume = 1;
            //_fm.Station = 93.3;

            _fm.Volume = 7;
            _fm.Station = 93.3;

            _fm.RadioTextChanged += _fm_RadioTextChanged;

            new Thread(Capture).Start();

            Thread.Sleep(Timeout.Infinite);
        }

        private static void _fm_RadioTextChanged(FMClick sender, string newradiotext)
        {
            Debug.Print("RDS Text received - " + newradiotext);
        }

        private static void Capture()
        {
            while (true)
            {
                var rssi = _fm.RSSI;
                var rssiPercentage = (double)(rssi) / 75 * 100;

                Debug.Print("RSSI: " + _fm.RSSI + " of 75 (" + rssiPercentage.ToString("F0") + "%)");
                Debug.Print("Stereo: " + _fm.IsStereo);
                Debug.Print("Is Muted ? " + _fm.Mute);
                Debug.Print("ChannelSpacing? " + _fm.ChannelSpacing);
                Debug.Print("RadioBand? " + _fm.RadioBand);
                Debug.Print("Station ? " + _fm.Station.ToString("D1") + " Hz");
                Debug.Print("RDS: " + _fm.RadioText + "\n");
                Thread.Sleep(3000);
            }
        }
    }
}
