This tutorial will show how to use the various Touch events or methods of the FT800. We will build a very useful menu and add some "non-menu" touch areas as well.

The FT800 is handling touch events with differently depending on the objects drawn/used. You can either :
- explicitely tell it to handle an object as well as the returned value when that object is touched (buttons, graphic primitives)
- rely on the default behaviour of an object ("Keys")
- define a region of the screen in which you have put an object (slider, scrollbar)
- retrieve raw screen coordinates

Of course, a mix of all those will often be used.

I will not teach the basics of the touch handling as the FT800 Programmer's guide is doing this quite well. Rather, I will show code that use it. And since this is the fourth tuto, I will not comment the mandatory lines of code either. You should then refer to the previous tutos.

So let's begin with a simple "top-menu" with four options. Those will be simple buttons, which are standard FT800 objects.

```csharp
			MFT.Cmd_Calibrate();
            MFT.Cmd_Track(0, 0, 0, 0, 254);

            var tag = 0;
            var _button1Pressed = false;
            var _button2Pressed = false;
            var _button3Pressed = false;
            var _button4Pressed = false;
            var tagValue = 0;
            var drawFrame = true;

            while (true)
            {
                if (drawFrame)
                {
                    MFT.Cmd_DlStart();
                    MFT.AddDL(MFT.Clear(1, 1, 1));

                    MFT.AddDL(MFT.Save_Context());

                    MFT.AddDL(MFT.Tag(1));
                    MFT.Cmd_Button(10, 10, 50, 30, 28, _button1Pressed ? 256 : 0, "File");
                    MFT.AddDL(MFT.Tag(2));
                    MFT.Cmd_Button(70, 10, 50, 30, 28, _button2Pressed ? 256 : 0, "Edit");
                    MFT.AddDL(MFT.Tag(3));
                    MFT.Cmd_Button(130, 10, 80, 30, 28, _button3Pressed ? 256 : 0, "Display");
                    MFT.AddDL(MFT.Tag(4));
                    MFT.Cmd_Button(250, 10, 50, 30, 28, _button4Pressed ? 256 : 0, "Exit");

                    switch (tag)
                    {
                        case 1:
                            MFT.Cmd_Text(10, 200, 30, 0, "File");
                            break;
                        case 2:
                            MFT.Cmd_Text(10, 200, 30, 0, "Edit");
                            break;
                        case 3:
                            MFT.Cmd_Text(10, 200, 30, 0, "Display");
                            break;
                        case 4:
                            MFT.Cmd_Text(10, 200, 30, 0, "Exit");
                            break;
                    }
                    MFT.AddDL(MFT.Restore_Context());
                    EndFrame();
                }
               
                tagValue = MFT.Tag_Touched();
                tag = tagValue & 0xFF;
                drawFrame = tag != 0;

                switch (tag)
                {
                    case 0:
                        _button1Pressed = false;
                        _button2Pressed = false;
                        _button3Pressed = false;
                        _button4Pressed = false;
                        break;
                    case 1:
                        _button1Pressed = true;
                        break;
                    case 2:
                        _button2Pressed = true;
                        break;
                    case 3:
                        _button3Pressed = true;
                        break;
                    case 4:
                        _button4Pressed = true;
                        break;
                }

                Thread.Sleep(10);
            }
```

In order to have the touch events behave correctly, we have to calibrate the touch system. This is done by using the MFT.Cmd_Calibrate(); method. It simply calls the FT800 internal calibration routine and you will have to touch three dots on the screen.
At this point, the calibration routine will be executed each time you run the program. But we will see later how to save and restore the result of the calibration routine to avoid calling it upon each startup.

The line MFT.Cmd_Track(0, 0, 0, 0, 254); is initializing the tracking system on the FT800. I have assigned a high tag number so that I am sure it will not be used. Without this line, no tracking will be performed.

Then, before each object, I assign a tag value. This is the value that will be returned by the tracker method if the object is touched. So, in the example, each button ias assigned a tag, from 1 to 4.

We then poll the MFT.Tag_Touched(); method that will return one of 5 values in our case : 0 for no touch event and 1 to 4 for touches on our buttons. So if you touch the "Display" button, which has been assigned tag #3, then you will receive the value 3.

This is as simple as that :)

In this small example, I do not redraw the frame if not needed. That is : if no touch event is detected, then the frame is not redrawn.


Now, let's add some special objects : Keys. The main difference with buttons or other objects is that the FT800 will automatically assign a tag to each of the possible values of keys. If you are using "ABCDE" as keys, then the MFT.Tag_Touched() will automatically return values from 65 to 69 (ascii code of letters) without needing to assign anything before.
You then only have to add some more "case" statements in your switch to handle the touch events on those keys. Let's try that :

