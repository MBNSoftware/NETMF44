using System;
using System.Collections;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using MBN;
using System.Threading;
using MBN.Exceptions;
using MBN.Modules;
using Microsoft.SPOT;

namespace Examples
{
    public class Program
    {
        private static GPS3Click gps;
        private static int baud = 9600;

        public static void Main()
        {
            try
            {
                gps = new GPS3Click(Hardware.SocketTwo, (GPS3Click.BaudRates) baud);
            }
            catch (DeviceInitialisationException ex)
            {
                Debug.Print(ex.Message);
                throw new DeviceInitialisationException("Unable to initialize GPS3 Click");
            }

            #region General Events

            gps.AntennaStatusChanged += gps_AntennaStatusChanged;
            gps.NMEASentenceReceived += gps_NMEASentenceReceived;
            gps.PMTKSentenceReceived += gps_PMTKSentenceReceived;
            gps.SerialError += gps_SerialError;

            #endregion

            #region RMC Specific Events

            gps.RMCSentenceReceived += gps_RMCSentenceReceived;
            gps.RMCSpeedChanged += gps_RMCSpeedChanged;
            gps.RMCCoordinatesChanged += gps_RMCCoordinatesChanged;
            gps.RMCCourseChanged += gps_RMCCourseChanged;
            gps.RMCPositioningModeChanged += gps_RMCPositioningModeChanged;

            #endregion

            #region GSV Specific Events

            gps.GSVSentenceReceived += gps_GSVSentenceReceived;
            gps.SatellitesInViewChanged += gps_SatellitesInViewChanged;

            #endregion

            #region GSA Specific Events

            gps.GSASentenceReceived += gps_GSASentenceReceived;
            gps.SatellitesUsedChanged += gps_SatellitesUsedChanged;
            gps.GNSSFixModeChanged += gps_GNSSFixModeChanged;
            gps.GNSSFixStatusChanged += gps_GNSSFixStatusChanged;

            #endregion

            #region VTG Specific Events

            gps.VTGSentenceReceived += gps_VTGSentenceReceived;
            gps.VTGSpeedChanged += gps_VTGSpeedChanged;
            gps.VTGCourseChanged += gps_VTGCourseChanged;
            gps.VTGPositioningModeChanged += gps_VTGPositioningModeChanged;

            #endregion

            #region GLL Specific Events

            gps.GLLSentenceReceived += gps_GLLSentenceReceived;
            gps.GLLCoordinatesChanged += gps_GLLCoordinatesChanged;
            gps.GLLPositioningModeChanged += gps_GLLPositioningModeChanged;

            #endregion

            #region GGA Specific Events

            gps.GGASentenceReceived += gps_GGASentenceReceived;
            gps.GGACoordinatesChanged += gps_GGACoordinatesChanged;

            #endregion

            var nmeaSuscription = new GPS3Click.NMEASubscriptions { RMCEnabled = true, RMCFixesPerSentence = 10, GSVEnabled = true, GSVFixesPerSentence = 10, GSAEnabled = true, GSAFixesPerSentence = 10, VTGEnabled = true, VTGFixesPerSentence = 10, GLLEnabled = true, GLLFixesPerSentence = 10, GGAEnabled = true, GGAFixesPerSentence = 10, CHNEnabled = false, CHNFixesPerSentence = 0, ZDAEnabled = false, ZDAFixesPerSentence = 0 };
            //var nmeaSuscription = new GPS3Click.NMEASubscriptions { GLLEnabled = true, GLLFixesPerSentence = 10 };
            
            Debug.Print("NMEA Set? " + gps.SetNMEASubscriptions(nmeaSuscription));
            Debug.Print("NMEA Subscriptions? " + gps.QueryNMEASubscriptions());

            Debug.Print("NMEASubscription Subscription Level - " + gps.Subscriptions.ToString());
            Debug.Print("GPS Firmware Information - " + gps.GPSFirmwareInfo);


            //Debug.Print("Full Cold Start");
            //gps.PerformRestart(GPS3Click.RestartType.FullCold);

            gps.StartGPS();

            #region Test Code

            //Debug.Print("Set BaudRate to 4800 ? " + gps.SetBaudRate(GPS3Click.BaudRates.Baud_4800));
            //Thread.Sleep(5000);
            //Debug.Print("Set BaudRate to 14400? " + gps.SetBaudRate(GPS3Click.BaudRates.Baud_14400));
            //Thread.Sleep(5000);
            //Debug.Print("Set BaudRate to 19200? " + gps.SetBaudRate(GPS3Click.BaudRates.Baud_19200));
            //Thread.Sleep(5000);
            //Debug.Print("Set BaudRate to 38400? " + gps.SetBaudRate(GPS3Click.BaudRates.Baud_38400));
            //Thread.Sleep(5000);
            //Debug.Print("Set BaudRate to 57600? " + gps.SetBaudRate(GPS3Click.BaudRates.Baud_57600));
            //Thread.Sleep(5000);
            //Debug.Print("Set BaudRate to 115200? " + gps.SetBaudRate(GPS3Click.BaudRates.Baud_115200));
            //Thread.Sleep(50000);
            //Debug.Print("Set BaudRate  to 9600? " + gps.SetBaudRate(GPS3Click.BaudRates.Baud_9600));

           
            //Debug.Print("Version Information  from Query Method - " + gps.QueryFirmwareInfo());
            //Debug.Print("Version Information from Property Info - " + gps.GPSFirmwareInfo);


            //gps.PerformColdStart();

           gps.PerformRestart(GPS3Click.RestartType.Warm);

            //if(gps.SetStaticNavigationThreshold(0.2)) throw new DeviceInitialisationException();

            //gps.SetBaudRate(GPS3Click.BaudRates.Baud_115200);

            //gps.Serial.Close();
            //gps.Connect(GPS3Click.BaudRates.Baud_115200);

            //gps.SendCommand("$PMTK314,5,5,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,5,5");

            //gps.SetNMEASubscriptions(5, 0, 0, 0, 0, 0, 0, 0);

            //gps.SendCommand("$PMTK386,1,0"); // Set the speed threshold for static navigation. Any value below 2.0 is this case will be reported as 0 knots by the GPS3 Click.
            Debug.Print("Set BaudRate to 115200? " + gps.SetBaudRate(GPS3Click.BaudRates.Baud_115200)); 
            Debug.Print("New Baud Rate? " + gps.BaudRate);
            //gps.SendCommand("$PMTK869,1,0");
            //Debug.Print("Setting EasyMode to false - " + gps.EnableEasy(false));
            //Debug.Print("Easy Query for disabled - " + gps.QueryEasyStatus(false)); //gps.SendCommand("$PMTK869,0");

            //gps.SendCommand("$PMTK286,0");
            //gps.SendCommand("$PMTK000");    // Test Message This message is a test packet used to test serial communication with the GNSS module.
            // Upon receiving this packet the module will respond with the packet message: $PMTK001,0,3*30<CR><LF>
            // This works :)

            //gps.SendCommand("$PMTK300,1000,0,0,0,0");
            //gps.SendCommand("$PMTK400");


            //var response = gps.QueryNMEASubscriptions();
            //Debug.Print("RMC Enabled ? " + response.RMCEnabled);
            //Debug.Print("RMC Fixes per sentences ? " + response.RMCFixesPerSentence);

            //Debug.Print("Set Navigation Threshold ? " + gps.SetStaticNavigationThreshold(1.1));
            //Debug.Print("Query Navigation Threshold ? " + gps.QueryStaticNavigationthreshold().ToString("F2"));

            //Debug.Print("Position Fix Set? " + gps.SetPositionFixControlRate(1000));
            //var fixControlRate = gps.QueryPositionFixControlRate();
            //Debug.Print("Fix Control Rate ? " + fixControlRate);

            //Debug.Print("Set AIC Enabled? " + gps.EnableAIC(true));
            //Debug.Print("AIC Enabled ? " + gps.AICEnabled);

            //Debug.Print("EASY Enabled? " + gps.QueryEasyStatus());
            //Debug.Print("Set EASY Disabled? " + gps.EnableEasy(false));
            //Debug.Print("EASY Enabled? " + gps.QueryEasyStatus());
            //Debug.Print("Set EASY Enabled? " + gps.EnableEasy(true));
            //Debug.Print("EASY Enabled? " + gps.QueryEasyStatus());

            //Debug.Print("SBAS Enabled? " + gps.QuerySBASStatus());
            //Debug.Print("Set SBAS Disabled? " + gps.EnableSBAS(false));
            //Debug.Print("SBAS Enabled? " + gps.QuerySBASStatus());
            //Debug.Print("Set SBAS Enabled? " + gps.EnableSBAS(true));
            //Debug.Print("SBAS Enabled? " + gps.QuerySBASStatus());

            //Debug.Print("DPGS Enabled? " + gps.QueryDPGSStatus());
            //Debug.Print("Set DPGS Disabled? " + gps.EnableDPGS(false));
            //Debug.Print("DPGS Enabled? " + gps.QueryDPGSStatus());
            //Debug.Print("Set DPGS Enabled? " + gps.EnableDPGS(true));
            //Debug.Print("DPGS Enabled? " + gps.QueryDPGSStatus());

            //Debug.Print("Set POS Fix Interval? " + gps.SetPositionFixInterval(1000));
            //Debug.Print("Query POS Fix Interval ? " + gps.PositionFixInterval);

            //Thread.Sleep(5000);

            //var hdop = gps.QueryHDOPThreshold();
            //Debug.Print("Query HDOP Threshold? " + hdop);
            //Debug.Print("Set HDOP Threshold? " + gps.SetHDOPThreshold(0.5));
            //Debug.Print("New HDOP Threshold? " + gps.QueryHDOPThreshold());
            //Debug.Print("Set HDOP to original? " + gps.SetHDOPThreshold(hdop));
            //Debug.Print("HDOP Threshold? " + gps.QueryHDOPThreshold());

            //Debug.Print("LSC Enabled? " + gps.QueryLSCEnabled());
            //Debug.Print("Set LSC Enabled? " + gps.EnableLSCSentences(true));
            //Debug.Print("LSC Enabled? " + gps.QueryLSCEnabled());
            //Thread.Sleep(20000);
            //Debug.Print("Set LSC Disabled? " + gps.EnableLSCSentences(false));
            //Debug.Print("LSC Enabled? " + gps.QueryLSCEnabled());



            //new Thread(UartMonitor).Start();

            //Thread.Sleep(25000);
            //Debug.Print("Entering standby mode");
            //gps.EnableStandbyMode();
            //Thread.Sleep(25000);
            //Debug.Print("Exiting standby mode");
            //gps.EnableStandbyMode();

            //Thread.Sleep(10000);
            //Debug.Print("Now entering Periodic Mode - Perpetual Backup");
            //gps.SendCommand("$PMTK225,4");
            //Thread.Sleep(25000);
            //Debug.Print("Returning to normal operation");
            //gps.CancelPeriodicMode();

            //Thread.Sleep(2000);
            //Debug.Print("Now entering AlwaysLocate - Backup");
            //Debug.Print("Set AlwaysLocate - Backup? " + gps.SetAlwaysLocateMode(GPS3Click.AlwaysLocateMode.Backup));
            //Thread.Sleep(20000);

            //Debug.Print("Now entering AlwaysLocate - Standby");
            //Debug.Print("Set AlwaysLocate - Standby? " + gps.SetAlwaysLocateMode(GPS3Click.AlwaysLocateMode.Standby));
            //Thread.Sleep(20000);

            //Debug.Print("Now entering AlwaysLocate - Normal");
            //Debug.Print("Set AlwaysLocate - Normal? " + gps.SetAlwaysLocateMode(GPS3Click.AlwaysLocateMode.Normal));


            //Debug.Print("Entering AL - Standby? " + gps.SetAlwaysLocateMode(GPS3Click.AlwaysLocateMode.Standby));
            //Thread.Sleep((5000));

            //Debug.Print("Entering AL - Backup? " + gps.SetAlwaysLocateMode(GPS3Click.AlwaysLocateMode.Backup));
            //Thread.Sleep((5000));

            //Debug.Print("Entering AL - Normal? " + gps.SetAlwaysLocateMode(GPS3Click.AlwaysLocateMode.Normal));
            //Thread.Sleep((5000));

            //Debug.Print("Entering Periodic Mode - Backup? " + gps.SetPeriodicMode(GPS3Click.PeriodicMode.Backup, 25, 5000));
            //Thread.Sleep((5000));

            //Debug.Print("Entering Periodic Mode - Standby? " + gps.SetPeriodicMode(GPS3Click.PeriodicMode.Standby, 25, 5000));
            //Thread.Sleep((5000));

            //Debug.Print("Entering Periodic Mode - Perpetual? " + gps.SetPeriodicMode(GPS3Click.PeriodicMode.Perpetual, 25, 5000));
            //Thread.Sleep((5000));

            //Debug.Print("Entering Periodic Mode - Normal? " + gps.SetPeriodicMode(GPS3Click.PeriodicMode.Normal));
            //Thread.Sleep((5000)); 
            #endregion

            // Put the GPS3 Click in Standby Mode.
            var commandString = "$PMTK161,0*";
            commandString += gps.CalculateChecksum(commandString + "\r\n" + "\r\n");
            var commandBytes = Encoding.UTF8.GetBytes(commandString);
            gps.Serial.Write(commandBytes, 0, commandBytes.Length);

            Debug.Print("Tim since last valid RMC fix - " + gps.LastValidFixAge);

            var status = gps.Antenna;
            Debug.Print("Antenna is " + (status == GPS3Click.AntennaStatus.Ok ? "Open" : status == GPS3Click.AntennaStatus.Ok ? "Ok" : "Shorted"));

            Debug.Print("GPS Fix Position Interval " + gps.PositionFixInterval);
            Debug.Print("Is Easy enabled? " + gps.EasyEnabled);
            Debug.Print("Are LSC ourput enabled? " + gps.LSCEnabled);

            // Send command t ut GPS3 Click in Standby Low Power Mode.
            gps.SendCommand("$PMTK161,0");

            Debug.Print("ChecksumValid? " + gps.IsValidChecksum("$PMTK001,869,3*37"));

            Debug.Print("Set PositionFix Control to 1000 returns " + gps.SetPositionFixControlRate(1000));
            Debug.Print("Position Fix Control Rate is now " + gps.QueryPositionFixControlRate());

            Debug.Print("Enable AIC ?" + gps.EnableAIC(true));
            Debug.Print("AIC enabled ? " + gps.AICEnabled);

            Debug.Print("Enable EASY ?" + gps.EnableEASY(true));
            Debug.Print("EASY enabled ? " + gps.QueryEASYStatus());

            gps.SetStaticNavigationThreshold(1.6);
            Debug.Print("Static Navigatin Threshold s " + gps.QueryStaticNavigationthreshold());
            Debug.Print("GPS3 Click Firmware Inf? " + gps.QueryFirmwareInfo());

            Debug.Print("Set Always Locate Mode to Standby Mode? " + gps.SetAlwaysLocateMode(GPS3Click.AlwaysLocateMode.Standby));

            Debug.Print("Set DEE ?" + gps.SetDEE(4, 26, 160000, 80000));



            Thread.Sleep(Timeout.Infinite);
        }
      
