/*
 * Storage driver
 * 
 * Version 2.4 - 31/03/2016 Christophe Gerbier
 *  - Added the EraseBlock() method
 *  - Made all properties abstract instead of virtual to force users to enter correct values for their chip
 *  
 * Version 1.0 - Christophe Gerbier
 *  - Initial revision
 * 
 * References needed :
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Native
 *  MikroBusNet
 *  mscorlib
 *  
 * Copyright 2014-2016 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System;

namespace MBN
{
    /// <summary>
    /// Storage class
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///     private static Storage _storage1, _storage2;
    ///
    ///     public static void Main()
    ///     {
    ///        _storage1 = new EEpromClick(Hardware.SocketOne, memorySize: 256);  // Here, the original 8KB chip has been replace by a 256KB one ;)
    ///        _storage2 = new OnboardFlash();
    ///
    ///        Debug.Print("Address 231 before : " + _storage1.ReadByte(231));
    ///        _storage1.WriteByte(231, 123);
    ///        Debug.Print("Address 231 after : " + _storage1.ReadByte(231));
    ///
    ///        _storage2.WriteData(400, new Byte[] { 100, 101, 102 }, 0, 3);
    ///        _storage2.ReadData(400, bArray, 0, 3);
    ///        Debug.Print("Read 3 bytes starting @400 (should be 100, 101, 102) : " + bArray[0] + ", " + bArray[1] + ", " + bArray[2]);
    ///     }
    /// </code>
    /// </example>
    public abstract class Storage
    {
        /// <summary>
        /// Gets the memory capacity.
        /// </summary>
        /// <value>
        /// The maximum capacity, in bytes.
        /// </value>
        public abstract Int32 Capacity { get; }
        /// <summary>
        /// Gets the size of a page in memory.
        /// </summary>
        /// <value>
        /// The size of a page in bytes
        /// </value>
        public abstract Int32 PageSize { get; }
        /// <summary>
        /// Gets the size of a sector.
        /// </summary>
        /// <value>
        /// The size of a sector in bytes.
        /// </value>
        public abstract Int32 SectorSize { get; }
        /// <summary>
        /// Gets the size of a block.
        /// </summary>
        /// <value>
        /// The size of a block in bytes.
        /// </value>
        public abstract Int32 BlockSize { get; }
        /// <summary>
        /// Gets the number of pages per cluster.
        /// </summary>
        /// <value>
        /// The number of pages per cluster.
        /// </value>
        public virtual Int32 PagesPerCluster { get { return 4; } }

        /// <summary>
        /// Completely erases the chip.
        /// </summary>
        /// <remarks>This method is mainly used by Flash memory chips, because of their internal behaviour. It can be safely ignored with other memory types.</remarks>
        /// <example>
        /// <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _storage;
        ///
        ///     public static void Main()
        ///     {
        ///        _storage = new OnboardFlash();
        ///
        ///        _storage.EraseChip();
        ///     }
        /// </code>
        /// </example>
        public abstract void EraseChip();

        /// <summary>
        /// Erases "count" sectors starting at "sector".
        /// </summary>
        /// <param name="sector">The starting sector.</param>
        /// <param name="count">The count of sectors to erase.</param>
        /// <example>
        /// <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _storage;
        ///
        ///     public static void Main()
        ///     {
        ///        _storage = new OnboardFlash();
        ///
        ///        _storage.EraseSector(10,1);
        ///     }
        /// </code>
        /// </example>
        public abstract void EraseSector(Int32 sector, Int32 count);

        /// <summary>
        /// Erases "count" blocks starting at "sector".
        /// </summary>
        /// <param name="block">The starting block.</param>
        /// <param name="count">The count of blocks to erase.</param>
        /// <example>
        /// <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _storage;
        ///
        ///     public static void Main()
        ///     {
        ///        _storage = new OnboardFlash();
        ///
        ///        _storage.EraseBlock(10,2);
        ///     }
        /// </code>
        /// </example>
        public abstract void EraseBlock(Int32 block, Int32 count);
		
        /// <summary>
        /// Writes data to a memory location.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="data">The data to write.</param>
        /// <param name="index">The starting index in the data array.</param>
        /// <param name="count">The count of bytes to write to memory.</param>
        /// <example>
        /// <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _storage;
        ///
        ///     public static void Main()
        ///     {
        ///        _storage = new EEpromClick(Hardware.Socket2);
        ///
        ///        _storage.WriteData(400, new Byte[] { 100, 101, 102 }, 0, 3);
        ///        _storage.ReadData(400, bArray, 0, 3);
        ///        Debug.Print("Read 3 bytes starting @400 (should be 100, 101, 102) : " + bArray[0] + ", " + bArray[1] + ", " + bArray[2]);
        ///     }
        /// </code>
        /// </example>
        public abstract void WriteData(Int32 address, Byte[] data, Int32 index, Int32 count);
        /// <summary>
        /// Reads data at a specific address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <param name="data">An array of bytes containing data read back</param>
        /// <param name="index">The starting index to read in the array.</param>
        /// <param name="count">The count of bytes to read.</param>
        /// <example>
        /// <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _storage;
        ///
        ///     public static void Main()
        ///     {
        ///        _storage = new FlashClick(Hardware.Socket1);
        ///
        ///        _storage.WriteData(400, new Byte[] { 100, 101, 102 }, 0, 3);
        ///        _storage.ReadData(400, bArray, 0, 3);
        ///        Debug.Print("Read 3 bytes starting @400 (should be 100, 101, 102) : " + bArray[0] + ", " + bArray[1] + ", " + bArray[2]);
        ///     }
        /// </code>
        /// </example>
        public abstract void ReadData(Int32 address, Byte[] data, Int32 index, Int32 count);

        /// <summary>
        /// Reads a single byte at a specified address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <returns>A byte value</returns>
        /// <example>
        /// <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _Flash;
        ///
        ///     public static void Main()
        ///     {
        ///        _Flash = new OnboardFlash();
        ///
        ///        _Flash.WriteByte(10, 200);
        ///        Debug.Print("Read byte @10 (should be 200) : " + _Flash.ReadByte(10));
        ///        _Flash.WriteByte(200, 201);
        ///        Debug.Print("Read byte @200 (should be 201) : " + _Flash.ReadByte(200));
        ///     }
        /// </code>
        /// </example>
        public Byte ReadByte(Int32 address)
        {
            var tmp = new Byte[1];
            ReadData(address, tmp, 0, 1);
            return tmp[0];
        }

        /// <summary>
        /// Writes a single byte at a specified address.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="value">The value to write at the specified address.</param>
        /// <example>
        /// <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _Flash;
        ///
        ///     public static void Main()
        ///     {
        ///        _Flash = new OnboardFlash();
        ///
        ///        _Flash.WriteByte(10, 200);
        ///        Debug.Print("Read byte @10 (should be 200) : " + _Flash.ReadByte(10));
        ///        _Flash.WriteByte(200, 201);
        ///        Debug.Print("Read byte @200 (should be 201) : " + _Flash.ReadByte(200));
        ///     }
        /// </code>
        /// </example>
        public void WriteByte(Int32 address, Byte value)
        {
            WriteData(address, new[] { value }, 0, 1);
        }
    }
}
