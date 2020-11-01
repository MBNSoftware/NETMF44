For this example, we will play with Strings, scissors, cells, gradients and FT800 primitives. This is mainly a show-case of different FT800 functions but it may help beginners to better understand some concepts of the FT800.

The workflow of the commands is almost identical to the one seen in the Hello World ! example : you populate the display on a socket, you start the frame sequence, you send the different commands and you end the frame to display it.


So, let's start !

We will display a very informative string on the screen, with the most beautiful colors available : white on black...

```csharp
// Populate the FT800 on the Quail's socket #1
MBNDisplay.Populate(Hardware.SocketOne, MBNDisplay.MBNDisplayTypes.FT800Q);

// Set up some constants
const int DisplayWidth = 480;
const int DisplayHeight = 272;

// As usual, we start the frame with those two commands           
MFT.Cmd_DlStart();
MFT.AddDL(MFT.Clear(1, 1, 1));

// Now the most complicated part
MFT.Cmd_Text(DisplayWidth/2, 50, 28, MFT.Options.OPT_CENTER, "String example");

// Classic ending for the frame
MFT.AddDL(MFT.Display());
MFT.Cmd_Swap();
```

The source is documented so I will not comment much. Default color is white and default clear color is black, this is why I did not put any color related commands.
The nice text is then displayed centered on both axes at (240,50), with a font size of 28. Please refer to the FT800 Programmer's guide for details about font sizes and formatting options.

So far, this is mainly the Hello World example :) Let's then add a green gradient background :

```csharp
// Populate the FT800 on the Quail's socket #1
MBNDisplay.Populate(Hardware.SocketOne, MBNDisplay.MBNDisplayTypes.FT800Q);

// Set up some constants and variables
const int DisplayWidth = 480;
const int DisplayHeight = 272;

// As usual, we start the frame with those two commands           
MFT.Cmd_DlStart();
MFT.AddDL(MFT.Clear(1, 1, 1));

// The background gradient
MFT.AddDL(MFT.Scissor_XY(0, 0));
MFT.AddDL(MFT.Scissor_Size(DisplayWidth, DisplayHeight));
MFT.Cmd_Gradient(0, 0, 0x000000, 0, DisplayHeight, 0x006600);
MFT.AddDL(MFT.Scissor_XY(0, 0));
MFT.AddDL(MFT.Scissor_Size(512, 512));

// Our text
MFT.Cmd_Text(DisplayWidth/2, 50, 28, MFT.Options.OPT_CENTER, "String example");

// Classic ending for the frame
MFT.AddDL(MFT.Display());
MFT.Cmd_Swap();
```

Here we are using the Scissor which restricts the area on which everything will be drawn. By default, the scissor is using a 512x512 area, so we will set it to the display's dimensions.
Scissor_XY() sets the starting point, while Scissor_Size() will set... its size :)
We then send the Cmd_Gradient() command, which will draw a vertical gradient from black to dark green. Once this is done, we reset the Scissor to its default values so that subsequent drawings on the display can use all of the surface.


The Cmd_Text() command is useful to write strings or chars. But, for single chars only, we can also use the Vertex2II() method with "special" parameters. Again, this is only a normal use of this command, no trick here.
We will now display the following fabulous three letters M, B and N using this technique.

```csharp
// Populate the FT800 on the Quail's socket #1
MBNDisplay.Populate(Hardware.SocketOne, MBNDisplay.MBNDisplayTypes.FT800Q);

// Set up some constants and variables
const int DisplayWidth = 480;
const int DisplayHeight = 272;

// As usual, we start the frame with those two commands           
MFT.Cmd_DlStart();
MFT.AddDL(MFT.Clear(1, 1, 1));

// The background gradient
MFT.AddDL(MFT.Scissor_XY(0, 0));
MFT.AddDL(MFT.Scissor_Size(DisplayWidth, DisplayHeight));
MFT.Cmd_Gradient(0, 0, 0x000000, 0, DisplayHeight, 0x006600);
MFT.AddDL(MFT.Scissor_XY(0, 0));
MFT.AddDL(MFT.Scissor_Size(512, 512));

// Our text
MFT.Cmd_Text(DisplayWidth/2, 50, 28, MFT.Options.OPT_CENTER, "String example");

// Add the MBN letters near the center
MFT.AddDL(MFT.Begin(MFT.Primitives.BITMAPS));
MFT.AddDL(MFT.Vertex2II(140, 136, 31, 'M'));
MFT.AddDL(MFT.Vertex2II(200, 136, 31, 66));
MFT.AddDL(MFT.Vertex2II(260, 136, 31, 'N'));
MFT.AddDL(MFT.End());
			
// Classic ending for the frame
MFT.AddDL(MFT.Display());
MFT.Cmd_Swap();
```

