using MBN;
using MBN.Modules;
using MBN.Exceptions;
using Microsoft.SPOT;
using System.Threading;

namespace Examples
{
    public class Program
    {
        private static SerialLCD _lcd;

        public static void Main()
        {
            try
            {
                _lcd = new SerialLCD(Hardware.SocketTwo, SerialLCD.Baud.BaudRate9600, SerialLCD.DisplaySize.Size4X20);
                _lcd.ClearScreen();
                _lcd.SetBacklight(true);
            }

            catch (PinInUseException ex)
            {
                Debug.Print("Some pins are in use while creating instances : " + ex.Message + " Stack Trace " + ex.StackTrace);
            }

            DisplayMBNLogo(5000);

            _lcd.Print("Hello MikroBus.Net", 0, 0);
            _lcd.Print("VBat ", 2, 0);
            _lcd.PutC((byte)SerialLCD.CustomCharacters.OneBar, 2, 5);
            _lcd.PutC((byte)SerialLCD.CustomCharacters.TwoBars, 2, 6);
            _lcd.PutC((byte)SerialLCD.CustomCharacters.ThreeBars, 2, 7);
            _lcd.PutC((byte)SerialLCD.CustomCharacters.FourBars, 2, 8);
            _lcd.PutC((byte)SerialLCD.CustomCharacters.FiveBars, 2, 9);
            _lcd.PutC((byte)SerialLCD.CustomCharacters.SixBars, 2, 10);
            _lcd.PutC((byte)SerialLCD.CustomCharacters.SevenBars, 2, 11);
            _lcd.Print("Temperature 25.0°C", 3, 0);
            _lcd.PutC((byte)SerialLCD.CustomCharacters.DegreesSymbol, 3, 16);

            Thread.Sleep(Timeout.Infinite);
        }

        private static void DisplayMBNLogo(int msDelay)
        {
            var oldCharacterSet = _lcd.CharacterSet;

            _lcd.SetUserCharacter(SerialLCD.UserCharacters.UserCharacter1, new byte[] { 1, 6, 12, 24, 24, 12, 6, 1 });
            _lcd.SetUserCharacter(SerialLCD.UserCharacters.UserCharacter2, new byte[] { 16, 12, 6, 3, 3, 6, 12, 16 });
            _lcd.SetUserCharacter(SerialLCD.UserCharacters.UserCharacter3, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 });
            _lcd.SetUserCharacter(SerialLCD.UserCharacters.UserCharacter4, new byte[] { 16, 16, 16, 16, 16, 16, 16, 16 });

            _lcd.CharacterSet = SerialLCD.CharacterSets.User;

            _lcd.PutC((byte)SerialLCD.UserCharacters.UserCharacter1, 0, 0);
            _lcd.PutC((byte)SerialLCD.UserCharacters.UserCharacter2, 0, 1);
            _lcd.PutC((byte)SerialLCD.UserCharacters.UserCharacter3, 1, 0);
            _lcd.PutC((byte)SerialLCD.UserCharacters.UserCharacter4, 1, 1);

            _lcd.Print("MikroBus.Net", 0, 4);
            _lcd.Print("Where NetMF", 2, 4);
            _lcd.Print("meets MikroBus", 3, 4);
            Thread.Sleep(msDelay);
            _lcd.ClearScreen();
            _lcd.CharacterSet = oldCharacterSet;
        }
    }
}
