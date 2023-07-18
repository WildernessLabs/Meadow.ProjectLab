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
using System.Threading;

namespace Meadow.Devices;

/// <summary>
/// Represents Project Lab V2 hardware and exposes its peripherals
/// </summary>
public class ProjectLabHardwareV2 : ProjectLabHardwareBase
{
    private IF7FeatherMeadowDevice _device;

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
    /// Gets the ST7789 Display on the Project Lab board
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

    internal ProjectLabHardwareV2(IF7FeatherMeadowDevice device, II2cBus i2cBus, Mcp23008 mcp1)
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

        //---- instantiate display
        Logger?.Trace("Instantiating display");
        var chipSelectPort = mcp1.CreateDigitalOutputPort(mcp1.Pins.GP5);
        var dcPort = mcp1.CreateDigitalOutputPort(mcp1.Pins.GP6);
        var resetPort = mcp1.CreateDigitalOutputPort(mcp1.Pins.GP7);
        Thread.Sleep(50);

        Display = new St7789(
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
        ((St7789)Display).SetRotation(RotationType._270Degrees);

        Logger?.Trace("Display up");

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

        try
        {
            Logger?.Trace("Instantiating speaker");
            Speaker = new PiezoSpeaker(device.Pins.D11);
            Logger?.Trace("Speaker up");
        }
        catch (Exception ex)
        {
            Resolver.Log.Error($"Unable to create the Piezo Speaker: {ex.Message}");
        }

        //            SetMikroBusPins();
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