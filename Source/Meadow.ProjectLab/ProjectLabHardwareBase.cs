using Meadow.Foundation.Sensors.Atmospheric;
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

namespace Meadow.Devices
{
    /// <summary>
    /// Contains common elements of Project Lab hardware
    /// </summary>
    public abstract class ProjectLabHardwareBase : IProjectLabHardware
    {
        private IConnector?[]? _connectors;
        private IPixelDisplay? _display;
        private ILightSensor? _lightSensor;
        private Bme688? _atmosphericSensor;
        private Bmi270? _motionSensor;
        private IGyroscope? _gyroscope;
        private IAccelerometer? _accelerometer;
        private ITemperatureSensor? _temperatureSensor;
        private IHumiditySensor? _humiditySensor;
        private IBarometricPressureSensor? _barometricPressureSensor;
        private IGasResistanceSensor? _gasResistanceSensor;

        /// <summary>
        /// Get a reference to Meadow Logger
        /// </summary>
        protected Logger? Logger { get; } = Resolver.Log;

        /// <inheritdoc/>
        public abstract ISpiBus SpiBus { get; }

        /// <inheritdoc/>
        public abstract II2cBus I2cBus { get; }

        /// <inheritdoc/>
        public abstract IButton? UpButton { get; }

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
        public Bme688? AtmosphericSensor => GetAtmosphericSensor();

        /// <inheritdoc/>
        public Bmi270? MotionSensor => GetMotionSensor();

        /// <inheritdoc/>
        public IGyroscope? Gyroscope => GetGyroscope();

        /// <inheritdoc/>
        public IAccelerometer? Accelerometer => GetAccelerometer();

        /// <inheritdoc/>
        public ITemperatureSensor? TemperatureSensor => GetTemperatureSensor();

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

        private ITemperatureSensor? GetTemperatureSensor()
        {
            if (_temperatureSensor == null)
            {
                InitializeBmi270();
            }

            return _temperatureSensor;
        }

        private void InitializeBmi270()
        {
            try
            {
                Logger?.Trace("Instantiating motion sensor");
                var bmi = new Bmi270(I2cBus);
                _motionSensor = bmi;
                _gyroscope = bmi;
                _accelerometer = bmi;
                // we use the BMI270 because, I believe, the 688 is closer to an on-board heat source and reads high
                _temperatureSensor = bmi;
                Resolver.SensorService.RegisterSensor(_motionSensor);
                Logger?.Trace("Motion sensor up");
            }
            catch (Exception ex)
            {
                Logger?.Error($"Unable to create the BMI270 IMU: {ex.Message}");
            }
        }

        private Bmi270? GetMotionSensor()
        {
            if (_motionSensor == null)
            {
                InitializeBmi270();
            }

            return _motionSensor;
        }

        private ILightSensor? GetLightSensor()
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

        private Bme688? GetAtmosphericSensor()
        {
            if (_atmosphericSensor == null)
            {
                InitializeBme688();
            }

            return _atmosphericSensor;
        }

        private IHumiditySensor? GetHumiditySensor()
        {
            if (_humiditySensor == null)
            {
                InitializeBme688();
            }

            return _humiditySensor;
        }

        private IBarometricPressureSensor? GetBarometricPressureSensor()
        {
            if (_barometricPressureSensor == null)
            {
                InitializeBme688();
            }

            return _barometricPressureSensor;
        }

        private IGasResistanceSensor? GetGasResistanceSensor()
        {
            if (_gasResistanceSensor == null)
            {
                InitializeBme688();
            }

            return _gasResistanceSensor;
        }

        private void InitializeBme688()
        {
            try
            {
                Logger?.Trace("Instantiating atmospheric sensor");
                var bme = new Bme688(I2cBus, (byte)Bme68x.Addresses.Address_0x76);
                _atmosphericSensor = bme;
                _humiditySensor = bme;
                _barometricPressureSensor = bme;
                _gasResistanceSensor = bme;
                Resolver.SensorService.RegisterSensor(bme);
                Logger?.Trace("Atmospheric sensor up");
            }
            catch (Exception ex)
            {
                Logger?.Error($"Unable to create the BME688 atmospheric sensor: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a ModbusRtuClient for the on-board RS485 connector
        /// </summary>
        /// <param name="baudRate"></param>
        /// <param name="dataBits"></param>
        /// <param name="parity"></param>
        /// <param name="stopBits"></param>
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