using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Accelerometers;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Light;
using Meadow.Hardware;
using Meadow.Logging;
using Meadow.Units;

namespace Meadow.Devices
{
    public class ProjectLab : IProjectLabHardware
    {
        protected Logger? Logger { get; } = Resolver.Log;
        internal IProjectLabHardware Hardware { get; }

        //==== convenience rollup properties
        // common ones
        public ISpiBus SpiBus { get; set; }
        public II2cBus I2CBus { get; set; }
        public Bh1750? LightSensor { get; set; }
        public Bme688? EnvironmentalSensor { get; set; }
        public PiezoSpeaker Speaker { get; set; }
        public Bmi270? MotionSensor { get; set; }
        // version specific
        public St7789? Display { get => Hardware.Display; set => Hardware.Display = value; }
        public PushButton UpButton { get => Hardware.UpButton; set => Hardware.UpButton = value; }
        public PushButton DownButton { get => Hardware.DownButton; set => Hardware.DownButton = value; }
        public PushButton LeftButton { get => Hardware.LeftButton; set => Hardware.LeftButton = value; }
        public PushButton RightButton { get => Hardware.RightButton; set => Hardware.RightButton = value; }
        public string RevisionString => Hardware.RevisionString;

        // v2+ stuff
        public Mcp23008? Mcp_1 { get; }
        public Mcp23008? Mcp_2 { get; }
        public Mcp23008? Mcp_Version { get; }

        public ProjectLab()
        {
            // make sure not getting instantiated before the App Initialize method
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

            Logger?.Info("SPI Bus instantiated.");

            I2CBus = device.CreateI2cBus();

            Logger?.Info("I2C Bus instantiated.");

            // determine hardware

            try
            {
                // MCP the First
                IDigitalInputPort mcp1_int = device.CreateDigitalInputPort(
                    device.Pins.D09, InterruptMode.EdgeRising, ResistorMode.InternalPullDown);
                IDigitalOutputPort mcp_Reset = device.CreateDigitalOutputPort(device.Pins.D14);

                Mcp_1 = new Mcp23008(I2CBus, address: 0x20, mcp1_int, mcp_Reset);

                Logger?.Info("Mcp_1 up.");
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

                Logger?.Info("Mcp_2 up.");
            }
            catch (Exception e)
            {
                Logger?.Trace($"Failed to create MCP2: {e.Message}");
            }
            try
            {
                Mcp_Version = new Mcp23008(I2CBus, address: 0x27);
                Logger?.Info("Mcp_Version up.");
            }
            catch (Exception e)
            {
                Logger?.Trace($"ERR creating the MCP that has version information: {e.Message}");
            }

            if (Mcp_1 == null)
            {
                Logger?.Info("Instantiating Project Lab v1 specific hardware.");
                Hardware = new ProjectLabHardwareV1(device, SpiBus);
            }
            else
            {
                Logger?.Info("Instantiating Project Lab v2 specific hardware.");
                Hardware = new ProjectLabHardwareV2(Mcp_1, Mcp_Version, device, SpiBus);
            }

            //==== Initialize the shared/common stuff
            try
            {
                Logger?.Info("Instantiating light sensor.");
                Hardware.LightSensor = new Bh1750(
                    i2cBus: I2CBus,
                    measuringMode: Bh1750.MeasuringModes.ContinuouslyHighResolutionMode, // the various modes take differing amounts of time.
                    lightTransmittance: 0.5, // lower this to increase sensitivity, for instance, if it's behind a semi opaque window
                    address: (byte)Bh1750.Addresses.Address_0x23);
                Logger?.Info("Light sensor up.");
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Unable to create the BH1750 Light Sensor: {ex.Message}");
            }

            //rightButton = new Lazy<PushButton>(Hardware.RightButton());

            //if (!this.IsV1Hardware())
            //{
            //    upButton = new Lazy<PushButton>(Hardware.UpButton());
            //    leftButton = new Lazy<PushButton>(Hardware.LeftButton());
            //    downButton = new Lazy<PushButton>(Hardware.DownButton());
            //}

            try
            {
                Logger?.Info("Instantiating environmental sensor.");
                Hardware.EnvironmentalSensor = new Bme688(I2CBus, (byte)Bme688.Addresses.Address_0x76);
                Logger?.Info("Environmental sensor up.");
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Unable to create the BME688 Environmental Sensor: {ex.Message}");
            }

            try
            {
                Logger?.Info("Instantiating speaker.");
                Hardware.Speaker = new PiezoSpeaker(device, device.Pins.D11);
                Logger?.Info("Speaker up.");
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Unable to create the Piezo Speaker: {ex.Message}");
            }


            try
            {
                Logger?.Info("Instantiating motion sensor.");
                Hardware.MotionSensor = new Bmi270(I2CBus);
                Logger?.Info("Motion sensor up.");
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Unable to create the BMI270 IMU: {ex.Message}");
            }
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