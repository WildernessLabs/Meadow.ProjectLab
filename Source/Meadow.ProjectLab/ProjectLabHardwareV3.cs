using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Modbus;
using Meadow.Peripherals.Leds;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Peripherals.Speakers;
using Meadow.Units;
using System;
using System.Threading;

namespace Meadow.Devices;

/// <summary>
/// Represents Project Lab V3 hardware and exposes its peripherals
/// </summary>
public class ProjectLabHardwareV3 : ProjectLabHardwareBase
{
    private readonly IF7CoreComputeMeadowDevice _device;
    private readonly IConnectorProvider _connectors;
    private IToneGenerator? _speaker;
    private IRgbPwmLed? _rgbled;
    private IGraphicsDisplay? _display;

    /// <summary>
    /// The MCP23008 IO expander connected to internal peripherals on Project Lab
    /// </summary>
    public Mcp23008? Mcp_1 { get; protected set; }

    /// <summary>
    /// The MCP23008 IO expander connected to IO headers and terminals on Project Lab
    /// </summary>
    public Mcp23008? Mcp_2 { get; protected set; }

    /// <summary>
    /// The MCP23008 IO expander that contains the ProjectLab hardware version 
    /// </summary>
    private Mcp23008? Mcp_Version { get; set; }

    /// <inheritdoc/>
    public sealed override II2cBus I2cBus { get; }

    /// <inheritdoc/>
    public sealed override ISpiBus SpiBus { get; }

    /// <inheritdoc/>
    public override IButton? UpButton { get; }

    /// <inheritdoc/>
    public override IButton? DownButton { get; }

    /// <inheritdoc/>
    public override IButton? LeftButton { get; }

    /// <inheritdoc/>
    public override IButton? RightButton { get; }

    /// <inheritdoc/>
    public override IToneGenerator? Speaker => GetSpeaker();

    /// <inheritdoc/>
    public override IRgbPwmLed? RgbLed => GetRgbLed();

    /// <summary>
    /// Display enable port for backlight control
    /// </summary>
    public IDigitalOutputPort? DisplayEnablePort { get; protected set; }

