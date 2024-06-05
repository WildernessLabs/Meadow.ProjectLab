using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Peripherals.Displays;
using Meadow.Units;

namespace ProjectLab_Demo;

public class DisplayController
{
    private readonly int rowOffset = 29;
    private readonly int rowHeight = 21;
    private readonly int rowMargin = 2;

    private Color foregroundColor = Color.White;
    private Color atmosphericColor = Color.White;
    private Color motionColor = Color.FromHex("23ABE3");
    private Color buttonColor = Color.FromHex("EF7D3B");

    private readonly Font12x20 font12X20 = new();

    private readonly DisplayScreen displayScreen;

    private readonly Label temperature;
    private readonly Label humidity;
    private readonly Label pressure;
    private readonly Label iluminance;
    private readonly Label acceleration3D;
    private readonly Label angularVelocity3D;
    private readonly Label buttonUp;
    private readonly Label buttonDown;
    private readonly Label buttonLeft;
    private readonly Label buttonRight;

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

        displayScreen.Controls.Add(CreateLeftLabel("Temperature:", atmosphericColor, rowMargin, rowOffset, displayScreen.Width, rowHeight));
        displayScreen.Controls.Add(CreateLeftLabel("Pressure:", atmosphericColor, rowMargin, rowOffset + rowHeight, displayScreen.Width, rowHeight));
        displayScreen.Controls.Add(CreateLeftLabel("Humidity:", atmosphericColor, rowMargin, rowOffset + rowHeight * 2, displayScreen.Width, rowHeight));
        displayScreen.Controls.Add(CreateLeftLabel("Illuminance:", atmosphericColor, rowMargin, rowOffset + rowHeight * 3, displayScreen.Width, rowHeight));
        displayScreen.Controls.Add(CreateLeftLabel("Accel:", motionColor, rowMargin, rowOffset + rowHeight * 4, displayScreen.Width, rowHeight));
        displayScreen.Controls.Add(CreateLeftLabel("Gyro:", motionColor, rowMargin, rowOffset + rowHeight * 5, displayScreen.Width, rowHeight));
        displayScreen.Controls.Add(CreateLeftLabel("Up:", buttonColor, rowMargin, rowOffset + rowHeight * 6, displayScreen.Width, rowHeight));
        displayScreen.Controls.Add(CreateLeftLabel("Down:", buttonColor, rowMargin, rowOffset + rowHeight * 7, displayScreen.Width, rowHeight));
        displayScreen.Controls.Add(CreateLeftLabel("Left:", buttonColor, rowMargin, rowOffset + rowHeight * 8, displayScreen.Width, rowHeight));
        displayScreen.Controls.Add(CreateLeftLabel("Right:", buttonColor, rowMargin, rowOffset + rowHeight * 9, displayScreen.Width, rowHeight));

        temperature = CreateRightLabel("0ºC", atmosphericColor, rowMargin, rowOffset, displayScreen.Width - rowMargin * 2, rowHeight);
        pressure = CreateRightLabel("0atm", atmosphericColor, rowMargin, rowOffset + rowHeight, displayScreen.Width - rowMargin * 2, rowHeight);
        humidity = CreateRightLabel("0%", atmosphericColor, rowMargin, rowOffset + rowHeight * 2, displayScreen.Width - rowMargin * 2, rowHeight);
        iluminance = CreateRightLabel("0Lux", atmosphericColor, rowMargin, rowOffset + rowHeight * 3, displayScreen.Width - rowMargin * 2, rowHeight);
        acceleration3D = CreateRightLabel("0,0,0g", motionColor, rowMargin, rowOffset + rowHeight * 4, displayScreen.Width - rowMargin * 2, rowHeight);
        angularVelocity3D = CreateRightLabel("0,0,0rpm", motionColor, rowMargin, rowOffset + rowHeight * 5, displayScreen.Width - rowMargin * 2, rowHeight);
        buttonUp = CreateRightLabel("Released", buttonColor, rowMargin, rowOffset + rowHeight * 6, displayScreen.Width - rowMargin * 2, rowHeight);
        buttonDown = CreateRightLabel("Released", buttonColor, rowMargin, rowOffset + rowHeight * 7, displayScreen.Width - rowMargin * 2, rowHeight);
        buttonLeft = CreateRightLabel("Released", buttonColor, rowMargin, rowOffset + rowHeight * 8, displayScreen.Width - rowMargin * 2, rowHeight);
        buttonRight = CreateRightLabel("Released", buttonColor, rowMargin, rowOffset + rowHeight * 9, displayScreen.Width - rowMargin * 2, rowHeight);

        displayScreen.Controls.Add(new[] { temperature, pressure, humidity, iluminance, acceleration3D, angularVelocity3D, buttonUp, buttonDown, buttonLeft, buttonRight });
    }

    private Label CreateLeftLabel(string text, Color color, int left, int top, int width, int height)
    {
        return new Label(left, top, width, height)
        {
            Text = text,
            TextColor = color,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Left
        };
    }

    private Label CreateRightLabel(string text, Color color, int left, int top, int width, int height)
    {
        return new Label(left, top, width, height)
        {
            Text = text,
            TextColor = color,
            Font = font12X20,
            HorizontalAlignment = HorizontalAlignment.Right
        };
    }

    public void UpdateTemperatureValue(Temperature temperature)
    {
        this.temperature.Text = $"{temperature.Celsius:N1}ºC";
    }

    public void UpdatePressureValue(Pressure pressure)
    {
        this.pressure.Text = $"{pressure.StandardAtmosphere:N1}atm";
    }

    public void UpdateHumidityValue(RelativeHumidity humidity)
    {
        this.humidity.Text = $"{humidity.Percent:N1}%";
    }

    public void UpdateIluminanceValue(Illuminance iluminance)
    {
        this.iluminance.Text = $"{iluminance.Lux:N1}Lux";
    }

    public void UpdateAcceleration3DValue(Acceleration3D acceleration3D)
    {
        this.acceleration3D.Text = $"{acceleration3D.X.Gravity:N1},{acceleration3D.Y.Gravity:N1},{acceleration3D.Z.Gravity:N1}g";
    }

    public void UpdateAngularVelocity3DValue(AngularVelocity3D angularVelocity3D)
    {
        this.angularVelocity3D.Text = $"{angularVelocity3D.X.DegreesPerSecond:N0},{angularVelocity3D.Y.DegreesPerSecond:N0},{angularVelocity3D.Z.DegreesPerSecond:N0}º/s";
    }

    public void UpdateButtonUp(bool isPressed)
    {
        Resolver.Log.Info($"Button Up: {isPressed}");
        buttonUp.Text = isPressed ? "Pressed" : "Released";
    }

    public void UpdateButtonDown(bool isPressed)
    {
        Resolver.Log.Info($"Button Down: {isPressed}");
        buttonDown.Text = isPressed ? "Pressed" : "Released";
    }

    public void UpdateButtonLeft(bool isPressed)
    {
        Resolver.Log.Info($"Button Left: {isPressed}");
        buttonLeft.Text = isPressed ? "Pressed" : "Released";
    }

    public void UpdateButtonRight(bool isPressed)
    {
        Resolver.Log.Info($"Button Right: {isPressed}");
        buttonRight.Text = isPressed ? "Pressed" : "Released";
    }
}