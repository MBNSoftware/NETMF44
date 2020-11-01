using System;

// ReSharper disable once CheckNamespace
//namespace MikroBusNet.Interfaces
//{
//    /// <summary>
//    ///     Power modes that may be applicable to a module
//    /// </summary>
//    public enum PowerModes : byte
//    {
//        /// <summary>
//        ///      Module is turned off, meaning it generally can't perform measurements.
//        /// </summary>
//        Off,
//        /// <summary>
//        ///     Module is either in hibernate mode or low power mode (depending on the module)
//        /// </summary>
//        Low,
//        /// <summary>
//        ///     Module is turned on, at full power, meaning it is fully functional.
//        /// </summary>
//        On
//    }

//    /// <summary>
//    ///     Reset modes that may be applicable to a module
//    /// </summary>
//    public enum ResetModes : byte
//    {
//        /// <summary>
//        ///     Soft reset of the module
//        /// </summary>
//        Soft,
//        /// <summary>
//        ///     Hard Reset of the module. Usually dback to Factgory defaults for the IC.
//        /// </summary>
//        Hard
//    }

//    /// <summary>
//    ///     Main Driver Interface
//    /// </summary>
//    public interface IDriver
//    {
//        /// <summary>
//        ///     Gets the version number of the driver assembly.
//        /// </summary>
//        /// <code>
//        /// Assembly assem = Assembly.GetAssembly(GetType());
//        /// AssemblyName assemName = assem.GetName();
//        /// Version ver = assemName.Version;
//        /// return ver;
//        /// </code>
//        Version Version { get; }

//        /// <summary>
//        ///     Gets or sets the power mode.
//        /// </summary>
//        /// <value>
//        /// The current power mode of the module.
//        /// </value>
//        /// <remarks>If the module has no power modes, then GET should always return PowerModes.ON while SET should throw a NotImplementedException.</remarks>
//        PowerModes PowerMode { get; set; }

//        /// <summary>
//        /// Resets the module
//        /// </summary>
//        /// <param name="resetMode">The reset mode : 
//        /// <para>SOFT reset : generally by sending a software command to the chip</para>
//        /// <para>HARD reset : generally by activating a special chip's pin</para>
//        /// </param>
//        void Reset(ResetModes resetMode);

//    }
//}