To achieve this, we need to start a BITMAP primitive. This will tell to the FT800 that the next commands will deal with bitmaps.
The MFT.AddDL(MFT.Vertex2II(140, 136, 31, 'M')); line is simply asking to draw at (140,136) the bitmap that is present in cell 'M' (ascii code 77) of the bitmap handle 31.
What is nice here is that handles from 16 to 31 are assigned to internal fonts (smallest size to biggest), so the cell number is simply the ascii code of the char !
As you can see on the following line, I could have used an integer representing the actual Ascii value ('B' is 66). But it is less readable than a char.
The third line is doing the same with the 'N', of course.
For your information, handles from 0 to 15 are available for any use. Only 16 to 31 are reserved to internal fonts.

Since we have started a FT800 primitive (the BITMAP), we have to terminate its drawing. This is done by sending the End() command.

This almost all about this example, except that the text written so far does not say much. So I will add something to make it better. This will be another text but this time it will be yellow on a blue rectangle background.
We will then see another primitive and some settings for colors.

```csharp
// Populate the FT800 on the Quail's socket #1
MBNDisplay.Populate(Hardware.SocketOne, MBNDisplay.MBNDisplayTypes.FT800Q);

// Set up some constants and variables
const int DisplayWidth = 480;
const int DisplayHeight = 272;

// As usual, we start the frame with those two commands           
MFT.Cmd_DlStart();
MFT.AddDL(MFT.Clear(1, 1, 1));

// The background gradient
MFT.AddDL(MFT.Scissor_XY(0, 0));
MFT.AddDL(MFT.Scissor_Size(DisplayWidth, DisplayHeight));
MFT.Cmd_Gradient(0, 0, 0x000000, 0, DisplayHeight, 0x006600);
MFT.AddDL(MFT.Scissor_XY(0, 0));
MFT.AddDL(MFT.Scissor_Size(512, 512));

// Our text
MFT.Cmd_Text(DisplayWidth/2, 50, 28, MFT.Options.OPT_CENTER, "String example");

// Add the MBN letters near the center
MFT.AddDL(MFT.Begin(MFT.Primitives.BITMAPS));
MFT.AddDL(MFT.Vertex2II(140, 136, 31, 'M'));
MFT.AddDL(MFT.Vertex2II(200, 136, 31, 66));
MFT.AddDL(MFT.Vertex2II(260, 136, 31, 'N'));
MFT.AddDL(MFT.End());

// Finally the text that is explaining everything
MFT.AddDL(MFT.Color_RGB(0x00, 0x00, 0xAA));
MFT.AddDL(MFT.Begin(MFT.Primitives.RECTS));
MFT.AddDL(MFT.Vertex2F(100*16, 200*16));
MFT.AddDL(MFT.Vertex2F(230*16, 250*16));
MFT.AddDL(MFT.End());
MFT.AddDL(MFT.Color_RGB(0xAA, 0xAA, 0x00));
MFT.Cmd_Text(100, 200, 31, 0, "rocks !");
			
// Classic ending for the frame
MFT.AddDL(MFT.Display());
MFT.Cmd_Swap();
```

