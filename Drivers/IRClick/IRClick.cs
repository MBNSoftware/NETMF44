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
using System.Collections;
using System.Reflection;
using MBN.Enums;
using MBN.Exceptions;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace MBN.Modules
{
	/// <summary>
	/// A MikroBusNet Driver for the MikroE Ir Click.
	/// <para><b>This module is a Generic Device.</b></para>
	/// <para><b>Pins used :</b> An, Pwm, Rx, Tx</para>
	/// <para><b>References required:</b> MikroBus.Net, Microsoft.SPOT.Native, Microsoft.Spot.Hardware, mscorlib</para>
	/// <para>For a information about Sony IR Protocol Remotes and Commands see <see href="http://www.rockabilly.net/sony/sony_home.htm"> the ultimate source for Sony remote control codes.</see></para>
	/// </summary>
	/// <remarks>
	/// This driver only decodes NEC RC5 and Sony IR IR Signals. IR Encoding support is not provided by this driver.
	/// <para>Many Sony IR Remotes send multiple DeviceID codes depending on the function. Similiar to a Universal Remote.</para>
	/// </remarks>
	/// <example>Example usage:
	/// <code language = "C#">
	/// using System.Threading;
	/// using MBN;
	/// using MBN.Modules;
	/// using Microsoft.SPOT;
	///
	/// namespace Examples
	/// {
	/// 	public class Program
	/// 	{
	/// 		private static IRClick _ir;
	///
	/// 		public static void Main()
	/// 		{
	/// 			Debug.Print("Startup");
	/// 			_ir = new IRClick(Hardware.SocketThree);
	/// 			_ir.IRSignalReceived += _ir_IRSignalReceived;
	///
	/// 			Thread.Sleep(Timeout.Infinite);
	/// 		}
	///
	/// 		static void _ir_IRSignalReceived(object sender, IRClick.IREventArgs e)
	/// 		{
	/// 			Debug.Print("IR Event - Button detected - " + e.Button + " at " + e.ReadTime);
	/// 		}
	/// 	}
	/// }
	/// </code>
	/// <code language = "VB">
	/// Option Explicit On
	/// Option Strict On
	///
	/// Option Explicit On
	/// Option Strict On
	///
	/// Imports MBN
	/// Imports MBN.Modules
	/// Imports System.Threading
	/// Imports Microsoft.SPOT
	///
	/// Namespace Examples
	/// 	Public Module Module1
	/// 		Dim WithEvents _ir As IRClick
	///
	/// 		Sub Main()
	/// 			_ir = New IRClick(Hardware.SocketThree)
	/// 			Thread.Sleep(Timeout.Infinite)
	/// 		End Sub
	///
	/// 		Private Sub _ir_IRSignalReceived(sender As Object, e As IRClick.IREventArgs) Handles _ir.IRSignalReceived
	/// 			Debug.Print("IR Event - Button detected - " <![CDATA[&]]> e.Button <![CDATA[&]]> " at " <![CDATA[&]]> e.ReadTime)
	/// 		End Sub
	///
	/// 	End Module
	/// End Namespace
	/// </code>
	/// </example>
	public class IRClick : IrReceiver, IDriver
	{

		#region Fields
		
		private readonly Cpu.Pin _receiverPin;
		private static IrReceiver _instance;
		private static Hashtable _devicesHashTable;

		#endregion

		#region ENUMS
		
		/// <summary>
		/// The IR Protocol to use for IR detection. 
		/// </summary>
		public enum Protocol
		{
			/// <summary>
			/// Use the NEC IR Remote Protocol
			/// </summary>
			NEC = -1,
			/// <summary>
			/// Unknown IR Protocol
			/// </summary>
			Unknown = 0,
			/// <summary>
			/// Use the Sony IR Remote Protocol
			/// </summary>
			Sony = 1
		}

		#endregion        

        #region CTOR

		/// <summary>
		/// Initializes a new instance of the <see cref="IRClick" /> class.
		/// </summary>
		/// <param name="socket">The socket that this module is plugged in to.</param>
		/// <exception cref="PinInUseException">A PinInUseException will be thrown if the Pwm, An, Tx, Rx pins are being used by another driver in a stacked module configuration of the same socket.</exception>
		public IRClick(Hardware.Socket socket)
        {
            try
            {
                Hardware.CheckPins(socket, socket.Pwm, socket.An, socket.Tx, socket.Rx);

				CreateDevicesHashTable();

	            _receiverPin = socket.An;

	            IRProtocol = Protocol.NEC;

            }
            // Catch only the PinInUse exception, so that program will halt on other exceptions and send it directly to caller
            catch (PinInUseException ex)
            {
                throw new PinInUseException(ex.Message);
            }
        }

		public override void Dispose()
		{
			if (_instance != null) _instance.Dispose();
		}

		#endregion

		#region Private Methods
		
		void IrDataReceived(IrReceiver sender, int command, int address, DateTime time)
		{
			OnIrEvent(new IrEventArgs { IrProtocol = IRProtocol, DeviceType = address, Button = command, ReadTime = time });
		}

		private static void CreateDevicesHashTable()
		{
			_devicesHashTable = new Hashtable
			{
				{-1, "Generic NEC IR Remote"},
				{0, "Unknown IR Remote"},
				{1, "Sony TV IR Remote"},
				{2, "Sony CamCorder or WebCam IR Remote"},
				{3, "Sony TeleText IR Remote"},
				{6, "Sony LaserDisc IR Remote"},
				{7, "Sony VCR IR Remote"},
				{8, "Sony HD IR Remote"},
				{9, "Sony VCR or BetaCam or VideoPlayer IR Remote"},
				{11, "Sony Digital Camera IR remote"},
				{12, "Sony AV Receiver IR Remote"},
				{13, "Sony Tuner IR Remote"},
				{14, "Sony Cassette Player  IR Remote"},
				{15, "Sony MiniDisc IR Remote"},
				{16, "Sony AV Receiver IR Remote"},
				{17, "Sony CD Player IR Remote"},
				{18, "Sony Equalizer IR Remote"},
				{19, "Unknown Audio Device IR Remote "},
				{23, "Sony Digital Still Video Recorder IR Remote"},
				{26, "Sony DVD Player IR Remote"},
				{28, "Sony DAT IR Remote"},
				{30, "Sony Video Conference Controller IR Remote"},
				{52, "Sony Laser Video Disc IR Remote"},
				{68, "Sony BoomBox IR Remote"},
				{84, "Sony Projector IR Remote"},
				{89, "Sony DSR IR Remote"},
				{132, "Sony Car Stereo IR Remote"},
				{183, "Sony Satellite Receiver IR Remote"},
				{223, "Sony DV Camera IR Remote"},
				{228, "Sony AC Switch IR Remote"},
				{247, "Sony In-Car DVD Player IR Remote"},
				{249, "Sony Color Video Printer IR Remote"}
			};
		}

        #endregion

		#region Public Properties

		/// <summary>
		/// Gets the string representation of the IR Remote Device Type.
		/// </summary>
		/// <param name="deviceId">The DeviceID returned by the <see cref="IrSignalReceived"/> event.</param>
		public String GetIRDeviceName(int deviceId)
		{
			if (!_devicesHashTable.Contains(deviceId)) return "Unknown IR Remote";
			foreach (DictionaryEntry de in _devicesHashTable)
			{
				if ((int)de.Key == deviceId) return de.Value.ToString();
			}
			return "Unknown IR Remote";
		}

		/// <summary>
		/// Gets or sets the power mode.
		/// </summary>
		/// <value>
		/// The current power mode of the module.
		/// </value>
		/// <remarks>If the module has no power modes, then GET should always return PowerModes.ON while SET should throw a NotImplementedException.</remarks>
		/// <exception cref="NotImplementedException">This module has no power mode setting. If using the set accessor, it will throw an exception.</exception>
		/// <example>Example usage: None.</example>
		public PowerModes PowerMode
		{
			get
			{
				return PowerModes.On;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets the driver version.
		/// </summary>
		/// <value>
		/// The driver version see <see cref="Version"/>.
		/// </value>
		/// <example>Example usage:
		/// <code language="C#">
		/// Debug.Print("Driver Version Info : " + _clock1.DriverVersion);
		/// </code>
		/// <code language="VB">
		/// Debug.Print("Driver Version Info : " <![CDATA[&]]> _clock1.DriverVersion)
		/// </code>
		/// </example>
		public Version DriverVersion
		{
			get { return Assembly.GetAssembly(GetType()).GetName().Version; }
		}

		/// <summary>
		/// Gets or sets the IR Decoder Protocol to use for decoding.
		/// <para>Use NEC for NEC IR Remote Controls or Sony for Sony IR Remote controls. </para>
		/// </summary>
		/// <remarks>Defaults to <see cref="Protocol.NEC"/>.</remarks>
		/// <returns></returns>
		public Protocol IRProtocol { get; set; }

		#endregion

		#region Public Methods
		
		/// <summary>
		/// Resets the Ir Click.
		/// </summary>
		/// <param name="resetMode">The reset mode, see <see cref="ResetModes"/> for more information.</param>
		/// <remarks>
		/// This module has no Reset method, calling this method will throw an exception.
		/// </remarks>
		/// <exception cref="NotImplementedException">This module does not implement a reset method. Calling this method will throw a <see cref="NotImplementedException"/></exception>
		/// <example>None: This module does not support a Reset method.</example>
		public bool Reset(ResetModes resetMode)
		{
			throw new NotImplementedException("This module does not implement a Reset method.");
		}

		public void StartReceiver()
		{
			if (IRProtocol == Protocol.Sony)
			{
				IrReceiver sony = new SonyReceiver(_receiverPin);
				_instance = sony;
				sony.DataReceived += IrDataReceived;
			}
			else
			{
				IrReceiver nec = new NECReceiver(_receiverPin);
				_instance = nec;
				nec.DataReceived += IrDataReceived;
			}
		}

		public void StopReceiver()
		{
			_instance.Dispose();
		}

		#endregion

		#region Events

		/// <summary>
		/// Represents the delegate that is used to handle the <see cref="IRClick.IrSignalReceived" /> event.
		/// </summary>
		/// <param name="sender">The IR Click that sends the event.</param>
		/// <param name="e">Event information, including the button that was pressed and the time it was pressed.</param>
		public delegate void IrSignalReceivedEventDelegate(object sender, IrEventArgs e);

		/// <summary>
		/// The event that is raised when the IRclick detects an IR signal.
		/// </summary>
		public event IrSignalReceivedEventDelegate IrSignalReceived;

		private void OnIrEvent(IrEventArgs e)
		{
			if (e.DeviceType == -1 && e.Button == 0 ) return; 
			if (IrSignalReceived != null) IrSignalReceived(this, e);
		}

		/// <summary>
		/// Class that holds the information about the button when a button press is detected.
		/// </summary>
		public class IrEventArgs : EventArgs
		{
			/// <summary>
			/// The IR Device Type either NEC or Sony.
			/// </summary>
			public Protocol IrProtocol { get; set; }

			/// <summary>
			/// The type of IR remote device sending the IR pulses.
			/// <remarks>In the case of a NEC IR Remote, the DeviceType will be -1. In the case of a Sony IR Remote, the value returned will be the type of remote sending the IR Pulses. See the <see cref="SonyIRDevices"/> enumeration for possible devices.</remarks>
			/// </summary>
			public int DeviceType { get; set; }
			/// <summary>
			/// The button what was pressed.
			/// </summary>
			public int Button { get; set; }

			/// <summary>
			/// The <see cref="DateTime"/> that the button was pressed.
			/// </summary>
			public DateTime ReadTime { get; set; }
		}

		#endregion		

    }
}