using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Accelerometers;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Light;
using Meadow.Hardware;
using Meadow.Logging;
using Meadow.Modbus;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Units;
using System;
using System.Threading;

namespace Meadow.Devices
{
    /// <summary>
    /// Contains common elements of Project Lab Hardware
    /// </summary>
    public abstract class ProjectLabHardwareBase : IProjectLabHardware
    {
        private IConnector?[]? _connectors;
        private IGraphicsDisplay? _display;
        private Bh1750? _lightSensor;
        private Bme688? _environmentalSensor;
        private Bmi270? _motionSensor;

        /// <summary>
        /// Get a reference to Meadow Logger
        /// </summary>
        protected Logger? Logger { get; } = Resolver.Log;

        /// <summary>
        /// Gets the SPI Bus
        /// </summary>
        public ISpiBus SpiBus { get; protected set; }

        /// <summary>
        /// Gets the I2C Bus
        /// </summary>
        public II2cBus I2cBus { get; protected set; }

        /// <summary>
        /// Gets the BH1750 Light Sensor on the Project Lab board
        /// </summary>
        public Bh1750? LightSensor => GetLightSensor();

        /// <summary>
        /// Gets the BME688 environmental sensor  on the Project Lab board
        /// </summary>
        public Bme688? EnvironmentalSensor => GetEnvironmentalSensor();

        /// <summary>
        /// Gets the Piezo noise maker on the Project Lab board
        /// </summary>
        public abstract PiezoSpeaker? Speaker { get; }

        /// <summary>
        /// Gets the Piezo noise maker on the Project Lab board
        /// </summary>
        public abstract RgbPwmLed? RgbLed { get; }

        /// <summary>
        /// Gets the BMI inertial movement unit (IMU) on the Project Lab board
        /// </summary>
        public Bmi270? MotionSensor => GetMotionSensor();

        /// <summary>
        /// Gets the Ili9341 Display on the Project Lab board
        /// </summary>
        public IGraphicsDisplay? Display
        {
            get => GetIli9341Display();
            set => _display = value;
        }

        /// <summary>
        /// Gets the Up PushButton on the Project Lab board
        /// </summary>
        public abstract IButton? UpButton { get; }

        /// <summary>
        /// Gets the Down PushButton on the Project Lab board
        /// </summary>
        public abstract IButton? DownButton { get; }

        /// <summary>
        /// Gets the Left PushButton on the Project Lab board
        /// </summary>
        public abstract IButton? LeftButton { get; }
        /// <summary>
        /// Gets the Right PushButton on the Project Lab board
        /// </summary>
        public abstract IButton? RightButton { get; }

        /// <summary>
        /// Gets the ProjectLab board hardware revision
        /// </summary>
        public virtual string RevisionString { get; set; } = "unknown";

        /// <summary>
        /// Gets the MikroBus connector for slot 1 on the Project Lab board.
        /// </summary>
        public MikroBusConnector MikroBus1 => (MikroBusConnector)Connectors[0];

        /// <summary>
        /// Gets the MikroBus connector for slot 2 on the Project Lab board.
        /// </summary>
        public MikroBusConnector MikroBus2 => (MikroBusConnector)Connectors[1];

        /// <summary>
        /// Gets the Grove Digital connector on the Project Lab board.
        /// </summary>
        public GroveDigitalConnector? GroveDigital => (GroveDigitalConnector?)Connectors[2];

        /// <summary>
        /// Gets the Grove Analog connector on the Project Lab board.
        /// </summary>
        public GroveDigitalConnector GroveAnalog => (GroveDigitalConnector)Connectors[3];

        /// <summary>
        /// Gets the Grove UART connector on the Project Lab board.
        /// </summary>
        public UartConnector GroveUart => (UartConnector)Connectors[4];

        /// <summary>
        /// Gets the Qwiic connector on the Project Lab board.
        /// </summary>
        public I2cConnector Qwiic => (I2cConnector)Connectors[5];

        /// <summary>
        /// Gets the IO Terminal connector on the Project Lab board.
        /// </summary>
        public IOTerminalConnector IOTerminal => (IOTerminalConnector)Connectors[6];

        /// <summary>
        /// Gets the display header connector on the Project Lab board.
        /// </summary>
        public DisplayConnector DisplayHeader => (DisplayConnector)Connectors[7];


        /// <summary>
        /// Constructor the Project Lab Hardware base class
        /// </summary>
        /// <param name="device">The meadow device</param>
        internal ProjectLabHardwareBase(IF7MeadowDevice device)
        {
        }

