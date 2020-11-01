using MBN;
using MBN.Exceptions;
using MBN.Modules;
using Microsoft.SPOT;
using System;
using System.Collections;
using System.Threading;

namespace Examples
{
    public class Program
    {
        private static DS18B20 _ds18B20;

        private static readonly byte[] Device0 = new byte[] { 40, 27, 213, 247, 4, 0, 0, 88 };
        private static readonly byte[] Device1 = new byte[] { 40, 209, 177, 246, 4, 0, 0, 137 };
        private static readonly byte[] Device2 = new byte[] { 40, 206, 219, 247, 4, 0, 0, 213 };

        public static void Main()
        {
            try
            {
                _ds18B20 = new DS18B20(Hardware.SocketTwo);
            }

            catch (PinInUseException ex) // Some pins are already used by another driver
            {
                Debug.Print("Some pins are in use while creating instances : " + ex.Message);
                Debug.Print("Stack trace : " + ex.StackTrace);
            }
            catch (DeviceInitialisationException ex) // Device initialization failed. Maybe a retry could be enough
            {
                Debug.Print("Exception during device initialization : " + ex.Message);
                _ds18B20 = new DS18B20(Hardware.SocketThree);
            }
            catch (Exception ex) // Other exception from NETMF core
            {
                Debug.Print("Exception while creating instances : " + ex.Message);
            }

            // Run this first if you want to obtain a list of current DS18B20 Sensors on the OneWire Bus.
            // To get your 64-Bit DeviceIds Uncomment the line DisplayDeviceIds()

            // I am using three sensors for this Demo1, you will need to change the values of Device0, Device1 and Device2 to reflect your 
            // sensor id for the Demo1 Code to work. Or just Comment out "RunDemo1(); and Un-Comment RunDemo2();

            //DisplayDeviceIds();

            //RunDemo1();

            RunDemo2();

            new Thread(Capture).Start();

            Thread.Sleep(Timeout.Infinite);
        }

        private static void DisplayDeviceIds()
        {
            foreach (byte[] device in _ds18B20.DeviceList)
            {
                Debug.Print(GetDeviceId(device));
            }
        }

