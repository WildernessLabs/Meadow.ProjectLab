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
    public class ProjectLab
    {
        private ProjectLab() { }

        /// <summary>
        /// Create an instance of the ProjectLab class
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IProjectLabHardware CreateProjectLab()
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

            //==== instantiate our busses
            var config = new SpiClockConfiguration(
                           new Frequency(48000, Frequency.UnitType.Kilohertz),
                           SpiClockConfiguration.Mode.Mode3);

            //==== instantiate stuff based on the hardware running it
            switch (Resolver.Device) {

                //==== F7 Feather
                case IF7FeatherMeadowDevice f7FeatherDevice:
                    logger?.Info("Device is powered by a Meadow F7");

                    //==== Comms
                    logger?.Debug("Creating comms busses...");
                    //---- SPI
                    spiBus = Resolver.Device.CreateSpiBus(
                        f7FeatherDevice.Pins.SCK,
                        f7FeatherDevice.Pins.COPI,
                        f7FeatherDevice.Pins.CIPO,
                        config);
                    logger?.Debug("SPI Bus instantiated");
                    //---- I2C
                    i2cBus = f7FeatherDevice.CreateI2cBus();
                    logger?.Debug("I2C Bus instantiated");

                    //---- MCP 1
                    try
                    {
                        // MCP interrupt
                        IDigitalInputPort mcp1_int = f7FeatherDevice.CreateDigitalInputPort(
                            f7FeatherDevice.Pins.D09, InterruptMode.EdgeRising, ResistorMode.InternalPullDown);
                        // MCP reset
                        IDigitalOutputPort mcp_Reset = f7FeatherDevice.CreateDigitalOutputPort(f7FeatherDevice.Pins.D14);
                        // MCP
                        mcp_1 = new Mcp23008(i2cBus, address: 0x20, mcp1_int, mcp_Reset);
                        logger?.Trace("Mcp_1 up");
                    }
                    catch (Exception e)
                    {
                        logger?.Debug($"Failed to create MCP1: {e.Message}, could be a v1 board");
                    }

                    //---- instantiate a v1 or v2 (V3+ is CCM)
                    if (mcp_1 == null)
                    {
                        logger?.Debug("Instantiating Project Lab v1 specific hardware");
                        hardware = new ProjectLabHardwareV1(f7FeatherDevice, spiBus, i2cBus);
                    }
                    else
                    {
                        logger?.Info("Instantiating Project Lab v2 specific hardware");
                        hardware = new ProjectLabHardwareV2(f7FeatherDevice, spiBus, i2cBus, mcp_1);
                    }
                    return hardware;

                //==== F7 Core-Compute Module
                case IF7CoreComputeMeadowDevice f7CcmDevice:
                    logger?.Info("Device is powered by a Meadow Core-Compute Module.");

                    //==== Comms
                    logger?.Debug("Creating comms busses...");
                    //---- SPI
                    spiBus = Resolver.Device.CreateSpiBus(
                        f7CcmDevice.Pins.SCK,
                        f7CcmDevice.Pins.COPI,
                        f7CcmDevice.Pins.CIPO,
                        config);
                    logger?.Debug("SPI Bus instantiated");
                    //---- I2C
                    i2cBus = f7CcmDevice.CreateI2cBus();
                    logger?.Debug("I2C Bus instantiated");

                    //---- MCP 1
                    try
                    {
                        // MCP interrupt
                        IDigitalInputPort mcp1_int = f7CcmDevice.CreateDigitalInputPort(
                            f7CcmDevice.Pins.SPI5_SCK, InterruptMode.EdgeRising, ResistorMode.InternalPullDown);
                        logger?.Debug("MCP 1 Interrupt up.");
                        // MCP reset
                        IDigitalOutputPort mcp_Reset = f7CcmDevice.CreateDigitalOutputPort(f7CcmDevice.Pins.D05);
                        logger?.Debug("MCP 1 Reset up.");
                        // MCP
                        mcp_1 = new Mcp23008(i2cBus, address: 0x20, mcp1_int, mcp_Reset);
                        logger?.Trace("Mcp_1 up");
                    }
                    catch (Exception e)
                    {
                        logger?.Debug($"Failed to create MCP1: {e.Message}, could be a v1 board");
                    }

                    //---- get the hardware up
                    logger?.Debug("Instantiating Project Lab v3 specific hardware");
                    hardware = new ProjectLabHardwareV3(f7CcmDevice, spiBus, i2cBus, mcp_1);
                    return hardware;

                default:
                    var msg = "ProjLab Device must be an F7Feather or CCM";
                    logger?.Error(msg);
                    throw new Exception(msg);
            }
        }
    }
}