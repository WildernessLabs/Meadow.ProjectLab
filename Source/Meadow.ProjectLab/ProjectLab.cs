using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using Meadow.Logging;
using Meadow.Units;
using System;

namespace Meadow.Devices
{
    public class ProjectLab
    {
        private ProjectLab() { }

        /// <summary>
        /// Create an instance of the ProjectLab class
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IProjectLabHardware Create()
        {
            IProjectLabHardware hardware;
            Logger? logger = Resolver.Log;
            II2cBus i2cBus;
            ISpiBus spiBus;

            // v2+ stuff
            Mcp23008? mcp_1 = null;

            logger?.Debug("Initializing Project Lab...");

            // make sure not getting instantiated before the App Initialize method
            if (Resolver.Device == null)
            {
                var msg = "ProjLab instance must be created no earlier than App.Initialize()";
                logger?.Error(msg);
                throw new Exception(msg);
            }

            if (!(Resolver.Device is IF7FeatherMeadowDevice device))
            {
                var msg = "ProjLab Device must be an F7Feather";
                logger?.Error(msg);
                throw new Exception(msg);
            }

            logger?.Debug("Creating comms busses...");
            var config = new SpiClockConfiguration(
                           new Frequency(48000, Frequency.UnitType.Kilohertz),
                           SpiClockConfiguration.Mode.Mode3);

            spiBus = Resolver.Device.CreateSpiBus(
                device.Pins.SCK,
                device.Pins.COPI,
                device.Pins.CIPO,
                config);

            logger?.Debug("SPI Bus instantiated");

            i2cBus = device.CreateI2cBus();

            logger?.Debug("I2C Bus instantiated");

            try
            {
                // MCP the First
                IDigitalInputPort mcp1_int = device.CreateDigitalInputPort(
                    device.Pins.D09, InterruptMode.EdgeRising, ResistorMode.InternalPullDown);
                IDigitalOutputPort mcp_Reset = device.CreateDigitalOutputPort(device.Pins.D14);

                mcp_1 = new Mcp23008(i2cBus, address: 0x20, mcp1_int, mcp_Reset);

                logger?.Trace("Mcp_1 up");
            }
            catch (Exception e)
            {
                logger?.Debug($"Failed to create MCP1: {e.Message}, could be a v1 board");
            }

            if (mcp_1 == null)
            {
                logger?.Debug("Instantiating Project Lab v1 specific hardware");
                hardware = new ProjectLabHardwareV1(device, spiBus, i2cBus);
            }
            else
            {
                logger?.Info("Instantiating Project Lab v2 specific hardware");
                hardware = new ProjectLabHardwareV2(device, spiBus, i2cBus, mcp_1);
            }

            return hardware;
        }
    }
}