        private static void RunDemo1()
        {
            Debug.Print("*********DS18B20 Demo**********\n");
            Debug.Print("Driver Version - " + _ds18B20.DriverVersion + "\n");
            Debug.Print("Number of Devices: " + _ds18B20.NumberOfDevices() + "\n");

            Debug.Print("Testing Setting Resolutions");
            _ds18B20.SetResolution(Device0, DS18B20.Resolution.Resolution9Bit);
            _ds18B20.SetResolution(Device1, DS18B20.Resolution.Resolution11Bit);
            _ds18B20.SetResolution(Device2, DS18B20.Resolution.Resolution12Bit);

            /* Alternate Method 1
            foreach (byte[] device in _ds18B20.DeviceList)
            {
                _ds18B20.SetResolution(device, DS18B20.Resolution.Resolution9Bit);
            }
            */

            /* Alternate Method 2
            _ds18B20.SetResolutionForAllDevices(DS18B20.Resolution.Resolution9Bit);
            */

            Debug.Print("\nSetting Resolutions done, now reading back resolutions.");
            Debug.Print("Current Resolution for Device 0: " + _ds18B20.GetResolutionString(_ds18B20.GetResolution(Device0)));
            Debug.Print("Current Resolution for Device 1: " + _ds18B20.GetResolutionString(_ds18B20.GetResolution(Device1)));
            Debug.Print("Current Resolution for Device 2: " + _ds18B20.GetResolutionString(_ds18B20.GetResolution(Device2)) + "\n");

            /* Alternate method
            foreach (byte[] device in _ds18B20.DeviceList)
            {
                Debug.Print("Current Resolution for " + GetDeviceId(device) + " - "  + _ds18B20.GetResolutionString(_ds18B20.GetResolution(device)));
            }
            */

            Debug.Print("Test - Checking for Parasitic Devices");
            Debug.Print("Is Parasitic? -" + _ds18B20.IsParasitic(Device0));
            Debug.Print("Is Parasitic? -" + _ds18B20.IsParasitic(Device1));
            Debug.Print("Is Parasitic? -" + _ds18B20.IsParasitic(Device2) + "\n");

            /* Alternate method
            foreach (byte[] device in _ds18B20.DeviceList)
            {
                Debug.Print("Is Parasitic? -" + _ds18B20.IsParasitic(device));
            }
            */

            Debug.Print("Test - Resetting Alarms");
            Debug.Print("Resetting Alarms 0 yields " + _ds18B20.ResetAlarmSettings(Device0));
            Debug.Print("Resetting Alarms 1 yields " + _ds18B20.ResetAlarmSettings(Device1));
            Debug.Print("Resetting Alarms 2 yields " + _ds18B20.ResetAlarmSettings(Device2) + "\n");

            /* ResetAlarmSettings - Alternate Method
            foreach (byte[] device in _ds18B20.DeviceList)
            {
                _ds18B20.ResetAlarmSettings(device);
            }
            */

            Debug.Print("Test - Setting individual Alarms");
            _ds18B20.SetLowTempertureAlarm(Device0, -55);
            _ds18B20.SetHighTempertureAlarm(Device0, 100);
            _ds18B20.SetLowTempertureAlarm(Device1, 124);
            _ds18B20.SetHighTempertureAlarm(Device1, 125);
            _ds18B20.SetLowTempertureAlarm(Device2, -55);
            _ds18B20.SetHighTempertureAlarm(Device2, 125);

            /* Setting Alarms - alternate method 1
            _ds18B20.SetBothTemperatureAlarms(Device0, -55, 125);
            _ds18B20.SetBothTemperatureAlarms(Device0, 30, 125);
            _ds18B20.SetBothTemperatureAlarms(Device2, -55, 125);
            */

            /* Setting Alarms - Alternate method 2
            _ds18B20.SetAlarmsForAlllDevices(-55, 125);
            */

            /* Setting alarms - Alternate method 3
            foreach (byte[] device in _ds18B20.DeviceList)
            {
               _ds18B20.SetBothTemperatureAlarms(device, -55, 125);
            }
            */

            Debug.Print("\nSetting Alarms done, now reading back alarm settings.");
            Debug.Print("Low Temp Alarm for Device0 - " + _ds18B20.ReadLowTempAlarmSetting(Device0) + "°C");
            Debug.Print("High Temp Alarm for Device0 - " + _ds18B20.ReadHighTempAlarmSetting(Device0) + "°C");
            Debug.Print("Low Temp Alarm for Device1 - " + _ds18B20.ReadLowTempAlarmSetting(Device1) + "°C");
            Debug.Print("High Temp Alarm for Device1 - " + _ds18B20.ReadHighTempAlarmSetting(Device1) + "°C");
            Debug.Print("Low Temp Alarm for Device2 - " + _ds18B20.ReadLowTempAlarmSetting(Device2) + "°C");
            Debug.Print("High Temp Alarm for Device2 - " + _ds18B20.ReadHighTempAlarmSetting(Device2) + "°C\n");

            /* Read Alarm Settings - Alternate method
            foreach (byte[] device in _ds18B20.DeviceList)
            {
                Debug.Print("Low Temp Alarm for Device - " + GetDeviceId(device) + " - " + _ds18B20.ReadLowTempAlarmSetting(device));
                Debug.Print("High Temp Alarm for Device - " + GetDeviceId(device) + " - " + _ds18B20.ReadHighTempAlarmSetting(device));
            }
            */

            Debug.Print("Only Device 1 (" + GetDeviceId(Device1) + ") will have an alarm when reading the temperature.\n");
        }

