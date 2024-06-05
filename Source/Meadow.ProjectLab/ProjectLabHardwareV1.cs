using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Modbus;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Leds;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Peripherals.Speakers;
using Meadow.Units;
using System;
using System.Threading;

namespace Meadow.Devices;

/// <summary>
/// Represents Project Lab V1 hardware and exposes its peripherals
/// </summary>
public class ProjectLabHardwareV1 : ProjectLabHardwareBase
{
    private readonly IF7FeatherMeadowDevice _device;
    private IToneGenerator? _speaker;
    private IRgbPwmLed? _rgbled;
    private IPixelDisplay? _display;

    private readonly string revision = "v1.x";

    /// <inheritdoc/>
    public override IButton UpButton { get; }

    /// <inheritdoc/>
    public override IButton DownButton { get; }

    /// <inheritdoc/>
    public override IButton LeftButton { get; }

    /// <inheritdoc/>
    public override IButton RightButton { get; }

    /// <inheritdoc/>
    public override IToneGenerator? Speaker => GetSpeaker();

    /// <inheritdoc/>
    public override IRgbPwmLed? RgbLed => GetRgbLed();

    internal ProjectLabHardwareV1(IF7FeatherMeadowDevice device, II2cBus i2cBus) : base(i2cBus)
    {
        _device = device;

        Logger?.Trace("Instantiating buttons");
        LeftButton = GetPushButton(device.Pins.D10);
        RightButton = GetPushButton(device.Pins.D05);
        UpButton = GetPushButton(device.Pins.D15);
        DownButton = GetPushButton(device.Pins.D02);
        Logger?.Trace("Buttons up");
    }

    /// <inheritdoc/>
    protected override IPixelDisplay? GetDefaultDisplay()
    {
        if (_display == null)
        {
            Logger?.Trace("Instantiating display");

            var chipSelectPort = DisplayHeader.Pins.DISPLAY_CS.CreateDigitalOutputPort();
            var dcPort = DisplayHeader.Pins.DISPLAY_DC.CreateDigitalOutputPort();
            var resetPort = DisplayHeader.Pins.DISPLAY_RST.CreateDigitalOutputPort();
            Thread.Sleep(50);

            _display = new St7789(
                spiBus: DisplayHeader.SpiBusDisplay,
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

    private IToneGenerator? GetSpeaker()
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

    private IRgbPwmLed? GetRgbLed()
    {
        if (_rgbled == null)
        {
            try
            {
                Logger?.Trace("Instantiating RGB LED");
                _rgbled = new RgbPwmLed(
                    redPwmPin: _device.Pins.OnboardLedRed,
                    greenPwmPin: _device.Pins.OnboardLedGreen,
                    bluePwmPin: _device.Pins.OnboardLedBlue,
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
        return new MikroBusConnector(
            "MikroBus1",
            new PinMapping
            {
                new PinMapping.PinAlias(MikroBusConnector.PinNames.AN, _device.Pins.A00),
                // no DISPLAY_RST connected
                new PinMapping.PinAlias(MikroBusConnector.PinNames.CS, _device.Pins.D14),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SCK, _device.Pins.SCK),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.CIPO, _device.Pins.CIPO),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.COPI, _device.Pins.COPI),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.PWM, _device.Pins.D04),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.INT, _device.Pins.D03),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.RX, _device.Pins.D12),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.TX, _device.Pins.D13),
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
        return new MikroBusConnector(
            "MikroBus2",
            new PinMapping
            {
                new PinMapping.PinAlias(MikroBusConnector.PinNames.AN, _device.Pins.A01),
                // no RST connected
                new PinMapping.PinAlias(MikroBusConnector.PinNames.CS, _device.Pins.A02),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SCK, _device.Pins.SCK),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.CIPO, _device.Pins.CIPO),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.COPI, _device.Pins.COPI),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.PWM, _device.Pins.D03),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.INT, _device.Pins.D04),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.RX, _device.Pins.D12),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.TX, _device.Pins.D13),
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
                new PinMapping.PinAlias(GroveDigitalConnector.PinNames.D0, _device.Pins.A01),
                new PinMapping.PinAlias(GroveDigitalConnector.PinNames.D1, _device.Pins.A02),
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
                new PinMapping.PinAlias(IOTerminalConnector.PinNames.A1, _device.Pins.A00),
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
                new PinMapping.PinAlias(DisplayConnector.PinNames.DISPLAY_CS, _device.Pins.A03),
                new PinMapping.PinAlias(DisplayConnector.PinNames.DISPLAY_RST, _device.Pins.A05),
                new PinMapping.PinAlias(DisplayConnector.PinNames.DISPLAY_DC, _device.Pins.A04),
                new PinMapping.PinAlias(DisplayConnector.PinNames.DISPLAY_CLK, _device.Pins.SCK),
                new PinMapping.PinAlias(DisplayConnector.PinNames.DISPLAY_COPI, _device.Pins.COPI),
            },
            new SpiBusMapping(_device, _device.Pins.SCK, _device.Pins.COPI, _device.Pins.CIPO));
    }

    /// <inheritdoc/>
    public override string RevisionString => revision;

    private IButton GetPushButton(IPin pin)
    {
        if (pin.Supports<IDigitalChannelInfo>(c => c.InterruptCapable))
        {
            return new PushButton(pin, ResistorMode.InternalPullDown);
        }
        else
        {
            return new PollingPushButton(pin, ResistorMode.InternalPullDown);
        }
    }

    /// <inheritdoc/>
    public override ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
    {
        if (Resolver.Device is not F7FeatherBase device) throw new NotSupportedException();
        var portName = device.PlatformOS.GetSerialPortName("com4")!;
        var port = device.CreateSerialPort(portName, baudRate, dataBits, parity, stopBits);
        port.WriteTimeout = port.ReadTimeout = TimeSpan.FromSeconds(5);
        var serialEnable = device.CreateDigitalOutputPort(device.Pins.D09, false);
        return new ProjectLabModbusRtuClient(port, serialEnable);
    }
}