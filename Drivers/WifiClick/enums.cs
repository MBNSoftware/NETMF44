using System;
using Microsoft.SPOT;

namespace MBN.Modules
{
    public partial class Wifi
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Network modes
        /// </summary>
        public enum NetworkModes
        {
            /// <summary>
            /// Infrastructure mode.
            /// <para>This mode enables the connection to an AP</para>
            /// </summary>
            Infrastructure,
            /// <summary>
            /// Ad Hoc mode
            /// <remarks>This mode is not implemented yet.</remarks>
            /// </summary>
            AdHoc
        }

        /// <summary>
        /// List of allowed modes for connecting to an AP.
        /// </summary>
        public enum SecurityModes
        {
            /// <summary>
            /// Open security mode : no security key required to connect to AP
            /// </summary>
            Open,
            /// <summary>
            /// WEP security mode with a 64 bits key
            /// </summary>
            WEP40,
            /// <summary>
            /// WEP security mode with a 128 bits key
            /// </summary>
            WEP104,
            /// <summary>
            /// WPA/PSK security mode
            /// </summary>
            WPA,
            /// <summary>
            /// WPA2/PSK security mode
            /// </summary>
            WPA2
        }
        /// <summary>
        /// Indicates the socket type used in the socket methods.
        /// </summary>
        public enum SocketTypes : byte
        {
            /// <summary>
            /// UDP socket.
            /// </summary>
            /// <remarks>This has not been tested yet. Please send feedback or debug information if it fails.</remarks>
            UDP,
            /// <summary>
            /// TCP socket.
            /// </summary>
            TCP
        }
        /// <summary>
        /// Indicates the status of the network
        /// </summary>
        public enum ConnectionStatus : byte
        {
            /// <summary>
            /// The profile has attempted a <see cref="ConnectStaticIP"/> command but the connection was not successful
            /// </summary>
            NotConnectedStaticIP = 0,
            /// <summary>
            /// The profile has attempted a <see cref="ConnectStaticIP"/> command and the connection was successful
            /// </summary>
            ConnectedStaticIP = 1,
            /// <summary>
            /// The profile has attempted a <see cref="ConnectDHCP"/> command but the connection was not successful
            /// </summary>
            NotConnectedDHCP = 2,
            /// <summary>
            /// The profile has attempted a <see cref="ConnectDHCP"/> command and the connection was successful
            /// </summary>
            ConnectedDHCP = 3
        }
        // ReSharper restore InconsistentNaming
    }
}
