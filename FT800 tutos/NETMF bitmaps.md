As promised in the previous example, here is now some hints and how-to in order to use standard NETMF bitmaps with the FT800 firmware.

There is a big difference between how it will be used with the FT800 when compared to the other displays supported by the MBN firmware : here, the NETMF bitmap will become a FT800 bitmap, allowing the power of the FT800 to be applied to your NETMF bitmap.
To be honest, a single Bitmap.Flush() will not be enough. But you will not need too much more code either.

But let's start and see how it is going :

```csharp
// Create the NETMF bitmap and draw a rectangle on it
var _bmp1 = new Bitmap(128, 128);
_bmp1.DrawRectangle(ColorUtility.ColorFromRGB(255, 0, 0), 4, 0, 0, _bmp1.Width, _bmp1.Height, 0, 0,
                ColorUtility.ColorFromRGB(255, 255, 0), 0, 0, ColorUtility.ColorFromRGB(0, 0, 255), _bmp1.Width, _bmp1.Height,
                Bitmap.OpacityOpaque);

// Start the FT800 display list
MFT.Cmd_DlStart();
MFT.AddDL(MFT.Clear(1, 1, 1));

// Assign handle #0 to our bitmap and flush it
MFT.AddDL(MFT.Bitmap_Handle(0));
_bmp1.Flush();
// Set the starting address in FT800 memory to 0
MFT.AddDL(MFT.Bitmap_Source(0));
// Send information about the layout and size of our bitmap
MFT.AddDL(MFT.Bitmap_Layout(MFT.BitmapFormat.RGB565, _bmp1.Width * 2, _bmp1.Height));
MFT.AddDL(MFT.Bitmap_Size(MFT.FilterMode.NEAREST, MFT.WrapMode.BORDER, MFT.WrapMode.BORDER, _bmp1.Width, _bmp1.Height));

// Start the BITMAPS primitive and display our NETMF bitmap
MFT.AddDL(MFT.Begin(MFT.Primitives.BITMAPS));
MFT.AddDL(MFT.Vertex2II(10, 10, 0, 0));
MFT.AddDL(MFT.End());

// Refresh the display to show our nice square
MFT.AddDL(MFT.Display());
MFT.Cmd_Swap();
MFT.FlushCoBuffer();
MFT.FlushDLBuffer();
MFT.WaitCmdFifo_Empty();
```

We start by creating a 128x128 bitmap, in the standard NETMF way. Then we draw a nice square with a gradient inside it. So far, no difference in usual way of playing with NETMF bitmaps.
We then start the FT800 display list and clear the screen.

Now comes the interesting part : with the Bitmap_Handle() command, we will assign a FT800 internal handle to our NETMF bitmap. If you remember, handles from 0 to 15 are available for users' needs, so I will take the first one : 0.
Then, we flush the entire bitmap. At this point, the bitmap data has been transferred into the FT800 memory, starting at address 0. And this is this address that we will now pass to the Bitmap_Source() method.

The FT800 still needs some information about the bitmap format, so we tell it : format RGB565 (internal NETMF format for bitmaps), stride and height. Same for its size with the Bitmap_Size() command. Again, please refer to the FT800 Programmer's Guide for details on those commands.

At this moment, the FT800 has all the needed informations to use our bitmap, so let's display it by starting a BITMAP primitive and by setting its position at (10,10) with the Vertex2II command.
The third parameter of Vertex2II() is a handle. Guess what ? Yes, we will give the handle 0 so that it knows what to display :)

As usual, we end the frame with standard commands. You show now see the nice square in the upper left corner of the display. Again, what you are seeing is a NETMF bitmap and not a FT800 primitive.

Well, I have to admit that this is not very spectacular... So, let's go further ;)


Since our bitmap has been assigned a handle, it also means that Vertex2II can be called again with the same handle and other coordinates... This will then put a copy of our bitmap at no cost ! No need to transfer it again from the Quail to the FT800.
Try to add the following line just between the Vertex2II() and End() lines :
```csharp
MFT.AddDL(MFT.Vertex2II(210, 10, 0, 0));
```

