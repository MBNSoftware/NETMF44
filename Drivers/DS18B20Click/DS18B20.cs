/*
 * DS18B20 Click board driver
 * 
 * Version 1.0 :
 *  - Initial revision coded by Stephen Cardinale
 * 
 * References needed :
 *  Microsoft.SPOT.OneWire
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

using System.Collections;
using MBN.Events;
using MBN.Exceptions;
using Microsoft.SPOT.Hardware;

// ReSharper disable once CheckNamespace
namespace MBN
{
    /// <summary>
    /// </summary>
    public class Ds18B20
    {
        #region Constants

        private const byte MatchRom = 0x55;
        private const byte StartTemperatureConversion = 0x44;
        private const byte ReadScratchPad = 0xBE;

        #endregion

        #region Fields

        private readonly OneWire _ow;
        private readonly Hashtable _temperature = new Hashtable();
        private ArrayList _list = new ArrayList();

        /// <summary>
        ///     The TemperatureMeasured Event. This event is raised whenever you call the <see cref="ReadTemperature" /> function.
        /// </summary>
        public event TemperatureMeasured TemperatureEventFired = delegate { };

        #endregion

        #region CTOR

        /// <summary>
        ///     Default Costructor.
        /// </summary>
        /// <param name="socket"></param>
        public Ds18B20(Hardware.Socket socket)
        {
            try
            {
                Hardware.CheckPins(socket, socket.An);

                using (var outPort = new OutputPort(socket.An, false))
                {
                    _ow = new OneWire(outPort);
                }
                ReadTemperature();
            }
                // Catch only the PinInUse exception, so that program will halt on other exceptions
                // Send it directly to caller
            catch (PinInUseException ex)
            {
                throw new PinInUseException("The An Pin is alreasy in use on " + socket.Name + " , try moving Click Board to another available socket.", ex);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///     The ReadOnly Property that holds a HashTable of the last temperature
        ///     reading taken of all DS18B20 sensors on the One-wire Bus.
        /// </summary>
        /// <remarks>
        ///     The HashTable contains a Key/Value Pair of the unique 64-Bit Address of the DS18B20 and the temperature in °C.
        ///     C#
        ///     ---------------------------------------------------USAGE--------------------------------------------------------------------
        ///     foreach (DictionaryEntry t in temperature) { Debug.Print("Key - " + t.Key.ToString() + " Value - " +
        ///     t.Value.ToString()); }
        ///     ----------------------------------------------------------------------------------------------------------------------------
        /// </remarks>
        public Hashtable Temperature
        {
            get { return _temperature; }
        }

        #endregion

        #region Methods

        #region Private

        private double ReadTemperture(byte[] owAddress)
        {
            var scratchpad = new byte[9];
            _ow.TouchReset();
            _ow.WriteByte(MatchRom);
            for (int i = 0; i <= 7; i++)
            {
                _ow.WriteByte(owAddress[i]);
            }
            _ow.WriteByte(StartTemperatureConversion);
            while (_ow.ReadByte() == 0)
            {
            }
            _ow.TouchReset();
            _ow.WriteByte(MatchRom);
            for (int i = 0; i <= 7; i++)
            {
                _ow.WriteByte(owAddress[i]);
            }
            _ow.WriteByte(ReadScratchPad);
            for (int i = 0; i <= 8; i++)
            {
                scratchpad[i] = (byte) _ow.ReadByte();
            }
            short tempLow = scratchpad[0];
            short tempHigh = scratchpad[1];
            long temptemp = (((long) tempHigh) << 8) | ((uint) tempLow);
            float temperatureC = temptemp*0.0625f;
            return temperatureC;
        }

        #endregion

        #region Public

        /// <summary>
        ///     Reads all DS18B20 Temperature Sensors on the OneWire Bus and raises the <see cref="TemperatureMeasured" /> event.
        /// </summary>
        /// <remarks>
        ///     With multiple DS18B20 devices on the OneWire Bus, the <seealso cref="TemperatureMeasured" /> event will only
        ///     be raised about every three seconds regardless if this method is called more often than every 3 seconnds.
        ///     This is due to the time it takes to poll the OneWire Bus with multiple devices.
        /// </remarks>
        public void ReadTemperature()
        {
            lock (Temperature.SyncRoot)
            {
                _list.Clear();
                Temperature.Clear();

                _list = _ow.FindAllDevices();

                if (_list.Count <= 0) return;

                foreach (byte[] s in _list)
                {
                    Temperature.Add(s[0] + "-" + s[1] + "-" + s[2] + "-" + s[3] + "-" + s[4] + "-" + s[5] + "-" + s[6] + "-" + s[7], ReadTemperture(s));
                }

                TemperatureMeasured tempEvent = TemperatureEventFired;
                tempEvent(this, Temperature);
            }
        }

        #endregion

        #endregion
    }
}