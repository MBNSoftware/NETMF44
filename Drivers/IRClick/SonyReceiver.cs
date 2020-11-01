/*
 * IR Click MBN Driver
 * 
 * Version 1.0 :
 *  - Initial release coded by Stephen Cardinale
 *
 * References needed :
 *   - Microsoft.SPOT.Hardware
 *   - Microsoft.SPOT.Native
 *   - MikroBus.Net
 *   - mscorlib
 *  
 * Copyright 2014 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace MBN.Modules
{
	internal class SonyReceiver : IrReceiver
	{
		
		#region Fields
		
		private const int DataLength = 12;
		private const int StartSequence = 20000;
		private const int LogicalOne = 12000;
		private readonly Thread _mainThread;
		private readonly long[] _pulses;
		private bool _runningThread; 
		private bool _disposing = false;

		private readonly InputPort _irPort;

		#endregion

		#region CTOR/DTOR
		
		// Creates a new instance of the Sony IR Receiver class
		internal SonyReceiver(Cpu.Pin receiverPin)
		{
			//ReceiverPin = receiverPin;
			_irPort = new InputPort(receiverPin, false, Port.ResistorMode.PullDown);

			_pulses = new long[DataLength];
			_runningThread = true;

			_mainThread = new Thread(DoWork);
			_mainThread.Start();

		}


		/// <summary>
		/// Disposes the Sony IR object.
		/// </summary>
		public override void Dispose()
		{
			_runningThread = false;
			_disposing = true;
			_mainThread.Join(1000);

			while (_mainThread.IsAlive)
			{
				Thread.Sleep(250);
			}
			_irPort.Dispose();
		}

		#endregion
		
		#region Private Methods
		
		private void DoWork()
		{
			while (_runningThread && !_disposing)
			{
				if (_disposing) return;

				int address;
				int command;
				Receive(out command, out address);
				OnDataReceived(command, address);

				Thread.Sleep(250);
			}
			Debug.Print("DoWork has terminated");
		}

		private void Receive(out int command, out int address)
		{
			address = 0;
			command = 0;

			while (PulseIn(_irPort, false) < StartSequence)
			{
				if (_disposing) return;
			}

			for (var i = 0; i < DataLength; i++)
			{
				if (_irPort== null) continue;
				_pulses[i] = PulseIn(_irPort, false);
			}

			DecodePulses(_pulses, out command, out address);
		}

		private static void DecodePulses(long[] pulses, out int command, out int address)
		{
			command = 0;
			address = 0;
			byte mask = 0;

			// Decode command
			for (var i = 0; i < 7; i++)
			{
				mask = (pulses[i] > LogicalOne) ? (byte) 1 : (byte) 0;
				mask <<= i;

				command |= mask;
			}

			// Decode address
			for (var i = 7; i < DataLength; i++)
			{
				mask = (pulses[i] > LogicalOne) ? (byte) 1 : (byte) 0;
				mask <<= (i - 7);

				address |= mask;
			}
		}

		private long PulseIn(Port port, bool state)
		{
			try
			{
				while (port.Read() != state)
				{
					if (_disposing) return 0L;
				}

				var startTime = DateTime.Now;

				while (port.Read() == state)
				{
					if (_disposing) return 0L;
				}

				var delta = DateTime.Now - startTime;

				return delta.Ticks;
			}
			catch (ObjectDisposedException)
			{
				// This shouldn't happen.
			}
			return 0;
		}

		#endregion

	}
}