        private static void RunDemo2()
        {
            Debug.Print("*********DS18B20 Demo**********\n");
            Debug.Print("Driver Version - " + _ds18B20.DriverVersion + "\n");
            Debug.Print("Number of Devices: " + _ds18B20.NumberOfDevices() + "\n");

            Debug.Print("Testing Setting Resolutions");
            foreach (byte[] device in _ds18B20.DeviceList)
            {
                _ds18B20.SetResolution(device, DS18B20.Resolution.Resolution9Bit);
            }

            Debug.Print("Setting Resolutions done, now reading back resolutions.");
            foreach (byte[] device in _ds18B20.DeviceList)
            {
                Debug.Print("Current Resolution for " + GetDeviceId(device) + " - " + _ds18B20.GetResolutionString(_ds18B20.GetResolution(device)));
            }

            Debug.Print("\nTest - Checking for Parasitic Devices");
            foreach (byte[] device in _ds18B20.DeviceList)
            {
                Debug.Print("Is Parasitic? -" + GetDeviceId(device) + " - " + _ds18B20.IsParasitic(device));
            }

            Debug.Print("\nTest - Resetting Alarms");
            foreach (byte[] device in _ds18B20.DeviceList)
            {
                Debug.Print("Resetting Alarms for Device " + GetDeviceId(device) + " yields " + _ds18B20.ResetAlarmSettings(device));
            }

            Debug.Print("\nTest - Setting all Alarms at once");
            foreach (byte[] device in _ds18B20.DeviceList)
            {
                _ds18B20.SetBothTemperatureAlarms(device, 100, 125);
            }

            Debug.Print("\nSetting Alarms done, now reading back alarm settings.");
            foreach (byte[] device in _ds18B20.DeviceList)
            {
                Debug.Print("Low Temp Alarm for Device - " + GetDeviceId(device) + " - " + _ds18B20.ReadLowTempAlarmSetting(device));
                Debug.Print("High Temp Alarm for Device - " + GetDeviceId(device) + " - " + _ds18B20.ReadHighTempAlarmSetting(device));
            }

            Debug.Print("\nAll Devices will have an alarm when reading the temperature.\n");

        }

        private static void Capture()
        {
            while (true)
            {
                Debug.Print("Reading temperatures by DeviceID");

                foreach (byte[] id in _ds18B20.DeviceList)
                {
                    Debug.Print("By ID - Device Address - " + GetDeviceId(id) + ", Temperature - " + _ds18B20.ReadTemperatureByAddress(id) + "°C");
                }

                Debug.Print("\nReading all temperatures using ReadTemperatureForAllDevices() method");

                Hashtable temperature = _ds18B20.ReadTemperatureForAllDevices();

                foreach (DictionaryEntry t in temperature)
                {
                    Debug.Print("Key - " + GetDeviceId((byte[])t.Key) + " Value - " + t.Value);
                }

                Debug.Print("\nReadTemperature (single device) - " + _ds18B20.ReadTemperature() + "°C");

                Debug.Print("\nChecking for devices in alarm - Method 1");

                foreach (byte[] alarmingDevice in _ds18B20.AlarmList())
                {
                    Debug.Print("Alarming Device Address - " + GetDeviceId(alarmingDevice));
                }

                Debug.Print("\nChecking for devices in alarm - Method 2");

                foreach (byte[] alarmingDevice in _ds18B20.AlarmList())
                {
                    Debug.Print("Devices in alarm:  " + _ds18B20.HasAlarm(alarmingDevice) + " - " + GetDeviceId(alarmingDevice));
                }

                Debug.Print("\n***************************************************************************************************");
                Thread.Sleep(5000);
            }
        }

        private static string GetDeviceId(byte[] id)
        {
            return id[0] + "," + id[1] + "," + id[2] + "," + id[3] + "," + id[4] + "," + id[5] + "," + id[6] + "," + id[7];
        }
    }
}