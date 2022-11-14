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
using Meadow.Modbus;
using Meadow.Units;
using System;

namespace Meadow.Devices
{
    public class ProjectLab
    {
        protected Logger? Logger { get; } = Resolver.Log;
        public IProjectLabHardware Hardware { get; protected set; }

        /// <summary>
        /// Create an instance of the ProjectLab class
        /// </summary>
        /// <exception cref="Exception"></exception>
        public ProjectLab()
        {
            // shared stuff
            II2cBus i2cBus;
            ISpiBus spiBus;

            // v2+ stuff
            Mcp23008? mcp_1 = null;
            Mcp23008? mcp_2 = null;
            Mcp23008? mcp_Version = null;

            Logger?.Debug("Initializing Project Lab...");

            //==== hardware and lifecycle checks
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

            //==== create our busses
            Logger?.Info("Creating comms busses...");
            var config = new SpiClockConfiguration(
                           new Frequency(48000, Frequency.UnitType.Kilohertz),
                           SpiClockConfiguration.Mode.Mode3);

            spiBus = Resolver.Device.CreateSpiBus(
                device.Pins.SCK,
                device.Pins.COPI,
                device.Pins.CIPO,
                config);

            Logger?.Info("SPI Bus instantiated.");

            i2cBus = device.CreateI2cBus();

            Logger?.Info("I2C Bus instantiated.");

            //==== determine hardware
            try
            {
                // MCP the First
                IDigitalInputPort mcp1_int = device.CreateDigitalInputPort(
                    device.Pins.D09, InterruptMode.EdgeRising, ResistorMode.InternalPullDown);
                IDigitalOutputPort mcp_Reset = device.CreateDigitalOutputPort(device.Pins.D14);

                mcp_1 = new Mcp23008(i2cBus, address: 0x20, mcp1_int, mcp_Reset);

                Logger?.Info("Mcp_1 up.");
            }
            catch (Exception e)
            {
                Logger?.Trace($"Failed to create MCP1: {e.Message}, could be a v1 board.");
            }
            try
            {
                // MCP the Second
                IDigitalInputPort mcp2_int = device.CreateDigitalInputPort(
                    device.Pins.D10, InterruptMode.EdgeRising, ResistorMode.InternalPullDown);
                mcp_2 = new Mcp23008(i2cBus, address: 0x21, mcp2_int);

                Logger?.Info("Mcp_2 up.");
            }
            catch (Exception e)
            {
                Logger?.Trace($"Failed to create MCP2: {e.Message}");
            }
            try
            {
                mcp_Version = new Mcp23008(i2cBus, address: 0x27);
                Logger?.Info("Mcp_Version up.");
            }
            catch (Exception e)
            {
                Logger?.Trace($"ERR creating the MCP that has version information: {e.Message}");
            }

            //==== instantiate the appropriate hardware per the version
            if (mcp_1 == null)
            {
                Logger?.Info("Instantiating Project Lab v1 specific hardware.");
                Hardware = new ProjectLabHardwareV1(device, spiBus, i2cBus);
            }
            else
            {
                Logger?.Info("Instantiating Project Lab v2 specific hardware.");
                Hardware = new ProjectLabHardwareV2(device, spiBus, i2cBus, mcp_1, mcp_2, mcp_Version);
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