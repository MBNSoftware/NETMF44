For our first code sample, nothing better than this legendary useful code !

No time to loose, here is the complete code :
```csharp
using System;
using System.Threading;
using MBN;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;
using MikroBusNet.Hardware;
using TestFT800.Properties;
using MFT = MikroBusNet.Hardware.MBNDisplay.FT800;

namespace FirmwareDemo
{
    public class Program
    {
        // If you are using EVE Click, then the next line has to be added, using the correct socket of course
        //private static OutputPort _an = new OutputPort(Hardware.SocketFour.An,false);
     
      // If you are using the ConnectEVE board, then you have to connect its PD# pin to the socket RST pin.

        public static void Main()
        {
            MBNDisplay.Populate(Hardware.SocketOne, MBNDisplay.MBNDisplayTypes.FT800Q);

            MFT.Cmd_DlStart();
            MFT.AddDL(MFT.Clear(1, 1, 1));

            MFT.Cmd_Text(240, 136, 28, MFT.Options.OPT_CENTER, "Hello world !");

            MFT.AddDL(MFT.Display());
            MFT.Cmd_Swap();

            Thread.Sleep(Timeout.Infinite);
        }
   }
}
```

To be able to run this code snippet, you will have to add a reference to our Display.dll assembly in your C# project (that can be created using our template). If you did not use our template to generate the code example, then you may also need to include the MBN Core assembly : MikroBusNet.dll
Both will be located where you unzipped the firmware archive.

A good and very useful help will be the FT800 Programmer's guide.


So, what does this code do and how can I do more or better ?

First, we tell that a FT800 display will be connected on the Socket #1 of the Quail. This sets up some internal things.


Next, we are simply using the standard FT800 commands or display list commands to display our text.


What is important here is that a frame has to always begin with the following code :

```csharp
MFT.Cmd_DlStart();
MFT.AddDL(MFT.Clear(1, 1, 1));
```

It tells the FT800 that we will send commands, the first one being to clear the screen.

Then we say we want the text "Hello world !" to be displayed centered (both on (X,Y)) at 240,136 coordinates. The font size if 28, as noticed in the programmer's guide.
The OPT_CENTER option will center the string on both axes.

And finally, again for every frame that has to be drawn, the following two commands has to be sent to the FT800 :

```csharp
MFT.AddDL(MFT.Display());
MFT.Cmd_Swap();
```

The first one indicates that no more commands will follow and the last one asks for the effective display.

What can we do to have a really cute hellow world ? We could add a blue background and the text could be yellow and cast a shadow ?
The code could then look like this :

```csharp
public static void Main()
        {
            MBNDisplay.Populate(Hardware.SocketOne, MBNDisplay.MBNDisplayTypes.FT800Q);

            MFT.Cmd_DlStart();
            MFT.AddDL(MFT.Clear_Color_RGB(0,0,255));
            MFT.AddDL(MFT.Clear(1, 1, 1));

            MFT.AddDL(MFT.Color_RGB(0, 0, 0));
            MFT.Cmd_Text(242, 138, 28, MFT.Options.OPT_CENTER, "Hello world !");
            MFT.AddDL(MFT.Color_RGB(255, 255, 0));
            MFT.Cmd_Text(240, 136, 28, MFT.Options.OPT_CENTER, "Hello world !");

            MFT.AddDL(MFT.Display());
            MFT.Cmd_Swap();

            Thread.Sleep(Timeout.Infinite);
        }
```

Before clearing the screen, we say that the "Clear" color will be Blue.
To simulate a shadow, we simply write the text in black (it will become the shadow) and then the same text again but with a little offset (2 pixels) and in Yellow (RGB 255 255 0)

Quite easy, isn't it ? :)


In the next code examples, we will demonstrate different commands of the FT800 along with how to integrate them with NETMF bitmaps when needed.

The FT800 programmer's guide should always be near you to read the syntax or the signification of the different parameters we will use. FTDI has made a good job, we will not reinvent the wheel here ;)