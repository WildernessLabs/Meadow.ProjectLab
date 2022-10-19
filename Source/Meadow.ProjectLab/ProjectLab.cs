using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Accelerometers;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Light;
using Meadow.Hardware;
using Meadow.Logging;
using Meadow.Units;
using System;

namespace Meadow.Devices
{
    public class ProjectLab
    {
        protected Logger? Logger { get; } = Resolver.Log;
        public ISpiBus SpiBus { get; }
        public II2cBus I2CBus { get; }

        private readonly Lazy<RgbPwmLed> _led;
        private readonly Lazy<St7789?> _display;
        private readonly Lazy<Bh1750?> _lightSensor;
        private readonly Lazy<PushButton> _upButton;
        private readonly Lazy<PushButton> _downButton;
        private readonly Lazy<PushButton> _leftButton;
        private readonly Lazy<PushButton> _rightButton;
        private readonly Lazy<Bme680?> _bme680;
        private readonly Lazy<PiezoSpeaker> _speaker;
        private readonly Lazy<Bmi270?> _imu;

        public RgbPwmLed Led => _led.Value;
        public St7789? Display => _display.Value;
        public Bh1750? LightSensor => _lightSensor.Value;
        public PushButton UpButton => _upButton.Value;
        public PushButton DownButton => _downButton.Value;
        public PushButton LeftButton => _leftButton.Value;
        public PushButton RightButton => _rightButton.Value;
        public Bme680? EnvironmentalSensor => _bme680.Value;
        public PiezoSpeaker Speaker => _speaker.Value;
        public Bmi270? IMU => _imu.Value;

        internal IProjectLabHardware Hardware { get; }

        private string? _rev;

        public Mcp23008? Mcp_1 { get; }
        public Mcp23008? Mcp_2 { get; }
        public Mcp23008? Mcp_Version { get; }

