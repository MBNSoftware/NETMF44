/*
 * Keypad4X4 driver
 * 
 * Version 1.0 :
 *  - Initial revision coded by Christophe Gerbier
 * 
 * References needed :
 *  Microsoft.SPOT.Hardware
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
using Microsoft.SPOT.Hardware;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the Keypad4x3 driver
    /// <para><b>Pins used :</b> Various GPIOs, depending on user's choice</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    ///	   public class Program
    ///    {
    ///        private static Keypad4X3 _keypad;
    ///
    ///        public static void Main()
    ///        {
    ///            // Pins PE5..PE2 are connected to rows, PD7..PD3 to columns
    ///            _keypad = new Keypad4X3(Pin.PE5, Pin.PE4, Pin.PE3, Pin.PE2, Pin.PD7, Pin.PD4, Pin.PD3);
    ///            _keypad.KeyReleased += KeypadKeyReleased;
    ///            _keypad.KeyPressed += KeypadKeyPressed;
    ///
    ///            _keypad.StartScan();
    ///
    ///            Thread.Sleep(Timeout.Infinite);
    ///        }
    ///
    ///        static void KeypadKeyPressed(object sender, Keypad4X3.KeyPressedEventArgs e)
    ///        {
    ///            Hardware.Led1.Write(true);
    ///        }
    ///
    ///        static void KeypadKeyReleased(object sender, Keypad4X3.KeyReleasedEventArgs e)
    ///        {
    ///            Debug.Print("Key : " + e.KeyChar);
    ///            Hardware.Led1.Write(false);
    ///        }
    ///    }
    /// </code>
    /// </example>
    public partial class Keypad4X3 : IDriver
    {
        /// <summary>
        /// Occurs when a key is pressed
        /// </summary>
        public event KeyPressedEventHandler KeyPressed = delegate { };
        /// <summary>
        /// Occurs when a key is released
        /// </summary>
        public event KeyReleasedEventHandler KeyReleased = delegate { };

        private static OutputPort[] _rows;
        private static InputPort[] _columns;
        private Thread _scanThread;
        private Boolean _scanThreadActive;

        /// <summary>
        /// Initializes a new instance of the <see cref="Keypad4X3"/> class.
        /// </summary>
        /// <param name="row1">Pin connected to the first row</param>
        /// <param name="row2">Pin connected to the second row</param>
        /// <param name="row3">Pin connected to the third row</param>
        /// <param name="row4">Pin connected to the fourth row</param>
        /// <param name="column1">Pin connected to the first column</param>
        /// <param name="column2">Pin connected to the second column</param>
        /// <param name="column3">Pin connected to the third column</param>
        public Keypad4X3(Cpu.Pin row1, Cpu.Pin row2, Cpu.Pin row3, Cpu.Pin row4, Cpu.Pin column1, Cpu.Pin column2, Cpu.Pin column3)
        {
            _rows = new []
            {
                new OutputPort(row1, false),
                new OutputPort(row2, false),
                new OutputPort(row3, false),
                new OutputPort(row4, false)
            };

            _columns = new []
            {
                new InputPort(column1, false, Port.ResistorMode.PullDown),
                new InputPort(column2, false, Port.ResistorMode.PullDown),
                new InputPort(column3, false, Port.ResistorMode.PullDown)
            };
        }

        /// <summary>
        /// Enables keys scanning
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///     _keypad.StartScan();
        /// </code>
        /// </example>
        public void StartScan()
        {
            if (_scanThreadActive) { return; }
            _scanThreadActive = true;
            _scanThread = new Thread(ScanThreadMethod);
            _scanThread.Start();
        }

        /// <summary>
        /// Disables keys scanning
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///     _keypad.StopScan();
        /// </code>
        /// </example>
        public void StopScan()
        {
            _scanThreadActive = false;
        }

        private void ScanThreadMethod()
        {
            var prevKey = -1;
            while (_scanThreadActive)
            {
                var nbKey = 0;
                // Scans the matrix
                for (var i = 0; i < 4; i++)
                {
                    for (var j = 0; j < 3; j++)
                    {
                        if (!ReadMatrix(i, j)) continue;
                        // A key has been pressed
                        var keyNum = (i * 3) + j + 1;
                        nbKey += keyNum;
                        if ((prevKey != keyNum) && (prevKey == -1))  // A key has been pressed and no other is currently pressed (avoids dealing with multiple keys at the same time)
                        {
                            prevKey = keyNum;
                            var tempEvent = KeyPressed;
                            tempEvent(this, new KeyPressedEventArgs(keyNum, KeytoChar(keyNum)));
                        }
                        break;
                    }
                }
                if (nbKey == 0)  // No key pressed in this pass
                {
                    // Was there a key pressed before ?
                    if (prevKey != -1)
                    {
                        var tempEvent = KeyReleased;
                        tempEvent(this, new KeyReleasedEventArgs(prevKey, KeytoChar(prevKey)));
                    }
                    prevKey = -1;
                }
                // Leave time for other processes
                Thread.Sleep(50);
            }
        }

        private static bool ReadMatrix(int row, int column)
        {
            _rows[row].Write(true);
            var colState = _columns[column].Read();
            _rows[row].Write(false);

            return colState;
        }

        private static char KeytoChar(int keyValue)
        {
            char c;
            if (keyValue < 10) { c = (char)(keyValue + 48); }
            else
            {
                switch (keyValue)
                {
                    case 10: c = '*';
                        break;
                    case 11: c = '0';
                        break;
                    case 12: c = '#';
                        break;
                    default: c = ' ';   // Should never happen
                        break;
                }
            }

            return c;
        }

        /// <summary>
        /// Do you really expect a driver to return a string ?! Come on ! ;-)
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that contains information, though.
        /// </returns>
        public override string ToString()
        {
            return "'Keypad 4x3' driver for MBN boards, by Christophe Gerbier. Based on GHI's original driver code.";
        }

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">Thrown because this module has no power mode feature.</exception>
        public PowerModes PowerMode
        {
            get { return PowerModes.On; }
            set
            {
                throw new NotImplementedException("PowerMode");
            }
        }

        /// <summary>
        /// Gets the driver version.
        /// </summary>
        /// <value>
        /// The driver version.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///      Debug.Print ("Current driver version : "+_keypad.DriverVersion);
        /// </code>
        /// </example>
        public Version DriverVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        /// <summary>
        /// Resets the module
        /// </summary>
        /// <param name="resetMode">The reset mode :
        /// <para>SOFT reset : generally by sending a software command to the chip</para><para>HARD reset : generally by activating a special chip's pin</para></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException">Thrown because this module has no reset feature.</exception>
        public bool Reset(ResetModes resetMode)
        {
            throw new NotImplementedException("Reset");
        }
    }
}