```csharp
			MFT.Cmd_Calibrate();
            MFT.Cmd_Track(0, 0, 0, 0, 254);

            var keyPressedValue = 32;
            var tag = 0;
            var _button1Pressed = false;
            var _button2Pressed = false;
            var _button3Pressed = false;
            var _button4Pressed = false;
            var tagValue = 0;
            var drawFrame = true;

            while (true)
            {
                if (drawFrame)
                {
                    MFT.Cmd_DlStart();
                    MFT.AddDL(MFT.Clear(1, 1, 1));

                    MFT.AddDL(MFT.Save_Context());

                    MFT.AddDL(MFT.Tag(1));
                    MFT.Cmd_Button(10, 10, 50, 30, 28, _button1Pressed ? 256 : 0, "File");
                    MFT.AddDL(MFT.Tag(2));
                    MFT.Cmd_Button(70, 10, 50, 30, 28, _button2Pressed ? 256 : 0, "Edit");
                    MFT.AddDL(MFT.Tag(3));
                    MFT.Cmd_Button(130, 10, 80, 30, 28, _button3Pressed ? 256 : 0, "Display");
                    MFT.AddDL(MFT.Tag(4));
                    MFT.Cmd_Button(250, 10, 50, 30, 28, _button4Pressed ? 256 : 0, "Exit");

                    MFT.Cmd_Keys(200, 136, 180, 50, 29, keyPressedValue, "ABCDEF");
                    MFT.Cmd_Keys(300, 190, 180, 20, 29, keyPressedValue, "123");
                    MFT.Cmd_Keys(300, 212, 180, 20, 29, keyPressedValue, "456");
                    MFT.Cmd_Keys(300, 234, 180, 20, 29, keyPressedValue, "789");
                    MFT.Cmd_Keys(300, 256, 180, 20, 29, keyPressedValue, "0");

                    switch (tag)
                    {
                        case 1:
                            MFT.Cmd_Text(10, 200, 30, 0, "File");
                            break;
                        case 2:
                            MFT.Cmd_Text(10, 200, 30, 0, "Edit");
                            break;
                        case 3:
                            MFT.Cmd_Text(10, 200, 30, 0, "Display");
                            break;
                        case 4:
                            MFT.Cmd_Text(10, 200, 30, 0, "Exit");
                            break;
                        default:
                            if (tag >= 48)
                            {
                                MFT.AddDL(MFT.Vertex2II(10, 200, 30, keyPressedValue));
                            }
                            break;
                    }
                    MFT.AddDL(MFT.Restore_Context());
                    EndFrame();
                }
               
                tagValue = MFT.Tag_Touched();
                tag = tagValue & 0xFF;
                drawFrame = tag != 0;

                switch (tag)
                {
                    case 0:
                        _button1Pressed = false;
                        _button2Pressed = false;
                        _button3Pressed = false;
                        _button4Pressed = false;
                        break;
                    case 1:
                        _button1Pressed = true;
                        break;
                    case 2:
                        _button2Pressed = true;
                        break;
                    case 3:
                        _button3Pressed = true;
                        break;
                    case 4:
                        _button4Pressed = true;
                        break;
                    default:
                        if (tag >= 48)
                        {
                            keyPressedValue = tag;
                        }
                        break;
                }

                Thread.Sleep(10);
            }
```

The value for the keys are now checked in the default statement of the switch() and displayed accordingly.

You can even add basic drawing primitives such as rectangles or points and have them return a tag value if they are touched. To do this, you simply have to assign them a tag value just before drawing them.
For example, we will add a square and a circle. And we will add a Dial control as well ;-)