We set the drawing color to blue (RGB 0,0,0xAA) and start a new primitive. Since we want a rectangle, what is best than a Primitive.RECTS ? We then send the coordinates of the upper-left corner and then the coordinates of the lower-right corner.
Here, we are using the Vertex2F() command. It is different than the Vertex2II() command in that it is working in 1/16 pixel instead of real screen coordinates and can accept negative values (UInt16 format). This can be useful to display animated things that may overlap screen boundaries, for example. Here, they are used in their simple meaning, only for the purpose of the demonstration.
Finally, we End() the RECTS primitive.

Now, to write the interesting text, we set the color to be yellow (RGB 255,255,0) and we use the Cmd_Text() command. Isn't it powerful ? :)


Finally, we will see another example of the Scissor and the Gradient. This time, we will write the same text on a cute diagonal gradient background instead of a solid blue one.

```csharp
// Populate the FT800 on the Quail's socket #1
MBNDisplay.Populate(Hardware.SocketOne, MBNDisplay.MBNDisplayTypes.FT800Q);

// Set up some constants and variables
const int DisplayWidth = 480;
const int DisplayHeight = 272;

// As usual, we start the frame with those two commands           
MFT.Cmd_DlStart();
MFT.AddDL(MFT.Clear(1, 1, 1));

// The background gradient
MFT.AddDL(MFT.Scissor_XY(0, 0));
MFT.AddDL(MFT.Scissor_Size(DisplayWidth, DisplayHeight));
MFT.Cmd_Gradient(0, 0, 0x000000, 0, DisplayHeight, 0x006600);
MFT.AddDL(MFT.Scissor_XY(0, 0));
MFT.AddDL(MFT.Scissor_Size(512, 512));

// Our text
MFT.Cmd_Text(DisplayWidth/2, 50, 28, MFT.Options.OPT_CENTER, "String example");

// Add the MBN letters near the center
MFT.AddDL(MFT.Begin(MFT.Primitives.BITMAPS));
MFT.AddDL(MFT.Vertex2II(140, 136, 31, 'M'));
MFT.AddDL(MFT.Vertex2II(200, 136, 31, 66));
MFT.AddDL(MFT.Vertex2II(260, 136, 31, 'N'));
MFT.AddDL(MFT.End());

// Finally the text that is explaining everything
MFT.AddDL(MFT.Color_RGB(0x00, 0x00, 0xAA));
MFT.AddDL(MFT.Begin(MFT.Primitives.RECTS));
MFT.AddDL(MFT.Vertex2F(100*16, 200*16));
MFT.AddDL(MFT.Vertex2F(230*16, 250*16));
MFT.AddDL(MFT.End());
MFT.AddDL(MFT.Color_RGB(0xAA, 0xAA, 0x00));
MFT.Cmd_Text(100, 200, 31, 0, "rocks !");

// We repeat the same text because it is really a nice text
// but we add a cute background as well
MFT.AddDL(MFT.Scissor_XY(300, 200));
MFT.AddDL(MFT.Scissor_Size(130, 50));
MFT.Cmd_Gradient(300, 200, 0xFF0000, 430, 250, 0x0000FF);
MFT.AddDL(MFT.Scissor_XY(0, 0));
MFT.AddDL(MFT.Scissor_Size(512, 512));
MFT.AddDL(MFT.Color_RGB(0xAA, 0xAA, 0x00));
MFT.Cmd_Text(300, 200, 31, 0, "rocks !");
			
// Classic ending for the frame
MFT.AddDL(MFT.Display());
MFT.Cmd_Swap();
```

We set the Scissor to start at (300,200) with a size of 130x50. We then draw a diagonal Gradient (with real screen coordinates, remember) from red to blue and set the Scissor back to its default values.
The text is then drawn on top of that rectangle. Easy as it sounds.


Well, this is the end of this tutorial. We have seen many different and useful things in this one :
* Scissor
* Gradient
* Graphic primitives (BITMAP and RECTS)
* how to display strings or single chars with two different techniques


The next example will show you how to embed NETMF bitmaps in the FT800 code and how to set it up so that you can enjoy the FT800 goodies about bitmaps with your NETMF graphics :)
To be continued...