using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using Meadow.Logging;
using System;

namespace Meadow.Devices;

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
    /// <exception cref="NotSupportedException">Couldn't detect known ProjectLab hardware</exception>
    public static IProjectLabHardware Create()
    {
        IProjectLabHardware hardware;
        Logger? logger = Resolver.Log;

        Mcp23008? mcp = null;

        var device = Resolver.Device;

        logger?.Trace("Initializing Project Lab...");

        // make sure not getting instantiated before the App Initialize method
        if (device == null)
        {
            var msg = "ProjectLab instance must be created after App.Initialize()";
            logger?.Error(msg);
            throw new Exception(msg);
        }

        var i2cBus = device.CreateI2cBus();
        logger?.Debug("I2C Bus instantiated");

        IDigitalInterruptPort? mcpInterrupt = null;
        IDigitalOutputPort? mcpReset = null;
        bool isV3 = false;

        if (device is IF7FeatherMeadowDevice f)
        {
            try
            {
                mcpInterrupt = device.CreateDigitalInterruptPort(f.Pins.D09, InterruptMode.EdgeRising, ResistorMode.InternalPullDown);
                mcpReset = device.CreateDigitalOutputPort(f.Pins.D14);

                mcp = new Mcp23008(i2cBus, address: 0x20, mcpInterrupt, mcpReset);

                logger?.Trace("Mcp_1 up");
            }
            catch
            {
                logger?.Debug("Failed to create MCP1: could be a v1 board");
                mcpInterrupt?.Dispose();
                mcpReset?.Dispose();
            }
        }
        else if (device is IF7CoreComputeMeadowDevice c)
        {
            try
            {
                mcpReset = device.CreateDigitalOutputPort(c.Pins.PA10);

                mcp = new Mcp23008(i2cBus, address: 0x27, resetPort: mcpReset);

                logger?.Trace("Mcp_version up");
                isV3 = mcp.ReadFromPorts() < 17;
            }
            catch
            {
                logger?.Debug("Failed to create version MCP: could be a v3 board");
                isV3 = true;
            }
            finally
            {
                mcpReset?.Dispose();
                mcp = null;
            }
        }

        switch (device)
        {
            case IF7FeatherMeadowDevice feather when mcp is null:
                logger?.Info("Instantiating Project Lab v1 specific hardware");
                hardware = new ProjectLabHardwareV1(feather, i2cBus);
                break;
            case IF7FeatherMeadowDevice feather:
                logger?.Info("Instantiating Project Lab v2 specific hardware");
                hardware = new ProjectLabHardwareV2(feather, i2cBus, mcp);
                break;
            case IF7CoreComputeMeadowDevice ccm when isV3 == true:
                logger?.Info($"Instantiating Project Lab v3 specific hardware");
                hardware = new ProjectLabHardwareV3(ccm, i2cBus);
                break;
            case IF7CoreComputeMeadowDevice ccm:
                logger?.Info($"Instantiating Project Lab v4 specific hardware");
                hardware = new ProjectLabHardwareV4(ccm, i2cBus);
                break;
            default:
                throw new NotSupportedException();
        }

        return hardware;
    }
}