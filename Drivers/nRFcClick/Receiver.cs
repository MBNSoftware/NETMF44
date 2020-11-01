using System.Text;
using MBN.Modules;
using Microsoft.SPOT;

namespace Examples
{
    public partial class Program
    {
        public static void StartReceiver()
        {
            _nrf.Configure(Encoding.UTF8.GetBytes("RCVR"), 1, NRFC.DataRate.DR250kbps);
            _nrf.OnDataReceived += nrf_OnDataReceived;
            _nrf.Enable();
        }

        private static void nrf_OnDataReceived(byte[] data)
        {
            Debug.Print("Received : "+ new string(Encoding.UTF8.GetChars(data)));
        }
    }
}
