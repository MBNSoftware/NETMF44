using System;

namespace MBN.Modules
{
    /// <summary>
    /// Delegate for the TagDetected event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="TagDetectedEventArgs"/> instance containing the event data.</param>
    public delegate void TagDetectedEventHandler(object sender, TagDetectedEventArgs e);

    /// <summary>
    /// Class holding arguments for the TagDetected event.
    /// </summary>
    public class TagDetectedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TagDetectedEventArgs" /> class.
        /// </summary>
        /// <param name="tagIDHex">The tag identifier hexadecimal.</param>
        /// <param name="tagID">The tag identifier.</param>
        /// <param name="crc">The CRC.</param>
        public TagDetectedEventArgs(String tagIDHex, UInt32 tagID, Byte crc)
        {
            TagID = tagID;
            TagIDHex = tagIDHex;
            CRC = crc;
        }

        /// <summary>
        /// Gets the tag identifier in a xx:yy:zz:tt form.
        /// </summary>
        /// <value>
        /// The tag identifier.
        /// </value>
        /// <example>
        /// <code language = "C#">
        /// 		public static void Main()
        ///         {
        ///                 _rfid = new RFIDClick(Hardware.SocketOne);
        ///                 InitLcd();
        /// 
        ///                 Debug.Print("RFID identification : " + _rfid.Identification());
        /// 
        ///                 _lcd.Write(1, 4, "Calibration...");
        ///                 _rfid.Calibration(Hardware.Led2);
        ///                 _lcd.Write(1, 4, "              ");
        /// 
        ///                 _rfid.TagDetected += _rfid_TagDetected;
        ///                 _rfid.TagRemoved += _rfid_TagRemoved;
        /// 
        ///                 // Starts detection
        ///                 _rfid.DetectionEnabled = true;
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        /// 
        ///         static void _rfid_TagRemoved(object sender, TagRemovedEventArgs e)
        ///         {
        ///             Hardware.Led1.Write(false);
        ///             _lcd.Write(1, 3, "                    ");
        ///             _lcd.Write(1, 4, "                    ");
        ///         }
        /// 
        ///         static void _rfid_TagDetected(object sender, TagDetectedEventArgs e)
        ///         {
        ///             Hardware.Led1.Write(true);
        ///             _lcd.Write(1, 3, e.TagID.ToString());
        ///             _lcd.Write(1, 4, e.TagIDHex+"  #"+e.CRC);
        ///         }
        /// 
        /// </code>
        /// </example>
        public String TagIDHex { get; private set; }

        /// <summary>
        /// Gets the tag identifier as a UInt32
        /// </summary>
        /// <value>
        /// The tag identifier.
        /// </value>
        /// <example>
        /// <code language = "C#">
        /// 		public static void Main()
        ///         {
        ///                 _rfid = new RFIDClick(Hardware.SocketOne);
        ///                 InitLcd();
        /// 
        ///                 Debug.Print("RFID identification : " + _rfid.Identification());
        /// 
        ///                 _lcd.Write(1, 4, "Calibration...");
        ///                 _rfid.Calibration(Hardware.Led2);
        ///                 _lcd.Write(1, 4, "              ");
        /// 
        ///                 _rfid.TagDetected += _rfid_TagDetected;
        ///                 _rfid.TagRemoved += _rfid_TagRemoved;
        /// 
        ///                 // Starts detection
        ///                 _rfid.DetectionEnabled = true;
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        /// 
        ///         static void _rfid_TagRemoved(object sender, TagRemovedEventArgs e)
        ///         {
        ///             Hardware.Led1.Write(false);
        ///             _lcd.Write(1, 3, "                    ");
        ///             _lcd.Write(1, 4, "                    ");
        ///         }
        /// 
        ///         static void _rfid_TagDetected(object sender, TagDetectedEventArgs e)
        ///         {
        ///             Hardware.Led1.Write(true);
        ///             _lcd.Write(1, 3, e.TagID.ToString());
        ///             _lcd.Write(1, 4, e.TagIDHex+"  #"+e.CRC);
        ///         }
        /// 
        /// </code>
        /// </example>
        public UInt32 TagID { get; private set; }

