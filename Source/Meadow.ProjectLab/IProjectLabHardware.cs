using Meadow.Foundation.Audio;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Accelerometers;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Light;
using Meadow.Hardware;
using Meadow.Modbus;
using Meadow.Peripherals.Sensors.Buttons;

namespace Meadow.Devices
{
    /// <summary>
    /// Interface for ProjectLab hardware
    /// </summary>
    public interface IProjectLabHardware
    {
        /// <summary>
        /// Gets the SPI bus.
        /// </summary>
        public ISpiBus SpiBus { get; }

        /// <summary>
        /// Gets the I2C bus.
        /// </summary>
        public II2cBus I2cBus { get; }

        /// <summary>
        /// Get a Modbus RTU client with optional parameters.
        /// </summary>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="dataBits">The number of data bits.</param>
        /// <param name="parity">The parity setting.</param>
        /// <param name="stopBits">The stop bits setting.</param>
        /// <returns>A Modbus RTU client.</returns>
        public ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One);

        /// <summary>
        /// Gets the graphics display.
        /// </summary>
        public IGraphicsDisplay? Display { get; }

        /// <summary>
        /// Gets the light sensor.
        /// </summary>
        public Bh1750? LightSensor { get; }

        /// <summary>
        /// Gets the environmental sensor.
        /// </summary>
        public Bme688? EnvironmentalSensor { get; }

        /// <summary>
        /// Gets the motion sensor.
        /// </summary>
        public Bmi270? MotionSensor { get; }

        /// <summary>
        /// Gets the piezo speaker.
        /// </summary>
        public PiezoSpeaker? Speaker { get; }

        /// <summary>
        /// Gets the RGB PWM LED.
        /// </summary>
        public RgbPwmLed? RgbLed { get; }

        /// <summary>
        /// Gets the left button.
        /// </summary>
        public IButton? LeftButton { get; }

        /// <summary>
        /// Gets the right button.
        /// </summary>
        public IButton? RightButton { get; }

        /// <summary>
        /// Gets the up button.
        /// </summary>
        public IButton? UpButton { get; }

        /// <summary>
        /// Gets the down button.
        /// </summary>
        public IButton? DownButton { get; }

        /// <summary>
        /// Gets the revision string.
        /// </summary>
        public string RevisionString { get; }

        /// <summary>
        /// Gets MikroBus connector 1.
        /// </summary>
        public MikroBusConnector MikroBus1 { get; }

        /// <summary>
        /// Gets MikroBus connector 2.
        /// </summary>
        public MikroBusConnector MikroBus2 { get; }

        /// <summary>
        /// Gets the Grove Digital connector.
        /// </summary>
        public GroveDigitalConnector? GroveDigital { get; }

        /// <summary>
        /// Gets the Grove Analog connector.
        /// </summary>
        public GroveDigitalConnector GroveAnalog { get; }

        /// <summary>
        /// Gets the Grove UART connector.
        /// </summary>
        public UartConnector GroveUart { get; }

        /// <summary>
        /// Gets the Qwiic connector.
        /// </summary>
        public I2cConnector Qwiic { get; }

        /// <summary>
        /// Gets the IO Terminal connector.
        /// </summary>
        public IOTerminalConnector IOTerminal { get; }

        /// <summary>
        /// Gets the display header connector.
        /// </summary>
        public DisplayConnector DisplayHeader { get; }
    }
}