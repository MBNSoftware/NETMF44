
using System;
using Microsoft.SPOT.Hardware;

// ReSharper disable once CheckNamespace
namespace MBN.Extensions
{
    /// <summary>
    /// Extensions methods for Strings
    /// </summary>
    public static class MBNStrings
    {
        /// <summary>
        /// Returns a new string that right-aligns the characters in this instance by padding them on the left with a specified Unicode character, for a specified total length.
        /// </summary>
        /// <param name="source">The underlying String object. Omit this parameter when you call that method.</param>
        /// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
        /// <param name="paddingChar">A Unicode padding character.</param>
        /// <returns>A new string that is equivalent to this instance, but right-aligned and padded on the left with as many paddingChar characters as needed to create a length of totalWidth.
        ///  However, if totalWidth is less than the length of this instance, the method returns a reference to the existing instance.
        ///  If totalWidth is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
        /// <example>Example usage 
        /// <code language="C#">
        /// String str = "12";
        /// 
        /// Debug.Print("Padded left : " + str.PadLeft(5, '0')); // Displays "Padded left : 00012"
        /// </code>
        /// </example>
        public static String PadLeft(this String source, Int32 totalWidth, Char paddingChar)
        {
            if (source.Length >= totalWidth) return source;
            do
            {
                source = paddingChar + source;
            }
            while (source.Length < totalWidth);

            return source;
        }

        /// <summary>
        /// Returns a new string that left-aligns the characters in this string by padding them on the right with a specified Unicode character, for a specified total length.
        /// </summary>
        /// <param name="source">The underlying String object. Omit this parameter when you call that method.</param>
        /// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
        /// <param name="paddingChar">A Unicode padding character.</param>
        /// <returns>A new string that is equivalent to this instance, but left-aligned and padded on the right with as many paddingChar characters as needed to create a length of totalWidth.
        ///  However, if totalWidth is less than the length of this instance, the method returns a reference to the existing instance.
        ///  If totalWidth is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
        /// <example>Example usage 
        /// <code language="C#">
        /// String str = "12";
        /// 
        /// Debug.Print("Padded right : " + str.PadRight(5, '0')); // Displays "Padded right : 12000"
        /// </code>
        /// </example>
        public static String PadRight(this String source, Int32 totalWidth, Char paddingChar)
        {
            if (source.Length >= totalWidth) return source;
            do
            {
                source = source + paddingChar;
            }
            while (source.Length < totalWidth);

            return source;
        }
    }
    /// <summary>
    /// SPI bus on MBN boards. This should be used instead of standard SPI object of NETMF.
    /// </summary>
    public static class SPIBus
    {
        /// <summary>
        /// Writes an array of bytes arguments to the interface.
        /// </summary>
        /// <param name="spiDev">The underlying SPI object. Omit this parameter when you call that method</param>
        /// <param name="pConfig">The SPI configuration used for this transfer.</param>
        /// <param name="pArray">The buffer that will write to the interface.</param>
        public static void Write(this SPI spiDev, SPI.Configuration pConfig, byte[] pArray)
        {
            lock (Hardware.SpiLock)
            {
                spiDev.Config = pConfig;
                spiDev.Write(pArray);
            }
        }

        /// <summary>
        /// Writes an array of unsigned short arguments to the interface.
        /// </summary>
        /// <param name="spiDev">The underlying SPI object. Omit this parameter when you call that method</param>
        /// <param name="pConfig">The SPI configuration used for this transfer.</param>
        /// <param name="pArray">The buffer that will write to the interface.</param>
        public static void Write(this SPI spiDev, SPI.Configuration pConfig, UInt16[] pArray)
        {
            lock (Hardware.SpiLock)
            {
                spiDev.Config = pConfig;
                spiDev.Write(pArray);
            }
        }

        /// <summary>
        /// Writes an array of bytes to the interface, and reads an array of bytes from the interface.
        /// </summary>
        /// <param name="spiDev">The underlying SPI object. Omit this parameter when you call that method</param>
        /// <param name="pConfig">The SPI configuration used for this transfer.</param>
        /// <param name="writeBuffer">The buffer that will write to the interface.</param>
        /// <param name="readBuffer">The buffer that will store the data that is read from the interface.</param>
        public static void WriteRead(this SPI spiDev, SPI.Configuration pConfig, byte[] writeBuffer, byte[] readBuffer)
        {
            lock (Hardware.SpiLock)
            {
                spiDev.Config = pConfig;
                spiDev.WriteRead(writeBuffer, readBuffer);
            }
        }

