using Meadow;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Displays;
using Meadow.Units;

namespace ProjectLab_Demo
{
    public class DisplayController
    {
        private bool isUpdating = false;
        private bool needsUpdate = false;

        private readonly string hardwareRev;
        private readonly MicroGraphics graphics;

        public Temperature? Temperature
        {
            get => temperature;
            set
            {
                temperature = value;
                Update();
            }
        }
        private Temperature? temperature;

        public RelativeHumidity? RelativeHumidity
        {
            get => relativeHumidity;
            set
            {
                relativeHumidity = value;
                Update();
            }
        }
        private RelativeHumidity? relativeHumidity;

        public Pressure? Pressure
        {
            get => pressure;
            set
            {
                pressure = value;
                Update();
            }
        }
        private Pressure? pressure;

        public Illuminance? LightConditions
        {
            get => lightConditions;
            set
            {
                lightConditions = value;
                Update();
            }
        }
        private Illuminance? lightConditions;

        public Acceleration3D? AccelerationConditions
        {
            get => accelerationConditions;
            set
            {
                accelerationConditions = value;
                Update();
            }
        }
        private Acceleration3D? accelerationConditions;

        public AngularVelocity3D? GyroConditions
        {
            get => gyroConditions;
            set
            {
                gyroConditions = value;
                Update();
            }
        }
        private AngularVelocity3D? gyroConditions;

        public bool UpButtonState
        {
            get => upButtonState;
            set
            {
                upButtonState = value;
                Update();
            }
        }
        private bool upButtonState = false;

        public bool DownButtonState
        {
            get => downButtonState;
            set
            {
                downButtonState = value;
                Update();
            }
        }
        private bool downButtonState = false;

        public bool LeftButtonState
        {
            get => leftButtonState;
            set
            {
                leftButtonState = value;
                Update();
            }
        }
        private bool leftButtonState = false;

        public bool RightButtonState
        {
            get => rightButtonState;
            set
            {
                rightButtonState = value;
                Update();
            }
        }
        private bool rightButtonState = false;

        public DisplayController(IPixelDisplay display, string hardwareRevision)
        {
            hardwareRev = hardwareRevision;
            graphics = new MicroGraphics(display)
            {
                CurrentFont = new Font12x20()
            };

            graphics.Clear(true);
        }

        public void Update()
        {
            if (isUpdating)
            {   //queue up the next update
                needsUpdate = true;
                return;
            }

            isUpdating = true;

            graphics.Clear();
            Draw();
            graphics.Show();

            isUpdating = false;

            if (needsUpdate)
            {
                needsUpdate = false;
                Update();
            }
        }

        private void DrawStatus(string label, string value, Color color, int yPosition)
        {
            graphics.DrawText(x: 2, y: yPosition, label, color: color);
            graphics.DrawText(x: graphics.Width - 2, y: yPosition, value, alignmentH: HorizontalAlignment.Right, color: color);
        }

        private void Draw()
        {
            graphics.DrawText(x: 2, y: 0, $"Hello PROJ LAB {hardwareRev}!", WildernessLabsColors.AzureBlue);

            if (Temperature is { } temp)
            {
                DrawStatus("Temperature:", $"{temp.Celsius:N1}°C", WildernessLabsColors.GalleryWhite, 35);
            }

            if (Pressure is { } pressure)
            {
                DrawStatus("Pressure:", $"{pressure.StandardAtmosphere:N1}atm", WildernessLabsColors.GalleryWhite, 55);
            }

            if (RelativeHumidity is { } humidity)
            {
                DrawStatus("Humidity:", $"{humidity.Percent:N1}%", WildernessLabsColors.GalleryWhite, 75);
            }

            if (LightConditions is { } light)
            {
                DrawStatus("Lux:", $"{light:N0}Lux", WildernessLabsColors.GalleryWhite, 95);
            }

            if (AccelerationConditions is { } acceleration)
            {
                DrawStatus("Accel:", $"{acceleration.X.Gravity:0.#},{acceleration.Y.Gravity:0.#},{acceleration.Z.Gravity:0.#}g", WildernessLabsColors.AzureBlue, 115);
            }

            if (GyroConditions is { } angular3D)
            {
                DrawStatus("Gyro:", $"{angular3D.X:0},{angular3D.Y:0},{angular3D.Z:0}rpm", WildernessLabsColors.AzureBlue, 135);
            }

            DrawStatus("Left:", $"{(LeftButtonState ? "pressed" : "released")}", WildernessLabsColors.ChileanFire, 200);
            DrawStatus("Down:", $"{(DownButtonState ? "pressed" : "released")}", WildernessLabsColors.ChileanFire, 180);
            DrawStatus("Up:", $"{(UpButtonState ? "pressed" : "released")}", WildernessLabsColors.ChileanFire, 160);
            DrawStatus("Right:", $"{(RightButtonState ? "pressed" : "released")}", WildernessLabsColors.ChileanFire, 220);
        }
    }
}