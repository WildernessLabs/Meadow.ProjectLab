using Meadow.Hardware;
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

namespace Meadow.Devices;

/// <summary>
/// Interface for ProjectLab hardware
/// </summary>
public interface IProjectLabHardware : IMeadowAppEmbeddedHardware
{
    /// <summary>
    /// Gets the up button on the Project Lab board.
    /// </summary>
    public IButton? UpButton { get; }

    /// <summary>
    /// Gets the down button on the Project Lab board.
    /// </summary>
    public IButton? DownButton { get; }

    /// <summary>
    /// Gets the left button on the Project Lab board.
    /// </summary>
    public IButton? LeftButton { get; }

    /// <summary>
    /// Gets the right button on the Project Lab board.
    /// </summary>
    public IButton? RightButton { get; }

    /// <summary>
    /// Gets the piezo speaker on the Project Lab board.
    /// </summary>
    public IToneGenerator? Speaker { get; }

    /// <summary>
    /// Gets the RGB PWM LED on the Project Lab board.
    /// </summary>
    public IRgbPwmLed? RgbLed { get; }

    /// <summary>
    /// Gets the light sensor on the Project Lab board.
    /// </summary>
    public ILightSensor? LightSensor { get; }

    /// <summary>
    /// Gets the ITemperatureSensor on the Project Lab board.
    /// </summary>
    public ITemperatureSensor? TemperatureSensor { get; }

    /// <summary>
    /// Gets the second/alternate ITemperatureSensor on the Project Lab board.
    /// </summary>
    public ITemperatureSensor? TemperatureSensor2 { get; }

    /// <summary>
    /// Gets the IHumiditySensor on the Project Lab board.
    /// </summary>
    public IHumiditySensor? HumiditySensor { get; }

    /// <summary>
    /// Gets the IBarometricPressureSensor on the Project Lab board.
    /// </summary>
    public IBarometricPressureSensor? BarometricPressureSensor { get; }

    /// <summary>
    /// Gets the IGasResistanceSensor on the Project Lab board.
    /// </summary>
    public IGasResistanceSensor? GasResistanceSensor { get; }

    /// <summary>
    /// Gets the IGyroscope on the Project Lab board
    /// </summary>
    public IGyroscope? Gyroscope { get; }

    /// <summary>
    /// Gets the IAccelerometer on the Project Lab board
    /// </summary>
    public IAccelerometer? Accelerometer { get; }

    /// <summary>
    /// Gets the graphics display on the Project Lab board.
    /// </summary>
    public IPixelDisplay? Display { get; }

    /// <summary>
    /// Gets the revision string of the Project Lab board.
    /// </summary>
    public string RevisionString { get; }

    /// <summary>
    /// Gets MikroBus connector 1 on the Project Lab board.
    /// </summary>
    public MikroBusConnector MikroBus1 { get; }

    /// <summary>
    /// Gets MikroBus connector 2 on the Project Lab board.
    /// </summary>
    public MikroBusConnector MikroBus2 { get; }

    /// <summary>
    /// Gets the Grove Digital connector on the Project Lab board.
    /// </summary>
    public GroveDigitalConnector? GroveDigital { get; }

    /// <summary>
    /// Gets the Grove Analog connector on the Project Lab board.
    /// </summary>
    public GroveDigitalConnector GroveAnalog { get; }

    /// <summary>
    /// Gets the Grove UART connector on the Project Lab board.
    /// </summary>
    public UartConnector GroveUart { get; }

    /// <summary>
    /// Gets the Qwiic connector on the Project Lab board.
    /// </summary>
    public I2cConnector Qwiic { get; }

    /// <summary>
    /// Gets the IO Terminal connector on the Project Lab board.
    /// </summary>
    public IOTerminalConnector IOTerminal { get; }

    /// <summary>
    /// Gets the display header connector on the Project Lab board.
    /// </summary>
    public DisplayConnector DisplayHeader { get; }

    /// <summary>
    /// Gets the touchscreen on the Project Lab display
    /// </summary>
    public ITouchScreen? Touchscreen { get; }

    /// <summary>
    /// Get a Modbus RTU client with optional parameters.
    /// </summary>
    /// <param name="baudRate">The baud rate.</param>
    /// <param name="dataBits">The number of data bits.</param>
    /// <param name="parity">The parity setting.</param>
    /// <param name="stopBits">The stop bits setting.</param>
    /// <returns>A Modbus RTU client.</returns>
    public ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One);
}