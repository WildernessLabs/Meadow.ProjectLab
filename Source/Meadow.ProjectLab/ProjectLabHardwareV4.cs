using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Hid;
using Meadow.Hardware;
using Meadow.Modbus;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Leds;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Peripherals.Speakers;
using Meadow.Units;
using System;
using System.Diagnostics;
using System.Threading;

namespace Meadow.Devices;

/// <summary>
/// Represents Project Lab V4 hardware and exposes its peripherals
/// </summary>
public class ProjectLabHardwareV4 : ProjectLabHardwareBase
{
    private readonly IF7CoreComputeMeadowDevice _device;
    private IToneGenerator? _speaker;
    private IRgbPwmLed? _rgbled;
    private IPixelDisplay? _display;
    private ITouchScreen? _touchscreen;

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

    private readonly Sc16is752 _uartExpander;

    /// <summary>
    /// Display enable port
    /// </summary>
    public IDigitalOutputPort? DisplayEnablePort { get; protected set; }

    /// <summary>
    /// Display backliight port for backlight control
    /// </summary>
    public IDigitalOutputPort? DisplayLedPort { get; protected set; }

    internal ProjectLabHardwareV4(IF7CoreComputeMeadowDevice device, II2cBus i2cBus) : base(i2cBus)
    {
        _device = device;

        IDigitalInterruptPort? mcp1Interrupt = null;
        IDigitalOutputPort? mcp1Reset = null;

        try
        {
            mcp1Interrupt = device.CreateDigitalInterruptPort(device.Pins.PC0, InterruptMode.EdgeRising);

            mcp1Reset = device.CreateDigitalOutputPort(device.Pins.PA10);

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
            mcp2Interrupt = device.CreateDigitalInterruptPort(device.Pins.PC8, InterruptMode.EdgeRising);

            Mcp_2 = new Mcp23008(i2cBus, address: 0x21, mcp2Interrupt);

            Logger?.Trace("Mcp_2 up");
        }
        catch (Exception e)
        {
            Logger?.Trace($"Failed to create MCP2: {e.Message}");
            mcp2Interrupt?.Dispose();
        }

        try
        {
            Mcp_Version = new Mcp23008(i2cBus, address: 0x27);
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

        _uartExpander = new Sc16is752(i2cBus, new Frequency(1.8432, Frequency.UnitType.Megahertz), Sc16is7x2.Addresses.Address_0x4D);
    }

    /// <inheritdoc/>
    protected override IPixelDisplay? GetDefaultDisplay()
    {
        DisplayEnablePort ??= Mcp_1?.CreateDigitalOutputPort(Mcp_1.Pins.GP4, false);
        DisplayEnablePort.State = false;
        DisplayLedPort ??= Mcp_1?.CreateDigitalOutputPort(DisplayHeader.Pins.DISPLAY_LED, true);
        DisplayLedPort.State = true; //Mcp23008 isn't setting state correctly on create 

        if (_display == null)
        {
            Logger?.Trace("Instantiating display");

            var chipSelectPort = DisplayHeader.Pins.DISPLAY_CS.CreateDigitalOutputPort();
            var dcPort = DisplayHeader.Pins.DISPLAY_DC.CreateDigitalOutputPort();
            var resetPort = DisplayHeader.Pins.DISPLAY_RST.CreateDigitalOutputPort();
            Thread.Sleep(50);

            var spiBus5 = _device.CreateSpiBus(
                _device.Pins.SPI5_SCK,
                _device.Pins.SPI5_COPI,
                _device.Pins.SPI5_CIPO,
                new Frequency(24000, Frequency.UnitType.Kilohertz));

            _display = new Ili9341(
                spiBus: spiBus5,
                chipSelectPort: chipSelectPort,
                dataCommandPort: dcPort,
                resetPort: resetPort,
                width: 240, height: 320,
                colorMode: ColorMode.Format12bppRgb444)
            {
                SpiBusMode = SpiClockConfiguration.Mode.Mode3,
                SpiBusSpeed = new Frequency(24000, Frequency.UnitType.Kilohertz)
            };

            ((Ili9341)_display).SetRotation(RotationType._270Degrees);
            ((Ili9341)_display).InvertDisplay(true);

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
                _speaker = new PiezoSpeaker(_device.Pins.PA0);
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
                    redPwmPin: _device.Pins.PC6,
                    greenPwmPin: _device.Pins.PC7,
                    bluePwmPin: _device.Pins.PC9,
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
        Debug.Assert(Mcp_2 != null, nameof(Mcp_2) + " != null");
        return new MikroBusConnector(
            "MikroBus1",
        new PinMapping
        {
                new PinMapping.PinAlias(MikroBusConnector.PinNames.AN, _device.Pins.PA3),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.RST, _device.Pins.PH10),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.CS, _device.Pins.PB12),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SCK, _device.Pins.SPI5_SCK),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.CIPO, _device.Pins.SPI5_CIPO),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.COPI, _device.Pins.SPI5_COPI),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.PWM, _device.Pins.PB8),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.INT, _device.Pins.PC2),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.RX, _device.Pins.PB15),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.TX, _device.Pins.PB14),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SCL, _device.Pins.I2C3_SCL),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SDA, _device.Pins.I2C3_SDA),
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
                new PinMapping.PinAlias(MikroBusConnector.PinNames.AN, _device.Pins.PB0),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.RST, Mcp_2.Pins.GP1),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.CS, Mcp_2.Pins.GP2),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SCK, _device.Pins.SPI3_SCK),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.CIPO, _device.Pins.SPI3_CIPO),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.COPI, _device.Pins.SPI3_COPI),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.PWM, _device.Pins.PB9),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.INT, Mcp_2.Pins.GP3),
