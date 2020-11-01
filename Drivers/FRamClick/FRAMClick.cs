/* FRAM Click Driver for MikrobusNet 
 * Version 1.1 :
 *  - Added EraseChip() implementation
 * Version 1.0
 *  - Initial version coded by Christophe Gerbier
 *  
 * References needed :
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Native
 *  MikroBusNet
 *  mscorlib
 *  
 * Copyright 2015 MikroBus.Net
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
using MBN.Extensions;
using Microsoft.SPOT.Hardware;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the FRAM Click driver
    /// <para><b>Pins used :</b> Miso, Mosi, Cs, Sck, Pwm, Rst</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///     private static Storage _fram;
    ///
    ///     public static void Main()
    ///     {
    ///        _fram = new FRAMClick(Hardware.SocketOne);
    ///
    ///        Debug.Print("Address 231 before : " + _fram.ReadByte(231));
    ///        _fram.WriteByte(231, 123);
    ///        Debug.Print("Address 231 after : " + _fram.ReadByte(231));
    ///
    ///        _fram.WriteData(400, new Byte[] { 100, 101, 102 }, 0, 3);
    ///        _fram.ReadData(400, bArray, 0, 3);
    ///        Debug.Print("Read 3 bytes starting @400 (should be 100, 101, 102) : " + bArray[0] + ", " + bArray[1] + ", " + bArray[2]);
    ///     }
    /// </code>
    /// </example>
    public class FRAMClick : Storage, IDriver
    {
        /// <summary>
        /// Gets the memory capacity.
        /// </summary>
        /// <value>
        /// The maximum capacity, in KBytes. Ex: 256 for 256KB
        /// </value>
        /// <remarks>
        /// 1MB = 1024KB, so 8MB will be : 8*1024 = 8192KB
        /// </remarks>
        public override Int32 Capacity { get { return 0x8000; } }   // 32KB for MikroE FRam chip
        /// <summary>
        /// Gets the size of a page in memory.
        /// </summary>
        /// <value>
        /// The size of a page in bytes
        /// </value>
        public override Int32 PageSize { get { return 64; } }
        /// <summary>
        /// Gets the size of a sector.
        /// </summary>
        /// <value>
        /// The size of a sector in bytes.
        /// </value>
        public override Int32 SectorSize { get { return 256; } }

        public override int BlockSize
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the number of pages per cluster.
        /// </summary>
        /// <value>
        /// The number of pages per cluster.
        /// </value>
        public override Int32 PagesPerCluster { get { return 1; } }

        private readonly SPI.Configuration _spiConfig;
        private Byte[] _dataPage;

        /// <summary>
        /// Initializes a new instance of the <see cref="FRAMClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the EEpromClick module is plugged on MikroBus.Net board</param>
        /// <exception cref="MBN.Exceptions.PinInUseException"></exception>
        public FRAMClick(Hardware.Socket socket)
        {
            try
            {
                Hardware.CheckPins(socket, socket.Miso, socket.Mosi, socket.Cs, socket.Pwm, socket.Rst);
                _spiConfig = new SPI.Configuration(socket.Cs, false, 0, 0, false, true, 10000, socket.SpiModule);
                if (Hardware.SPIBus == null) { Hardware.SPIBus = new SPI(_spiConfig); }
                WriteEnable();
            }
            catch (PinInUseException) { throw new PinInUseException(); }
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
        /// _storage = new FRAMClick();
        /// _storage.EraseChip();
        /// }
        /// </code>
        /// </example>
        public override void EraseChip()
        {
            if (_dataPage == null) { _dataPage = new Byte[PageSize + 3]; }

            Hardware.SPIBus.Write(_spiConfig, new byte[] { 0x01, 0x00 });
            WriteEnable();
            for (var i = 0; i < Capacity; i += PageSize)
            {
                _dataPage[0] = 0x02;
                _dataPage[1] = (Byte)(i >> 8);
                _dataPage[2] = (Byte)(i & 0xFF);
                Hardware.SPIBus.Write(_spiConfig, _dataPage);

                while (WriteInProgress()) Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Erases "count" sectors starting at "sector".
        /// </summary>
        /// <param name="sector">The starting sector.</param>
        /// <param name="count">The count of sectors to erase.</param>
        /// <example>
        ///   <code language="C#">
        /// public class Program
        /// {
        /// private static Storage _storage;
        /// public static void Main()
        /// {
        /// _storage = new FRAMClick();
        /// _storage.EraseSector(10,1);
        /// }
        /// </code>
        /// </example>
        public override void EraseSector(Int32 sector, Int32 count) { }

        public override void EraseBlock(int block, int count)
        {
            throw new NotImplementedException();
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
        /// _storage = new FRAMClick(Hardware.SocketOne);
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
            if ((array.Length - index) < count) throw new ArgumentException("Invalid index + count");
            if ((Capacity - count) < address) throw new ArgumentException("Invalid address + count");
            if (_dataPage == null) { _dataPage = new Byte[PageSize + 3]; }
            WriteEnable();
            var block = count / PageSize;
            var length = count;
            var i = 0;
            if (block > 0)
            {
                for (i = 0; i < block; i++)
                {
                    _dataPage[0] = 0x02;
                    _dataPage[1] = (Byte)(address >> 8);
                    _dataPage[2] = (Byte)(address & 0xFF);
                    Array.Copy(array, index + (i * PageSize), _dataPage, 3, PageSize);
                    Hardware.SPIBus.Write(_spiConfig, _dataPage);

                    while (WriteInProgress()) Thread.Sleep(1);
                    address += PageSize;
                    length -= PageSize;
                }
            }

            if (length > 0)
            {
                _dataPage[0] = 0x02;
                _dataPage[1] = (Byte)(address >> 8);
                _dataPage[2] = (Byte)(address & 0xFF);
                Array.Copy(array, index + (i * PageSize), _dataPage, 3, length);
                Hardware.SPIBus.WriteRead(_spiConfig, _dataPage, 0, length + 3, null, 0, 0, 0);
                while (WriteInProgress()) Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Reads data at a specific address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <param name="data">An array of bytes containing data read back</param>
        /// <param name="index">The starting index to read in the array.</param>
        /// <param name="count">The count of bytes to read.</param>
        /// <example>
        ///   <code language="C#">
        /// public class Program
        /// {
        /// private static Storage _storage;
        /// public static void Main()
        /// {
        /// _storage = new FRAMClick(Hardware.Socket1);
        /// _storage.WriteData(400, new Byte[] { 100, 101, 102 }, 0, 3);
        /// _storage.ReadData(400, bArray, 0, 3);
        /// Debug.Print("Read 3 bytes starting @400 (should be 100, 101, 102) : " + bArray[0] + ", " + bArray[1] + ", " + bArray[2]);
        /// }
        /// </code>
        /// </example>
        public override void ReadData(Int32 address, Byte[] data, Int32 index, Int32 count)
        {
            Hardware.SPIBus.WriteRead(_spiConfig, new Byte[] { 0x03, (Byte)(address >> 8), (Byte)(address & 0xFF) }, 0, 3, data, index, count, 3);
        }

        private void WriteEnable()
        {
            Hardware.SPIBus.Write(_spiConfig, new Byte[] { 0x06 });
        }

        private Boolean WriteInProgress()
        {
            var data2 = new Byte[2];

            Hardware.SPIBus.WriteRead(_spiConfig, new Byte[] { 0x05 }, data2);
            return ((data2[1] & 0x01) != 0x00);
        }

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">This module does not have power modes feature.</exception>
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
        /// <example> This sample shows how to use the DriverVersion property.
        /// <code language="C#">
        ///             Debug.Print ("Current driver version : "+_fram.DriverVersion);
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