        /// <summary>
        /// Writes an array of bytes to the interface, and reads an array of bytes from the interface.
        /// </summary>
        /// <param name="spiDev">The underlying SPI object. Omit this parameter when you call that method</param>
        /// <param name="pConfig">The SPI configuration used for this transfer.</param>
        /// <param name="writeBuffer">The buffer that will write to the interface.</param>
        /// <param name="readBuffer">The buffer that will store the data that is read from the interface.</param>
        /// <param name="startReadOffset">The offset in time, measured in transacted elements from writeBuffer, when to start reading back data into readBuffer.</param>
        public static void WriteRead(this SPI spiDev, SPI.Configuration pConfig, byte[] writeBuffer, byte[] readBuffer, int startReadOffset)
        {
            lock (Hardware.SpiLock)
            {
                spiDev.Config = pConfig;
                spiDev.WriteRead(writeBuffer, readBuffer, startReadOffset);
            }
        }

        /// <summary>
        /// Writes an array of bytes to the interface, and reads an array of bytes from the interface.
        /// </summary>
        /// <param name="spiDev">The underlying SPI object. Omit this parameter when you call that method</param>
        /// <param name="pConfig">The SPI configuration used for this transfer.</param>
        /// <param name="writeBuffer">The buffer whose contents will be written to the interface.</param>
        /// <param name="writeOffset">The offset in writeBuffer to start write data from.</param>
        /// <param name="writeCount">The number of elements in writeBuffer to write.</param>
        /// <param name="readBuffer">The buffer that will store the data that is read from the interface.</param>
        /// <param name="readOffset">The offset in readBuffer to start read data from.</param>
        /// <param name="readCount">The number of elements in readBuffer to fill.</param>
        /// <param name="startReadOffset">The offset in time, measured in transacted elements from writeBuffer, when to start reading back data into readBuffer.</param>
        public static void WriteRead(this SPI spiDev, SPI.Configuration pConfig, byte[] writeBuffer, int writeOffset, int writeCount, byte[] readBuffer, int readOffset, int readCount, int startReadOffset)
        {
            lock (Hardware.SpiLock)
            {
                spiDev.Config = pConfig;
                spiDev.WriteRead(writeBuffer, writeOffset, writeCount, readBuffer, readOffset, readCount, startReadOffset);
            }
        }
    }

    /// <summary>
    /// I²C bus on MBN boards. This should be used instead of standard I²C object of NETMF.
    /// </summary>
    public static class I2CBus
    {
        /// <summary>
        /// Executes the specified I²C transaction.
        /// </summary>
        /// <param name="I2CDev">The underlying I²C object. Omit this parameter when you call that method</param>
        /// <param name="pConfig">The I²C configuration used for this transfer.</param>
        /// <param name="xActions">Array containing a list of I²C transactions.</param>
        /// <param name="timeout">Timeout before telling that there was an error executing the transaction(s).</param>
// ReSharper disable once InconsistentNaming
        public static Int32 Execute(this I2CDevice I2CDev, I2CDevice.Configuration pConfig, I2CDevice.I2CTransaction[] xActions, int timeout)
        {
            lock (Hardware.I2CLock)
            {
                I2CDev.Config = pConfig;
                return I2CDev.Execute(xActions, timeout);
            }
        }
    }

    /// <summary>
    ///     An Enumeration extension class providing additional functionality to Microsoft Net Framework (4.3).
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        ///     Extension method to determine is a specific flag is set in the reference Enumeration.
        /// </summary>
        /// <param name="source">The source Enumeration to test against.</param>
        /// <param name="flag">The flag to test for.</param>
        /// <returns>True if the referenced array contains the flag passed in the flag parameter, otherwise false.</returns>
        /// <example>Example usage to determine if there are any Alarms returned in the TemperatureHumidityMeasured Event. 
        /// <code language="C#">
        /// static void _sht11Click_TemperatureHumidityMeasured(object sender, TemperatureHumidityEventArgs e)
        /// {
        ///    if (e.Alarms.Contains(Alarms.TemperatureHigh)) Debug.Print("High Temperature Alarm Present");
        /// }
        /// </code>
        /// <code language="VB">
        /// Private Shared Sub _sht11Click_TemperatureHumidityMeasured(sender As Object, e As TemperatureHumidityEventArgs)
        ///     If e.Alarms.Contains(Alarms.TemperatureHigh) Then 
        ///          Debug.Print("High Temperature Alarm Present")
        ///     End If
        /// End Sub
        /// </code>
        /// </example>
        public static bool ContainsFlag(this Enum source, Enum flag)
        {
            var sourceValue = ToUInt64(source);
            var flagValue = ToUInt64(flag);

            return (sourceValue & flagValue) == flagValue;
        }