        /// <summary>
        /// Gets the CRC value of the tag ID
        /// </summary>
        /// <value>
        /// The tag identifier.
        /// </value>
        /// <example>
        /// <code language = "C#">
        /// 		public static void Main()
        ///         {
        ///                 _rfid = new RFIDClick(Hardware.SocketOne);
        ///                 InitLcd();
        /// 
        ///                 Debug.Print("RFID identification : " + _rfid.Identification());
        /// 
        ///                 _lcd.Write(1, 4, "Calibration...");
        ///                 _rfid.Calibration(Hardware.Led2);
        ///                 _lcd.Write(1, 4, "              ");
        /// 
        ///                 _rfid.TagDetected += _rfid_TagDetected;
        ///                 _rfid.TagRemoved += _rfid_TagRemoved;
        /// 
        ///                 // Starts detection
        ///                 _rfid.DetectionEnabled = true;
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        /// 
        ///         static void _rfid_TagRemoved(object sender, TagRemovedEventArgs e)
        ///         {
        ///             Hardware.Led1.Write(false);
        ///             _lcd.Write(1, 3, "                    ");
        ///             _lcd.Write(1, 4, "                    ");
        ///         }
        /// 
        ///         static void _rfid_TagDetected(object sender, TagDetectedEventArgs e)
        ///         {
        ///             Hardware.Led1.Write(true);
        ///             _lcd.Write(1, 3, e.TagID.ToString());
        ///             _lcd.Write(1, 4, e.TagIDHex+"  #"+e.CRC);
        ///         }
        /// 
        /// </code>
        /// </example>
        public Byte CRC { get; private set; }
    }

    /// <summary>
    /// Delegate for the TagRemoved event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="TagRemovedEventArgs"/> instance containing the event data.</param>
    public delegate void TagRemovedEventHandler(object sender, TagRemovedEventArgs e);

    /// <summary>
    /// Class holding arguments for the TagRemoved event.
    /// </summary>
    public class TagRemovedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TagRemovedEventArgs" /> class.
        /// </summary>
        /// <param name="tagIDHex">The tag identifier hexadecimal.</param>
        /// <param name="tagID">The tag identifier.</param>
        /// <param name="crc">The CRC.</param>
        public TagRemovedEventArgs(String tagIDHex, UInt32 tagID, Byte crc)
        {
            TagID = tagID;
            TagIDHex = tagIDHex;
            CRC = crc;
        }

        /// <summary>
        /// Gets the tag identifier in a xx:yy:zz:tt form.
        /// </summary>
        /// <value>
        /// The tag identifier.
        /// </value>
        /// <example>
        /// <code language = "C#">
        /// 		public static void Main()
        ///         {
        ///                 _rfid = new RFIDClick(Hardware.SocketOne);
        ///                 InitLcd();
        /// 
        ///                 Debug.Print("RFID identification : " + _rfid.Identification());
        /// 
        ///                 _lcd.Write(1, 4, "Calibration...");
        ///                 _rfid.Calibration(Hardware.Led2);
        ///                 _lcd.Write(1, 4, "              ");
        /// 
        ///                 _rfid.TagDetected += _rfid_TagDetected;
        ///                 _rfid.TagRemoved += _rfid_TagRemoved;
        /// 
        ///                 // Starts detection
        ///                 _rfid.DetectionEnabled = true;
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        /// 
        ///         static void _rfid_TagRemoved(object sender, TagRemovedEventArgs e)
        ///         {
        ///             Hardware.Led1.Write(false);
        ///             _lcd.Write(1, 3, "                    ");
        ///             _lcd.Write(1, 4, "                    ");
        ///         }
        /// 
        ///         static void _rfid_TagDetected(object sender, TagDetectedEventArgs e)
        ///         {
        ///             Hardware.Led1.Write(true);
        ///             _lcd.Write(1, 3, e.TagID.ToString());
        ///             _lcd.Write(1, 4, e.TagIDHex+"  #"+e.CRC);
        ///         }
        /// 
        /// </code>
        /// </example>
        public String TagIDHex { get; private set; }

        /// <summary>
        /// Gets the tag identifier as a UInt32
        /// </summary>
        /// <value>
        /// The tag identifier.
        /// </value>
        /// <example>
        /// <code language = "C#">
        /// 		public static void Main()
        ///         {
        ///                 _rfid = new RFIDClick(Hardware.SocketOne);
        ///                 InitLcd();
        /// 
        ///                 Debug.Print("RFID identification : " + _rfid.Identification());
        /// 
        ///                 _lcd.Write(1, 4, "Calibration...");
        ///                 _rfid.Calibration(Hardware.Led2);
        ///                 _lcd.Write(1, 4, "              ");
        /// 
        ///                 _rfid.TagDetected += _rfid_TagDetected;
        ///                 _rfid.TagRemoved += _rfid_TagRemoved;
        /// 
        ///                 // Starts detection
        ///                 _rfid.DetectionEnabled = true;
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        /// 
        ///         static void _rfid_TagRemoved(object sender, TagRemovedEventArgs e)
        ///         {
        ///             Hardware.Led1.Write(false);
        ///             _lcd.Write(1, 3, "                    ");
        ///             _lcd.Write(1, 4, "                    ");
        ///         }
        /// 
        ///         static void _rfid_TagDetected(object sender, TagDetectedEventArgs e)
        ///         {
        ///             Hardware.Led1.Write(true);
        ///             _lcd.Write(1, 3, e.TagID.ToString());
        ///             _lcd.Write(1, 4, e.TagIDHex+"  #"+e.CRC);
        ///         }
        /// 
        /// </code>
        /// </example>
        public UInt32 TagID { get; private set; }

        /// <summary>
        /// Gets the CRC value of the tag ID
        /// </summary>
        /// <value>
        /// The tag identifier.
        /// </value>
        /// <example>
        /// <code language = "C#">
        /// 		public static void Main()
        ///         {
        ///                 _rfid = new RFIDClick(Hardware.SocketOne);
        ///                 InitLcd();
        /// 
        ///                 Debug.Print("RFID identification : " + _rfid.Identification());
        /// 
        ///                 _lcd.Write(1, 4, "Calibration...");
        ///                 _rfid.Calibration(Hardware.Led2);
        ///                 _lcd.Write(1, 4, "              ");
        /// 
        ///                 _rfid.TagDetected += _rfid_TagDetected;
        ///                 _rfid.TagRemoved += _rfid_TagRemoved;
        /// 
        ///                 // Starts detection
        ///                 _rfid.DetectionEnabled = true;
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        /// 
        ///         static void _rfid_TagRemoved(object sender, TagRemovedEventArgs e)
        ///         {
        ///             Hardware.Led1.Write(false);
        ///             _lcd.Write(1, 3, "                    ");
        ///             _lcd.Write(1, 4, "                    ");
        ///         }
        /// 
        ///         static void _rfid_TagDetected(object sender, TagDetectedEventArgs e)
        ///         {
        ///             Hardware.Led1.Write(true);
        ///             _lcd.Write(1, 3, e.TagID.ToString());
        ///             _lcd.Write(1, 4, e.TagIDHex+"  #"+e.CRC);
        ///         }
        /// 
        /// </code>
        /// </example>
        public Byte CRC { get; private set; }
    }
}