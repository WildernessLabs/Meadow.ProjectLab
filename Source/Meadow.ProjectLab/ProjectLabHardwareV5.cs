using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Modbus;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Leds;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Peripherals.Speakers;
using Meadow.Units;
using System;
using System.Diagnostics;
using System.Threading;

namespace Meadow.Devices;

/// <summary>
/// Represents Project Lab V5 hardware and exposes its peripherals
/// </summary>
public class ProjectLabHardwareV5 : ProjectLabHardwareBase
{
    private readonly IF7CoreComputeMeadowDevice _device;
    private IToneGenerator? _speaker;
    private IRgbPwmLed? _rgbled;
    private readonly ITouchScreen? _touchscreen;
    private ModbusRtuClient? _client;
    private ProjectLabRs485Connector? _rs485Connector;

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

    private readonly Sc16is752 _uartExpander_1;
    private readonly Sc16is752 _uartExpander_2;

    private readonly Pca9685 _pwmExpander;

    /// <summary>
    /// Display enable port
    /// </summary>
    public IDigitalOutputPort? DisplayEnablePort { get; protected set; }

    /// <summary>
    /// Display backliight port for backlight control
    /// </summary>
    public IDigitalOutputPort? DisplayLedPort { get; protected set; }

    internal ProjectLabHardwareV5(IF7CoreComputeMeadowDevice device, II2cBus i2cBus)
        : base(device, i2cBus)
    {
        _device = device;

        IDigitalInterruptPort? mcp1Interrupt = null;
        IDigitalOutputPort? mcp1Reset = null;

        _pwmExpander = new Pca9685(i2cBus, address: 0x70);
        Logger?.Trace("PWM expander up");
        _uartExpander_1 = new Sc16is752(i2cBus, new Frequency(1.8432, Frequency.UnitType.Megahertz), Sc16is7x2.Addresses.Address_0x4D);
        Logger?.Trace("UART1 expander up");
        _uartExpander_2 = new Sc16is752(i2cBus, new Frequency(1.8432, Frequency.UnitType.Megahertz), Sc16is7x2.Addresses.Address_0x4C);
        Logger?.Trace("UART2 expander up");

        try
        {
            mcp1Interrupt = device.CreateDigitalInterruptPort(device.Pins.PC0, InterruptMode.EdgeRising);

            mcp1Reset = device.CreateDigitalOutputPort(device.Pins.PH10);

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


    }

    /// <inheritdoc/>
    protected override IPixelDisplay? GetDefaultDisplay()
    {
        DisplayEnablePort ??= Mcp_1?.CreateDigitalOutputPort(Mcp_1.Pins.GP4, true);
        DisplayLedPort ??= Mcp_1?.CreateDigitalOutputPort(DisplayHeader.Pins.DISPLAY_LED, true);

        if (_display == null)
        {
            Logger?.Trace("Instantiating display");

            var chipSelectPort = DisplayHeader.Pins.DISPLAY_CS.CreateDigitalOutputPort();
            var dcPort = DisplayHeader.Pins.DISPLAY_DC.CreateDigitalOutputPort();
            var resetPort = DisplayHeader.Pins.DISPLAY_RST.CreateDigitalOutputPort();
            Thread.Sleep(50);

            _display = new Ili9341(
                spiBus: DisplayHeader.SpiBusDisplay,
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
            ((Ili9341)_display).InvertDisplayColor(true);

            Logger?.Trace("Display up");
        }

        return _display;
    }

    internal override ISamplingTemperatureSensor? GetTemperatureSensor()
    {
        if (_temperatureSensor == null)
        {
            InitializeAht10();
        }

        return _temperatureSensor;
    }

    internal override IHumiditySensor? GetHumiditySensor()
    {
        if (_humiditySensor == null)
        {
            InitializeAht10();
        }

        return _humiditySensor;
    }

    private void InitializeAht10()
    {
        try
        {
            Logger?.Trace("Instantiating atmospheric sensor");
            var aht = new Aht10(_peripheralI2cBus, (byte)Aht10.Addresses.Address_0x38);
            _humiditySensor = aht;
            _temperatureSensor = aht;
            Resolver.SensorService.RegisterSensor(aht);
            Logger?.Trace("Atmospheric sensor up");
        }
        catch (Exception ex)
        {
            Logger?.Error($"Unable to create the AHT10 atmospheric sensor: {ex.Message}");
        }
    }

    private IToneGenerator? GetSpeaker()
    {
        if (_speaker == null)
        {
            try
            {
                Logger?.Trace("Instantiating speaker");
                _speaker = new PiezoSpeaker(_device.Pins.PC9);
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
                    redPwmPin: _pwmExpander.Pins.LED2,
                    greenPwmPin: _pwmExpander.Pins.LED1,
                    bluePwmPin: _pwmExpander.Pins.LED0,
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
        Debug.Assert(_pwmExpander != null, nameof(_pwmExpander) + " != null");
        Debug.Assert(_uartExpander_2 != null, nameof(_uartExpander_2) + " != null");
        return new MikroBusConnector(
            "MikroBus1",
        new PinMapping
        {
                new PinMapping.PinAlias(MikroBusConnector.PinNames.AN, _device.Pins.PA3),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.RST, Mcp_2.Pins.GP4),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.CS, Mcp_2.Pins.GP5),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SCK, _device.Pins.SPI5_SCK),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.CIPO, _device.Pins.SPI5_CIPO),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.COPI, _device.Pins.SPI5_COPI),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.PWM, _pwmExpander.Pins.LED3),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.INT, Mcp_2.Pins.GP6),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SCL, _device.Pins.I2C3_SCL),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SDA, _device.Pins.I2C3_SDA),
            },
            _uartExpander_2.PortB,
            new I2cBusMapping(_device, 3),
            new SpiBusMapping(_device, _device.Pins.SPI5_SCK, _device.Pins.SPI5_COPI, _device.Pins.SPI5_CIPO)
            );
    }

    internal override MikroBusConnector CreateMikroBus2()
    {
        Logger?.Trace("Creating MikroBus2 connector");
        Debug.Assert(Mcp_2 != null, nameof(Mcp_2) + " != null");
        Debug.Assert(_pwmExpander != null, nameof(_pwmExpander) + " != null");
        Debug.Assert(_uartExpander_1 != null, nameof(_uartExpander_1) + " != null");
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
                new PinMapping.PinAlias(MikroBusConnector.PinNames.PWM, _pwmExpander.Pins.LED4),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.INT, Mcp_2.Pins.GP3),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SCL, _device.Pins.I2C3_SCL),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SDA, _device.Pins.I2C3_SDA),
            },
            _uartExpander_2.PortA,
            new I2cBusMapping(_device, 3),
            new SpiBusMapping(_device, _device.Pins.SPI3_SCK, _device.Pins.SPI3_COPI, _device.Pins.SPI3_CIPO)
            );
    }

    internal override GroveDigitalConnector? CreateGroveDigitalConnector()
    {
        Logger?.Trace("Creating Grove digital connector");
        Debug.Assert(_pwmExpander != null, nameof(_pwmExpander) + " != null");

        return new GroveDigitalConnector(
           nameof(GroveDigital),
            new PinMapping
            {
                new PinMapping.PinAlias(GroveDigitalConnector.PinNames.D0, _pwmExpander.Pins.LED5),
                new PinMapping.PinAlias(GroveDigitalConnector.PinNames.D1, _pwmExpander.Pins.LED6),
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
        Debug.Assert(_uartExpander_2 != null, nameof(_uartExpander_2) + " != null");

        return new UartConnector(
           nameof(GroveUart),
            new PinMapping
            { },
            _uartExpander_2.PortA
        );
    }

    internal override I2cConnector CreateQwiicConnector()
    {
        Logger?.Trace("Creating Qwiic I2C connector");

        return new I2cConnector(
           nameof(Qwiic),
            new PinMapping
            {
                new PinMapping.PinAlias(I2cConnector.PinNames.SCL, _device.Pins.PH7),
                new PinMapping.PinAlias(I2cConnector.PinNames.SDA, _device.Pins.PH8),
            },
            new I2cBusMapping(_device, 3));
    }

    internal override IOTerminalConnector CreateIOTerminalConnector()
    {
        Logger?.Trace("Creating IO terminal connector");

        return new IOTerminalConnector(
           nameof(IOTerminal),
            new PinMapping
            {
                new PinMapping.PinAlias(IOTerminalConnector.PinNames.A1, _device.Pins.PB1),
                new PinMapping.PinAlias(IOTerminalConnector.PinNames.D2, _device.Pins.PC6),
                new PinMapping.PinAlias(IOTerminalConnector.PinNames.D3, _device.Pins.PC7),
            });
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
            return _revisionString ??= $"v5.{(Mcp_Version == null ? "x" : RevisionNumber)}";
        }
    }

    internal override Rs485Connector CreateRs485UartConnector()
    {
        if (_rs485Connector == null)
        {
            _rs485Connector = new ProjectLabRs485Connector(_uartExpander_1, _uartExpander_1.PortB);
        }
        return _rs485Connector;
    }

    /// <inheritdoc/>
    public override ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
    {
        if (_client == null)
        {
            try
            {
                Resolver.Log.Info($"Creating 485 port...", Constants.LogGroup);

                var port = _uartExpander_1.PortB.CreateRs485SerialPort(baudRate, dataBits, parity, stopBits, false);
                Resolver.Log.Trace($"485 port created", Constants.LogGroup);
                _client = new ModbusRtuClient(port);
            }
            catch (Exception ex)
            {
                Resolver.Log.Warn($"Error creating 485 port: {ex.Message}", Constants.LogGroup);
                throw new Exception("Unable to connect to UART expander");
            }
        }
        return _client;
    }

    /// <inheritdoc/>
    public override ITouchScreen? Touchscreen
    {
        get
        {
            return null;
        }
    }
}