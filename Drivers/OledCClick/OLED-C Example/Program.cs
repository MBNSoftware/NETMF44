﻿/*
 * OLED-C Click Demo Application generated on 11/21/2014 12:59:13 PM
 * 
 * Initial version coded by Stephen Cardinale
 * 
 */

using System;
using MBN.Enums;
using MBN.Modules;
using MBN;
using MBN.Exceptions;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace MBN_basic_application1
{
	public class Program
	{
		private static readonly MikroFont _font1 = FontManager.GetFont(FontManager.FontName.TahomaReg7);
		private static readonly MikroFont _font2 = new MikroFont(Resources.GetBytes(Resources.BinaryResources.tahoma_8));
		private static readonly MikroFont _font3 = new MikroFont(Resources.GetBytes(Resources.BinaryResources.tahoma_9));
		private static readonly MikroFont _font4 = new MikroFont(Resources.GetBytes(Resources.BinaryResources.tahoma_10));
		private static readonly MikroFont _font5 = new MikroFont(Resources.GetBytes(Resources.BinaryResources.tahoma_11));
		private static readonly MikroFont _font6 = FontManager.GetFont(FontManager.FontName.TahomaReg12);
		private static readonly MikroFont _font7 = new MikroFont(Resources.GetBytes(Resources.BinaryResources.tahoma_8e));

		
		private const string str1 = "MikroBus.Net";
		private const string str2 = "OLED-C Click";
		private const string str3 = "Demo App";
		private const string str4 = "Copyright © 2014";
		private const string str5 = "Stephen Cardinale";

		private static readonly MikroBitmap logo = new MikroBitmap(Resources.GetBytes(Resources.BinaryResources.mbn_bin));

		
		private static OLEDCClick _oled;

		public static void Main()
		{
			try
			{
				_oled = new OLEDCClick(Hardware.SocketFour) {FrameRate = OLEDCClick.FrameRates.OCS_140Hz};
			}
			catch (PinInUseException ex)
			{
				Debug.Print(ex.Message);
			}

			while (true)
			{
				_oled.Canvas.Clear();

				// Show some intro text
				Debug.Print("Show some intro text");
				_oled.Canvas.DrawText(str1, _font6, KnownColors.Red, 0, 0, 96, _font6.FontHeight, true);
				_oled.Canvas.DrawText(str2, _font6, KnownColors.Yellow, 0, _font6.FontHeight*2, 96, _font6.FontHeight, true);
				_oled.Canvas.DrawText(str3, _font6, KnownColors.Blue, 0, (_font6.FontHeight*4) - 4, 96, _font6.FontHeight, true);
				_oled.Flush();

				Thread.Sleep(3000);

				_oled.Canvas.Clear();
				_oled.Canvas.DrawText(str4, _font7, KnownColors.White, 0, 20, 96, 36, true);
				_oled.Canvas.DrawText(str5, _font2, KnownColors.Red, 0, 60, 96, 46, true);
				_oled.Flush();

				Thread.Sleep(3000);

				// Draw Logo1
				Debug.Print("Draw Logo1");
				_oled.Canvas.Clear(KnownColors.White);
				_oled.Canvas.DrawImage(logo, (_oled.CanvasWidth - logo.Width)/2, (_oled.CanvasHeight - logo.Height)/2);
				_oled.Flush();

				Thread.Sleep(3000);
	
				// Draw an image from and embedded RGB 888 Bitmap and then overlay an image from a byte array over top of it.
				Debug.Print("Draw an image from and embedded RGB 888 Bitmap and then overlay an image from a byte array over top of it.");

				using (var bmp = new Bitmap(Resources.GetBytes(Resources.BinaryResources.mbn_logo), Bitmap.BitmapImageType.Bmp))
				{
					_oled.Canvas.Clear(KnownColors.White);
					var img = bmp.ToMikroBitmap();
					_oled.Canvas.DrawImage(img, (_oled.CanvasWidth - bmp.Width) / 2, (_oled.CanvasHeight - bmp.Height) / 2);
					_oled.Flush();
					Thread.Sleep(1000);
					_oled.Canvas.DrawImage(new MikroBitmap(questionMark), 96 - 24, 96 - 24);
					_oled.Flush();
				}

				Thread.Sleep(3000);

				// ScreenSaver Test
				Debug.Print("ScreenSaver Test");
				for (byte x = 0; x < 8; x++)
				{
					_oled.StartScreenSaver((OLEDCClick.ScreenSaverMode) x, x == 7 ? (byte) 0x02 : (byte) 0x00);
					Thread.Sleep(5000);
					_oled.StopScreenSaver();
				}

				// Draw some rectangles
				// Draw Rectangle passing a Rect Structure
				Debug.Print("Draw Rectangle passing a Rect Structure");
				_oled.Canvas.DrawRectangle(KnownColors.Red, KnownColors.Red, new Rect(0, 0, 96, 16));
				_oled.Canvas.DrawRectangle(KnownColors.Yellow, KnownColors.Yellow, new Rect(0, 16, 96, 16));
				_oled.Canvas.DrawRectangle(KnownColors.Purple, KnownColors.Purple, new Rect(0, 32, 96, 16));
				_oled.Canvas.DrawRectangle(KnownColors.Blue, KnownColors.Blue, new Rect(0, 48, 96, 16));
				_oled.Canvas.DrawRectangle(KnownColors.DarkSeaGreen, KnownColors.DarkSeaGreen, new Rect(0, 64, 96, 16));
				_oled.Canvas.DrawRectangle(KnownColors.CadetBlue, KnownColors.CadetBlue, new Rect(0, 80, 96, 16));
				_oled.Flush();

				Thread.Sleep(2000);

				// Draw Rectangle passing X, Y, Width and Height
				Debug.Print("Draw Rectangle passing X, Y, Width and Height");
				_oled.Canvas.DrawRectangle(KnownColors.White, KnownColors.CadetBlue, 0, 0, 16, 96);
				_oled.Canvas.DrawRectangle(KnownColors.White, KnownColors.DarkSeaGreen, 16, 0, 16, 96);
				_oled.Canvas.DrawRectangle(KnownColors.White, KnownColors.Blue, 32, 0, 16, 96);
				_oled.Canvas.DrawRectangle(KnownColors.White, KnownColors.Purple, 48, 0, 16, 96);
				_oled.Canvas.DrawRectangle(KnownColors.White, KnownColors.Yellow, 64, 0, 16, 96);
				_oled.Canvas.DrawRectangle(KnownColors.White, KnownColors.PapayaWhip, 80, 0, 16, 96);
				_oled.Flush();

				Thread.Sleep(2000);

				// Fill the screen with a Random colors  a few times
				Debug.Print("Fill the screen with a Random colors  a few times");
				for (int x = 0; x < 50; x++)
				{
					_oled.Canvas.Clear(GenerateRandomColor());
					_oled.Flush();
				}

				// Fill Canvas with gradient
				Debug.Print("Fill Canvas with gradient");
				_oled.Canvas.DrawRectangle(KnownColors.White, 2, KnownColors.Blue, KnownColors.Red, 0, 0, _oled.CanvasWidth, _oled.CanvasHeight, true);
				_oled.Flush();

				Thread.Sleep(1500);

				// Draw some random Gradients, first 25 horizontally and the next 25 vertically.
				Debug.Print("Draw some random Gradients, first 25 horizontally and the next 25 vertically.");
				var vertical = true;
				for (int x = 0; x < 50; x++)
				{
					if (x > 25) vertical = false;
					_oled.Canvas.DrawRectangle(GenerateRandomColor(), 2, GenerateRandomColor(), GenerateRandomColor(), 0, 0, _oled.CanvasWidth, _oled.CanvasHeight, vertical);
					_oled.Flush();
				}

				// Draw some randomly placed hollow circles
				Debug.Print("Draw some randomly placed hollow circles");
				_oled.Canvas.Clear();
				for (int x = 0; x < 50; x++)
				{
					var point = GenerateRandomPoint();
					var color = GenerateRandomColor();
					var radius = GenerateRandomRadius();
					_oled.Canvas.DrawEllipse(color, point.X, point.Y, radius, radius);
					_oled.Flush();
				}

				//  Draw some randomly placed filled circles
				Debug.Print("Draw some randomly placed filled circles");
				_oled.Canvas.Clear();

				for (int x = 0; x < 50; x++)
				{
					var point = GenerateRandomPoint();
					var color = GenerateRandomColor();
					var radius = GenerateRandomRadius();
					_oled.Canvas.DrawEllipse(color, color, point.X, point.Y, radius, radius);
					_oled.Flush();
				}

				//  Draw some randomly placed gradient filled circles
				Debug.Print("Draw some randomly placed gradient filled circles");
				_oled.Canvas.Clear();

				for (int x = 0; x < 50; x++)
				{
					var point = GenerateRandomPoint();
					var color1 = GenerateRandomColor();
					var color2 = GenerateRandomColor();
					var color3 = GenerateRandomColor();
					var radius1 = GenerateRandomRadius();
					var radius2 = GenerateRandomRadius();
					_oled.Canvas.DrawEllipse(color1, color2, color3, point.X, point.Y, radius1, radius2);
					_oled.Flush();
				}

				//  Draw some randomly placed lines
				Debug.Print("Draw some randomly placed lines");
				_oled.Canvas.Clear(KnownColors.Black);

				for (int x = 0; x < 100; x++)
				{
					var point1 = GenerateRandomPoint();
					var point2 = GenerateRandomPoint();
					var color = GenerateRandomColor();
					_oled.Canvas.DrawLine(color, point1.X, point1.Y, point2.X, point2.Y);
					_oled.Flush();
				}

				//  Draw some randomly place hollow rectangles
				Debug.Print("Draw some randomly place hollow rectangles");
				_oled.Canvas.Clear(KnownColors.Black);

				for (int x = 0; x < 50; x++)
				{
					var point1 = GenerateRandomPoint();
					var point2 = GenerateRandomPoint();
					var color = GenerateRandomColor();
					_oled.Canvas.DrawRectangle(color, point1.X, point1.Y, point2.X, point2.Y);
					_oled.Flush();
				}

				//  Draw some randomly place filled rectangles
				Debug.Print("Draw some randomly place filled rectangles");
				_oled.Canvas.Clear(KnownColors.Black);

				for (int x = 0; x < 50; x++)
				{
					var point1 = GenerateRandomPoint();
					var point2 = GenerateRandomPoint();
					var color = GenerateRandomColor();
					_oled.Canvas.DrawRectangle(color, color, point1.X, point1.Y, point2.X, point2.Y);
					_oled.Flush();
				}

				//  Draw some randomly place gradient filled rectangles
				Debug.Print("Draw some randomly place gradient filled rectangles");
				_oled.Canvas.Clear(KnownColors.Black);
				vertical = true;

				for (int x = 0; x < 50; x++)
				{
					if (x > 25) vertical = false;
					var point1 = GenerateRandomPoint();
					var point2 = GenerateRandomPoint();
					var color = GenerateRandomColor();
					_oled.Canvas.DrawRectangle(color, color, point1.X, point1.Y, point2.X, point2.Y);
					_oled.Canvas.DrawRectangle(GenerateRandomColor(), 2, GenerateRandomColor(), GenerateRandomColor(), point1.X, point1.Y, point2.X, point2.Y, vertical);
					_oled.Flush();
				}

				// Draw some text
				Debug.Print("Draw some text");
				_oled.Canvas.Clear(KnownColors.Black);
				var color4 = GenerateRandomColor();
				var color5 = GenerateRandomColor();
				var color6 = GenerateRandomColor();
				var color7 = GenerateRandomColor();
				var color8 = GenerateRandomColor();
				var color9 = GenerateRandomColor();

				_oled.Canvas.DrawText("0123AaBb", _font1, color4, 0, 0, 96, _font1.FontHeight);
				_oled.Canvas.DrawText("0123AaBb", _font2, color5, 0, _font1.FontHeight + 1, 96, _font2.FontHeight);
				_oled.Canvas.DrawText("0123AaBb", _font3, color6, 0, _font1.FontHeight + _font2.FontHeight + 1, 96, _font3.FontHeight);
				_oled.Canvas.DrawText("0123AaBb", _font4, color7, 0, _font1.FontHeight + _font2.FontHeight + _font3.FontHeight + 1, 96, _font4.FontHeight);
				_oled.Canvas.DrawText("0123AaBb", _font5, color8, 0, _font1.FontHeight + _font2.FontHeight + _font3.FontHeight + _font4.FontHeight + 1, 96, _font5.FontHeight);
				_oled.Canvas.DrawText("0123AaBb", _font6, color9, 0, _font1.FontHeight + _font2.FontHeight + _font2.FontHeight + _font4.FontHeight + _font5.FontHeight + 1, 96, _font6.FontHeight);
				_oled.Flush(_oled.Canvas);

				Thread.Sleep(2000);

				// Draw some text in a rectangle
				Debug.Print("Draw some text in a rectangle");
				var r1 = new Rect(10, 20, 76, 28);
				var r2 = new Rect(10, r1.Height + r1.Y + 10, 76, 28);

				_oled.Canvas.Clear(KnownColors.White);
				_oled.Canvas.DrawRectangle(KnownColors.Red, KnownColors.White, r1);
				r1 = r1.Inflate(0, 6, 0, 0);
				_oled.Canvas.DrawText("MikroBus.Net", _font3, KnownColors.Blue, r1, true);
				_oled.Canvas.DrawRectangle(KnownColors.Red, KnownColors.White, r2);
				r2 = r2.Inflate(0, 6, 0, 0);
				_oled.Canvas.DrawText("Rocks", _font4, KnownColors.Blue, r2, true);
				_oled.Flush();
				Thread.Sleep(5000);

				Debug.GC(true); // just about at the memory limit here.
				
				// PowerMode
				Debug.Print("PowerMode Test");
				_oled.Canvas.Clear();
				_oled.Canvas.DrawText("PowerMode Test", _font3, KnownColors.White, 0, 0, 96, 48);
				_oled.Canvas.DrawText("Low Power for 5 secs", _font1, KnownColors.White, 0, 49, 96, 48);
				_oled.Flush();
				Thread.Sleep(5000);
				_oled.PowerMode = PowerModes.Low;
				Thread.Sleep(5000);
				_oled.PowerMode = PowerModes.On;
				_oled.Canvas.DrawText("PowerMode Test", _font3, KnownColors.White, 0, 0, 96, 48);
				_oled.Canvas.DrawText("Low Power for 5 secs", _font1, KnownColors.Black, 0, 49, 96, 48);
				_oled.Canvas.DrawText("Power back On", _font1, KnownColors.White, 0, 49, 96, 48);
				_oled.Flush();
				Thread.Sleep(2000);

				// Reset Mode
				Debug.Print("Reset Test");
				_oled.Canvas.Clear();
				_oled.Canvas.DrawText("Reset Test", _font3, KnownColors.White, 0, 0, 96, 48);
				_oled.Canvas.DrawText("Reset in 5 secs", _font1, KnownColors.White, 0, 49, 96, 48);
				_oled.Flush();
				Thread.Sleep(5000);
				_oled.Reset(ResetModes.Hard);
				_oled.Canvas.DrawText("Reset Test", _font3, KnownColors.White, 0, 0, 95, 48);
				_oled.Canvas.DrawText("Reset in 5 secs", _font1, KnownColors.Black, 0, 49, 96, 48);
				_oled.Canvas.DrawText("Resume from reset", _font1, KnownColors.White, 0, 49, 96, 48);
				_oled.Flush();
				Thread.Sleep(2000);

				//  Display On/Off Test
				Debug.Print("Display On/Off Test");
				_oled.Canvas.Clear();
				_oled.Canvas.DrawText("Display Off", _font3, KnownColors.White, 0, 0, 96, _font3.FontHeight);
				_oled.Flush();
				Thread.Sleep(2000);
				_oled.SetDisplayOn(false);
				Thread.Sleep(1000);
				_oled.SetDisplayOn(true);
				_oled.Canvas.DrawText("Display On", _font3, KnownColors.White, 0, _font3.FontHeight + 1, 96, _font3.FontHeight);
				_oled.Flush();
				Thread.Sleep(2000);
				
				Debug.Print("Memory - " + Debug.GC(true));

				// Now for a really large font?
				Debug.Print("Now for a really large font");
				var numbers = new MikroFont(Resources.GetBytes(Resources.BinaryResources.tahoma_30e));
				_oled.Canvas.Clear();

				for (int x = 10; x >= 0; x += -1)
				{
					_oled.Canvas.Clear();
					var sz = numbers.MeasureString(x.ToString());
					_oled.Canvas.DrawText(x.ToString(), numbers, KnownColors.LightSkyBlue, ((96 - sz.Width) / 2) - 1, ((96 - sz.Height) / 2) - 1, 96, 96);
					_oled.Canvas.DrawText(x.ToString(), numbers, KnownColors.Blue, (96 - sz.Width) /2, (96 - sz.Height) /2, 96, 96);
					_oled.Flush();
					Thread.Sleep(1000);
				}
				numbers = null;
				Debug.Print("Memory - " + Debug.GC(true));
			}
		}

		private static MikroColor GenerateRandomColor()
		{
			var randomGen = new Random();
			var b = new Byte[3];
			randomGen.NextBytes(b);
			var color = new MikroColor(b[0], b[1], b[2]);
			return color;
		}

		private static Point GenerateRandomPoint()
		{
			var randomGen = new Random();
			int x = randomGen.Next(95);
			int y = randomGen.Next(95);
			var point = new Point(x, y);
			return point;
		}

		private static int GenerateRandomRadius()
		{
			var randomGen = new Random();
			int radius = randomGen.Next(20);
			return radius;
		}

		private static readonly byte[] questionMark =
		{
			0x18, 0x18, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xBE, 0xFF, 0x9E, 0xFF, 0x5D, 0xFF, 0x5D, 0xFF, 0x3C, 0xFF, 0x5D, 0xFF, 0x9E, 
			0xFF, 0xDF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xDF, 
			0xFF, 0x9E, 0xFE, 0x9A, 0xFC, 0xD3, 0xEB, 0xEF, 0xD3, 0x2C, 0xD2, 0xEB, 0xE3, 0xAE, 0xF4, 0xD3, 
			0xFF, 0x3C, 0xFF, 0xBE, 0xFF, 0xDF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xDF, 0xFF, 0x9E, 
			0xDC, 0x0F, 0xB8, 0xC3, 0xD0, 0x82, 0xC8, 0xA2, 0xC8, 0xA2, 0xD0, 0xA2, 0xD0, 0x82, 0xD0, 0x82, 
			0xD1, 0xA6, 0xFD, 0xF7, 0xFF, 0x9E, 0xFF, 0xDF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xDF, 0xFF, 0x9E, 
			0xD3, 0x8E, 0xB8, 0xE3, 0xD9, 0x04, 0xEA, 0x28, 0xFB, 0x4C, 0xFA, 0xCB, 0xD9, 0x04, 0xD8, 0x61, 
			0xD0, 0x82, 0xB9, 0x86, 0xFE, 0xBA, 0xFF, 0xBF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xBE, 
			0xD4, 0xF3, 0xED, 0x34, 0xFF, 0x3C, 0xFF, 0x7D, 0xFF, 0x7D, 0xFF, 0x7D, 0xFF, 0x1C, 0xFB, 0xCF, 
			0xC8, 0xA2, 0xC0, 0xA3, 0xFB, 0xAE, 0xFF, 0x9E, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xDF, 
			0xFF, 0xDF, 0xFF, 0xDF, 0xFF, 0xDF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xBE, 0xFE, 0xFB, 
			0xC9, 0x24, 0xD0, 0x82, 0xEA, 0x69, 0xFF, 0x7D, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xBE, 0xFF, 0x1C, 
			0xD2, 0x08, 0xD0, 0x82, 0xD9, 0xC7, 0xFF, 0x7D, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x9E, 0xFE, 0xFB, 
			0xB9, 0x04, 0xC8, 0x82, 0xF2, 0xCB, 0xFF, 0x9D, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7D, 0xFC, 0x71, 
			0xC8, 0xA2, 0xC0, 0xC3, 0xFC, 0x51, 0xFF, 0x9E, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xBF, 0xFF, 0x9D, 0xFD, 0xB6, 0xC9, 0x04, 
			0xC8, 0xA2, 0xC1, 0xE7, 0xFF, 0x1C, 0xFF, 0xBE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xBE, 0xFF, 0x5D, 0xFC, 0x92, 0xC8, 0xE3, 0xC8, 0xA2, 
			0xC9, 0x86, 0xFE, 0x59, 0xFF, 0xBE, 0xFF, 0xDF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xDF, 0xFE, 0xBA, 0xD2, 0xEB, 0xC0, 0xC3, 0xC8, 0xA2, 0xDA, 0x49, 
			0xFE, 0x7A, 0xFF, 0xBE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x9E, 0xFC, 0x10, 0xB8, 0xE3, 0xC0, 0xE3, 0xFC, 0x30, 0xFF, 0x3C, 
			0xFF, 0xBE, 0xFF, 0xDF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x9E, 0xFB, 0xCF, 0xC0, 0xC3, 0xC9, 0xA6, 0xFF, 0x1C, 0xFF, 0xBE, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x9E, 0xFB, 0xEF, 0xB8, 0xE3, 0xB9, 0xE7, 0xFF, 0x3C, 0xFF, 0xBE, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xBE, 0xFD, 0x14, 0xCA, 0xCA, 0xC3, 0x8D, 0xFF, 0x7D, 0xFF, 0xDF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xBE, 0xFF, 0x9E, 0xFF, 0xBE, 0xFF, 0xDF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xDF, 0xFF, 0xBE, 0xFF, 0x9E, 0xFF, 0xBE, 0xFF, 0xDF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xBE, 0xF4, 0x71, 0xD2, 0xAA, 0xBA, 0xEB, 0xFF, 0x7D, 0xFF, 0xDF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x9E, 0xE3, 0x4D, 0xB0, 0xE4, 0x99, 0x45, 0xFF, 0x5D, 0xFF, 0xBF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xBE, 0xD3, 0xAE, 0x99, 0x45, 0x89, 0x86, 0xFF, 0x7D, 0xFF, 0xDF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xDF, 0xFF, 0x7D, 0xFF, 0x5D, 0xFF, 0x7D, 0xFF, 0xBE, 0xFF, 0xDF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xDF, 0xFF, 0xDF, 0xFF, 0xDF, 0xFF, 0xDF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
			0xFF, 0xFF
		};

	}
}