        static void gps_GGACoordinatesChanged(GPS3Click sender, double latitude, double longitude, GPS3Click.Time eventTime)
        {
            if (Debugger.IsAttached) Debug.Print("GGA coordinates changed to Longitude - " + longitude + ", Latitude - " + latitude + " changed at " + eventTime + "\n");
        }

        static void gps_GGASentenceReceived(GPS3Click sender, GPS3Click.GGAData ggaData)
        {
            if (Debugger.IsAttached) Debug.Print("GGA Sentence Information");
            if (Debugger.IsAttached) Debug.Print(ggaData + "\n");
        }

        static void gps_GLLPositioningModeChanged(GPS3Click sender, GPS3Click.PositioningModes positioningMode)
        {
            if (Debugger.IsAttached) Debug.Print("GLL Positioning Mode changed to  " + (positioningMode == GPS3Click.PositioningModes.NoFix ? "No Fix" : positioningMode == GPS3Click.PositioningModes.AutonomousFix ? "Autonomous Fix" : "Differential Fix") + "\n");
        }

        static void gps_GLLCoordinatesChanged(GPS3Click sender, double latitude, double longitude, GPS3Click.Time eventTime)
        {
            if (Debugger.IsAttached) Debug.Print("GLL coordinates changed to Longitude - " + longitude + ", Latitude - " + latitude + " changed at " + eventTime + "\n");
        }

