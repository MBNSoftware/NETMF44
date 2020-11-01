/*
 * Onboard Flash driver
 *
 * Version 2.4 : Christophe Gerbier
 *  - Removed the FastRead command as it was not reliable
 *  - Added the EraseBlock() implementation (new Storage class needs it)
 *  - Implemented the PowerMode and Reset methods
 *  - Added default values
 *  - Added private AutoDetect() method to detect the standard properties values from the JEDEC portion of the chip
 *  
 * Version 2.1 :
 *  - Fixed bug in WriteData that was not using PageSize
 *  - Fixed bug in ReadData where returned data was "shifted" by one byte, thus missing one byte
 *  - Added FastRead command when more that one byte has to be read from Flash
 * 
 * Version 2.0 :
 *  - Completely rewritten to comply to the new Storage class definition
 *  
 * Version 1.0
 *  - Initial revision, by Christophe Gerbier
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
using System.Text;
using MBN.Enums;
using MBN.Extensions;
using Microsoft.SPOT.Hardware;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the Onboard Flash memory driver
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///     private static Storage _flash;
    ///
    ///     public static void Main()
    ///     {
    ///        _flash = new OnboardFlash();
    ///
    ///        Debug.Print("Address 231 before : " + _flash.ReadByte(231));
    ///        _flash.WriteByte(231, 123);
    ///        Debug.Print("Address 231 after : " + _flash.ReadByte(231));
    ///
    ///        _flash.WriteData(400, new Byte[] { 100, 101, 102 }, 0, 3);
    ///        _flash.ReadData(400, bArray, 0, 3);
    ///        Debug.Print("Read 3 bytes starting @400 (should be 100, 101, 102) : " + bArray[0] + ", " + bArray[1] + ", " + bArray[2]);
    ///     }
    /// </code>
    /// </example>
    public class OnboardFlash : Storage, IDriver
    {
        private Int32 _PageSize = 0x100, _SectorSize = 0x1000, _BlockSize = 0x10000, _Capacity = 0x800000;
        private Byte _SectorEraseInstruction = 0x20;
        private Byte _BlockEraseInstruction = 0xD8;

        private PowerModes _PowerMode = PowerModes.On;

        /// <summary>
        /// Gets the memory capacity.
        /// </summary>
        /// <value>
        /// The maximum capacity, in bytes.
        /// </value>
        public override Int32 Capacity
        {
            get { return _Capacity; }
        }

        /// <summary>
        /// Gets the size of a page in memory.
        /// </summary>
        /// <value>
        /// The size of a page in bytes
        /// </value>
        public override Int32 PageSize
        {
            get { return _PageSize; }
        }

        /// <summary>
        /// Gets the size of a sector.
        /// </summary>
        /// <value>
        /// The size of a sector in bytes.
        /// </value>
        public override Int32 SectorSize
        {
            get { return _SectorSize; }
        }

        /// <summary>
        /// Gets the size of a block.
        /// </summary>
        /// <value>
        /// The size of a block in bytes.
        /// </value>
        public override Int32 BlockSize
        {
            get { return _BlockSize; }
        }

        private readonly SPI.Configuration _spiConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="OnboardFlash"/> class.
        /// </summary>
        /// <exception cref="InvalidOperationException">No Flash memory on board</exception>
        public OnboardFlash()
        {
            if (!Hardware.FlashAvailable) { throw new InvalidOperationException("No Flash memory on board"); }
            _spiConfig = new SPI.Configuration(Pin.PA13, false, 0, 0, false, true, 30000, SPI.SPI_module.SPI3);
            if (Hardware.SPIBus == null) { Hardware.SPIBus = new SPI(_spiConfig); }
            PowerMode = PowerModes.On;
            DetectParameters();
        }

        /// <summary>
        /// Completely erases the chip.
        /// </summary>
        /// <remarks>
        /// This method is mainly used by Flash memory chips, because of their internal behaviour. It can be safely ignored with other memory types.
        /// </remarks>
        /// <example>
        ///   <code language="C#">
        /// public class Program
        /// {
        /// private static Storage _storage;
        /// public static void Main()
        /// {
        /// _storage = new OnboardFlash();
        /// _storage.EraseChip();
        /// }
        /// </code>
        /// </example>
        public override void EraseChip()
        {
            if (PowerMode == PowerModes.Off) return;

            while (!WriteEnabled) { }
            Hardware.SPIBus.Write(_spiConfig, new Byte[] { 0xC7 });
            while (WriteInProgress()) { }
        }

        /// <summary>
        /// Erases "count" sectors (4KB) starting at "sector".
        /// </summary>
        /// <param name="sector">The starting sector.</param>
        /// <param name="count">The count of sectors to erase.</param>
        /// <exception cref="ArgumentException">Invalid sector + count</exception>
        /// <example>
        ///   <code language="C#">
        /// public class Program
        /// {
        /// private static Storage _storage;
        /// public static void Main()
        /// {
        /// _storage = new OnboardFlash();
        /// _storage.EraseSector(10,1);
        /// }
        /// </code>
        /// </example>
        public override void EraseSector(Int32 sector, Int32 count)
        {
            if (PowerMode == PowerModes.Off) return;

            var data4 = new Byte[4];

            if ((sector + count) * SectorSize > Capacity) throw new ArgumentException("Invalid sector + count");
            var address = sector * SectorSize;
            for (var i = 0; i < count; i++)
            {
                while (!WriteEnabled) { }
                data4[0] = _SectorEraseInstruction;
                data4[1] = (Byte)(address >> 16);
                data4[2] = (Byte)(address >> 8);
                data4[3] = (Byte)(address >> 0);
                Hardware.SPIBus.Write(_spiConfig, data4);
                while (WriteInProgress()) { }
                address += SectorSize;
            }
        }

        /// <summary>
        /// Erases "count" blocks (64KB) starting at "block".
        /// </summary>
        /// <param name="block">The starting block.</param>
        /// <param name="count">The count of blocks to erase.</param>
        /// <exception cref="ArgumentException">Invalid block + count</exception>
        /// <example>
        ///   <code language="C#">
        /// public class Program
        /// {
        /// private static Storage _storage;
        /// public static void Main()
        /// {
        /// _storage = new OnboardFlash();
        /// _storage.EraseBlock(10,1);
        /// }
        /// </code>
        /// </example>
        public override void EraseBlock(Int32 block, Int32 count)
        {
            if (PowerMode == PowerModes.Off) return;

            var data4 = new Byte[4];

            if ((block + count) * BlockSize > Capacity) throw new ArgumentException("Invalid block + count");
            var address = block * BlockSize;
            for (var i = 0; i < count; i++)
            {
                while (!WriteEnabled) { }
                data4[0] = _BlockEraseInstruction;
                data4[1] = (Byte)(address >> 16);
                data4[2] = (Byte)(address >> 8);
                data4[3] = (Byte)(address >> 0);
                Hardware.SPIBus.Write(_spiConfig, data4);
                while (WriteInProgress()) { }
                address += BlockSize;
            }
        }

        /// <summary>
        /// Writes data to a memory location.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="array">The data to write.</param>
        /// <param name="index">The starting index in the data array.</param>
        /// <param name="count">The count of bytes to write to memory.</param>
        /// <example>
        ///   <code language="C#">
        /// public class Program
        /// {
        /// private static Storage _storage;
        /// public static void Main()
        /// {
        /// _storage = new OnboardFlash();
        /// _storage.WriteData(400, new Byte[] { 100, 101, 102 }, 0, 3);
        /// _storage.ReadData(400, bArray, 0, 3);
        /// Debug.Print("Read 3 bytes starting @400 (should be 100, 101, 102) : " + bArray[0] + ", " + bArray[1] + ", " + bArray[2]);
        /// }
        /// </code>
        /// </example>
        /// <exception cref="System.ArgumentException">
        /// Invalid index + count
        /// or
        /// Invalid address + count
        /// </exception>
        public override void WriteData(Int32 address, Byte[] array, Int32 index, Int32 count)
        {
            if (PowerMode == PowerModes.Off) return;

            var len = _PageSize - (address & 0xFF); // remaining of first page
            while (count > 0)
            {
                if (len > count) len = count;
                var wr_cmd = new Byte[len + 4];
                wr_cmd[0] = 0x02;
                wr_cmd[1] = (Byte)(address >> 16);
                wr_cmd[2] = (Byte)(address >> 8);
                wr_cmd[3] = (Byte)address;

                while (!WriteEnabled) { }
                Array.Copy(array, index, wr_cmd, 4, len);
                Hardware.SPIBus.Write(_spiConfig, wr_cmd);
                while (WriteInProgress()) { }
                address += len;
                count -= len;
                index += len;
                len = _PageSize;
                wr_cmd = null;
            }

        }

        /// <summary>
        /// Reads the data.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="array">The array.</param>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <exception cref="ArgumentException">
        /// Invalid index + count
        /// or
        /// Invalid address + count
        /// </exception>
        public override void ReadData(Int32 address, Byte[] array, Int32 index, Int32 count)
        {
            if (PowerMode == PowerModes.Off) return;

            var data4 = new Byte[4];

            while (!WriteEnabled) { }
            data4[0] = 0x03;
            data4[1] = (Byte)(address >> 16);
            data4[2] = (Byte)(address >> 8);
            data4[3] = (Byte)(address >> 0);

            Hardware.SPIBus.WriteRead(_spiConfig, data4, 0, 4, array, index, count, 4);
        }

        private Boolean WriteInProgress()
        {
            if (PowerMode == PowerModes.Off) return false;

            var data2 = new Byte[2];

            Hardware.SPIBus.WriteRead(_spiConfig, new Byte[] { 0x05 }, data2);
            return (data2[1] & 0x01) != 0x00;
        }

        private Boolean WriteEnabled
        {
            get
            {
                if (PowerMode == PowerModes.Off) return false;

                var data2 = new Byte[2];

                Hardware.SPIBus.Write(_spiConfig, new Byte[] { 0x06 });
                Hardware.SPIBus.WriteRead(_spiConfig, new Byte[] { 0x05 }, data2);
                return (data2[1] & 0x02) != 0;
            }
        }

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">This module does not support PowerModes.Low</exception>
        public PowerModes PowerMode
        {
            get { return _PowerMode; }
            set
            {
                if (value == PowerModes.Low) throw new NotSupportedException("PowerModes.Low");
                Hardware.SPIBus.Write(_spiConfig, value == PowerModes.Off
                        ? new Byte[] { 0xB9 }
                        : new Byte[] { 0xAB, 0x00, 0x00, 0x00, 0x00 });
                _PowerMode = value;
            }
        }

        /// <summary>
        /// Gets the driver version.
        /// </summary>
        /// <example> This sample shows how to use the DriverVersion property.
        /// <code language="C#">
        ///             Debug.Print ("Current driver version : "+_OnboardFlash.DriverVersion);
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
        /// <exception cref="System.NotImplementedException">Thrown because this module does not support hardware reset.</exception>
        public bool Reset(ResetModes resetMode)
        {
            if (resetMode != ResetModes.Soft) throw new NotSupportedException();
            if (PowerMode == PowerModes.Off) return false;

            // Enable software reset
            while (!WriteEnabled) { }
            Hardware.SPIBus.Write(_spiConfig, new Byte[] { 0x66 });
            while (WriteInProgress()) { }

            // Reset the chip
            while (!WriteEnabled) { }
            Hardware.SPIBus.Write(_spiConfig, new Byte[] { 0x99 });
            while (WriteInProgress()) { }

            return true;
        }

        private void DetectParameters()
        {
            var data2 = new Byte[4];
            var data4 = new Byte[256];
            data2[0] = 0x5A;
            data2[1] = 0x00;
            data2[2] = 0x00;
            data2[3] = 0x00;
            Hardware.SPIBus.WriteRead(_spiConfig, data2, 0, 4, data4, 0, 256, 5);

            if (new String(Encoding.UTF8.GetChars(data4, 0, 4)) != "SFDP") return;    // No JEDEC information detected, use default values

            var Offset = data4[0x0C];
            _SectorSize = 1 << data4[Offset + 0x1C];
            _SectorEraseInstruction = data4[Offset + 0x1D];
            _BlockSize = 1 << data4[Offset + 0x1E];
            _BlockEraseInstruction = data4[Offset + 0x1F];
            _Capacity = (((data4[Offset + 0x07] << 24) +
                          (data4[Offset + 0x06] << 16) +
                          (data4[Offset + 0x05] << 8) +
                           data4[Offset + 0x04]) >> 3) + 1;
        }
    }
}
