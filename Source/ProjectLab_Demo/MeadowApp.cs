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
    public class MeadowApp : App<F7FeatherV2>
    {
        private DisplayController displayController;
        private MicroAudio audio;
        private IProjectLabHardware projLab;

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
                displayController = new DisplayController(display);
                Resolver.Log.Trace("DisplayController up");
            }

            //---- BH1750 Light Sensor
            if (projLab.LightSensor is { } bh1750)
            {
                bh1750.Updated += Bh1750Updated;
            }

            //---- BME688 Atmospheric sensor
            if (projLab.EnvironmentalSensor is { } bme688)
            {
                bme688.Updated += Bme688Updated;
            }

            //---- BMI270 Accel/IMU
            if (projLab.MotionSensor is { } bmi270)
            {
                bmi270.Updated += Bmi270Updated;
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

            Resolver.Log.Info("Initialization complete");

            return base.Initialize();
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run...");

            _ = audio.PlaySystemSound(SystemSoundEffect.Success);

            //---- BH1750 Light Sensor
            if (projLab.LightSensor is { } bh1750)
            {
                bh1750.StartUpdating(TimeSpan.FromSeconds(5));
            }

            //---- BME688 Atmospheric sensor
            if (projLab.EnvironmentalSensor is { } bme688)
            {
                bme688.StartUpdating(TimeSpan.FromSeconds(5));
            }

            //---- BMI270 Accel/IMU
            if (projLab.MotionSensor is { } bmi270)
            {
                bmi270.StartUpdating(TimeSpan.FromSeconds(5));
            }

            displayController?.Update();

            Resolver.Log.Info("starting blink");
            _ = projLab.RgbLed?.StartBlink(WildernessLabsColors.PearGreen, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(2000), 0.5f);

            return base.Run();
        }


        private void Bmi270Updated(object sender, IChangeResult<(Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D, Temperature? Temperature)> e)
        {
            Resolver.Log.Info($"BMI270: {e.New.Acceleration3D!.Value.X.Gravity:0.0},{e.New.Acceleration3D.Value.Y.Gravity:0.0},{e.New.Acceleration3D.Value.Z.Gravity:0.0}g");
            displayController.AccelerationConditions = e.New;
        }

        private void Bme688Updated(object sender, IChangeResult<(Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure, Resistance? GasResistance)> e)
        {
            Resolver.Log.Info($"BME688: {(e.New.Temperature?.Celsius ?? double.NaN)}C - {(e.New.Humidity?.Percent ?? double.NaN)}% - {(e.New.Pressure?.Millibar ?? double.NaN)}mbar");
            displayController.AtmosphericConditions = e.New;
        }

        private void Bh1750Updated(object sender, IChangeResult<Illuminance> e)
        {
            Resolver.Log.Info($"BH1750: {e.New.Lux}");
            displayController.LightConditions = e.New;
        }
    }
}