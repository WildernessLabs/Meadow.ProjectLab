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
using Meadow.Units;
using System;
using System.Diagnostics;
using System.Threading;

namespace Meadow.Devices;

/// <summary>
/// Represents Project Lab V2 hardware and exposes its peripherals
/// </summary>
public class ProjectLabHardwareV2 : ProjectLabHardwareBase
{
    private readonly IF7FeatherMeadowDevice _device;
    private PiezoSpeaker? _speaker;
    private IGraphicsDisplay? _display;

    /// <summary>
    /// The MCP23008 IO expander connected to internal peripherals
    /// </summary>
    public Mcp23008 Mcp_1 { get; protected set; }

    /// <summary>
    /// The MCP23008 IO expander connected to IO headers and terminals on Project Lab
    /// </summary>
    public Mcp23008? Mcp_2 { get; protected set; }

    /// <summary>
    /// The MCP23008 IO expander that contains the ProjectLab hardware version 
    /// </summary>
    private Mcp23008? Mcp_Version { get; }

    /// <inheritdoc/>
    public sealed override II2cBus I2cBus { get; }

    /// <inheritdoc/>
    public sealed override ISpiBus SpiBus { get; }
    
    /// <inheritdoc/>
    public override IButton UpButton { get; }

    /// <inheritdoc/>
    public override IButton DownButton { get; }

    /// <inheritdoc/>
    public override IButton LeftButton { get; }

    /// <inheritdoc/>
    public override IButton RightButton { get; }

    /// <inheritdoc/>
    public override PiezoSpeaker? Speaker => GetSpeaker();

    /// <inheritdoc/>
    public override RgbPwmLed? RgbLed { get; }

