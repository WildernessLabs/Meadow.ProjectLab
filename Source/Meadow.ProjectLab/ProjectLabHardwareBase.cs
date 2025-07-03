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
/// Contains common elements of Project Lab hardware
/// </summary>
public abstract class ProjectLabHardwareBase : IProjectLabHardware
{
    internal IConnector?[]? _connectors;
    internal IPixelDisplay? _display;
    internal ILightSensor? _lightSensor;
    internal Bmi270? _motionSensor;
    internal IGyroscope? _gyroscope;
    internal IAccelerometer? _accelerometer;
    internal ISamplingTemperatureSensor? _temperatureSensor;
    internal ISamplingTemperatureSensor? _temperatureSensor2;
    internal IHumiditySensor? _humiditySensor;
    internal IBarometricPressureSensor? _barometricPressureSensor;
    internal IGasResistanceSensor? _gasResistanceSensor;

    /// <summary>
    /// Get a reference to Meadow Logger
    /// </summary>
    protected Logger? Logger { get; } = Resolver.Log;

    /// <inheritdoc/>
    public abstract IButton? UpButton { get; }

    /// <inheritdoc/>
    public IMeadowDevice ComputeModule { get; }

    /// <inheritdoc/>
    public abstract IButton? DownButton { get; }

    /// <inheritdoc/>
    public abstract IButton? LeftButton { get; }

    /// <inheritdoc/>
    public abstract IButton? RightButton { get; }

    /// <inheritdoc/>
    public abstract IToneGenerator? Speaker { get; }

    /// <inheritdoc/>
    public abstract IRgbPwmLed? RgbLed { get; }

    /// <inheritdoc/>
    public virtual ITouchScreen? Touchscreen => null;

    /// <inheritdoc/>
    public ILightSensor? LightSensor => GetLightSensor();

    /// <inheritdoc/>
    public IGyroscope? Gyroscope => GetGyroscope();

    /// <inheritdoc/>
    public IAccelerometer? Accelerometer => GetAccelerometer();

    /// <inheritdoc/>
    public ISamplingTemperatureSensor? TemperatureSensor => GetTemperatureSensor();

    /// <inheritdoc/>
    public ISamplingTemperatureSensor? TemperatureSensor2 => GetTemperatureSensor2();

    /// <inheritdoc/>
    public IHumiditySensor? HumiditySensor => GetHumiditySensor();

    /// <inheritdoc/>
    public IBarometricPressureSensor? BarometricPressureSensor => GetBarometricPressureSensor();

    /// <inheritdoc/>
    public IGasResistanceSensor? GasResistanceSensor => GetGasResistanceSensor();

    /// <inheritdoc/>
    public IPixelDisplay? Display
    {
        get
        {
            _display ??= GetDefaultDisplay();
            return _display;
        }
        set => _display = value;
    }

    /// <summary>
    /// Gets the default display for the Project Lab board.
    /// </summary>
    protected abstract IPixelDisplay? GetDefaultDisplay();

    /// <inheritdoc/>
    public virtual string RevisionString { get; set; } = "unknown";

    /// <inheritdoc/>
    public MikroBusConnector MikroBus1 => (MikroBusConnector)Connectors[0]!;

    /// <inheritdoc/>
    public MikroBusConnector MikroBus2 => (MikroBusConnector)Connectors[1]!;

    /// <inheritdoc/>
    public GroveDigitalConnector? GroveDigital => (GroveDigitalConnector?)Connectors[2];

    /// <inheritdoc/>
    public GroveDigitalConnector GroveAnalog => (GroveDigitalConnector)Connectors[3]!;

    /// <inheritdoc/>
    public UartConnector GroveUart => (UartConnector)Connectors[4]!;

    /// <inheritdoc/>
    public I2cConnector Qwiic => (I2cConnector)Connectors[5]!;

    /// <inheritdoc/>
    public IOTerminalConnector IOTerminal => (IOTerminalConnector)Connectors[6]!;

    /// <inheritdoc/>
    public DisplayConnector DisplayHeader => (DisplayConnector)Connectors[7]!;

    /// <inheritdoc/>
    public Rs485Connector Rs485Connector => (Rs485Connector)Connectors[8]!;

