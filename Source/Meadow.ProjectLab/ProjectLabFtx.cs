using Meadow.Foundation.Displays;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Light;
using Meadow.Foundation.Sensors.Motion;
using Meadow.Hardware;
using Meadow.Logging;
using Meadow.Modbus;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Leds;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Peripherals.Sensors.Environmental;
using Meadow.Peripherals.Sensors.Light;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Peripherals.Speakers;
using System;

namespace Meadow.Devices;

/// <summary>
/// Represents Project Lab FTx hardware based on the FT2232H dual-channel USB interface.
/// Channel A is used for GPIO (buttons), Channel B is used for I2C (sensors).
/// </summary>
public class ProjectLabFtx : IProjectLabHardware
{
    private class PinDefinitions
    {
        public PinDefinitions(FtdiExpander expander)
        {
            BTN1 = expander.Pins.D4; // BC_D4 == UP
            BTN2 = expander.Pins.D5; // BC_D5 == RIGHT
            BTN3 = expander.Pins.D7; // BC_D7 == DOWN
            BTN4 = expander.Pins.D6; // BC_D6 == LEFT
        }

        public IPin BTN1 { get; } // UP
        public IPin BTN2 { get; } // RIGHT
        public IPin BTN3 { get; } // DOWN
        public IPin BTN4 { get; } // LEFT
    }

    private readonly PinDefinitions? _pinDefs;
    private readonly FtdiExpander? _channelA;  // GPIO (buttons)
    private readonly FtdiExpander _channelB;   // I2C (sensors)
    private readonly Logger? _logger;

    private II2cBus? _i2c1;
    private bool _i2cInitialized;

    private IButton? _upButton;
    private IButton? _downButton;
    private IButton? _leftButton;
    private IButton? _rightButton;

    private Bh1750? _lightSensor;
    private bool _lightSensorInitialized;
    private Bme688? _bme688;
    private bool _bme688Initialized;
    private Bmi270? _bmi270;
    private bool _bmi270Initialized;

    // Display fields
    private IPixelDisplay? _display;
    private bool _displayInitialized;
    private ISpiBus? _spiBus;
    private IDigitalOutputPort? _displayCs;
    private IDigitalOutputPort? _displayDc;
    private IDigitalOutputPort? _displayRst;

    /// <summary>
    /// Creates a new ProjectLabFtx instance with both FT2232H channels.
    /// </summary>
    /// <param name="channelA">Channel A expander for GPIO (buttons). Can be null if not using buttons.</param>
    /// <param name="channelB">Channel B expander for I2C (sensors). Required.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    public ProjectLabFtx(FtdiExpander? channelA, FtdiExpander channelB, Logger? logger = null)
    {
        _channelA = channelA;
        _channelB = channelB ?? throw new ArgumentNullException(nameof(channelB), "Channel B (I2C) expander is required");
        _logger = logger;

        // Buttons are on Channel B GPIO pins (BC_D4, BC_D5, BC_D6, BC_D7)
        _pinDefs = new PinDefinitions(_channelB);
    }

    /// <summary>
    /// Creates a new ProjectLabFtx instance with a single expander (for backward compatibility).
    /// Uses the provided expander for I2C. GPIO/buttons will not be available.
    /// </summary>
    /// <param name="expander">The FT2232H channel expander to use for I2C.</param>
    [Obsolete("Use the constructor with two expanders for full functionality. This constructor only supports I2C sensors.")]
    public ProjectLabFtx(FtdiExpander expander)
        : this(null, expander)
    {
    }

    private II2cBus? I2cBus1
    {
        get
        {
            if (!_i2cInitialized)
            {
                _i2cInitialized = true;
                try
                {
                    _i2c1 = _channelB.CreateI2cBus(0);
                    _logger?.Debug("I2C bus initialized successfully");
                }
                catch (Exception ex)
                {
                    _logger?.Error($"Failed to initialize I2C bus: {ex.Message}");
                    _i2c1 = null;
                }
            }
            return _i2c1;
        }
    }

    private Bme688? Bme688
    {
        get
        {
            if (!_bme688Initialized)
            {
                _bme688Initialized = true;
                if (I2cBus1 != null)
                {
                    try
                    {
                        _bme688 = new Bme688(I2cBus1, 0x76);
                        _logger?.Debug("BME688 initialized at 0x76");
                    }
                    catch (Exception ex)
                    {
                        _logger?.Warn($"Failed to initialize BME688: {ex.Message}");
                        _bme688 = null;
                    }
                }
            }
            return _bme688;
        }
    }

