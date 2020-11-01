using MBN.Modules;
using MBN;
using MBN.Exceptions;
using System.Threading;
using Microsoft.SPOT;

namespace Examples
{
    public class Program
    {
        private static SpeakUpClick _speakUp;

        public static void Main()
        {
            try
            {
                _speakUp = new SpeakUpClick(Hardware.SocketTwo);
            }
            catch (PinInUseException)
            {
                Debug.Print("Error initializing SpeakUp Click board.");
            }

            _speakUp.SpeakDetected +=_speakUp_SpeakDetected;
            _speakUp.Listening = true;

            Thread.Sleep(Timeout.Infinite);
        }

        private static void _speakUp_SpeakDetected(object sender, SpeakUpClick.SpeakUpEventArgs e)
        {
            Debug.Print("SpeakUp has detected an order, index : " +e.Command);
        }
    }
}
