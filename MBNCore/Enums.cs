namespace MBN.Enums
{
    /// <summary>
    /// Known boards
    /// </summary>
    public enum BoardTypes
    {
        /// <summary>
        /// Unknown board. Maybe prototype or defective board.
        /// </summary>
        Unknown,
        /// <summary>
        /// The Dalmatian board, with 2 sockets
        /// </summary>
        Dalmatian,
        /// <summary>
        /// The Tuatara board, with 3 sockets
        /// </summary>
        Tuatara,
        /// <summary>
        /// The Quail board, with 4 sockets
        /// </summary>
        QuailV2,
        /// <summary>
        /// The Quail board, with 4 sockets and Prime firmware 4.3.1
        /// </summary>
        QuailV22,
        /// <summary>
        /// The F429 Discovery carrier board, with 4 sockets and Prime firmware 4.3.1
        /// </summary>
        F429Carrier
    };

    /// <summary>
    /// Power modes that may be applicable to a module
    /// </summary>
    public enum PowerModes : byte
    {
        /// <summary>
        /// Module is turned off, meaning it generally can't perform measures or operate
        /// </summary>
        Off,
        /// <summary>
        /// Module is either in hibernate mode or low power mode (depending on the module)
        /// </summary>
        Low,
        /// <summary>
        /// Module is turned on, at full power, meaning it is fully functionnal
        /// </summary>
        On
    }

    /// <summary>
    /// Reset modes that may be applicable to a module
    /// </summary>
    public enum ResetModes : byte
    {
        /// <summary>
        /// Software reset, which usually consists in a command sent to the device.
        /// </summary>
        Soft,
        /// <summary>
        /// Hardware reset, which usually consists in toggling a IO pin connected to the device.
        /// </summary>
        Hard
    }

    /// <summary>
    /// Units used by the ITemperature interface
    /// </summary>
    public enum TemperatureUnits
    {
        /// <summary>
        /// Celsius unit
        /// </summary>
        Celsius,
        /// <summary>
        /// Fahrenheit unit
        /// </summary>
        Fahrenheit,
        /// <summary>
        /// Kelvin unit
        /// </summary>
        Kelvin
    }

    /// <summary>
    /// Temperature sources used by the ITemperature interface.
    /// </summary>
    public enum TemperatureSources
    {
        /// <summary>
        /// Measures the ambient (room) temperature.
        /// </summary>
        Ambient,
        /// <summary>
        /// Measures an object temperature, either via external sensor or IR sensor, for example.
        /// </summary>
        Object
    }

    /// <summary>
    /// Measurement modes used by the IHumidity interface.
    /// </summary>
    public enum HumidityMeasurementModes
    {
        /// <summary>
        /// Relative humidity measurement mode
        /// </summary>
        Relative,
        /// <summary>
        /// Absolute humidity measurement mode
        /// </summary>
        Absolute
    }

    /// <summary>
    /// Standard I²C clock rates
    /// </summary>
    public enum ClockRatesI2C
    {
        /// <summary>
        /// 100 KHz clock rate
        /// </summary>
        Clock100KHz = 100,
        /// <summary>
        /// 400 KHz clock rate
        /// </summary>
        Clock400KHz = 400,
    }

    /// <summary>
    /// Compensation modes for pressure sensors
    /// </summary>
    public enum PressureCompensationModes
    {
        /// <summary>
        /// Sea level compensated
        /// </summary>
        SeaLevelCompensated,
        /// <summary>
        /// Raw uncompensated
        /// </summary>
        Uncompensated
    }

}