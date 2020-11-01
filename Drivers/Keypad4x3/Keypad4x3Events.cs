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

namespace MBN.Modules
{
    public partial class Keypad4X3
    {
        /// <summary>
        /// Delegate for the KeyPressed event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyPressedEventArgs"/> instance containing the event data.</param>
        public delegate void KeyPressedEventHandler(object sender, KeyPressedEventArgs e);

        /// <summary>
        /// Delegate for the KeyReleased event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyReleasedEventArgs"/> instance containing the event data.</param>
        public delegate void KeyReleasedEventHandler(object sender, KeyReleasedEventArgs e);

        /// <summary>
        /// Class holding arguments for the KeyPressed event
        /// </summary>
        public class KeyPressedEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="KeyPressedEventArgs"/> class.
            /// </summary>
            /// <param name="pKeyValue">The key pressed value.</param>
            /// <param name="pKeyChar">The key pressed character representation.</param>
            public KeyPressedEventArgs(int pKeyValue, char pKeyChar)
            {
                KeyValue = pKeyValue;
                KeyChar = pKeyChar;
            }

            /// <summary>
            /// Gets the key pressed value.
            /// </summary>
            /// <value>
            /// The key value.
            /// </value>
            public int KeyValue { get; private set; }
            /// <summary>
            /// Gets the key pressed character.
            /// </summary>
            /// <value>
            /// The key character.
            /// </value>
            public char KeyChar { get; private set; }
        }

        /// <summary>
        /// Class holding arguments for the KeyReleased event
        /// </summary>
        public class KeyReleasedEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="KeyReleasedEventArgs"/> class.
            /// </summary>
            /// <param name="pKeyValue">The key released value.</param>
            /// <param name="pKeyChar">The key released character representation.</param>
            public KeyReleasedEventArgs(int pKeyValue, char pKeyChar)
            {
                KeyValue = pKeyValue;
                KeyChar = pKeyChar;
            }

            /// <summary>
            /// Gets the key released value.
            /// </summary>
            /// <value>
            /// The key value.
            /// </value>
            public int KeyValue { get; private set; }
            /// <summary>
            /// Gets the key released character.
            /// </summary>
            /// <value>
            /// The key character.
            /// </value>
            public char KeyChar { get; private set; }
        }
    }
}