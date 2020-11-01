using System;
using MBN;
using MBN.Modules;
using Microsoft.SPOT;
using System.Threading;
using Microsoft.SPOT.Hardware;

namespace Examples
{
	public class Program
	{
		private static MotionClick motion;
		private static OutputPort led;
		private static Timer activityTimer;

		public static void Main()
		{
			activityTimer = new Timer(LedMonitor, null, new TimeSpan(TimeSpan.MaxValue.Ticks), new TimeSpan(TimeSpan.MaxValue.Ticks));

			led = new OutputPort(Pin.PB13, false);

			motion = new MotionClick(Hardware.SocketFour);

			motion.MotionDetected += motion_MotionDetected;

			Thread.Sleep(Timeout.Infinite);
		}

		private static void LedMonitor(object state)
		{
			led.Write(false);
			Debug.Print("LED should be off");
			activityTimer.Change(new TimeSpan(TimeSpan.MaxValue.Ticks), new TimeSpan(TimeSpan.MaxValue.Ticks));
		}


		static void motion_MotionDetected(object sender, MotionClick.MotionDetectedEventArgs e)
		{
			if (e.Activity)
			{
				led.Write(true);
				activityTimer.Change(new TimeSpan(TimeSpan.MaxValue.Ticks), new TimeSpan(TimeSpan.MaxValue.Ticks));
				Debug.Print("Activity Detected at " + e.EventTime + " turning LED on.");
			}
			else
			{
				activityTimer.Change(10000, 0);
				Debug.Print("No Activity Detected at " + e.EventTime + " LED will turn off in 10 seconds.");
			}
		}

	}
}
