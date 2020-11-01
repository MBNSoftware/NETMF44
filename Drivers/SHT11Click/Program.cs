using System;
using MBN.Extensions;
using MBN.Modules;
using Microsoft.SPOT;
using MBN;
using MBN.Utilities;
using System.Threading;
using MBN.Enums;
using MBN.Exceptions;

// ReSharper disable once CheckNamespace
namespace Examples
{
    public class Program
    {
        private static SHT11Click _sht11;

        public static void Main()
        {
            try
            {
                _sht11 = new SHT11Click(Hardware.SocketFour);
                _sht11.TemperatureHumidityMeasured += TemperatureHumidityMeasured;
            }

            catch (PinInUseException ex) // Some pins are already used by another driver
            {
                Debug.Print("Some pins are in use while creating instances : " + ex.Message);
                Debug.Print("Stack trace : " + ex.StackTrace);
            }

            catch (DeviceInitialisationException ex)
            {
                Debug.Print("Exception during device initialization : " + ex.Message);
            }

            catch (Exception ex) // Other exception from NETMF core
            {
                Debug.Print("Exception while creating instances : " + ex.Message);
            }

            Debug.Print("*********************************SHT11click Demo**********************************");
            Debug.Print("Driver Version Info : " + _sht11.DriverVersion + "\n");
            
            ResolutionDemo();
            HeaterDemo();
            NoReloadFromOTPDemo();
            ResetDemo();
            TempHumidityDemo();

            new Thread(Capture).Start();

            Thread.Sleep(Timeout.Infinite);
        }

        private static void TempHumidityDemo()
        {
            Debug.Print("********************Direct Reading of Temperature and Humidity Demo****************");

            for (int count = 0; count <= 15; count++)
            {
                Debug.Print("Temperature - " + Temperature.ConvertTo(TemperatureUnits.Celsius, TemperatureUnits.Fahrenheit, _sht11.ReadTemperature()).ToString("f2") + " °F");
                Debug.Print("Humidity - " + _sht11.ReadHumidity().ToString("f2") + " %RH");
                Thread.Sleep(1000);
            }
            Debug.Print("Direct Reading of Temperature and Humidity Demo is complete\n");
            Thread.Sleep(5000);
        }

        private static void ResolutionDemo()
        {
            Debug.Print("*********************************Resolution Demo***********************************");
            Debug.Print("Sensor Resolution before changing resolution  (0 = High, 1 = Low) : " + _sht11.SensorResolution);
            Debug.Print("Now setting the resolution to low");
            _sht11.SensorResolution = SHT11Click.Resolution.Low;
            Debug.Print("Sensor Resolution after changing resolution (0 = High, 1 = Low) : " + _sht11.SensorResolution);
            Debug.Print("Resolution Demo is complete\n");
            Thread.Sleep(5000);
        }

        private static void HeaterDemo()
        {
            Debug.Print("***********************************Heater Demo*************************************");
            Debug.Print("Heater setting before changing  (0 = Off, 1 = On) : " + _sht11.GetHeaterStatus());
            Debug.Print("Now turning on the heater, it will be automatically set to turn off in 15 seconds.");
            _sht11.SetHeater(SHT11Click.HeaterStatus.On, new TimeSpan(0, 0, 0, 15));
            Debug.Print("Heater setting after changing  (0 = Off, 1 = On) : " + _sht11.GetHeaterStatus());

            while (_sht11.GetHeaterStatus() == SHT11Click.HeaterStatus.On)
            {
                Debug.Print("Time : " + DateTime.Now + " - " + (_sht11.GetHeaterStatus() == SHT11Click.HeaterStatus.On ? "Heater is On" : "Heater is Off"));
                Thread.Sleep(1000);
            }
            Debug.Print("Time : " + DateTime.Now + " - " + (_sht11.GetHeaterStatus() == SHT11Click.HeaterStatus.On ? "Heater is On" : "Heater is Off"));
            Debug.Print("Heater Demo is complete\n");
            Thread.Sleep(5000);
        }

        private static void NoReloadFromOTPDemo()
        {
            Debug.Print("*******************************NoReloadFromOTP Demo********************************");
            Debug.Print("NoReloadFromOTP setting before changing (False = Off, True = On) : " + _sht11.NoReloadFromOTP);
            Debug.Print("Now setting the NoReloadFromOTP to True (On)");
            _sht11.NoReloadFromOTP = true;
            Debug.Print("NoReloadFromOTP after changing resolution (False = Off, True = On) : " + _sht11.NoReloadFromOTP);
            _sht11.NoReloadFromOTP = false;
            Debug.Print("NoReloadFromOTP Demo is complete\n");
            Thread.Sleep(5000);
        }