        static void gps_GLLSentenceReceived(GPS3Click sender, GPS3Click.GLLData gllData)
        {
            if (!gllData.DataValid) return;
            if (Debugger.IsAttached) Debug.Print("GLL Sentence Information");
            if (Debugger.IsAttached) Debug.Print(gllData.ToString() + "\n");
        }

        static void gps_VTGPositioningModeChanged(GPS3Click sender, GPS3Click.PositioningModes positioningMode)
        {
            if (Debugger.IsAttached) Debug.Print("VTG Positioning Mode changed to  " + (positioningMode == GPS3Click.PositioningModes.NoFix ? "No Fix" : positioningMode == GPS3Click.PositioningModes.AutonomousFix ? "Autonomous Fix" : "Differential Fix") + "\n");
        }

        static void gps_VTGCourseChanged(GPS3Click sender, double heading)
        {
            if (Debugger.IsAttached) Debug.Print("VTG Course changed to  - " + heading + "\n");
        }

        static void gps_VTGSpeedChanged(GPS3Click sender, double knots, double kph)
        {
            if (Debugger.IsAttached) Debug.Print("VTG Speed Changed to " + knots + " (Knots), " + kph + " (KPH)" + "\n");
        }

        static void gps_VTGSentenceReceived(GPS3Click sender, GPS3Click.VTGData vtgData)
        {
            if (Debugger.IsAttached) Debug.Print("VTG Sentence Information");
            Debug.Print(vtgData + "\n");
        }