//                new PinMapping.PinAlias(MikroBusConnector.PinNames.RX, uart1rx), // on the I2C uart and not usable for anything else
//                new PinMapping.PinAlias(MikroBusConnector.PinNames.TX, uart1tx), // on the I2C uart and not usable for anything else
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SCL, _device.Pins.I2C1_SCL),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SDA, _device.Pins.I2C1_SDA),
            },
            _device.PlatformOS.GetSerialPortName("com1")!,
            new I2cBusMapping(_device, 1),
            new SpiBusMapping(_device, _device.Pins.SCK, _device.Pins.COPI, _device.Pins.CIPO)
            );
    }

    internal override GroveDigitalConnector? CreateGroveDigitalConnector()
    {
        Logger?.Trace("Creating Grove digital connector");

        return new GroveDigitalConnector(
           nameof(GroveDigital),
            new PinMapping
            {
                new PinMapping.PinAlias(GroveDigitalConnector.PinNames.D0, _device.Pins.PB4),
                new PinMapping.PinAlias(GroveDigitalConnector.PinNames.D1, Mcp_2!.Pins.GP4),
            });
    }

    internal override GroveDigitalConnector CreateGroveAnalogConnector()
    {
        Logger?.Trace("Creating Grove analog connector");

        return new GroveDigitalConnector(
           nameof(GroveAnalog),
            new PinMapping
            {
                new PinMapping.PinAlias(GroveDigitalConnector.PinNames.D0, _device.Pins.PA4),
                new PinMapping.PinAlias(GroveDigitalConnector.PinNames.D1, _device.Pins.PA5),
            });
    }

    internal override UartConnector CreateGroveUartConnector()
    {
        Logger?.Trace("Creating Grove UART connector");

        return new UartConnector(
           nameof(GroveUart),
            new PinMapping
            {
                new PinMapping.PinAlias(UartConnector.PinNames.RX, _device.Pins.PI9),
                new PinMapping.PinAlias(UartConnector.PinNames.TX, _device.Pins.PH13),
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
                new PinMapping.PinAlias(I2cConnector.PinNames.SCL, _device.Pins.PB6),
                new PinMapping.PinAlias(I2cConnector.PinNames.SDA, _device.Pins.PB7),
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
                new PinMapping.PinAlias(DisplayConnector.PinNames.DISPLAY_CS, _device.Pins.PD5),
                new PinMapping.PinAlias(DisplayConnector.PinNames.DISPLAY_RST, _device.Pins.PB13),
                new PinMapping.PinAlias(DisplayConnector.PinNames.DISPLAY_DC, _device.Pins.PI11),
                new PinMapping.PinAlias(DisplayConnector.PinNames.DISPLAY_CLK, _device.Pins.SPI5_SCK),
                new PinMapping.PinAlias(DisplayConnector.PinNames.DISPLAY_COPI, _device.Pins.SPI5_COPI),
                new PinMapping.PinAlias(DisplayConnector.PinNames.DISPLAY_LED, Mcp_1!.Pins.GP5),
                new PinMapping.PinAlias(DisplayConnector.PinNames.TOUCH_INT, Mcp_2!.Pins.GP0),
                new PinMapping.PinAlias(DisplayConnector.PinNames.TOUCH_CS, Mcp_1.Pins.GP6),
                new PinMapping.PinAlias(DisplayConnector.PinNames.TOUCH_CLK, _device.Pins.SPI3_SCK),
                new PinMapping.PinAlias(DisplayConnector.PinNames.TOUCH_COPI, _device.Pins.SPI3_COPI),
                new PinMapping.PinAlias(DisplayConnector.PinNames.TOUCH_CIPO, _device.Pins.SPI3_CIPO),
            },
            new SpiBusMapping(_device, _device.Pins.SPI5_SCK, _device.Pins.SPI5_COPI, _device.Pins.SPI5_CIPO),
            new SpiBusMapping(_device, _device.Pins.SPI3_SCK, _device.Pins.SPI3_COPI, _device.Pins.SPI3_CIPO));
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
            return _revisionString ??= $"v4.{(Mcp_Version == null ? "x" : RevisionNumber)}";
        }
    }

    /// <inheritdoc/>
    public override ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
    {
        try
        {
            // v3.e+ uses an SC16is I2C UART expander for the RS485
            var port = _uartExpander.PortB.CreateRs485SerialPort(baudRate, dataBits, parity, stopBits, false);
            Resolver.Log.Trace($"485 port created");
            return new ModbusRtuClient(port);
        }
        catch (Exception ex)
        {
            Resolver.Log.Warn($"Error creating 485 port: {ex.Message}");
            throw new Exception("Unable to connect to UART expander");
        }
    }

    /// <inheritdoc/>
    public override ITouchScreen? Touchscreen
    {
        get
        {
            return _touchscreen ??= new Xpt2046(
                DisplayHeader.SpiBusDisplay,
                DisplayHeader.Pins.TOUCH_INT.CreateDigitalInterruptPort(InterruptMode.EdgeFalling, ResistorMode.Disabled),
                DisplayHeader.Pins.TOUCH_CS.CreateDigitalOutputPort(true),
                RotationType.Normal
                );
        }
    }
}