using System;
using MBN;
using System.Threading;
using MBN.Modules;
using Microsoft.SPOT;

namespace Examples
{
    public class Program
    {
        public static void Main()
        {
            var flash = new OnboardFlash();

            // For this example, we completely erase the chip to be sure that we get back correct values.
            // Otherwise, as it is a Flash memory, writing will do a logical AND on the data already present at the same address,
            // thus resulting in a "wrong" value read back.
            // Led indicator is used to show activity as it can take up to 45 sec to erase the 8MB onboard Flash !
            Hardware.Led1.Write(true);
            flash.EraseChip();
            Hardware.Led1.Write(false);

            flash.WriteByte(10, 200);
            Debug.Print("Read byte @10 (should be 200) : " + flash.ReadByte(10));
            flash.WriteByte(11, 201);
            Debug.Print("Read byte @11 (should be 201) : " + flash.ReadByte(11));

            flash.WriteData(20, new Byte[] { 100, 101, 102 },0,3);
            var bArray = new Byte[3];
            flash.ReadData(20, bArray,0,3);
            Debug.Print("Read 3 bytes starting @20 (should be 100, 101, 102) : " + bArray[0] + ", " + bArray[1] + ", " + bArray[2]);

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