    internal ProjectLabHardwareV2(IF7FeatherMeadowDevice device, II2cBus i2cBus, Mcp23008 mcp1)
    {
        _device = device;
        I2cBus = i2cBus;

        SpiBus = Resolver.Device.CreateSpiBus(
            device.Pins.SCK,
            device.Pins.COPI,
            device.Pins.CIPO,
            new Frequency(48000, Frequency.UnitType.Kilohertz));

        Mcp_1 = mcp1;
        IDigitalInterruptPort? mcp2_int = null;

        try
        {
            // MCP the Second
            if (device.Pins.D10.Supports<IDigitalChannelInfo>(c => c.InterruptCapable))
            {
                mcp2_int = device.CreateDigitalInterruptPort(
                    device.Pins.D10, InterruptMode.EdgeRising, ResistorMode.InternalPullDown);
            }

            Mcp_2 = new Mcp23008(I2cBus, address: 0x21, mcp2_int);

            Logger?.Info("Mcp_2 up");
        }
        catch (Exception e)
        {
            Logger?.Trace($"Failed to create MCP2: {e.Message}");
            mcp2_int?.Dispose();
        }

        try
        {
            Mcp_Version = new Mcp23008(I2cBus, address: 0x27);
            Logger?.Info("Mcp_Version up");
        }
        catch (Exception e)
        {
            Logger?.Trace($"ERR creating the MCP that has version information: {e.Message}");
        }

        //---- led
        RgbLed = new RgbPwmLed(
            redPwmPin: device.Pins.OnboardLedRed,
            greenPwmPin: device.Pins.OnboardLedGreen,
            bluePwmPin: device.Pins.OnboardLedBlue,
            CommonType.CommonAnode);

        //---- buttons
        Logger?.Trace("Instantiating buttons");
        var leftPort = mcp1.CreateDigitalInterruptPort(mcp1.Pins.GP2, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
        LeftButton = new PushButton(leftPort);
        var rightPort = mcp1.CreateDigitalInterruptPort(mcp1.Pins.GP1, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
        RightButton = new PushButton(rightPort);
        var upPort = mcp1.CreateDigitalInterruptPort(mcp1.Pins.GP0, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
        UpButton = new PushButton(upPort);
        var downPort = mcp1.CreateDigitalInterruptPort(mcp1.Pins.GP3, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
        DownButton = new PushButton(downPort);
        Logger?.Trace("Buttons up");
    }

    /// <inheritdoc/>
    protected override IGraphicsDisplay? GetDefaultDisplay()
    {
        if (_display == null)
        {
            Logger?.Trace("Instantiating display");

            var chipSelectPort = DisplayHeader.Pins.CS.CreateDigitalOutputPort();
            var dcPort = DisplayHeader.Pins.DC.CreateDigitalOutputPort();
            var resetPort = DisplayHeader.Pins.RST.CreateDigitalOutputPort();
            Thread.Sleep(50);

            _display = new St7789(
                spiBus: SpiBus,
                chipSelectPort: chipSelectPort,
                dataCommandPort: dcPort,
                resetPort: resetPort,
                width: 240, height: 240,
                colorMode: ColorMode.Format16bppRgb565)
            {
                SpiBusMode = SpiClockConfiguration.Mode.Mode3,
                SpiBusSpeed = new Frequency(48000, Frequency.UnitType.Kilohertz)
            };
            ((St7789)_display).SetRotation(RotationType._270Degrees);

            Logger?.Trace("Display up");
        }

        return _display;
    }

    private PiezoSpeaker? GetSpeaker()
    {
        if (_speaker == null)
        {
            try
            {
                Logger?.Trace("Instantiating speaker");
                _speaker = new PiezoSpeaker(_device.Pins.D11);
                Logger?.Trace("Speaker up");
            }
            catch (Exception ex)
            {
                Logger?.Error($"Unable to create the Piezo Speaker: {ex.Message}");
            }
        }

        return _speaker;
    }

    internal override MikroBusConnector CreateMikroBus1()
    {
        Logger?.Trace("Creating MikroBus1 connector");
        Debug.Assert(Mcp_2 != null, nameof(Mcp_2) + " != null");
        return new MikroBusConnector(
            "MikroBus1",
            new PinMapping
            {
                new PinMapping.PinAlias(MikroBusConnector.PinNames.AN, _device.Pins.A02),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.CS, Mcp_2.Pins.GP4),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.RST, Mcp_2.Pins.GP5),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SCK, _device.Pins.SCK),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.CIPO, _device.Pins.CIPO),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.COPI, _device.Pins.COPI),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.PWM, _device.Pins.D03),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.INT, Mcp_2.Pins.GP6),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.RX, _device.Pins.D13),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.TX, _device.Pins.D12),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SCL, _device.Pins.I2C_SCL),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SDA, _device.Pins.I2C_SDA),
            },
            _device.PlatformOS.GetSerialPortName("com1")!,
            new I2cBusMapping(_device, 1),
            new SpiBusMapping(_device, _device.Pins.SCK, _device.Pins.COPI, _device.Pins.CIPO)
            );
    }

    internal override MikroBusConnector CreateMikroBus2()
    {
        Logger?.Trace("Creating MikroBus2 connector");
        Debug.Assert(Mcp_2 != null, nameof(Mcp_2) + " != null");
        return new MikroBusConnector(
            "MikroBus2",
            new PinMapping
            {
                new PinMapping.PinAlias(MikroBusConnector.PinNames.AN, _device.Pins.A03),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.CS, Mcp_2.Pins.GP1),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.RST, Mcp_2.Pins.GP2),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SCK, _device.Pins.SCK),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.CIPO, _device.Pins.CIPO),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.COPI, _device.Pins.COPI),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.PWM, _device.Pins.D04),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.INT, Mcp_2.Pins.GP3),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.RX, _device.Pins.D13),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.TX, _device.Pins.D12),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SCL, _device.Pins.I2C_SCL),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SDA, _device.Pins.I2C_SDA),
            },
            _device.PlatformOS.GetSerialPortName("com1")!,
            new I2cBusMapping(_device, 1),
            new SpiBusMapping(_device, _device.Pins.SCK, _device.Pins.COPI, _device.Pins.CIPO)
            );
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
                new PinMapping.PinAlias(UartConnector.PinNames.RX, _device.Pins.D13),
                new PinMapping.PinAlias(UartConnector.PinNames.TX, _device.Pins.D12),
            },
            _device.PlatformOS.GetSerialPortName("com1")!);
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
                new PinMapping.PinAlias(IOTerminalConnector.PinNames.A1, _device.Pins.A04),
                new PinMapping.PinAlias(IOTerminalConnector.PinNames.D2, _device.Pins.D03),
                new PinMapping.PinAlias(IOTerminalConnector.PinNames.D3, _device.Pins.D04),
            });
    }

    internal override DisplayConnector CreateDisplayConnector()
    {
        Logger?.Trace("Creating display connector");

        return new DisplayConnector(
           nameof(Display),
            new PinMapping
            {
                new PinMapping.PinAlias(DisplayConnector.PinNames.CS, Mcp_1.Pins.GP5),
                new PinMapping.PinAlias(DisplayConnector.PinNames.RST, Mcp_1.Pins.GP7),
                new PinMapping.PinAlias(DisplayConnector.PinNames.DC, Mcp_1.Pins.GP6),
                new PinMapping.PinAlias(DisplayConnector.PinNames.CLK, _device.Pins.SCK),
                new PinMapping.PinAlias(DisplayConnector.PinNames.COPI, _device.Pins.COPI),
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
            return _revisionString ??= $"v2.{(Mcp_Version == null ? "x" : RevisionNumber)}";
        }
    }


    /// <inheritdoc/>
    public override ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
    {
        if (Resolver.Device is not F7FeatherBase device) throw new NotSupportedException();

        var portName = device.PlatformOS.GetSerialPortName("com4")!;
        var port = device.CreateSerialPort(portName, baudRate, dataBits, parity, stopBits);
        port.WriteTimeout = port.ReadTimeout = TimeSpan.FromSeconds(5);
        Debug.Assert(Mcp_2 != null, nameof(Mcp_2) + " != null");
        var serialEnable = Mcp_2.CreateDigitalOutputPort(Mcp_2.Pins.GP0, false);

        return new ProjectLabModbusRtuClient(port, serialEnable);
    }
}