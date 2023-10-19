using Meadow.Foundation.Audio;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Modbus;
using Meadow.Peripherals.Leds;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Units;
using System;

namespace Meadow.Devices;

/// <summary>
/// Represents Project Lab V2 hardware and exposes its peripherals
/// </summary>
public class ProjectLabHardwareV2 : ProjectLabHardwareBase
{
    private readonly IF7FeatherMeadowDevice _device;
    private PiezoSpeaker? _speaker;

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
    public override PiezoSpeaker? Speaker => GetSpeaker();

    /// <summary>
    /// Gets the Piezo noise maker on the Project Lab board
    /// </summary>
    public override RgbPwmLed? RgbLed { get; }

    internal ProjectLabHardwareV2(IF7FeatherMeadowDevice device, II2cBus i2cBus, Mcp23008 mcp1)
        : base(device)
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
                Resolver.Log.Error($"Unable to create the Piezo Speaker: {ex.Message}");
            }
        }

        return _speaker;
    }

    internal override MikroBusConnector CreateMikroBus1()
    {
        Logger?.Trace("Creating MikroBus1 connector");
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
            _device.PlatformOS.GetSerialPortName("com1"),
            new I2cBusMapping(_device, 1),
            new SpiBusMapping(_device, _device.Pins.SCK, _device.Pins.COPI, _device.Pins.CIPO)
            );
    }

    internal override MikroBusConnector CreateMikroBus2()
    {
        Logger?.Trace("Creating MikroBus2 connector");
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
            _device.PlatformOS.GetSerialPortName("com1"),
            new I2cBusMapping(_device, 1),
            new SpiBusMapping(_device, _device.Pins.SCK, _device.Pins.COPI, _device.Pins.CIPO)
            );
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
                new PinMapping.PinAlias(UartConnector.PinNames.RX, _device.Pins.D13),
                new PinMapping.PinAlias(UartConnector.PinNames.TX, _device.Pins.D12),
            },
            _device.PlatformOS.GetSerialPortName("com1"));
    }

    internal override I2cConnector CreateQwiicConnector()
    {
        Logger?.Trace("Creating Qwiic I2C connector");

        return new I2cConnector(
           "Qwiic",
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
           "IOTerminal",
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
           "Display",
            new PinMapping
            {
                new PinMapping.PinAlias(DisplayConnector.PinNames.CS, Mcp_1.Pins.GP5),
                new PinMapping.PinAlias(DisplayConnector.PinNames.RST, Mcp_1.Pins.GP7),
                new PinMapping.PinAlias(DisplayConnector.PinNames.DC, Mcp_1.Pins.GP6),
                new PinMapping.PinAlias(DisplayConnector.PinNames.CLK, _device.Pins.SCK),
                new PinMapping.PinAlias(DisplayConnector.PinNames.COPI, _device.Pins.COPI),
            });
    }

    /// <summary>
    /// The hardware revision of the board
    /// </summary>
    public override string RevisionString
    {
        get
        {
            if (revision == null)
            {
                if (Mcp_Version == null)
                {
                    revision = $"v2.x";
                }
                else
                {
                    byte rev = Mcp_Version.ReadFromPorts(Mcp23xxx.PortBank.A);
                    revision = $"v2.{rev}";
                }
            }
            return revision;
        }
    }

    private string? revision;

    /// <inheritdoc/>
    public override ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
    {
        if (Resolver.Device is F7FeatherBase device)
        {
            var portName = device.PlatformOS.GetSerialPortName("com4");
            var port = device.CreateSerialPort(portName, baudRate, dataBits, parity, stopBits);
            port.WriteTimeout = port.ReadTimeout = TimeSpan.FromSeconds(5);
            var serialEnable = Mcp_2?.CreateDigitalOutputPort(Mcp_2.Pins.GP0, false);

            return new ProjectLabModbusRtuClient(port, serialEnable);
        }

        throw new NotSupportedException();
    }
}