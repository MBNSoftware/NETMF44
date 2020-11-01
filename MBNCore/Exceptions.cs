/*
 * MikroBus.Net main assembly
 * 
 * Custom exceptions class
 * 
 * Version 2.0 :
 *  - Changed namespaces
 *  - Added UnknownSocketException
 *  
 * Version 1.0 :
 *  - Initial revision coded by Christophe Gerbier
 * 
 * References needed :
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Native
 *  mscorlib
 *  
 * Copyright 2014 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */
using System;

namespace MBN.Exceptions
{
    /// <summary>
    /// Exception thrown when a check in driver's constructor find a pin that is already in use by another driver.
    /// </summary>
    [Serializable]
    public class PinInUseException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PinInUseException"/> class.
        /// </summary>
        public PinInUseException() {}
        /// <summary>
        /// Initializes a new instance of the <see cref="PinInUseException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public PinInUseException(string message) : base(message) {}
        /// <summary>
        /// Initializes a new instance of the <see cref="PinInUseException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public PinInUseException(string message, Exception innerException) : base(message, innerException) {}

        /// <summary>
        /// Gets the <see cref="T:System.Exception" /> instance that caused the current exception.
        /// </summary>
        /// <returns>An instance of Exception that describes the error that caused the current exception. The InnerException property returns the same value as was passed into the constructor, or a null reference (Nothing in Visual Basic) if the inner exception value was not supplied to the constructor. This property is read-only.</returns>
        public new Exception InnerException { get { return base.InnerException; } }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        /// <PermissionSet><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" /></PermissionSet>
        public override string ToString() { return "PinInUseException : "+base.Message; }
    }

    /// <summary>
    /// Exception thrown when a new instance of a driver can't be created. It may be because of too short delays or bad commands sent to the device.
    /// </summary>
    [Serializable]
    public class DeviceInitialisationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceInitialisationException"/> class.
        /// </summary>
        public DeviceInitialisationException() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceInitialisationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DeviceInitialisationException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceInitialisationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public DeviceInitialisationException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Gets the <see cref="T:System.Exception" /> instance that caused the current exception.
        /// </summary>
        /// <returns>An instance of Exception that describes the error that caused the current exception. The InnerException property returns the same value as was passed into the constructor, or a null reference (Nothing in Visual Basic) if the inner exception value was not supplied to the constructor. This property is read-only.</returns>
        public new Exception InnerException { get { return base.InnerException; } }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        /// <PermissionSet><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" /></PermissionSet>
        public override string ToString() { return "DeviceInitialisationException : " + base.Message; }
    }

    /// <summary>
    /// Exception thrown when a socket does not exist on a board but was selected by user (i.e. SocketThree on Dalmatian)
    /// </summary>
    [Serializable]
    public class SocketUseException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SocketUseException"/> class.
        /// </summary>
        public SocketUseException() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="SocketUseException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SocketUseException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="SocketUseException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public SocketUseException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Gets the <see cref="T:System.Exception" /> instance that caused the current exception.
        /// </summary>
        /// <returns>An instance of Exception that describes the error that caused the current exception. The InnerException property returns the same value as was passed into the constructor, or a null reference (Nothing in Visual Basic) if the inner exception value was not supplied to the constructor. This property is read-only.</returns>
        public new Exception InnerException { get { return base.InnerException; } }
 
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        /// <PermissionSet><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" /></PermissionSet>
        public override string ToString() { return "SocketUseException : " + base.Message; }
    }
}