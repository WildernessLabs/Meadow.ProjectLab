using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Accelerometers;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Light;
using Meadow.Hardware;
using Meadow.Logging;
using Meadow.Modbus;
using Meadow.Units;
using System;

namespace Meadow.Devices
{
    public class ProjectLab : IProjectLabHardware
    {
        protected Logger? Logger { get; } = Resolver.Log;
        protected IProjectLabHardware Hardware { get; set; }

        public II2cBus I2cBus { get; protected set; }
        public ISpiBus SpiBus { get; protected set; }
        public St7789? Display => Hardware.Display;
        public Bh1750? LightSensor => Hardware.LightSensor;
        public Bme688? EnvironmentalSensor => Hardware.EnvironmentalSensor;
        public Bmi270? MotionSensor => Hardware.MotionSensor;
        public PiezoSpeaker? Speaker => Hardware.Speaker;
        public PushButton? LeftButton => Hardware.LeftButton;
        public PushButton? RightButton => Hardware.RightButton;
        public PushButton? UpButton => Hardware.UpButton;
        public PushButton? DownButton => Hardware.DownButton;
        public string RevisionString => Hardware.RevisionString;

        /// <summary>
        /// Create an instance of the ProjectLab class
        /// </summary>
        /// <exception cref="Exception"></exception>
        public ProjectLab()
        {
            // v2+ stuff
            Mcp23008? mcp_1 = null;

            Logger?.Debug("Initializing Project Lab...");

            // make sure not getting instantiated before the App Initialize method
            if (Resolver.Device == null)
            {
                var msg = "ProjLab instance must be created no earlier than App.Initialize()";
                Logger?.Error(msg);
                throw new Exception(msg);
            }

            if (!(Resolver.Device is IF7FeatherMeadowDevice device))
            {
                var msg = "ProjLab Device must be an F7Feather";
                Logger?.Error(msg);
                throw new Exception(msg);
            }

            Logger?.Debug("Creating comms busses...");
            var config = new SpiClockConfiguration(
                           new Frequency(48000, Frequency.UnitType.Kilohertz),
                           SpiClockConfiguration.Mode.Mode3);

            SpiBus = Resolver.Device.CreateSpiBus(
                device.Pins.SCK,
                device.Pins.COPI,
                device.Pins.CIPO,
                config);

            Logger?.Debug("SPI Bus instantiated");

            I2cBus = device.CreateI2cBus();

            Logger?.Debug("I2C Bus instantiated");

            try
            {
                // MCP the First
                IDigitalInputPort mcp1_int = device.CreateDigitalInputPort(
                    device.Pins.D09, InterruptMode.EdgeRising, ResistorMode.InternalPullDown);
                IDigitalOutputPort mcp_Reset = device.CreateDigitalOutputPort(device.Pins.D14);

                mcp_1 = new Mcp23008(I2cBus, address: 0x20, mcp1_int, mcp_Reset);

                Logger?.Trace("Mcp_1 up");
            }
            catch (Exception e)
            {
                Logger?.Debug($"Failed to create MCP1: {e.Message}, could be a v1 board");
            }

            if (mcp_1 == null)
            {
                Logger?.Debug("Instantiating Project Lab v1 specific hardware");
                Hardware = new ProjectLabHardwareV1(device, SpiBus, I2cBus);
            }
            else
            {
                Logger?.Info("Instantiating Project Lab v2 specific hardware");
                Hardware = new ProjectLabHardwareV2(device, SpiBus, I2cBus, mcp_1);
            }
        }

        /// <summary>
        /// Gets a ModbusRtuClient for the on-baord RS485 connector
        /// </summary>
        /// <param name="baudRate"></param>
        /// <param name="dataBits"></param>
        /// <param name="parity"></param>
        /// <param name="stopBits"></param>
        /// <returns></returns>
        public ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
        {
            return Hardware.GetModbusRtuClient(baudRate, dataBits, parity, stopBits);
        }
    }
}