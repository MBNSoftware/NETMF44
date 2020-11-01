using System;
using Microsoft.SPOT;

// ReSharper disable once CheckNamespace

namespace MBN.Modules
{
    public partial class ThunderClick
    {
        /// <summary>
        /// Delegate for the LightningDetected event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="LightningEventArgs"/> instance containing the event data.</param>
        public delegate void LightningEventHandler(object sender, LightningEventArgs e);

        /// <summary>
        /// Delegate for the DisturbanceDetected and NoiseDetected events.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public delegate void EventHandler(object sender, EventArgs e);

        /// <summary>
        /// Class holding arguments for the ThunderDetected event.
        /// </summary>
        public class LightningEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LightningEventArgs"/> class.
            /// </summary>
            /// <param name="distance">Distance of the detected lightning.</param>
            /// <param name="energy">Energy of the detected lightning. This value is just a pure number and has no physical meaning.</param>
            public LightningEventArgs(Int32 distance, Int32 energy)
            {
                Distance = distance;
                Energy = energy;
            }

            /// <summary>
            /// Gets the distance of the detected lightning.
            /// </summary>
            /// <value>
            /// The distance in kilokmeters. Max distance is 40 km.
            /// </value>
            public Int32 Distance { get; private set; }

            /// <summary>
            /// Gets the energy of the detected lightning.
            /// </summary>
            /// <value>
            /// This value is just a pure number and has no physical meaning.
            /// </value>
            public Int32 Energy { get; private set; }
        }
    }
}