        static void gps_RMCPositioningModeChanged(GPS3Click sender, GPS3Click.PositioningModes positioningMode)
        {
            if (Debugger.IsAttached) Debug.Print("RMC Positioning Mode changed to  " + (positioningMode == GPS3Click.PositioningModes.NoFix ? "No Fix" : positioningMode == GPS3Click.PositioningModes.AutonomousFix ? "Autonomous Fix" : "Differential Fix") + "\n");
        }

        static void gps_RMCCourseChanged(GPS3Click sender, double heading, DateTime eventTime)
        {
            if (Debugger.IsAttached) Debug.Print("RMC Course changed to  - " + heading + " at " + eventTime + "\n");
        }

        static void gps_RMCCoordinatesChanged(GPS3Click sender, double latitude, double longitude, DateTime eventTime)
        {
            if (Debugger.IsAttached) Debug.Print("RMC coordinates changed to Longitude - " + longitude + ", Latitude - " + latitude + " changed at " + eventTime + "\n");
        }

        static void gps_RMCSpeedChanged(GPS3Click sender, double knots, double acceleration, double gForce)
        {
            if (Debugger.IsAttached) Debug.Print("RMC speed changed to - " + knots + "(Knots), Acceleration - " + acceleration + "(m/sec^2), G-Force - " + gForce + "(G)");
        }

