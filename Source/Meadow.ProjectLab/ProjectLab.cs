using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using Meadow.Logging;
using System;

namespace Meadow.Devices
{
    /// <summary>
    /// Represents Project Lab hardware and exposes its peripherals
    /// </summary>
    public class ProjectLab
    {
        private ProjectLab() { }

        /// <summary>
        /// Create an instance of the ProjectLab class
        /// </summary>
        /// <returns>ProjectLab instance</returns>
        /// <exception cref="Exception">ProjectLab instance must be created after <c>App.Initialize()</c></exception>
        /// <exception cref="NotSupportedException"></exception>
        public static IProjectLabHardware Create()
        {
            IProjectLabHardware hardware;
            Logger? logger = Resolver.Log;

            // v2+ stuff
            Mcp23008? mcp1 = null;

            logger?.Trace("Initializing Project Lab...");

            var device = Resolver.Device; //convenience local var

            // make sure not getting instantiated before the App Initialize method
            if (Resolver.Device == null)
            {
                var msg = "ProjectLab instance must be created after App.Initialize()";
                logger?.Error(msg);
                throw new Exception(msg);
            }

            I32PinFeatherBoardPinout pins = device switch
            {
                IF7FeatherMeadowDevice f => f.Pins,
                IF7CoreComputeMeadowDevice c => c.Pins,
                _ => throw new NotSupportedException("Device must be a Feather F7 or F7 Core Compute module"),
            };

            var i2cBus = device.CreateI2cBus();
            logger?.Debug("I2C Bus instantiated");

            IDigitalInterruptPort? mcp1Interrupt = null;
            IDigitalOutputPort? mcp1Reset = null;

            try
            {
                if (device is IF7FeatherMeadowDevice)
                {
                    mcp1Interrupt = device.CreateDigitalInterruptPort(pins.D09, InterruptMode.EdgeRising, ResistorMode.InternalPullDown);
                    mcp1Reset = device.CreateDigitalOutputPort(pins.D14);

                    mcp1 = new Mcp23008(i2cBus, address: 0x20, mcp1Interrupt, mcp1Reset);

                    logger?.Trace("Mcp_1 up");
                }
            }
            catch (Exception e)
            {
                logger?.Debug($"Failed to create MCP1: {e.Message}, could be a v1 board?");
                mcp1Interrupt?.Dispose();
                mcp1Reset?.Dispose();
            }

            switch (device)
            {
                case IF7FeatherMeadowDevice feather when mcp1 is null:
                    logger?.Info("Instantiating Project Lab v1 specific hardware");
                    hardware = new ProjectLabHardwareV1(feather, i2cBus);
                    break;
                case IF7FeatherMeadowDevice feather:
                    logger?.Info("Instantiating Project Lab v2 specific hardware");
                    hardware = new ProjectLabHardwareV2(feather, i2cBus, mcp1);
                    break;
                case IF7CoreComputeMeadowDevice ccm:
                    logger?.Info("Instantiating Project Lab v3 specific hardware");
                    hardware = new ProjectLabHardwareV3(ccm, i2cBus);
                    break;
                default:
                    throw new NotSupportedException(); //should never get here
            }

            return hardware;
        }
    }
}