    private Bmi270? Bmi270
    {
        get
        {
            if (!_bmi270Initialized)
            {
                _bmi270Initialized = true;
                if (I2cBus1 != null)
                {
                    try
                    {
                        _bmi270 = new Bmi270(I2cBus1, 0x68);
                        _logger?.Debug("BMI270 initialized at 0x68");
                    }
                    catch (Exception ex)
                    {
                        _logger?.Warn($"Failed to initialize BMI270: {ex.Message}");
                        _bmi270 = null;
                    }
                }
            }
            return _bmi270;
        }
    }

    /// <inheritdoc/>
    public IButton? UpButton
    {
        get
        {
            if (_upButton == null && _pinDefs != null)
            {
                try
                {
                    var button = new PollingPushButton(_pinDefs.BTN1.CreateDigitalInputPort(ResistorMode.ExternalPullUp));
                    button.ButtonPollingInterval = TimeSpan.FromMilliseconds(250); // Slower polling for stability
                    _upButton = button;
                }
                catch (Exception ex)
                {
                    _logger?.Warn($"Failed to initialize Up button: {ex.Message}");
                }
            }
            return _upButton;
        }
    }

    /// <inheritdoc/>
    public IButton? DownButton
    {
        get
        {
            if (_downButton == null && _pinDefs != null)
            {
                try
                {
                    var button = new PollingPushButton(_pinDefs.BTN3.CreateDigitalInputPort(ResistorMode.ExternalPullUp));
                    button.ButtonPollingInterval = TimeSpan.FromMilliseconds(250); // Slower polling for stability
                    _downButton = button;
                }
                catch (Exception ex)
                {
                    _logger?.Warn($"Failed to initialize Down button: {ex.Message}");
                }
            }
            return _downButton;
        }
    }

    /// <inheritdoc/>
    public IButton? LeftButton
    {
        get
        {
            if (_leftButton == null && _pinDefs != null)
            {
                try
                {
                    var button = new PollingPushButton(_pinDefs.BTN4.CreateDigitalInputPort(ResistorMode.ExternalPullUp));
                    button.ButtonPollingInterval = TimeSpan.FromMilliseconds(250); // Slower polling for stability
                    _leftButton = button;
                }
                catch (Exception ex)
                {
                    _logger?.Warn($"Failed to initialize Left button: {ex.Message}");
                }
            }
            return _leftButton;
        }
    }

    /// <inheritdoc/>
    public IButton? RightButton
    {
        get
        {
            if (_rightButton == null && _pinDefs != null)
            {
                try
                {
                    var button = new PollingPushButton(_pinDefs.BTN2.CreateDigitalInputPort(ResistorMode.ExternalPullUp));
                    button.ButtonPollingInterval = TimeSpan.FromMilliseconds(250); // Slower polling for stability
                    _rightButton = button;
                }
                catch (Exception ex)
                {
                    _logger?.Warn($"Failed to initialize Right button: {ex.Message}");
                }
            }
            return _rightButton;
        }
    }

    /// <inheritdoc/>
    public ILightSensor? LightSensor
    {
        get
        {
            if (!_lightSensorInitialized)
            {
                _lightSensorInitialized = true;
                if (I2cBus1 != null)
                {
                    try
                    {
                        _lightSensor = new Bh1750(I2cBus1, 0x23);
                        _logger?.Debug("BH1750 initialized at 0x23");
                    }
                    catch (Exception ex)
                    {
                        _logger?.Warn($"Failed to initialize BH1750 light sensor: {ex.Message}");
                        _lightSensor = null;
                    }
                }
            }
            return _lightSensor;
        }
    }

    /// <inheritdoc/>
    public ISamplingTemperatureSensor? TemperatureSensor => Bme688;
    /// <inheritdoc/>
    public IHumiditySensor? HumiditySensor => Bme688;
    /// <inheritdoc/>
    public IBarometricPressureSensor? BarometricPressureSensor => Bme688;
    /// <inheritdoc/>
    public IGasResistanceSensor? GasResistanceSensor => Bme688;
    /// <inheritdoc/>
    public ISamplingTemperatureSensor? TemperatureSensor2 => Bmi270;
    /// <inheritdoc/>
    public IGyroscope? Gyroscope => Bmi270;
    /// <inheritdoc/>
    public IAccelerometer? Accelerometer => Bmi270;

    // Not yet implemented for FTx version
    public IToneGenerator? Speaker => null;
    public IRgbPwmLed? RgbLed => null;
    