        /// <summary>
        ///     Extension method to determine is a any flag passed as a parameter array is set in the reference Enumeration.
        /// </summary>
        /// <param name="source">The source Enumeration to test against.</param>
        /// <param name="flags">The parameter array of flags to test for.</param>
        /// <returns>True if the referenced array contains any one of the flags passed in the parameter array, otherwise false.</returns>
        /// <example>Example usage to determine if there are any Alarms returned in the TemperatureHumidityMeasured Event. 
        /// <code language="C#">
        /// static void _sht11Click_TemperatureHumidityMeasured(object sender, TemperatureHumidityEventArgs e)
        /// {
        ///    if (e.Alarms.ContainsAny(Alarms.TemperatureHigh, Alarms.HumidityHigh)) Debug.Print("High Temperature and High Humidity Alarms Present");
        /// }
        /// </code>
        /// <code language="VB">
        /// Private Shared Sub _sht11Click_TemperatureHumidityMeasured(sender As Object, e As TemperatureHumidityEventArgs)
        ///     If e.Alarms.ContainsAny(Alarms.TemperatureHigh, Alarms.HumidityHigh) Then 
        ///          Debug.Print("High Temperature and High Humidity Alarms Present")
        ///     End If
        /// End Sub
        /// </code>
        /// </example>
        public static bool ContainsAnyFlag(this Enum source, params Enum[] flags)
        {
            var sourceValue = ToUInt64(source);

            foreach (var flag in flags)
            {
                var flagValue = ToUInt64(flag);

                if ((sourceValue & flagValue) == flagValue)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Extension method to determine is a any flag passed as a parameter array is set in the reference Enumeration.
        /// </summary>
        /// <param name="source">The source Enumeration to test against.</param>
        /// <param name="flag">The flag to test for.</param>
        /// <returns>True if the referenced array contains the flag passed in the flag parameter, otherwise false.</returns>
        /// <example>Example usage to determine if there are any Alarms returned in the TemperatureHumidityMeasured Event. 
        /// <code language="C#">
        /// static void _sht11Click_TemperatureHumidityMeasured(object sender, TemperatureHumidityEventArgs e)
        /// {
        ///    if (e.Alarms.IsSet(Alarms.NoAlarm)) Debug.Print("No alarms present");
        /// }
        /// </code>
        /// <code language="VB">
        /// Private Shared Sub _sht11Click_TemperatureHumidityMeasured(sender As Object, e As TemperatureHumidityEventArgs)
        ///     If e.Alarms.IsSet(Alarms.NoAlarm) Then 
        ///         Debug.Print("No alarms present")
        ///     End If
        /// End Sub
        /// </code>
        /// </example>
        public static bool IsSet(this Enum source, Enum flag)
        {
            return (Convert.ToUInt32(source.ToString()) & Convert.ToUInt32(flag.ToString())) != 0;
        }

        private static ulong ToUInt64(object value)
        {
            return Convert.ToUInt64(value.ToString());
        }
    }

    /// <summary>
    /// An utility class for 2's complements
    /// </summary>
	public static class MBNBytes
    {
        /// <summary>
        /// Gets the 2's complement of a Byte value
        /// </summary>
        /// <param name="value">The byte value to convert.</param>
        /// <returns>An Int32 representing the 2's complement</returns>
        public static Int32 TwoComplement(this Byte value)
        {
            if ((value & 0x80) != 0x80) return value;
            int valtmp = value;
            return -1 * ((Byte)(~valtmp)) - 1;
        }

        /// <summary>
        /// Gets the Byte's 2's complement of a Integer value
        /// </summary>
        /// <param name="value">The Int32 value to convert.</param>
        /// <returns>An Byte representing the 2's complement</returns>
        public static Byte TwoComplement(this Int32 value)
        {
            if ((value & 0x80) != 0x80) return (Byte)value;
            return (Byte)(0xFF + value + 1);
        }
    }

    /// <summary>
    /// Extensions for Time methods
    /// </summary>
    public static class MBNTimeExtensions
    {
        /// <summary>
        /// Convert an 8-byte array from NTP format to .NET DateTime.  
        /// </summary>
        /// <param name="ntpTime">NTP format 8-byte array containing date and time</param>
        /// <returns>A Standard .NET DateTime</returns>
        public static DateTime ToDateTime(this byte[] ntpTime)
        {
            Microsoft.SPOT.Debug.Assert(ntpTime.Length == 8, "The passed array is too short to be a valid NTP DateTime.");

            ulong intpart = 0;
            ulong fractpart = 0;

            for (var i = 0; i <= 3; i++)
                intpart = (intpart << 8) | ntpTime[i];

            for (int i = 4; i <= 7; i++)
                fractpart = (fractpart << 8) | ntpTime[i];

            var milliseconds = (intpart*1000 + (fractpart*1000)/0x100000000L);

            var timeSince1900 = TimeSpan.FromTicks((long) milliseconds*TimeSpan.TicksPerMillisecond);
            return new DateTime(1900, 1, 1).Add(timeSince1900);
        }
    }
}