    internal abstract MikroBusConnector CreateMikroBus1();
    internal abstract MikroBusConnector CreateMikroBus2();
    internal virtual GroveDigitalConnector? CreateGroveDigitalConnector()
    {
        return null;
    }

    internal abstract GroveDigitalConnector CreateGroveAnalogConnector();

    internal abstract UartConnector CreateGroveUartConnector();

    internal abstract Rs485Connector CreateRs485UartConnector();

    internal abstract I2cConnector CreateQwiicConnector();

    internal abstract IOTerminalConnector CreateIOTerminalConnector();

    internal abstract DisplayConnector CreateDisplayConnector();

    private readonly object _syncRoot = new object();

    /// <summary>
    /// Collection of connectors on the Project Lab board
    /// </summary>
    public IConnector?[] Connectors
    {
        get
        {
            lock (_syncRoot)
            {
                if (_connectors == null)
                {
                    _connectors = new IConnector[9];
                    _connectors[0] = CreateMikroBus1();
                    _connectors[1] = CreateMikroBus2();
                    _connectors[2] = CreateGroveDigitalConnector();
                    _connectors[3] = CreateGroveAnalogConnector();
                    _connectors[4] = CreateGroveUartConnector();
                    _connectors[5] = CreateQwiicConnector();
                    _connectors[6] = CreateIOTerminalConnector();
                    _connectors[7] = CreateDisplayConnector();
                    _connectors[8] = CreateRs485UartConnector();
                }

                return _connectors;
            }
        }
    }

    internal readonly II2cBus _peripheralI2cBus;

    internal ProjectLabHardwareBase(IMeadowDevice compute, II2cBus peripheralI2cBus)
    {
        ComputeModule = compute;

        _peripheralI2cBus = peripheralI2cBus;
    }

    private IAccelerometer? GetAccelerometer()
    {
        if (_accelerometer == null)
        {
            InitializeBmi270();
        }

        return _accelerometer;
    }

    private IGyroscope? GetGyroscope()
    {
        if (_gyroscope == null)
        {
            InitializeBmi270();
        }

        return _gyroscope;
    }

    private ISamplingTemperatureSensor? GetTemperatureSensor2()
    {
        if (_temperatureSensor2 == null)
        {
            InitializeBmi270();
        }

        return _temperatureSensor2;
    }

    internal virtual ISamplingTemperatureSensor? GetTemperatureSensor()
    {
        return null;
    }

    internal virtual IBarometricPressureSensor? GetBarometricPressureSensor()
    {
        return null;
    }

    internal virtual IHumiditySensor? GetHumiditySensor()
    {
        return null;
    }

    internal virtual IGasResistanceSensor? GetGasResistanceSensor()
    {
        return null;
    }

    private void InitializeBmi270()
    {
        try
        {
            Logger?.Trace("Instantiating motion sensor");
            var bmi = new Bmi270(_peripheralI2cBus);
            _motionSensor = bmi;
            _gyroscope = bmi;
            _accelerometer = bmi;
            // we use the BMI270 because, I believe, the 688 is closer to an on-board heat source and reads high
            _temperatureSensor2 = bmi;
            Resolver.SensorService.RegisterSensor(_motionSensor);
            Logger?.Trace("Motion sensor up");
        }
        catch (Exception ex)
        {
            Logger?.Error($"Unable to create the BMI270 IMU: {ex.Message}");
        }
    }

    private ILightSensor? GetLightSensor()
    {
        if (_lightSensor == null)
        {
            try
            {
                Logger?.Trace("Instantiating light sensor");
                _lightSensor = new Bh1750(
                    i2cBus: _peripheralI2cBus,
                    measuringMode: Bh1750.MeasuringModes.ContinuouslyHighResolutionMode, // the various modes take differing amounts of time.
                    lightTransmittance: 0.5, // lower this to increase sensitivity, for instance, if it's behind a semi opaque window
                    address: (byte)Bh1750.Addresses.Address_0x23);
                Resolver.SensorService.RegisterSensor(_lightSensor);
                Logger?.Trace("Light sensor up");
            }
            catch (Exception ex)
            {
                Logger?.Error($"Unable to create the BH1750 light sensor: {ex.Message}");
            }
        }

        return _lightSensor;
    }

    /// <summary>
    /// Gets a ModbusRtuClient for the on-board RS485 connector
    /// </summary>
    public abstract ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One);
}