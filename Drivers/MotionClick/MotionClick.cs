/*
 * MotionClick driver skeleton generated on 11/3/2014 5:36:12 PM
 * 
 * Initial version coded by Stephen Cardinale
 *  - Re-Trigger Only Mode. Non Re-Trigger not implemented due to unexpected behavior.
 *  - Night-Only mode not tested, but it should work as expected.
 * 
 * References needed: 
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Native
 *  MikroBusNet
 *  mscorlib
 *  
 * Copyright 2014 Stephen Cardinale
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 * 
 */

using MBN.Enums;
using MBN.Exceptions;
using Microsoft.SPOT.Hardware;
using System;
using System.Reflection;

namespace MBN.Modules
{

	/// <summary>
	/// Main class for the MotionClick driver. the Motion click is a Generic Module.
	/// <para><b>Pins used :</b> Rst, Int</para>
	/// </summary>
	/// <example>Example of how to use the Motion Click
	/// <code language = "C#">
	/// using MBN;
	/// using MBN.Modules;
	/// using Microsoft.SPOT;
	/// using Microsoft.SPOT.Hardware;
	/// using System;
	/// using System.Threading;
	///
	/// namespace MotionclickestApp
	/// {
	/// 	public class Program 
	/// 	{
	/// 		private static MotionClick motion;
	/// 		private static OutputPort led;
	/// 		private static Timer activityTimer;
	///
	/// 		public static void Main()
	/// 		{
	/// 			activityTimer = new Timer(LedMonitor, null, new TimeSpan(TimeSpan.MaxValue.Ticks), new TimeSpan(TimeSpan.MaxValue.Ticks));
	///
	/// 			led = new OutputPort(Pin.PB13, false); // LED is connected to Pin PB13 on the screw terminal of the Quail board.
	///
	/// 			motion = new MotionClick(Hardware.SocketFour);
	///
	/// 			motion.MotionDetected += motion_MotionDetected;
	///
	/// 			Thread.Sleep(Timeout.Infinite);
	/// 		}
	///
	/// 		private static void LedMonitor(object state)
	/// 		{
	/// 			led.Write(false);
	/// 			Debug.Print("LED should be off");
	/// 			activityTimer.Change(new TimeSpan(TimeSpan.MaxValue.Ticks), new TimeSpan(TimeSpan.MaxValue.Ticks));
	/// 		}
	///
	///
	/// 		static void motion_MotionDetected(object sender, MotionClick.MotionDetectedEventArgs e)
	/// 		{
	/// 			if (e.Activity)
	/// 			{
	/// 				led.Write(true);
	/// 				activityTimer.Change(new TimeSpan(TimeSpan.MaxValue.Ticks), new TimeSpan(TimeSpan.MaxValue.Ticks));
	/// 				Debug.Print("Activity Detected at " + e.EventTime + " turning LED on.");
	/// 			}
	/// 			else
	/// 			{
	/// 				activityTimer.Change(10000, 0);
	/// 				Debug.Print("No Activity Detected at " + e.EventTime + " LED will turn off in 10 seconds.");
	/// 			}
	/// 		}
	///
	/// 	}
	/// }
	/// </code>
	/// <code language = "VB">
	/// Option Explicit On
	/// Option Strict On
	///
	/// Imports MBN
	/// Imports MBN.Modules
	/// Imports Microsoft.SPOT.Hardware
	/// Imports Microsoft.SPOT
	/// Imports System
	/// Imports System.Threading
	///
	/// Namespace MotionclickestApp
	///
	/// 	Public Module Module1
	///
	/// 		Private WithEvents motion As MotionClick
	/// 		Private led As OutputPort
	/// 		Private activityTimer As Timer
	/// 		Private ReadOnly tcb As TimerCallback = New TimerCallback(AddressOf LedMonitor)
	///
	/// 		Sub Main()
	///
	/// 			activityTimer = New Timer(tcb, Nothing, New TimeSpan(TimeSpan.MaxValue.Ticks), New TimeSpan(TimeSpan.MaxValue.Ticks))
	///
	/// 			led = New OutputPort(Pin.PB13, False) ' LED is connected to Pin PB13 on the screw terminal of the Quail board.
	///
	/// 			motion = New MotionClick(Hardware.SocketFour)
	///
	/// 			Thread.Sleep(Timeout.Infinite)
	///
	/// 		End Sub
	///
	/// 		Private Sub LedMonitor(state As Object)
	/// 			led.Write(False)
	/// 			Debug.Print("LED should be off")
	/// 			activityTimer.Change(New TimeSpan(TimeSpan.MaxValue.Ticks), New TimeSpan(TimeSpan.MaxValue.Ticks))
	/// 		End Sub
	///
	/// 		Private Sub motion_MotionDetected(sender As Object, e As MotionClick.MotionDetectedEventArgs) Handles motion.MotionDetected
	/// 			If (e.Activity) Then
	/// 				led.Write(True)
	/// 				activityTimer.Change(New TimeSpan(TimeSpan.MaxValue.Ticks), New TimeSpan(TimeSpan.MaxValue.Ticks))
	/// 				Debug.Print("Activity Detected at " <![CDATA[&]]> e.EventTime.ToString() <![CDATA[&]]> " turning LED on.")
	/// 			Else
	/// 				activityTimer.Change(10000, 0)
	/// 				Debug.Print("No Activity Detected at " <![CDATA[&]]> e.EventTime.ToString() <![CDATA[&]]> " LED will turn off in 10 seconds.")
	/// 				End If
	/// 		End Sub
	///
	/// 	End Module
	///
	/// End Namespace
	/// </code>
	/// </example>
	public class MotionClick : IDriver
	{

