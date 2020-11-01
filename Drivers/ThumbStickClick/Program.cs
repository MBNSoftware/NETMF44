using MBN;
using MBN.Modules;
using Microsoft.SPOT;
using System.Threading;

namespace ThumbstickClickTestApp
{
    public class Program
    {

        private static ThumbstickClick _thumb;

        public static void Main()
        {
            _thumb = new ThumbstickClick(Hardware.SocketFour);

            _thumb.ThumbstickOrientation = ThumbstickClick.Orientation.Rotate90Degrees;

            _thumb.ThumbstickPressed += _thumb_ThumbstickPressed;
            _thumb.ThumbstickReleased += _thumb_ThumbstickReleased;

            _thumb.Calibrate();

            while (true)
            {
                var position = _thumb.GetPosition();
                Debug.Print("Current X: " + position.X.ToString("f2"));
                Debug.Print("Current Y: " + position.Y.ToString("f2"));

                Thread.Sleep(1000);
            }

            //Thread.Sleep(Timeout.Infinite);
        }

        static void _thumb_ThumbstickReleased(ThumbstickClick sender, ThumbstickClick.ButtonState state)
        {
            Debug.Print("Thumbstick Released");
        }

        static void _thumb_ThumbstickPressed(ThumbstickClick sender, ThumbstickClick.ButtonState state)
        {
            Debug.Print("Thumbstick Pressed");
        }
    }
}
