using System;
using System.Threading;
using MBN;
using MBN.Modules;
using Microsoft.SPOT;

namespace Examples
{
    public class Program
    {
        static RelayClick _relays;

        public static void Main()
        {
            try
            {
                // Relay Click board is plugged on socket #1 of the MikroBus.Net mainboard
                // Relay 0 will be OFF and Relay 1 will be ON at startup
                _relays = new RelayClick(Hardware.SocketTwo, relay1InitialState : true);

                // Register to the event generated when a relay state has been changed
                _relays.RelayStateChanged += Relays_RelayStateChanged;

                // Sets relay 0 to ON using the SetRelay() method
                _relays.SetRelay(0, true);
                Thread.Sleep(2000);

                // Sets relay 0 to OFF using the Relay0 property
                _relays.Relay0 = false;
                Thread.Sleep(2000);

                // Sets relay 1 to ON using the SetRelay() method
                _relays.SetRelay(1, true);
                Thread.Sleep(2000);

                // Sets relay 1 to OFF using the Relay0 property
                _relays.Relay1 = false;
                Thread.Sleep(2000);

                // Gets relay 0 state using the Relay0 property
                Debug.Print("Relay 0 state : " + _relays.Relay0.ToString());

                // Gets relay 1 state using the GetRelay() method
                Debug.Print("Relay 1 state : " + _relays.GetRelay(1).ToString());

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception ex)
            {
                Debug.Print("ERROR : " + ex.Message);
            }
        }

        static void Relays_RelayStateChanged(object sender, RelayClick.RelayStateChangedEventArgs e)
        {
            if (e.Relay == 0)   // Red led for relay 0
            {
                Debug.Print("Relay 0 state has changed");
            }
            else                // Green led for relay 1
            {
                Debug.Print("Relay 1 state has changed");
            }
        }

    }
}
