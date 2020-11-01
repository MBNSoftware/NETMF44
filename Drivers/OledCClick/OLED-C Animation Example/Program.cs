/*
 * OLED-C Click Animation Demo Application generated on 11/21/2014 12:59:13 PM
 * 
 * Initial version coded by Stephen Cardinale
 * 
 */

using MBN;
using MBN.Modules;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace OLED_C_Animation_Example
{
	public class Program
	{
		private static OLEDCClick _oled;

		public static void Main()
		{
			Debug.GC(true);

			_oled = new OLEDCClick(Hardware.SocketFour) {FrameRate = OLEDCClick.FrameRates.OCS_140_4Hz};

			var img1 = new MikroBitmap(Resources.GetBytes(Resources.BinaryResources.bfly1));
			var img2 = new MikroBitmap(Resources.GetBytes(Resources.BinaryResources.bfly2));
			var img3 = new MikroBitmap(Resources.GetBytes(Resources.BinaryResources.bfly3));
			var img4 = new MikroBitmap(Resources.GetBytes(Resources.BinaryResources.bfly4));
			var img5 = new MikroBitmap(Resources.GetBytes(Resources.BinaryResources.bfly5));
			var img6 = new MikroBitmap(Resources.GetBytes(Resources.BinaryResources.bfly6));

			while (true)
			{
				_oled.Canvas.DrawImage(img1, (_oled.CanvasWidth - img1.Width) / 2, (_oled.CanvasHeight - img1.Height) / 2);
				_oled.Flush();
				//Debug.GC(true);
				Thread.Sleep(100);
				_oled.Canvas.DrawImage(img2, (_oled.CanvasWidth - img2.Width) / 2, (_oled.CanvasHeight - img2.Height) / 2);
				_oled.Flush();
				//Debug.GC(true);
				Thread.Sleep(100);
				_oled.Canvas.DrawImage(img3, (_oled.CanvasWidth - img3.Width) / 2, (_oled.CanvasHeight - img3.Height) / 2);
				_oled.Flush();
				//Debug.GC(true);
				Thread.Sleep(100);
				_oled.Canvas.DrawImage(img4, (_oled.CanvasWidth - img4.Width) / 2, (_oled.CanvasHeight - img4.Height) / 2);
				_oled.Flush();
				//Debug.GC(true);
				Thread.Sleep(100);
				_oled.Canvas.DrawImage(img5, (_oled.CanvasWidth - img5.Width) / 2, (_oled.CanvasHeight - img5.Height) / 2);
				_oled.Flush();
				//Debug.GC(true);
				Thread.Sleep(100);
				_oled.Canvas.DrawImage(img6, (_oled.CanvasWidth - img6.Width) / 2, (_oled.CanvasHeight - img6.Height) / 2);
				_oled.Flush();
				Thread.Sleep(100);
				Debug.GC(true);
			}
		}
	}
}
