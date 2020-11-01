using System.Threading;
using MBN;
using MBN.Modules;

namespace Examples
{
    public partial class Program
    {
        static NRFC _nrf;

        public static void Main()
        {
            _nrf = new NRFC(Hardware.SocketOne);

            StartReceiver();
            //StartSender();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
