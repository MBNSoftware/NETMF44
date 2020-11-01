using MBN;
using MBN.Enums;
using MBN.Modules;
using Microsoft.SPOT;
using System;
using System.Threading;

namespace Examples
{
    public class Program
    {
        private static RTC2Click _clock;

        public static void Main()
        {
            _clock = new RTC2Click(Hardware.SocketOne, ClockRatesI2C.Clock400KHz, 100 );

            // Check the Oscillator.
            if (!_clock.IsOscillatorOn())
            {
                //Make sure the clock is running
                _clock.ClockHalt(false);
            }

            // Set the clock to some arbitrary date / time
            _clock.SetClock(new DateTime(2014, 05, 25, 11, 59, 45), true);

            // Test reading RTC clock registers
            Debug.Print("Before Clock Halt: " + _clock.GetDateTime());

            // Halt the clock for 5 seconds
            _clock.ClockHalt(true);
            Thread.Sleep(5000);

            // Should be the same time as "Before Halt"
            Debug.Print("After Clock Halt for 5 seconds (it should be the same time): " + _clock.GetDateTime());

            // Resume the clock
            _clock.ClockHalt(false);

            // Sleep for another 1.5 second
            Thread.Sleep(1500);

            // Should be just one second later since the clock's oscillator was resumed
            Debug.Print("Resumed ClockHalt (should be time + ~1 sec): " + _clock.GetDateTime());

            // Requires an oscilloscope or an interrupt handler on the micro controller to see the effects
            TestSquareWaves();

            // Test writing to arbitrary registers
            Debug.Print("Writing distinct RAM registers (writing the register number to itself)");
            for (byte b = RTC2Click.Registers.RTC_START_ADDRESS; b <= RTC2Click.Registers.USER_RAM_END_ADDRESS; b++)
            {
                _clock.WriteUserSramAddress(b, b);
            }

            // Test reading from arbitrary registers
            Debug.Print("Reading distinct RAM registers (the registers and values read should be the same)");
            for (byte b = RTC2Click.Registers.USER_RAM_START_ADDRESS; b <= RTC2Click.Registers.USER_RAM_END_ADDRESS; b++)
            {
                Debug.Print(b + ": " + _clock.ReadUserSramAddress(b));
            }

            // Test writing to the RAM as a single block
            string text = "There are 48 available bytes in USER SRAM.";
            Debug.Print("Writing string: " + text + " to USER SRAM as a block.");

            var userSram = new byte[RTC2Click.Registers.USER_RAM_SIZE];

            // Copy the string to the ram buffer
            for (byte b = 0; b < text.Length; b++)
            {
                userSram[b] = (byte) text[b];
            }
            // Write it to the RAM in the clock
            _clock.WriteUSerSram(userSram);

            // Zero out the string
            text = null;

            // Test reading from the RAM as a single block
            Debug.Print("Reading from the RAM as a block...");
            userSram = _clock.ReadUserSram();

            for (byte I = 0; I < userSram.Length; I++)
            {
                text += (char) userSram[I];
            }

            if (text != null) Debug.Print("Reading from USER SRAM: " + text + "\n");

            // Sleep another 5 seconds before exiting
            Thread.Sleep(5*1000);

            // Reset the clock & RAM
            _clock.SetClock(new DateTime(2014, 05, 31, 09, 30, 30));

            // Check if Oscillator is on and the RTC Set bit
            Debug.Print("RTC set? " + _clock.IsDateTimeSet());
            Debug.Print("OSC on? " + _clock.IsOscillatorOn() + "\n");

            // Turn on the output of the SquareWave Frequency Generator to blink the LED.
            _clock.SetSquareWave(RTC2Click.SqwFrequency.Sqw1Hz, RTC2Click.SqwOutputControl.On);

            new Thread(Capture).Start();
            
            Thread.Sleep(Timeout.Infinite);
        }

        private static void TestSquareWaves()
        {
            Debug.Print("1Hz frequency test");
            _clock.SetSquareWave(RTC2Click.SqwFrequency.Sqw1Hz, RTC2Click.SqwOutputControl.On);
            Thread.Sleep(10 * 1000);

            Debug.Print("4kHz frequency test");
            _clock.SetSquareWave(RTC2Click.SqwFrequency.Sqw_4KHz, RTC2Click.SqwOutputControl.On);
            Thread.Sleep(5 * 1000);

            Debug.Print("8kHz frequency test");
            _clock.SetSquareWave(RTC2Click.SqwFrequency.Sqw_8KHz, RTC2Click.SqwOutputControl.On);
            Thread.Sleep(5 * 1000);

            Debug.Print("32kHz frequency test");
            _clock.SetSquareWave(RTC2Click.SqwFrequency.Sqw32KHz, RTC2Click.SqwOutputControl.On);
            Thread.Sleep(5 * 1000);

            // Test the logic levels when the oscillator is off
            _clock.ClockHalt(true);

            // No frequency, square wave output pin pulled high
            Debug.Print("Square Wave disabled, square wave output pin pulled high");
            _clock.SetSquareWave(RTC2Click.SqwFrequency.SqwOff, RTC2Click.SqwOutputControl.On);
            Thread.Sleep(10 * 1000);

            // No frequency, square wave output pin pulled low
            Debug.Print("Square Wave disabled, square wave output pin pulled low");
            _clock.SetSquareWave(RTC2Click.SqwFrequency.SqwOff);
            Thread.Sleep(10 * 1000);

            // Resume the oscillator
            _clock.ClockHalt(false);
        }

        private static void Capture()
        {
            while (true)
            {
                var dt = _clock.GetDateTime();
                Debug.Print("Current RTC DateTime - " + dt + "\n");
                Thread.Sleep(1000);
            }
        } 
    }
}