using MBN;
using MBN.Enums;
using MBN.Exceptions;
using MBN.Modules;
using System;
using Microsoft.SPOT;
using System.Threading;

namespace Examples
{
    public class Program
    {
        private static AltitudeClick _altitude;
        private static Boolean _deviceOk;

        public static void Main()
        {
            var counter = 1;
            _deviceOk = false;

            while (!_deviceOk)
            {
                try
                {
                    _altitude = new AltitudeClick(Hardware.SocketOne, ClockRatesI2C.Clock100KHz) {OverSampling = AltitudeClick.Oss.Oss7, TemperatureUnit = TemperatureUnits.Fahrenheit};
                    _altitude.MeasurementComplete += MeasurementComplete;

                    Debug.Print("Initialization at " + " try number " + counter);
                    _deviceOk = true;
                }
                catch (DeviceInitialisationException)
                {
                    Debug.Print("Initialization failed, retrying...");
                    if (counter >= 10) throw new DeviceInitialisationException("Altitude Click failed to initialize. Please check your Hardware.");
                    counter++;
                }
            }

            Debug.Print("Who am I" + _altitude.WhoAmI);

            while (true)
            {
                _altitude.ReadSensor();

                /* Uncomment the following to use non-event driven methods for obtaining sensor data. */

                //var rawAltitude = _altitude.ReadAltitude(PressureCompensationModes.Uncompensated);
                //var compensatedAltitude = _altitude.ReadPressure(PressureCompensationModes.SeaLevelCompensated);
                //var rawPressure = _altitude.ReadPressure(PressureCompensationModes.Uncompensated);
                //var compensatedPressure = _altitude.ReadPressure(PressureCompensationModes.SeaLevelCompensated);
                //var temp = _altitude.ReadTemperature(TemperatureSources.Ambient);

                //Debug.Print("Raw Altitude - " + rawAltitude + "  meters");
                //Debug.Print("Sea Level Compensated Altitude - " + compensatedAltitude + "  meters");
                //Debug.Print("Raw Pressure - " + rawPressure + " Pa");
                //Debug.Print("Seal Level Compensated Pressure - " + compensatedPressure + " Pa");
                //Debug.Print("Temperature - " + temp + (_altitude.TemperatureUnit == TemperatureUnits.Celsius ? "°F" : _altitude.TemperatureUnit == TemperatureUnits.Fahrenheit ? " °F" : "°K") + "\n");

                Thread.Sleep(5000);
            }
        }

        static void MeasurementComplete(object sender, AltitudeClick.MeasurementCompleteEventArgs e)
        {
            Debug.Print("Uncompensated Altitude - " + e.RawAltitude + "  meters");
            Debug.Print("Sea Level Compensated Altitude - " + e.CompensatedAltitude + "  meters");
            Debug.Print("Uncompensated Pressure - " + e.RawPressure + " Pa");
            Debug.Print("Sea Level Compensated Pressure - " + e.CompensatedPressure + " Pa");
            Debug.Print("Temperature - " + e.Temperature + (_altitude.TemperatureUnit == TemperatureUnits.Celsius ? "°F" : _altitude.TemperatureUnit == TemperatureUnits.Fahrenheit ? " °F" : "°K") + "\n");
        }
    }
}




