using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Audio;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace ProjectLab_Demo
{
    // Change F7FeatherV2 to F7FeatherV1 if using Feather V1 Meadow boards
    // Change to F7CoreComputeV2 for Project Lab V3.x
    public class MeadowApp : App<F7CoreComputeV2>
    {
        private DisplayController displayController;
        private MicroAudio audio;
        private IProjectLabHardware projLab;
        private readonly IDigitalInterruptPort _powerPort;
        private readonly IDigitalOutputPort _backlighPort;

        public override Task Initialize()
        {
            Resolver.Log.LogLevel = Meadow.Logging.LogLevel.Trace;

            Resolver.Log.Info("Initialize hardware...");

            //==== instantiate the project lab hardware
            projLab = ProjectLab.Create();

            Resolver.Log.Info($"Running on ProjectLab Hardware {projLab.RevisionString}");

            projLab.RgbLed?.SetColor(Color.Blue);

            projLab.Speaker?.SetVolume(0.5f);
            if (projLab.Speaker != null) audio = new MicroAudio(projLab.Speaker);

            //---- display controller (handles display updates)
            if (projLab.Display is { } display)
            {
                Resolver.Log.Trace("Creating DisplayController");
                displayController = new DisplayController(display, projLab.RevisionString);
                Resolver.Log.Trace("DisplayController up");
            }

            //---- Light Sensor
            if (projLab.LightSensor is { } lightSensor)
            {
                lightSensor.Updated += OnLightSensorUpdated;
            }

            //---- Atmospheric sensor
            if (projLab.HumiditySensor is { } humiditySensor)
            {
                humiditySensor.Updated += OnHumiditySensorUpdated;
            }

            if (projLab.BarometricPressureSensor is { } pressureSensor)
            {
                pressureSensor.Updated += OnPressureSensorUpdated;
            }

            //---- Accel/IMU
            if (projLab.Gyroscope is { } gyroscope)
            {
                gyroscope.Updated += OnGyroscopeUpdated;
            }

            if (projLab.Accelerometer is { } accelerometer)
            {
                accelerometer.Updated += OnAccelerometerUpdated;
            }

            if (projLab.TemperatureSensor is { } tempSensor)
            {
                tempSensor.Updated += OnTempSensorUpdated;
            }

            //---- buttons
            if (projLab.RightButton is { } rightButton)
            {
                rightButton.PressStarted += (s, e) => displayController.RightButtonState = true;
                rightButton.PressEnded += (s, e) => displayController.RightButtonState = false;
            }

            if (projLab.DownButton is { } downButton)
            {
                downButton.PressStarted += (s, e) => displayController.DownButtonState = true;
                downButton.PressEnded += (s, e) => displayController.DownButtonState = false;
            }
            if (projLab.LeftButton is { } leftButton)
            {
                leftButton.PressStarted += (s, e) => displayController.LeftButtonState = true;
                leftButton.PressEnded += (s, e) => displayController.LeftButtonState = false;
            }
            if (projLab.UpButton is { } upButton)
            {
                upButton.PressStarted += (s, e) => displayController.UpButtonState = true;
                upButton.PressEnded += (s, e) => displayController.UpButtonState = false;
            }

            if (projLab.Touchscreen != null)
            {
                Resolver.Log.Info("Initialize touch...");
                projLab.Touchscreen.TouchDown += Touchscreen_TouchDown;
                projLab.Touchscreen.TouchUp += Touchscreen_TouchUp;
            }

            Resolver.Log.Info("Initialization complete");

            return base.Initialize();
        }

        private void Touchscreen_TouchUp(ITouchScreen sender, TouchPoint point)
        {
            Resolver.Log.Info("touch up");
        }

        private void Touchscreen_TouchDown(ITouchScreen sender, TouchPoint point)
        {
            Resolver.Log.Info("touch down");
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run...");

            _ = audio.PlaySystemSound(SystemSoundEffect.Success);

            //---- Light Sensor
            if (projLab.LightSensor is { } bh1750)
            {
                bh1750.StartUpdating(TimeSpan.FromSeconds(5));
            }

            //---- Atmospheric sensor
            if (projLab.TemperatureSensor is { } temp)
            {
                temp.StartUpdating(TimeSpan.FromSeconds(5));
            }
            if (projLab.BarometricPressureSensor is { } barometer)
            {
                barometer.StartUpdating(TimeSpan.FromSeconds(5));
            }
            if (projLab.HumiditySensor is { } humidity)
            {
                humidity.StartUpdating(TimeSpan.FromSeconds(5));
            }

            //---- IMU
            if (projLab.Accelerometer is { } accelerometer)
            {
                accelerometer.StartUpdating(TimeSpan.FromSeconds(5));
            }
            if (projLab.Gyroscope is { } gyroscope)
            {
                gyroscope.StartUpdating(TimeSpan.FromSeconds(5));
            }

            displayController?.Update();

            Resolver.Log.Info("starting blink");
            _ = projLab.RgbLed?.StartBlink(WildernessLabsColors.PearGreen, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(2000), 0.5f);

            return base.Run();
        }

        private void OnGyroscopeUpdated(object sender, IChangeResult<AngularVelocity3D> e)
        {
            Resolver.Log.Info($"GYRO:        {e.New.X.DegreesPerSecond:0.0}, {e.New.Y.DegreesPerSecond:0.0}, {e.New.Z.DegreesPerSecond:0.0}deg/s");
            displayController.GyroConditions = e.New;
        }

        private void OnAccelerometerUpdated(object sender, IChangeResult<Acceleration3D> e)
        {
            Resolver.Log.Info($"ACCEL:       {e.New.X.Gravity:0.0}, {e.New.Y.Gravity:0.0}, {e.New.Z.Gravity:0.0}g");
            displayController.AccelerationConditions = e.New;
        }

        private void OnTempSensorUpdated(object sender, IChangeResult<Temperature> e)
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
    }
}