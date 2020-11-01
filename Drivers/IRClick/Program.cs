using System.Threading;
using MBN;
using MBN.Modules;
using Microsoft.SPOT;

namespace Examples
{
	public class Program
	{
		private static IRClick _ir;

		public static void Main()
		{
			_ir = new IRClick(Hardware.SocketFour);

			_ir.IRProtocol = IRClick.Protocol.NEC;

			_ir.IrSignalReceived += _ir_IrSignalReceived;

			_ir.StartReceiver();
			Debug.Print("Use a NEC Protocol Remote");
			Debug.Print("In 20 seconds we will switch Protocol to Sony.");

			Thread.Sleep(20000);

			_ir.StopReceiver();

			_ir.IRProtocol = IRClick.Protocol.Sony;

			_ir.StartReceiver();
			Debug.Print("Use a Sony Protocol Remote");

			Thread.Sleep(Timeout.Infinite);
		}

		static void _ir_IrSignalReceived(object sender, IRClick.IrEventArgs e)
		{
			var deviceString = _ir.GetIRDeviceName(e.DeviceType);

			Debug.Print("IR Event: IR Protocol - " + (e.IrProtocol == IRClick.Protocol.NEC ? "NEC" : "Sony") + ", IR Device Type - " + deviceString + ", Button Pressed - " + e.Button + " occurred at " + e.ReadTime);
		}
	}
}