    internal ProjectLabHardwareV3(IF7CoreComputeMeadowDevice device, II2cBus i2cBus)
    {
        _device = device;

        I2cBus = i2cBus;

        SpiBus = device.CreateSpiBus(
            device.Pins.SCK,
            device.Pins.COPI,
            device.Pins.CIPO,
            new Frequency(48000, Frequency.UnitType.Kilohertz));

        IDigitalInterruptPort? mcp1Interrupt = null;
        IDigitalOutputPort? mcp1Reset = null;

        try
        {
            mcp1Interrupt = device.CreateDigitalInterruptPort(
                device.Pins.A05,
                InterruptMode.EdgeRising,
                ResistorMode.InternalPullDown);

            mcp1Reset = device.CreateDigitalOutputPort(device.Pins.D05);

            Mcp_1 = new Mcp23008(i2cBus, address: 0x20, mcp1Interrupt, mcp1Reset);

            Logger?.Trace("Mcp_1 up");
        }
        catch (Exception e)
        {
            Logger?.Trace($"Failed to create MCP1: {e.Message}");
            mcp1Interrupt?.Dispose();
        }

        IDigitalInterruptPort? mcp2Interrupt = null;

        try
        {
            // MCP the Second
            if (device.Pins.D19.Supports<IDigitalChannelInfo>(c => c.InterruptCapable))
            {
                mcp2Interrupt = device.CreateDigitalInterruptPort(
                    device.Pins.D19, InterruptMode.EdgeRising, ResistorMode.InternalPullDown);
            }

            Mcp_2 = new Mcp23008(I2cBus, address: 0x21, mcp2Interrupt);

            Logger?.Trace("Mcp_2 up");
        }
        catch (Exception e)
        {
            Logger?.Trace($"Failed to create MCP2: {e.Message}");
            mcp2Interrupt?.Dispose();
        }

        try
        {
            Mcp_Version = new Mcp23008(I2cBus, address: 0x27);
            Logger?.Trace("Mcp_Version up");
        }
        catch (Exception e)
        {
            Logger?.Trace($"ERR creating the MCP that has version information: {e.Message}");
        }

        Logger?.Trace("Instantiating buttons");
        var leftPort = Mcp_1?.CreateDigitalInterruptPort(Mcp_1.Pins.GP2, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
        if (leftPort != null) LeftButton = new PushButton(leftPort);
        var rightPort = Mcp_1?.CreateDigitalInterruptPort(Mcp_1.Pins.GP1, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
        if (rightPort != null) RightButton = new PushButton(rightPort);
        var upPort = Mcp_1?.CreateDigitalInterruptPort(Mcp_1.Pins.GP0, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
        if (upPort != null) UpButton = new PushButton(upPort);
        var downPort = Mcp_1?.CreateDigitalInterruptPort(Mcp_1.Pins.GP3, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
        if (downPort != null) DownButton = new PushButton(downPort);
        Logger?.Trace("Buttons up");

        if (RevisionNumber < 15) // before 3.e
        {
            Logger?.Trace("Hardware is 3.d or earlier");
            _connectors = new ConnectorProviderV3();
        }
        else
        {
            Logger?.Trace("Hardware is 3.e or later");
            _connectors = new ConnectorProviderV3e(this);
        }
    }

    /// <inheritdoc/>
    protected override IGraphicsDisplay? GetDefaultDisplay()
    {
        DisplayEnablePort ??= Mcp_1?.CreateDigitalOutputPort(Mcp_1.Pins.GP4, true);

        if (_display == null)
        {
            Logger?.Trace("Instantiating display");

            var chipSelectPort = DisplayHeader.Pins.CS.CreateDigitalOutputPort();
            var dcPort = DisplayHeader.Pins.DC.CreateDigitalOutputPort();
            var resetPort = DisplayHeader.Pins.RST.CreateDigitalOutputPort();

            Thread.Sleep(50);

            _display = new Ili9341(
                spiBus: SpiBus,
                chipSelectPort: chipSelectPort,
                dataCommandPort: dcPort,
                resetPort: resetPort,
                width: 240, height: 320,
                colorMode: ColorMode.Format16bppRgb565)
            {
                SpiBusMode = SpiClockConfiguration.Mode.Mode3,
                SpiBusSpeed = new Frequency(48000, Frequency.UnitType.Kilohertz)
            };

            ((Ili9341)_display).SetRotation(RotationType._270Degrees);

            Logger?.Trace("Display up");
        }

        return _display;
    }

    private IToneGenerator? GetSpeaker()
    {
        if (_speaker == null)
        {
            try
            {
                Logger?.Trace("Instantiating speaker");
                _speaker = new PiezoSpeaker(_device.Pins.D20);
                Logger?.Trace("Speaker up");
            }
            catch (Exception ex)
            {
                Logger?.Error($"Unable to create the Piezo Speaker: {ex.Message}");
            }
        }

        return _speaker;
    }

    private IRgbPwmLed? GetRgbLed()
    {
        if (_rgbled == null)
        {
            try
            {
                Logger?.Trace("Instantiating RGB LED");
                _rgbled = new RgbPwmLed(
                    redPwmPin: _device.Pins.D09,
                    greenPwmPin: _device.Pins.D10,
                    bluePwmPin: _device.Pins.D11,
                    CommonType.CommonAnode);
                Logger?.Trace("RGB LED up");
            }
            catch (Exception ex)
            {
                Logger?.Error($"Unable to create the RGB LED: {ex.Message}");
            }
        }

        return _rgbled;
    }

    internal override MikroBusConnector CreateMikroBus1()
    {
        Logger?.Trace("Creating MikroBus1 connector");
        return _connectors.CreateMikroBus1(_device, Mcp_2!);
    }

    internal override MikroBusConnector CreateMikroBus2()
    {
        Logger?.Trace("Creating MikroBus2 connector");
        return _connectors.CreateMikroBus2(_device, Mcp_2!);
    }

    internal override GroveDigitalConnector? CreateGroveDigitalConnector()
    {
        Logger?.Trace("Creating Grove digital connector");

        return new GroveDigitalConnector(
           nameof(GroveDigital),
            new PinMapping
            {
                new PinMapping.PinAlias(GroveDigitalConnector.PinNames.D0, _device.Pins.D16),
                new PinMapping.PinAlias(GroveDigitalConnector.PinNames.D1, _device.Pins.D17),
            });
    }

    internal override GroveDigitalConnector CreateGroveAnalogConnector()
    {
        Logger?.Trace("Creating Grove analog connector");

        return new GroveDigitalConnector(
           nameof(GroveAnalog),
            new PinMapping
            {
                new PinMapping.PinAlias(GroveDigitalConnector.PinNames.D0, _device.Pins.A00),
                new PinMapping.PinAlias(GroveDigitalConnector.PinNames.D1, _device.Pins.A01),
            });
    }

    internal override UartConnector CreateGroveUartConnector()
    {
        Logger?.Trace("Creating Grove UART connector");

        return new UartConnector(
           nameof(GroveUart),
            new PinMapping
            {
                new PinMapping.PinAlias(UartConnector.PinNames.RX, _device.Pins.D00),
                new PinMapping.PinAlias(UartConnector.PinNames.TX, _device.Pins.D01),
            },
            _device.PlatformOS.GetSerialPortName("com4")!);
    }

    internal override I2cConnector CreateQwiicConnector()
    {
        Logger?.Trace("Creating Qwiic I2C connector");

        return new I2cConnector(
           nameof(Qwiic),
            new PinMapping
            {
                new PinMapping.PinAlias(I2cConnector.PinNames.SCL, _device.Pins.D08),
                new PinMapping.PinAlias(I2cConnector.PinNames.SDA, _device.Pins.D07),
            },
            new I2cBusMapping(_device, 1));
    }

    internal override IOTerminalConnector CreateIOTerminalConnector()
    {
        Logger?.Trace("Creating IO terminal connector");

        return new IOTerminalConnector(
           nameof(IOTerminal),
            new PinMapping
            {
                new PinMapping.PinAlias(IOTerminalConnector.PinNames.A1, _device.Pins.PB1),
                new PinMapping.PinAlias(IOTerminalConnector.PinNames.D2, Mcp_2!.Pins.GP6),
                new PinMapping.PinAlias(IOTerminalConnector.PinNames.D3, Mcp_2.Pins.GP5),
            });
    }

    internal override DisplayConnector CreateDisplayConnector()
    {
        Logger?.Trace("Creating display connector");

        return new DisplayConnector(
           nameof(Display),
            new PinMapping
            {
                new PinMapping.PinAlias(DisplayConnector.PinNames.CS, Mcp_1!.Pins.GP5),
                new PinMapping.PinAlias(DisplayConnector.PinNames.RST, Mcp_1.Pins.GP7),
                new PinMapping.PinAlias(DisplayConnector.PinNames.DC, Mcp_1.Pins.GP6),
                new PinMapping.PinAlias(DisplayConnector.PinNames.CLK, _device.Pins.SCK),
                new PinMapping.PinAlias(DisplayConnector.PinNames.COPI, _device.Pins.COPI),
                new PinMapping.PinAlias(DisplayConnector.PinNames.LED, Mcp_1!.Pins.GP4),
            });
    }

    private byte? _revisionNumber;
    /// <summary>
    /// The hardware revision number, read from the on-board MCP
    /// </summary>
    protected byte RevisionNumber
    {
        get
        {
            _revisionNumber ??= Mcp_Version?.ReadFromPorts(Mcp23xxx.PortBank.A) ?? 0;
            return _revisionNumber.Value;
        }
    }

    private string? _revisionString;
    /// <inheritdoc/>
    public override string RevisionString
    {
        get
        {
            return _revisionString ??= $"v3.{(Mcp_Version == null ? "x" : RevisionNumber)}";
        }
    }

    /// <inheritdoc/>
    public override ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
    {
        return _connectors.GetModbusRtuClient(this, baudRate, dataBits, parity, stopBits);
    }
}