Bingo ! You have just duplicated and displayed a 128x128x2 = 32KB in less than 1/60 sec :) And that is not all... Since our bitmap is (almost) a FT800 object, it can be used as such and be applied some transformations like scaling, rotations or translations, for example.

Let's have fun and demonstrate it :

```csharp
// Create the NETMF bitmap and draw a rectangle on it
var _bmp1 = new Bitmap(128, 128);
_bmp1.DrawRectangle(ColorUtility.ColorFromRGB(255, 0, 0), 4, 0, 0, _bmp1.Width, _bmp1.Height, 0, 0,
                ColorUtility.ColorFromRGB(255, 255, 0), 0, 0, ColorUtility.ColorFromRGB(0, 0, 255), _bmp1.Width, _bmp1.Height,
                Bitmap.OpacityOpaque);

// Start the FT800 display list
MFT.Cmd_DlStart();
MFT.AddDL(MFT.Clear(1, 1, 1));

// Assign handle #0 to our bitmap and flush it
MFT.AddDL(MFT.Bitmap_Handle(0));
_bmp1.Flush();
// Set the starting address in FT800 memory to 0
MFT.AddDL(MFT.Bitmap_Source(0));
// Send information about the layout and size of our bitmap
MFT.AddDL(MFT.Bitmap_Layout(MFT.BitmapFormat.RGB565, _bmp1.Width * 2, _bmp1.Height));
MFT.AddDL(MFT.Bitmap_Size(MFT.FilterMode.NEAREST, MFT.WrapMode.BORDER, MFT.WrapMode.BORDER, _bmp1.Width, _bmp1.Height));

// Start the BITMAPS primitive and display our NETMF bitmap
MFT.AddDL(MFT.Begin(MFT.Primitives.BITMAPS));
MFT.AddDL(MFT.Vertex2II(10, 10, 0, 0));
MFT.AddDL(MFT.Vertex2II(210, 10, 0, 0));	// Displays a second identical square

// Distort the bitmap
MFT.Cmd_LoadIdentity();
MFT.Cmd_Scale((int)(0.5 * 65536), (int)(0.3 * 65536));
MFT.Cmd_Translate(100 * 65536, 20 * 65536);
MFT.Cmd_Rotate(Degrees(45));
MFT.Cmd_SetMatrix();
// Again, using handle #0, we display the same NETMF bitmap without resending its data
MFT.AddDL(MFT.Vertex2II(350, 160, 0, 0));
MFT.AddDL(MFT.End());

// Refresh the display to show our nice square
MFT.AddDL(MFT.Display());
MFT.Cmd_Swap();
MFT.FlushCoBuffer();
MFT.FlushDLBuffer();
MFT.WaitCmdFifo_Empty();
```

As you can see, we have sent the bitmap's data only once (the Bitmap.Flush() command) but we have displayed it three times after that.


Of course, you are not limited to display only one NETMF bitmap ! You can send another completely different one as well and display both of them.
This is exactly what we will do now :

