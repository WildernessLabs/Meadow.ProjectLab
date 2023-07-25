using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Gateways.Bluetooth;
using Meadow.Hardware;
using Meadow.Modbus;
using Meadow.Peripherals.Leds;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Units;
using System;
using System.Threading;

namespace Meadow.Devices;

/// <summary>
/// Represents Project Lab V3 hardware and exposes its peripherals
/// </summary>
public class ProjectLabHardwareV3 : ProjectLabHardwareBase
{
    private string? revisionString;
    private byte? revisionNumber;
    private readonly IF7CoreComputeMeadowDevice _device;
    private readonly IConnectorProvider _connectors;

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
    private Mcp23008? Mcp_Version { get; set; }

    /// <summary>
    /// Gets the Ili9341 Display on the Project Lab board
    /// </summary>
    public override IGraphicsDisplay? Display { get; set; }

    /// <summary>
    /// Gets the Up PushButton on the Project Lab board
    /// </summary>
    public override IButton? UpButton { get; }

    /// <summary>
    /// Gets the Down PushButton on the Project Lab board
    /// </summary>
    public override IButton? DownButton { get; }

    /// <summary>
    /// Gets the Left PushButton on the Project Lab board
    /// </summary>
    public override IButton? LeftButton { get; }

    /// <summary>
    /// Gets the Right PushButton on the Project Lab board
    /// </summary>
    public override IButton? RightButton { get; }

    /// <summary>
    /// Gets the Piezo noise maker on the Project Lab board
    /// </summary>
    public override PiezoSpeaker? Speaker { get; }

    /// <summary>
    /// Gets the Piezo noise maker on the Project Lab board
    /// </summary>
    public override RgbPwmLed? RgbLed { get; }

    /// <summary>
    /// Display enable port for backlight control
    /// </summary>
    public IDigitalOutputPort DisplayEnablePort { get; protected set; }

    internal ProjectLabHardwareV3(IF7CoreComputeMeadowDevice device, II2cBus i2cBus)
        : base(device)
    {
        _device = device;

        I2cBus = i2cBus;

        base.Initialize(device);

        SpiBus = Resolver.Device.CreateSpiBus(
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

            Logger?.Info("Mcp_2 up");
        }
        catch (Exception e)
        {
            Logger?.Trace($"Failed to create MCP2: {e.Message}");
            mcp2Interrupt?.Dispose();
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

        //---- instantiate display
        Logger?.Trace("Instantiating display");

        DisplayEnablePort = Mcp_1?.CreateDigitalOutputPort(Mcp_1.Pins.GP4, true);

        var chipSelectPort = Mcp_1?.CreateDigitalOutputPort(Mcp_1.Pins.GP5);
        var dcPort = Mcp_1?.CreateDigitalOutputPort(Mcp_1.Pins.GP6);
        var resetPort = Mcp_1?.CreateDigitalOutputPort(Mcp_1.Pins.GP7);
        Thread.Sleep(50);

        Display = new Ili9341(
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

        ((Ili9341)Display).SetRotation(RotationType._270Degrees);

        Logger?.Trace("Display up");

        //---- led
        RgbLed = new RgbPwmLed(
            redPwmPin: device.Pins.D09,
            greenPwmPin: device.Pins.D10,
            bluePwmPin: device.Pins.D11,
            CommonType.CommonAnode);

        //---- buttons
        Logger?.Trace("Instantiating buttons");
        var leftPort = Mcp_1?.CreateDigitalInterruptPort(Mcp_1.Pins.GP2, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
        LeftButton = new PushButton(leftPort);
        var rightPort = Mcp_1?.CreateDigitalInterruptPort(Mcp_1.Pins.GP1, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
        RightButton = new PushButton(rightPort);
        var upPort = Mcp_1?.CreateDigitalInterruptPort(Mcp_1.Pins.GP0, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
        UpButton = new PushButton(upPort);
        var downPort = Mcp_1?.CreateDigitalInterruptPort(Mcp_1.Pins.GP3, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
        DownButton = new PushButton(downPort);
        Logger?.Trace("Buttons up");

        try
        {
            Logger?.Trace("Instantiating speaker");
            Speaker = new PiezoSpeaker(device.Pins.D20);
            Logger?.Trace("Speaker up");
        }
        catch (Exception ex)
        {
            Resolver.Log.Error($"Unable to create the Piezo Speaker: {ex.Message}");
        }

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

    internal override MikroBusConnector CreateMikroBus1()
    {
        Logger?.Trace("Creating MikroBus1 connector");
        return _connectors.CreateMikroBus1(_device, Mcp_2);
    }

    internal override MikroBusConnector CreateMikroBus2()
    {
        Logger?.Trace("Creating MikroBus2 connector");
        return _connectors.CreateMikroBus2(_device, Mcp_2);
    }

    internal override GroveDigitalConnector? CreateGroveDigitalConnector()
    {
        Logger?.Trace("Creating Grove digital connector");

        return new GroveDigitalConnector(
           "GroveDigital",
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
           "GroveAnalog",
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
           "GroveUart",
            new PinMapping
            {
                new PinMapping.PinAlias(UartConnector.PinNames.RX, _device.Pins.D00),
                new PinMapping.PinAlias(UartConnector.PinNames.TX, _device.Pins.D01),
            },
            _device.PlatformOS.GetSerialPortName("com4"));
    }

    internal override I2cConnector CreateQwiicConnector()
    {
        Logger?.Trace("Creating Grove analog connector");

        return new I2cConnector(
           "GroveQwiic",
            new PinMapping
            {
                new PinMapping.PinAlias(I2cConnector.PinNames.SCL, _device.Pins.D08),
                new PinMapping.PinAlias(I2cConnector.PinNames.SDA, _device.Pins.D07),
            },
            new I2cBusMapping(_device, 1));
    }

    /// <summary>
    /// The hardware revision number, read from the on-board MCP
    /// </summary>
    protected byte RevisionNumber
    {
        get
        {
            revisionNumber ??= Mcp_Version?.ReadFromPorts(Mcp23xxx.PortBank.A) ?? 0;

            return revisionNumber.Value;
        }
    }

    /// <inheritdoc/>
    public override string RevisionString
    {
        get
        {
            if (revisionString == null)
            {
                if (Mcp_Version == null)
                {
                    revisionString = $"v3.x";
                }
                else
                {
                    revisionString = $"v3.{RevisionNumber}";
                }
            }
            return revisionString;
        }
    }

    /// <inheritdoc/>
    public override ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
    {
        return _connectors.GetModbusRtuClient(this, baudRate, dataBits, parity, stopBits);
    }
}