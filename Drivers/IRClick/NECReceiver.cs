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
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace MBN.Modules
{
	internal class NECReceiver : IrReceiver
	{

		#region Fields

		private static long _lastTick;
		private static long _bitTime;
		private static int _pattern;
		private static bool _streaming;
		private static int _shiftBit;
		private static bool _newPress;
		private static InputPort _irPort;

		#endregion

		#region CTOR/DTOR
		
		// Creates a new instance of the NEC IR Receiver class
		internal NECReceiver(Cpu.Pin receiverPin)
		{
			_newPress = false;
			_lastTick = DateTime.Now.Ticks;

			_irPort = new InterruptPort(receiverPin, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);

			_irPort.OnInterrupt += IrPortOnInterrupt;
			_irPort.EnableInterrupt();
		}

		public override void Dispose()
		{
			_irPort.Dispose();
			Debug.Print("NEC Receiver is Disposed");
		}

		#endregion

		#region Private Methods

		private void IrPortOnInterrupt(uint data1, uint data2, DateTime time)
		{
			DecodeNecProtocol(data2, time);
		}

		private void DecodeNecProtocol(uint data2, DateTime time)
		{
			_bitTime = time.Ticks - _lastTick;
			_lastTick = time.Ticks;

			if (_bitTime > 1000000) _newPress = true;

			if (_bitTime > 26670)
			{
				_bitTime = 0;
				_pattern = 0;

				if (data2 == 0)
				{
					_streaming = true;
					_shiftBit = 1;
					_pattern |= _shiftBit;
				}
				else
				{
					_streaming = false;
				}
				return;
			}

			if (_streaming)
			{
				if (_bitTime > 10668)
				{
					_shiftBit = (int) (data2 == 0 ? 1U : 0U);
					_pattern <<= 1;
					_pattern |= _shiftBit;
				}
				else
				{
					if (data2 == 0)
					{
						_pattern <<= 1;
						_pattern |= _shiftBit;
					}
				}

				if ((_pattern & 0x2000) > 0)
				{
					if (_newPress)
					{
						OnDataReceived(_pattern & 0x3F, -1);
						_newPress = false;
					}

					_pattern = 0;
					_bitTime = 0;
					_streaming = false;
				}
			}
		}

		#endregion

	}
}