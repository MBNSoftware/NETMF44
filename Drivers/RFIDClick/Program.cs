using System;
using MBN;
using System.Threading;
using Microsoft.SPOT;
using MBN.Modules;

namespace Examples
{
    public class Program
    {
        private static RFIDClick _rfid;
        private static DevantechLcd03 _lcd;
        private static Timer _timerLcd;

        public static void Main()
        {
            try
            {
                _rfid = new RFIDClick(Hardware.SocketOne);
                InitLcd();

                Debug.Print("RFID identification : " + _rfid.Identification());

                _lcd.Write(1, 4, "Calibration...");
                _rfid.Calibration(Hardware.Led2);
                _lcd.Write(1, 4, "              ");

                _rfid.TagDetected += _rfid_TagDetected;
                _rfid.TagRemoved += _rfid_TagRemoved;

                InitTimer();

                _rfid.DetectionEnabled = true;
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }

            Thread.Sleep(Timeout.Infinite);
        }

        static void InitTimer()
        {
            _timerLcd = new Timer(DimLCD, null, new TimeSpan(0, 0, 15), new TimeSpan(0, 0, 0));
        }

        static void DimLCD(object state)
        {
            _lcd.BackLight = false;
        }

        static void _rfid_TagRemoved(object sender, TagRemovedEventArgs e)
        {
            Hardware.Led1.Write(false);
            Debug.Print("Tag removed : " + e.TagID);
            _lcd.Write(1, 3, "                    ");
            _lcd.Write(1, 4, "                    ");
        }

        static void _rfid_TagDetected(object sender, TagDetectedEventArgs e)
        {
            _lcd.BackLight = true;
            Hardware.Led1.Write(true);
            Debug.Print("Tag detected : " + e.TagID);
            _lcd.Write(1, 3, e.TagID.ToString());
            _lcd.Write(1, 4, e.TagIDHex + "  #" + e.CRC);
            _timerLcd.Change(new TimeSpan(0, 0, 15), new TimeSpan(0, 0, 0));    // Dim Backlight after 15 seconds
        }

        private static void InitLcd()
        {
            _lcd = new DevantechLcd03(Hardware.SocketTwo, 0xC8 >> 1)
            {
                BackLight = true,
                Cursor = DevantechLcd03.Cursors.Hide
            };
            _lcd.ClearScreen();
            _lcd.Write(1, 1, "    MikroBus.Net");
            _lcd.Write(1, 2, "RFID Click demo");
        }
    }
}
