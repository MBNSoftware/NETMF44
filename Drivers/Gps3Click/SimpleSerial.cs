/*
 * GPS3Click Driver
 * 
 * Version 1.0 :
 *  - Initial version coded by Stephen Cardinale
 * 
 * References needed :
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Hardware.SerialPort
 *  Microsoft.SPOT.Native
 *  Microsoft.SPOT.IO
 *  MBN
 *  mscorlib
 *  
 * Source for SimpleSerial taken from IggMoe's SimpleSerial Class https://www.ghielectronics.com/community/codeshare/entry/644
 *  
 * Copyright 2014 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System;
using System.IO.Ports;
using System.Text;

namespace MBN.Modules
{

    public partial class GPS3Click
    {
        /// <summary>
        ///     Extends the .NET Micro Framework SerialPort Class with additional methods from the
        ///     Full .NET Framework SerialPort Class as well as other useful methods.
        /// </summary>
        internal class SimpleSerial : SerialPort
        {

            #region Fields

            private string _remainder;

            #endregion

            #region CTOR

            internal SimpleSerial(string portName, int baudRate) : base(portName, baudRate, Parity.None, 8, StopBits.One)
            {
                DiscardInBuffer();
                DiscardOutBuffer();
            }

            #endregion

            #region Internal Properties

            /// <summary>
            ///     Stores any incomplete message that hasn't yet been terminated with a delimiter.
            ///     This will be concatenated with new data from the next DataReceived event to (hopefully) form a complete message.
            ///     This property is only populated after the Deserialize() method has been called.
            /// </summary>
            internal string Remainder
            {
                get { return _remainder; }
            }

            #endregion

            #region Internal Methods

            /// <summary>
            ///     Writes the specified string to the serial port.
            /// </summary>
            /// <param name="txt" />
            internal void Write(string txt)
            {
                base.Write(Encoding.UTF8.GetBytes(txt), 0, txt.Length);
            }

            /// <summary>
            ///     Writes the specified string and the NewLine value to the output buffer.
            /// </summary>
            internal void WriteLine(string txt)
            {
                Write(txt + "\r\n");
            }

            /// <summary>
            ///     Reads all immediately available bytes, as binary data, in both the stream and the input buffer of the SerialPort
            ///     object.
            /// </summary>
            /// <returns>
            ///    System.Byte[]
            /// </returns>
            internal byte[] ReadExistingBinary()
            {
                var arraySize = BytesToRead;
                var received = new byte[arraySize];
                Read(received, 0, arraySize);
                return received;
            }

            ///// <summary>
            /////     Reads all immediately available bytes, based on the encoding, in both the stream and the input buffer of the
            /////     SerialPort object.
            ///// </summary>
            ///// <returns>String</returns>
            //internal string ReadExisting()
            //{
            //    try
            //    {
            //        String tempString;
            //        Byte[] existingBytes;
            //        Byte[] tmpBytes = ReadExistingBinary();
            //        foreach (var b in tmpBytes)
            //        {
            //            if (b > 32 && b < 127 || b == 13 || b == 10)
            //            {
            //                existingBytes += b; //new string(Encoding.UTF8.GetChars(b));
            //            }
            //            return tempString;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        if (Debugger.IsAttached) Debug.Print(ex.Message);
            //        return string.Empty;
            //    }
            //}


            internal string ReadExisting()
            {
                try
                {
                    return new string(Encoding.UTF8.GetChars(ReadExistingBinary()));
                }
                catch (Exception)
                {
                    return String.Empty;
                }
            }
            //internal String ReadExisting()
            //{
            //    var existing = String.Empty;
            //    //var _dataBuffer = String.Empty;
            //    try
            //    {
            //        var b = new byte[BytesToRead];
            //        if (b.Length == 0) return String.Empty;
            //        Read(b, 0, b.Length);

            //        foreach (var b1 in b)
            //        {
            //            if (b1 > 32 && b1 < 127 || b1 == 13 || b1 == 10)
            //            {
            //                //tmp = _dataBuffer + new string(Encoding.UTF8.GetChars(b));
            //                existing += new string(Encoding.UTF8.GetChars(b));
            //            }
            //        }
            //        return existing;
            //    }
            //    catch (Exception ex)
            //    {
            //        if (Debugger.IsAttached) Debug.Print(ex.Message);
            //        return string.Empty;
            //    }
            //}

            /// <summary>
            ///     Opens a new serial port connection.
            /// </summary>
            internal new void Open()
            {
                _remainder = String.Empty;
                base.Open();
            }

            /// <summary>
            ///     Splits data from a serial buffer into separate messages, provided that each message is delimited by one or more
            ///     end-of-line character(s).
            /// </summary>
            /// <param name="delimiter">Character sequence that terminates a message line. Default is "\r\n".</param>
            /// <returns>
            ///     An array of strings whose items correspond to individual messages, without the delimiters.
            ///     Only complete, properly terminated messages are included. Incomplete message fragments are saved to be appended to
            ///     the next received data.
            ///     If no complete messages are found in the serial buffer, the output array will be empty with Length = 0.
            /// </returns>
            internal string[] Deserialize(string delimiter = "\r\n")
            {
                return SplitString(_remainder + ReadExisting(), out _remainder, delimiter);
            }

            #endregion

            #region Private Methods

            /// <summary>
            ///     Splits a stream into separate lines, given a delimiter.
            /// </summary>
            /// <param name="input">
            ///     The string that will be de-serialized.
            ///     Example:
            ///     Assume a device transmits serial messages, and each message is separated by \r\n (carriage return + line feed).
            ///     For illustration, picture the following output from such a device:
            ///     First message.\r\n
            ///     Second message.\r\n
            ///     Third message.\r\n
            ///     Fourth message.\r\n
            ///     Once a SerialPort object receives the first bytes, the DataReceived event will be fired,
            ///     and the interrupt handler may read a string from the serial buffer like so:
            ///     "First message.\r\nSecond message.\r\nThird message.\r\nFourth me"
            ///     The message above has been cut off to simulate the DataReceived event being fired before the sender has finished
            ///     transmitting all messages (the "ssage.\r\n" characters have not yet traveled down the wire, so to speak).
            ///     At the moment the DataReceived event is fired, the interrupt handler only has access to the (truncated)
            ///     input message above.
            ///     In this example, the string from the serial buffer will be the input to this method.
            /// </param>
            /// <param name="remainder">
            ///     Any incomplete messages that have not yet been properly terminated will be returned via this output parameter.
            ///     In the above example, this parameter will return "Fourth me". Ideally, this output parameter will be appended to
            ///     the next
            ///     transmission to reconstruct the next complete message.
            /// </param>
            /// <param name="delimiter">
            ///     A string specifying the delimiter between messages.
            ///     If omitted, this defaults to "\r\n" (carriage return + line feed).
            /// </param>
            /// <param name="includeDelimiterInOutput">
            ///     Determines whether each item in the output array will include the specified delimiter.
            ///     If True, the delimiter will be included at the end of each string in the output array.
            ///     If False (default), the delimiter will be excluded from the output strings.
            /// </param>
            /// <returns>
            ///     string[]
            ///     Every item in this string array will be an individual, complete message. The first element
            ///     in the array corresponds to the first message, and so forth. The length of the array will be equal to the number of
            ///     complete messages extracted from the input string.
            ///     From the above example, if includeDelimiterInOutput == True, this output will be:
            ///     output[0] = "First message.\r\n"
            ///     output[1] = "Second message.\r\n"
            ///     output[2] = "Third message.\r\n"
            ///     If no complete messages have been received, the output array will be empty with Length = 0.
            /// </returns>
            private static string[] SplitString(string input, out string remainder, string delimiter = "\r\n", bool includeDelimiterInOutput = false)
            {
                var prelimOutput = input.Split(delimiter.ToCharArray());

                // Check last element of prelimOutput to determine if it was a delimiter.
                // We know that the last element was a delimiter if the string.Split() method makes it empty.
                if (prelimOutput[prelimOutput.Length - 1] == String.Empty) remainder = String.Empty; // input string terminated in a delimiter, so there is no remainder
                else
                {
                    remainder = prelimOutput[prelimOutput.Length - 1]; // store the remainder
                    prelimOutput[prelimOutput.Length - 1] = String.Empty; // remove the remainder string from prelimOutput to avoid redundancy
                }
                return ScrubStringArray(prelimOutput, String.Empty, includeDelimiterInOutput ? delimiter : String.Empty);
            }

            /// <summary>
            ///     Removes items in an input array that are equal to a specified string.
            /// </summary>
            /// <param name="input">String array to scrub.</param>
            /// <param name="removeString">String whose occurrences will be removed if an item consists of it. Default: string.Empty.</param>
            /// <param name="delimiter">
            ///     Delimiter that will be appended to the end of each element in the output array. Default: \r\n (carriage return +
            ///     line feed).
            ///     To omit delimiters from the end of each message, set this parameter to string.Empty.
            /// </param>
            /// <returns>
            ///     String array containing only desired strings. The length of this output will likely be shorter than the input array.
            /// </returns>
            private static string[] ScrubStringArray(string[] input, string removeString = "", string delimiter = "\r\n")
            {
                var numOutputElements = 0;

                // Determine the bounds of the output array by looking for input elements that meet inclusion criterion
                for (var k = 0; k < input.Length; k++)
                {
                    if (input[k] != removeString) numOutputElements++;
                }

                // Declare and populate output array
                var output = new string[numOutputElements];

                var m = 0; // output index
                for (var k = 0; k < input.Length; k++)
                {
                    if (input[k] == removeString) continue;
                    output[m] = input[k] + delimiter;
                    m++;
                }

                return output;
            }

            #endregion
        }
    }
}