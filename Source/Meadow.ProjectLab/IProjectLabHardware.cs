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
        /// The SPI bus
        /// </summary>
        public ISpiBus SpiBus { get; }

        /// <summary>
        /// The I2C bus
        /// </summary>
        public II2cBus I2cBus { get; }

        /// <summary>
        /// Get a Modbus RTU client
        /// </summary>
        public ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One);

        public IGraphicsDisplay? Display { get; protected set; }
        public Bh1750? LightSensor { get; }
        public Bme688? EnvironmentalSensor { get; }
        public Bmi270? MotionSensor { get; }
        public PiezoSpeaker? Speaker { get; }

        public RgbPwmLed? RgbLed { get; }

        public IButton? LeftButton { get; }
        public IButton? RightButton { get; }
        public IButton? UpButton { get; }
        public IButton? DownButton { get; }

        public string RevisionString { get; }

        MikroBusConnector MikroBus1 { get; }
        MikroBusConnector MikroBus2 { get; }

        GroveDigitalConnector? GroveDigital { get; }

        GroveDigitalConnector GroveAnalog { get; }

        UartConnector GroveUart { get; }

        I2cConnector QwiicConnector { get; }
    }
}