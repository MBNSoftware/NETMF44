/*
 * RTC Click board driver
 * 
 * Version 1.0 :
 *  - Initial revision coded by Stephen Cardinale
 *  
 *  - Not implemented - Alarm Function, Timer and Event functionality, Writing/Reading to available User SRAM.
 *                    - Note: The PCF8583 only has a four (4) year clock.
 *                      In order to correctly read he DateTime of the RTC,
 *                      The current base year is stored in SRAM at address 0x10.
 *                      If implementing SRAM functionality, please reserve this register
 *                      or adjust the storage location accordingly.
 * Version 1.1 :
 *  - Implementation of the MikroBusNet Interface requirements.
 *  - Bug Fix - Changed all I2C Read/Write methods to use the Hardware.I2CBus.Execute extension method instead of Hardware.I2CBus.Execute method.
 *  - Implemented Clock Mode Alarm functionality (Daily, Weekday and Dated Alarms).
 *  - Implemented utilization of Free CMOS Ram for user storage.
 *  
 *  - Not implemented - Timer and Event Mode functionality.
 *  
 *  Version 2.0 :
 *  - Integration of the new namespaces and new organization.
 * 
 * * References needed :
 *   - Microsoft.SPOT.Hardware
 *   - Microsoft.SPOT.Native
 *   - MikroBusNet
 *   - mscorlib
 *  
 * Copyright 2014 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using MBN.Extensions;
using MBN.Enums;
using Microsoft.SPOT.Hardware;
using MBN.Exceptions;
using System;
using System.Reflection;

namespace MBN.Modules
{
    /// <summary>
    /// This is the MBN driver class for the <see cref="RTCClick"/> board by MikroE.
    /// <para><b>This module is an I2C Device.</b></para>
    /// <para><b>Pins used :</b> Scl, Sda (I²C bus)</para>
    /// </summary>
    /// <para><b>References required:</b> MikroBus.Net, Microsoft.SPOT.Native, Microsoft.Spot.Hardware, mscorlib</para>
    /// <example>
    /// <code language = "C#">
    /// using System;
    /// using System.Threading;
    /// using MBN;
    /// using MBN.Enums;
    /// using MBN.Modules;
    /// using Microsoft.SPOT;
    ///
    /// namespace Examples
    /// {
    ///
    ///     // Set the DateTime for first power up only
    ///     // WARNING - Do not attempt to adjust the System Time using Microsoft.SPOT.Hardware.Utility.SetLocalTime()
    ///     // or it will lock up your MBN Board and it will have to be re-flashed. Trust me on this.
    ///     // A Bug Report has been submitted to Justin
    ///
    ///     public class Program
    ///     {
    ///         private static RTCClick _clock1;
    ///
    ///         private static readonly DateTime NDate = new DateTime(2014, 5, 29, 23, 59, 50);
    ///         private static readonly DateTime LDate = new DateTime(2016, 2, 28, 23, 59, 50);
    ///         private static readonly DateTime YDate = new DateTime(2014, 12, 31, 23, 59, 50);
    ///
    ///         static int _iterations = 0;
    ///
    ///         public static void Main()
    ///         {
    ///             _clock1 = new RTCClick(Hardware.SocketThree, ClockRatesI2C.Clock400KHz, 100);
    ///
    ///             _clock1.AlarmTriggered += _clock1_AlarmTriggered;
    ///
    ///             Debug.Print("**********RTC Click Demo**********");
    ///             Debug.Print("\nDriver Version - " + _clock1.DriverVersion);
    ///             Debug.Print("Is DateTime set? " + _clock1.IsDateTimeSet);
    ///
    ///             DateRolloverDemo();
    ///             LeapYearDemo();
    ///             YearRolloverDemo();
    ///             ClockStopperDemo();
    ///             UserRamDemo();
    ///
    ///             // Select one of the three Alarm Tests below:
    ///             //DailyAlarmTest();
    ///             //WeekDayAlarmTest();
    ///             DatedAlarmTest();
    ///
    ///             while (true)
    ///             {
    ///                 var dt = _clock1.GetDateTime();
    ///                 Debug.Print("RTC DateTime - " + dt + "\n");
    ///
    ///                 Thread.Sleep(1 * 1000);
    ///             }
    ///         }
    ///
    ///         private static void DatedAlarmTest()
    ///         {
    ///             Debug.Print("\n" + "**********Doing Dated Alarm Test**********");
    ///
    ///             _clock1.AlarmAutoEnable = true;
    ///             _clock1.AlarmAutoReset = true;
    ///
    ///             _clock1.SetClock(new DateTime(2014, 8, 19, 23, 59, 55));
    ///
    ///             _clock1.ClearAlarmSettings();
    ///
    ///             _clock1.SetDatedAlarm(2014, 8, 20, 0, 0, 5);
    ///
    ///             Debug.Print("\nAlarm Settings");
    ///
    ///             RTCClick.AlarmData aData = _clock1.GetAlarmSettings();
    ///             aData = _clock1.GetAlarmSettings();
    ///             Debug.Print("Hundreds - " + aData.Time.HundredthsSecond);
    ///             Debug.Print("Seconds - " + aData.Time.Seconds);
    ///             Debug.Print("Minutes - " + aData.Time.Minutes);
    ///             Debug.Print("Hours - " + aData.Time.Hours);
    ///             Debug.Print("Day - " + aData.Day);
    ///             Debug.Print("Day of Week - " + (aData.DayOfWeek == RTCClick.WeekDays.Sunday ? "Sunday" : aData.DayOfWeek == RTCClick.WeekDays.Monday ? "Monday" : aData.DayOfWeek == RTCClick.WeekDays.Tuesday ? "Tuesday" : aData.DayOfWeek == RTCClick.WeekDays.Wednesday ? "Wednesday" : aData.DayOfWeek == RTCClick.WeekDays.Thursday ? "Thursday" : aData.DayOfWeek == RTCClick.WeekDays.Friday ? "Friday" : aData.DayOfWeek == RTCClick.WeekDays.Saturday ? "Saturday " : aData.DayOfWeek == RTCClick.WeekDays.Weekdays ? "Weekdays" : aData.DayOfWeek == RTCClick.WeekDays.Weekends ? "Weekends" : aData.DayOfWeek == RTCClick.WeekDays.AllDays ? "All Days" : aData.DayOfWeek == RTCClick.WeekDays.None ? "None" : "Multiple Days"));
    ///             Debug.Print("Month - " + aData.Month);
    ///             Debug.Print("Alarm Type - " + (aData.AlarmType == RTCClick.AlarmBehavior.DailyAlarm ? "Daily Alarm" : aData.AlarmType == RTCClick.AlarmBehavior.WeekDayAlarm ? "Weekday Alarm" : aData.AlarmType == RTCClick.AlarmBehavior.DatedAlarm ? "Dated Alarm" : "No alarm set.") + "\n");
    ///
    ///             Debug.Print("RTC Initialized? " + _clock1.IsDateTimeSet);
    ///             Debug.Print("Alarm Set? " + _clock1.IsAlarmTimeSet + "\n");
    ///         }
    ///
    ///         private static void DailyAlarmTest()
    ///         {
    ///             Debug.Print("\n" + "**********Doing Daily Alarm Test**********");
    ///
    ///             _clock1.AlarmAutoEnable = true;
    ///             _clock1.AlarmAutoReset = true;
    ///
    ///             _clock1.SetClock(new DateTime(2014, 8, 19, 23, 59, 55));
    ///
    ///             _clock1.ClearAlarmSettings();
    ///
    ///             _clock1.SetDailyAlarm(0, 0, 5);
    ///
    ///             Debug.Print("\nAlarm Settings");
    ///
    ///             RTCClick.AlarmData aData = _clock1.GetAlarmSettings();
    ///             aData = _clock1.GetAlarmSettings();
    ///             Debug.Print("Hundreds - " + aData.Time.HundredthsSecond);
    ///             Debug.Print("Seconds - " + aData.Time.Seconds);
    ///             Debug.Print("Minutes - " + aData.Time.Minutes);
    ///             Debug.Print("Hours - " + aData.Time.Hours);
    ///             Debug.Print("Day - " + aData.Day);
    ///             Debug.Print("Day of Week - " + (aData.DayOfWeek == RTCClick.WeekDays.Sunday ? "Sunday" : aData.DayOfWeek == RTCClick.WeekDays.Monday ? "Monday" : aData.DayOfWeek == RTCClick.WeekDays.Tuesday ? "Tuesday" : aData.DayOfWeek == RTCClick.WeekDays.Wednesday ? "Wednesday" : aData.DayOfWeek == RTCClick.WeekDays.Thursday ? "Thursday" : aData.DayOfWeek == RTCClick.WeekDays.Friday ? "Friday" : aData.DayOfWeek == RTCClick.WeekDays.Saturday ? "Saturday " : aData.DayOfWeek == RTCClick.WeekDays.Weekdays ? "Weekdays" : aData.DayOfWeek == RTCClick.WeekDays.Weekends ? "Weekends" : aData.DayOfWeek == RTCClick.WeekDays.AllDays ? "All Days" : aData.DayOfWeek == RTCClick.WeekDays.None ? "None" : "Multiple Days"));
    ///             Debug.Print("Month - " + aData.Month);
    ///             Debug.Print("Alarm Type - " + (aData.AlarmType == RTCClick.AlarmBehavior.DailyAlarm ? "Daily Alarm" : aData.AlarmType == RTCClick.AlarmBehavior.WeekDayAlarm ? "Weekday Alarm" : aData.AlarmType == RTCClick.AlarmBehavior.DatedAlarm ? "Dated Alarm" : "No alarm set.") + "\n");
    ///
    ///             Debug.Print("RTC Initialized? " + _clock1.IsDateTimeSet);
    ///             Debug.Print("Alarm Set? " + _clock1.IsAlarmTimeSet + "\n");
    ///         }
    ///
    ///         private static void WeekDayAlarmTest()
    ///         {
    ///             Debug.Print("\n" + "**********Doing Weekday Alarm Test**********");
    ///
    ///             _clock1.AlarmAutoEnable = true;
    ///             _clock1.AlarmAutoReset = true;
    ///
    ///             _clock1.SetClock(new DateTime(2014, 8, 19, 23, 59, 55));
    ///
    ///             _clock1.ClearAlarmSettings();
    ///
    ///             RTCClick.WeekDays weekDays = RTCClick.WeekDays.AllDays;
    ///             _clock1.SetWeekdayAlarm(weekDays, 00, 00, 05);
    ///
    ///             Debug.Print("\nAlarm Settings");
    ///
    ///             RTCClick.AlarmData aData = _clock1.GetAlarmSettings();
    ///             aData = _clock1.GetAlarmSettings();
    ///             Debug.Print("Hundreds - " + aData.Time.HundredthsSecond);
    ///             Debug.Print("Seconds - " + aData.Time.Seconds);
    ///             Debug.Print("Minutes - " + aData.Time.Minutes);
    ///             Debug.Print("Hours - " + aData.Time.Hours);
    ///             Debug.Print("Day - " + aData.Day);
    ///             Debug.Print("Day of Week - " + (aData.DayOfWeek == RTCClick.WeekDays.Sunday ? "Sunday" : aData.DayOfWeek == RTCClick.WeekDays.Monday ? "Monday" : aData.DayOfWeek == RTCClick.WeekDays.Tuesday ? "Tuesday" : aData.DayOfWeek == RTCClick.WeekDays.Wednesday ? "Wednesday" : aData.DayOfWeek == RTCClick.WeekDays.Thursday ? "Thursday" : aData.DayOfWeek == RTCClick.WeekDays.Friday ? "Friday" : aData.DayOfWeek == RTCClick.WeekDays.Saturday ? "Saturday " : aData.DayOfWeek == RTCClick.WeekDays.Weekdays ? "Weekdays" : aData.DayOfWeek == RTCClick.WeekDays.Weekends ? "Weekends" : aData.DayOfWeek == RTCClick.WeekDays.AllDays ? "All Days" : aData.DayOfWeek == RTCClick.WeekDays.None ? "None" : "Multiple Days"));
    ///             Debug.Print("Month - " + aData.Month);
    ///             Debug.Print("Alarm Type - " + (aData.AlarmType == RTCClick.AlarmBehavior.DailyAlarm ? "Daily Alarm" : aData.AlarmType == RTCClick.AlarmBehavior.WeekDayAlarm ? "Weekday Alarm" : aData.AlarmType == RTCClick.AlarmBehavior.DatedAlarm ? "Dated Alarm" : "No alarm set.") + "\n");
    ///
    ///             Debug.Print("RTC Initialized? " + _clock1.IsDateTimeSet);
    ///             Debug.Print("Alarm Set? " + _clock1.IsAlarmTimeSet + "\n");
    ///         }
    ///
    ///         private static void YearRolloverDemo()
    ///         {
    ///             Debug.Print("\n" + "**********Doing Year roll over test**********");
    ///
    ///             _clock1.SetClock(YDate);
    ///
    ///             while (true)
    ///             {
    ///                 if (_iterations > 20)
    ///                 {
    ///                     _iterations = 0;
    ///                     break;
    ///                 }
    ///                 var dt1 = _clock1.GetDateTime();
    ///
    ///                 Debug.Print("RTC DateTime - " + dt1);
    ///
    ///                 Thread.Sleep(1000);
    ///                 _iterations += 1;
    ///             }
    ///         }
    ///
    ///         private static void LeapYearDemo()
    ///         {
    ///             Debug.Print("\n" + "**********Doing Leap Year test**********");
    ///
    ///             _clock1.SetClock(LDate);
    ///
    ///             while (true)
    ///             {
    ///                 if (_iterations > 20)
    ///                 {
    ///                     _iterations = 0;
    ///                     break;
    ///                 }
    ///                 var dt1 = _clock1.GetDateTime();
    ///
    ///                 Debug.Print("RTC DateTime - " + dt1);
    ///
    ///                 Thread.Sleep(1000);
    ///                 _iterations += 1;
    ///             }
    ///         }
    ///
    ///         private static void DateRolloverDemo()
    ///         {
    ///             _clock1.SetClock(NDate);
    ///
    ///             Debug.Print("**********Doing date roll over test**********");
    ///
    ///             while (true)
    ///             {
    ///                 if (_iterations > 20)
    ///                 {
    ///                     _iterations = 0;
    ///                     break;
    ///                 }
    ///                 var dt1 = _clock1.GetDateTime();
    ///
    ///                 Debug.Print("RTC DateTime - " + dt1);
    ///
    ///                 Thread.Sleep(1000);
    ///                 _iterations += 1;
    ///             }
    ///         }
    ///
    ///         private static void ClockStopperDemo()
    ///         {
    ///             bool done = false;
    ///             _iterations = 0;
    ///
    ///             _clock1.StopClock();
    ///
    ///             Debug.Print("\n**********Doing Stop/Start Clock test**********");
    ///             Debug.Print("The clock will stop incrementing for about 10 seconds then resume.");
    ///
    ///             while (true)
    ///             {
    ///                 if (_iterations > 20)
    ///                 {
    ///                     _iterations = 0;
    ///                     break;
    ///                 }
    ///
    ///                 var dt = _clock1.GetDateTime();
    ///                 Debug.Print("RTC DateTime - " + dt);
    ///
    ///                 if ((_iterations >= 10) <![CDATA[&]]><![CDATA[&]]> !done)
    ///                 {
    ///                     done = true;
    ///                     _clock1.StartClock();
    ///                 }
    ///
    ///                 Thread.Sleep(1000);
    ///                 _iterations += 1;
    ///             }
    ///         }
    ///
    ///         private static void UserRamDemo()
    ///         {
    ///             Debug.Print("\n" + "**********Doing User Ram Test**********");
    ///
    ///             // Test writing to arbitrary registers
    ///             Debug.Print("\nWriting distinct RAM registers (writing the register number to itself)");
    ///             for (int b = (byte)RTCClick.UserRam.StartAddressBlock; b <![CDATA[<]]>= (byte)RTCClick.UserRam.EndAddressBlock; b++)
    ///             {
    ///                 _clock1.WriteUserRamAddress((byte)b, (byte)b);
    ///             }
    ///
    ///             // Test reading from arbitrary registers
    ///             Debug.Print("Reading distinct RAM registers (the registers and values read should be the same)");
    ///             for (int b = (byte)RTCClick.UserRam.StartAddressBlock; b <![CDATA[<]]>= (byte)RTCClick.UserRam.EndAddressBlock; b++)
    ///             {
    ///                 Debug.Print(b + ": " + _clock1.ReadUserRamAddress((byte)b));
    ///             }
    ///
    ///             Debug.Print("\n");
    ///
    ///             var ipsumLorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer nec odio. Praesent libero. Sed cursus ante dapibus diam. Sed nisi. Nulla quis sem at nibh elementum imperdiet. Duis sagittis ipsum. Praesent mauris. Fusce nec tellus.";
    ///             byte[] userRam = new byte[(byte)RTCClick.UserRam.RamSize];
    ///             Debug.Print("Writing a long string to User Ram - " + ipsumLorem);
    ///
    ///             // Copy the string to the ram buffer
    ///             for (byte b = 0; b <![CDATA[<]]>= ipsumLorem.Length - 1; b++)
    ///             {
    ///                 userRam[b] = (byte)ipsumLorem[b];
    ///             }
    ///             // Write it to the User RAM.
    ///             _clock1.WriteUserRam(userRam);
    ///
    ///             Debug.Print("\n");
    ///
    ///             //Test reading from the RAM as a single block
    ///             Debug.Print("Reading from the RAM as a block of memory...");
    ///
    ///             userRam = _clock1.ReadUserRam();
    ///
    ///             var ramString = string.Empty;
    ///
    ///             for (int i = 0; i <![CDATA[<]]>= userRam.Length - 1; i++)
    ///             {
    ///                 ramString += (char)userRam[i];
    ///             }
    ///
    ///             if (ramString.Length != 0) Debug.Print("Reading from USER RAM: " + ramString + "\n");
    ///
    ///             Debug.Print("Writing a char to a specific Ram address");
    ///             _clock1.WriteUserRamAddress(0xFf, (byte)'c');
    ///             Debug.Print("Reading back from the same address - " + (char)_clock1.ReadUserRamAddress(0xFF));
    ///         }
    ///
    ///         static void _clock1_AlarmTriggered(object sender, RTCClick.AlarmEventArgs e)
    ///         {
    ///             Debug.Print("Alarm of type - " + (e.AlarmType == RTCClick.AlarmBehavior.DailyAlarm ? "Daily Alarm" : e.AlarmType == RTCClick.AlarmBehavior.WeekDayAlarm ? "Weekday Alarm" : "Dated Alarm") + " - occurred at " + e.AlarmEventEventTime + "\n");
    ///             //For Testing only so it will re-fire.
    ///             _clock1.SetClock(new DateTime(2014, 8, 19, 23, 59, 55));
    ///         }
    ///
    ///     }
    /// }
    /// </code>
    /// <code language = "VB">
    /// Option Explicit On
    /// Option Strict On
    ///
    /// Imports System
    /// Imports MBN
    /// Imports MBN.Enums
    /// Imports Microsoft.SPOT
    /// Imports System.Threading
    /// Imports MBN.Modules
    ///
    /// Namespace Examples
    ///
    ///     Public Module Module1
    ///
    ///         Dim WithEvents _clock1 As RTCClick
    ///
    ///         Private ReadOnly NDate As DateTime = New DateTime(2014, 5, 29, 23, 59, 50)
    ///         Private ReadOnly LDate As DateTime = New DateTime(2016, 2, 28, 23, 59, 50)
    ///         Private ReadOnly YDate As DateTime = New DateTime(2014, 12, 31, 23, 59, 50)
    ///
    ///         Private _iterations As Integer = 0
    ///
    ///         Sub Main()
    ///
    ///             _clock1 = New RTCClick(Hardware.SocketThree, ClockRatesI2C.Clock400KHz, 100)
    ///
    ///             Debug.Print("**********RTC Click Demo**********")
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///             Debug.Print("Driver Version - "  <![CDATA[&]]> _clock1.DriverVersion.ToString())
    ///             Debug.Print("Is DateTime set? "  <![CDATA[&]]> _clock1.IsDateTimeSet)
    ///
    ///             DateRolloverDemo()
    ///             LeapYearDemo()
    ///             YearRolloverDemo()
    ///             ClockStopperDemo()
    ///             UserRamDemo()
    ///
    ///             ' Select one of the three Alarm Tests below:
    ///             'DailyAlarmTest()
    ///             'WeekDayAlarmTest()
    ///             DatedAlarmTest()
    ///
    ///             While True
    ///                 Dim dt = _clock1.GetDateTime()
    ///                 Debug.Print("RTC DateTime - "  <![CDATA[&]]> dt.ToString()  <![CDATA[&]]> Microsoft.VisualBasic.Constants.vbCrLf)
    ///
    ///                 Thread.Sleep(1 * 1000)
    ///             End While
    ///
    ///         End Sub
    ///
    ///         Private Sub _clock1_AlarmTriggered(sender As Object, e As RTCClick.AlarmEventArgs) Handles _clock1.AlarmTriggered
    ///             Debug.Print("Alarm of type - "  <![CDATA[&]]> (If(e.AlarmType = RTCClick.AlarmBehavior.DailyAlarm, "Daily Alarm", If(e.AlarmType = RTCClick.AlarmBehavior.WeekDayAlarm, "Weekday Alarm", "Dated Alarm")))  <![CDATA[&]]> " - occurred at "  <![CDATA[&]]> e.AlarmEventEventTime.ToString())
    ///             'For Testing only so it will re-fire.
    ///             _clock1.SetClock(New DateTime(2014, 8, 19, 23, 59, 55))
    ///         End Sub
    ///
    ///         Private Sub DailyAlarmTest()
    ///
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///             Debug.Print("**********Doing Daily Alarm Test**********")
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///
    ///             _clock1.AlarmAutoEnable = True
    ///             _clock1.AlarmAutoReset = True
    ///
    ///             _clock1.SetClock(New DateTime(2014, 8, 19, 23, 59, 55))
    ///
    ///             _clock1.ClearAlarmSettings()
    ///
    ///             _clock1.SetDailyAlarm(0, 0, 5)
    ///
    ///             Debug.Print("Alarm Settings")
    ///
    ///             Dim aData As RTCClick.AlarmData = _clock1.GetAlarmSettings()
    ///
    ///             Debug.Print("Hundreds - "  <![CDATA[&]]> aData.Time.HundredthsSecond)
    ///             Debug.Print("Seconds - "  <![CDATA[&]]> aData.Time.Seconds)
    ///             Debug.Print("Minutes - "  <![CDATA[&]]> aData.Time.Minutes)
    ///             Debug.Print("Hours - "  <![CDATA[&]]> aData.Time.Hours)
    ///             Debug.Print("Day - "  <![CDATA[&]]> aData.Day)
    ///             Debug.Print("Day of Week - "  <![CDATA[&]]> (If(aData.DayOfWeek = RTCClick.WeekDays.Sunday, "Sunday", If(aData.DayOfWeek = RTCClick.WeekDays.Monday, "Monday", If(aData.DayOfWeek = RTCClick.WeekDays.Tuesday, "Tuesday", If(aData.DayOfWeek = RTCClick.WeekDays.Wednesday, "Wednesday", If(aData.DayOfWeek = RTCClick.WeekDays.Thursday, "Thursday", If(aData.DayOfWeek = RTCClick.WeekDays.Friday, "Friday", If(aData.DayOfWeek = RTCClick.WeekDays.Saturday, "Saturday ", If(aData.DayOfWeek = RTCClick.WeekDays.Weekdays, "Weekdays", If(aData.DayOfWeek = RTCClick.WeekDays.Weekends, "Weekends", If(aData.DayOfWeek = RTCClick.WeekDays.AllDays, "All Days", If(aData.DayOfWeek = RTCClick.WeekDays.None, "None", "Multiple Days")))))))))))))
    ///             Debug.Print("Month - "  <![CDATA[&]]> aData.Month)
    ///             Debug.Print("Alarm Type - "  <![CDATA[&]]> (If(aData.AlarmType = RTCClick.AlarmBehavior.DailyAlarm, "Daily Alarm", If(aData.AlarmType = RTCClick.AlarmBehavior.WeekDayAlarm, "Weekday Alarm", If(aData.AlarmType = RTCClick.AlarmBehavior.DatedAlarm, "Dated Alarm", "No alarm set."))))  <![CDATA[&]]> vbLf)
    ///             Debug.Print("RTC Initialized? "  <![CDATA[&]]> _clock1.IsDateTimeSet)
    ///             Debug.Print("Alarm Set? "  <![CDATA[&]]> _clock1.IsAlarmTimeSet)
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///         End Sub
    ///
    ///         Private Sub WeekDayAlarmTest()
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///             Debug.Print("**********Doing Weekday Alarm Test**********")
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///
    ///             _clock1.AlarmAutoEnable = True
    ///             _clock1.AlarmAutoReset = True
    ///             _clock1.SetClock(New DateTime(2014, 8, 19, 23, 59, 55))
    ///             _clock1.ClearAlarmSettings()
    ///
    ///             Dim weekDays As RTCClick.WeekDays = RTCClick.WeekDays.AllDays
    ///             _clock1.SetWeekdayAlarm(weekDays, 0, 0, 5)
    ///
    ///             Debug.Print("Alarm Settings")
    ///
    ///             Dim aData As RTCClick.AlarmData = _clock1.GetAlarmSettings()
    ///
    ///             Debug.Print("Hundreds - "  <![CDATA[&]]> aData.Time.HundredthsSecond)
    ///             Debug.Print("Seconds - "  <![CDATA[&]]> aData.Time.Seconds)
    ///             Debug.Print("Minutes - "  <![CDATA[&]]> aData.Time.Minutes)
    ///             Debug.Print("Hours - "  <![CDATA[&]]> aData.Time.Hours)
    ///             Debug.Print("Day - "  <![CDATA[&]]> aData.Day)
    ///             Debug.Print("Day of Week - "  <![CDATA[&]]> (If(aData.DayOfWeek = RTCClick.WeekDays.Sunday, "Sunday", If(aData.DayOfWeek = RTCClick.WeekDays.Monday, "Monday", If(aData.DayOfWeek = RTCClick.WeekDays.Tuesday, "Tuesday", If(aData.DayOfWeek = RTCClick.WeekDays.Wednesday, "Wednesday", If(aData.DayOfWeek = RTCClick.WeekDays.Thursday, "Thursday", If(aData.DayOfWeek = RTCClick.WeekDays.Friday, "Friday", If(aData.DayOfWeek = RTCClick.WeekDays.Saturday, "Saturday ", If(aData.DayOfWeek = RTCClick.WeekDays.Weekdays, "Weekdays", If(aData.DayOfWeek = RTCClick.WeekDays.Weekends, "Weekends", If(aData.DayOfWeek = RTCClick.WeekDays.AllDays, "All Days", If(aData.DayOfWeek = RTCClick.WeekDays.None, "None", "Multiple Days")))))))))))))
    ///             Debug.Print("Month - "  <![CDATA[&]]> aData.Month)
    ///             Debug.Print("Alarm Type - "  <![CDATA[&]]> (If(aData.AlarmType = RTCClick.AlarmBehavior.DailyAlarm, "Daily Alarm", If(aData.AlarmType = RTCClick.AlarmBehavior.WeekDayAlarm, "Weekday Alarm", If(aData.AlarmType = RTCClick.AlarmBehavior.DatedAlarm, "Dated Alarm", "No alarm set."))))  <![CDATA[&]]> vbLf)
    ///             Debug.Print("RTC Initialized? "  <![CDATA[&]]> _clock1.IsDateTimeSet)
    ///             Debug.Print("Alarm Set? "  <![CDATA[&]]> _clock1.IsAlarmTimeSet)
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///         End Sub
    ///
    ///         Private Sub DatedAlarmTest()
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///             Debug.Print("**********Doing Dated Alarm Test**********")
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///
    ///             _clock1.AlarmAutoEnable = True
    ///             _clock1.AlarmAutoReset = True
    ///             _clock1.SetClock(New DateTime(2014, 8, 19, 23, 59, 55))
    ///             _clock1.ClearAlarmSettings()
    ///             _clock1.SetDatedAlarm(2014, 8, 20, 0, 0, 5)
    ///
    ///             Debug.Print("Alarm Settings")
    ///
    ///             Dim aData As RTCClick.AlarmData = _clock1.GetAlarmSettings()
    ///             Debug.Print("Hundreds - "  <![CDATA[&]]> aData.Time.HundredthsSecond)
    ///             Debug.Print("Seconds - "  <![CDATA[&]]> aData.Time.Seconds)
    ///             Debug.Print("Minutes - "  <![CDATA[&]]> aData.Time.Minutes)
    ///             Debug.Print("Hours - "  <![CDATA[&]]> aData.Time.Hours)
    ///             Debug.Print("Day - "  <![CDATA[&]]> aData.Day)
    ///             Debug.Print("Day of Week - "  <![CDATA[&]]> (If(aData.DayOfWeek = RTCClick.WeekDays.Sunday, "Sunday", If(aData.DayOfWeek = RTCClick.WeekDays.Monday, "Monday", If(aData.DayOfWeek = RTCClick.WeekDays.Tuesday, "Tuesday", If(aData.DayOfWeek = RTCClick.WeekDays.Wednesday, "Wednesday", If(aData.DayOfWeek = RTCClick.WeekDays.Thursday, "Thursday", If(aData.DayOfWeek = RTCClick.WeekDays.Friday, "Friday", If(aData.DayOfWeek = RTCClick.WeekDays.Saturday, "Saturday ", If(aData.DayOfWeek = RTCClick.WeekDays.Weekdays, "Weekdays", If(aData.DayOfWeek = RTCClick.WeekDays.Weekends, "Weekends", If(aData.DayOfWeek = RTCClick.WeekDays.AllDays, "All Days", If(aData.DayOfWeek = RTCClick.WeekDays.None, "None", "Multiple Days")))))))))))))
    ///             Debug.Print("Month - "  <![CDATA[&]]> aData.Month)
    ///             Debug.Print("Alarm Type - "  <![CDATA[&]]> (If(aData.AlarmType = RTCClick.AlarmBehavior.DailyAlarm, "Daily Alarm", If(aData.AlarmType = RTCClick.AlarmBehavior.WeekDayAlarm, "Weekday Alarm", If(aData.AlarmType = RTCClick.AlarmBehavior.DatedAlarm, "Dated Alarm", "No alarm set."))))  <![CDATA[&]]> vbLf)
    ///             Debug.Print("RTC Initialized? "  <![CDATA[&]]> _clock1.IsDateTimeSet)
    ///             Debug.Print("Alarm Set? "  <![CDATA[&]]> _clock1.IsAlarmTimeSet)
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///         End Sub
    ///
    ///         Private Sub YearRolloverDemo()
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///             Debug.Print("**********Doing Year roll over test**********")
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///
    ///             _clock1.SetClock(YDate)
    ///
    ///             While True
    ///                 If _iterations > 20 Then
    ///                     _iterations = 0
    ///                     Exit While
    ///                 End If
    ///                 Dim dt1 = _clock1.GetDateTime()
    ///
    ///                 Debug.Print("RTC DateTime - "  <![CDATA[&]]> dt1.ToString())
    ///
    ///                 Thread.Sleep(1000)
    ///                 _iterations += 1
    ///             End While
    ///         End Sub
    ///
    ///         Private Sub LeapYearDemo()
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///             Debug.Print("**********Doing Leap Year test**********")
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///
    ///             _clock1.SetClock(LDate)
    ///
    ///             While True
    ///                 If _iterations > 20 Then
    ///                     _iterations = 0
    ///                     Exit While
    ///                 End If
    ///                 Dim dt1 = _clock1.GetDateTime()
    ///
    ///                 Debug.Print("RTC DateTime - "  <![CDATA[&]]> dt1.ToString())
    ///
    ///                 Thread.Sleep(1000)
    ///                 _iterations += 1
    ///             End While
    ///         End Sub
    ///
    ///         Private Sub DateRolloverDemo()
    ///             _clock1.SetClock(NDate)
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///             Debug.Print("**********Doing date roll over test**********")
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///
    ///             While True
    ///                 If _iterations > 20 Then
    ///                     _iterations = 0
    ///                     Exit While
    ///                 End If
    ///                 Dim dt1 = _clock1.GetDateTime()
    ///
    ///                 Debug.Print("RTC DateTime - "  <![CDATA[&]]> dt1.ToString())
    ///
    ///                 Thread.Sleep(1000)
    ///                 _iterations += 1
    ///             End While
    ///         End Sub
    ///
    ///         Private Sub ClockStopperDemo()
    ///             Dim done As Boolean = False
    ///             _iterations = 0
    ///
    ///             _clock1.StopClock()
    ///
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///             Debug.Print("**********Doing Stop/Start Clock test**********")
    ///             Debug.Print("The clock will stop incrementing for about 10 seconds then resume.")
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///
    ///             While True
    ///                 If _iterations > 20 Then
    ///                     _iterations = 0
    ///                     Exit While
    ///                 End If
    ///
    ///                 Dim dt = _clock1.GetDateTime()
    ///                 Debug.Print("RTC DateTime - "  <![CDATA[&]]> dt.ToString())
    ///
    ///                 If (_iterations >= 10) AndAlso Not done Then
    ///                     done = True
    ///                     _clock1.StartClock()
    ///                 End If
    ///
    ///                 Thread.Sleep(1000)
    ///                 _iterations += 1
    ///             End While
    ///         End Sub
    ///
    ///         Private Sub UserRamDemo()
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///             Debug.Print("**********Doing User Ram Test**********")
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///
    ///             ' Test writing to arbitrary registers
    ///             Debug.Print("Writing distinct RAM registers (writing the register number to itself)")
    ///             For b As Integer = CByte(RTCClick.UserRam.StartAddressBlock) To CByte(RTCClick.UserRam.EndAddressBlock)
    ///                 _clock1.WriteUserRamAddress(CByte(b), CByte(b))
    ///             Next
    ///
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///
    ///             ' Test reading from arbitrary registers
    ///             Debug.Print("Reading distinct RAM registers (the registers and values read should be the same)")
    ///             For b As Integer = CByte(RTCClick.UserRam.StartAddressBlock) To CByte(RTCClick.UserRam.EndAddressBlock)
    ///                 Debug.Print(b  <![CDATA[&]]> ": "  <![CDATA[&]]> _clock1.ReadUserRamAddress(CByte(b)))
    ///             Next
    ///
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///
    ///             Dim ipsumLorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer nec odio. Praesent libero. Sed cursus ante dapibus diam. Sed nisi. Nulla quis sem at nibh elementum imperdiet. Duis sagittis ipsum. Praesent mauris. Fusce nec tellus."
    ///             Dim userRam As Byte() = New Byte(CByte(RTCClick.UserRam.RamSize) - 1) {}
    ///             Debug.Print("Writing a long string to User Ram - "  <![CDATA[&]]> ipsumLorem)
    ///
    ///             ' Copy the string to the ram buffer
    ///             For b As Byte = 0 To CByte(ipsumLorem.Length - 1)
    ///                 userRam(b) = CByte(Strings.AscW(ipsumLorem(b)))
    ///             Next
    ///             ' Write it to the User RAM.
    ///             _clock1.WriteUserRam(userRam)
    ///
    ///             Debug.Print(Microsoft.VisualBasic.Constants.vbCrLf)
    ///
    ///             'Test reading from the RAM as a single block
    ///             Debug.Print("Reading from the RAM as a block of memory...")
    ///
    ///             userRam = _clock1.ReadUserRam()
    ///
    ///             Dim ramString = String.Empty
    ///
    ///             For i As Integer = 0 To userRam.Length - 1
    ///                 ramString += Strings.ChrW(userRam(i))
    ///             Next
    ///
    ///             If ramString.Length <![CDATA[<]]><![CDATA[>]]> 0 Then
    ///                 Debug.Print("Reading from USER RAM: "  <![CDATA[&]]> ramString  <![CDATA[&]]> Microsoft.VisualBasic.Constants.vbCrLf)
    ///             End If
    ///
    ///             Debug.Print("Writing a char to a specific Ram address")
    ///             _clock1.WriteUserRamAddress( <![CDATA[&]]>HFF, CByte(Strings.AscW("c"c)))
    ///             Debug.Print("Reading back from the same address - "  <![CDATA[&]]> CChar(Strings.ChrW(_clock1.ReadUserRamAddress(<![CDATA[&]]>HFF))))
    ///         End Sub
    ///
    ///     End Module
    ///
    /// End Namespace
    /// </code>
    /// </example>
// ReSharper disable once InconsistentNaming
    public class RTCClick : IDriver
    {
        #region Constants

        // Control/Status Registers - Clock Mode
        private const byte ControlandStatusRegister = 0x00;
        private const byte ControlHundredthsSeconds = 0x01;
/* Not used in this release
        private const byte Control_Seconds = 0x02;
        private const byte Control_Minutes = 0x03;
        private const byte Control_YearDate = 0x05;
        private const byte Control_WeekdayMonth = 0x06;
        private const byte Control_Timer = 0x07;
*/
        // Alarm Control Registers - Clock Mode
        private const byte AlarmControlRegister = 0x08;
        private const byte AlarmControlHundredthsSecond = 0x09;
