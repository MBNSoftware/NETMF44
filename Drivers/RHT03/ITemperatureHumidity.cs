using MBN.Interfaces;
using MikroBusNet.Interfaces;

// ReSharper disable once CheckNamespace

namespace MikroBusNet.Click.RHT03Click
{
    /// <summary>
    /// </summary>
    public interface ITemperatureHumidity : IDriver, ISensor
    {
        /// <summary>
        ///     Raised when a TemperatureHumidity measurement is complete.
        /// </summary>
        event TemperatureHumidityMeasurementEventHandler TemperatureHumidityMeasured;
    }

    /// <summary>
    ///     The event delegate method that is used by the TemperatureHumidityMeasured event..
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="temperature"></param>
    /// <param name="humidity"></param>
    public delegate void TemperatureHumidityMeasurementEventHandler(object sender, double temperature, double humidity);
}