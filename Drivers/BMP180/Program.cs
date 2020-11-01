using MBN;
using MBN.Modules;
using Microsoft.SPOT;
using MBN.Enums;
using System.Threading;
using MBN.Utilities;

namespace Examples
{
    public class Program
    {
        private static BMP180 _bmp180;

        public static void Main()
        {
            _bmp180 = new BMP180(Hardware.SocketThree, ClockRatesI2C.Clock400KHz, 1000, Hardware.SocketThree.An)
            {
                OverSamplingSetting = BMP180.Oss.UltraHighResolution,
                TemperatureUnit = TemperatureUnits.Fahrenheit
            };

            _bmp180.MeasurementComplete += MeasurementComplete;

            Debug.Print("BMP180 Demo");
            Debug.Print("Driver Version Info - " + _bmp180.DriverVersion);
            Debug.Print("Is a BMP180 connected? " + _bmp180.IsConnected());
            Debug.Print("BMP180 Sensor OSS is - " + _bmp180.OverSamplingSetting + "\n");

            /* Use one of the following methods to read sensor */
            new Thread(PollingwithEventsThread).Start();
            //new Thread(DirectReadThread).Start();

            Thread.Sleep(Timeout.Infinite);
        }

        static void MeasurementComplete(object sender, BMP180.SensorData sensorData)
        {
            Debug.Print("Temperature - " + sensorData.Temperature.ToString("f2") + (_bmp180.TemperatureUnit == TemperatureUnits.Celsius ? "°C" : _bmp180.TemperatureUnit == TemperatureUnits.Fahrenheit ? " °F" : "°K"));
            Debug.Print("Pressure (Sea Level Compensated) - " + Pressure.ToInchesHg(sensorData.CompensatedPressure).ToString("f2") + " in Hg");
            Debug.Print("Pressure (Sea Level Compensated) - " + Pressure.ToHectoPascals(sensorData.CompensatedPressure).ToString("f1") + " hPa");
            Debug.Print("Pressure (Raw) - " + Pressure.ToInchesHg(sensorData.RawPressure).ToString("f2") + " in Hg");
            Debug.Print("Pressure (Raw) - " + Pressure.ToHectoPascals(sensorData.RawPressure).ToString("f1") + " hPa");
            Debug.Print("Altitude - " + Altitude.ToFeet(sensorData.Altitude).ToString("f0") + " feet");
            Debug.Print("Altitude - " + sensorData.Altitude.ToString("f0") + " meters");
            Debug.Print("SensorData ToString method - " + sensorData + "\n");

        }

        private static void DirectReadThread()
        {
            Debug.Print("____________Sensor Data using direct read methods____________");

            while (true)
            {
                Debug.Print("Temperature - " + _bmp180.ReadTemperature().ToString("f2") + (_bmp180.TemperatureUnit == TemperatureUnits.Celsius ? "°C" : _bmp180.TemperatureUnit == TemperatureUnits.Fahrenheit ? " °F" : "°K"));
                Debug.Print("Temperature RawData - " +  (_bmp180 as ITemperature).RawData);
                Debug.Print("SLC Compensated Pressure - " + Pressure.ToInchesHg(_bmp180.ReadPressure()).ToString("f1") + " in Hg");
                Debug.Print("Uncompensated Pressure - " + Pressure.ToInchesHg(_bmp180.ReadPressure(PressureCompensationModes.Uncompensated)).ToString("f1") + " in Hg");
                Debug.Print("Pressure RawData - " + (_bmp180 as IPressure).RawData);
                Debug.Print("Altitude - " + Altitude.ToFeet(_bmp180.ReadAltitude()) + " feet");
                Debug.Print("\n");
                Thread.Sleep(5000);
            }
        }

        private static void PollingwithEventsThread()
        {
            Debug.Print("____________Sensor Data - Event Driven____________");

            while (true)
            {
                _bmp180.ReadSensor();
                Thread.Sleep(5000);
            }
        }
    }
}