/* Not used in this release
        private const byte AlarmControlSeconds = 0x0A;
        private const byte AlarmControlMinutes = 0x0B;
        private const byte AlarmControlHours = 0x0C;
        private const byte AlarmControlDay = 0x0D;
 */ 
        private const byte AlarmControlWeekDayMonth = 0x0E;

        private const byte BaseYearHoldingRegister = 0x10;

        #endregion

        #region Fields

        private static int _i2CTimeout;
        private const byte I2CAddress = 0xA0 >> 1;
        private static I2CDevice.Configuration _i2CConfiguration;

        private static InterruptPort _alarmInterruptPort;

        private static bool _alarmAutoReset;
        private bool _alarmAutoEnable;

        #endregion

        #region CTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="RTCClick"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="Hardware.Socket"/> that the RTC Click board is inserted into.</param>
        /// <param name="clockKHz">The speed of the I2C Clock. See <see cref="ClockRatesI2C"/></param>
        /// <param name="timeout">I2C Transaction timeout in milliseconds.</param>
        public RTCClick(Hardware.Socket socket, ClockRatesI2C clockKHz, int timeout = 1000)
        {
            try
            {
                // Checks if needed I²C pins are available
                Hardware.CheckPinsI2C(socket);

                _i2CTimeout = timeout;

                // Create the driver's I²C configuration
                _i2CConfiguration = new I2CDevice.Configuration(I2CAddress, (int) clockKHz);

                // Check if it's the first time an I²C device is created
                if (Hardware.I2CBus == null)
                {
                    Hardware.I2CBus = new I2CDevice(_i2CConfiguration);
                }

                _alarmInterruptPort = new InterruptPort(socket.Int, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);
                _alarmInterruptPort.OnInterrupt += AlarmInterruptPortOnInterruptPort;
                _alarmInterruptPort.DisableInterrupt();

                Initialize(); // Initialize the PCF8583 on the RTC Click board to default values for Clock Mode
            }
                // Catch only the PinInUse exception, so that program will halt on other exceptions
            catch (PinInUseException ex)
            {
                throw new PinInUseException(ex.Message);
            }
        }

        #endregion

        #region Private Methods

        private static int GetBaseYear(int year)
        {
            return year - year%4;
        }

        private static void Initialize()
        {
            var sramData = ReadRegister(BaseYearHoldingRegister);

            if ((sramData[5] == 0x50)) WriteRegister(BaseYearHoldingRegister, new byte[] {1, 6, 0, 1, 0, 0, 0, 0});

            Configuration config = Configuration.GetConfiguration();
            config.FunctionModeFlag = Mode.ClockMode32Khz;
            config.AlarmFlag = false;
            config.TimerFlag = false;
            config.AlarmEnableFlag = false;
            config.MaskFlag = false;
            config.HoldLastCountFlag = false;
            config.StopCountingFlag = false;
            Configuration.SetConfiguration(config);
        }

        private static void SetClockModeAlarmType(AlarmBehavior alarmBehavior)
        {
            WriteByte(AlarmControlRegister, alarmBehavior == AlarmBehavior.NoAlarm ? Bits.Set(ReadByte(AlarmControlRegister), "xx00xxxx") : alarmBehavior == AlarmBehavior.DailyAlarm ? Bits.Set(ReadByte(AlarmControlRegister), "xx01xxxx") : alarmBehavior == AlarmBehavior.WeekDayAlarm ? Bits.Set(ReadByte(AlarmControlRegister), "xx10xxxx") : Bits.Set(ReadByte(AlarmControlRegister), "xx11xxxx"));
        }

        private static byte ReadByte(byte register)
        {
            var result = new Byte[1];

            var actions = new I2CDevice.I2CTransaction[2];
            actions[0] = I2CDevice.CreateWriteTransaction(new[] {register});
            actions[1] = I2CDevice.CreateReadTransaction(result);

            Hardware.I2CBus.Execute(_i2CConfiguration, actions, _i2CTimeout);

            return result[0];
        }

        private static byte[] ReadRegister(byte register)
        {
            var result = new Byte[8];

            var actions = new I2CDevice.I2CTransaction[2];
            actions[0] = I2CDevice.CreateWriteTransaction(new[] {register});
            actions[1] = I2CDevice.CreateReadTransaction(result);

            Hardware.I2CBus.Execute(_i2CConfiguration, actions, _i2CTimeout);

            return result;
        }

        private static void WriteByte(byte register, byte data)
        {
            var actions = new I2CDevice.I2CTransaction[1];
            actions[0] = I2CDevice.CreateWriteTransaction(new[] {register, data});

            Hardware.I2CBus.Execute(_i2CConfiguration, actions, _i2CTimeout);
        }

        private static void WriteRegister(byte register, byte[] value)
        {
            var buffer = new byte[sizeof (byte) + value.Length];
            buffer[0] = register;
            value.CopyTo(buffer, 1);

            var actions = new I2CDevice.I2CTransaction[1];
            actions[0] = I2CDevice.CreateWriteTransaction(buffer);

            Hardware.I2CBus.Execute(_i2CConfiguration, actions, _i2CTimeout);
        }

        #endregion

        #region Public Methods

        #region Clock Methods

        /// <summary>
        ///     Returns the current DateTime from the PCF8583 IC on the RTC Click board.
        /// </summary>
        /// <returns>The current <see cref="DateTime"/> of the PCF8583 IC on the RTC Click.</returns>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("RTC DateTime - " + _rtcClick.GetDateTime());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("RTC DateTime - " <![CDATA[&]]> _rtcClick.GetDateTime()) 
        /// </code>
        /// </example>
        public DateTime GetDateTime()
        {
            DateTime dt;

            var currentYear = 0;

            // Read the RTC registers
            byte[] data = ReadRegister(ControlHundredthsSeconds);

            if (data.Length != 0)
            {
                short hundredsSeconds = data[0];
                short seconds = data[1].FromBcd();
                short minutes = data[2].FromBcd();
                short hour = data[3].FromBcd();
                short day = ((byte) (data[4] & 0x3f)).FromBcd();
                var year = (short) ((data[4] >> 6) & 0x03);
                short month = ((byte) (data[5] & 0x1F)).FromBcd();

                //  but that's not all - we need to find out what the base year is
                //  so we can add the 2 bits we got above and find the real year
                var sramData = ReadRegister(BaseYearHoldingRegister);

                if (sramData.Length != 0)
                {
                    // Check the year, compare to what is stored in SRAM at 0x00 if it not the same the year has changed, so update the data stored in SRAM.
                    if (sramData[4] != year)
                    {
                        currentYear = sramData[0]*1000 + sramData[1]*100 + sramData[2]*10 + sramData[3] + year;
                        var yearbase = GetBaseYear(currentYear);
                        var newSramData = new[]
                        {
                            (byte) (yearbase / 1000),
                            (byte) (yearbase / 100 % 10),
                            (byte) (yearbase / 10 % 10),
                            (byte) (yearbase % 10),
                            (byte) (year % 4)
                        };
                        WriteRegister(BaseYearHoldingRegister, newSramData);
                    }
                    else
                    {
                        currentYear = sramData[5] == 0 ? 1601 : (sramData[0]*1000) + (sramData[1]*100) + (sramData[2]*10) + sramData[3] + sramData[4];
                    }
                }

                // Now return the actual DateTime
                try
                {
                    dt = new DateTime(currentYear, month, day, hour, minutes, seconds, hundredsSeconds*10);
                }
                catch
                {
                    dt = DateTime.MinValue;
                }
            }
            else
            {
                throw new Exception("Unable to read DataTime from RTC");
            }
            return dt;
        }

        /// <summary>
        ///     Sets the DateTime of PCF8583 RTC IC on the RTC Click board.
        /// </summary>
        /// <param name="dateTime">The DateTime to set the PCF8583 IC on the RTC Click board to.</param>
        /// <example>Sample usage:
        /// <code language = "C#">
        /// var dt = new DateTime(2012, 2, 28, 23, 59, 45);
        /// _clock1.SetClock(dt);
        /// </code>
        /// <code language = "VB">
        /// Dim dt = New DateTime(2012, 2, 28, 23, 59, 45)
        /// _clock1.SetClock(dt)
        /// </code>
        /// </example>
        public void SetClock(DateTime dateTime)
        {
            SetClock(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond, dateTime.DayOfWeek);
        }

        /// <summary>
        ///     Sets the <see cref="DateTime"/> of PCF8583 RTC IC on the RTC Click board.
        /// </summary>
        /// <param name="year">The year to set the RTC DateTime to.</param>
        /// <param name="month">The month to set the RTC DateTime to.</param>
        /// <param name="day">The day to set the RTC DateTime to.</param>
        /// <param name="hour">The hour to set the RTC DateTime to.</param>
        /// <param name="minutes">The minutes to set the RTC DateTime to.</param>
        /// <param name="seconds">Optional Parameter - The seconds to set the RTC DateTime to.</param>
        /// <param name="milliSeconds">Optional Parameter - The milliseconds to set the RTC DateTime to.</param>
        /// <param name="dayOfWeek"></param>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _clock1.SetClock(2014, 2, 28, 2, 15); // Note: the minutes, seconds and milliseconds parameters are optional and default to Zero (0).
        /// </code>
        /// <code> language = "VB"
        /// _clock1.SetClock(2014, 2, 28, 2, 15) ' Note: the minutes, seconds and milliseconds parameters are optional and default to Zero (0).
        /// </code>
        /// </example>
        public void SetClock(int year, int month, int day, int hour, int minutes = 0, int seconds = 0, int milliSeconds = 0, DayOfWeek dayOfWeek = DayOfWeek.Sunday)
        {
            // First we need to stop incrementing the clock to update the current time of the RTC .
            Configuration config = Configuration.GetConfiguration();
            config.HoldLastCountFlag = true;
            config.StopCountingFlag = true;
            Configuration.SetConfiguration(config);

            // Set the time
            byte[] clockData =
            {
                ((byte) (milliSeconds/10)).ToBcd(),
                ((byte) seconds).ToBcd(),
                ((byte) minutes).ToBcd(),
                ((byte) hour).ToBcd(),
                (byte) (((byte) (year % 4) << 6) | ((byte) day).ToBcd()),
                (byte) (((byte) month).ToBcd() | ((byte) (dayOfWeek) << 5))
            };

            WriteRegister(ControlHundredthsSeconds, clockData);

            // Now save the DateTime in SRAM for later comparison. It will be used to determine the correct year in the GetDateTime function.
            // This must be written to SRAM as the PCF8583 only has four year clock. First four (4) bytes are the Base Year and byte five (5) is the year  (0-3) of the RTC.
            // We use byte five (5) for later comparison to see if the Year has rolled over to the next year.
            // Byte six (6) as a flag to notify that the DateTime on RTC has been set. 
            int yearBase = GetBaseYear(year);
            var sramData = new[]
            {
                (byte) (yearBase / 1000),
                (byte) (yearBase / 100 % 10),
                (byte) (yearBase / 10 % 10),
                (byte) (yearBase % 10),
                (byte) (year % 4),
                (byte) 1
            };

            WriteRegister(BaseYearHoldingRegister, sramData);

            // Now start incrementing the Clock again.
            config.HoldLastCountFlag = false;
            config.StopCountingFlag = false;
            Configuration.SetConfiguration(config);
        }

        /// <summary>
        ///     Starts the clock incrementing the DateTime.
        /// </summary>
        /// <example>Example usage to start the RTC incrementing the DateTime.
        /// <code language = "C#">
        /// _clock1.StartClock();
        /// </code>
        /// <code language = "VB">
        /// _clock1.StartClock()
        /// </code>
        /// </example>
        public void StartClock()
        {
            var config = Configuration.GetConfiguration();
            if (config.StopCountingFlag && config.HoldLastCountFlag)
            {
                config.StopCountingFlag = false;
                config.HoldLastCountFlag = false;
                Configuration.SetConfiguration(config);
            }
        }

        /// <summary>
        ///     Stops the clock from incrementing the DateTime, all values will be preserved until resumed with the <see cref="StartClock"/> method.
        /// </summary>
        /// <example>Example usage to start the RTC incrementing the DateTime.
        /// <code language = "C#">
        /// _clock1.StopClock();
        /// </code>
        /// <code language = "VB">
        /// _clock1.StopClock()
        /// </code>
        /// </example>
        public void StopClock()
        {
            var config = Configuration.GetConfiguration();
            if (config.StopCountingFlag == false && config.HoldLastCountFlag == false)
            {
                config.StopCountingFlag = true;
                config.HoldLastCountFlag = true;
                Configuration.SetConfiguration(config);
            }
        }

        #endregion

        #region Alarm Methods

        /// <summary>
        ///     When an alarm is triggered on the PCF8583 IC on the <see cref="RTCClick"/>, the AlarmFlag bit (Memory Location 0x00 Bit 2) is set.
        ///     For the alarm to be re-triggered at is's next scheduled occurrence, the Alarm Flag bit must be cleared.
        /// </summary>
        ///<remarks>Use this method to clear the AlarmFlag when the <see cref="AlarmAutoReset"/> property is set to false.</remarks>
        /// <example>Example usage to clear the Alarm flag.
        /// <code language = "C#">
        /// _clock1.ClearAlarmFlag();
        /// </code>
        /// <code language = "VB">
        /// _clock1.ClearAlarmFlag()
        /// </code>
        /// </example>
        public void ClearAlarmFlag()
        {
            var control = ReadByte(ControlandStatusRegister);
            Bits.Set(ref control, 1, false);
            WriteByte(ControlandStatusRegister, control);
        }

        /// <summary>
        ///     Toggles the alarm on or off.
        /// </summary>
        /// <param name="on">Enables Alarm Triggering. To enable set to true or otherwise false.</param>
        /// <example>Example usage to enable or disable an alarm.
        /// <code language = "C#">
        /// _clock1.Enable(true); // This will enable any previously set alarms. To disable, pass false to the method.
        /// </code>
        /// <code language = "VB">
        /// _clock1.Enable(true) ' This will enable any previously set alarms. To disable, pass false to the method.
        /// </code>
        /// </example>
        public void EnableAlarm(bool on)
        {
            var control = ReadByte(ControlandStatusRegister);
            control = Bits.Set(control, 2, on);
            WriteByte(ControlandStatusRegister, control);

            control = ReadByte(AlarmControlRegister);
            control = Bits.Set(control, 7, on);
            WriteByte(AlarmControlRegister, control);

            if (on)
            {
                _alarmInterruptPort.EnableInterrupt();
            }
            else
            {
                _alarmInterruptPort.DisableInterrupt();
            }
        }

        /// <summary>
        ///     Returns the <see cref="WeekDays"/> setting for a Weekday alarm.
        /// </summary>
        /// <returns>The weekday that a WeekDay alarm is scheduled. See <see cref="WeekDays"/> and <see cref="SetWeekdayAlarm"/> for more information.</returns>
        /// <example>Example usage to retrieve the day of the week that a Weekday Alarm is scheduled.
        /// <code language = "C#">
        /// _clock1.GetAlarmWeekdaySetting();
        /// </code>
        /// <code language = "VB">
        /// _clock1.GetAlarmWeekdaySetting()
        /// </code>
        /// </example>
        public WeekDays GetAlarmWeekdaySetting()
        {
            return (WeekDays) ReadByte(AlarmControlWeekDayMonth);
        }

        /// <summary>
        ///    Returns all of the alarm settings. 
        ///  </summary>
        /// <returns>The all of the alarm settings alarm is scheduled. See <see cref="AlarmData"/> for more information.</returns>
        /// <example>Example usage to retrieve the alarm settings.
        /// <code language = "C#">
        /// RTCClick.AlarmData aData = _clock1.GetAlarmSettings();
        /// Debug.Print("Hundreds - " + aData.Time.HundredthsSecond);
        /// Debug.Print("Seconds - " + aData.Time.Seconds);
        /// Debug.Print("Minutes - " + aData.Time.Minutes);
        /// Debug.Print("Hours - " + aData.Time.Hours);
        /// Debug.Print("Day - " + aData.Day);
        /// Debug.Print("Day of Week - " + (aData.DayOfWeek == RTCClick.WeekDays.Sunday ? "Sunday" : aData.DayOfWeek == RTCClick.WeekDays.Monday ? "Monday" : aData.DayOfWeek == RTCClick.WeekDays.Tuesday ? "Tuesday" : aData.DayOfWeek == RTCClick.WeekDays.Wednesday ? "Wednesday" : aData.DayOfWeek == RTCClick.WeekDays.Thursday ? "Thursday" : aData.DayOfWeek == RTCClick.WeekDays.Friday ? "Friday" :  aData.DayOfWeek == RTCClick.WeekDays.Saturday ? "Saturday " : aData.DayOfWeek == RTCClick.WeekDays.Weekdays ? "Weekdays" : aData.DayOfWeek == RTCClick.WeekDays.Weekends ? "Weekends" : aData.DayOfWeek == RTCClick.WeekDays.AllDays ? "All Days" : aData.DayOfWeek == RTCClick.WeekDays.None ? "None" : "Multiple Days"));
        /// Debug.Print("Month - " + aData.Month);
        /// Debug.Print("Alarm Type - " + (aData.AlarmType == RTCClick.AlarmBehavior.DailyAlarm ? "Daily Alarm" : aData.AlarmType == RTCClick.AlarmBehavior.WeekDayAlarm ? "Weekday Alarm" :  aData.AlarmType == RTCClick.AlarmBehavior.DatedAlarm ? "Dated Alarm" : "No Alarm"));
        /// </code>
        /// <code language = "VB">
        /// Dim aData As RTCClick.AlarmData = _clock1.GetAlarmSettings()
        /// Debug.Print("Hundreds - " <![CDATA[&]]> aData.Time.HundredthsSecond)
        /// Debug.Print("Seconds - " <![CDATA[&]]> aData.Time.Seconds)
        /// Debug.Print("Minutes - " <![CDATA[&]]> aData.Time.Minutes)
        /// Debug.Print("Hours - " <![CDATA[&]]> aData.Time.Hours)
        /// Debug.Print("Day - " <![CDATA[&]]> aData.Day)
        /// Debug.Print("Day of Week - " <![CDATA[&]]> (If(aData.DayOfWeek = RTCClick.WeekDays.Sunday, "Sunday", If(aData.DayOfWeek = RTCClick.WeekDays.Monday, "Monday", If(aData.DayOfWeek = RTCClick.WeekDays.Tuesday, "Tuesday", If(aData.DayOfWeek = RTCClick.WeekDays.Wednesday, "Wednesday", If(aData.DayOfWeek = RTCClick.WeekDays.Thursday, "Thursday", If(aData.DayOfWeek = RTCClick.WeekDays.Friday, "Friday", If(aData.DayOfWeek = RTCClick.WeekDays.Saturday, "Saturday ", If(aData.DayOfWeek = RTCClick.WeekDays.Weekdays, "Weekdays", If(aData.DayOfWeek = RTCClick.WeekDays.Weekends, "Weekends", If(aData.DayOfWeek = RTCClick.WeekDays.AllDays, "All Days", If(aData.DayOfWeek = RTCClick.WeekDays.None, "None", "Multiple Days")))))))))))))
        /// Debug.Print("Month - " <![CDATA[&]]> aData.Month)
        /// Debug.Print("Alarm Type - " <![CDATA[&]]> (If(aData.AlarmType = RTCClick.AlarmBehavior.DailyAlarm, "Daily Alarm", If(aData.AlarmType = RTCClick.AlarmBehavior.WeekDayAlarm, "Weekday Alarm", If(aData.AlarmType = RTCClick.AlarmBehavior.DatedAlarm, "Dated Alarm", "No Alarm")))))
        /// </code>
        /// </example>
        public AlarmData GetAlarmSettings()
        {
            if (!IsAlarmTimeSet) return new AlarmData();

            var alarmData = new AlarmData();
            var timeData = new Time();

            var data = ReadRegister(AlarmControlHundredthsSecond);
            timeData.HundredthsSecond = data[0].FromBcd();
            timeData.Seconds = data[1].FromBcd();
            timeData.Minutes = data[2].FromBcd();
            timeData.Hours = data[3].FromBcd();

            alarmData.Time = timeData;

            if (GetAlarmType() == AlarmBehavior.DatedAlarm)
            {
                alarmData.DayOfWeek = WeekDays.None;
                alarmData.Day = ((byte) (data[4] & 0x3f)).FromBcd();
                alarmData.Month = data[5].FromBcd();
            }

            if (GetAlarmType() == AlarmBehavior.WeekDayAlarm)
            {
                alarmData.DayOfWeek = (WeekDays) data[5];
                alarmData.Day = 0;
                alarmData.Month = 0;
            }

            if (GetAlarmType() == AlarmBehavior.DailyAlarm)
            {
                alarmData.DayOfWeek = WeekDays.None;
                alarmData.Day = 0;
                alarmData.Month = 0;
            }

            alarmData.AlarmType = GetAlarmType();

            return alarmData;
        }

        /// <summary>
        ///     Clears the alarm settings to default values.
        /// </summary>
        /// <remarks>This will completely reset the alarm settings and disable all alarming capability. If you would like to disable the alarm but retain the settings, use the <see cref="EnableAlarm"/> method instead.</remarks>
        /// <example>Example usage to clear the alarm settings.
        /// <code language = "C#">
        /// _clock1.ClearAlarmSettings();
        /// </code>
        /// <code language = "VB">
        /// _clock1.ClearAlarmSettings();
        /// </code>
        /// </example>
        public void ClearAlarmSettings()
        {
            byte[] alarmData = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

            // Write the alarm data to the Alarm Register at 0x08
            WriteRegister(AlarmControlRegister, alarmData);
            WriteByte(AlarmControlWeekDayMonth, 0x00);

            SetClockModeAlarmType(AlarmBehavior.NoAlarm);

            var config = Configuration.GetConfiguration();
            config.AlarmEnableFlag = false;
            config.AlarmFlag = false;
            Configuration.SetConfiguration(config);

            _alarmInterruptPort.DisableInterrupt();
        }

        /// <summary>
        ///     Gets the alarm type for a Clock Mode alarm.
        /// </summary>
        /// <returns>The alarm type. See <see cref="AlarmBehavior"/> for more information.</returns>
        /// <example>Example usage to retrieve the alarm type.
        /// <code language = "C#">
        /// var at = _clock1.GetAlarmType();
        /// Debug.Print("Alarm type - " + (at == RTCClick.AlarmBehavior.DailyAlarm ? "Daily Alarm" : at == RTCClick.AlarmBehavior.WeekDayAlarm ? "Weekday Alarm" : at == RTCClick.AlarmBehavior.DatedAlarm ? "Dated Alarm" : "No Alarm is set."));
        /// </code>
        /// <code language = "VB">
        /// Dim at = _clock1.GetAlarmType()
        /// Debug.Print("Alarm type - " <![CDATA[&]]> (If(at = RTCClick.AlarmBehavior.DailyAlarm, "Daily Alarm", If(at = RTCClick.AlarmBehavior.WeekDayAlarm, "Weekday Alarm", If(at = RTCClick.AlarmBehavior.DatedAlarm, "Dated Alarm", "No Alarm is set.")))))
        /// </code>
        /// </example>
        public AlarmBehavior GetAlarmType()
        {
            var alarmType = ReadByte(AlarmControlRegister);

            if (alarmType.IsBitSet(4) && !alarmType.IsBitSet(5)) return AlarmBehavior.DailyAlarm;
            if (!alarmType.IsBitSet(4) && alarmType.IsBitSet(5)) return AlarmBehavior.WeekDayAlarm;
            if (alarmType.IsBitSet(4) && alarmType.IsBitSet(5)) return AlarmBehavior.DatedAlarm;

            return AlarmBehavior.NoAlarm;
        }

        /// <summary>
        ///     Sets a Daily Alarm using a <see cref="Time"/> structure that will happen at the selected time every day.
        /// </summary>
        /// <param name="time">The <see cref="Time"/> structure value at which the alarm will be triggered.</param>
        /// <remarks>Alarms are not enabled by default. If you want the alarm to be auto enabled when calling this method, set the <see cref="AlarmAutoEnable"/> property to true or use the <see cref="EnableAlarm"/> method.</remarks>
        /// <remarks>Once an alarm event is triggered, the AlarmFlag is set and must be cleared for the alarm to be triggered at it next scheduled occurrence. If you want the AlarmFlag to be automatically reset, set the <see cref="AlarmAutoReset"/> property to true or use the <see cref="ClearAlarmFlag"/> method.</remarks>
        /// <example>Example usage to set a Daily Alarm using a <see cref="Time"/> Structure as a parameter.
        /// <code language = "C#">
        /// var alarmTime = new RTCClick.Time {HundredsSeconds = 50, Seconds = 30, Minutes = 15, Hours = 8};
        /// _clock1.SetAlarm(alarmTime);
        /// </code>
        /// <code language = "VB">
        /// Dim alarmTime As New RTCClick.Time
        /// With alarmTime
        ///     .HundredsSeconds = 50
        ///     .Seconds = 30
        ///     .Minutes = 15
        ///     .Hours = 8
        /// End With
        /// _clock1.SetAlarm(alarmTime)
        /// </code>
        /// </example>
        public void SetAlarm(Time time)
        {
            SetDailyAlarm(time.Hours, time.Minutes, time.Seconds, time.HundredthsSecond);
        }

        /// <summary>
        ///     Sets a Dated Alarm using a <see cref="DateTime"/> structure that will happen at the selected time on the selected date and time.
        /// </summary>
        /// <param name="time">The date and time<![CDATA["]]><see cref="DateTime"/><![CDATA["]]> at which the alarm will be triggered.</param>
        /// <param>The year parameter is ignored for most dated alarms. The only exception is when setting a Dated Alarm that occurs on Leap Day (February 29) in a Leap Year. When setting a dated alarm for February 29, the alarm will only be triggered every four years on February 29 and does not roll over to March 1 every year. In the case of a Dated alarm being set for Leap Day, the Year parameter must be a valid Leap year or an exception will be thrown.</param>
        /// <remarks>Values for Month, Day, Hour, Minute, Second and Milliseconds will be constrained to values appropriate for the device.</remarks>
        /// <remarks>Alarms are not enabled by default. If you want the alarm to be auto enabled when calling this method, set the <see cref="AlarmAutoEnable"/> property to true or use the <see cref="EnableAlarm"/> method.</remarks>
        /// <remarks>Once an alarm event is triggered, the AlarmFlag is set and must be cleared for the alarm to be triggered at it next scheduled occurrence. If you want the AlarmFlag to be automatically reset, set the <see cref="AlarmAutoReset"/> property to true or use the <see cref="ClearAlarmFlag"/> method. </remarks>
        /// <exception cref="ArgumentException">An <see cref="ArgumentException"/> will be thrown if the Day and Month parameters passed are Zero (0) or an invalid year parameter is passed for a Leap Day (February 29) Dated Alarm.</exception>
        /// <example>Example usage to set a Dated Alarm using a <see cref="DateTime"/> Structure as a parameter.
        /// <code language = "C#">
        /// var aDate = new DateTime(2014, 8, 18, 23, 59, 55);
        /// _clock1.SetClock(aDate);
        /// </code>
        /// <code language = "VB">
        /// Dim aDate = New DateTime(2014, 8, 18, 23, 59, 55)
        /// _clock1.SetClock(aDate)
        /// </code>
        /// </example>
        public void SetAlarm(DateTime time)
        {
            SetDatedAlarm(time.Year, (byte) time.Month, (byte) time.Day, (byte) time.Hour, (byte) time.Minute, (byte) time.Millisecond);
        }

        /// <summary>
        ///     Sets a WeekDay Alarm using a <see cref="Time"/> structure that will happen at the selected time on the selected day(s) of the week.
        /// </summary>
        /// <param name="time">The <see cref="Time"/> structure value at which the alarm will be triggered.</param>
        /// <param name="dayOfWeek">The <see cref="WeekDays"/> that the alarm is to be triggered. The WeekDays enumeration can be treated as a bit field; that is, a set of flags. For example multiple days can be selected.</param>
        /// <remarks>Alarms are not enabled by default. If you want the alarm to be auto enabled when calling this method, set the <see cref="AlarmAutoEnable"/> property to true or use the <see cref="EnableAlarm"/> method.</remarks>
        /// <remarks>Once an alarm event is triggered, the AlarmFlag is set and must be cleared for the alarm to be triggered at it next scheduled occurrence. If you want the AlarmFlag to be automatically reset, set the <see cref="AlarmAutoReset"/> property to true or use the <see cref="ClearAlarmFlag"/> method.</remarks>
        /// <example>Example usage to set a Weekday Alarm using a <see cref="Time"/> Structure and <see cref="WeekDays"/> enumeration as parameters.
        /// <code language = "C#">
        /// var alarmTime = new RTCClick.Time {HundredsSeconds = 50, Seconds = 30, Minutes = 15, Hours = 8};
        /// var weekDays = RTCClick.WeekDays.AllDays <![CDATA[&]]> ~RTCClick.WeekDays.Saturday;
        /// _clock1.SetAlarm(alarmTime, weekdays);
        /// </code>
        /// <code language = "VB">
        /// Dim alarmTime As New RTCClick.Time
        /// With alarmTime
        ///     .HundredsSeconds = 50
        ///     .Seconds = 30
        ///     .Minutes = 15
        ///     .Hours = 8
        /// End With
        /// Dim weekdays As RTCClick.WeekDays = RTCClick.WeekDays.AllDays And Not RTCClick.WeekDays.Saturday
        /// _clock1.SetAlarm(alarmTime, weekdays)
        /// </code>
        /// </example>
        public void SetAlarm(Time time, WeekDays dayOfWeek)
        {
            SetWeekdayAlarm(dayOfWeek, time.Hours, time.Minutes, time.Seconds, (byte) (time.HundredthsSecond/10));
        }

        /// <summary>
        ///     Sets an alarm that will happen at the same time every day of the week.
        /// </summary>
        /// <param name="hour">The hour (24 Hour clock) that the alarm will be triggered. Valid values are 0 to 23.</param>
        /// <param name="minute">The minute that the alarm will be triggered. Valid values are 0 to 59.</param>
        /// <param name="second">The second that the alarm will be triggered. Valid values are 0 to 59.</param>
        /// <param name="milliSecond">Optional Parameter - The milliseconds that the alarm will be triggered. Valid values are 0 to 999.</param>
        /// <example>Example usage to set a Daily Alarm using hour, minute, second and (optional) millisecond as parameters.
        /// <code language = "C#">
        /// _clock1.SetDailyAlarm(0, 0, 5);
        /// </code>
        /// <code language = "VB">
        /// _clock1.SetDailyAlarm(0, 0, 5)
        /// </code>
        /// </example>
        public void SetDailyAlarm(byte hour, byte minute, byte second, int milliSecond = 0)
        {
            ClearAlarmSettings();

            // Set the alarm values
            byte[] alarmData =
            {
                ((byte) (milliSecond/10)).Constrain(0, 99).ToBcd(),
                second.Constrain(0, 59).ToBcd(),
                minute.Constrain(0, 59).ToBcd(),
                hour.Constrain(0, 23).ToBcd()
            };

            WriteRegister(AlarmControlHundredthsSecond, alarmData);

            SetClockModeAlarmType(AlarmBehavior.DailyAlarm);

            EnableAlarm(_alarmAutoEnable);

            ClearAlarmFlag();
        }

        /// <summary>
        ///     A Dated Alarm will occur on the same date and time every year. This would be useful for setting a reminder for events that occur yearly. I.E. - a birthday or anniversary reminder.
        /// </summary>
        /// <param name="year">The year parameter is ignored for most dated alarms. The only exception is when setting a Dated Alarm that occurs on Leap Day (February 29) in a Leap Year. When setting a dated alarm for February 29, the alarm will only be triggered every four years on February 29 and does not roll over to March 1 every year. In the case of a Dated alarm being set for Leap Day, the Year parameter must be a valid Leap year or an exception will be thrown.</param>
        /// <param name="month">The month in which the alarm will be triggered. Valid values are 1 to 12.</param>
        /// <param name="day">The day in which the alarm will be triggered. Valid values 1 to 31 depending on the month and Leap Year.</param>
        /// <param name="hour">The hour (24 Hour clock) that the alarm will be triggered. Valid values are 0 to 23.</param>
        /// <param name="minute">The minute that the alarm will be triggered. Valid values are 0 to 59.</param>
        /// <param name="second">The second that the alarm will be triggered. Valid values are 0 to 59.</param>
        /// <param name="milliSecond">Optional Parameter - The milliseconds that the alarm will be triggered. Valid values are 0 to 999.</param>
        /// <remarks>Values for Month, Day, Hour, Minute, Second and Milliseconds will be constrained to values appropriate for the device.</remarks>
        /// <remarks>Alarms are not enabled by default. If you want the alarm to be auto enabled when calling this method, set the <see cref="AlarmAutoEnable"/> property to true or use the <see cref="EnableAlarm"/> method.</remarks>
        /// <remarks>Once an alarm event is triggered, the AlarmFlag is set and must be cleared for the alarm to be triggered at it next scheduled occurrence. If you want the AlarmFlag to be automatically reset, set the <see cref="AlarmAutoReset"/> property to true or use the <see cref="ClearAlarmFlag"/> method. </remarks>
        /// <exception cref="ArgumentException">An <see cref="ArgumentException"/> will be thrown if the Day and Month parameters passed are Zero (0) or an invalid year parameter is passed for a Leap Day (February 29) Dated Alarm.</exception>
        /// <example>Example usage to set a Dated Alarm using year, month, hour, minute, second and (optional) millisecond as parameters.
        /// <code language = "C#">
        /// _clock1.SetDatedAlarm(2016, 8, 19, 0, 0, 5);
        /// </code>
        /// <code language = "VB">
        /// _clock1.SetDatedAlarm(2016, 8, 19, 0, 0, 5)
        /// </code>
        /// </example>
        public void SetDatedAlarm(int year, byte month, byte day, byte hour, byte minute, byte second, int milliSecond = 0)
        {
            // Do some basic error checking.
            if ((day == 0) || (month == 0)) throw new ArgumentException("Invalid Day and/or Month parameters passed. Valid Day parameters are 1 to 31 depending on the month and Leap Year and valid Month parameters are 1 to 12 for a Dated Alarm.");
            if ((month == 2) && (DateTime.DaysInMonth(year, 2) != 29)) throw new ArgumentException("Invalid Year parameter passed. When specifying the date of February 29 (Leap Day), you must pass a valid Leap Year as the Year parameter for a Dated Alarm");

            // clear the previous alarm settings.
            ClearAlarmSettings();

            // Determine the day max value based on the month parameter passed to the method.
            byte dayMaxValue = 1;
            if (month == 12 || month == 10 || month == 8 || month == 7 || month == 5 || month == 3 || month == 1)
            {
                dayMaxValue = 31;
            }
            else if (month == 11 || month == 9 || month == 6 || month == 4)
            {
                dayMaxValue = 30;
            }
            else if (month == 2)
            {
                dayMaxValue = (byte) (DateTime.DaysInMonth(year, 2) == 29 ? 29 : 28);
            }

            // Check and initialize the Alarm parameter for proper initialization. Overflows will be corrected or constrained.
            // Set the alarm values
            byte[] alarmData =
            {
                ((byte) (milliSecond/10)).Constrain(0, 99).ToBcd(),
                second.Constrain(0, 59).ToBcd(),
                minute.Constrain(0, 59).ToBcd(),
                hour.Constrain(0, 23).ToBcd(),
                day.Constrain(1, dayMaxValue).ToBcd(),
                month.Constrain(1, 12).ToBcd()
            };

            // Write the alarm data to the Alarm Register at 0x08
            WriteRegister(AlarmControlHundredthsSecond, alarmData);

            SetClockModeAlarmType(AlarmBehavior.DatedAlarm);

            EnableAlarm(_alarmAutoEnable);

            ClearAlarmFlag();
        }

        /// <summary>
        ///     Sets an alarm that will happen at the time on the selected day of the week.
        /// </summary>
        /// <param name="dayOfWeek">The <see cref="WeekDays"/> that the alarm is to be triggered. The WeekDays enumeration can be treated as a bit field; that is, a set of flags. For example multiple days can be selected.</param>
        /// <param name="hour">The hour (24 Hour clock) that the alarm will be triggered. Valid values are 0 to 23.</param>
        /// <param name="minute">The minute that the alarm will be triggered. Valid values are 0 to 59.</param>
        /// <param name="second">The second that the alarm will be triggered. Valid values are 0 to 59.</param>
        /// <param name="milliSecond">Optional Parameter - The milliseconds that the alarm will be triggered. Valid values are 0 to 999.</param>
        /// <remarks>Values for Hour, Minute, Second and Milliseconds will be constrained to values appropriate for the device.</remarks>
        /// <remarks>Alarms are not enabled by default. If you want the alarm to be auto enabled when calling this method, set the <see cref="AlarmAutoEnable"/> property to true or use the <see cref="EnableAlarm"/> method.</remarks>
        /// <remarks>Once an alarm event is triggered, the AlarmFlag is set and must be cleared for the alarm to be triggered at it next scheduled occurrence. If you want the AlarmFlag to be automatically reset, set the <see cref="AlarmAutoReset"/> property to true or use the <see cref="ClearAlarmFlag"/> method. </remarks>
        /// <example>Example usage to set a Weekday Alarm using the WeekDays enumeration and hour, minute, second and (optional) millisecond as parameters.
        /// <code language = "C#">
        /// _clock1.SetWeekdayAlarm(RTCClick.WeekDays.Weekdays, 8, 30, 0);
        /// </code>
        /// <code language = "VB">
        /// _clock1.SetWeekdayAlarm(RTCClick.WeekDays.Weekdays, 8, 30, 0)
        /// </code>
        /// </example>
        public void SetWeekdayAlarm(WeekDays dayOfWeek, byte hour, byte minute, byte second, int milliSecond = 0)
        {
            ClearAlarmSettings();

            // Set the alarm values
            byte[] alarmData =
            {
                ((byte) (milliSecond/10)).Constrain(0, 99).ToBcd(),
                second.Constrain(0, 59).ToBcd(),
                minute.Constrain(0, 59).ToBcd(),
                hour.Constrain(0, 23).ToBcd(),
                0x00, // the Day is ignored in a Weekday Alarm.
                ((byte) (dayOfWeek)).Constrain(0, 127)
            };

            WriteRegister(AlarmControlHundredthsSecond, alarmData);

            SetClockModeAlarmType(AlarmBehavior.WeekDayAlarm);

            EnableAlarm(_alarmAutoEnable);

            ClearAlarmFlag();
        }

        #endregion

        #region Device Level Methods

        /// <summary>
        ///     Resets the RTCClick.
        /// </summary>
        /// <param name="resetMode">The reset mode, see <see cref="ResetModes"/> for more information.</param>
        /// <returns cref="NotImplementedException">Calling this method will throw a <see cref="NotImplementedException"/>.</returns>
        /// <remarks>
        ///     This module has no Reset method, calling this method will throw a <see cref="NotImplementedException"/>.
        /// </remarks>
        /// <exception cref="NotImplementedException"></exception>
        /// <example>None: This sensor does not support a Reset method.</example>
        public bool Reset(ResetModes resetMode)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region User Ram Methods

        /// <summary>
        ///     Writes to the user RAM registers as a block of memory.
        /// </summary>
        /// <param name="buffer">A byte buffer to write to available User Ram.</param>
        /// <remarks>Over writes all user ram with new data.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">The buffer passed must not exceed the size of <see cref="UserRam.RamSize"/> or an exception will be thrown.</exception>
        /// <example>Example usage to write User Ram as a block of memory.
        /// <code language = "C#">
        /// // Create a string of length of UserRamSize minus 1 byte.
        /// string stringSymbols = new string('_', (byte)RTCClick.UserRam.RamSize - 1);
        /// stringSymbols += "e"; // add an arbitrary character to the end of the string.    
        /// byte[] userRam = new byte[(byte)RTCClick.UserRam.RamSize];
        /// // Copy the string to the ram buffer
        /// for (byte b = 0; b <![CDATA[<=]]> stringSymbols.Length - 1; b++)
        /// {
        ///     userRam[b] = (byte)stringSymbols[b];
        /// }
        /// // Write it to the User RAM.
        /// _clock1.WriteUserRam(userRam);
        /// </code>
        /// <code language = "VB">
        /// ' Create a string of length of UserRamSize minus 1 byte.
        /// Dim stringSymbols As New String("_"C, CByte(RTCClick.UserRam.RamSize) - 1)
        /// stringSymbols += "e"
        /// ' add an arbitrary character to the end of the string.
        /// Dim userRam As Byte() = New Byte(CByte(RTCClick.UserRam.RamSize) - 1) {}
        /// ' Copy the string to the ram buffer
        /// For b As Byte = 0 To stringSymbols.Length - 1
        ///     userRam(b) = CByte(AscW(stringSymbols(b)))
        /// Next
        /// ' Write it to the User RAM.
        /// _clock1.WriteUserRam(userRam)
        /// </code>
        /// </example>
        public void WriteUserRam(byte[] buffer)
        {
            if (buffer.Length > (byte)UserRam.RamSize) { throw new ArgumentOutOfRangeException("buffer", "Invalid buffer length. Buffer is larger than available user ram"); }

            var trxBuffer = new byte[sizeof(byte) + (byte)UserRam.RamSize];
            trxBuffer[0] = (byte)UserRam.StartAddressBlock;
            buffer.CopyTo(trxBuffer, 1);
            WriteRegister((byte)UserRam.StartAddressBlock, trxBuffer);
        }

        /// <summary>
        ///     Writes a to SRAM at the specified address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <param name="value">The byte to write.</param>
        /// <example>Example usage to write User Ram at the specified address.
        /// <code language = "C#">
        /// for (int b = (byte)_clock1.UserRam.StartAddressBlock; b <![CDATA[<=]]> (byte)_clock1.UserRam.EndAddressBlock; b++)
        /// {
        ///    _clock1.WriteUserRamAddress((byte)b, (byte)b);
        /// }
        /// // or a much simpler version:
        /// _clock1.WriteUserRamAddress(0xFf, (byte)'c');
        /// </code>
        /// <code language = "VB">
        /// For b As Integer = _clock1.UserRamStartAddressBlock To _clock1.UserRamEndAddressBlock
        ///     _clock1.WriteUserRamAddress(CByte(b), CByte(b))
        /// Next
        /// ' or a much simpler version:
        /// _clock1.WriteUserRamAddress(<![CDATA[&]]>HFF, CByte(AscW("c"C))) 
        /// </code>
        /// </example>
        public void WriteUserRamAddress(byte address, byte value)
        {
            if (address < (byte)UserRam.StartAddressBlock)
            {
                throw new ArgumentOutOfRangeException("address", "Cannot write to RAM address before the UserRamStartAddressBlock.");
            }
            if (address > (byte)UserRam.EndAddressBlock)
            {
                throw new ArgumentOutOfRangeException("address", "Cannot write to RAM address after the UserRamEndAddressBlock.");
            }
            WriteByte(address, value);
        }

        /// <summary>
        ///     Reads the User RAM registers as a block of memory.
        /// </summary>
        /// <returns>A byte array of size USER_RAM_SIZE containing the user RAM data</returns>
        /// <example>Example usage to read User Ram as a block of memory.
        /// <code language = "C#">
        /// Debug.Print("Reading from the RAM as a block of memory...");
        /// byte[] userRam = _clock1.ReadUserRam();
        /// var ramString = string.Empty;
        /// for (int i = 0; i <![CDATA[<=]]> userRam.Length - 1; i++)
        /// {
        ///     ramString += (char)userRam[i];
        /// }
        /// Debug.Print(ramString);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Reading from the RAM as a block of memory...")
        /// Dim userRam As Byte() = _clock1.ReadUserRam()
        /// Dim ramString = String.Empty
        /// For i As Integer = 0 To userRam.Length - 1
        ///     ramString += ChrW(userRam(i))
        /// Next
        /// Debug.Print(ramString)
        /// </code>
        /// </example>
        public byte[] ReadUserRam()
        {
            var data = new byte[(byte)UserRam.RamSize];

            var transaction = new I2CDevice.I2CTransaction[2];
            transaction[0] = I2CDevice.CreateWriteTransaction(new[] { (byte)UserRam.StartAddressBlock });
            transaction[1] = I2CDevice.CreateReadTransaction(data);
            Hardware.I2CBus.Execute(transaction, _i2CTimeout);
            return data;
        }

        /// <summary>
        ///     Reads User RAM  at the specified register address.
        /// </summary>
        /// <param name="address">Register address between 0x18 up to and including 0xFF (232 Bytes)</param>
        /// <returns>The value of the byte read at the specified address</returns>
        /// <exception cref="ArgumentOutOfRangeException">An ArgumentOutOfRange Exception will be thrown if the address passed to read is before the <see cref="UserRam.StartAddressBlock"/> or after the <see cref="UserRam.EndAddressBlock"/>.</exception>
        /// <example>Example usage to read User Ram at the specified address.
        /// <code language = "C#">
        /// for (int b = (byte)RTCClick.UserRam.StartAddressBlock; b <![CDATA[<=]]> (byte)RTCClick.UserRam.EndAddressBlock; b++)
        /// {
        ///     Debug.Print(b + ": " + _clock1.ReadUserRamAddress((byte)b));
        /// }
        /// // or a much simpler version:
        /// _clock1.ReadUserRamAddress(0xFF, (byte)'c');
        /// </code>
        /// <code language = "VB">
        /// For b As Integer = CByte(RTCClick.UserRam.StartAddressBlock) To CByte(RTCClick.UserRam.EndAddressBlock)
        ///     Debug.Print(b <![CDATA[&]]> ": " <![CDATA[&]]> _clock1.ReadUserRamAddress(CByte(b)))
        /// Next
        /// ' or a much simpler version:
        /// _clock1.ReadUserRamAddress(<![CDATA[&]]>HFF, CByte(AscW("c"C)))
        /// </code>
        /// </example>
        public byte ReadUserRamAddress(byte address)
        {
            if (address < (byte)UserRam.StartAddressBlock)
            {
                throw new ArgumentOutOfRangeException("address", "Cannot read from User RAM address before the UserRamStartAddressBlock.");
            }
            if (address > (byte)UserRam.EndAddressBlock)
            {
                throw new ArgumentOutOfRangeException("address", "Cannot read from User RAM address after the UserRamEndAddressBlock.");
            }
            return ReadByte(address);
        } 

        #endregion

        #endregion

        #region Public Properties

        /// <summary>
        ///     When a clock mode alarm is triggered on the PCF8583 IC on the <see cref="RTCClick"/>, the AlarmFlag bit (Memory Location 0x00 Bit 2) must be cleared manually for the alarm to be triggered at its next occurrence.
        ///     If you would like the driver to do this automatically for you, set the value of the AlarmAutoReset Property to True.   
        /// </summary>
        public bool AlarmAutoReset
        {
            get { return _alarmAutoReset; }
            set { _alarmAutoReset = value; }
        }

        /// <summary>
        ///     Alarm triggering is not enabled by default, if you would like any of the SetAlarm methods to automatically enable alarm triggering, set the value of this property to True.
        /// </summary>
        /// <remarks>The enable or disable alarm triggering </remarks>
        public bool AlarmAutoEnable
        {
            get { return _alarmAutoEnable; }
            set { _alarmAutoEnable = value; }
        }

        /// <summary>
        ///     Gets the driver version.
        /// </summary>
        /// <value>
        ///     The driver version see <see cref="Version"/>.
        /// </value>
        /// <example>Example usage:
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

        /// <summary>
        ///     This property is used to determine if an alarm is enabled.
        /// </summary>
        /// <returns>True if an alarm is enabled or otherwise false.</returns>
        public bool IsAlarmEnabled
        {
            get { return ReadByte(ControlandStatusRegister).IsBitSet(2) && ReadByte(AlarmControlRegister).IsBitSet(7); }
        }

        /// <summary>
        ///     This property returns whether a valid alarm setting has been set in the <see cref="RTCClick"/>.
        /// </summary>
        /// <returns>True if a valid alarm setting has been set in the RTC Click or otherwise false.</returns>
        public bool IsAlarmTimeSet
        {
            get
            {
                var temp = ReadRegister(AlarmControlRegister);
                return (temp[0] != 0 || temp[1] != 0 || temp[2] != 0 || temp[3] != 0) && GetAlarmType() != (byte) WeekDays.None;
            }
        }

        /// <summary>
        ///     This property is used to determine if the <see cref="DateTime"/> of the RTC Click has been set. 
        /// </summary>
        /// <returns>True if the DateTime of the <see cref="RTCClick"/> has been properly initialized or otherwise false.</returns>
        /// <remarks>This property does not guarantee that the DateTime that has been set is accurate.</remarks>
        public bool IsDateTimeSet
        {
            get { return (ReadRegister(BaseYearHoldingRegister)[5] == 1); }
        }

        /// <summary>
        ///     Gets or sets the power mode.
        /// </summary>
        /// <value>
        ///     The current power mode of the module.
        /// </value>
        /// <returns cref="NotImplementedException">Calling this method will throw a <see cref="NotImplementedException"/>.</returns>
        /// <remarks>
        ///     This module does not use Power Modes, the GET accessor will always return PowerModes.On. See <see cref="PowerModes"/>, while the SET accessor will throw a <see cref="NotImplementedException"/>.
        /// </remarks>
        /// <exception cref="NotImplementedException"></exception>
        /// <example>None: This sensor does not support PowerMode.</example>
        public PowerModes PowerMode
        {
            get { return PowerModes.On; }
            set { throw new NotImplementedException("Power Mode not implemented for this sensor"); }
        }

        #endregion

        #region ENUMS

        #region Public

        /// <summary>
        /// The various alarm types.
        /// </summary>
        public enum AlarmBehavior
        {
            /// <summary>
            ///     No Alarm.
            /// </summary>
            NoAlarm,

            /// <summary>
            ///     A daily alarm that happens on the same time every day.
            /// </summary>
            DailyAlarm = 1,

            /// <summary>
            ///     A weekday alarm that happens on the same time for the selected day of the week.
            /// </summary>
            WeekDayAlarm = 2,

            /// <summary>
            ///     A dated alarm that happens on a specific date and time.
            /// </summary>
            DatedAlarm = 3
        }

        /// <summary>
        ///     The days of the week enumeration used for setting a Weekday Alarm.     
        /// </summary>
        /// <remarks>The WeekDays enumeration can be treated as a bit field; that is, a set of flags. For example multiple days can be selected.</remarks>
        /// <example>Example to select multiple days of the week.
        /// <code language = "C#">
        /// var weekDays = RTCClick.WeekDays.AllDays <![CDATA[&]]> ~RTCClick.WeekDays.Sunday;
        /// _clock1.SetWeekdayAlarm(weekDays, 08, 30, 00);
        ///  </code>
        /// <code language = "VB">
        /// Dim weekDays = RTCClick.WeekDays.AllDays And Not RTCClick.WeekDays.Sunday
        /// _clock1.SetWeekdayAlarm(weekDays, 8, 30, 0)
        /// </code>
        /// </example>
        [Flags]
        public enum WeekDays : byte
        {
            /// <summary>
            ///     Day of the week - No Days.
            /// </summary>
            None = 0,

            /// <summary>
            ///     Indicates Sunday.
            /// </summary>
            Sunday = 1,

            /// <summary>
            ///     Indicates Monday.
            /// </summary>
            Monday = 2,

            /// <summary>
            ///     Indicates Tuesday.
            /// </summary>
            Tuesday = 4,

            /// <summary>
            ///     Indicates Wednesday.
            /// </summary>
            Wednesday = 8,

            /// <summary>
            ///     Indicates Thursday.
            /// </summary>
            Thursday = 16,

            /// <summary>
            ///     Indicates Friday.
            /// </summary>
            Friday = 32,

            /// <summary>
            ///     Indicates Saturday.
            /// </summary>
            Saturday = 64,

            /// <summary>
            ///     Indicates all weekdays Monday, Tuesday, Wednesday, Thursday and Friday.
            /// </summary>
            Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,

            /// <summary>
            ///     Indicates weekends Saturday and Sunday.
            /// </summary>
            Weekends = Saturday | Sunday,

            /// <summary>
            ///     Indicates all days Sunday, Monday, Tuesday, Wednesday, Thursday, Friday and Saturday. 
            /// </summary>
            AllDays = Weekdays | Weekends
        }

        /// <summary>
        ///     The values of various Ram settings.
        /// </summary>
        public enum UserRam
        {
        /// <summary>
        ///     The start addresses of the User RAM registers
        /// </summary>
        StartAddressBlock = 0x18,

        /// <summary>
        ///     The start address of the last byte of User RAM
        /// </summary>
        EndAddressBlock = 0xFF,

        /// <summary>
        ///     Total size of the user RAM block
        /// </summary>
        RamSize = 0xE8

        }

        #endregion

        #region Private

        internal enum Mode
        {
            ClockMode32Khz = 0x00,
            ClockMode50Hz = 0x01,
            EventCounter = 0x02,
            TestMode = 0x03 // Per data sheet - not recommended.
        }

        #endregion

        #endregion

        #region Configuration

        // This class to holds the values of the configuration data for the PCF8583 IC on the RTC click board.
        // The access level is internal for now. Will have to change to public once the other function modes are implemented.
        internal class Configuration
        {
            protected internal Configuration()
            {
            }

            protected internal bool TimerFlag { get; set; }
            protected internal bool AlarmFlag { get; set; }
            protected internal bool AlarmEnableFlag { get; set; }
            protected internal bool MaskFlag { get; set; }
            protected internal Mode FunctionModeFlag { get; set; }
            protected internal bool HoldLastCountFlag { get; set; }
            protected internal bool StopCountingFlag { get; set; }

            protected internal static Configuration GetConfiguration()
            {
                var currentConfiguration = new Configuration();
                byte config = ReadByte(ControlandStatusRegister);

                if (config == 0) return currentConfiguration;

                currentConfiguration.TimerFlag = config.IsBitSet(0);
                currentConfiguration.AlarmFlag = config.IsBitSet(1);
                currentConfiguration.AlarmEnableFlag = config.IsBitSet(2);
                currentConfiguration.MaskFlag = config.IsBitSet(3);

                if (!config.IsBitSet(4) && !config.IsBitSet(5)) currentConfiguration.FunctionModeFlag = Mode.ClockMode32Khz;
                if (!config.IsBitSet(4) && config.IsBitSet(5)) currentConfiguration.FunctionModeFlag = Mode.ClockMode50Hz;
                if (config.IsBitSet(4) && config.IsBitSet(5)) currentConfiguration.FunctionModeFlag = Mode.EventCounter;
                if (config.IsBitSet(4) && config.IsBitSet(5)) currentConfiguration.FunctionModeFlag = Mode.TestMode;

                currentConfiguration.HoldLastCountFlag = config.IsBitSet(6);
                currentConfiguration.StopCountingFlag = config.IsBitSet(7);

                return currentConfiguration;
            }

            protected internal static void SetConfiguration(Configuration rtcConfig)
            {
                const byte empty = (byte) 0x00;
                var timerFlag = (byte) (rtcConfig.TimerFlag ? 0x01 : empty);
                var alarmFlag = (byte) (rtcConfig.AlarmFlag ? 0x02 : empty);
                var alarmEnabled = (byte) (rtcConfig.AlarmEnableFlag ? 0x04 : empty);
                var maskFlag = (byte) (rtcConfig.MaskFlag ? 0x08 : empty);
                var mode = empty;
                var holdFlag = (byte) (rtcConfig.HoldLastCountFlag ? 0x40 : empty);
                var stopCountingFlag = (byte) (rtcConfig.StopCountingFlag ? 0x80 : empty);

                switch (rtcConfig.FunctionModeFlag)
                {
                    case Mode.ClockMode32Khz:
                        mode = 0x00;
                        break;
                    case Mode.ClockMode50Hz:
                        mode = 0x10;
                        break;
                    case Mode.EventCounter:
                        mode = 0x20;
                        break;
                    case Mode.TestMode:
                        mode = 0x30;
                        break;
                }

                var config = (byte) (empty | timerFlag | alarmFlag | alarmEnabled | maskFlag | mode | holdFlag | stopCountingFlag);

                WriteByte(ControlandStatusRegister, config);
            }
        }

        #endregion

        #region Events

        /// <summary>
        ///     The event that is raised on an Alarm condition is satisfied. 
        /// </summary>
        public event AlarmEventHandler AlarmTriggered;// = delegate { };

        /// <summary>
        ///     Represents the delegate that is used for the <see cref="RTCClick.AlarmTriggered"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="RTCClick"/> that raises the event.</param>
        /// <param name="e">The <see cref="AlarmEventArgs"/>.</param>
        public delegate void AlarmEventHandler(object sender, AlarmEventArgs e);

        private void AlarmInterruptPortOnInterruptPort(uint data1, uint data2, DateTime time)
        {
            if (data2 == 0)
            {
                if (_alarmAutoReset)
                {
                    ClearAlarmFlag();
                }

                AlarmEventHandler handler = AlarmTriggered;

                if (handler == null) return;

                handler(this, new AlarmEventArgs(GetAlarmType(), GetDateTime()));
            }
        }

        /// <summary>
        ///     A class holding the values of the Alarm Event that triggered the Alarm (the alarm type and time of the alarm occurrence) see <see cref="RTCClick.AlarmTriggered"/> event.
        /// </summary>
        public class AlarmEventArgs
        {
            internal AlarmEventArgs(RTCClick.AlarmBehavior alarmType, DateTime alarmEventTime)
            {
                AlarmType = alarmType;
                AlarmEventEventTime = alarmEventTime;
            }

            /// <summary>
            ///     The property holding the <see cref="AlarmType"/> that triggered the alarm.
            /// </summary>
            public RTCClick.AlarmBehavior AlarmType { get; private set; }

            /// <summary>
            ///     The property holding the <see cref="DateTime"/> that the alarm event occurred.
            /// </summary>
            public DateTime AlarmEventEventTime { get; private set; }
        }

        #endregion

        #region Time Structure

        /// <summary>
        ///     A structure containing the Time data that is used for various alarm methods. Consisting of the Hour, Minutes, Seconds and Hundredths of a Second.
        /// </summary>
        public struct Time
        {
            /// <summary>
            ///     The  time in Hundredths of a Second.
            /// </summary>
            public byte HundredthsSecond { get; set; }

            /// <summary>
            ///     The  time in Seconds.
            /// </summary>
            public byte Seconds { get; set; }

            /// <summary>
            ///     The  time in Minutes.
            /// </summary>
            public byte Minutes { get; set; }

            /// <summary>
            ///     The  time in Hours.
            /// </summary>
            public byte Hours { get; set; }

            /// <summary>
            ///     Default constructor
            /// </summary>
            /// <param name="hundredthSecond">The Hundredths of a second that the alarm will be triggered.</param>
            /// <param name="seconds">The seconds that the alarm will be triggered.</param>
            /// <param name="minutes">The minutes that the alarm will be triggered.</param>
            /// <param name="hours">The hour that the alarm will be triggered.</param>
            public Time(byte hundredthSecond, byte seconds, byte minutes, byte hours)
                : this()
            {
                HundredthsSecond = hundredthSecond;
                Seconds = seconds;
                Minutes = minutes;
                Hours = hours;
            }
        }

        #endregion

        #region AlarmData Structure

        /// <summary>
        ///     A structure consisting of the various alarm settings.
        /// </summary>
        public struct AlarmData
        {
            /// <summary>
            ///     The Time structure <see cref="Time"/>.
            /// </summary>
            public Time Time;

            /// <summary>
            ///     The day of the week. See <see cref="WeekDays"/>
            /// </summary>
            public WeekDays DayOfWeek;

            /// <summary>
            ///     The day of the month.  
            /// </summary>
            public byte Day;

            /// <summary>
            ///  The month.
            /// </summary>
            public byte Month;

            /// <summary>
            ///  The alarm type see <see cref="AlarmBehavior"/> for more information.
            /// </summary>
            public AlarmBehavior AlarmType;
        }

        #endregion
    }
}