        static void gps_RMCSentenceReceived(GPS3Click sender, GPS3Click.RMCData rmcData)
        {
            if (!rmcData.DataValid) return;
            if (Debugger.IsAttached) Debug.Print("RMC Sentence Information");
            if (Debugger.IsAttached) Debug.Print(rmcData + "\n");
        }

        static void gps_GNSSFixStatusChanged(GPS3Click sender, GPS3Click.GNSSFixStatus fixStatus)
        {
            if (Debugger.IsAttached) Debug.Print("GNSS Fix Status - " + (fixStatus == GPS3Click.GNSSFixStatus.NoFix ? "No Fix" : fixStatus == GPS3Click.GNSSFixStatus.Fix2D ? "2D Fix" : "3D Fix") + "\n");
        }

        static void gps_GNSSFixModeChanged(GPS3Click sender, GPS3Click.GNSSFixMode fixMode)
        {
            if (Debugger.IsAttached) Debug.Print("GNSS Fix Mode - " + (fixMode == GPS3Click.GNSSFixMode.Auto ? "Auto" : "Manual") + "\n");
        }

        static void gps_GSASentenceReceived(GPS3Click sender, GPS3Click.GSAData gsaData)
        {
            if (Debugger.IsAttached) Debug.Print("GSA Sentence Information");
            if (Debugger.IsAttached) Debug.Print(gsaData.ToString() + "\n");
        }

