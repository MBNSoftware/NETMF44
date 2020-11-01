/*
 * MikroBus.Net main assembly
 * 
 * Bits manipulation class
 *
 * Version 2.0 :
 *  - Changed namespaces
 * 
 * Version 1.0 :
 *  - Initial revision coded by Christophe Gerbier
 * 
 * References needed :
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Native
 *  mscorlib
 *  
 * Copyright 2014 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System;

namespace MBN
{
    /// <summary>
    /// Bits manipulation class
    /// </summary>
    public class Bits
    {
        /// <summary>
        /// Determines whether a bit is set at a given position in a byte.
        /// </summary>
        /// <param name="value">The byte value.</param>
        /// <param name="pos">The position to check.</param>
        /// <returns>A boolean : true if bit is set, false otherwise</returns>
		public static bool IsBitSet(byte value, byte pos)
        {
            return (value & (1 << pos)) != 0;
        }

        /// <summary>
        /// Determines whether a bit is set at a given position in an Int16.
        /// </summary>
        /// <param name="value">The byte value.</param>
        /// <param name="pos">The position to check.</param>
        /// <returns>A boolean : true if bit is set, false otherwise</returns>
		public static bool IsBitSet(Int16 value, byte pos)
        {
            return (value & (1 << pos)) != 0;
        }
		
        /// <summary>
        /// Sets or unsets a specified bit.
        /// </summary>
        /// <param name="value">The byte in which a bit will be set/unset</param>
        /// <param name="index">The index of the bit.</param>
        /// <param name="state">if set to <c>true</c> then bit will be 1, else it will be 0.</param>
        public static void Set(ref Byte value, Byte index, Boolean state)
        {
            var mask = (Byte)(1 << index);

            if (state) { value |= mask; }
            else { value &= (Byte)~mask; }
        }

        /// <summary>
        /// Sets or unsets a specified bit.
        /// </summary>
        /// <param name="value">The byte in which a bit will be set/unset</param>
        /// <param name="index">The index of the bit.</param>
        /// <param name="state">if set to <c>true</c> then bit will be 1, else it will be 0.</param>
        public static Byte Set(Byte value, Byte index, Boolean state)
        {
            var mask = (Byte)(1 << index);

            if (state) { value |= mask; }
            else { value &= (Byte)~mask; }

            return value;
        }

        /// <summary>
        /// Toggles a specified bit.
        /// </summary>
        /// <param name="value">The byte in which a bit will be toggled.</param>
        /// <param name="index">The index of the bit.</param>
        public static void Toggle(ref Byte value, Byte index)
        {
            value ^= (Byte)(1 << index);
        }

        /// <summary>
        /// Sets a byte's value using a binary string mask.
        /// </summary>
        /// <param name="value">The byte that should be set.</param>
        /// <param name="mask">The bit mask, like "x11x0110". 'x' means "ignore".</param>
        public static void Set(ref Byte value, String mask)
        {
            var valTmp = value;
            for (var i = mask.Length-1; i >=0; i--)
            {
                if (mask[i] != 'x') { Set(ref valTmp, (Byte)(7-i), mask[i] == '1'); }
            }
            value = valTmp;
        }

        /// <summary>
        /// Sets a byte's value using a binary string mask.
        /// </summary>
        /// <param name="value">The byte that should be set.</param>
        /// <param name="mask">The bit mask, like "x11x0110". 'x' means "ignore".</param>
        public static Byte Set(Byte value, String mask)
        {
            var valTmp = value;
            for (var i = mask.Length - 1; i >= 0; i--)
            {
                if (mask[i] != 'x') { Set(ref valTmp, (Byte)(7 - i), mask[i] == '1'); }
            }
            return valTmp;
        }

        /// <summary>
        /// Sets a specified number of bits in a Byte, according to a specified value and a binary mask.
        /// <para>Example : SetRegister(Registers.BW_RATE, 0x20, (Byte)value);</para>
        /// </summary>
        /// <param name="originalValue">The original byte value.</param>
        /// <param name="mask">The mask.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static Byte Set(Byte originalValue, Byte mask, Byte value)
        {
            return (Byte)((originalValue & ~mask) | (value & mask));
        }

        /// <summary>
        /// Sets a specified number of bits in a Byte, according to a specified value and a string mask. Bits marked "1" in the String mask will be replaced by value.
        /// <para>Example : SetRegister(Registers.BW_RATE, "00001111", (Byte)value);</para>
        /// </summary>
        /// <param name="originalValue">The original byte value.</param>
        /// <param name="mask">The mask.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static Byte Set(Byte originalValue, String mask, Byte value)
        {
            var i = mask.Length - 1;
            var binMask = ParseBinary(mask);
            while (mask[i] != '1') { i--; }
            return (Byte)((originalValue & ~binMask) | ((value << (7 - i)) & binMask));
        }

        /// <summary>
        /// Parses a number given in a binary format string.
        /// </summary>
        /// <param name="input">The input string, representing bits positions, e.g. "01110011".</param>
        /// <returns>The Int32 representation of the binary number.</returns>
        public static Int32 ParseBinary(String input)
        {
            // Thanks to Jon Skeet for this one
            var output = 0;
            for (var i = 0; i < input.Length; i++)
            {
                if (input[i] == '1') { output |= 1 << (input.Length - i - 1); }
            }
            return output;
        }

        
    }
}


