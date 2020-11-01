using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Time;

namespace RTC
{
    public class Program
    {
        public static void Main()
        {
            Debug.Print("1 - Date time local : " + DateTime.Now);
            Debug.Print("1 - Date time UTC   : " + DateTime.UtcNow);

            Debug.Print("Setting UTC time, with 2 hours offset");
            TimeService.SetTimeZoneOffset(120);
            TimeService.SetUtcTime(new DateTime(2014, 11, 4, 20, 55, 0).Ticks);
            Debug.Print("2 - Date time local : " + DateTime.Now);
            Debug.Print("2 - Date time UTC   : " + DateTime.UtcNow);

            Debug.Print("Setting local time");
            Utility.SetLocalTime(new DateTime(2014, 11, 16, 10, 53, 0));
            Debug.Print("3 - Date time local : " + DateTime.Now);
            Debug.Print("3 - Date time UTC   : " + DateTime.UtcNow);

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
