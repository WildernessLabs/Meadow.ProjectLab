using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Audio;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace ProjectLab_Demo;

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
            speaker.SetVolume(0.2f);
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
            upButton.PressStarted += (s, e) => displayController!.UpdateButtonUp(true);
            upButton.PressEnded += (s, e) => displayController!.UpdateButtonUp(false);
        }
        if (projectLab.DownButton is { } downButton)
        {
            downButton.PressStarted += (s, e) => displayController!.UpdateButtonDown(true);
            downButton.PressEnded += (s, e) => displayController!.UpdateButtonDown(false);
        }
        if (projectLab.LeftButton is { } leftButton)
        {
            leftButton.PressStarted += (s, e) => displayController!.UpdateButtonLeft(true);
            leftButton.PressEnded += (s, e) => displayController!.UpdateButtonLeft(false);
        }
        if (projectLab.RightButton is { } rightButton)
        {
            rightButton.PressStarted += (s, e) => displayController!.UpdateButtonRight(true);
            rightButton.PressEnded += (s, e) => displayController!.UpdateButtonRight(false);
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
        displayController!.UpdateTemperatureValue(e.New);
    }

    private void OnPressureSensorUpdated(object sender, IChangeResult<Pressure> e)
    {
        Resolver.Log.Info($"PRESSURE:    {e.New.Millibar:N1}mbar");
        displayController!.UpdatePressureValue(e.New);
    }

    private void OnHumiditySensorUpdated(object sender, IChangeResult<RelativeHumidity> e)
    {
        Resolver.Log.Info($"HUMIDITY:    {e.New.Percent:N1}%");
        displayController!.UpdateHumidityValue(e.New);
    }

    private void OnLightSensorUpdated(object sender, IChangeResult<Illuminance> e)
    {
        Resolver.Log.Info($"LIGHT:       {e.New.Lux:N1}lux");
        displayController!.UpdateIluminanceValue(e.New);
    }

    private void OnAccelerometerUpdated(object sender, IChangeResult<Acceleration3D> e)
    {
        Resolver.Log.Info($"ACCEL:       {e.New.X.Gravity:N1}, {e.New.Y.Gravity:N1}, {e.New.Z.Gravity:N1}g");
        displayController!.UpdateAcceleration3DValue(e.New);
    }

    private void OnGyroscopeUpdated(object sender, IChangeResult<AngularVelocity3D> e)
    {
        Resolver.Log.Info($"GYRO:        {e.New.X.DegreesPerSecond:N0}, {e.New.Y.DegreesPerSecond:N0}, {e.New.Z.DegreesPerSecond:N0}deg/s");
        displayController!.UpdateAngularVelocity3DValue(e.New);
    }

    public override async Task Run()
    {
        Resolver.Log.Info("Run...");

        if (audio != null)
        {
            await audio.PlaySystemSound(SystemSoundEffect.Success);
        }

        if (projectLab?.TemperatureSensor is { } temperature)
        {
            temperature.StartUpdating(TimeSpan.FromSeconds(5));
        }
        if (projectLab?.BarometricPressureSensor is { } barometer)
        {
            barometer.StartUpdating(TimeSpan.FromSeconds(5));
        }
        if (projectLab?.HumiditySensor is { } humidity)
        {
            humidity.StartUpdating(TimeSpan.FromSeconds(5));
        }
        if (projectLab?.LightSensor is { } luminance)
        {
            luminance.StartUpdating(TimeSpan.FromSeconds(5));
        }

        if (projectLab?.Accelerometer is { } accelerometer)
        {
            accelerometer.StartUpdating(TimeSpan.FromSeconds(5));
        }
        if (projectLab?.Gyroscope is { } gyroscope)
        {
            gyroscope.StartUpdating(TimeSpan.FromSeconds(5));
        }

        if (projectLab?.RgbLed is { } rgbLed)
        {
            Resolver.Log.Info("starting blink");
            _ = rgbLed.StartBlink(WildernessLabsColors.PearGreen, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(2000), 0.5f);
        }
    }
}