    /// <inheritdoc/>
    public IPixelDisplay? Display
    {
        get
        {
            if (!_displayInitialized)
            {
                _displayInitialized = true;
                var i2cBus = I2cBus1;
                Console.WriteLine($"[Display Init] Channel A: {(_channelA != null ? "OK" : "NULL")}, I2C Bus: {(i2cBus != null ? "OK" : "NULL")}");
                if (_channelA != null && i2cBus != null)
                {
                    try
                    {
                        Console.WriteLine("[Display Init] Starting display initialization...");
                        
                        // Create SPI bus on Channel A with Mode3 (like existing ProjectLab)
                        var spiConfig = new SpiClockConfiguration(
                            new Meadow.Units.Frequency(10, Meadow.Units.Frequency.UnitType.Megahertz),
                            SpiClockConfiguration.Mode.Mode3);
                        _spiBus = _channelA.CreateSpiBus(0, spiConfig);
                        Console.WriteLine("[Display Init] SPI bus created");
                        
                        // Display control pins based on schematic:
                        // SPI1_D3 -> DISPLAY_CS (D3 - SPI chip select)
                        // AC_D5 -> DISPLAY_DC (C5 - ACBUS pin 5, FTx pin 56)
                        _displayCs = _channelA.Pins.D3.CreateDigitalOutputPort(true);   // CS initially high (inactive)
                        Console.WriteLine("[Display Init] CS pin (D3) created");
                        _displayDc = _channelA.Pins.C5.CreateDigitalOutputPort(false);  // DC on ACBUS5
                        Console.WriteLine("[Display Init] DC pin (C5) created");
                        
                        // PWM_01 -> DISPLAY_RST and PWM_02 -> DISPLAY_LED via PCA9685
                        // All address pins (A0-A5) are tied to GND, so address is 0x40 (default)
                        Console.WriteLine("[Display Init] Creating PCA9685 PWM generator at 0x40...");
                        var pwmGenerator = new Pca9685(i2cBus, (byte)0x40);
                        Console.WriteLine("[Display Init] PCA9685 created, creating RST pin (LED1)...");
                        _displayRst = pwmGenerator.CreateDigitalOutputPort(pwmGenerator.Pins.LED1, true);  // RST initially high (PWM_01 = LED1)
                        Console.WriteLine("[Display Init] RST created, creating LED pin (LED2)...");
                        var displayLed = pwmGenerator.CreateDigitalOutputPort(pwmGenerator.Pins.LED2, true); // LED backlight ON (PWM_02 = LED2)
                        Console.WriteLine("[Display Init] PCA9685 pins created");
                        
                        // Perform hardware reset sequence
                        _displayRst.State = true;
                        System.Threading.Thread.Sleep(50);
                        _displayRst.State = false;
                        System.Threading.Thread.Sleep(50);
                        _displayRst.State = true;
                        System.Threading.Thread.Sleep(150);
                        Console.WriteLine("[Display Init] Reset sequence complete");
                        
                        // ILI9341 display - same configuration as ProjectLabHardwareV5
                        Console.WriteLine("[Display Init] Creating ILI9341 display...");
                        _display = new Ili9341(
                            spiBus: _spiBus,
                            chipSelectPort: _displayCs,
                            dataCommandPort: _displayDc,
                            resetPort: _displayRst,
                            width: 240, 
                            height: 320,
                            colorMode: ColorMode.Format12bppRgb444)
                        {
                            SpiBusMode = SpiClockConfiguration.Mode.Mode3,
                            SpiBusSpeed = new Meadow.Units.Frequency(12000, Meadow.Units.Frequency.UnitType.Kilohertz)  // V5 uses 12MHz
                        };
                        
                        // Apply rotation to match physical display orientation
                        ((Ili9341)_display).SetRotation(RotationType._270Degrees);
                        // V5 calls InvertDisplayColor(true) after SetRotation
                        ((Ili9341)_display).InvertDisplayColor(true);
                        
                        Console.WriteLine("[Display Init] ILI9341 display initialized successfully!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Display Init] FAILED: {ex.Message}");
                        Console.WriteLine($"[Display Init] Stack: {ex.StackTrace}");
                        _display = null;
                    }
                }
            }
            return _display;
        }
    }
    
    public string RevisionString => "FTx v1.a";
    public MikroBusConnector MikroBus1 => throw new NotImplementedException();
    public MikroBusConnector MikroBus2 => throw new NotImplementedException();
    public GroveDigitalConnector? GroveDigital => null;
    public GroveDigitalConnector GroveAnalog => throw new NotImplementedException();
    public UartConnector GroveUart => throw new NotImplementedException();
    public Rs485Connector Rs485Connector => throw new NotImplementedException();
    public I2cConnector Qwiic => throw new NotImplementedException();
    public IOTerminalConnector IOTerminal => throw new NotImplementedException();
    public DisplayConnector DisplayHeader => throw new NotImplementedException();
    public ITouchScreen? Touchscreen => null;

    public IMeadowDevice ComputeModule => throw new NotImplementedException();

    public ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
    {
        throw new NotImplementedException();
    }
}
