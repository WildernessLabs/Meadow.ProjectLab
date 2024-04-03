using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Audio;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace ProjectLab_Demo
{
    // Change F7FeatherV2 to F7FeatherV1 if using Feather V1 Meadow boards
    // Change to F7CoreComputeV2 for Project Lab V3.x
    public class MeadowApp : App<F7CoreComputeV2>
    {
        private IProjectLabHardware? projectLab;

        private DisplayController? displayController;

        private MicroAudio? audio;

        public override Task Initialize()
        {
            Resolver.Log.LogLevel = Meadow.Logging.LogLevel.Trace;

            Resolver.Log.Info("Initialize hardware...");

            projectLab = ProjectLab.Create();

            Resolver.Log.Info($"Running on ProjectLab Hardware {projectLab.RevisionString}");

            if (projectLab.RgbLed is { } rgbLed)
            {
                rgbLed.SetColor(Color.Blue);
            }

            if (projectLab.Display is { } display)
            {
                Resolver.Log.Trace("Creating DisplayController");
                displayController = new DisplayController(display, projectLab.RevisionString);
                Resolver.Log.Trace("DisplayController up");
            }

            if (projectLab.Speaker is { } speaker)
            {
                speaker.SetVolume(0.5f);
                audio = new MicroAudio(speaker);
            }

            if (projectLab.TemperatureSensor is { } temperatureSensor)
            {
                temperatureSensor.Updated += OnTemperatureSensorUpdated;
            }
            if (projectLab.BarometricPressureSensor is { } pressureSensor)
            {
                pressureSensor.Updated += OnPressureSensorUpdated;
            }
            if (projectLab.HumiditySensor is { } humiditySensor)
            {
                humiditySensor.Updated += OnHumiditySensorUpdated;
            }
            if (projectLab.LightSensor is { } lightSensor)
            {
                lightSensor.Updated += OnLightSensorUpdated;
            }

            if (projectLab.Accelerometer is { } accelerometer)
            {
                accelerometer.Updated += OnAccelerometerUpdated;
            }
            if (projectLab.Gyroscope is { } gyroscope)
            {
                gyroscope.Updated += OnGyroscopeUpdated;
            }

            if (projectLab.UpButton is { } upButton)
            {
                upButton.PressStarted += (s, e) => displayController.UpButtonState = true;
                upButton.PressEnded += (s, e) => displayController.UpButtonState = false;
            }
            if (projectLab.DownButton is { } downButton)
            {
                downButton.PressStarted += (s, e) => displayController.DownButtonState = true;
                downButton.PressEnded += (s, e) => displayController.DownButtonState = false;
            }
            if (projectLab.LeftButton is { } leftButton)
            {
                leftButton.PressStarted += (s, e) => displayController.LeftButtonState = true;
                leftButton.PressEnded += (s, e) => displayController.LeftButtonState = false;
            }
            if (projectLab.RightButton is { } rightButton)
            {
                rightButton.PressStarted += (s, e) => displayController.RightButtonState = true;
                rightButton.PressEnded += (s, e) => displayController.RightButtonState = false;
            }

            if (projectLab.Touchscreen is { } touchScreen)
            {
                touchScreen.TouchDown += (s, e) =>
                {
                    Resolver.Log.Info("touch down");
                };
                touchScreen.TouchUp += (s, e) =>
                {
                    Resolver.Log.Info("touch up");
                };
            }

            Resolver.Log.Info("Initialization complete");

            return base.Initialize();
        }

        private void OnTemperatureSensorUpdated(object sender, IChangeResult<Temperature> e)
        {
            Resolver.Log.Info($"TEMPERATURE: {e.New.Celsius:N1}C");
            displayController.Temperature = e.New;
        }

        private void OnPressureSensorUpdated(object sender, IChangeResult<Pressure> e)
        {
            Resolver.Log.Info($"PRESSURE:    {e.New.Millibar:N1}mbar");
            displayController.Pressure = e.New;
        }

        private void OnHumiditySensorUpdated(object sender, IChangeResult<RelativeHumidity> e)
        {
            Resolver.Log.Info($"HUMIDITY:    {e.New.Percent:N1}%");
            displayController.RelativeHumidity = e.New;
        }

        private void OnLightSensorUpdated(object sender, IChangeResult<Illuminance> e)
        {
            Resolver.Log.Info($"LIGHT:       {e.New.Lux:N1}lux");
            displayController.LightConditions = e.New;
        }

        private void OnAccelerometerUpdated(object sender, IChangeResult<Acceleration3D> e)
        {
            Resolver.Log.Info($"ACCEL:       {e.New.X.Gravity:0.0}, {e.New.Y.Gravity:0.0}, {e.New.Z.Gravity:0.0}g");
            displayController.AccelerationConditions = e.New;
        }

        private void OnGyroscopeUpdated(object sender, IChangeResult<AngularVelocity3D> e)
        {
            Resolver.Log.Info($"GYRO:        {e.New.X.DegreesPerSecond:0.0}, {e.New.Y.DegreesPerSecond:0.0}, {e.New.Z.DegreesPerSecond:0.0}deg/s");
            displayController.GyroConditions = e.New;
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run...");

            _ = audio?.PlaySystemSound(SystemSoundEffect.Success);

            if (projectLab.TemperatureSensor is { } temperature)
            {
                temperature.StartUpdating(TimeSpan.FromSeconds(5));
            }
            if (projectLab.BarometricPressureSensor is { } barometer)
            {
                barometer.StartUpdating(TimeSpan.FromSeconds(5));
            }
            if (projectLab.HumiditySensor is { } humidity)
            {
                humidity.StartUpdating(TimeSpan.FromSeconds(5));
            }
            if (projectLab.LightSensor is { } luminance)
            {
                luminance.StartUpdating(TimeSpan.FromSeconds(5));
            }

            if (projectLab.Accelerometer is { } accelerometer)
            {
                accelerometer.StartUpdating(TimeSpan.FromSeconds(5));
            }
            if (projectLab.Gyroscope is { } gyroscope)
            {
                gyroscope.StartUpdating(TimeSpan.FromSeconds(5));
            }

            displayController?.Update();

            Resolver.Log.Info("starting blink");
            _ = projectLab.RgbLed?.StartBlink(WildernessLabsColors.PearGreen, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(2000), 0.5f);

            return base.Run();
        }
    }
}