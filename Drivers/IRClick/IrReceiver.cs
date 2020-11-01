using System;
using Microsoft.SPOT.Hardware;

namespace MBN.Modules
{
    /// <summary>
    /// Abstract class representing IR remote receiver
    /// </summary>
    abstract public class IrReceiver : IDisposable
    {
        /// <summary>
        /// Get or set pin used for IR demodulator
        /// </summary>
        protected Cpu.Pin ReceiverPin;

		public abstract void Dispose();

        /// <summary>
        /// Data received delegate
        /// </summary>
        /// <param name="sender">Instance where event occurred</param>
        /// <param name="command">Command received</param>
        /// <param name="address">Address received</param>
        internal delegate void IrDataHandler(IrReceiver sender, int command, int address, DateTime time);
        
        /// <summary>
        /// Event occurs when data is received
        /// </summary>
        internal virtual event IrDataHandler DataReceived;

	    internal void OnDataReceived(int command, int address)
        {
            var handler = DataReceived;
            if (handler != null) handler(this, command, address, DateTime.Now);
        }  
    }
}