        static void gps_GSVSentenceReceived(GPS3Click sender, GPS3Click.GSVData gsvData)
        {
            if (Debugger.IsAttached) Debug.Print("GSV Sentence Information");
            if (Debugger.IsAttached) Debug.Print(gsvData + "\n");
        }

        private static int satCounter = 0;
        static void gps_SatellitesUsedChanged(GPS3Click sender, ushort[] satellites)
        {
            foreach (var satellite in satellites)
            {
                ++satCounter;
                if (Debugger.IsAttached) Debug.Print("Satellite " + satCounter + " - " + satellite);
            }
            satCounter = 0;
            if (Debugger.IsAttached) Debug.Print("\n");
        }

        static void gps_SatellitesInViewChanged(GPS3Click sender, GPS3Click.Satellite[] satellites)
        {
            foreach (var satellite in satellites)
            {
                if (Debugger.IsAttached) Debug.Print(satellite.ToString());
            }
            if (Debugger.IsAttached) Debug.Print("\n");
        }

        static void gps_SerialError(object sender, string errorDescription, System.DateTime eventTime)
        {
            Debug.Print("Serial error - " + errorDescription + "\n");
            gps.Serial.DiscardInBuffer();
            gps.Serial.Flush();
        }

        static void gps_PMTKSentenceReceived(GPS3Click sender, string pmtksentence)
        {
            Debug.Print("PMTK Sentence - " + pmtksentence + "\n");
        }

        static void gps_AntennaStatusChanged(GPS3Click sender, GPS3Click.AntennaStatus status)
        {
            if (Debugger.IsAttached) Debug.Print("Antenna Status changed to - " + (status == GPS3Click.AntennaStatus.Ok ? "Open" : status == GPS3Click.AntennaStatus.Ok ? "Ok" : "Shorted") + "\n");
        }

        static void gps_NMEASentenceReceived(GPS3Click sender, string nmeasentence)
        {
            Debug.Print("NMEA Sentence - " + nmeasentence + "\n");
        }

        public static bool StartsWith(string src, string target)
        {
            return (src.IndexOf(target) == 0);
        }
    }
}
