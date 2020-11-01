using System;

namespace MBN.Modules
{
    public partial class JoystickClick
    {
        internal static class Registers
        {
            // ReSharper disable InconsistentNaming
            public const Byte CONTROL1 = 0x0F;
            public const Byte CONTROL2 = 0x2E;
            public const Byte AGC = 0x2A;
            public const Byte T_CTRL = 0x2D;
            public const Byte X = 0x10;
            public const Byte Y = 0x11;
            public const Byte Xp = 0x12;
            public const Byte Xn = 0x13;
            public const Byte Yp = 0x14;
            public const Byte Yn = 0x15;
            // ReSharper restore InconsistentNaming
        }
    }
}