		#region Fields

		private static OutputPort resetPin;

		#endregion

		#region CTOR

		/// <summary>
		/// Initializes a new instance of the <see cref="MotionClick"/> class.
		/// </summary>
		/// <param name="socket">The socket on which the MotionClick module is plugged on MikroBus.Net board</param>
		public MotionClick(Hardware.Socket socket)
		{
			try
			{
				Hardware.CheckPins(socket, socket.Rst, socket.Int);

				resetPin = new OutputPort(socket.Rst, true); // Non Re-trigger mode.
				var interruptPin = new InterruptPort(socket.Int, true, Port.ResistorMode.PullUp,
					Port.InterruptMode.InterruptEdgeBoth);
				interruptPin.OnInterrupt += interruptPin_OnInterrupt;
			}
			catch (PinInUseException)
			{
				throw new PinInUseException();
			}
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the power mode.
		/// </summary>
		/// <example> This sample shows how to use the PowerMode property.
		/// <code language="C#">Example usage:
		/// // None provided as this module does not support Power Modes.
		/// </code>
		/// <code language="VB">Example usage:
		/// ' None provided as this module does not support Power Modes.
		/// </code>
		/// </example>
		/// <exception cref="System.NotImplementedException">By setting this property, a NotImplementedException will be thrown as this module does not support Power Modes.</exception>
		public PowerModes PowerMode
		{
			get { return PowerModes.On; }
			set { throw new NotImplementedException("Power modes are not supported by this module."); }
		}

		/// <summary>
		/// Gets the driver version.
		/// </summary>
		/// <example> This sample shows how to use the DriverVersion property.
		/// <code language="C#">
		/// Debug.Print ("Current driver version : " + motion.DriverVersion);
		/// </code>
		/// <code language="VB">
		/// Debug.Print ("Current driver version : " <![CDATA[&]]> motion.DriverVersion);
		/// </code>
		/// </example>
		/// <value>
		/// The driver version.
		/// </value>
		public Version DriverVersion
		{
			get { return Assembly.GetExecutingAssembly().GetName().Version; }
		}

		#endregion

		#region Public Methods
		
		/// <summary>
		/// Resets the module
		/// </summary>
		/// <param name="resetMode">The reset mode :
		/// <para>SOFT reset : generally by sending a software command to the chip</para><para>HARD reset : generally by activating a special chip's pin</para></param>
		/// <exception cref="System.NotImplementedException">A NotImplementedException will be thrown as this module has no reset feature.</exception>
		public bool Reset(ResetModes resetMode)
		{
			throw new NotImplementedException("This module has no reset feature.");
		}

		#endregion
		
		#region Events

		private void interruptPin_OnInterrupt(uint data1, uint data2, DateTime time)
		{
			var tempEvent = MotionDetected;
			tempEvent(this, new MotionDetectedEventArgs(data2 == 1, time));
		}
		
		/// <summary>
		/// Occurs when motion is detected by the Motion Click.
		/// </summary>
		/// <remarks>
		/// When activity is detected by the Motion click, it will raise the <see cref="MotionDetected"/> event with the <see cref="MotionDetectedEventArgs.Activity"/> property set to true.
		/// When there is no additional activity within the range of the sensors detection range, the event will re-trigger with the <see cref="MotionDetectedEventArgs.Activity"/> property set to false.</remarks>
		public event MotionDetectedEventHandler MotionDetected = delegate { };

		/// <summary>
		/// Delegate for the Activity event.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="MotionDetectedEventArgs"/> instance containing the event data.</param>
		public delegate void MotionDetectedEventHandler(object sender, MotionDetectedEventArgs e);

		/// <summary>
		/// Class holding arguments for the Activity event.
		/// </summary>
		public class MotionDetectedEventArgs
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="MotionDetectedEventArgs"/> class.
			/// </summary>
			/// <param name="activity">True if the sensor was activated by motion, otherwise false.</param>
			/// <param name="eventTime">The time that the event took place.</param>
			public MotionDetectedEventArgs(Boolean activity, DateTime eventTime)
			{
				Activity = activity;
				EventTime = eventTime;
			}

			/// <summary>
			/// Gets the value of motion detection.
			/// </summary>
			/// <value>
			/// True if the Motion Click was activated by motion, otherwise false.
			/// </value>
			/// <remarks>
			/// When motion is detected by the Motion click, it will raise the <see cref="MotionDetected"/> event with this property set to true.
			/// When there is no additional activity within the range of the sensors detection range, the event will re-trigger with the property set to false.</remarks>
			public Boolean Activity { get; private set; }

			/// <summary>
			/// The time the event took place.
			/// </summary>
			public DateTime EventTime { get; private set; }
		}

		#endregion
	}
}

