/*
 * OLED-C Click driver.
 * 
 * Initial version coded by Stephen Cardinale
 * 
 *  - ToDo - Add Methodology and Property/Method for Display Orientation.
 *  
 * References needed:
 *  <icrosoft.SPOT.Graphics
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Native
 *  MikroBusNet
 *  mscorlib
 *  
 * Copyright 2014 Stephen Cardinale
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System;

namespace MBN.Modules
{

	/// <summary>
	/// A Structure containing  an objects X and Y location of a specific point
	/// </summary>
	[Serializable]
	public struct Point
	{
		#region Variables

		/// <summary>
		/// The X location of the point
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Point pt = new Point(10, 20);
		///	pt.X = 20;
		///	Debug.Print("pt.X = " + pt.X);
		/// </code>
		/// <code language = "VB">
		/// Dim pt As Point = New Point(10, 20)
		///	pt.X = 20
		///	Debug.Print("pt.X = " <![CDATA[&]]> pt.X)
		/// </code>
		/// </example>
		public Int32 X;

		/// <summary>
		/// The Y location of the point
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Point pt = new Point(10, 20);
		///	pt.Y = 20;
		///	Debug.Print("pt.Y = " + pt.Y);
		/// </code>
		/// <code language = "VB">
		/// Dim pt As Point = New Point(10, 20)
		///	pt.X = 20
		///	Debug.Print("pt.Y = " <![CDATA[&]]> pt.Y)
		/// </code>
		/// </example>
		public Int32 Y;

		#endregion

		#region Constructor

		/// <summary>
		/// Creates a new point structure
		/// </summary>
		/// <param name="x">The X Location</param>
		/// <param name="y">The Y Location</param>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Point pt = new Point(10, 20);
		/// </code>
		/// <code language = "VB">
		/// Dim pt As Point = New Point(10, 20)
		/// </code>
		/// </example>
		public Point(Int32 x, Int32 y)
		{
			X = x;
			Y = y;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Returns the string representation of the Point structure
		/// </summary>
		/// <returns>{X, Y}</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Point pt = new Point(10, 20);
		///	Debug.Print("pt = " + pt.ToString());
		/// </code>
		/// <code language = "VB">
		/// Dim pt As Point = New Point(10, 20)
		/// Debug.Print("pt = " <![CDATA[&]]> pt.ToString())
		/// </code>
		/// </example>
		public override string ToString()
		{
			return "{" + X + ", " + Y + "}";
		}

		#endregion
	}

	/// <summary>
	/// Describes the width, height, and location of a rectangle. 
	/// </summary>
	[Serializable]
	public struct Rect
	{
		#region Variables

		private int _h;
		private int _w;
		private int _x;
		private int _y;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the Rect structure that has the specified x-coordinate, y-coordinate, width, and height. 
		/// </summary>
		/// <param name="x">The x-coordinate of the top-left corner of the rectangle.</param>
		/// <param name="y">The y-coordinate of the top-left corner of the rectangle.</param>
		/// <param name="width">The width of the rectangle.</param>
		/// <param name="height">The height of the rectangle.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Rect myRect = new Rect(0, 0, 96, 96);
		/// </code>
		/// <code language = "VB">
		///	Dim myRect as Rect = New Rect(0, 0, 96, 96)
		/// </code>
		/// </example>
		public Rect(Int32 x, Int32 y, Int32 width, Int32 height)
		{
			_x = x;
			_y = y;
			_w = width;
			_h = height;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the x-axis value of the left side of the rectangle. 
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Rect myRect = new Rect(0, 0, 96, 96);
		/// Debug.Print("myRect X is " + myRect.X); 
		/// </code>
		/// <code language = "VB">
		///	Dim myRect as Rect = New Rect(0, 0, 96, 96)
		/// Debug.Print("myRect Width is " <![CDATA[&]]> myRect.X) 
		/// </code>
		/// </example>
		public Int32 X
		{
			get { return _x; }
			set { _x = value; }
		}

		/// <summary>
		/// Gets or sets the y-axis value of the top side of the rectangle. 
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Rect myRect = new Rect(0, 0, 96, 96);
		/// Debug.Print("myRect Y is " + myRect.Y); 
		/// </code>
		/// <code language = "VB">
		///	Dim myRect as Rect = New Rect(0, 0, 96, 96)
		/// Debug.Print("myRect Width is " <![CDATA[&]]> myRect.Y) 
		/// </code>
		/// </example>
		public Int32 Y
		{
			get { return _y; }
			set { _y = value; }
		}

		/// <summary>
		/// Gets or sets the width of the rectangle. 
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Rect myRect = new Rect(0, 0, 96, 96);
		/// Debug.Print("myRect Width is " + myRect.Width); 
		/// </code>
		/// <code language = "VB">
		///	Dim myRect as Rect = New Rect(0, 0, 96, 96)
		/// Debug.Print("myRect Width is " <![CDATA[&]]> myRect.Width) 
		/// </code>
		/// </example>
		public Int32 Width
		{
			get { return _w; }
			set
			{
				if (value < 0) throw new ArgumentOutOfRangeException("value", @"Width cannot be less than 0.");
				_w = value;
			}
		}

		/// <summary>
		/// Gets or sets the height of the rectangle. 
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Rect myRect = new Rect(0, 0, 96, 96);
		/// Debug.Print("myRect Height is " + myRect.Height); 
		/// </code>
		/// <code language = "VB">
		///	Dim myRect as Rect = New Rect(0, 0, 96, 96)
		/// Debug.Print("myRect Height is " <![CDATA[&]]> myRect.Height) 
		/// </code>
		/// </example>
		public Int32 Height
		{
			get { return _h; }
			set
			{
				if (value < 0) throw new ArgumentOutOfRangeException("value", @"Height cannot be less than 0.");
				_h = value;
			}
		}

		/// <summary>
		/// Gets or sets the location (top left corner) of the Rect Structure.
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Rect myRect = new Rect(10, 10, 76, 48);
		/// Debug.Print("myRect Location - " + myRect.Location.ToString());
		/// </code>
		/// <code language = "VB">
		///	Dim myRect As New Rect(10, 10, 76, 48)
		///	Debug.Print("myRect Location - " <![CDATA[&]]> myRect.Location.ToString())
		/// </code>
		/// </example>
		public Point Location
		{
			get { return new Point(X, Y); }
			set
			{
				X = value.X;
				Y = value.Y;
			}
		}

		/// <summary>
		/// Get the Size of the Rect structure.
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Rect myRect = new Rect(10, 10, 76, 48);
		/// Debug.Print("myRect Size - " + myRect.Size.ToString());
		/// </code>
		/// <code language = "VB">
		///	Dim myRect As New Rect(10, 10, 76, 48)
		///	Debug.Print("myRect Size - " <![CDATA[&]]> myRect.Size.ToString())
		/// </code>
		/// </example>
		public Size Size
		{
			get { return new Size(Width, Height); }
			set
			{
				Width = value.Width;
				Height = value.Height;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Clones this rectangle.
		/// </summary>
		/// <returns>The cloned Rectangle object.</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Rect myRect = new Rect(0, 0, 96, 96);
		///	var myRectClone = myRect.Clone();
		/// </code>
		/// <code language = "VB">
		///	Dim myRect As Rect = New Rect(0, 0, 96, 96)
		///	Dim myRectClone As Rect = myRect.Clone()
		/// </code>
		/// </example>
		public Rect Clone()
		{
			return new Rect(X, Y, Width, Height);
		}

		/// <summary>
		/// Adds the area of a second Rectangle to an existing Rectangle
		/// </summary>
		/// <param name="newRect">The Rectangle to add</param>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Rect myRect1 = new Rect(0, 0, 96, 48);
		///	Rect myRect2 = new Rect(0, 49, 96, 48);
		///	myRect1.Combine(myRect2);
		/// </code>
		/// <code language = "VB">
		///	Dim myRect1 As Rect = New Rect(0, 0, 96, 96)
		///	Dim myRect2 As Rect = New Rect(0, 49, 96, 48)
		///	myRect1.Combine(myRect2)
		/// </code>
		/// </example>
		public void Combine(Rect newRect)
		{
			if (_w == 0)
			{
				_x = newRect.X;
				_y = newRect.Y;
				_w = newRect.Width;
				_h = newRect.Height;
				return;
			}

			int x1 = (_x < newRect.X) ? _x : newRect.X;
			int y1 = (_y < newRect.Y) ? _y : newRect.Y;
			int x2 = (_x + Width > newRect.X + newRect.Width) ? _x + _w : newRect.X + newRect.Width;
			int y2 = (_y + Height > newRect.Y + newRect.Height) ? _y + _h : newRect.Y + newRect.Height;
			_x = x1;
			_y = y1;
			_w = x2 - x1;
			_h = y2 - y1;
		}

		/// <summary>
		/// Returns the combination of two Rectangles
		/// </summary>
		/// <param name="region1">Rectangle 1 to combine.</param>
		/// <param name="region2">Rectangle 2 to combine.</param>
		/// <returns>The combined Rectangle</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		/// Rect myRect1 = new Rect(0, 0, 96, 48);
		/// Rect myRect2 = new Rect(0, 49, 96, 48);
		/// Rect myRectcombined = new Rect().Combine(myRect1, myRect2);
		/// </code>
		/// <code language = "VB">
		///	Dim myRect1 As Rect = New Rect(0, 0, 96, 96)
		///	Dim myRect2 As Rect = New Rect(0, 49, 96, 48)
		///	Dim myRectCombined As Rect = New Rect().Combine(myRect1, myRect2)
		/// </code>
		/// </example>
		public Rect Combine(Rect region1, Rect region2)
		{
			if (region1.Width == 0) return region2;
			if (region2.Width == 0) return region1;

			int x1 = (region1.X < region2.X) ? region1.X : region2.X;
			int y1 = (region1.Y < region2.Y) ? region1.Y : region2.Y;
			int x2 = (region1.X + region1.Width > region2.X + region2.Width)
				? region1.X + region1.Width
				: region2.X + region2.Width;
			int y2 = (region1.Y + region1.Height > region2.Y + region2.Height)
				? region1.Y + region1.Height
				: region2.Y + region2.Height;
			return new Rect(x1, y1, x2 - x1, y2 - y1);
		}

		/// <summary>
		/// Indicates whether the rectangle contains the specified x-coordinate and y-coordinate. 
		/// </summary>
		/// <param name="pointX">X location</param>
		/// <param name="pointY">Y location</param>
		/// <returns>True if the Point is inside the bounds of the Rectangle</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Rect myRect = new Rect(0, 0, 96, 48);
		///	bool doesContain = myRect.Contains(4, 13);
		///	Debug.Print("MyRect contains my Point? " + doesContain);
		/// </code>
		/// <code language = "VB">
		///	Dim myRect As New Rect(0, 0, 96, 48)
		///	Dim doesContain As Boolean = myRect.Contains(4, 13)
		///	Debug.Print("MyRect contains my Point? " <![CDATA[&]]> doesContain)
		/// </code>
		/// </example>
		public bool Contains(Int32 pointX, Int32 pointY)
		{
			return pointX >= _x && pointX <= _x + _w && pointY >= _y && pointY <= _y + _h;
		}

		/// <summary>
		/// Indicates whether the rectangle contains the specified point.
		/// </summary>
		/// <param name="e">Point to check</param>
		/// <returns>True if the Point is inside the bounds of the Rectangle</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Rect myRect = new Rect(0, 0, 96, 48);
		///	Point myPoint = new Point(10, 10);
		///	Debug.Print("MyRect contains my Point? " + myRect.Contains(myPoint));
		/// </code>
		/// <code language = "VB">
		/// Dim myRect As New Rect(0, 0, 96, 48)
		/// Dim myPoint As New Point(10, 10)
		/// Debug.Print("MyRect contains my Point? " <![CDATA[&]]> myRect.Contains(myPoint))
		/// </code>
		/// </example>
		public bool Contains(Point e)
		{
			return e.X >= _x && e.X <= _x + _w && e.Y >= _y && e.Y <= _y + _h;
		}

		/// <summary>
		/// Determines whether the object specified by the parameters intersects with this Rectangle object.
		/// </summary>
		/// <param name="area">Rectangle to check</param>
		/// <returns>True if the two Rectangles intersect</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Rect myRect1 = new Rect(0, 0, 96, 48);
		///	Rect myRect2 = new Rect(0, 47, 96, 48);
		///	bool intersects = myRect1.Intersects(myRect2);
		///	Debug.Print("MyRect1 Intersects myRect2? " + intersects);
		/// </code>
		/// <code language = "VB">
		/// Dim myRect1 As New Rect(0, 0, 96, 48)
		/// Dim myRect2 As New Rect(0, 47, 96, 48)
		/// Dim intersects As Boolean = myRect1.Intersects(myRect2)
		/// Debug.Print("MyRect1 Intersects myRect2? " <![CDATA[&]]> intersects)		/// </code>
		/// </example>
		public bool Intersects(Rect area)
		{
			return !(area.X >= (_x + _w)
			         || (area.X + area.Width) <= _x
			         || area.Y >= (_y + _h)
			         || (area.Y + area.Height) <= _y
				);
		}

		/// <summary>
		/// Inflates a Rect structure by the supplied dimensions.
		/// </summary>
		/// <param name="marginX">The X amount to add to the Rect structure.</param>
		/// <param name="marginY">The Y amount to add to the Rect structure.</param>
		/// <param name="marginWidth">The Width amount to add to the Rect structure.</param>
		/// <param name="marginHeight">The Height amount to add to the Rect structure.</param>
		/// <returns>A Rect structure with the new dimensions.</returns>
		/// <remarks>There is no error checking done with this method. Make sure that the passed parameters will result in a valid Rect structure. Also, this does not modify the original Rect structure.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Rect myRect = new Rect(10, 10, 76, 48);
		///	Rect myNewRect = myRect.Inflate(0, 10, 0, 0);
		///	bool isEqual = myRect.Equals(myNewRect);
		///	Debug.Print("Is Equal? " + isEqual);
		/// </code>
		/// <code language = "VB">
		///	Dim myRect As New Rect(10, 10, 76, 48)
		///	Dim myNewRect = myRect.Inflate(0, 10, 0, 0)
		///	Dim isEqual = myRect.Equals(myNewRect)
		///	Debug.Print("Is Equal? " <![CDATA[&]]> isEqual)
		/// </code>
		/// </example>
		public Rect Inflate(Int32 marginX, Int32 marginY, Int32 marginWidth, Int32 marginHeight)
		{ 
			return new Rect(X + marginX, Y + marginY, Width + marginWidth, Height += marginHeight);
		}

		/// <summary>
		/// Deflates a Rect structure by the supplied dimensions.
		/// </summary>
		/// <param name="marginX">The X amount to subtract to the Rect structure.</param>
		/// <param name="marginY">The Y amount to subtract to the Rect structure.</param>
		/// <param name="marginWidth">The Width amount to subtract to the Rect structure.</param>
		/// <param name="marginHeight">The Height amount to subtract to the Rect structure.</param>
		/// <returns>A Rect structure with the new dimensions.</returns>
		/// <remarks>There is no error checking done with this method. Make sure that the passed parameters will result in a valid Rect structure.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Rect myRect = new Rect(10, 10, 76, 48);
		///	Rect myNewRect = myRect.Deflate(10, 10, 0, 0);
		///	bool isEqual = myRect.Equals(myNewRect);
		///	Debug.Print("Is Equal? " + isEqual);
		/// </code>
		/// <code language = "VB">
		///	Dim myRect As New Rect(10, 10, 76, 48)
		///	Dim myNewRect = myRect.Inflate(100, 10, 0, 0)
		///	Dim isEqual = myRect.Equals(myNewRect)
		///	Debug.Print("Is Equal? " <![CDATA[&]]> isEqual)
		/// </code>
		/// </example>
		public Rect Deflate(Int32 marginX, Int32 marginY, Int32 marginWidth, Int32 marginHeight)
		{
			return new Rect(X - marginX, Y - marginY, Width - marginWidth, Height - marginHeight);
		}

		/// <summary>
		/// Returns the intersection of two Rectangles
		/// </summary>
		/// <param name="region1">Rectangle 1</param>
		/// <param name="region2">Rectangle 2</param>
		/// <returns>The a new Rectangle of the intersection of two rectangles.</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		/// Rect myRect1 = new Rect(0, 0, 96, 48);
		/// Rect myRect2 = new Rect(0, 47, 96, 48);
		/// Rect intersection = myRect1.Intersection(myRect1, myRect2);
		/// </code>
		/// <code language = "VB">
		/// Dim myRect1 As New Rect(0, 0, 96, 48)
		/// Dim myRect2 As New Rect(0, 47, 96, 48)
		/// Dim intersection As Rect = myRect1.Intersection(myRect1, myRect2)
		/// </code>
		/// </example>
		public Rect Intersection(Rect region1, Rect region2)
		{
			if (!region1.Intersects(region2)) return new Rect(0, 0, 0, 0);

			var rct = new Rect
			{
				X = (region1.X > region2.X) ? region1.X : region2.X,
				Y = (region1.Y > region2.Y) ? region1.Y : region2.Y
			};

			int r1V2 = region1.X + region1.Width;
			int r2V2 = region2.X + region2.Width;
			rct.Width = (r1V2 < r2V2) ? r1V2 - rct.X : r2V2 - rct.X;
			r1V2 = region1.Y + region1.Height;
			r2V2 = region2.Y + region2.Height;
			rct.Height = (r1V2 < r2V2) ? r1V2 - rct.Y : r2V2 - rct.Y;

			return rct;
		}

		/// <summary>
		/// Compares two Rect structures to determine whether the left expression is not equal to the right expression.
		/// </summary>
		/// <param name="rect1">The first Rect structure to compare for equality</param>
		/// <returns>Returns true if the values of its operands are equal, false otherwise.</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Rect myRect = new Rect(10, 10, 76, 48);
		///	Rect myNewRect = myRect.Inflate(0, 10, 0, 0);
		///	bool isEqual = myRect.Equals(myNewRect);
		///	Debug.Print("Is Equal? " + isEqual);
		/// </code>
		/// <code language = "VB">
		///	Dim myRect As New Rect(10, 10, 76, 48)
		///	Dim myNewRect = myRect.Inflate(0, 10, 0, 0)
		///	Dim isEqual = myRect.Equals(myNewRect)
		///	Debug.Print("Is Equal? " <![CDATA[&]]> isEqual)
		/// </code>
		/// </example>
		public bool Equals(Rect rect1)
		{
			return (rect1.X == _x && rect1.Y == _y && rect1.Width == _w && rect1.Height == _h);
		}

		/// <summary>
		/// Returns the string representation of the Rectangle
		/// </summary>
		/// <returns>{X, Y, width, height}</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Rect myRect = new Rect(10, 10, 76, 48);
		///	Debug.Print(myRect.ToString());
		/// </code>
		/// <code language = "VB">
		/// Dim myRect As New Rect(10, 10, 76, 48)
		/// Debug.Print(myRect.ToString())
		/// </code>
		/// </example>
		public override string ToString()
		{
			return "{" + X + ", " + Y + ", " + Width + ", " + Height + "}";
		}

		#endregion
	}

	/// <summary>
	/// Stores an ordered pair of integers, which specify a Height and Width.
	/// </summary>
	[Serializable]
	public struct Size
	{
		#region Variables

		/// <summary>
		/// Gets the vertical component of this Size structure.
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Size sz = new Size(10, 10);
		///	Debug.Print("Height? " + sz.Height);
		/// </code>
		/// <code language = "VB">
		///	Dim sz As Size = New Size(10, 10)
		///	Debug.Print("Height? " <![CDATA[&]]> sz.Height)
		/// </code>
		/// </example>
		public int Height;

		/// <summary>
		/// Gets or sets the horizontal component of this Size structure.
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Size sz = new Size(10, 10);
		///	Debug.Print("Width? " + sz.Width);
		/// </code>
		/// <code language = "VB">
		///	Dim sz As Size = New Size(10, 10)
		///	Debug.Print("Width? " <![CDATA[&]]> sz.Width)
		/// </code>
		/// </example>
		public int Width;

		#endregion

		#region Constructor

		/// <summary>
		///  Initializes a new instance of the Size structure from the specified dimensions.
		/// </summary>
		/// <param name="width">The width of the new Size structure.</param>
		/// <param name="height">The height of the new size structure.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Size sz = new Size(10, 10);
		/// </code>
		/// <code language = "VB">
		///	Dim sz As Size = New Size(10, 10)
		/// </code>
		/// </example>
		public Size(int width, int height)
		{
			Width = width;
			Height = height;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Adds height and width to existing size structure
		/// </summary>
		/// <param name="addWidth">The height in which to grow the Size structure.</param>
		/// <param name="addHeight">The width in which to grow the Size structure.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Size sz = new Size(10, 10);
		///	sz.Grow(10, 10);
		///	Debug.Print("New Size? " + sz.ToString());
		/// </code>
		/// <code language = "VB">
		///	Dim sz As New Size(10, 10)
		///	sz.Grow(10, 10)
		/// Debug.Print("New Size? " <![CDATA[&]]> sz.ToString())
		/// </code>
		/// </example>
		public void Grow(Int32 addWidth, Int32 addHeight)
		{
			Width += addWidth;
			Height += addHeight;
		}

		/// <summary>
		/// Subtracts height and width from existing size structure
		/// </summary>
		/// <param name="subtractWidth">The height in which to shrink the Size structure.</param>
		/// <param name="subtractHeight">The width in which to shrink the Size structure.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Size sz = new Size(20, 20);
		///	sz.Shrink(10, 10);
		///	Debug.Print("New Size? " + sz.ToString());
		/// </code>
		/// <code language = "VB">
		///	Dim sz As New Size(20, 20)
		///	sz.Shrink(10, 10)
		/// Debug.Print("New Size? " <![CDATA[&]]> sz.ToString())
		/// </code>
		/// </example>
		public void Shrink(int subtractWidth, int subtractHeight)
		{
			Width += subtractWidth;
			if (Width < 0) Width = 0;
			Height += subtractHeight;
			if (Height < 0) Height = 0;
		}

		/// <summary>
		/// Returns the string representation of the Size structure.
		/// </summary>
		/// <returns>{width, height}</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Size sz = new Size(10, 10);
		///	sz.Grow(10, 10);
		///	Debug.Print("New Size? " + sz.ToString());
		/// </code>
		/// <code language = "VB">
		///	Dim sz As New Size(10, 10)
		///	sz.Grow(10, 10)
		/// Debug.Print("New Size? " <![CDATA[&]]> sz.ToString())
		/// </code>
		/// </example>
		public override string ToString()
		{
			return "{" + Width + ", " + Height + "}";
		}

		#endregion
	}
}