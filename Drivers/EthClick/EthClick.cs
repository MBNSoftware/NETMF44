/*
 * Eth Click board driver for MikroBus.Net
 * 
 * Version 1.0 :
 *  - Initial version coded by Stephen Cardinale
 *  
 *  - Based in part on mIP - the Managed TCP/IP Stack.
 *		Hosted on CodePlex: http://mip.codeplex.com
 *		mIP is free software licensed under the Apache License 2.0
 *		© Copyright 2012 ValkyrieTech, LLC
 * 
 * References needed :
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Native
 *  MikroBusNet
 *  mscorlib
 *  
 * Copyright 2014 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using MBN.Enums;
using MBN.Exceptions;
using MBN.Extensions;
using Microsoft.SPOT.Hardware;
using Debug = System.Diagnostics.Debug;

namespace MBN.Modules
{
	/// <summary>
	/// A MikroBusNet Driver for the MikroE Eth Click.
	/// <para><b>This module is an SPI Device</b></para>
	/// <para><b>Pins used :</b> Miso, Mosi, Sck, Cs, Rst and Int</para>
	/// <para><b>References required:</b> MikroBus.Net, Microsoft.SPOT.Native, Microsoft.Spot.Hardware, mscorlib</para>
	/// </summary>
	/// <example>Example usage:
	/// <code language = "C#">
	///using MBN;
	///using MBN.Modules;
	///using Microsoft.SPOT;
	///
	///namespace NetworkingExample
	///{
	///	public class DNSLookupProgram
	///	{
	///		private static EthClick _eth;
	///
	///		public static void Main()
	///		{
	///			_eth = new EthClick(Hardware.SocketOne);
	///
	/// 		/* Use this for Static IP and No DHCP */
	///			//_eth.Name = "Quail";
	///			//_eth.MacAddress = _eth.GenerateUniqueMacAddress("Quail");
	///			//_eth.IPAddress = "192.168.1.95".ToBytes();
	///			//_eth.DefaultGateway = "192.168.1.1".ToBytes();
	///			//_eth.PreferredDomainNameServer = "8.8.8.8".ToBytes(); // Google DNS Servers
	///			//_eth.AlternateDomainNameServer = "8.8.8.4".ToBytes(); // Google DNS Servers
	///			//_eth.DHCPDisabled = true;
	///
	///			_eth.Start(_eth.GenerateUniqueMacAddress("MikroBusNet"), "MikroBusNet");
	///
	///			while (true)
	///			{
	///				if (_eth.ConnectedToInternet)
	///				{
	///					Debug.Print("Connected to Internet");
	///					break;
	///	 			}
	///	 			Debug.Print("Waiting on Internet connection");
	///	 		}
	/// 
	///	 		var addressBytes = DNS.Lookup("www.google.com");
	///	 		Debug.Print("DNS Lookup: www.google.com -> " + addressBytes.ToAddress());
	///	 		addressBytes = DNS.Lookup("www.mikrobusnet.org");
	///	 		Debug.Print("DNS Lookup: www.mikrobusnet.org -> " + addressBytes.ToAddress());
	///	 	}
	///	  }
	/// }
	/// </code>
	/// <code language = "VB">
	/// Option Explicit On
	/// Option Strict On
	///
	/// Imports MBN
	/// Imports Microsoft.SPOT
	/// Imports MBN.Modules
	///
	/// Namespace NetworkingExample
	///
	///	Public Module DNSLookupProgram
	///
	///		Dim _eth As EthClick
	/// 
	///		Sub Main()
	///
	///			_eth = New EthClick(Hardware.SocketOne)
	///
	///			' Use this for Static IP and No DHCP
	///			'_eth.Name = "Quail";
	///			'_eth.MacAddress = _eth.GenerateUniqueMacAddress("Quail")
	///			'_eth.IPAddress = "192.168.1.95".ToBytes()
	///			'_eth.DefaultGateway = "192.168.1.1".ToBytes()
	///			'_eth.PreferredDomainNameServer = "8.8.8.8".ToBytes() ' Google DNS Servers
	///			'_eth.AlternateDomainNameServer = "8.8.8.4".ToBytes() ' Google DNS Servers
	///			'_eth.DHCPDisabled = True
	///
	///			_eth.Start(_eth.GenerateUniqueMacAddress("MikroBusNet"), "MikroBusNet")
	///
	///			While (True)
	///
	///				If (_eth.ConnectedToInternet) Then
	///					Debug.Print("Connected to Internet")
	///					Exit While
	///				End If
	///				Debug.Print("Waiting on Internet connection")
	///			End While
	///
	///			Dim addressBytes = DNS.Lookup("www.google.com")
	///			Debug.Print("DNS Lookup: www.google.com -> " <![CDATA[&]]> addressBytes.ToAddress())
	///			addressBytes = DNS.Lookup("www.mikrobusnet.org")
	///			Debug.Print("DNS Lookup: www.mikrobusnet.org -> " <![CDATA[&]]> addressBytes.ToAddress())
	///
	///		End Sub
	///
	///	End Module
	///
	/// End Namespace
	/// </code>
	/// </example>
	public class EthClick : IDriver
	{

		#region Constants

		private const int CheckDelay = 5000;

		private const ushort RxStart = 0x0000;
		private const ushort RxStop = 0x19FF;
		private const ushort TxStart = 0x1AFF;
		private const ushort TxStop = 0x1FFF;

		private const byte WriteControlRegister = (0x02 << 5);
		private const byte BitFieldSet = (0x04 << 5);
		private const byte BitFieldClear = (0x05 << 5);
		private const byte ReadControlRegister = (0x00 << 5);
		private const byte ReadBufferMemory = ((0x01 << 5) | 0x1A);
		private const byte WriteBufferMemnory = ((0x03 << 5) | 0x1A);
		//private const byte SystemReset = ((0x07 << 5) | 0x1F); // It requires 0x1F, however.

		/******************************************************************************
		* Register locations
		******************************************************************************/
		// Bank 0 registers --------
		// ReSharper disable InconsistentNaming
		private const ushort ERDPTL = 0x00;
		private const ushort ERDPTH = 0x01;
		private const ushort EWRPTL = 0x02;
		private const ushort EWRPTH = 0x03;
		private const ushort ETXSTL = 0x04;
		private const ushort ETXSTH = 0x05;
		private const ushort ETXNDL = 0x06;
		private const ushort ETXNDH = 0x07;
		private const ushort ERXSTL = 0x08;
		private const ushort ERXSTH = 0x09;
		private const ushort ERXNDL = 0x0A;
		private const ushort ERXNDH = 0x0B;
		private const ushort ERXRDPTL = 0x0C;
		private const ushort ERXRDPTH = 0x0D;
		private const ushort ERXWRPTL = 0x0E;
		private const ushort ERXWRPTH = 0x0F;
		//private const ushort EDMASTL = 0x10;
		//private const ushort EDMASTH = 0x11;
		//private const ushort EDMANDL = 0x12;
		//private const ushort EDMANDH = 0x13;
		//private const ushort EDMADSTL = 0x14;
		//private const ushort EDMADSTH = 0x15;
		//private const ushort EDMACSL = 0x16;
		//private const ushort EDMACSH = 0x17;
		private const byte EIE = 0x1B;
		private const byte EIR = 0x1C;
		private const byte ESTAT = 0x1D;
		private const byte ECON2 = 0x1E;
		private const byte ECON1 = 0x1F;

		// Bank 1 registers -----
		//private const ushort EHT0 = 0x100;
		//private const ushort EHT1 = 0x101;
		//private const ushort EHT2 = 0x102;
		//private const ushort EHT3 = 0x103;
		//private const ushort EHT4 = 0x104;
		//private const ushort EHT5 = 0x105;
		//private const ushort EHT6 = 0x106;
		//private const ushort EHT7 = 0x107;
		private const ushort EPMM0 = 0x108;
		private const ushort EPMM1 = 0x109;
		private const ushort EPMM2 = 0x10A;
		private const ushort EPMM3 = 0x10B;
		private const ushort EPMM4 = 0x10C;
		private const ushort EPMM5 = 0x10D;
		private const ushort EPMM6 = 0x10E;
		private const ushort EPMM7 = 0x10F;
		private const ushort EPMCSL = 0x110;
		private const ushort EPMCSH = 0x111;
		private const ushort EPMOL = 0x114;
		private const ushort EPMOH = 0x115;
		private const ushort ERXFCON = 0x118;
		private const ushort EPKTCNT = 0x119;

		// Bank 2 registers -----
		private const ushort MACON1 = 0x200;
		private const ushort MACON2 = 0x201;
		private const ushort MACON3 = 0x202;
		private const ushort MACON4 = 0x203;
		private const ushort MABBIPG = 0x204;
		private const ushort MAIPGL = 0x206;
		private const ushort MAIPGH = 0x207;
		//private const ushort MACLCON1 = 0x208;
		private const ushort MACLCON2 = 0x209;
		private const ushort MAMXFLL = 0x20A;
		private const ushort MAMXFLH = 0x20B;
		private const ushort MICMD = 0x212;
		private const ushort MIREGADR = 0x214;
		private const ushort MIWRL = 0x216;
		private const ushort MIWRH = 0x217;
		private const ushort MIRDL = 0x218;
		private const ushort MIRDH = 0x219;

		// Bank 3 registers -----
		private const ushort MAADR5 = 0x300;
		private const ushort MAADR6 = 0x301;
		private const ushort MAADR3 = 0x302;
		private const ushort MAADR4 = 0x303;
		private const ushort MAADR1 = 0x304;
		private const ushort MAADR2 = 0x305;
		//private const ushort EBSTSD = 0x306;
		//private const ushort EBSTCON = 0x307;
		//private const ushort EBSTCSL = 0x308;
		//private const ushort EBSTCSH = 0x309;
		private const ushort MISTAT = 0x30A;
		private const ushort EREVID = 0x312;
		private const ushort ECOCON = 0x315;
		//private const ushort EFLOCON = 0x317;
		//private const ushort EPAUSL = 0x318;
		//private const ushort EPAUSH = 0x319;

		/******************************************************************************
		* PH Register Locations
		******************************************************************************/
		private const byte PHCON1 = 0x00;
		//private const byte PHSTAT1 = 0x01;
		//private const byte PHID1 = 0x02;
		//private const byte PHID2 = 0x03;
		private const byte PHCON2 = 0x10;
		private const byte PHSTAT2 = 0x11;
		private const byte PHIE = 0x12;
		private const byte PHIR = 0x13;
		//private const byte PHLCON = 0x14;


		/******************************************************************************
		* Individual Register Bits
		******************************************************************************/
		// ETH/MAC/MII bits

		// EIE bits ----------
		private const byte EIE_INTIE = (1 << 7);
		private const byte EIE_PKTIE = (1 << 6);
		//private const byte EIE_DMAIE = (1 << 5);
		private const byte EIE_LINKIE = (1 << 4);
		//private const byte EIE_TXIE = (1 << 3);
		private const byte EIE_TXERIE = (1 << 1);
		private const byte EIE_RXERIE = (1);

		// EIR bits ----------
		private const byte EIR_PKTIF = (1 << 6);
		//private const byte EIR_DMAIF = (1 << 5);
		private const byte EIR_LINKIF = (1 << 4);
		//private const byte EIR_TXIF = (1 << 3);
		private const byte EIR_TXERIF = (1 << 1);
		private const byte EIR_RXERIF = (1);

		// ESTAT bits ---------
		//private const byte ESTAT_INT = (1 << 7);
		//private const byte ESTAT_BUFER = (1 << 6);
		//private const byte ESTAT_LATECOL = (1 << 4);
		private const byte ESTAT_RXBUSY = (1 << 2);
		//private const byte ESTAT_TXABRT = (1 << 1);
		private const byte ESTAT_CLKRDY = (1);

		// ECON2 bits --------
		private const byte ECON2_AUTOINC = (1 << 7);
		private const byte ECON2_PKTDEC = (1 << 6);
		private const byte ECON2_PWRSV = (1 << 5);
		//private const byte ECON2_VRPS = (1 << 3);

		// ECON1 bits --------
		//private const byte ECON1_TXRST = (1 << 7);
		private const byte ECON1_RXRST = (1 << 6);
		//private const byte ECON1_DMAST = (1 << 5);
		//private const byte ECON1_CSUMEN = (1 << 4);
		private const byte ECON1_TXRTS = (1 << 3);
		private const byte ECON1_RXEN = (1 << 2);
		private const byte ECON1_BSEL1 = (1 << 1);
		private const byte ECON1_BSEL0 = (1);

		// ERXFCON bits ------
		private const byte ERXFCON_UCEN = (1 << 7);
		//private const byte ERXFCON_ANDOR = (1 << 6);
		private const byte ERXFCON_CRCEN = (1 << 5);
		private const byte ERXFCON_PMEN = (1 << 4);
		//private const byte ERXFCON_MPEN = (1 << 3);
		//private const byte ERXFCON_HTEN = (1 << 2);
		//private const byte ERXFCON_MCEN = (1 << 1);
		private const byte ERXFCON_BCEN = (1);

		// MACON1 bits --------
		private const byte MACON1_TXPAUS = (1 << 3);
		private const byte MACON1_RXPAUS = (1 << 2);
		//private const byte MACON1_PASSALL = (1 << 1);
		private const byte MACON1_MARXEN = (1);

		// MACON3 bits --------
		//private const byte MACON3_PADCFG2 = (1 << 7);
		//private const byte MACON3_PADCFG1 = (1 << 6);
		private const byte MACON3_PADCFG0 = (1 << 5);
		private const byte MACON3_TXCRCEN = (1 << 4);
		//private const byte MACON3_PHDREN = (1 << 3);
		//private const byte MACON3_HFRMEN = (1 << 2);
		private const byte MACON3_FRMLNEN = (1 << 1);
		//private const byte MACON3_FULDPX = (1);

		// MACON4 bits --------
		private const byte MACON4_DEFER = (1 << 6);
		//private const byte MACON4_BPEN = (1 << 5);
		//private const byte MACON4_NOBKOFF = (1 << 4);
		private const byte MACON4_PUREPRE = (1);

		// MICMD bits ---------
		//private const byte MICMD_MIISCAN = (1 << 1);
		private const byte MICMD_MIIRD = (1);

		// EBSTCON bits -----
		//private const byte EBSTCON_PSV2 = (1 << 7);
		//private const byte EBSTCON_PSV1 = (1 << 6);
		//private const byte EBSTCON_PSV0 = (1 << 5);
		//private const byte EBSTCON_PSEL = (1 << 4);
		//private const byte EBSTCON_TMSEL1 = (1 << 3);
		//private const byte EBSTCON_TMSEL0 = (1 << 2);
		//private const byte EBSTCON_TME = (1 << 1);
		//private const byte EBSTCON_BISTST = (1);

		// MISTAT bits --------
		//private const byte MISTAT_NVALID = (1 << 2);
		//private const byte MISTAT_SCAN = (1 << 1);
		private const byte MISTAT_BUSY = (1);

		// ECOCON bits -------
		//private const byte ECOCON_COCON2 = (1 << 2);
		//private const byte ECOCON_COCON1 = (1 << 1);
		//private const byte ECOCON_COCON0 = (1);

		// EFLOCON bits -----
		//private const byte EFLOCON_FULDPXS = (1 << 2);
		//private const byte EFLOCON_FCEN1 = (1 << 1);
		//private const byte EFLOCON_FCEN0 = (1);

		// PHY bits

		// PHCON1 bits ----------
		//private const ushort PHCON1_PRST = (1 << 15);
		//private const ushort PHCON1_PLOOPBK = (1 << 14);
		//private const ushort PHCON1_PPWRSV = (1 << 11);
		//private const ushort PHCON1_PDPXMD = (1 << 8);

		// PHSTAT1 bits --------
		//private const ushort PHSTAT1_PFDPX = (1 << 12);
		//private const ushort PHSTAT1_PHDPX = (1 << 11);
		//private const ushort PHSTAT1_LLSTAT = (1 << 2);
		//private const ushort PHSTAT1_JBSTAT = (1 << 1);

		// PHID2 bits --------
		//private const ushort PHID2_PID24 = (1 << 15);
		//private const ushort PHID2_PID23 = (1 << 14);
		//private const ushort PHID2_PID22 = (1 << 13);
		//private const ushort PHID2_PID21 = (1 << 12);
		//private const ushort PHID2_PID20 = (1 << 11);
		//private const ushort PHID2_PID19 = (1 << 10);
		//private const ushort PHID2_PPN5 = (1 << 9);
		//private const ushort PHID2_PPN4 = (1 << 8);
		//private const ushort PHID2_PPN3 = (1 << 7);
		//private const ushort PHID2_PPN2 = (1 << 6);
		//private const ushort PHID2_PPN1 = (1 << 5);
		//private const ushort PHID2_PPN0 = (1 << 4);
		//private const ushort PHID2_PREV3 = (1 << 3);
		//private const ushort PHID2_PREV2 = (1 << 2);
		//private const ushort PHID2_PREV1 = (1 << 1);
		//private const ushort PHID2_PREV0 = (1);

		// PHCON2 bits ----------
		//private const ushort PHCON2_FRCLNK = (1 << 14);
		//private const ushort PHCON2_TXDIS = (1 << 13);
		//private const ushort PHCON2_JABBER = (1 << 10);
		private const ushort PHCON2_HDLDIS = (1 << 8);

		// PHSTAT2 bits --------
		//private const ushort PHSTAT2_TXSTAT = (1 << 13);
		//private const ushort PHSTAT2_RXSTAT = (1 << 12);
		//private const ushort PHSTAT2_COLSTAT = (1 << 11);
		private const ushort PHSTAT2_LSTAT = (1 << 10);
		//private const ushort PHSTAT2_DPXSTAT = (1 << 9);
		//private const ushort PHSTAT2_PLRITY = (1 << 5);

		// PHIE bits -----------
		//private const ushort PHIE_PLNKIE = (1 << 4);
		//private const ushort PHIE_PGEIE = (1 << 1);

		// PHIR bits -----------
		//private const ushort PHIR_PLNKIF = (1 << 4);
		//private const ushort PHIR_PGIF = (1 << 2);

		// PHLCON bits -------
		//private const ushort PHLCON_LACFG3 = (1 << 11);
		//private const ushort PHLCON_LACFG2 = (1 << 10);
		//private const ushort PHLCON_LACFG1 = (1 << 9);
		//private const ushort PHLCON_LACFG0 = (1 << 8);
		//private const ushort PHLCON_LBCFG3 = (1 << 7);
		//private const ushort PHLCON_LBCFG2 = (1 << 6);
		//private const ushort PHLCON_LBCFG1 = (1 << 5);
		//private const ushort PHLCON_LBCFG0 = (1 << 4);
		//private const ushort PHLCON_LFRQ1 = (1 << 3);
		//private const ushort PHLCON_LFRQ0 = (1 << 2);
		//private const ushort PHLCON_STRCH = (1 << 1);
		// ReSharper restore InconsistentNaming

		private const ushort MaxFrameSize = 1518; // 1518 is the IEEE 802.3 specified limit

		#endregion

		#region Fields

        #if TINYCLR_TRACE
		internal static bool _verboseDebugging = false;
		#endif

		internal static readonly byte[] BroadcastMac = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
		internal static readonly byte[] BlankMac = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
		internal static readonly byte[] BlankIpAddress = { 0x00, 0x00, 0x00, 0x00 };
		internal static readonly byte[] BroadcastIpAddress = { 0xFF, 0xFF, 0xFF, 0xFF };
		internal static ushort EphmeralPort = (ushort)((new Random()).Next(16380) + 49153);  // port should be in Range: 49152 to 65535; see http://en.wikipedia.org/wiki/Ephemeral_port

		private static readonly object BankAccess = new object();
		private readonly object _interruptAccess = new object();
		private readonly InterruptPort _irq;
		private static readonly object SendAccess = new object();

		private static readonly byte[] SpiBuffer = new byte[3];
		private static ushort _bank;
		private bool _initialized;
		private byte _lastFilter;
		private TimeSpan _lastPacketReceived = Utility.GetMachineTime();
		private TimeSpan _lastReceiveReset = TimeSpan.MinValue;
		private TimeSpan _lastRestart = TimeSpan.MinValue;
		private bool _linkInit;
		internal static bool IsRenewing;

		internal static readonly ManualResetEvent StartupHold = new ManualResetEvent(false);
		private static readonly Timer PollingTimer = new Timer(PollNow, null, 5000, 10000);
		
		private static TimeSpan _linkUpTime = TimeSpan.MaxValue;

		internal static byte[] _macAddress = { 0x5c, 0x86, 0x4a, 0x00, 0x00, 0xdd };
		private ushort _nextPacketPointer = RxStart;

		private Timer _regCheckTimer;
		private bool _resetPending;
		private bool _rxResetPending;
		private Timer _watchdog;
		internal static byte[] _preferredDomainNameServer;
		internal static bool _dhcpDisabled;
		internal static byte[] _subnetMask;
		internal static byte[] _gatewayMac;
		internal static byte[] _alternateDomainNameServer;

		internal static string _name = "EthClick";
		internal static byte[] _ip;
		internal static byte[] _defaultGateway;

		private static TimeSpan _lastInternetCheck = TimeSpan.MinValue;
		private static bool _internetUp;
		
		private static readonly Hashtable ListeningPorts = new Hashtable();

		private static SPI.Configuration _spiConfiguration;
		private static OutputPort _resetPin;

		//private static MultiSPI SPI;

		#endregion
		
		#region CTOR

		/// <summary>
		/// Creates a new instance of the <see cref="EthClick"/> class, the underlying ENC28J60 IC will be held in reset until the <see cref="Start()"/> or <see cref="Start(byte[], string)"/> method is called.
		/// </summary>
		/// <param name="socket">The <see cref="Hardware.Socket"/> that the <see cref="EthClick"/> is inserted into.</param>
		/// <exception cref="PinInUseException">A PinInUseException will be thrown if the Miso, Mosi, Cs, Sck, Int, Rst Pins are already in use by another driver on the same socket.</exception>
		public EthClick(Hardware.Socket socket)
		{
			try
			{
				Hardware.CheckPins(socket, socket.Miso, socket.Mosi, socket.Cs, socket.Sck, socket.Int, socket.Rst);

				_irq = new InterruptPort(socket.Int, true, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeLevelLow);
				_irq.OnInterrupt += irq_OnInterrupt;

				_spiConfiguration = new SPI.Configuration(socket.Cs, false, 0, 0, false, true, 10000, socket.SpiModule);
				//_spiConfiguration = new SPI.Configuration(socket.Cs, false, 0, 0, false, true, 10000, SPI.SPI_module.SPI3);


				//SPI = new MultiSPI(_spiConfiguration, true);


				if (Hardware.SPIBus == null)
				{
					Hardware.SPIBus = new SPI(_spiConfiguration);
					//Hardware.SPIBus = SPI;
				}

				_resetPin = new OutputPort(socket.Rst, true);
			}
			catch (PinInUseException ex)
			{
				
				throw new PinInUseException(ex.Message);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="spiBus"></param>
		public EthClick(Hardware.Socket socket, SPI.SPI_module spiBus)
		{
			_irq = new InterruptPort(socket.Int, true, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeLevelLow);
			_irq.OnInterrupt += irq_OnInterrupt;

			_spiConfiguration = new SPI.Configuration(Pin.PA15, false, 0, 0, false, true, 10000, spiBus);

			//SPI = new MultiSPI(_spiConfiguration, false);
			
			_resetPin = new OutputPort(socket.Rst, true);
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or Sets the Netbios (mDNS) Device Name.
		/// </summary>
		/// <remarks>Do not include the .local on the end. Anything after the period is dropped. Also, there is probably a character limit (probably 32), so keep it short.</remarks>
 		/// <example>Example Usage:
		/// <code language = "C#">
		/// _eth.Name = "MikroBusNet";
		/// </code>
		/// <code language = "VB">
		/// _eth.Name = "MikroBusNet"
		/// </code>
		/// </example>
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if (value == null || value == String.Empty || value.Trim().Length == 0) throw new ArgumentException("Name property cannot be null, empty or consist of white space.", "value");
				_name = value.ToLower().Trim();
			}
		}

		/// <summary>
		/// The Globally Unique Media Access Control (MAC) address for this device
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _eth.MacAddress = GenerateUniqueMacAddress("MikroBusNet");
		/// Or use,
		/// _eth.MacAddress =  = { 0x5C, 0x86, 0x4A, 0x00, 0x00, 0xDD };
		/// </code>
		/// <code language = "VB">
		/// _eth.MacAddress = GenerateUniqueMacAddress("MikroBusNet")
		/// Or use,
		/// _eth.MacAddress =  =  New Byte() {<![CDATA[&]]>H5C, <![CDATA[&]]>H86, <![CDATA[&]]>H4A, <![CDATA[&]]>H0, <![CDATA[&]]>H0, <![CDATA[&]]>HDD}
		/// </code>
		/// </example>
		public byte[] MacAddress
		{
			get { return _macAddress; }
			set { _macAddress = value; }
		}

		/// <summary>
		/// Gets or sets Automatic/Dynamic IP Address assignment (DHCP); set to TRUE if you want to manually assign an IP address (static).
		/// </summary>
		/// <remarks>If DHCP is disabled, you must set the <see cref="DefaultGateway"/>, <see cref="IPAddress"/>, <see cref="PreferredDomainNameServer"/>
		/// properties as well as the <see cref="MacAddress"/> property before starting networking with the <see cref="Start()"/> or <see cref="Start(byte[], string)"/>
		/// methods. The <see cref="AlternateDomainNameServer"/> is optional but will help in DNS Name Resolution if the Preferred DNS fails.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _eth.DHCPDisabled = true;
		/// </code>
		/// <code language = "VB">
		/// _eth.DHCPDisabled = True
		/// </code>
		/// </example>
		public bool DHCPDisabled
		{
			get { return _dhcpDisabled; }
			set { _dhcpDisabled = value; }
		}

		/// <summary>
		/// Gets or Sets the IP Address of the EthClick.
		/// </summary>
		/// <remarks>To assign a static IPAddress manually, set this property to the desired IP address and set the <see cref="DHCPDisabled"/> Property to true.
		/// <para>Note: this will be overwritten with one provided by the router if DHCP is enabled and router provides a DNS server.</para>
		/// </remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _eth.IPAddress = new byte[] {192, 168, 1, 24};
		/// </code>
		/// <code language = "VB">
		/// _eth.IPAddress = New Byte() {192, 168, 1, 24}
		/// </code>
		/// </example>
		public byte[] IPAddress
		{
			get
			{
				return _ip;
			}
			set
			{
				_ip = value;
			}
		}

		/// <summary>
		/// Gets or Sets the IP Address of Preferred Domain Name Server (DNS) that the EthClck will connect to when establishing a network connection.
		/// </summary>
		/// <remarks>Note: this will be overwritten with one provided by the router if DHCP is enabled and router provides a DNS server.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _eth.PreferredDomainNameServer = new Byte[] { 8, 8, 8, 8 }; // This is Google DNS or use your ISP PrimaryDNS Address or simply assign to your Routers DefaultGateway.
		/// </code>
		/// <code language = "VB">
		/// _eth.PreferredDomainNameServer = new Byte() { 8, 8, 8, 8 } ' This is Google DNS or use your  ISP PrimaryDNS Address or simply assign to your Routers DefaultGateway.
		/// </code>
		/// </example>
		public byte[] PreferredDomainNameServer
		{
			get { return _preferredDomainNameServer; }
			set { _preferredDomainNameServer = value; }
		}

		/// <summary>
		/// Gets or Sets the IP Address of Alternate Domain Name Server (DNS) that the EthClck will connect to when establishing a network connection.
		/// </summary>
		/// <remarks>
		/// The Alternate DNS server is never automatically assigned by DNS.  You have to set this value yourself.
		/// It will be used if it is populated and the Preferred DNS server fails.  
		/// </remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _eth.AlternateDomainNameServer = new Byte[] { 8, 8, 8, 4 }; // Google Alternate DNS or use your ISP SecondaryDNS Address.
		/// </code>
		/// <code language = "VB">
		/// _eth.AlternateDomainNameServer = new Byte() { 8, 8, 8, 4 } ' Google Alternate DNS or use your ISP SecondaryDNS Address.
		/// </code>
		/// </example>
		public byte[] AlternateDomainNameServer
		{
			get { return _alternateDomainNameServer; }
			set { _alternateDomainNameServer = value; }
		}

		/// <summary>
		/// Sets or Gets the Default Gateway IP Address of theRouter that the EthClick will connect to when establishing a network connection.
		/// <para>The default gateway provides an entry point and an exit point in a network.</para>
		/// <para>Set this value to the LAN/TCP IPAddress of your Router.</para>
		/// </summary>
		/// <remarks>Note: this will be overwritten with one provided by the router if DHCP is enabled and router provides a DNS server.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _eth.DefaultGateway = new Byte[] { 192, 168, 1, 1 }; // Your Routers DHCP / mDNS IP Address.
		/// </code>
		/// <code language = "VB">
		/// _eth.DefaultGateway = new Byte() { 192, 168, 1, 1 } ' Your Routers DHCP / mDNS IP Address.
		/// </code>
		/// </example>
		public byte[] DefaultGateway
		{
			get { return _defaultGateway; }
			set
			{
				if (value == null)
				{
					GatewayMac = null;
				}
				else
				{
					if (_defaultGateway == null || !_defaultGateway.BytesEqual(value))
					{
						_defaultGateway = value;
					}
				}
				_defaultGateway = value;
			}
		}

		/// <summary>
		/// Gets or sets the Gateway MAC Address of the Router that the EthClick will connect to when establishing a network connection.
		/// <para>Set this value to the Gateway MAC Address of the Router.</para>
		/// </summary>
		/// <remarks>Note: this will be overwritten with one provided by the router if DHCP is enabled and router provides a DNS server.</remarks>
		public byte[] GatewayMac
		{
			get { return _gatewayMac; }
			set { _gatewayMac = value; }
		}

		/// <summary>
		/// Gets or sets the IP Subnet Mask of the Router that the EthClick will connect to when establishing a network connection.
		/// <para>Set this value to the IP Subnet Mask of the Router of the Router.</para>
		/// </summary>
		/// <remarks>Note: this will be overwritten with one provided by the router if DHCP is enabled and router provides a DNS server.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _eth.SubnetMask = new byte[] {255, 255, 255, 0};
		/// </code>
		/// <code language = "VB">
		/// _eth.SubnetMask = New Byte() {255, 255, 255, 0}
		/// </code>
		/// </example>
		public byte[] SubnetMask
		{
			get { return _subnetMask; }
			set { _subnetMask = value; }
		}

		/// <summary>
		/// Gets the value of the Internet connection of the EthClick.
		/// </summary>
		/// <returns>
		/// True means you are connected to the Internet. False means you are not able to reach computers on the Internet, although local computers may be reachable.
		/// </returns>
		/// <remarks>
		/// Note: this call is expensive and synchronous, so don't call it a lot!  Use it Judiciously!  
		/// Also, if you call this more frequently than every 5 seconds, you will get the last cached value if Ethernet is connected!
		/// This call can take up to 60 seconds to complete.
		/// </remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		/// while (true)
		///	{
		///		if (_eth.ConnectedToInternet)
		///		{
		///			Debug.Print("Connected to Internet");
		///			break;
		///		}
		///		Debug.Print("Waiting on Internet connection");
		///		Thread.Sleep(5000);
		///	}
		/// </code>
		/// <code language = "VB">
		/// While (True)
		///		If (_eth.ConnectedToInternet) Then
		///			Debug.Print("Connected to Internet")
		///			Exit While
		///		End If
		///		Debug.Print("Waiting on Internet connection")
		///		Thread.sleep(5000)
		///	End While
		/// </code>
		/// </example>
		public bool ConnectedToInternet
		{
			get
			{
				if (!ConnectedToEthernet) return false;

				if (_lastInternetCheck > TimeSpan.MinValue && _lastInternetCheck > PowerState.Uptime.Subtract(new TimeSpan(0, 0, 5)))
				{
					return _internetUp;
				}

				_lastInternetCheck = PowerState.Uptime;

				HttpResponse response;

				try
				{
					var r = new HttpRequest("http://www.msftncsi.com/ncsi.txt");
					r.Headers.Add("Accept", "*/*");
					response = r.Send();
				}
				catch
				{
					return false;
				}

				_internetUp = response != null;

				return _internetUp;
			}
		}

		/// <summary>
		/// Gets the value of the Ethernet connection of the EthClick.
		/// </summary>
		/// <returns>
		/// True if the Ethernet cable is plugged in and you have established a link to something. False if the cable is unplugged or the network is down.
		/// </returns>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Debug.Print( "Connected to Ethernet? " + (_eth.ConnectedToEthernet ? "True" : "False"));
		/// </code>
		/// <code language = "VB">
		/// Debug.Print("Connected to Ethernet? " <![CDATA[&]]> If(_eth.ConnectedToEthernet, "True", "False"))
		/// </code>
		/// </example>
		public bool ConnectedToEthernet
		{
			get
			{
				try
				{
					return _linkUpTime < TimeSpan.MaxValue;
				}
				catch
				{
					return false;
				}
			}
		}

		/// <summary>
		/// Gets or sets the power mode.
		/// </summary>
		/// <value>
		/// The current power mode of the module.
		/// </value>
		/// <returns>The current <see cref="PowerModes"/> state of the EthClick.</returns>
		/// <exception cref="NotImplementedException">A NotImplementedException will be thrown if setting the PowerMode to <see cref="PowerModes.Low"/>.</exception>
		/// <example>
		/// <code language = "C#">
		/// Debug.Print("Current PowerMode? " + _eth.PowerMode);
		/// _eth.PowerMode = PowerModes.Off;
		/// Debug.Print("Current PowerMode? " + _eth.PowerMode);
		/// _eth.PowerMode = PowerModes.On;
		/// Debug.Print("Current PowerMode? " + _eth.PowerMode);
		/// </code>
		/// <code language = "VB">
		/// Debug.Print("Current PowerMode? " <![CDATA[&]]> _eth.PowerMode)
		/// _eth.PowerMode = PowerModes.Off
		/// Debug.Print("Current PowerMode? " <![CDATA[&]]> _eth.PowerMode)
		/// _eth.PowerMode = PowerModes.On;
		/// Debug.Print("Current PowerMode? " <![CDATA[&]]> _eth.PowerMode)
		/// </code>
		/// </example>
		public PowerModes PowerMode
		{
			get { return _resetPin.Read() ? PowerModes.On : PowerModes.Off; }
			set
			{
				if (value == PowerModes.Off && _resetPin.Read()) MacPowerDown();
				if (value == PowerModes.On && !_resetPin.Read()) MacPowerUp();
				if (value == PowerModes.Low) throw new NotImplementedException("Low Power Mode not implemented for this module.");
			}
		}

		/// <summary>
		/// Gets the driver version.
		/// </summary>
		/// <value>
		/// The driver version see <see cref="Version" />.
		/// </value>
		/// <example>
		/// Example usage to get the Driver Version in formation:
		/// <code language="C#">
		/// System.Diagnostics.Debug.WriteLine("Driver Version Info : " + _clock1.DriverVersion);
		/// </code>
		/// <code language="VB">
		/// System.Diagnostics.Debug.WriteLine("Driver Version Info : " <![CDATA[&]]> _clock1.DriverVersion)
		/// </code>
		/// </example>
		public Version DriverVersion
		{
			get { return Assembly.GetAssembly(GetType()).GetName().Version; }
		}

		#if TINYCLR_TRACE
        /// <summary>
        /// Sets or gets the output of Debug information 
        /// </summary>
        /// <remarks>This property is only available during Debug builds.</remarks>
        public Boolean VerboseDebugging
        {
            get
            {
                return _verboseDebugging;
            }
            set
            {
                _verboseDebugging = value;
            }
        }
		#endif

		/// <summary>
		/// Get the revision id of the ENC28J60 IC, see current silicon errata @ Microchip
		/// </summary>
		/// <returns>The chip revision of the ENC28J60. 0x02 = Rev B1, 0x03 = Rev B4, 0x04 = Rev B5 and 0x06 = Rev B7 </returns>
		/// <example>Example usage:
		/// <code language = "C#">
		/// Debug.Print("Chip Revision ID ? " + _eth.ChipRevisionId());
		/// </code>
		/// <code language = "VB">
		/// Debug.Print("Chip Revision ID ? " <![CDATA[&]]> _eth.ChipRevisionId())
		/// </code>
		/// </example>
		public byte ChipRevisionId
		{
			get
			{
				return ReadEthReg(EREVID);
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Starts the network using the supplied MAC Address and Name.
		/// </summary>
		/// <param name="macAddress">The unique and qualified MAC Address for the EthClick.</param>
		/// <param name="netBiosName">The NetBios/mDNS Name used to identify the EthClick on your Network.</param>
		/// <remarks>Use this method if you want to provide your own MAC Address and Name and to avoid possible collisions with existing devices on the network.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _eth.Start(_eth.GenerateUniqueMacAddress("MikroBusNet"), "MikroBusNet");
		/// // or use
		/// _eth.Start(new byte[] { 0x5C, 0x86, 0x4A, 0x00, 0x00, 0xDD }, "MikroBusNet");
		/// </code>
		/// <code language = "VB">
		/// _eth.Start(_eth.GenerateUniqueMacAddress("MikroBusNet"), "MikroBusNet")
		/// ' or use
		/// _eth.Start(new Byte() { <![CDATA[&]]>H5C, <![CDATA[&]]>H86, <![CDATA[&]]>H4A, <![CDATA[&]]>H0, <![CDATA[&]]>H0, <![CDATA[&]]>HDD }, "MikroBusNet")
		/// </code>
		/// </example>
		public void Start(byte[] macAddress, string netBiosName)
        {
            if(macAddress == null || macAddress.GetHashCode() == 0) throw new ArgumentException("The MAC Address parameter must be set prior to start networking.", "macAddress");
			if (netBiosName == null || netBiosName == string.Empty || netBiosName.Trim().Length == 0) throw new ArgumentException("The netBiosName parameter must be set to start networking.", "netBiosName");

            MacAddress = macAddress;
			Name = netBiosName;

			Start();
        }

		/// <summary>
		/// Starts the network using the default MAC Address (0x5C, 0x86, 0x4A, 0x00, 0x00, 0xDD) and Name (MikroBusNet).
		/// <para>If the <see cref="MacAddress"/> and <see cref="Name"/> properties are set, these will be used instead of the default values.</para>
		/// </summary>
		/// <remarks>To avoid possible collisions with existing devices on the network, use the <see cref="Start(byte[], string)"/> method.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _eth.Start(); // Use default MacAddress and Name.
		/// </code>
		/// <code language = "VB">
		/// _eth.Start() ' Use default MacAddress and Name.
		/// </code>
		/// </example>
		public void Start()
		{
			if (_macAddress == null || _macAddress.GetHashCode() == 0) throw new InvalidOperationException("The MacAddress Property must be set to start networking.");
			if ((_name == null) || (_name == String.Empty || _name.Trim().Length == 0)) throw new InvalidOperationException("The Name Property must be set to start networking.");

			OnLinkChanged += NicOnLinkChanged;
			OnFrameArrived += nic_OnFrameArrived;

			StartNetwork();

			if (!DHCPDisabled && (PreferredDomainNameServer == null || DefaultGateway == null || _ip == null))
			{
				if (!StartupHold.WaitOne(20000, true)) throw new DeviceInitialisationException("WARNING! Time out while waiting for DHCP.");
			}
			else if (DHCPDisabled && _ip != null && DefaultGateway != null && GatewayMac == null)
			{
				if (!StartupHold.WaitOne(20000, true)) throw new DeviceInitialisationException("WARNING!  Time out while waiting for DefaultGateway to Respond to ARP request");
			}
			else
			{
				if (!StartupHold.WaitOne(20000, true)) throw new DeviceInitialisationException("WARNING!  Networking is not properly configured to start.");
			}
		}

        /// <summary>
		/// Generates a Unique MAC Address based on a a HashCode
		/// </summary>
		/// <param name="uniqueIdString">Optional - Any <see cref="System.String"/> that you want to use as a seed. If a uniqueIdString is not supplied, the method will generate on based on a <see cref="Random"/> seed which is not guaranteed to be unique.</param>
		/// <returns>A properly encoded and unique MAC Address based on the string passed to the method.</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		/// var _macAddress = _eth.GenerateUniqueMacAddress("MikroBusNet");
		/// </code>
		/// <code language = "VB">
		/// Dim _macAddress as Byte() = _eth.GenerateUniqueMacAddress("MikroBusNet")
		/// </code>
		/// </example>
		public byte[] GenerateUniqueMacAddress(string uniqueIdString = null)
		{
			var uniqueMac = new byte[6];
			var r = uniqueIdString != null ? new Random(uniqueIdString.GetHashCode()) : new Random();
			r.NextBytes(uniqueMac);
			uniqueMac[0] = (byte)(uniqueMac[0] | 0x01);
			uniqueMac[0] = (byte)(uniqueMac[0] << 1);
			return uniqueMac;
		}

		/// <summary>
		/// Returns the string representation of a MAC Address.
		/// </summary>
		/// <param name="macAddress">The five byte MAC Address.</param>
		/// <param name="asHexString">If true returns a string as 0x5C:0x86:0x4A:0x00:0x00:0xDD, otherwise the returned string will be 5C:86:4A:00:00:DD</param>
		/// <returns>The string representation of the MAC Address.</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		/// var _macAddress = _eth.GenerateUniqueMacAddress("MikroBusNet");
		/// Debug.Print("MAC Address - " + _eth.MacAddressToString(_macAddress));
		/// </code>
		/// <code language = "VB">
		/// Dim _macAddress as Byte() = _eth.GenerateUniqueMacAddress("MikroBusNet")
		/// Debug.Print("MAC Address - " <![CDATA[&]]> _eth.MacAddressToString(_macAddress))
		/// </code>
		/// </example>
		public string MacAddressToString(byte[] macAddress, bool asHexString = true)
		{
			return asHexString
				? "0x" + macAddress[0].ToString("x") + ":" + "0x" + macAddress[1].ToString("x") + ":" + "0x" +
				  macAddress[2].ToString("x") + ":" + "0x" + macAddress[3].ToString("x") + ":" + "0x" +
				  macAddress[4].ToString("x") + ":" + "0x" + macAddress[5].ToString("x")
				: macAddress[0].ToString("x") + ":" + macAddress[1].ToString("x") + ":" + macAddress[2].ToString("x") + ":" +
				  macAddress[3].ToString("x") + ":" + macAddress[4].ToString("x") + ":" + macAddress[5].ToString("x");
		}

		/// <summary>
		/// Adds the specified port to the list of ports to use for listening for network traffic.
		/// </summary>
		/// <param name="portNumber">The port number to listen to.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _eth.ListenToPort(80);
		/// </code>
		/// <code language = "VB">
		/// _eth.ListenToPort(80)
		/// </code>
		/// </example>
		public void ListenToPort(ushort portNumber)
		{
			if (!ListeningPorts.Contains(portNumber)) ListeningPorts.Add(portNumber, portNumber.ToBytes());
		}

		/// <summary>
		/// Returns true if the port is currently is listening for network traffic.
		/// </summary>
		/// <param name="portNumber">The port number to check.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// Debug.Print("Listening to Port#  - " + _eth.IsListening(80));
		/// </code>
		/// <code language = "VB">
		/// Debug.Print("Listening to Port#  - " <![CDATA[&]]> _eth.IsListening(80))
		/// </code>
		/// </example>
		public bool IsListening(ushort portNumber)
		{
			return ListeningPorts.Contains(portNumber);
		}

		/// <summary>
		/// Removes the specified port from the list of ports to listen for network traffic.
		/// </summary>
		/// <remarks> All packets sent to this port will be discarded and ignored.</remarks>
		/// <param name="portNumber">The port number to remove.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _eth.StopListeningToPort(80);
		/// </code>
		/// <code language = "VB">
		/// _eth.StopListeningToPort(80)
		/// </code>
		/// </example>
		public void StopListeningToPort(ushort portNumber)
		{
			if (ListeningPorts.Contains(portNumber)) ListeningPorts.Remove(portNumber);
		}

		/// <summary>
		/// Resets the module
		/// </summary>
		/// <param name="resetMode">The reset mode - Only<see cref="ResetModes.Soft"/> is supported.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _eth.Reset(ResetModes.Hard);
		/// </code>
		/// <code language = "VB">
		/// _eth.Reset(ResetModes.Hard)
		/// </code>
		/// </example>
		public bool Reset(ResetModes resetMode)
		{
			if (resetMode == ResetModes.Soft)
			{
				Restart();
				return ConnectedToEthernet;
			}
			throw new NotImplementedException("This module does not support a Hard Reset");
		}

		#endregion
		
		#region Private Properties

		private static ushort CurrentBank
		{
			set
			{
				lock (BankAccess)
				{
					value = (ushort)((value >> 8) & 0x03);
					if (value == _bank) return;

					BfcReg(ECON1, ECON1_BSEL1 | ECON1_BSEL0);
					BfsReg(ECON1, (byte)value);

					_bank = value;
				}
			}
		}

		#endregion

		#region Private Methods
		
		private static byte Low(ushort a)
		{
			return (byte)(a & 0xFF);
		}

		private static byte High(ushort a)
		{
			return (byte)(a >> 8);
		}

		private void StartNetwork()
		{
			byte i;
			var loopMax = 100;

			do
			{
				i = ReadCommonReg(ESTAT);
				loopMax--;
				Thread.Sleep(100);
			} while (((i & 0x08) != 0 || (~i & ESTAT_CLKRDY) != 0) && loopMax > 0);

			#if  TINYCLR_TRACE
			if (_verboseDebugging) Debug.WriteLine("ESTAT: " + ReadCommonReg(ESTAT));
			#endif

			if (loopMax <= 0) throw new Exception("Unable to Communicate to the Ethernet Controller.");

			_regCheckTimer = new Timer(CheckRegisters, null, CheckDelay, CheckDelay);
			_watchdog = new Timer(WatchDogCheck, null, 10000, 3000);

			Restart();
		}

		private void SetupReceiveBuffer()
		{
			WriteReg(ERXSTL, Low(RxStart));
			WriteReg(ERXSTH, High(RxStart));
			WriteReg(ERXRDPTL, Low(RxStart));
			WriteReg(ERXRDPTH, High(RxStart));
			WriteReg(ERXNDL, Low(RxStop));
			WriteReg(ERXNDH, High(RxStop));

			_nextPacketPointer = RxStart;
		}

		private static void SetupTransmitBuffer()
		{
			WriteReg(ETXNDL, Low(TxStop));
			WriteReg(ETXNDH, High(TxStop));
			WriteReg(ETXSTL, Low(TxStart));
			WriteReg(ETXSTH, High(TxStart));
			WriteReg(EWRPTL, Low(TxStart));
			WriteReg(EWRPTH, High(TxStart));
		}

		private static void SetupMacFilters()
		{
			WriteReg(ERXFCON, ERXFCON_UCEN | ERXFCON_BCEN | ERXFCON_PMEN | ERXFCON_CRCEN);
			WriteReg(EPMOH, 0x00);
			WriteReg(EPMOL, 0x00);
			WriteReg(EPMM0, 0x07);
			WriteReg(EPMM1, 0x00);
			WriteReg(EPMM2, 0x00);
			WriteReg(EPMM3, 0xc0);
			WriteReg(EPMM4, 0x01);
			WriteReg(EPMM5, 0x00);
			WriteReg(EPMM6, 0x00);
			WriteReg(EPMM7, 0x00);
			WriteReg(EPMCSH, 0xa0);
			WriteReg(EPMCSL, 0x1f);
		}

		private void WatchDogCheck(object o)
		{
			#if  TINYCLR_TRACE
			if (_verboseDebugging) Debug.WriteLine(DateTime.Now + " -- Watchdog Check!  Memory = " + Microsoft.SPOT.Debug.GC(false)); 
			#endif

			if (_linkUpTime == TimeSpan.MaxValue)
			{
				UpdateLinkState(true);
				if (_linkUpTime == TimeSpan.MaxValue) return;
			}

			irq_OnInterrupt(0, 0, DateTime.Now);
		}

		private void CheckRegisters(object o)
		{
			if (_rxResetPending || _resetPending) return;

			var eir = ReadCommonReg(EIR);

			if ((eir & EIR_RXERIF) != 0)
			{
				BfcReg(EIR, EIR_RXERIF);
				#if TINYCLR_TRACE
				if (_verboseDebugging) Debug.WriteLine("CheckRegisters Clearing RX Error - EIR: " + eir + " => " + ReadCommonReg(EIR)); 
				#endif
			}

			#if TINYCLR_TRACE
			var eie = ReadCommonReg(EIE);
			#endif

			if (ReadCommonReg(EIE) != (EIE_INTIE | EIE_PKTIE | EIE_LINKIE | EIE_TXERIE | EIE_RXERIE))
			{
				WriteReg(EIE, EIE_INTIE | EIE_PKTIE | EIE_LINKIE | EIE_TXERIE | EIE_RXERIE);
				#if TINYCLR_TRACE
				if (_verboseDebugging) Debug.WriteLine("CheckRegisters Correction - EIE: " + eie + " => " + ReadCommonReg(EIE));
				#endif
			}

			#if TINYCLR_TRACE
			var econ1 = ReadCommonReg(ECON1);
			#endif
			if (!_rxResetPending && ((ReadCommonReg(ECON1) & ECON1_RXEN) == 0))
			{
				WriteReg(ECON1, ECON1_RXEN);
				#if TINYCLR_TRACE
				if (_verboseDebugging) Debug.WriteLine("CheckRegisters Correction - ECON1: " + econ1 + " => " + ReadCommonReg(ECON1));
				#endif
			}
		}

		private void irq_OnInterrupt(uint data1, uint data2, DateTime time)
		{

			if (!_initialized) return;

			lock (_interruptAccess)
			{

				#if TINYCLR_TRACE
				if (_verboseDebugging) Debug.WriteLine("Interrupt Received"); 
				#endif

				byte eir = ReadCommonReg(EIR);

				#if TINYCLR_TRACE
				if (_verboseDebugging) Debug.WriteLine("EIR: " + eir); 
				#endif

				if (eir == 0) CheckRegisters(null);

				if ((eir & EIR_TXERIF) != 0)
				{
					#if  TINYCLR_TRACE
					if (_verboseDebugging) Debug.WriteLine("TX Error Detected"); 
					#endif
					BfcReg(EIR, EIR_TXERIF);
				}
				else if ((eir & EIR_RXERIF) != 0)
				{
					#if TINYCLR_TRACE
					if (_verboseDebugging) Debug.WriteLine("RX Error Detected"); 
					#endif
					BfcReg(EIR, EIR_RXERIF);
				}

				if (!_resetPending && !_rxResetPending)
				{
					#if TINYCLR_TRACE
					if (_verboseDebugging) Debug.WriteLine("EIR: " + eir.ToString()); 
					#endif

					UpdateLinkState();

					while (OnFrameArrived != null && !_resetPending && (((ReadCommonReg(EIR) & EIR_PKTIF) != 0) || ReadEthReg(EPKTCNT) > 0))
					{
						OnFrameArrived.Invoke(this, ReceiveFrame(), time);
					}
				}
				_irq.ClearInterrupt();
			}

			if (_resetPending)
			{
				_resetPending = false;
				Thread.Sleep(50);
				Restart();
			}
			else if (Utility.GetMachineTime() < _lastReceiveReset.Add(new TimeSpan(0, 0, 12)) &&
					 Utility.GetMachineTime().Subtract(_linkUpTime).Ticks > TimeSpan.TicksPerSecond * 12 &&
					 Utility.GetMachineTime().Subtract(_lastPacketReceived).Ticks > TimeSpan.TicksPerSecond * 12)
			{
				_resetPending = false;
				Thread.Sleep(50);
				Restart();
			}
			else if (!_rxResetPending && Utility.GetMachineTime().Subtract(_lastPacketReceived).Ticks > TimeSpan.TicksPerSecond * 5)
			{
				ReceiveReset();
				Thread.Sleep(10);
			}
		}

		private byte[] ReceiveFrame()
		{
			lock (SendAccess)
			{
				var packetCount = ReadEthReg(EPKTCNT);

				ThrottleFrameReception(packetCount);

				#if TINYCLR_TRACE
				if (_verboseDebugging) Debug.WriteLine("Packet Count is: " + packetCount); 
				#endif

				var readPointer = (ushort) (ReadEthReg(ERXRDPTL) | ReadEthReg(ERXRDPTH) << 8);
				var writePointer = (ushort) (ReadEthReg(ERXWRPTL) | ReadEthReg(ERXWRPTH) << 8);

				#if TINYCLR_TRACE
				if (_verboseDebugging)
				{
					Debug.WriteLine("ERXRDPT: " + readPointer);
					Debug.WriteLine("ERXWRPT: " + writePointer);
				} 
				#endif

				if (packetCount >= 255 || readPointer > RxStop || writePointer > RxStop)
				{
					#if TINYCLR_TRACE
					if (_verboseDebugging) Debug.WriteLine("Something is very wrong here..."); 
					#endif
					ReceiveReset();
					return null;
				}

				WriteReg(ERDPTL, Low(_nextPacketPointer));
				WriteReg(ERDPTH, High(_nextPacketPointer));

				#if TINYCLR_TRACE
				if (_verboseDebugging) Debug.WriteLine("Setting Read Pointer in buffer to: " + _nextPacketPointer); 
				#endif

				var lastPacketPointer = _nextPacketPointer;

				var frame = ReadBuffer(6);
				var receivedByteCount = (ushort)(frame[2] | frame[3] << 8);
				#if TINYCLR_TRACE
				var statusVector = (ushort)(frame[4] | frame[5] << 8);
				#endif
				_nextPacketPointer = (ushort)(frame[0] | frame[1] << 8);

				#if TINYCLR_TRACE
				if (_verboseDebugging) Debug.WriteLine("Setting Next Packet Pointer to: " + _nextPacketPointer); 
				#endif

				bool receivedOk = (frame[4] & (1 << 7)) != 0;
				bool zeroBitMissing = (frame[5] & (1 << 7)) != 0;

				if (_nextPacketPointer > RxStop || _nextPacketPointer == 0 || !receivedOk || zeroBitMissing)
				{
					#if TINYCLR_TRACE
					if (_verboseDebugging) Debug.WriteLine("Buffer Read Fix Experiment"); 
					#endif

					WriteReg(ERDPTL, Low(lastPacketPointer));
					WriteReg(ERDPTH, High(lastPacketPointer));

					Thread.Sleep(10);

					frame = ReadBuffer(6);
					receivedByteCount = (ushort)(frame[2] | frame[3] << 8);
					#if TINYCLR_TRACE
					statusVector = (ushort)(frame[4] | frame[5] << 8);
					#endif
					_nextPacketPointer = (ushort)(frame[0] | frame[1] << 8);

					#if TINYCLR_TRACE
					if (_verboseDebugging) Debug.WriteLine("Setting Next Packet Pointer to: " + _nextPacketPointer); 
					#endif
				}

				#if TINYCLR_TRACE
				if (_verboseDebugging)
				{
					Debug.WriteLine("Received Byte Count: " + receivedByteCount);
					Debug.WriteLine("Status Vector: " + statusVector);
				}
				#endif

				var crcError = (frame[4] & (1 << 4)) != 0;

				#if TINYCLR_TRACE
				var lengthCheckError = (frame[4] & (1 << 5)) != 0;
				var lengthOutOfRange = (frame[4] & (1 << 6)) != 0;
				#endif

				receivedOk = (frame[4] & (1 << 7)) != 0;
				var isMulticast = (frame[5] & (1 << 0)) != 0;
				var isBroadcast = (frame[5] & (1 << 1)) != 0;
				var unknownOpcode = (frame[5] & (1 << 5)) != 0;
				zeroBitMissing = (frame[5] & (1 << 7)) != 0;

				#if TINYCLR_TRACE
				if (_verboseDebugging)
				{
					Debug.WriteLine("**** Packet Received Status Vector ****");
					Debug.WriteLine("CRC Error: " + crcError);
					Debug.WriteLine("Length Check Error: " + lengthCheckError);
					Debug.WriteLine("Length Out of Range (large than 1500 bytes?): " + lengthOutOfRange);
					Debug.WriteLine("Received OK: " + receivedOk);
					Debug.WriteLine("Is Multi-cast? " + isMulticast);
					Debug.WriteLine("Is Broadcast? " + isBroadcast);
					Debug.WriteLine("UnknownOpcode: " + unknownOpcode);
				}
				#endif

				if (crcError || !receivedOk || unknownOpcode || zeroBitMissing || receivedByteCount - 4 > MaxFrameSize || receivedByteCount < 38)
				{
					#if TINYCLR_TRACE
					if (_verboseDebugging)
					{
						Debug.WriteLine("Error detected\nStatus Vector = " + statusVector);
						Debug.WriteLine("Rec'd Byte Count = " + receivedByteCount);
						Debug.WriteLine("Packets: " + ReadEthReg(EPKTCNT));
					}
					#endif

					if (receivedByteCount > MaxFrameSize + 4 || zeroBitMissing || receivedByteCount < 38)
					{
						var trycount = 256;

						Debug.WriteLine("## Dropping " + ReadEthReg(EPKTCNT) + " packets ## - Experimental");

						while (ReadEthReg(EPKTCNT) > 0 && trycount-- > 0)
						{
							BfsReg(ECON2, ECON2_PKTDEC);
						}

						_nextPacketPointer = (ushort)(ReadEthReg(ERXWRPTL) | ReadEthReg(ERXWRPTH) << 8);

						#if TINYCLR_TRACE
						if (_verboseDebugging) Debug.WriteLine("Setting Next Packet Pointer to: " + _nextPacketPointer); 
						#endif

						if ((_nextPacketPointer - 1) < RxStart || (_nextPacketPointer - 1) > RxStop)
						{
							#if TINYCLR_TRACE
							if (_verboseDebugging) Debug.WriteLine("Read Pointer set to STOP"); 
							#endif

							WriteReg(ERXRDPTL, Low(RxStop));
							WriteReg(ERXRDPTH, High(RxStop));
						}
						else
						{
							#if TINYCLR_TRACE
							if (_verboseDebugging) Debug.WriteLine("Read Pointer set to NextPacketPointer"); 
							#endif

							WriteReg(ERXRDPTL, Low((ushort)(_nextPacketPointer - 1)));
							WriteReg(ERXRDPTH, High((ushort)(_nextPacketPointer - 1)));
						}

						WriteReg(ERDPTL, Low(_nextPacketPointer));
						WriteReg(ERDPTH, High(_nextPacketPointer));

						//frame = null;
						return null; // frame;
					}

					if ((_nextPacketPointer - 1) < RxStart || (_nextPacketPointer - 1) > RxStop)
					{
						#if TINYCLR_TRACE
						if (_verboseDebugging) Debug.WriteLine("Read Pointer set to STOP"); 
						#endif

						WriteReg(ERXRDPTL, Low(RxStop));
						WriteReg(ERXRDPTH, High(RxStop));
					}
					else
					{
						WriteReg(ERXRDPTL, Low((ushort)(_nextPacketPointer - 1)));
						WriteReg(ERXRDPTH, High((ushort)(_nextPacketPointer - 1)));
					}

					BfsReg(ECON2, ECON2_PKTDEC);

					WriteReg(ERDPTL, Low(_nextPacketPointer));
					WriteReg(ERDPTH, High(_nextPacketPointer));

					//frame = null;
					return null; //frame;
				}

				frame = ReadBuffer(receivedByteCount - 4);

				var sourceAndDestinationAreSame = (frame[0] == frame[6] && frame[1] == frame[7] && frame[2] == frame[8] &&
													frame[3] == frame[9] && frame[4] == frame[10] && frame[5] == frame[11]);
				var isValidBroadcast = isBroadcast && frame[0] == 0xFF && frame[1] == 0xFF && frame[2] == 0xFF && frame[3] == 0xFF &&
										frame[4] == 0xFF && frame[5] == 0xFF;
				var isValidMulticast = isMulticast && frame[0] == 0x01 && frame[1] == 0x00 && frame[2] == 0x5e;
				var isValidUnicast = !isBroadcast && MacAddress[0] == frame[0]
									  && MacAddress[1] == frame[1]
									  && MacAddress[2] == frame[2]
									  && MacAddress[3] == frame[3]
									  && MacAddress[4] == frame[4]
									  && MacAddress[5] == frame[5];

				if (sourceAndDestinationAreSame)
				{
					#if TINYCLR_TRACE
					if (_verboseDebugging) Debug.WriteLine("Source and Destination MAC: " + new[] { frame[6], frame[7], frame[8], frame[9], frame[10], frame[11] }.ToHexString()); 
					#endif
				}

				#if TINYCLR_TRACE
				if (_verboseDebugging)
					Debug.WriteLine("Source MAC: " + new[] { frame[6], frame[7], frame[8], frame[9], frame[10], frame[11] }.ToHexString() +
									" -- Destination MAC: " +
									new[] { frame[0], frame[1], frame[2], frame[3], frame[4], frame[5] }.ToHexString()); 
				#endif

				if ((_nextPacketPointer - 1) < RxStart || (_nextPacketPointer - 1) > RxStop)
				{
					#if TINYCLR_TRACE
					if (_verboseDebugging) Debug.WriteLine("Read Pointer set to STOP"); 
					#endif

					WriteReg(ERXRDPTL, Low(RxStop));
					WriteReg(ERXRDPTH, High(RxStop));
					WriteReg(ERXRDPTL, Low(RxStop));
					WriteReg(ERXRDPTH, High(RxStop));
				}
				else
				{
					WriteReg(ERXRDPTL, Low((ushort)(_nextPacketPointer - 1)));
					WriteReg(ERXRDPTH, High((ushort)(_nextPacketPointer - 1)));
					WriteReg(ERXRDPTL, Low((ushort)(_nextPacketPointer - 1)));
					WriteReg(ERXRDPTH, High((ushort)(_nextPacketPointer - 1)));
				}

				BfsReg(ECON2, ECON2_PKTDEC);

				if ((isValidBroadcast || isValidMulticast || isValidUnicast) && !sourceAndDestinationAreSame)
				{
					_lastPacketReceived = Utility.GetMachineTime();
					_regCheckTimer.Change(CheckDelay, CheckDelay);
					return frame;
				}
				return null;
			}
		}

		private void ThrottleFrameReception(byte packetCount)
		{
			byte newFilter;

			if (packetCount < 10)
			{
				newFilter = ERXFCON_UCEN | ERXFCON_BCEN | ERXFCON_PMEN | ERXFCON_CRCEN;
			}
			else if (packetCount < 20)
				newFilter = ERXFCON_UCEN | ERXFCON_PMEN | ERXFCON_CRCEN;
			else
				newFilter = ERXFCON_UCEN | ERXFCON_CRCEN;

			if (newFilter != _lastFilter)
			{
				#if TINYCLR_TRACE
				if (_verboseDebugging) Debug.WriteLine("Throttle set to filter: " + (packetCount < 10 ? "None" : (packetCount < 20 ? "All Broadcasts" : "All but directly addressed packets"))); 
				#endif

				WriteReg(ERXFCON, newFilter);
				_lastFilter = newFilter;
			}
		}

		private void ReceiveReset()
		{
			if (Utility.GetMachineTime() < _lastReceiveReset.Add(new TimeSpan(0, 0, 5)) || Utility.GetMachineTime().Subtract(_linkUpTime).Ticks < TimeSpan.TicksPerSecond * 5)
			{
				return;
			}

			#if TINYCLR_TRACE
			if (_verboseDebugging) Debug.WriteLine(DateTime.Now + " Executing a RECEIVE RESET! "); 
			#endif

			_rxResetPending = true;

			_lastReceiveReset = Utility.GetMachineTime();

			BfsReg(ECON1, ECON1_RXRST);
			Thread.Sleep(1);

			var timeout = 100;

			while ((ReadCommonReg(ESTAT) & ESTAT_RXBUSY) != 0 && timeout-- > 0) Thread.Sleep(2);

			timeout = 100;

			while ((ReadCommonReg(ECON1) & ECON1_TXRTS) != 0 && timeout-- > 0) Thread.Sleep(2);

#if TINYCLR_TRACE
			if (_verboseDebugging)
			{
				Debug.WriteLine("Dropping " + ReadEthReg(EPKTCNT) + " packet(s)");
				Debug.WriteLine("Packet Count is 2: " + ReadEthReg(EPKTCNT) + "  packet(s)");
			} 
#endif

			BfcReg(EIR, EIR_RXERIF);

			SetupReceiveBuffer();

			#if TINYCLR_TRACE

			var trycount = 256;

			if (_verboseDebugging)
			{

				Debug.WriteLine("Packet Count is 3: " + ReadEthReg(EPKTCNT));

				Debug.WriteLine("1*** ERXRDPT: " + ((ushort)(ReadEthReg(ERXRDPTL) | ReadEthReg(ERXRDPTH) << 8)));
				Debug.WriteLine("1*** ERXWRPT: " + ((ushort)(ReadEthReg(ERXWRPTL) | ReadEthReg(ERXWRPTH) << 8)));

				while (ReadEthReg(EPKTCNT) > 0 && trycount-- > 0) BfsReg(ECON2, ECON2_PKTDEC);

				Thread.Sleep(2);

				Debug.WriteLine("2*** ERXRDPT: " + ((ushort)(ReadEthReg(ERXRDPTL) | ReadEthReg(ERXRDPTH) << 8)));
				Debug.WriteLine("2*** ERXWRPT: " + ((ushort)(ReadEthReg(ERXWRPTL) | ReadEthReg(ERXWRPTH) << 8)));

				Debug.WriteLine("Packet Count is 4: " + ReadEthReg(EPKTCNT));
			} 
			#endif

			_nextPacketPointer = 0;

			#if TINYCLR_TRACE
			if (_verboseDebugging)
			{
				Debug.WriteLine("Packet Count is 5: " + ReadEthReg(EPKTCNT));
				Debug.WriteLine("3*** ERXRDPT: " + ((ushort)(ReadEthReg(ERXRDPTL) | ReadEthReg(ERXRDPTH) << 8)));
				Debug.WriteLine("3*** ERXWRPT: " + ((ushort)(ReadEthReg(ERXWRPTL) | ReadEthReg(ERXWRPTH) << 8)));
			} 
			#endif

			BfcReg(ECON1, ECON1_RXRST);

			Thread.Sleep(2);

			WritePhyReg(PHIE, 0x0012);

			WriteReg(EIE, EIE_PKTIE | EIE_INTIE | EIE_LINKIE | EIE_RXERIE);

			BfsReg(ECON1, ECON1_RXEN);

			_rxResetPending = false;

			#if TINYCLR_TRACE
			if (_verboseDebugging)
			{
				Debug.WriteLine("4*** ERXRDPT: " + ((ushort)(ReadEthReg(ERXRDPTL) | ReadEthReg(ERXRDPTH) << 8)));
				Debug.WriteLine("4*** ERXWRPT: " + ((ushort)(ReadEthReg(ERXWRPTL) | ReadEthReg(ERXWRPTH) << 8)));
				Debug.WriteLine("4*** Setting Next Packet Pointer to: " + _nextPacketPointer);
			} 
			#endif
		}

		private static byte[] ReadBuffer(int len)
		{
			lock (BankAccess)
			{
				var result = new byte[len];
				Hardware.SPIBus.WriteRead(_spiConfiguration, new[] { ReadBufferMemory }, 0, 1, result, 0, len, 1);

				//SPI.WriteRead( new []{ReadBufferMemory }, 0, 1, result, 0, len, 1);

				return result;
			}
		}

		private static void WriteBuffer(byte[] data, int startIndex = 0)
		{
			var status = new byte[0];

			if (startIndex == 2)
			{
				lock (BankAccess)
				{
					data[startIndex - 2] = WriteBufferMemnory;
					data[startIndex - 1] = 0x00;
					Hardware.SPIBus.WriteRead(_spiConfiguration, data, status);
					//SPI.WriteRead(data, status);
				}
			}
			else if (startIndex == 0)
			{
				lock (BankAccess)
				{
					Hardware.SPIBus.WriteRead(_spiConfiguration, Utility.CombineArrays(new byte[] { WriteBufferMemnory, 0x00 }, data), status);
					//SPI.WriteRead(Utility.CombineArrays(new byte[] { WriteBufferMemnory, 0x00 }, data), status);
				}
			}
			else
			{
				lock (BankAccess)
				{
					Hardware.SPIBus.WriteRead(_spiConfiguration, Utility.CombineArrays(new byte[] { WriteBufferMemnory, 0x00 }, 0, 2, data, startIndex, data.Length - startIndex), status);
					//SPI.WriteRead(Utility.CombineArrays(new byte[] { WriteBufferMemnory, 0x00 }, 0, 2, data, startIndex, data.Length - startIndex), status);
				}
			}
		}

		private static void SendSystemReset()
		{
			lock (BankAccess)
			{
				Hardware.SPIBus.WriteRead(_spiConfiguration, new byte[] { 0xFF }, 0, 1, null, 0, 0, 1);
				//SPI.WriteRead(new byte[] { 0xFF }, 0, 1, null, 0, 0, 1);
			}
			Thread.Sleep(5);
		}

		private static byte ReadEthReg(ushort address)
		{
			lock (BankAccess)
			{
				CurrentBank = address;

				SpiBuffer[0] = (byte)(ReadControlRegister | (address & 0x1F));
				Hardware.SPIBus.WriteRead(_spiConfiguration, SpiBuffer, 0, 2, SpiBuffer, 0, 2, 0);
				//SPI.WriteRead(SpiBuffer, 0, 2, SpiBuffer, 0, 2, 0);

				return SpiBuffer[1];
			}
		}

		private static byte ReadCommonReg(byte address)
		{
			lock (BankAccess)
			{
				SpiBuffer[0] = (byte)(ReadControlRegister | (address & 0x1F));
				Hardware.SPIBus.WriteRead(_spiConfiguration, SpiBuffer, 0, 2, SpiBuffer, 0, 2, 0);
				//SPI.WriteRead(SpiBuffer, 0, 2, SpiBuffer, 0, 2, 0);

				return SpiBuffer[1];
			}
		}

		private static byte ReadMacReg(ushort address)
		{
			lock (BankAccess)
			{
				CurrentBank = address;

				SpiBuffer[0] = (byte)(ReadControlRegister | (address & 0x1F));
				SpiBuffer[1] = 0x00;
				Hardware.SPIBus.WriteRead(_spiConfiguration, SpiBuffer, 0, 2, SpiBuffer, 0, 2, 1);
				//SPI.WriteRead(SpiBuffer, 0, 2, SpiBuffer, 0, 2, 1);

				return SpiBuffer[1];
			}
		}

		private static ushort ReadPhyReg(byte register)
		{
			WriteReg(MIREGADR, register);
			WriteReg(MICMD, MICMD_MIIRD);

			var timeout = 100;

			while ((ReadMacReg(MISTAT) & MISTAT_BUSY) != 0 && timeout-- > 0) Thread.Sleep(2);

			WriteReg(MICMD, 0x00);
			ushort result = (new[] { ReadMacReg(MIRDH), ReadMacReg(MIRDL) }).ToShort();

			return result;
		}

		private static void WriteReg(ushort address, byte data)
		{
			lock (BankAccess)
			{
				CurrentBank = address;

				Hardware.SPIBus.WriteRead(_spiConfiguration, new[] { (byte)(WriteControlRegister | (address & 0x1F)), data }, 0, 2, null, 0, 0, 2);
				//SPI.WriteRead(new[] { (byte)(WriteControlRegister | (address & 0x1F)), data }, 0, 2, null, 0, 0, 2);
			}
		}

		private static void BfcReg(byte address, byte data)
		{
			lock (BankAccess)
			{
				Hardware.SPIBus.WriteRead(_spiConfiguration, new[] { (byte)(BitFieldClear | address), data }, 0, 2, null, 0, 0, 2);
				//SPI.WriteRead(new[] { (byte)(BitFieldClear | address), data }, 0, 2, null, 0, 0, 2);
			}
		}

		private static void BfsReg(byte address, byte data)
		{
			lock (BankAccess)
			{
				Hardware.SPIBus.WriteRead(_spiConfiguration, new[] { (byte)(BitFieldSet | address), data }, 0, 2, null, 0, 0, 2);
				//SPI.WriteRead(new[] { (byte)(BitFieldSet | address), data }, 0, 2, null, 0, 0, 2);
			}
		}

		private static void WritePhyReg(byte register, ushort data)
		{
			WriteReg(MIREGADR, register);
			WriteReg(MICMD, 0);
			WriteReg(MIWRL, (byte)(data >> 0));
			WriteReg(MIWRH, (byte)(data >> 8));

			var timeout = 100;
			while ((ReadMacReg(MISTAT) & MISTAT_BUSY) != 0 && timeout-- > 0) Thread.Sleep(1);
		}

		private static void MacPowerDown()
		{
			BfcReg(ECON1, ECON1_RXEN);

			var timeout = 100;

			while ((ReadCommonReg(ESTAT) & ESTAT_RXBUSY) != 0 && timeout-- > 0) Thread.Sleep(2);

			timeout = 100;

			while ((ReadCommonReg(ECON1) & ECON1_TXRTS) != 0 && timeout-- > 0) Thread.Sleep(2);

			BfsReg(ECON2, ECON2_PWRSV);

			_resetPin.Write(false);

			if (OnConnectionChanged != null) OnConnectionChanged(false);
		}

		private static void MacPowerUp()
		{
			_resetPin.Write(true);

			BfcReg(ECON2, ECON2_PWRSV);

			while ((ReadCommonReg(ESTAT) & ESTAT_CLKRDY) != 0) Thread.Sleep(100);

			BfsReg(ECON1, ECON1_RXEN);

			if (OnConnectionChanged != null )OnConnectionChanged(true);
		}

		private static void PollNow(object o)
		{
			if (_linkUpTime < TimeSpan.MaxValue && _resetPin.Read())

			{
				if (!_dhcpDisabled && IsRenewing && _ip != null)
				{
					DHCP.SendMessage(DHCP.Request);
				}
				else if (!_dhcpDisabled && _ip == null)
				{
					DHCP.SendMessage(DHCP.Discover);
				}

				if (_ip != null && _defaultGateway != null && _gatewayMac == null) ARP.SendARP_Probe(_defaultGateway);
				if (_ip != null) ARP.SendARP_Gratuitus();
			}
		}

		private static void Stop()
		{
			#if TINYCLR_TRACE
			if (_verboseDebugging) Debug.WriteLine("Stopping Network"); 
			#endif

			try
			{
				PollingTimer.Change(Timeout.Infinite, Timeout.Infinite);
				_ip = null;
			}
			catch (Exception ex)
			{
				throw new Exception("Error occurred while stopping Networking. Message - " + ex.Message);
			}
		}

		#endregion

		#region Internal Methods
		
		internal void Restart()
		{
			if (_lastRestart > Utility.GetMachineTime().Subtract(new TimeSpan(0, 0, 7))) return;

			_watchdog.Change(10000, 10000); 

			_lastRestart = Utility.GetMachineTime();

			SendSystemReset();

			Thread.Sleep(100);

			Init();
		}

		internal void Init()
		{
			#if TINYCLR_TRACE
			if (_verboseDebugging) Debug.WriteLine("Initialization method invoked."); 
			#endif

			_initialized = false;

			SetupReceiveBuffer();
			SetupTransmitBuffer();

			SetupMacFilters();

			WriteReg(MACON1, MACON1_TXPAUS | MACON1_RXPAUS | MACON1_MARXEN);
			WriteReg(MACON2, 0x00);
			WriteReg(MACON3, MACON3_PADCFG0 | MACON3_TXCRCEN | MACON3_FRMLNEN);
			WriteReg(MABBIPG, 0x12);
			WriteReg(MACON4, MACON4_DEFER | MACON4_PUREPRE);
			WriteReg(MACLCON2, 63);
			WriteReg(MAIPGL, 0x12);
			WriteReg(MAIPGH, 0x0C);
			WriteReg(MAMXFLL, Low(MaxFrameSize)); 
			WriteReg(MAMXFLH, High(MaxFrameSize));
			WriteReg(MAADR1, _macAddress[0]);
			WriteReg(MAADR2, _macAddress[1]);
			WriteReg(MAADR3, _macAddress[2]);
			WriteReg(MAADR4, _macAddress[3]);
			WriteReg(MAADR5, _macAddress[4]);
			WriteReg(MAADR6, _macAddress[5]);
			WriteReg(ECOCON, 0x00); 
			WritePhyReg(PHCON2, PHCON2_HDLDIS);
			WritePhyReg(PHIE, 0x0012);
			WritePhyReg(PHCON1, 0x0000);

			_resetPending = false;
			_initialized = true;

			_irq.ClearInterrupt();

			WriteReg(EIE, EIE_PKTIE | EIE_INTIE | EIE_LINKIE | EIE_RXERIE | EIE_TXERIE);

			BfsReg(ECON2, ECON2_AUTOINC);

			BfsReg(ECON1, ECON1_RXEN);

			#if TINYCLR_TRACE
			if (_verboseDebugging) Debug.WriteLine("Packet Reception enabled."); 
			#endif
		}

		internal void UpdateLinkState(bool force = false)
		{
			var eir = ReadCommonReg(EIR);

			if (_linkInit == false || (eir & EIR_LINKIF) != 0 || force) 
			{
				var linkUp = (ReadPhyReg(PHSTAT2) & PHSTAT2_LSTAT) != 0;
				ReadPhyReg(PHIR); 

				if (linkUp && _linkUpTime == TimeSpan.MaxValue)
				{
					_linkUpTime = Utility.GetMachineTime();
					if (OnLinkChanged != null) OnLinkChanged.Invoke(this, DateTime.Now, true);
				}
				else if (!linkUp && _linkUpTime != TimeSpan.MaxValue)
				{
					_linkUpTime = TimeSpan.MaxValue;
					if (OnLinkChanged != null) OnLinkChanged.Invoke(this, DateTime.Now, false);
				}

				_linkInit = true;
			}
		}

		internal static void SendFrame(byte[] frame, int startIndex = 0)
		{
			lock (SendAccess)
			{
				WriteReg(EWRPTL, Low(TxStart));
				WriteReg(EWRPTH, High(TxStart));
				WriteBuffer(frame, startIndex);
				WriteReg(ETXNDL, Low((ushort) (TxStart + frame.Length)));
				WriteReg(ETXNDH, High((ushort) (TxStart + frame.Length)));

				BfsReg(ECON1, ECON1_TXRTS);

				int timeout = 100;

				while ((ReadCommonReg(ECON1) & ECON1_TXRTS) != 0 && timeout-- > 0) Thread.Sleep(5);

				if ((ReadCommonReg(EIR) & EIR_TXERIF) != 0) BfcReg(ECON1, ECON1_TXRTS); 
			}
		}

		#endregion

		#region Events

		#region Public

		#region Public Event Declarations

		/// <summary>
		/// This event fires when the network cable connection changes.
		/// </summary>
		public static event ConnectionStatusChangedEventHandler OnConnectionChanged;

		/// <summary>
		/// This event fires when the network stack receives a HTTP Packet.
		/// </summary>
		public static event HttpPacketReceivedEventHandler OnHttpPacketReceived;

		/// <summary>
		/// This event fires when the network stack receives a TCP Packet.
		/// </summary>
		public static event TcpPacketReceivedEventHandler OnTcpPacketReceived;

		/// <summary>
		/// This event fires when the network stack receives a UDP Packet.
		/// </summary>
		public static event UdpPacketReceivedEventHandler OnUdpPacketReceived;

		/// <summary>
		/// The event that is raised when a Ping Request is received by the EthClick.
		/// </summary>
		public static event PingReceivedEventHandler OnPingReceived;

		#endregion

		#region Public Event Delegate Declarations

		/// <summary>
		/// The delegate used for the <see cref="OnConnectionChanged"/> event.
		/// </summary>
		/// <param name="status">The link status. True is the Link is connected or otherwise false.</param>
		public delegate void ConnectionStatusChangedEventHandler(bool status);

		/// <summary>
		/// The delegate used for the <see cref="OnHttpPacketReceived"/> event.
		/// </summary>
		/// <param name="request">The HTTP <see cref="Packet"/> returned in the <see cref="EthClick.OnHttpPacketReceived"/> event.</param>
		public delegate void HttpPacketReceivedEventHandler(HttpRequest request);

		/// <summary>
		/// The delegate used for the <see cref="EthClick.OnTcpPacketReceived"/> event.
		/// </summary>
		/// <param name="packet">The TCP <see cref="Packet"/> returned in the <see cref="EthClick.OnTcpPacketReceived"/> event.</param>
		public delegate void TcpPacketReceivedEventHandler(Packet packet);

		/// <summary>
		/// The delegate used for the <see cref="EthClick.OnUdpPacketReceived"/> event.
		/// </summary>
		/// <param name="packet">The UDP <see cref="Packet"/> returned in the <see cref="EthClick.OnUdpPacketReceived"/> event.</param>
		public delegate void UdpPacketReceivedEventHandler(Packet packet);

		/// <summary>
		/// The delegate used for the <see cref="EthClick.OnPingReceived"/> event.
		/// </summary>
		/// <param name="originatingMac">The MAC Address of the device that originated the Ping Request.</param>
		/// <param name="originatingIp">The MAC Address of the device that originated the Ping Request.</param>
		/// <param name="id">The ID of the Ping Request.</param>
		/// <param name="seq">The Sequence Number of the Ping Request.</param>
		public delegate void PingReceivedEventHandler(byte[] originatingMac, byte[] originatingIp, byte[] id, byte[] seq);

		#endregion

		#endregion

		#region Internal 
		
		#region Internal Events	Declarations

		// The internal event that fires when one or more incoming frames have arrived from the network.
		internal event FrameArrivedEventHandler OnFrameArrived;

		// The internal event that fires when the Ethernet link goes up or down.
		internal event LinkChangedEventHandler OnLinkChanged;

		#endregion

		#region Internal Event Delegate Declarations

		/// <summary>
		/// The delegate method used by the <see cref="OnFrameArrived" /> event for incoming Ethernet frames
		/// </summary>
		/// <param name="sender">The EthClick instance from which the event originated.</param>
		/// <param name="timeReceived">Time when the event occurred.</param>
		/// <param name="frame"></param>
		internal delegate void FrameArrivedEventHandler(EthClick sender, byte[] frame, DateTime timeReceived);

		/// <summary>
		/// The delegate method used by the <see cref="OnLinkChanged" /> event for link status changes.
		/// </summary>
		/// <param name="sender">The EthClick instance from which the event originates</param>
		/// <param name="time">Time when the event occurred</param>
		/// <param name="isUp">Indicates link status</param>
		internal delegate void LinkChangedEventHandler(EthClick sender, DateTime time, bool isUp);

		#endregion

		#region Internal Event Methods

		internal void NicOnLinkChanged(EthClick sender, DateTime time, bool isUp)
		{
			#if TINYCLR_TRACE
			if (_verboseDebugging) Debug.WriteLine("Link is now " + (isUp ? "up :)" : "down :(")); 
			#endif

			if (OnConnectionChanged != null) OnConnectionChanged((isUp));

			if (isUp && (_ip == null || !_dhcpDisabled))
			{
				PollingTimer.Change(500, 10000);
			}
			else if (isUp && _ip != null && _dhcpDisabled && DefaultGateway != null)
			{
				PollingTimer.Change(500, 10000);
			}
			else if (!isUp && !_dhcpDisabled)
			{
				IsRenewing = true;
				PollingTimer.Change(500, 7000);
			}
		}

		internal void nic_OnFrameArrived(EthClick sender, byte[] frame, DateTime timeReceived)
		{
			if (frame == null) return;

			if (frame[13] == 0x06 && frame[12] == 0x08)
			{
				if (_ip != null)
				{
					if (frame[41] == _ip[3] && frame[40] == _ip[2] && frame[39] == _ip[1] && frame[38] == _ip[0])
					{
						#if TINYCLR_TRACE
						if (_verboseDebugging) Debug.WriteLine("Received ARP Packet -- " + frame.Length + " bytes"); 
						#endif
						ARP.HandlePacket(frame);
					}
					else if (frame[21] == 0x02 && frame[31] == _ip[3] && frame[30] == _ip[2] && frame[29] == _ip[1] && frame[28] == _ip[0])
					{
						#if TINYCLR_TRACE
						if (_verboseDebugging) Debug.WriteLine("Possible IP Address Conflict Detected. Stopping networking."); 
						#endif

						Stop();

						#if ! TINYCLR_TRACE
						throw new Exception("Possible IP Address Conflict Detected. Stopping networking.");
						#endif
					}
				}
			}
			else if (frame[13] == 0x00 && frame[12] == 0x08)
			{
				if (frame[23] == 0x01)
				{
					#if TINYCLR_TRACE
					if (_verboseDebugging) Debug.WriteLine("Received ICMP (Ping) Packet -- " + frame.Length + " bytes"); 
					#endif

					ICMP.HandlePacket(frame);
				}
				else if (frame[23] == 0x11)
				{
					if (frame[37] == 0x44 && !_dhcpDisabled && frame[36] == 0x00)
					{
						#if TINYCLR_TRACE
						if (_verboseDebugging) Debug.WriteLine("Received DHCP Packet -- " + frame.Length + " bytes"); 
						#endif
						DHCP.HandlePacket(frame);
					}
					else if (frame[37] == 0x89 && frame[36] == 0x00 && Name != null && Name != string.Empty && _ip != null)
					{
						#if TINYCLR_TRACE
						if (_verboseDebugging) Debug.WriteLine("Received NBNS Packet -- " + frame.Length + " bytes"); 
						#endif
						NetBiosNaming.HandlePacket(frame);
					}
					else if (frame[35] == 0x35 && frame[34] == 0x00)  
					{
						#if TINYCLR_TRACE
						if (_verboseDebugging) Debug.WriteLine("Received DNS Packet -- " + frame.Length + " bytes"); 
						#endif

						DNS.HandlePacket(frame);
					}
					else if (frame[37] == 0xe9 && frame[36] == 0x14 && frame[35] == 0xe9 && frame[34] == 0x14 && Name != null && Name != string.Empty && _ip != null) // mDNS Source and Destination Port of 5353 or LLMNR Destination Port of 5355
					{
						#if TINYCLR_TRACE
						if (_verboseDebugging) Debug.WriteLine("Received MDNS Packet -- " + frame.Length + " bytes"); 
						#endif
						MDNS.HandlePacket(frame);
					}
					else if (frame[37] == 0xeb && frame[36] == 0x14 && Name != null && Name != string.Empty && _ip != null)
					{
						#if TINYCLR_TRACE
						if (_verboseDebugging) Debug.WriteLine("Received LLMNR Packet -- " + frame.Length + " bytes"); 
						#endif

						LLMNR.HandlePacket(frame);
					}
					else if (OnUdpPacketReceived != null && _ip != null)
					{
						#if TINYCLR_TRACE
						if (_verboseDebugging) Debug.WriteLine("Received UDP Packet -- " + frame.Length + " bytes"); 
						#endif

						foreach (byte[] aPort in ListeningPorts.Values)
						{
							if (aPort[0] == frame[36] && aPort[1] == frame[37]) UDP.HandlePacket(frame);
						}
					}
				}
				else if (frame[23] == 0x06 && _ip != null)
				{
					#if TINYCLR_TRACE
					if (_verboseDebugging) Debug.WriteLine("Received TCP Packet -- " + frame.Length + " bytes"); 
					#endif

					foreach (byte[] aPort in ListeningPorts.Values)
					{
						if (aPort[0] == frame[36] && aPort[1] == frame[37])
						{
							TCP.HandlePacket(frame);
							return;
						}
					}

					var conId = TCP.GenerateConnectionId(frame);
					if (TCP._connections.Contains(conId)) TCP.HandlePacket(frame);

				}
			}
		}

		internal static void FireTcpPacketEvent(byte[] packet, uint seqNumber, Connection socket)
		{
			if (OnTcpPacketReceived != null) OnTcpPacketReceived.Invoke(new Packet(PacketType.TCP) { SequenceNumber = seqNumber, Content = packet, Socket = socket });
		}

		internal static void FireHttpPacketEvent(byte[] packet, Connection socket)
		{
				if (OnHttpPacketReceived != null) OnHttpPacketReceived.Invoke(new HttpRequest(packet, socket));
		}

		internal static void FireUdpPacketEvent(byte[] packet, Connection socket)
		{
			if (OnUdpPacketReceived != null) OnUdpPacketReceived.Invoke(new Packet(PacketType.UDP) { Content = packet, Socket = socket });
		}

		internal static void FirePingReceivedEvent(byte[] originatingMac, byte[] originatingIp, byte[] id, byte[] seq)
		{
			if (OnPingReceived != null) OnPingReceived.Invoke(originatingMac, originatingIp, id, seq);
		}

		#endregion

		#endregion		
		
		#endregion

	}
}