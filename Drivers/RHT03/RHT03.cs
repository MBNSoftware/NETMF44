using MBN.Enums;
using MBN.Exceptions;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using System.Reflection;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace MBN.Modules
{
    /// <summary>
    /// MiKroBusNet NetMF Driver for a prototyped RHT03 Sensor.
    /// <para><b>The RHT03 is a Generic Module.</b></para>
    /// <para><b>Pins used :</b> Rst, Int</para>
    /// <para><b>References required:</b> MikroBus.Net, Microsoft.SPOT.Native, Microsoft.Spot.Hardware, mscorlib</para>
    /// </summary>
    /// <example>
    /// <code language = "C#">
    /// using System;
    /// using MBN;
    /// using MBN.Modules;
    /// using Microsoft.SPOT;
    /// using System.Threading;
    ///
    /// namespace Examples
    /// {
    ///     public class Program
    ///     {
    ///         private static RHT03 _rht03;
    ///
    ///         public static void Main()
    ///         {
    ///             Debug.Print("Started");
    ///
    ///             _rht03 = new RHT03(Hardware.SocketFour);
    /// 
    ///             _rht03.SamplingFrequency = new TimeSpan(0, 0, 0, 2);
    ///             _rht03.TemperatureUnit = TemperatureUnits.Fahrenheit;
    /// 
    ///             _rht03.SensorError += _rht03_SensorError;
    ///
    ///             Debug.Print("Version Info - " +  _rht03.DriverVersion);
    ///
    ///             /* Uncomment the following two lines for Continuous/Event Driven Measurement Mode.*/
    ///             //_rht03.TemperatureHumidityMeasured += _rht03_OnMeasurement;
    ///             //_rht03.StartContinuousMeasurements();
    ///
    ///             // Comment to read temperature and Humidity Properties in the capture thread.
    ///             new Thread(Capture).Start();
    ///
    ///
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    ///
    ///         private static void _rht03_SensorError(object sender, string errorMessage)
    ///         {
    ///             Debug.Print(errorMessage);
    ///         }
    ///
    ///         private static void _rht03_OnMeasurement(object sender, float temperature, float humidity)
    ///         {
    ///             Debug.Print("---------Event Driven----------");
    ///             Debug.Print("Temperature - " + temperature.ToString("f2") + (_rht03.TemperatureUnit == TemperatureUnits.Celsius ? "°C" : _rht03.TemperatureUnit == TemperatureUnits.Fahrenheit ? " °F" : "°K"));
    ///             Debug.Print("Humidity - " + humidity.ToString("f2") + " % RH\n");
    ///         }
    ///
    ///         private static void Capture()
    ///         {
    ///             Debug.Print("\n--------Polling the Properties-----------");
    ///
    ///             while (true)
    ///             {
    ///                 _rht03.RequestMeasurement();
    ///
    ///                 Debug.Print("Humidity - " + _rht03.Humidity.ToString("f2") + " % RH");
    ///                 Debug.Print("Temperature - " + ToFahrenheit(_rht03.Temperature).ToString("f2") + " °F");
    ///                 Thread.Sleep(1000);
    ///             }
    ///        }
    ///     }
    /// } 
    /// </code>
    /// <code language = "VB">
    /// Option Explicit On
    /// Option Strict On
    ///
    /// Imports System
    /// Imports MBN
    /// Imports Microsoft.SPOT
    /// Imports System.Threading
    /// Imports MBN.Modules
    ///
    /// Namespace Examples
    ///
    ///     Public Module Module1
    ///
    ///         Dim WithEvents _rht03 As RHT03
    ///
    ///         Sub Main()
    ///
    ///             _rht03 = New RHT03(Hardware.SocketOne)
    ///
    ///             _rht03.SamplingFrequency = New TimeSpan(0, 0, 0, 2)
    ///             _rht03.TemperatureUnit = TemperatureUnits.Fahrenheit
    ///
    ///             Debug.Print("Version Info - " <![CDATA[&]]> _rht03.DriverVersion.ToString())
    ///
    ///             ' Uncomment the following line for Continuous/Event Driven Measurement Mode.
    ///             '_rht03.StartContinuousMeasurements();
    ///
    ///             '  Reads Temperature and Humidity Properties in the capture thread
    ///             ' Comment two lines below and uncomment line above to read in Continuous/Event Driven Measurement Mode..
    ///             Dim captureThread As New Thread(New ThreadStart(AddressOf Capture))
    ///             captureThread.Start()
    ///
    ///             Thread.Sleep(Timeout.Infinite)
    ///
    ///         End Sub
    ///
    ///         Private Sub Capture()
    ///
    ///             Debug.Print("--------Polling the Properties-----------")
    ///
    ///             While True
    ///                 _rht03.RequestMeasurement()
    ///
    ///                 Debug.Print("Humidity - " <![CDATA[&]]> _rht03.Humidity.ToString("f2") <![CDATA[&]]> " % RH")
    ///                 Debug.Print("Temperature - " <![CDATA[&]]> _rht03.Temperature.ToString("f2") <![CDATA[&]]> (If(_rht03.TemperatureUnit = TemperatureUnits.Celsius, "°C", If(_rht03.TemperatureUnit = TemperatureUnits.Fahrenheit, " °F", "°K"))))
    ///                 Thread.Sleep(1000)
    ///             End While
    ///         End Sub
    ///
    ///         Private Sub _rht03_SensorError(sender As Object, errorMessage As String) Handles _rht03.SensorError
    ///             Debug.Print(errorMessage)
    ///         End Sub
    ///
    ///         Private Sub _rht03_TemperatureHumidityMeasured(sender As Object, temperature As Single, humidity As Single) Handles _rht03.TemperatureHumidityMeasured
    ///             Debug.Print("---------Event Driven----------")
    ///             Debug.Print("Temperature - " <![CDATA[&]]> temperature.ToString("f2") <![CDATA[&]]> (If(_rht03.TemperatureUnit = TemperatureUnits.Celsius, "°C", If(_rht03.TemperatureUnit = TemperatureUnits.Fahrenheit, " °F", "°K"))))
    ///             Debug.Print("Humidity - " + humidity.ToString("f2") + " % RH")
    ///         End Sub
    ///     End Module
    ///
    /// End Namespace
    /// </code>
    /// </example>
// ReSharper disable once InconsistentNaming
    public class RHT03 : IDriver, ITemperature, IHumidity
    {

        #region Fields

        private static TimeSpan _samplingFrequency = new TimeSpan(0, 0, 0, 15);
        private static float _temperature;
        private static float _humidity;
        private static TemperatureUnits _tempUnit = TemperatureUnits.Celsius;


        private static InterruptPort _portIn;
        private static TristatePort _portOut;

        private static Timer _pollingTimer;
        private bool _isPolling;
        private static bool _sensorInitialized;

        private const long BitThreshold = 1150;
        private static readonly byte[] DataBytes = new byte[4];
        private readonly AutoResetEvent _dataReceived = new AutoResetEvent(false);

        private static long _bitMask;
        private static long _sensorData;
        private static long _lastTicks;


        #endregion

        #region CTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="RHT03"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="Hardware.Socket"/> that the RHT03 Sensor is connected to.</param>
        public RHT03(Hardware.Socket socket)
        {
            try
            {
                Hardware.CheckPins(socket, socket.Rst, socket.Int);

                _portIn = new InterruptPort(socket.Rst, false, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeHigh);
                _portIn.OnInterrupt += portIn_OnInterrupt;
                _portIn.DisableInterrupt(); // Enabled automatically in the previous call

                _portOut = new TristatePort(socket.Int, false, false, Port.ResistorMode.PullUp);
            }
            catch (PinInUseException ex)
            {

                throw new PinInUseException(ex.Message);
            }

            _pollingTimer = new Timer(UpdateReadings, null, Timeout.Infinite, Timeout.Infinite);

            InitSensor();

            if (SensorError != null) SensorError(this, "RHT03 Sensor Initialization is complete.");

        } 

        #endregion

        #region Internal Interrupts

        private void portIn_OnInterrupt(uint pin, uint state, DateTime time)
        {
            long ticks = time.Ticks;
            if ((ticks - _lastTicks) > BitThreshold)
            {
                // If the time between edges exceeds threshold, it is bit '1'
                _sensorData |= _bitMask;
            }
            if ((_bitMask >>= 1) == 0)
            {
                // Received the last edge, stop and signal completion
                _portIn.DisableInterrupt();
                _dataReceived.Set();
            }
            _lastTicks = ticks;
        }

        #endregion

        #region Private Methods

        private void InitSensor()
        {
            try
            {
                ExecutionConstraint.Install(2000, 0); // It should not take more than 1 second (using 2 to be safe) for the sensor to stabilize per datasheet.
                while (_sensorInitialized == false) // Keep reading until the Read() method returns true or and we get a ConstraintException
                {
                    if (Read(false)) _sensorInitialized = true;
                }
            }
            catch (ConstraintException)
            {
                throw new DeviceInitialisationException("RHT03 Sensor failed to initialize.");
            }
            finally
            {
                ExecutionConstraint.Install(-1, 0);
            }
        }

        private bool Read(bool raiseEvent = true)
        {
            // The 'bitMask' also serves as edge counter: data bit edges plus extra ones at the beginning of the communication (presence pulse).
            _bitMask = 1L << 42;
            _sensorData = 0;
            bool dataValid = false;

            // Initiate communication
            if (_portOut.Active == false) _portOut.Active = true;
            _portOut.Write(false); // Pull pin low
            Thread.Sleep(5); // At lest 1 mSec.
            _portIn.EnableInterrupt(); // Turn on the receiver
            if (_portOut.Active) _portOut.Active = false;

            // Now the interrupt handler is getting called on each falling edge.
            // The communication takes up to 5 ms, but the interrupt handler managed
            // code takes longer to execute than is the duration of sensor pulse
            // (interrupts are queued), so we must wait for the last one to finish
            // and signal completion. 20 ms should be enough, 50 ms is safe.
            // Set to 50 to minimize checksum and timeout errors. The higher the value
            // the less timeout errors, consequently longer conversion time. 
            if (_dataReceived.WaitOne(200, true))
            {
                DataBytes[0] = (byte)((_sensorData >> 32) & 0xFF);
                DataBytes[1] = (byte)((_sensorData >> 24) & 0xFF);
                DataBytes[2] = (byte)((_sensorData >> 16) & 0xFF);
                DataBytes[3] = (byte)((_sensorData >> 8) & 0xFF);

                var checksum = (byte)(DataBytes[0] + DataBytes[1] + DataBytes[2] + DataBytes[3]);
                if (checksum == (byte)(_sensorData & 0xFF))
                {
                    dataValid = true;
                    Convert(DataBytes, raiseEvent);
                }
                else
                {
                    if (SensorError != null) SensorError(this, "RHT sensor data has invalid checksum.");
                    _temperature = float.MinValue;
                    _humidity = float.MinValue;
                }
            }
            else
            {
                //_temperature = float.MinValue;
                //_humidity = float.MinValue;

                _portIn.DisableInterrupt(); // Stop receiver
                if (SensorError != null) SensorError(this, "RHT sensor data timeout.");
                return false;
            }
            return dataValid;
        }

        private void Convert(byte[] data, bool raiseEvent = true)
        {
            // The first byte is integral, the second byte is decimal part
            _humidity = ((data[0] << 8) | data[1]) * 0.1F;

            float temp = (((data[2] & 0x7F) << 8) | data[3]) * 0.1F;
            _temperature = (data[2] & 0x80) == 0 ? temp : -temp; // MSB = 1 means negative
            if (raiseEvent && TemperatureHumidityMeasured != null) TemperatureHumidityMeasured(this, Utilities.Temperature.ConvertTo(TemperatureUnits.Celsius, _tempUnit, _temperature), _humidity);
        }

        private void UpdateReadings(object state)
        {
            Read();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads the temperature.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="NotImplementedException">Not supported by this sensor.</exception>
        /// <returns>A single representing the temperature read from the source, degrees Celsius</returns>
        /// <example>None: Not supported.</example>
        public float ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            throw new NotImplementedException("This module does not support direct measurement of Temperature.");
        }

        /// <summary>
        /// Reads the relative or absolute humidity value from the sensor.
        /// </summary>
        /// <exception cref="NotImplementedException">Not supported by this sensor.</exception>
        /// <returns>A single representing the relative/absolute humidity as read from the sensor, in percentage (%) for relative reading or value in case of absolute reading.</returns>
        /// <example>None: Not supported.</example>
        public float ReadHumidity(HumidityMeasurementModes measurementMode = HumidityMeasurementModes.Relative)
        {
            throw new NotImplementedException("This module does not support direct measurement of Humidity.");
        }
       
        /// <summary>
        ///  Requests a single sensor data read from the RHT03 Click in the non-event driven mode only.
        /// </summary>
        /// <remarks>
        ///   In order to use this method, either do not start continuous measurements with the <see cref="StartContinuousMeasurements"/> method or stop continuous measurements by using the <see cref="StopContinuousMeasurements"/> method.
        /// <para><b>If you start receiving a number of sensor error messages from the <see cref="SensorError"/> event, try increasing the duration between calls to this method until the error messages are minimized to acceptable levels. Suggested minimum frequency is 500 milliseconds between calls.</b></para>
        /// </remarks>
        /// <example>Example usage:
        /// <code language = "C#">
        ///  while (true)
        ///  {
        ///       _rht03.RequestMeasurement();
        ///
        ///       Debug.Print("Humidity - " + _rht03.Humidity.ToString("f2") + " % RH");
        ///       Debug.Print("Temperature - " + _rht03.Temperature.ToString("f2") + " °C");
        ///       Thread.Sleep(1000);
        ///  }
        /// </code>
        /// <code language = "VB">
        /// While True
	    ///     _rht03.RequestMeasurement()
        ///
        ///	    Debug.Print("Humidity - " <![CDATA[&]]> _rht03.Humidity.ToString("f2") <![CDATA[&]]> " % RH")
	    ///     Debug.Print("Temperature - " <![CDATA[&]]> _rht03.Temperature.ToString("f2") <![CDATA[&]]> " °C")
	    ///    Thread.Sleep(1000)
        /// End While
        /// </code>
        /// </example>
        public void RequestMeasurement()
        {
            if (!_isPolling) Read(false);
        }

        /// <summary>
        ///     Resets the RHT03.
        /// </summary>
        /// <param name="resetMode">The reset mode, see <see cref="ResetModes"/> for more information.</param>
        /// <remarks>
        /// This module has no Reset method, calling this method will throw an exception.
        /// </remarks>
        /// <exception cref="NotImplementedException">This module does not implement a reset method. Calling this method will throw a <see cref="NotImplementedException"/></exception>
        /// <example>None: This module does not support a Reset method.</example>
        public bool Reset(ResetModes resetMode)
        {
            throw new NotImplementedException("This module does not implement a Reset method.");
        }

        /// <summary>
        /// Starts continuous polling of the RHT03 Click based on the time frequency set in the <see cref="SamplingFrequency"/> Property.
        /// If the SamplingFrequency Property is not set, it will default to 15 seconds.
        /// </summary>
        /// <remarks>SensorData will be returned by raising the OnMeasurement Event.
        /// <para><b>If you start receiving a number of sensor error messages from the <see cref="SensorError"/> event, try increasing the <see cref="SamplingFrequency"/>  property until the error messages are minimized to acceptable levels. Suggested minimum frequency is 500 milliseconds between readings.</b></para>
        /// </remarks>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _rht03.StartContinuousMeasurements();
        /// </code>
        /// <code language = "VB">
        /// _rht03.StartContinuousMeasurements()
        /// </code>
        /// </example>
        public void StartContinuousMeasurements()
        {
            while (_sensorInitialized != true)
            {
                Thread.Sleep(10); // Waiting for the RHT03 to finish initialization.
            }
            if (!_sensorInitialized) return;
            _pollingTimer.Change(TimeSpan.Zero, _samplingFrequency == TimeSpan.Zero ? new TimeSpan(0, 0, 0, 15) : _samplingFrequency); //Default is every 15 seconds.
            _isPolling = true;

        } 
        
        /// <summary>
        ///     Stops continuous polling of the RHT03 for sensor data.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _rht03.StopContinuousMeasurements();
        /// </code>
        /// <code language = "VB">
        /// _rht03.StopContinuousMeasurements()
        /// </code>
        /// </example>
        public void StopContinuousMeasurements()
        {
            _pollingTimer.Change(TimeSpan.Zero, TimeSpan.MaxValue);
            _isPolling = false;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Sets the TimeSpan interval in which the RHT03 click will be polled to return sensor data.
        /// </summary>
        /// <remarks>In order to avoid numerous <see cref="SensorError"/>s the Minimum Sampling Frequency is 250 milliseconds. Default is 15 seconds.</remarks>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _rht03.SamplingFrequency = new TimeSpan(0, 0, 0, 2);
        /// </code>
        /// <code language = "VB">
        /// _rht03.SamplingFrequency = new TimeSpan(0, 0, 0, 2)
        /// </code>
        /// </example>
        public TimeSpan SamplingFrequency
        {
            get { return _samplingFrequency; }
            set
            {
                if (value < new TimeSpan(2500000)) value = new TimeSpan(2500000); // Do not sample faster than the conversion time plus a few milliseconds.
                _samplingFrequency = value;
            }
        }

        /// <summary>
        /// Gets the temperature in °C.
        /// </summary> 
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("Temperature - " + _rht03.Temperature.ToString("f2") + " °C");
        /// </code>
        /// <code language = "VB">
	    /// Debug.Print("Temperature - " <![CDATA[&]]> _rht03.Temperature.ToString("f2") <![CDATA[&]]> " °C")
        /// </code>
        /// </example>
        public double Temperature
        {
            get { return Utilities.Temperature.ConvertTo(TemperatureUnits.Celsius, _tempUnit, _temperature); }
        }

        /// <summary>
        /// Gets or sets the temperature unit for the <seealso cref="ReadTemperature"/> method.
        /// <remarks><seealso cref="TemperatureUnits"/></remarks>
        /// </summary>
        /// <value>
        /// The temperature unit used.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///     // Set temperature unit to Fahrenheit
        ///     _rht03.TemperatureUnit = TemperatureUnits.Farhenheit;
        /// </code>
        /// <code language="VB">
        ///     ' Set temperature unit to Fahrenheit
        ///     _rht03.TemperatureUnit = TemperatureUnits.Farhenheit
        /// </code>
        /// </example>
        public TemperatureUnits TemperatureUnit
        {
            get { return _tempUnit; }
            set { _tempUnit = value; }
        }

        /// <summary>
        /// Returns the humidity as %RH.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("Humidity - " + _rht03.Humidity.ToString("f2") + " % RH");
        /// </code>
        /// <code language = "VB">
        ///	Debug.Print("Humidity - " <![CDATA[&]]> _rht03.Humidity.ToString("f2") <![CDATA[&]]> " % RH")
        /// </code>
        /// </example>
        public double Humidity
        {
            get { return _humidity; }
        }

        /// <summary>
        ///     Gets the raw data of the Temperature or Humidity value.
        /// </summary>
        /// <value>
        ///     Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <remarks>Not Implemented.</remarks>
        /// <exception cref="NotImplementedException">The Get Accessor of this property will throw a NotImplementdException.</exception>
        public int RawData
        {
            get { throw new NotImplementedException("RawData not implemented for this sensor"); }
        }

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <returns cref="NotImplementedException">Calling this method will throw a <see cref="NotImplementedException"/>.</returns>
        /// <remarks>
        /// This module does not use Power Modes, the GET accessor will always return PowerModes.On. See <see cref="PowerModes"/>, while the SET accessor will throw a <see cref="NotImplementedException"/>.
        /// </remarks>
        /// <exception cref="NotImplementedException">This sensor does not support PowerMode.</exception>
        /// <example>None: This sensor does not support PowerMode.</example>
        public PowerModes PowerMode
        {
            get { return PowerModes.On; }
            set { throw new NotImplementedException("Power Mode not implemented for this sensor"); }
        }

        /// <summary>
        ///     Gets the driver version.
        /// </summary>
        /// <value>
        ///     The driver version see <see cref="Version"/>.
        /// </value>
        /// <example>Example usage to get the Driver Version in formation:
        /// <code language="C#">
        /// Debug.Print("Driver Version Info : " + _clock1.DriverVersion);
        /// </code>
        /// <code language="VB">
        /// Debug.Print("Driver Version Info : " <![CDATA[&]]> _clock1.DriverVersion)
        /// </code>
        /// </example>
        public Version DriverVersion
        {
            get { return Assembly.GetAssembly(GetType()).GetName().Version; }
        }

        #endregion

        #region Events  
    
        /// <summary>
        ///     This event is raised when the RHT03 Sensor completes a measurement in continuous measurement mode.
        /// </summary>
        public event TemperatureHumidityMeasurementEventHandler TemperatureHumidityMeasured;

        /// <summary>
        ///     Represents the delegate that is used for the <see cref="TemperatureHumidityMeasured" />.
        /// </summary>
        /// <param name="sender">The RHT03 Sensor that raised </param>
        /// <param name="temperature">The temperature returned by the RHT03 Sensor.</param>
        /// <param name="humidity">The humidity returned by the RHT03 Sensor.</param>
        public delegate void TemperatureHumidityMeasurementEventHandler(object sender, float temperature, float humidity);

        /// <summary>
        ///     This event is raised when the RHT03 Sensor has a error or when it needs to send information.
        /// </summary>
        public event SensorErrorEventHandler SensorError;

        /// <summary>
        ///     Represents the delegate that is used for the <see cref="SensorError" />.
        /// </summary>
        /// <param name="sender">The RHT03 Sensor that raised the event.</param>
        /// <param name="errorMessage">The error returned by the RHT03 Sensor.</param>
        public delegate void SensorErrorEventHandler(object sender, string errorMessage);

        #endregion

    }
}