```csharp
// Create the NETMF bitmaps and draw a rectangle on one of them and a text on the other one
var _bmp1 = new Bitmap(128, 128);
var _bmp2 = new Bitmap(200,40);
var _font = Resources.GetFont(Resources.FontResources.NinaB);

_bmp1.DrawRectangle(ColorUtility.ColorFromRGB(255, 0, 0), 4, 0, 0, _bmp1.Width, _bmp1.Height, 0, 0,
                ColorUtility.ColorFromRGB(255, 255, 0), 0, 0, ColorUtility.ColorFromRGB(0, 0, 255), _bmp1.Width, _bmp1.Height,
                Bitmap.OpacityOpaque);
_bmp2.DrawText("A string in a NETMF bitmap", _font, Color.White, 0, 0);

// Start the FT800 display list
MFT.Cmd_DlStart();
MFT.AddDL(MFT.Clear(1, 1, 1));

// Assign handle #0 to our bitmap and flush it
MFT.AddDL(MFT.Bitmap_Handle(0));
_bmp1.Flush();
// Set the starting address in FT800 memory to 0
MFT.AddDL(MFT.Bitmap_Source(0));
// Send information about the layout and size of our bitmap
MFT.AddDL(MFT.Bitmap_Layout(MFT.BitmapFormat.RGB565, _bmp1.Width * 2, _bmp1.Height));
MFT.AddDL(MFT.Bitmap_Size(MFT.FilterMode.NEAREST, MFT.WrapMode.BORDER, MFT.WrapMode.BORDER, _bmp1.Width, _bmp1.Height));

// Assign handle #1 to our "text" bitmap and flush it
MFT.AddDL(MFT.Bitmap_Handle(1));
_bmp2.Flush();
// Sets the starting address in the FT800 memory at the end of the previous bitmap
MFT.AddDL(MFT.Bitmap_Source(_bmp1.Width * _bmp1.Height * 2));
MFT.AddDL(MFT.Bitmap_Layout(MFT.BitmapFormat.RGB565, _bmp2.Width * 2, _bmp2.Height));
MFT.AddDL(MFT.Bitmap_Size(MFT.FilterMode.NEAREST, MFT.WrapMode.BORDER, MFT.WrapMode.BORDER, _bmp2.Width, _bmp2.Height));

// Start the BITMAPS primitive and display our NETMF bitmap
MFT.AddDL(MFT.Begin(MFT.Primitives.BITMAPS));
MFT.AddDL(MFT.Vertex2II(10, 10, 0, 0));
MFT.AddDL(MFT.Vertex2II(210, 10, 0, 0));	// Displays a second identical square

// Distort the bitmap
MFT.Cmd_LoadIdentity();
MFT.Cmd_Scale((int)(0.5 * 65536), (int)(0.3 * 65536));
MFT.Cmd_Translate(100 * 65536, 20 * 65536);
MFT.Cmd_Rotate(Degrees(45));
MFT.Cmd_SetMatrix();
// Again, using handle #0, we display the same NETMF bitmap without resending its data
MFT.AddDL(MFT.Vertex2II(350, 160, 0, 0));

// Reset transformations to their default values
MFT.Cmd_LoadIdentity();
MFT.Cmd_SetMatrix();
// Put the same NETMF text bitmap at two different places, using handle #1
MFT.AddDL(MFT.Vertex2II(100, 150, 1, 0));
MFT.AddDL(MFT.Vertex2II(10, 230, 1, 0));

MFT.AddDL(MFT.End());

// Refresh the display to show our nice square
MFT.AddDL(MFT.Display());
MFT.Cmd_Swap();
MFT.FlushCoBuffer();
MFT.FlushDLBuffer();
MFT.WaitCmdFifo_Empty();
```

There are two important points here. First, we are using a different handle for the second image. This seems obvious but one can forget it easily :)
Second very important point, though, is the parameter of the Bitmap_Source() method for the second bitmap : it is the address in the FT800 memory where our bitmap is stored.

But how can we know it, you may ask ? Thanks for asking, then... Each time you ask a Bitmap.Flush(), the internal pointer is moved to the end of the bitmap just sent. NETMF bitmaps are stacked into the FT800 memory.
So bitmap #1 is starting at 0, bitmap #2 is starting at 0 + bitmap1.Width*bitmap1.Height*2, bitmap #3 is starting at bitmap2 address + (bitmap2.Width*bitmap2.Height*2) and so on...

The internal pointer is reset during the Cmd_DlStart() call, when you start a new frame.

And finally, let's show again that since our NETMF text bitmap has been assigned a (different) handle, we can duplicate it, change some of its properties like scale or color :

