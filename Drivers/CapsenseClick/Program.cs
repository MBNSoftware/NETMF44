using MBN;
using System;
using System.Threading;
using MBN.Modules;

namespace Examples
{
    public class Program
    {
        static CapSenseClick _cap;       // CapSense Click board
        static BarGraphClick _bar;       // BarGraph Click board

        public static void Main()
        {
            _cap = new CapSenseClick(Hardware.SocketOne);     // CapSense on socket 1 at address 0x00
            _bar = new BarGraphClick(Hardware.SocketTwo);                // BarGraph on socket 2

            _bar.Bars(0);                                            // Clear bars

            _cap.ButtonPressed += Cap_ButtonPressed;                 // Subscribe to the ButtonPressed event
            _cap.SliderDataChanged += Cap_SliderDataChanged;         // Subscribe to the SliderDataChanged event

            while (true)
            {
                _cap.CheckButtons();             // Checks if any button is pressed
                _cap.CheckSlider();              // Checks if slider value has changed
                Thread.Sleep(20);
            }
        }

        static void Cap_SliderDataChanged(object sender, CapSenseClick.SliderEventArgs e)
        {
            if (e.FingerPresent) { _bar.Bars((UInt16)(e.SliderValue / 5)); }     // Using default CapSense resolution of 50, displays 0 to 10 bars
        }

        static void Cap_ButtonPressed(object sender, CapSenseClick.ButtonPressedEventArgs e)
        {
            // Light on the led of the corresponding button when pressed (and switch off when depressed).
            _cap.LedBottom = e.ButtonBottom;
            _cap.LedTop = e.ButtonTop;
        }
    }
}

