using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Peripherals.Displays;

namespace ProjectLab_Demo;

public class DisplayController
{
    private int rowOffset = 29;
    private int rowHeight = 21;
    private int rowMargin = 2;

    private Color foregroundColor = Color.White;
    private Color atmosphericColor = Color.White;
    private Color motionColor = Color.FromHex("23ABE3");
    private Color buttonColor = Color.FromHex("EF7D3B");

    private Font12x20 font12X20 = new Font12x20();

    private DisplayScreen displayScreen;

    public Label Temperature { get; set; }

    public Label Humidity { get; set; }

    public Label Pressure { get; set; }

    public Label Luminance { get; set; }

    public Label Acceleration3D { get; set; }

    public Label AngularVelocity3D { get; set; }

    public Label ButtonUp { get; set; }

    public Label ButtonDown { get; set; }

    public Label ButtonLeft { get; set; }

    public Label ButtonRight { get; set; }

    public DisplayController(IPixelDisplay display, string revisionVersion)
    {
        displayScreen = new DisplayScreen(display, RotationType._270Degrees)
        {
            BackgroundColor = Color.FromHex("0B3749")
        };

        displayScreen.Controls.Add(new Label(rowMargin, 4, displayScreen.Width, rowHeight)
        {
            Text = $"Hello PROJ LAB {revisionVersion}!",
            TextColor = foregroundColor,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        displayScreen.Controls.Add(new Label(rowMargin, rowOffset, displayScreen.Width, rowHeight)
        {
            Text = $"Temperature:",
            TextColor = atmosphericColor,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Left
        });
        displayScreen.Controls.Add(new Label(rowMargin, rowOffset + rowHeight, displayScreen.Width, rowHeight)
        {
            Text = $"Pressure:",
            TextColor = atmosphericColor,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Left
        });
        displayScreen.Controls.Add(new Label(rowMargin, rowOffset + rowHeight * 2, displayScreen.Width, rowHeight)
        {
            Text = $"Humidity",
            TextColor = atmosphericColor,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Left
        });
        displayScreen.Controls.Add(new Label(rowMargin, rowOffset + rowHeight * 3, displayScreen.Width, rowHeight)
        {
            Text = $"Lux:",
            TextColor = atmosphericColor,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Left
        });
        displayScreen.Controls.Add(new Label(rowMargin, rowOffset + rowHeight * 4, displayScreen.Width, rowHeight)
        {
            Text = $"Accel:",
            TextColor = motionColor,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Left
        });
        displayScreen.Controls.Add(new Label(rowMargin, rowOffset + rowHeight * 5, displayScreen.Width, rowHeight)
        {
            Text = $"Gyro:",
            TextColor = motionColor,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Left
        });
        displayScreen.Controls.Add(new Label(rowMargin, rowOffset + rowHeight * 6, displayScreen.Width, rowHeight)
        {
            Text = $"Up:",
            TextColor = buttonColor,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Left
        });
        displayScreen.Controls.Add(new Label(rowMargin, rowOffset + rowHeight * 7, displayScreen.Width, rowHeight)
        {
            Text = $"Down:",
            TextColor = buttonColor,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Left
        });
        displayScreen.Controls.Add(new Label(rowMargin, rowOffset + rowHeight * 8, displayScreen.Width, rowHeight)
        {
            Text = $"Left:",
            TextColor = buttonColor,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Left
        });
        displayScreen.Controls.Add(new Label(rowMargin, rowOffset + rowHeight * 9, displayScreen.Width, rowHeight)
        {
            Text = $"Right:",
            TextColor = buttonColor,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Left
        });


        Temperature = new Label(rowMargin, rowOffset, displayScreen.Width - rowMargin * 2, rowHeight)
        {
            Text = $"0°C",
            TextColor = atmosphericColor,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        displayScreen.Controls.Add(Temperature);

        Pressure = new Label(rowMargin, rowOffset + rowHeight, displayScreen.Width - rowMargin * 2, rowHeight)
        {
            Text = $"0atm",
            TextColor = atmosphericColor,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        displayScreen.Controls.Add(Pressure);

        Humidity = new Label(rowMargin, rowOffset + rowHeight * 2, displayScreen.Width - rowMargin * 2, rowHeight)
        {
            Text = $"0%",
            TextColor = atmosphericColor,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        displayScreen.Controls.Add(Humidity);

        Luminance = new Label(rowMargin, rowOffset + rowHeight * 3, displayScreen.Width - rowMargin * 2, rowHeight)
        {
            Text = $"0Lux",
            TextColor = atmosphericColor,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        displayScreen.Controls.Add(Luminance);

        Acceleration3D = new Label(rowMargin, rowOffset + rowHeight * 4, displayScreen.Width - rowMargin * 2, rowHeight)
        {
            Text = $"0,0,0g",
            TextColor = motionColor,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        displayScreen.Controls.Add(Acceleration3D);

        AngularVelocity3D = new Label(rowMargin, rowOffset + rowHeight * 5, displayScreen.Width - rowMargin * 2, rowHeight)
        {
            Text = $"0,0,0rpm",
            TextColor = motionColor,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        displayScreen.Controls.Add(AngularVelocity3D);

        ButtonUp = new Label(rowMargin, rowOffset + rowHeight * 6, displayScreen.Width - rowMargin * 2, rowHeight)
        {
            Text = $"Released",
            TextColor = buttonColor,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        displayScreen.Controls.Add(ButtonUp);

        ButtonDown = new Label(rowMargin, rowOffset + rowHeight * 7, displayScreen.Width - rowMargin * 2, rowHeight)
        {
            Text = $"Released",
            TextColor = buttonColor,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        displayScreen.Controls.Add(ButtonDown);

        ButtonLeft = new Label(rowMargin, rowOffset + rowHeight * 8, displayScreen.Width - rowMargin * 2, rowHeight)
        {
            Text = $"Released",
            TextColor = buttonColor,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        displayScreen.Controls.Add(ButtonLeft);

        ButtonRight = new Label(rowMargin, rowOffset + rowHeight * 9, displayScreen.Width - rowMargin * 2, rowHeight)
        {
            Text = $"Released",
            TextColor = buttonColor,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        displayScreen.Controls.Add(ButtonRight);
    }
}