        public ProjectLab()
        {
            // check to see if we have an MCP23008 - it was introduced in v2 hardware
            if (Resolver.Device == null)
            {
                var msg = "ProjLab instance must be created no earlier than App.Initialize()";
                Logger?.Error(msg);
                throw new Exception(msg);
            }

            var device = Resolver.Device as IF7FeatherMeadowDevice;

            if (device == null)
            {
                var msg = "ProjLab Device must be an F7Feather";
                Logger?.Error(msg);
                throw new Exception(msg);
            }

            // create our busses
            Logger?.Info("Creating comms busses...");
            var config = new SpiClockConfiguration(
                           new Frequency(48000, Frequency.UnitType.Kilohertz),
                           SpiClockConfiguration.Mode.Mode3);

            SpiBus = Resolver.Device.CreateSpiBus(
                device.Pins.SCK,
                device.Pins.COPI,
                device.Pins.CIPO,
                config);

            I2CBus = device.CreateI2cBus();

            // determine hardware

            try
            {
                // MCP the First
                IDigitalInputPort mcp1_int = device.CreateDigitalInputPort(
                    device.Pins.D09, InterruptMode.EdgeRising, ResistorMode.InternalPullDown);
                IDigitalOutputPort mcp_Reset = device.CreateDigitalOutputPort(device.Pins.D14);

                Mcp_1 = new Mcp23008(I2CBus, address: 0x20, mcp1_int, mcp_Reset);
            }
            catch (Exception e)
            {
                Logger?.Trace($"Failed to create MCP1: {e.Message}");
            }
            try
            {
                // MCP the Second
                IDigitalInputPort mcp2_int = device.CreateDigitalInputPort(
                    device.Pins.D10, InterruptMode.EdgeRising, ResistorMode.InternalPullDown);
                Mcp_2 = new Mcp23008(I2CBus, address: 0x21, mcp2_int);
            }
            catch (Exception e)
            {
                Logger?.Trace($"Failed to create MCP2: {e.Message}");
            }
            try
            {
                Mcp_Version = new Mcp23008(I2CBus, address: 0x27);
            }
            catch (Exception e)
            {
                Logger?.Trace($"ERR creating the MCP that has version information: {e.Message}");
            }

            if (Mcp_1 == null)
            {
                Hardware = new ProjectLabHardwareV1(device, SpiBus);
            }
            else
            {
                Hardware = new ProjectLabHardwareV2(Mcp_1, Mcp_Version, device, SpiBus);
            }

            // lazy load all components
            try
            {
                _led = new Lazy<RgbPwmLed>(() =>
                    new RgbPwmLed(
                    device: device,
                    redPwmPin: device.Pins.OnboardLedRed,
                    greenPwmPin: device.Pins.OnboardLedGreen,
                    bluePwmPin: device.Pins.OnboardLedBlue));

                _display = new Lazy<St7789?>(() =>
                {
                    try
                    {
                        return Hardware.GetDisplay();
                    }
                    catch (Exception ex)
                    {
                        Resolver.Log.Error($"Unable to create the ST7789 Display Light Sensor: {ex.Message}");
                        return default;
                    }
                });


                _lightSensor = new Lazy<Bh1750?>(() =>
                {
                    try
                    {
                        return new Bh1750(
                            i2cBus: I2CBus,
                            measuringMode: Bh1750.MeasuringModes.ContinuouslyHighResolutionMode, // the various modes take differing amounts of time.
                            lightTransmittance: 0.5, // lower this to increase sensitivity, for instance, if it's behind a semi opaque window
                            address: (byte)Bh1750.Addresses.Address_0x23);
                    }
                    catch (Exception ex)
                    {
                        Resolver.Log.Error($"Unable to create the BH1750 Light Sensor: {ex.Message}");
                        return default;
                    }
                });


                _rightButton = new Lazy<PushButton>(Hardware.GetRightButton());

                if (!this.IsV1Hardware())
                {
                    _upButton = new Lazy<PushButton>(Hardware.GetUpButton());
                    _leftButton = new Lazy<PushButton>(Hardware.GetLeftButton());
                    _downButton = new Lazy<PushButton>(Hardware.GetDownButton());
                }

                _bme680 = new Lazy<Bme680?>(() =>
                {
                    try
                    {
                        return new Bme680(I2CBus, (byte)Bme680.Addresses.Address_0x76);
                    }
                    catch (Exception ex)
                    {
                        Resolver.Log.Error($"Unable to create the BME680 Environmental Sensor: {ex.Message}");
                        return default;
                    }
                });

                _speaker = new Lazy<PiezoSpeaker>(new PiezoSpeaker(device, device.Pins.D11));

                _imu = new Lazy<Bmi270?>(() =>
                {
                    try
                    {
                        return new Bmi270(I2CBus);
                    }
                    catch (Exception ex)
                    {
                        Resolver.Log.Error($"Unable to create the BMI270 IMU: {ex.Message}");
                        return default;
                    }
                });
            }

            catch (Exception ex)
            {
                Logger?.Error($"Error initializing ProjectLab: {ex.Message}");
            }
        }

        public string HardwareRevision
        {
            get => Hardware.GetRevisionString();
        }

        public static (
            IPin MB1_CS,
            IPin MB1_INT,
            IPin MB1_PWM,
            IPin MB1_AN,
            IPin MB1_SO,
            IPin MB1_SI,
            IPin MB1_SCK,
            IPin MB1_SCL,
            IPin MB1_SDA,

            IPin MB2_CS,
            IPin MB2_INT,
            IPin MB2_PWM,
            IPin MB2_AN,
            IPin MB2_SO,
            IPin MB2_SI,
            IPin MB2_SCK,
            IPin MB2_SCL,
            IPin MB2_SDA,

            IPin A0,
            IPin D03,
            IPin D04
            ) Pins = (
            Resolver.Device.GetPin("D14"),
            Resolver.Device.GetPin("D03"),
            Resolver.Device.GetPin("D04"),
            Resolver.Device.GetPin("A00"),
            Resolver.Device.GetPin("CIPO"),
            Resolver.Device.GetPin("COPI"),
            Resolver.Device.GetPin("SCK"),
            Resolver.Device.GetPin("D08"),
            Resolver.Device.GetPin("D07"),

            Resolver.Device.GetPin("A02"),
            Resolver.Device.GetPin("D04"),
            Resolver.Device.GetPin("D03"),
            Resolver.Device.GetPin("A01"),
            Resolver.Device.GetPin("CIPO"),
            Resolver.Device.GetPin("COPI"),
            Resolver.Device.GetPin("SCK"),
            Resolver.Device.GetPin("D08"),
            Resolver.Device.GetPin("D07"),

            Resolver.Device.GetPin("A00"),
            Resolver.Device.GetPin("D03"),
            Resolver.Device.GetPin("D04")
            );
    }
}

