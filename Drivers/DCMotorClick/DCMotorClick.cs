/*
 * DCMotor Click board driver
 * 
 * Version 2.0 :
 *  - Conformance to the new namespaces and organization
 *  
 * Version 1.0 :
 *  - Initial revision coded by Christophe Gerbier
 * 
 * References needed :
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Hardware.PWM
 *  Microsoft.SPOT.Native
 *  MikroBusNet
 *  mscorlib
 *  
 * Copyright 2014 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System;
using System.Reflection;
using System.Threading;
using MBN.Enums;
using MBN.Exceptions;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace MBN.Modules
{
// ReSharper disable once InconsistentNaming
    /// <summary>
    /// Main class for the DCMotor Click board driver
    /// <para><b>Pins used :</b> An, Rst, Cs, Pwm, Int</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///    static DCMotorClick _motor;
    ///
    ///    public static void Main()
    ///    {
    ///        _motor = new DCMotorClick(Hardware.SocketOne);
    ///
    ///        _motor.OnFault += Motor_OnFault;
    ///
    ///        _motor.Move(DCMotorClick.Directions.Forward, 1.0, 2000);
    ///        Thread.Sleep(5000);
    ///        _motor.Stop(2000);
    ///
    ///        while (_motor.IsMoving) { Thread.Sleep(10); }
    ///
    ///        Debug.Print("Sleep");
    ///        _motor.Sleeping = true;
    ///     }
    /// }
    /// </code>
    /// </example>
    public class DCMotorClick : IDriver
    {
        /// <summary>
        /// Main event handler for the <see cref="OnFault"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public delegate void EventHandler(object sender, EventArgs e);

        /// <summary>
        /// Occurs when a fault has been detected, mainly overcurrent.
        /// </summary>
        public event EventHandler OnFault = delegate { };               // Fault event raised to user

        /// <summary>
        /// Directions of the motor moves
        /// </summary>
        public enum Directions 
        {
            /// <summary>
            /// Move forward.
            /// </summary>
            Forward,
            /// <summary>
            /// Move backward.
            /// </summary>
            Backward 
        };

// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly InterruptPort _fault;
// ReSharper disable once NotAccessedField.Local
        private OutputPort _select1;
        private readonly OutputPort _select2;
        private readonly OutputPort _sleep;
        private readonly PWM _pwmOut;
        private PowerModes _powerMode;

        // Internal variables
        private Int32 _rampIncrement, _rampWaitTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="DCMotorClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the DCMotor Click board is plugged on MikroBus.Net</param>
        /// <param name="frequency">The frequency of the PWM output. Default value is 1000 Hz</param>
        /// <param name="dutyCycle">The initial duty cycle of PWM. Default to 0.0 %, that is : motor stopped</param>
        public DCMotorClick(Hardware.Socket socket, Double frequency = 1000.0, Double dutyCycle = 0.0)
        {
            try
            {
                Hardware.CheckPins(socket, socket.An, socket.Rst, socket.Cs, socket.Pwm, socket.Int);

                // Select1/2 : selection of decay modes. Only Fast decay implemented here.
                _select1 = new OutputPort(socket.Rst, false);
                _select2 = new OutputPort(socket.Cs, false);
                _sleep = new OutputPort(socket.An, true);                                // Sleep mode : OFF by default
                _pwmOut = new PWM(socket.PwmChannel, frequency, dutyCycle, false);       // PWM Output
                _fault = new InterruptPort(socket.Int, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeLevelLow);      // Fault interrupt line
                _fault.OnInterrupt += Fault_OnInterrupt;                                 // Subscribe to event in order to detect a fault
                _fault.EnableInterrupt();                                                // Enable interrupts
                IsMoving = false;                                                      // Motor not running
                _powerMode = PowerModes.On;
            }
            // Catch only the PinInUse exception, so that program will halt on other exceptions
            // Send it directly to caller
            catch (PinInUseException) { throw new PinInUseException(); }
        }

        /// <summary>
        /// Gets a value indicating whether the motor is currently moving.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the motor is moving; otherwise, <c>false</c>.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///     while (_motor.IsMoving) { Thread.Sleep(10); }
        /// </code>
        /// </example>
        
        public bool IsMoving { get; private set; }

        /// <summary>
        /// Moves the motor in the specified direction.
        /// </summary>
        /// <param name="direction">The direction : forward or backward.</param>
        /// <param name="speed">The speed, from 0.0 to 1.0 (100%)</param>
        /// <param name="rampTime">The ramp time if needed, in milliseconds. It's the time that will be taken to start from speed 0.0 to "Speed".</param>
        /// <example>
        /// <code language="C#">
        ///     // Moves the motor forward with a ramptime to full speed of 2 sec
        ///     _motor.Move(DCMotorClick.Directions.Forward, 1.0, 2000);
        /// </code>
        /// </example>
        public void Move(Directions direction, Double speed = 1.0, Int32 rampTime = 0)
        {
            if (IsMoving) { _pwmOut.Stop(); Thread.Sleep(200); }
            _select2.Write(direction == Directions.Backward);
            if (rampTime == 0) { _pwmOut.DutyCycle = speed; IsMoving = true; _pwmOut.Start(); }
            else
            {
                _rampIncrement = (Int32)(speed/0.05);
                _rampWaitTime = (Int32)(rampTime * 0.05 / speed);
                IsMoving = true;
                new Thread(RampUp).Start();
            }

        }

        /// <summary>
        /// Stops the motor.
        /// </summary>
        /// <param name="rampTime">The ramp time if needed, in milliseconds. It's the time that will be taken to decrease speed from "actual speed" to 0.0.</param>
        /// <example>
        /// <code language="C#">
        ///     // Stops the motor with a rampdown time of 2 sec
        ///     _motor.Stop(2000);
        /// </code>
        /// </example>
        public void Stop(Int32 rampTime=0)
        {
            if (!IsMoving) { return; }
            if (rampTime == 0) { _pwmOut.Stop(); IsMoving = false; }
            else
            {
                _rampWaitTime = (Int32)(rampTime * 0.05 / _pwmOut.DutyCycle);
                new Thread(RampDown).Start();
            }
        }

        #region Private methods
        private void RampUp()
        {
            Int32 i = 0;
            _pwmOut.Start();
            while (i < _rampIncrement)
            {
                _pwmOut.DutyCycle += 0.05;
                Thread.Sleep(_rampWaitTime);
                i++;
            }
        }

        private void RampDown()
        {
            while (_pwmOut.DutyCycle > 0)
            {
                _pwmOut.DutyCycle -= 0.05;
                Thread.Sleep(_rampWaitTime);
            }
            _pwmOut.Stop();
            IsMoving = false;
        }

        private void Fault_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            var tempEvent = OnFault;
            tempEvent(this, null);
        }
        #endregion

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">This module does not have a PowerModes.Off feature.</exception>
        /// <example>
        /// <code language="C#">
        ///     _motor.PowerMode = PowerModes.On;
        /// </code>
        /// </example>
        public PowerModes PowerMode
        {
            get { return _powerMode; }
            set
            {
                if (value == PowerModes.Off) { throw new NotImplementedException("PowerModes.Off");}
                _powerMode = value;
                _sleep.Write(value == PowerModes.Low);
            }
        }

        /// <summary>
        /// Gets the driver version.
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///      Debug.Print ("Current driver version : "+_motor.DriverVersion);
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
        /// Herited from the IDriver interface but not used by this module.
        /// </summary>
        /// <param name="resetMode">The reset mode :
        /// <para>SOFT reset : generally by sending a software command to the chip</para><para>HARD reset : generally by activating a special chip's pin</para></param>
        /// <returns>True if Reset has been acknowledged, false otherwise.</returns>
        /// <exception cref="System.NotImplementedException">Thrown because this module has no Reset feature.</exception>
        public bool Reset(ResetModes resetMode)
        {
            throw new NotImplementedException("Reset");
        }
    }
}

