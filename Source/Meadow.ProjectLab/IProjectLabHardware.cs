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
        /// Gets the SPI bus on the Project Lab board.
        /// </summary>
        public ISpiBus SpiBus { get; }

        /// <summary>
        /// Gets the I2C bus on the Project Lab board.
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
        /// Gets the graphics display on the Project Lab board.
        /// </summary>
        public IGraphicsDisplay? Display { get; }

        /// <summary>
        /// Gets the light sensor on the Project Lab board.
        /// </summary>
        public Bh1750? LightSensor { get; }

        /// <summary>
        /// Gets the environmental sensor on the Project Lab board.
        /// </summary>
        public Bme688? EnvironmentalSensor { get; }

        /// <summary>
        /// Gets the motion sensor on the Project Lab board.
        /// </summary>
        public Bmi270? MotionSensor { get; }

        /// <summary>
        /// Gets the piezo speaker on the Project Lab board.
        /// </summary>
        public PiezoSpeaker? Speaker { get; }

        /// <summary>
        /// Gets the RGB PWM LED on the Project Lab board.
        /// </summary>
        public RgbPwmLed? RgbLed { get; }

        /// <summary>
        /// Gets the left button on the Project Lab board.
        /// </summary>
        public IButton? LeftButton { get; }

        /// <summary>
        /// Gets the right button on the Project Lab board.
        /// </summary>
        public IButton? RightButton { get; }

        /// <summary>
        /// Gets the up button on the Project Lab board.
        /// </summary>
        public IButton? UpButton { get; }

        /// <summary>
        /// Gets the down button on the Project Lab board.
        /// </summary>
        public IButton? DownButton { get; }

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
    }
}