        private static void ResetDemo()
        {
            Debug.Print("********************************Reset Demo*******************************************");
            Debug.Print("Setting all StatusRegister bits to non default values");
            _sht11.SensorResolution = SHT11Click.Resolution.Low;
            _sht11.NoReloadFromOTP = true;
            _sht11.SetHeater(SHT11Click.HeaterStatus.On, new TimeSpan(0, 0, 0, 15));

            Debug.Print("Sensor Resolution before Reset(ResetMode.Soft) called (Should be 1) : " + _sht11.SensorResolution);
            Debug.Print("NoReloadFromOTP setting before Reset(ResetMode.Soft) called (Should be True) : " + _sht11.NoReloadFromOTP);
            Debug.Print("Heater setting before Reset(ResetMode.Soft) called (Should be 1) : " + _sht11.GetHeaterStatus() + "\n");

            Debug.Print("Now performing a Hard Reset. Values should not change.");
            if (!_sht11.Reset(ResetModes.Hard)) Debug.Print("Hard reset not successful");
            
            Debug.Print("Sensor Resolution after Reset(ResetModes.Hard) called (Should be 1) : " + _sht11.SensorResolution);
            Debug.Print("NoReloadFromOTP after before Reset(ResetModes.Hard) called (Should be True) : " + _sht11.NoReloadFromOTP);
            Debug.Print("Heater setting after Reset(ResetModes.Hard) called (Should be 1) : " + _sht11.GetHeaterStatus() + "\n");

            Debug.Print("Now performing a Soft Reset. Values should be changed to defaults.");
            if (!_sht11.Reset(ResetModes.Soft)) Debug.Print("Soft reset not successful");

            Debug.Print("Sensor Resolution after Reset(ResetModes.Soft) called (Should be 0) : " + _sht11.SensorResolution);
            Debug.Print("NoReloadFromOTP after before Reset(ResetModes.Soft) called (Should be False) : " + _sht11.NoReloadFromOTP);
            Debug.Print("Heater setting after Reset(ResetModes.soft) called (Should be 0) : " + _sht11.GetHeaterStatus());

            Debug.Print("End of ResetDemo\n");
            Thread.Sleep(5000);
        }

