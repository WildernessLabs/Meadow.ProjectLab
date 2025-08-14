using Meadow.Hardware;
using System.Diagnostics;

namespace Meadow.Devices;

/// <summary>
/// Represents Project Lab V5a hardware and exposes its peripherals
/// </summary>
internal class ProjectLabHardwareV5a : ProjectLabHardwareV5
{
    internal ProjectLabHardwareV5a(IF7CoreComputeMeadowDevice device, II2cBus i2cBus) : base(device, i2cBus)
    {
    }

    internal override IPin GetVersionResetPin(IF7CoreComputeMeadowDevice device)
    {
        return device.Pins.PH10;
    }

    internal override DisplayConnector CreateDisplayConnector()
    {
        Logger?.Trace("Creating display connector");
        Debug.Assert(Mcp_1 != null, nameof(Mcp_1) + " != null");
        Debug.Assert(Mcp_2 != null, nameof(Mcp_2) + " != null");

        return new DisplayConnector(
           nameof(Display),
            new PinMapping
            {
                new PinMapping.PinAlias(DisplayConnector.PinNames.DISPLAY_CS, _device.Pins.PB4),
                new PinMapping.PinAlias(DisplayConnector.PinNames.DISPLAY_RST, _device.Pins.PB8),
                new PinMapping.PinAlias(DisplayConnector.PinNames.DISPLAY_DC, _device.Pins.PI11),
                new PinMapping.PinAlias(DisplayConnector.PinNames.DISPLAY_CLK, _device.Pins.SPI5_SCK),
                new PinMapping.PinAlias(DisplayConnector.PinNames.DISPLAY_COPI, _device.Pins.SPI5_COPI),
                new PinMapping.PinAlias(DisplayConnector.PinNames.DISPLAY_LED, Mcp_1.Pins.GP5),
                new PinMapping.PinAlias(DisplayConnector.PinNames.TOUCH_INT, Mcp_2.Pins.GP0),
                new PinMapping.PinAlias(DisplayConnector.PinNames.TOUCH_CLK, _device.Pins.I2C1_SCL),
                new PinMapping.PinAlias(DisplayConnector.PinNames.TOUCH_SDA, _device.Pins.I2C1_SDA),
                new PinMapping.PinAlias(DisplayConnector.PinNames.TOUCH_RST, Mcp_1.Pins.GP6),
            },
            new SpiBusMapping(_device, _device.Pins.SPI5_SCK, _device.Pins.SPI5_COPI, _device.Pins.SPI5_CIPO),
            new I2cBusMapping(_device, 1)
            );
    }
}
