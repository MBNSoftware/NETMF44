using System;

namespace MBN.Modules
{
    public partial class SpeakUpClick
    {
        /// <summary>
        /// Delegate for the SpeakDetected event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SpeakUpEventArgs"/> instance containing the event data.</param>
        public delegate void SpeakUpEventHandler(object sender, SpeakUpEventArgs e);

        /// <summary>
        /// Class holding arguments for the SpeakDetected event.
        /// </summary>
        public class SpeakUpEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SpeakUpEventArgs"/> class.
            /// </summary>
            /// <param name="command">Index of the detected command</param>
            public SpeakUpEventArgs(Byte command)
            {
                Command = command;
            }

            /// <summary>
            /// Gets the index of the detected command.
            /// </summary>
            /// <value>
            /// Index of the command, as recorded in the SpeakUp board
            /// </value>
            public Byte Command { get; private set; }
        }
    }
}