```csharp
// Create the NETMF bitmaps and draw a rectangle on one of them and a text on the other one
var _bmp1 = new Bitmap(128, 128);
var _bmp2 = new Bitmap(200,40);
var _font = Resources.GetFont(Resources.FontResources.NinaB);

_bmp1.DrawRectangle(ColorUtility.ColorFromRGB(255, 0, 0), 4, 0, 0, _bmp1.Width, _bmp1.Height, 0, 0,
                ColorUtility.ColorFromRGB(255, 255, 0), 0, 0, ColorUtility.ColorFromRGB(0, 0, 255), _bmp1.Width, _bmp1.Height,
                Bitmap.OpacityOpaque);
_bmp2.DrawText("A string in a NETMF bitmap", _font, Color.White, 0, 0);

// Start the FT800 display list
MFT.Cmd_DlStart();
MFT.AddDL(MFT.Clear(1, 1, 1));

// Assign handle #0 to our bitmap and flush it
MFT.AddDL(MFT.Bitmap_Handle(0));
_bmp1.Flush();
// Set the starting address in FT800 memory to 0
MFT.AddDL(MFT.Bitmap_Source(0));
// Send information about the layout and size of our bitmap
MFT.AddDL(MFT.Bitmap_Layout(MFT.BitmapFormat.RGB565, _bmp1.Width * 2, _bmp1.Height));
MFT.AddDL(MFT.Bitmap_Size(MFT.FilterMode.NEAREST, MFT.WrapMode.BORDER, MFT.WrapMode.BORDER, _bmp1.Width, _bmp1.Height));

// Assign handle #1 to our "text" bitmap and flush it
MFT.AddDL(MFT.Bitmap_Handle(1));
_bmp2.Flush();
// Sets the starting address in the FT800 memory at the end of the previous bitmap
MFT.AddDL(MFT.Bitmap_Source(_bmp1.Width * _bmp1.Height * 2));
MFT.AddDL(MFT.Bitmap_Layout(MFT.BitmapFormat.RGB565, _bmp2.Width * 2, _bmp2.Height));
MFT.AddDL(MFT.Bitmap_Size(MFT.FilterMode.NEAREST, MFT.WrapMode.BORDER, MFT.WrapMode.BORDER, _bmp2.Width, _bmp2.Height));

// Start the BITMAPS primitive and display our NETMF bitmap
MFT.AddDL(MFT.Begin(MFT.Primitives.BITMAPS));
MFT.AddDL(MFT.Vertex2II(10, 10, 0, 0));
MFT.AddDL(MFT.Vertex2II(210, 10, 0, 0));	// Displays a second identical square

// Distort the bitmap
MFT.Cmd_LoadIdentity();
MFT.Cmd_Scale((int)(0.5 * 65536), (int)(0.3 * 65536));
MFT.Cmd_Translate(100 * 65536, 20 * 65536);
MFT.Cmd_Rotate(Degrees(45));
MFT.Cmd_SetMatrix();
// Again, using handle #0, we display the same NETMF bitmap without resending its data
MFT.AddDL(MFT.Vertex2II(350, 160, 0, 0));

// Let's play with the NETMF text bitmap :)
MFT.Cmd_LoadIdentity();
MFT.Cmd_Scale((int) (2 * 65535UL), (int)(1 * 65535UL));
MFT.Cmd_SetMatrix();
MFT.AddDL(MFT.Vertex2II(100, 150, 1, 0));
MFT.Cmd_LoadIdentity();
MFT.Cmd_SetMatrix();
MFT.AddDL(MFT.Color_RGB(0,255,0));
MFT.AddDL(MFT.Vertex2II(10, 230, 1, 0));

MFT.AddDL(MFT.End());

// Refresh the display to show our nice square
MFT.AddDL(MFT.Display());
MFT.Cmd_Swap();
MFT.FlushCoBuffer();
MFT.FlushDLBuffer();
MFT.WaitCmdFifo_Empty();
```

That's all for this one. We have demonstrated a more advanced use of our native driver with this example. Using NETMF bitmap as a FT800 bitmap primitive is a very powerful feature.


Next example will show how to use Touch events.