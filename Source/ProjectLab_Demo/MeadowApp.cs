﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Peripherals.Leds;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace ProjLab_Demo
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        DisplayController displayController;
        RgbPwmLed onboardLed;
        ProjectLab projLab;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            //==== RGB LED
            Resolver.Log.Info("Initializing onboard RGB LED.");
            onboardLed = new RgbPwmLed(device: Device,
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                CommonType.CommonAnode);
            Resolver.Log.Info("RGB LED up.");

            projLab = new ProjectLab();

            Resolver.Log.Info($"Running on ProjectLab Hardware {projLab.Hardware.RevisionString}");

            if (projLab.Hardware.Display is { } display)
            {
                displayController = new DisplayController(display, projLab.IsV1Hardware());
            }

            //---- BH1750 Light Sensor
            if (projLab.Hardware.LightSensor is { } bh1750)
            {
                Resolver.Log.Info($"Light sensor created");
                bh1750.Updated += Bh1750Updated;
            }

            //---- BME688 Atmospheric sensor
            if (projLab.Hardware.EnvironmentalSensor is { } bme688)
            {
                Resolver.Log.Info($"Environmental sensor created");
                bme688.Updated += Bme688Updated;
            }

            //---- BMI270 Accel/IMU
            if (projLab.Hardware.MotionSensor is { } bmi270)
            {
                Resolver.Log.Info($"IMU created");
                bmi270.Updated += Bmi270Updated;
            }

            //---- buttons
            if (projLab.Hardware.RightButton is { } rightButton)
            {
                Resolver.Log.Info($"Right button created");
                rightButton.PressStarted += (s, e) => displayController.RightButtonState = true;
                rightButton.PressEnded += (s, e) => displayController.RightButtonState = false;
            }

            if (projLab.Hardware.DownButton is { } downButton)
            {
                Resolver.Log.Info($"Down button created");
                downButton.PressStarted += (s, e) => displayController.DownButtonState = true;
                downButton.PressEnded += (s, e) => displayController.DownButtonState = false;
            }
            if (projLab.Hardware.LeftButton is { } leftButton)
            {
                Resolver.Log.Info($"Left button created");
                leftButton.PressStarted += (s, e) => displayController.LeftButtonState = true;
                leftButton.PressEnded += (s, e) => displayController.LeftButtonState = false;
            }
            if (projLab.Hardware.UpButton is { } upButton)
            {
                Resolver.Log.Info($"Up button created");
                upButton.PressStarted += (s, e) => displayController.UpButtonState = true;
                upButton.PressEnded += (s, e) => displayController.UpButtonState = false;
            }

            //---- heartbeat
            onboardLed.StartPulse(WildernessLabsColors.PearGreen);

            Console.WriteLine("Initialization complete");

            return base.Initialize();
        }

        public override Task Run()
        {
            Console.WriteLine("Run...");

            //---- BH1750 Light Sensor
            if (projLab.Hardware.LightSensor is { } bh1750)
            {
                bh1750.StartUpdating(TimeSpan.FromSeconds(5));
            }

            //---- BME688 Atmospheric sensor
            if (projLab.Hardware.EnvironmentalSensor is { } bme688)
            {
                bme688.StartUpdating(TimeSpan.FromSeconds(5));
            }

            //---- BMI270 Accel/IMU
            if (projLab.Hardware.MotionSensor is { } bmi270)
            {
                bmi270.StartUpdating(TimeSpan.FromSeconds(5));
            }

            if (displayController != null)
            {
                displayController.Update();
            }

            Console.WriteLine("starting blink");
            onboardLed.StartBlink(WildernessLabsColors.PearGreen, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(2000), 0.5f);

            return base.Run();
        }


        private void Bmi270Updated(object sender, IChangeResult<(Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D, Temperature? Temperature)> e)
        {
            Console.WriteLine($"BMI270: {e.New.Acceleration3D.Value.X.Gravity:0.0},{e.New.Acceleration3D.Value.Y.Gravity:0.0},{e.New.Acceleration3D.Value.Z.Gravity:0.0}g");
            if (displayController != null)
            {
                displayController.AccelerationConditions = e.New;
            }
        }

        private void Bme688Updated(object sender, IChangeResult<(Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure, Resistance? GasResistance)> e)
        {
            Console.WriteLine($"BME688: {(int)e.New.Temperature?.Celsius}°C - {(int)e.New.Humidity?.Percent}% - {(int)e.New.Pressure?.Millibar}mbar");
            if (displayController != null)
            {
                displayController.AtmosphericConditions = e.New;
            }
        }

        private void Bh1750Updated(object sender, IChangeResult<Illuminance> e)
        {
            Console.WriteLine($"BH1750: {e.New.Lux}");
            if (displayController != null)
            {
                displayController.LightConditions = e.New;
            }
        }
    }
}