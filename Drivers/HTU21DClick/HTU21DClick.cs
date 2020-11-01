/*
 * HTU21DClick driver for MikroBusNet
 *  Version 1.0
 *  - Initial version coded by Stephen Cardinale
 * 
 * References needed :
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

using System;
using System.Reflection;
using System.Threading;
using MBN.Enums;
using MBN.Exceptions;
using MBN.Extensions;
using Microsoft.SPOT.Hardware;

namespace MBN.Modules
{
	/// <summary>
	/// Main class for the HTU21DClick driver
	/// <para>This module is an I2C Device. <b>Pins used :</b> Scl, Sda</para>
	/// </summary>
	/// <example>Example usage:
	/// <code language = "C#">
	/// using MBN;
	/// using MBN.Enums;
	/// using MBN.Modules;
	/// using Microsoft.SPOT;
	/// using System.Threading;
	///
	/// namespace HTU21DClickTestApp
	/// {
	/// 	public class Program
	/// 	{
	/// 		private static HTU21DClick sensor;
	///
	/// 		public static void Main()
	/// 		{
	/// 			sensor = new HTU21DClick(Hardware.SocketTwo, ClockRatesI2C.Clock400KHz, 100);
	/// 			sensor.MeasurementMode = HTU21DClick.ReadMode.Hold;
	/// 			sensor.Resolution = HTU21DClick.DeviceResolution.UltraHigh;
	///
	/// 			while (true)
	/// 			{
	/// 				Debug.Print("Humidity    " + sensor.ReadHumidity(HumidityMeasurementModes.Relative).ToString("n2") + " %RH");
	/// 				Debug.Print("Temperature " + sensor.ReadTemperature(TemperatureSources.Ambient).ToString("n2") + " °C");
	/// 				Thread.Sleep(1000);
	/// 			}
	/// 		}
	/// 	}
	/// }	/// </code>
	/// <code language = "VB">
	/// Option Explicit On
	/// Option Strict On
	///
	/// Imports MBN.Enums
	/// Imports MBN
	/// Imports Microsoft.SPOT
	/// Imports MBN.Modules
	/// Imports System.Threading
	///
	/// Namespace HTU21DClickTestApp
	///
	/// 	Public Module Module1
	///
	/// 	Private sensor As HTU21DClick
	///
	/// 		Sub Main()
	///
	/// 			sensor = New HTU21DClick(Hardware.SocketTwo, ClockRatesI2C.Clock400KHz, 100)
	/// 			sensor.MeasurementMode = HTU21DClick.ReadMode.Hold
	/// 			sensor.Resolution = HTU21DClick.DeviceResolution.UltraHigh
	///
	/// 			While True
	/// 				Debug.Print("Humidity    " + sensor.ReadHumidity(HumidityMeasurementModes.Relative).ToString("n2") + " %RH")
	/// 				Debug.Print("Temperature " + sensor.ReadTemperature(TemperatureSources.Ambient).ToString("n2") + " °C")
	/// 				Thread.Sleep(1000)
	/// 			End While
	///
	/// 		End Sub
	///
	/// 	End Module
	///
	/// End Namespace
	/// </code>
	/// </example>
	public class HTU21DClick : IDriver, ITemperature, IHumidity
	{

		#region Constants
		
		private const Byte HTDU21D_WRITE_ADDRESS = 0x40; //0x80 >> 1; 
		private const Byte TRIGGER_TEMP_MEASURE_HOLD = 0xE3;
		private const Byte TRIGGER_HUMD_MEASURE_HOLD = 0xE5;
		private const Byte TRIGGER_TEMP_MEASURE_NOHOLD = 0xF3;
		private const Byte TRIGGER_HUMD_MEASURE_NOHOLD = 0xF5;
		private const Byte WRITE_USER_REG = 0xE6;
		private const Byte READ_USER_REG = 0xE7;
		private const Byte SOFT_RESET = 0xFE; 
        private const UInt32 SHIFTED_DIVISOR = 0x988000; // Used for CRC Checksum calculation.

		#endregion

		#region Fields
		
		private readonly I2CDevice.Configuration _config;
        private readonly int _timeout;
		private DeviceResolution _resolution = DeviceResolution.UltraHigh;
		private ReadMode _measurementMode = ReadMode.NoHold;

		#endregion
		
		#region ENUMS
		
		/// <summary>
		/// Enumeration for the measurement resolution settings for the HTU21DClick.
		/// </summary>
		public enum DeviceResolution : byte
		{

			/// <summary>
			/// 8-bit RH and 12-bit temperature. Measurement times are between 2 - 3 ms for Humidity and between 6 - 7 ms for Temperature.
			/// </summary>
			Low = 0,

			/// <summary>
			/// 10-bit RH and 13-bit temperature. Measurement times are between 4 - 5 ms for Humidity and between 11 - 13 ms for Temperature.
			/// </summary>
			Standard = 1,

			/// <summary>
			///  11-bit RH and 11-bit temperature. Measurement times are between 7 - 8 ms for Humidity and between 22 - 25 ms for Temperature.
			/// </summary>
			High = 2,
			
			/// <summary>
			///  12-bit RH and 14-bit temperature. Measurement times are between 14 - 16 ms for Humidity and between 44 - 50 ms for Temperature.
			/// <para>This is the default DeviceResolution upon power up and after reset.</para>
			/// </summary>
			UltraHigh = 3,

		}

		/// <summary>
		/// There are two different operation modes to communicate with the HTU21DClick, Hold Master mode and No Hold Master mode.
		/// </summary>
		/// <remarks>In the NoHold mode allows for processing other I²C communication tasks on a bus while the HTU21D sensor is measuring.
		/// In the Hold mode, the HTU21D(F) pulls down the SCK line while measuring to force the master into a wait state. By releasing the SCK line, the HTU21D Click indicates that internal processing is completed and that transmission may be continued.</remarks>
		public enum ReadMode : byte
		{
			/// <summary>
			/// Hold Master mode, the HTU21D(F) pulls down the SCK line while measuring to force the master into a wait state. By releasing the SCK line, the HTU21D Click indicates that internal processing is completed and that transmission may be continued.
			/// </summary>
			Hold = 0,
			/// <summary>
			/// No Hold Master mode allows for processing other I²C communication tasks on a bus while the HTU21D(F) sensor is measuring.
			/// </summary>
			NoHold = 1
		}

		internal enum MeasurementType
		{
			Humidity = 0,
			Temperature = 1
		}

		#endregion

		#region CTOR

		/// <summary>
		/// Initializes a new instance of the <see cref="HTU21DClick"/> class.
		/// </summary>
		/// <param name="socket">The socket on which the HTU21DClick module is plugged on MikroBus.Net board</param>
		/// <param name="clockRateKHz">Optional - The clock rate of the I²C device. <seealso cref="ClockRatesI2C"/> Defaults to 400KHz.</param>
		/// <param name="timeout">OPtional - The I2C Transaction timeout before returning. Defaults to 1000 ms.</param>
		public HTU21DClick(Hardware.Socket socket, ClockRatesI2C clockRateKHz = ClockRatesI2C.Clock400KHz, int timeout = 1000)
		{
			try
			{
				Hardware.CheckPinsI2C(socket);

				_config = new I2CDevice.Configuration(HTDU21D_WRITE_ADDRESS, (Int32)clockRateKHz);
				_timeout = timeout;

				Reset(ResetModes.Soft);

			}
			// Catch only the PinInUse exception, so that program will halt on other exceptions and send it directly to the caller.
			catch (PinInUseException) { throw new PinInUseException(); }
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or Sets the reading resolution of the HUD212D click. See <see cref="DeviceResolution"/> for additional information.
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		/// sensor.Resolution = HTU21DClick.DeviceResolution.Standard;
		/// </code>
		/// <code language = "VB">
		/// sensor.Resolution = HTU21DClick.DeviceResolution.Standard
		/// </code>
		/// </example>
		public DeviceResolution Resolution
		{
			get
			{
				var userRegister = ReadUserRegister();

				if (!IsBitSet(userRegister, 7) && IsBitSet(userRegister, 0)) return DeviceResolution.Low;
				if (IsBitSet(userRegister, 7) && !IsBitSet(userRegister, 0)) return DeviceResolution.Standard;
				if (IsBitSet(userRegister, 7) && IsBitSet(userRegister, 0)) return DeviceResolution.High;
				if (!IsBitSet(userRegister, 7) && !IsBitSet(userRegister, 0)) return DeviceResolution.UltraHigh;
				
				return DeviceResolution.UltraHigh;
			}
			set
			{
				_resolution = value;

				byte userRegister = ReadUserRegister();
				
				switch (value)
				{
					case DeviceResolution.UltraHigh:
					{
						userRegister = Bits.Set(userRegister, 7, false);
						userRegister = Bits.Set(userRegister, 0, false);
						break;
					}
					case DeviceResolution.High:
					{
						userRegister = Bits.Set(userRegister, 7, true);
						userRegister = Bits.Set(userRegister, 0, true);
						break;
					}
					case DeviceResolution.Standard:
					{
						userRegister = Bits.Set(userRegister, 7, true);
						userRegister = Bits.Set(userRegister, 0, false);

						break;
					}
					case DeviceResolution.Low:
					{
						userRegister = Bits.Set(userRegister, 7, false);
						userRegister = Bits.Set(userRegister, 0, true);
						break;
					}
				}

				var command = new byte[2];
				command[0] = WRITE_USER_REG;
				command[1] = userRegister;

				WriteRegister(command);
			}
		}

		/// <summary>
		/// Gets the driver version.
		/// </summary>
		/// <example> This sample shows how to use the DriverVersion property.
		/// <code language="C#">
		/// Debug.Print ("Current driver version : " + sensor.DriverVersion);
		/// </code>
		/// <code language = "VB">
		/// Debug.Print ("Current driver version : " <![CDATA[&]]> sensor.DriverVersion)
		/// </code>
		/// </example>
		/// <value>
		/// The driver version.
		/// </value>
		public Version DriverVersion
		{
			get { return Assembly.GetExecutingAssembly().GetName().Version; }
		}

		/// <summary>
		/// Gets the End of Battery (low voltage detection) status of the HTD21D Click. Detects and notifies of VDD voltages below 2.25V. Accuracy is ±0.1V.
		/// </summary>
		/// <returns>True is VDD is below 2.25, otherwise false.</returns>
		/// <remarks>The EndOfBattery Bit in the UserRegister is only updated after an measurement is complete. This method is included for completeness of the driver. Usage is not practical as the HUD21DClick is powered by the MBN main board and will be supplied a constant 3.3V VDD. If the HTU21D is powered remotely, this might come in handy.</remarks>
		/// <example>Example usage:
		/// <code language="C#">
		/// Debug.Print("End Of Battery : " + sensor.EndOfBattery);
		/// </code>
		/// <code language="VB">
		/// Debug.Print("End Of Battery : " <![CDATA[&]]> sensor.EndOfBattery);
		/// </code>
		/// </example>
		public bool EndOfBattery
		{
			get
			{
				var userRegister = ReadUserRegister();
				return IsBitSet(userRegister, 6);
			}
		}

		/// <summary>
		/// Enable or disables the on-chip heater of the HTU21D Click.
		/// </summary>
		/// <remarks>The heater is intended to be used for functionality diagnosis: relative humidity drops upon rising temperature. The heater consumes about 5.5mW and provides a temperature increase of about 0.5-1.5°C.</remarks> 
		/// <returns>True if the on-chip heater is enabled or otherwise false.</returns>
		/// <example>Example usage:
		/// <code language="C#">
		/// sensor.Heater = true;
		/// Debug.Print("Heater is -  " + sensor.Heater ? "On" : "Off");
		/// sensor.Heater = false;
		/// Debug.Print("Heater is -  " + sensor.Heater ? "On" : "Off");
		/// </code>
		/// <code language="VB">
		/// sensor.Heater = True
		/// Debug.Print(If("Heater is -  " <![CDATA[&]]> sensor.HeaterStatus, "On", "Off"))
		/// sensor.Heater = False
		/// Debug.Print(If("Heater is -  " <![CDATA[&]]> sensor.HeaterStatus, "On", "Off"))
		/// </code>
		/// </example>
		public bool Heater
		{
			get
			{
				var userRegister = ReadUserRegister();
				return IsBitSet(userRegister, 2) ? true : false;
			}
			set
			{
				var userRegister = ReadUserRegister();
				userRegister = Bits.Set(userRegister, 2, value);

				var command = new byte[2];
				command[0] = WRITE_USER_REG;
				command[1] = userRegister;

				WriteRegister(command);
			}
		}

		/// <summary>
		/// Sets the OTPReload (One-time Programmable Memory) setting.
		/// </summary>
		/// <value>If set to false, deactivates reloading the calibration data before each measurement.</value>
		/// <remarks>OTP reload is a safety feature and load the entire OTP settings to the register, with the exception of the heater bit, before every measurement. This feature is disabled per default and it is not recommended for use. Please use soft reset instead as it contains OTP reload.</remarks>
		/// <example>Example usage:
		/// <code language="C#">
		/// sensor.OTPReload = true;
		/// Debug.Print("OTPReload : " + sensor.OTPReload);
		/// </code>
		/// <code language="VB">
		/// sensor.OTPReload = true
		/// Debug.Print("OTPReload : " <![CDATA[&]]> sensor.OTPReload);
		/// </code>
		/// </example>
		public bool OTPReload
		{
			get
			{
				var userRegister = ReadUserRegister();
				return IsBitSet(userRegister, 1);
			}
			set
			{
				var userRegister = ReadUserRegister();
				userRegister = Bits.Set(userRegister, 1, value);

				var command = new byte[2];
				command[0] = WRITE_USER_REG;
				command[1] = userRegister;

				WriteRegister(command);
			}
		}
		
		/// <summary>
		/// Sets or gets the communication mode with the HTU21D click. See <see cref="ReadMode"/> for additional information.
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		/// sensor.MeasurementMode = HTU21DClick.ReadMode.Hold;
		/// </code>
		/// <code language = "VB">
		/// sensor.MeasurementMode = HTU21DClick.ReadMode.Hold
		/// </code>
		/// </example>
		public ReadMode MeasurementMode
		{
			get { return _measurementMode; }
			set { _measurementMode = value; }
		}	
			
		/// <summary>
		/// Gets or sets the power mode.
		/// </summary>
		/// <example>Example usage:
		/// <code language="C#">
		///  // None provided as this module does not support PowerModes.
		/// </code>
		/// <code language="VB">
		///  ' None provided as this module does not support PowerModes.
		/// </code>
		/// </example>
		/// <value>
		/// The current power mode of the module.
		/// </value>
		/// <exception cref="System.NotImplementedException">A NotImplementedException will be thrown if the Set accessor for this property is attempted.</exception>
		public PowerModes PowerMode
		{
			get { return PowerModes.On; }
			set
			{
				throw new NotImplementedException("PowerModes are not supported on this module");
			}
		}

		/// <summary>
		/// Gets the raw data of the humidity value.
		/// </summary>
		/// <value>
		/// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
		/// </value>
		/// <exception cref="System.NotImplementedException">A NotImplementedException will be thrown if the Get accessor for this property is attempted.</exception>
		/// <example>Example usage:
		/// <code language="C#">
		///  // None provided as this module does not support PowerModes.
		/// </code>
		/// <code language="VB">
		///  ' None provided as this module does not support PowerModes.
		/// </code>
		/// </example>
		int IHumidity.RawData
		{
			get { throw new NotImplementedException("RawData is not supported by this module."); }
		}

		/// <summary>
		/// Gets the raw data of the temperature value.
		/// </summary>
		/// <value>
		/// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
		/// </value>
		/// <exception cref="System.NotImplementedException">A NotImplementedException will be thrown if the Get accessor for this property is attempted.</exception>
		/// <example>Example usage:
		/// <code language="C#">
		///  // None provided as this module does not support PowerModes.
		/// </code>
		/// <code language="VB">
		///  ' None provided as this module does not support PowerModes.
		/// </code>
		/// </example>
		int ITemperature.RawData
		{
			get { throw new NotImplementedException("RawData is not supported by this module."); }
		}

		#endregion

		#region Public Methods
		
		/// <summary>
		/// Returns the temperature as read from the HTU21D Click in degrees Celsius.
		/// </summary>
		/// <param name="source">The measurement source. See <see cref="TemperatureSources"/> for more information.</param>
		/// <returns>Temperature in degrees Celsius</returns>
		/// <exception cref="NotImplementedException">A NotImplementedException will be thrown is attempting to read <see cref="TemperatureSources.Object"/></exception>
		/// <example>Example usage:
		/// <code language = "C#">
		/// Debug.Print("Temperature " + sensor.ReadTemperature(TemperatureSources.Ambient).ToString("n2") + " °C");
		/// </code>
		/// <code language = "VB">
		/// Debug.Print("Temperature " <![CDATA[&]]> sensor.ReadTemperature(TemperatureSources.Ambient).ToString("n2") <![CDATA[&]]> " °C");
		/// </code>
		/// </example>
		public float ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
		{
			if (source == TemperatureSources.Object) throw new NotImplementedException("Object temperature measurement not supported. Use TemperatureSources.Ambient for temperature measurement.");

			return ReadSensor(_measurementMode, MeasurementType.Temperature);
		}

		/// <summary>
 		/// Returns the humidity as read from the HTU21D Click in degrees % RH.
		/// </summary>
		/// <param name="measurementMode">The measurement mode. See <see cref="HumidityMeasurementModes"/> for more information.</param>
		/// <returns>The humidity as read from the HTU21D click in %RH.</returns>
 		/// <exception cref="NotImplementedException">A NotImplementedException will be thrown is attempting to read <see cref="HumidityMeasurementModes.Absolute"/></exception>
		/// <example>Example usage:
		/// <code language = "C#">
		/// Debug.Print("Humidity - " + sensor.ReadHumidity(HumidityMeasurementModes.Relative).ToString("n2") + " %RH");
		/// </code>
		/// <code language = "VB">
		/// Debug.Print("Humidity - " <![CDATA[&]]> sensor.ReadHumidity(HumidityMeasurementModes.Relative).ToString("n2") <![CDATA[&]]> " %RH")
		/// </code>
		/// </example>
		public float ReadHumidity(HumidityMeasurementModes measurementMode = HumidityMeasurementModes.Relative)
		{
			if (measurementMode == HumidityMeasurementModes.Absolute) throw new NotImplementedException("Absolute Humidity measurement not supported. Use HumidityMeasurementModes.Absolute for humidity measurement.");

			return ReadSensor(_measurementMode, MeasurementType.Humidity);
		}

		/// <summary>
		/// Resets the HTU21D Click.
		/// </summary>
		/// <param name="resetMode">The reset mode :
		/// <para>SOFT reset : generally by sending a software command to the chip</para><para>HARD reset : generally by activating a special chip's pin.</para></param>
		/// <returns>True if the reset was successful, or otherwise false.</returns>
		/// <exception cref="System.NotImplementedException">A NotImplementedException will be thrown if a <see cref="ResetModes.Hard"/> is attempted. Use <see cref="ResetModes.Soft"/> to reset this module.</exception>
		/// <example>Example usage:
		/// <code language = "C#">
		/// sensor.Reset(ResetModes.Soft);
		/// </code>
		/// <code language = "VB">
		/// sensor.Reset(ResetModes.Soft)
		/// </code>
		/// </example>
		public bool Reset(ResetModes resetMode)
		{
			if (resetMode == ResetModes.Hard) throw new NotImplementedException("Hard resets are not supported. Use ResetModes.Soft to reset this module");

			var buffer = new byte[1];
			buffer[0] = SOFT_RESET;
			var returnvalue = WriteRegister(buffer);
			Thread.Sleep(15);

			return returnvalue;
		}

		#endregion

		#region Private Methods
		
		private void ReadWait(byte[] readBuffer)
		{
			var bytesRead = 0;

			var xActions = new I2CDevice.I2CTransaction[1];
			xActions[0] = I2CDevice.CreateReadTransaction(readBuffer);

			while (bytesRead != readBuffer.Length)
			{
				bytesRead = Hardware.I2CBus.Execute(_config, xActions, _timeout);
			}
		}

		private Boolean WriteRegister(byte[] writeBuffer)
		{
			var xActions = new I2CDevice.I2CTransaction[1];
			xActions[0] = I2CDevice.CreateWriteTransaction(writeBuffer);
			var written = Hardware.I2CBus.Execute(_config, xActions, _timeout);
			return written != 0;
		}

		private Byte[] WriteReadRegister(byte[] writeBuffer, byte[] readBuffer)
		{
			var xActions = new I2CDevice.I2CTransaction[2];
			xActions[0] = I2CDevice.CreateWriteTransaction(writeBuffer);
			xActions[1] = I2CDevice.CreateReadTransaction(readBuffer);
			var read = Hardware.I2CBus.Execute(_config, xActions, _timeout);
			return readBuffer;
		}

		private float ReadSensor(ReadMode mode, MeasurementType type)
		{
			var writeBuffer = new byte[1];
			var readBuffer = new byte[3];

			switch (mode)
			{
				case ReadMode.Hold:
					{
						//writeBuffer[0] = TRIGGER_HUMD_MEASURE_HOLD;
						writeBuffer[0] = type == MeasurementType.Temperature ? TRIGGER_TEMP_MEASURE_HOLD : TRIGGER_HUMD_MEASURE_HOLD;
						readBuffer = WriteReadRegister(writeBuffer, readBuffer);
						break;
					}
				case ReadMode.NoHold:
					{
						//writeBuffer[0] = TRIGGER_HUMD_MEASURE_NOHOLD;
						writeBuffer[0] = type == MeasurementType.Temperature ? TRIGGER_TEMP_MEASURE_NOHOLD : TRIGGER_HUMD_MEASURE_NOHOLD;
						WriteRegister(writeBuffer);
						ReadWait(readBuffer);
						break;
					}
			}

			var raw = ((uint)readBuffer[0] << 8) | readBuffer[1];

			if (CheckCrc(raw, readBuffer[2]) != 0) return (float.MinValue); // Checksum fail.

			raw &= 0xFFFC;

			var tempValue = raw / (float)65536;

			return type == MeasurementType.Temperature ? (float)(-46.85 + (175.72 * tempValue)) : -6 + (125 * tempValue);

		}

		private byte CheckCrc(uint value, uint checksum)
		{
			UInt32 remainder = value << 8;
			remainder &= checksum;

			UInt32 divsor = SHIFTED_DIVISOR;

			for (int i = 0; i < 16; i++)
			{
				if ((remainder & (uint)1 << (23 - i)) == 1) remainder ^= divsor;
				divsor >>= 1;
			}

			return (byte)remainder;

		}

		private byte ReadUserRegister()
		{
			var userRegister = new byte[1];
			var writeRegister = new byte[1];
			writeRegister[0] = READ_USER_REG;
			WriteReadRegister(writeRegister, userRegister);
			return userRegister[0];
		}

		private static bool IsBitSet(Byte value, Byte pos)
		{
			return (value & (1 << pos)) != 0;
		}

		#endregion
	}
}

