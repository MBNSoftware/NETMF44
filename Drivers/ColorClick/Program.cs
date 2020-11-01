using Microsoft.SPOT;
using MBN;
using MBN.Modules;
using System;
using System.Threading;

namespace Examples
{
    public class Program
    {
        static ColorClick _color;
        static Double[] _tabColors;

        public static void Main()
        {
            _tabColors = new Double[4];

            _color = new ColorClick(Hardware.SocketOne);

            _color.LedR.Write(true);
            _color.LedG.Write(true);
            _color.LedB.Write(true);

            _color.Gain = ColorClick.Gains.x4;
            _color.IntegrationTime = 200;

            Debug.Print("Color ID : " + _color.GetID());
            Debug.Print("Integration time : " + _color.IntegrationTime);
            Debug.Print("Wait time : " + _color.WaitTime);
            Debug.Print("Wait long : " + _color.WaitLong);
            Debug.Print("Gain : " + _color.Gain);

            while (true)
            {
                var colorTmp = 0.0;

                for (var i = 0; i < 5; i++)
                {
                    _tabColors = _color.GetAllChannels();
// ReSharper disable PossibleLossOfFraction
                    colorTmp += _color.RGBtoHSL(_tabColors[0] / _tabColors[3], _tabColors[1] / _tabColors[3], _tabColors[2] / _tabColors[3]);
// ReSharper enable PossibleLossOfFraction
                }
                colorTmp /= 16;

                Debug.Print("Color : " + colorTmp);

                Thread.Sleep(1200);
            }
// ReSharper disable once FunctionNeverReturns
        }
    }
}
