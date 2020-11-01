/*
 * Click board driver skeleton generated on 5/10/2014 5:37:35 PM
 * Events namespace
 * 
 * Initial revision coded by Stephen Cardinale
 * 
 */

using System.Collections;

// ReSharper disable once CheckNamespace
namespace MBN.Events
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender">The DS18B20 Click Board that raised the Event.</param>
    /// <param name="temperature">The HashTable containing a Key/Value Pair of the unique 64-Bit Address of the DS18B20 and the temperature in °C.</param>
    public delegate void TemperatureMeasured(Ds18B20 sender, Hashtable temperature);

    /// <summary>
    /// The TemperatureMeasuredEventArgs Class
    /// </summary>
    public class TemperatureMeasuredEventArgs
    {
        /// <summary>
        /// Returns a HashTable consisting of a Key/Value pair of the unique 64-bit Address
        ///     of all sensors on the One-Wire Bus and it's corresponding temperature in °C. 
        /// </summary>
        /// <param name="temperature"></param>
        public TemperatureMeasuredEventArgs(Hashtable temperature)
        {
            Temperature = temperature;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public Hashtable Temperature { get; private set; }
    }
}