        internal abstract MikroBusConnector CreateMikroBus1();
        internal abstract MikroBusConnector CreateMikroBus2();
        internal virtual GroveDigitalConnector? CreateGroveDigitalConnector()
        {
            return null;
        }

        internal abstract GroveDigitalConnector CreateGroveAnalogConnector();

        internal abstract UartConnector CreateGroveUartConnector();

        internal abstract I2cConnector CreateQwiicConnector();

        internal abstract IOTerminalConnector CreateIOTerminalConnector();

        internal abstract DisplayConnector CreateDisplayConnector();

        /// <summary>
        /// Collection of connectors on the Project Lab board
        /// </summary>
        public IConnector?[] Connectors
        {
            get
            {
                if (_connectors == null)
                {
                    _connectors = new IConnector[8];
                    _connectors[0] = CreateMikroBus1();
                    _connectors[1] = CreateMikroBus2();
                    _connectors[2] = CreateGroveDigitalConnector();
                    _connectors[3] = CreateGroveAnalogConnector();
                    _connectors[4] = CreateGroveUartConnector();
                    _connectors[5] = CreateQwiicConnector();
                    _connectors[6] = CreateIOTerminalConnector();
                    _connectors[7] = CreateDisplayConnector();
                }

                return _connectors;
            }
        }

        /// <summary>
        /// Creates the default ILI9341 display
        /// </summary>
        protected virtual IGraphicsDisplay? GetIli9341Display()
        {
            if (_display == null)
            {
                Logger?.Trace("Instantiating display");

                var chipSelectPort = DisplayHeader.Pins.CS.CreateDigitalOutputPort();
                var dcPort = DisplayHeader.Pins.DC.CreateDigitalOutputPort();
                var resetPort = DisplayHeader.Pins.RST.CreateDigitalOutputPort();

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
            }

            return _display;
        }

        private Bmi270? GetMotionSensor()
        {
            if (_motionSensor == null)
            {
                try
                {
                    Logger?.Trace("Instantiating motion sensor");
                    _motionSensor = new Bmi270(I2cBus);
                    Logger?.Trace("Motion sensor up");
                }
                catch (Exception ex)
                {
                    Resolver.Log.Error($"Unable to create the BMI270 IMU: {ex.Message}");
                }
            }

            return _motionSensor;
        }

        private Bh1750? GetLightSensor()
        {
            if (_lightSensor == null)
            {

                try
                {
                    Logger?.Trace("Instantiating light sensor");
                    _lightSensor = new Bh1750(
                        i2cBus: I2cBus,
                        measuringMode: Bh1750.MeasuringModes.ContinuouslyHighResolutionMode, // the various modes take differing amounts of time.
                        lightTransmittance: 0.5, // lower this to increase sensitivity, for instance, if it's behind a semi opaque window
                        address: (byte)Bh1750.Addresses.Address_0x23);
                    Logger?.Trace("Light sensor up");
                }
                catch (Exception ex)
                {
                    Resolver.Log.Error($"Unable to create the BH1750 Light Sensor: {ex.Message}");
                }
            }

            return _lightSensor;
        }

        private Bme688? GetEnvironmentalSensor()
        {
            if (_environmentalSensor == null)
            {
                try
                {
                    Logger?.Trace("Instantiating environmental sensor");
                    _environmentalSensor = new Bme688(I2cBus, (byte)Bme688.Addresses.Address_0x76);
                    Logger?.Trace("Environmental sensor up");
                }
                catch (Exception ex)
                {
                    Resolver.Log.Error($"Unable to create the BME688 Environmental Sensor: {ex.Message}");
                }
            }

            return _environmentalSensor;
        }

        /// <summary>
        /// Gets a ModbusRtuClient for the on-baord RS485 connector
        /// </summary>
        /// <param name="baudRate"></param>
        /// <param name="dataBits"></param>
        /// <param name="parity"></param>
        /// <param name="stopBits"></param>
        /// <returns></returns>
        public abstract ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One);

        /// <summary>
        /// Gets the pin definitions for the Project Lab board
        /// </summary>
        public static (
            IPin A0,
            IPin A1,
            IPin A2,
            IPin D03,
            IPin D04,
            IPin D12,
            IPin D13
            ) Pins = (
            Resolver.Device.GetPin("A00"),
            Resolver.Device.GetPin("A01"),
            Resolver.Device.GetPin("A02"),
            Resolver.Device.GetPin("D03"),
            Resolver.Device.GetPin("D04"),
            Resolver.Device.GetPin("D12"),
            Resolver.Device.GetPin("D13")
            );
    }
}