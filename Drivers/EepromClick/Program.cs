using System;
using MBN.Modules;
using MBN;
using System.Threading;
using Microsoft.SPOT;

namespace Examples
{
    public class Program
    {
        private static Storage _eeprom;
        private static readonly Byte[] Data = new byte[3];

        public static void Main()
        {
            _eeprom = new EEpromClick(Hardware.SocketOne, memorySize: 256);  // Here, the original 8KB chip has been replaced by a 256KB one ;)
            
            Debug.Print("Address 231 before : " + _eeprom.ReadByte(231));
            _eeprom.WriteByte(231, 123);
            Debug.Print("Address 231 after : " + _eeprom.ReadByte(231));
            
            _eeprom.WriteData(400, new Byte[] { 100, 101, 102 }, 0, 3);
            _eeprom.ReadData(400, Data, 0, 3);
            Debug.Print("Read 3 bytes starting @400 (should be 100, 101, 102) : " + Data[0] + ", " + Data[1] + ", " + Data[2]);

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