```csharp
			MFT.Cmd_Calibrate();

            var keyPressedValue = 32;
            var tag = 0;
            var _button1Pressed = false;
            var _button2Pressed = false;
            var _button3Pressed = false;
            var _button4Pressed = false;
            var tagValue = 0;
            var drawFrame = true;
            var angle = 0x8000;

            while (true)
            {
                if (drawFrame)
                {
                    MFT.Cmd_DlStart();
                    MFT.AddDL(MFT.Clear(1, 1, 1));

                    MFT.AddDL(MFT.Save_Context());

                    MFT.AddDL(MFT.Tag(1));
                    MFT.Cmd_Button(10, 10, 50, 30, 28, _button1Pressed ? 256 : 0, "File");
                    MFT.AddDL(MFT.Tag(2));
                    MFT.Cmd_Button(70, 10, 50, 30, 28, _button2Pressed ? 256 : 0, "Edit");
                    MFT.AddDL(MFT.Tag(3));
                    MFT.Cmd_Button(130, 10, 80, 30, 28, _button3Pressed ? 256 : 0, "Display");
                    MFT.AddDL(MFT.Tag(4));
                    MFT.Cmd_Button(250, 10, 50, 30, 28, _button4Pressed ? 256 : 0, "Exit");

                    MFT.Cmd_Keys(200, 136, 180, 50, 29, keyPressedValue, "ABCDEF");
                    MFT.Cmd_Keys(300, 190, 180, 20, 29, keyPressedValue, "123");
                    MFT.Cmd_Keys(300, 212, 180, 20, 29, keyPressedValue, "456");
                    MFT.Cmd_Keys(300, 234, 180, 20, 29, keyPressedValue, "789");
                    MFT.Cmd_Keys(300, 256, 180, 20, 29, keyPressedValue, "0");

                    MFT.AddDL(MFT.Tag(5));
                    MFT.AddDL(MFT.Begin(MFT.Primitives.RECTS));
                    MFT.AddDL(MFT.Vertex2F(400 * 16, 10 * 16));
                    MFT.AddDL(MFT.Vertex2F(450 * 16, 50 * 16));
                    MFT.AddDL(MFT.End());

                    MFT.AddDL(MFT.Point_Size(40 * 16));
                    MFT.AddDL(MFT.Tag(6));
                    MFT.AddDL(MFT.Begin(MFT.Primitives.POINTS));
                    MFT.AddDL(MFT.Vertex2F(240 * 16, 80 * 16));
                    MFT.AddDL(MFT.End());

                    MFT.Cmd_Track(80, 140, 1, 1, 7);
                    MFT.AddDL(MFT.Tag(7));
                    MFT.Cmd_Dial(80, 140, 55, 0, angle);

                    switch (tag)
                    {
                        case 1:
                            MFT.Cmd_Text(10, 200, 30, 0, "File");
                            break;
                        case 2:
                            MFT.Cmd_Text(10, 200, 30, 0, "Edit");
                            break;
                        case 3:
                            MFT.Cmd_Text(10, 200, 30, 0, "Display");
                            break;
                        case 4:
                            MFT.Cmd_Text(10, 200, 30, 0, "Exit");
                            break;
                        case 5:
                            MFT.Cmd_Text(10, 200, 30, 0, "Square");
                            break;
                        case 6:
                            MFT.Cmd_Text(10, 200, 30, 0, "Circle");
                            break;
                        case 7:
                            MFT.Cmd_Text(10, 200, 30, 0, angle.ToString());
                            break;
                        default:
                            if (tag >= 48)
                            {
                                MFT.AddDL(MFT.Vertex2II(10, 200, 30, keyPressedValue));
                            }
                            break;
                    }
                    MFT.AddDL(MFT.Restore_Context());
                    EndFrame();
                }
               
                tagValue = MFT.Tag_Touched();
                tag = tagValue & 0xFF;
                drawFrame = tag != 0;

                switch (tag)
                {
                    case 0:
                        _button1Pressed = false;
                        _button2Pressed = false;
                        _button3Pressed = false;
                        _button4Pressed = false;
                        break;
                    case 1:
                        _button1Pressed = true;
                        break;
                    case 2:
                        _button2Pressed = true;
                        break;
                    case 3:
                        _button3Pressed = true;
                        break;
                    case 4:
                        _button4Pressed = true;
                        break;
                    case 7:
                        angle = (tagValue >> 16) & 0xFFFF;
                        break;
                    default:
                        if (tag >= 48)
                        {
                            keyPressedValue = tag;
                        }
                        break;
                }

                Thread.Sleep(10);
            }
```

As you can see, we do not have to check if the touch has been done inside the circle or square. The simple fact that they have a tag is enough.

To finish this tutorial, let's see some other useful methods, beginning with the MFT.TouchScreenXY() method. With this one, you will get an Int32 that will contain the X coordinate in the upper 16bits and the Y one in the lower 16bits. Hence the following math to get both informations :
var x = (val >> 16) & 0xFFFF; var y = val & 0xFFFF;

Next interesting methods are about calibration. In our example, we have to pass the calibration phase each time the board is powered up. If you have a mean to store the calibration data (in the Quail onboard Flash, for example), then you can run the calibration routine only once and store/retrieve the values calculated.
Once the touch system is calibrated, you can use the MFT.GetCalibrationData() method that will return an array of integers. This array can then passed later to the MFT.SetCalibrationData() method, which will in turn calibrate the touch system.
Here is a little code snippet :

```csharp
var CalibrationData = MFT.GetCalibrationData();
MFT.SetCalibrationData(CalibrationData);
MFT.SetCalibrationData(new[] { 33457, 379, -1594858, -352, 20127, -1180213 });
```

This is all for today.

Next, and last tuto, will be about loading and displaying pictures...