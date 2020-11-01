using System;
using MBN;
using MBN.Enums;
using MBN.Modules;
using Microsoft.SPOT;
using System.Threading;

namespace Examples
{
    public class Program
    {
        private static RHT03 _rht03;

        public static void Main()
        {
            Debug.Print("Started");

            _rht03 = new RHT03(Hardware.SocketFour)
            {
                SamplingFrequency = new TimeSpan(0, 0, 0, 2),
                TemperatureUnit = TemperatureUnits.Fahrenheit
            };

            _rht03.SensorError += _rht03_SensorError;

            Debug.Print("Version Info - " + _rht03.DriverVersion);

            /* Uncomment the following two lines for Continuous/Event Driven Measurement Mode.*/
            //_rht03.TemperatureHumidityMeasured += _rht03_OnMeasurement;
            //_rht03.StartContinuousMeasurements();

            // Comment to read temperature and Humidity Properties in the capture thread.
            new Thread(Capture).Start();


            Thread.Sleep(Timeout.Infinite);
        }

        private static void _rht03_SensorError(object sender, string errorMessage)
        {
            Debug.Print(errorMessage);
        }

        private static void _rht03_OnMeasurement(object sender, float temperature, float humidity)
        {
            Debug.Print("---------Event Driven----------");
            Debug.Print("Temperature - " + temperature.ToString("f2") + (_rht03.TemperatureUnit == TemperatureUnits.Celsius ? "°C" : _rht03.TemperatureUnit == TemperatureUnits.Fahrenheit ? " °F" : "°K"));
            Debug.Print("Humidity - " + humidity.ToString("f2") + " % RH\n");
        }


        static double ToFahrenheit(double celsius)
        {
            return celsius * 9 / 5 + 32;
        }

        private static void Capture()
        {
            Debug.Print("\n--------Polling the Properties-----------");

            while (true)
            {
                _rht03.RequestMeasurement();

                Debug.Print("Humidity - " + _rht03.Humidity.ToString("f2") + " % RH");
                Debug.Print("Temperature - " + _rht03.Temperature.ToString("f2") + (_rht03.TemperatureUnit == TemperatureUnits.Celsius ? "°C" : _rht03.TemperatureUnit == TemperatureUnits.Fahrenheit ? " °F" : "°K"));
                Thread.Sleep(1000);
            }
        }
    }
}