        static void TemperatureHumidityMeasured(object sender, SHT11Click.TemperatureHumidityEventArgs e)
        {

            string alarmsPresent = "SHT11Alarms Present : ";

            /* There are several ways in which to determine if there are any SHT11Alarms present */

            // Method 1 - SHT11Alarms enumeration can be treated as a bit field using the XOr Operator as it has the [FlagsAttribute] set;
            //if ((e.SHT11Alarms ^ SHT11Click.SHT11Alarms.NoAlarm) == 0x00) alarmsPresent += "No SHT11Alarms present.";
            //if ((e.SHT11Alarms ^ SHT11Click.SHT11Alarms.TemperatureLow) == 0x00) alarmsPresent += "Low Temperature.";
            //if ((e.SHT11Alarms ^ SHT11Click.SHT11Alarms.TemperatureHigh) == 0x00) alarmsPresent += "High Temperature.";
            //if ((e.SHT11Alarms ^ SHT11Click.SHT11Alarms.HumidityLow) == 0x00) alarmsPresent += "Low Humidity.";
            //if ((e.SHT11Alarms ^ SHT11Click.SHT11Alarms.HumidityHigh) == 0x00) alarmsPresent += "High Humidity.";
            //if ((e.SHT11Alarms ^ SHT11Click.SHT11Alarms.TemperatureLowHumidityLow) == 0x00) alarmsPresent += "Low Temperature, Low Humidity.";
            //if ((e.SHT11Alarms ^ SHT11Click.SHT11Alarms.TemperatureLowHumidityHigh) == 0x00) alarmsPresent += "Low Temperature, High Temperature.";
            //if ((e.SHT11Alarms ^ SHT11Click.SHT11Alarms.TemperatureHighHumidityLow) == 0x00) alarmsPresent += "Temperature High, Low Humidity.";
            //if ((e.SHT11Alarms ^ SHT11Click.SHT11Alarms.TemperatureHighHumidityHigh) == 0x00) alarmsPresent += "Temperature High, High Humidity.";

            // Method 2 - Using the Enum Extension Method - ContainsFlag()
            if (e.SHT11Alarms.ContainsFlag(SHT11Click.SHT11Alarms.NoAlarm)) alarmsPresent += "No SHT11Alarms present";
            if ((e.SHT11Alarms.ContainsFlag(SHT11Click.SHT11Alarms.TemperatureLow))) alarmsPresent += "Low Temperature";
            if ((e.SHT11Alarms.ContainsFlag(SHT11Click.SHT11Alarms.TemperatureHigh))) alarmsPresent += "High Temperature";
            if ((e.SHT11Alarms.ContainsFlag(SHT11Click.SHT11Alarms.HumidityLow))) alarmsPresent += ", Low Humidity";
            if ((e.SHT11Alarms.ContainsFlag(SHT11Click.SHT11Alarms.HumidityHigh))) alarmsPresent += ", High Humidity";

            // Method 3 - Using the Enum Extension Method - IsSet()
            //if (e.SHT11Alarms.IsSet(SHT11Click.SHT11Alarms.NoAlarm)) alarmsPresent += "No SHT11Alarms present";
            //if (e.SHT11Alarms.IsSet(SHT11Click.SHT11Alarms.TemperatureLow)) alarmsPresent += "Low Temperature";
            //if (e.SHT11Alarms.IsSet(SHT11Click.SHT11Alarms.TemperatureHigh)) alarmsPresent += "High Temperature";
            //if (e.SHT11Alarms.IsSet(SHT11Click.SHT11Alarms.HumidityLow)) alarmsPresent += ", Low Humidity";
            //if (e.SHT11Alarms.IsSet(SHT11Click.SHT11Alarms.HumidityHigh)) alarmsPresent += ", High Humidity";

            // Method 4 - Using the Enum Extension Method - ContainsAny()
            if (e.SHT11Alarms.ContainsAnyFlag(SHT11Click.SHT11Alarms.TemperatureHigh, SHT11Click.SHT11Alarms.HumidityHigh)) alarmsPresent += "SHT11Alarms preset."; // In this case I'm only looking for a high temperature alarm.

             //Method 5 - Using a switch statement
            //switch (e.SHT11Alarms)
            //{
            //    case SHT11Click.SHT11Alarms.NoAlarm:
            //        alarmsPresent += "No SHT11Alarms Present";
            //        break;
            //    case SHT11Click.SHT11Alarms.TemperatureLow:
            //        alarmsPresent += "Low Temperature.";
            //        break;
            //    case SHT11Click.SHT11Alarms.TemperatureHigh:
            //        alarmsPresent += "High Temperature.";
            //        break;
            //    case SHT11Click.SHT11Alarms.HumidityLow:
            //        alarmsPresent += "Low Humidity.";
            //        break;
            //    case SHT11Click.SHT11Alarms.HumidityHigh:
            //        alarmsPresent += "High Humidity.";
            //        break;
            //    case SHT11Click.SHT11Alarms.TemperatureLowHumidityLow:
            //        alarmsPresent += "Low Temperature and Low Humidity.";
            //        break;
            //    case SHT11Click.SHT11Alarms.TemperatureHighHumidityHigh:
            //        alarmsPresent += "High Temperature and High Humidity.";
            //        break;
            //    case SHT11Click.SHT11Alarms.TemperatureLowHumidityHigh:
            //        alarmsPresent += "Low Temperature and High Humidity.";
            //        break;
            //    case SHT11Click.SHT11Alarms.TemperatureHighHumidityLow:
            //        alarmsPresent += "High Temperature and Low Humidity.";
            //        break;
            //}

            Debug.Print("Temperature - " + Temperature.ConvertTo(TemperatureUnits.Celsius, TemperatureUnits.Fahrenheit, e.Temerature).ToString("f2") + " °F");
            Debug.Print("Humidity - " + e.Humidity.ToString("f2") + " %RH");
            Debug.Print("ReadRaw (Temperature) - " + (_sht11 as ITemperature).RawData);
            Debug.Print("ReadRaw (Humidity) - " + (_sht11 as ITemperature).RawData);
            Debug.Print("Dew Point - " + Temperature.ConvertTo(TemperatureUnits.Celsius, TemperatureUnits.Fahrenheit, Humidity.CalculateDewPoint(e.Temerature, e.Humidity)).ToString("f2") + " °F");
            Debug.Print(alarmsPresent);
            Debug.Print("Heater is -  " + (_sht11.GetHeaterStatus() == SHT11Click.HeaterStatus.On ? "On" : "Off\nSafe to end demo."));
            Debug.Print("---------------------------------------------------\n");
        }

       
       private static void Capture()
        {
           Debug.Print("*********************************Main Demo********************************************");
           Debug.Print("Setting AlarmThreshold to trigger High Temperature and High Humidity SHT11Alarms.");
           _sht11.AlarmThresholds = new AlarmThresholds(-40, -39, 0, 1);
           Debug.Print("Turning on the Heater. Watch the temperature rise and the humidity fall.\nThe Dew Point should stay relatively unchanged.");
           Debug.Print("The heater will automatically turn off in 2 minutes. Watch the temperature return to normal.\n");
           _sht11.SetHeater(SHT11Click.HeaterStatus.On, new TimeSpan(0, 0, 30));
           Debug.Print("CAUTION - Be patient and do not end the demo until the heater turns off.\n");
           Thread.Sleep(5000);

            while (true)
            {
                _sht11.ReadTemperatureHumidity();
               Thread.Sleep(2000);
            }
        }
    }
}
