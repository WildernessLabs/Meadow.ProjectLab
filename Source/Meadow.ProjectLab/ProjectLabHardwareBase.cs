using Meadow.Foundation.Audio;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Accelerometers;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Light;
using Meadow.Hardware;
using Meadow.Logging;
using Meadow.Modbus;
using Meadow.Peripherals.Sensors.Buttons;
using System;

namespace Meadow.Devices
{
    /// <summary>
    /// Contains common elements of Project Lab Hardware
    /// </summary>
    public abstract class ProjectLabHardwareBase : IProjectLabHardware
    {
        private IConnector[]? _connectors;

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
        public Bh1750? LightSensor { get; private set; }

        /// <summary>
        /// Gets the BME688 environmental sensor  on the Project Lab board
        /// </summary>
        public Bme688? EnvironmentalSensor { get; private set; }

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
        public Bmi270? MotionSensor { get; private set; }

        /// <summary>
        /// Gets the display on the Project Lab board
        /// </summary>
        public abstract IGraphicsDisplay? Display { get; set; }

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
        /// Get the ProjectLab pins for mikroBUS header 1
        /// </summary>
        public abstract (IPin AN, IPin? RST, IPin CS, IPin SCK, IPin CIPO, IPin COPI, IPin PWM, IPin INT, IPin RX, IPin TX, IPin SCL, IPin SCA) MikroBus1Pins { get; protected set; }

        /// <summary>
        /// Get the ProjectLab pins for mikroBUS header 1
        /// </summary>
        public abstract (IPin AN, IPin? RST, IPin CS, IPin SCK, IPin CIPO, IPin COPI, IPin PWM, IPin INT, IPin RX, IPin TX, IPin SCL, IPin SCA) MikroBus2Pins { get; protected set; }


        public MikroBusConnector MikroBus1 => (MikroBusConnector)Connectors[0];
        public MikroBusConnector MikroBus2 => (MikroBusConnector)Connectors[1];

        /// <summary>
        /// Constructor the Project Lab Hardware base class
        /// </summary>
        /// <param name="device">The meadow device</param>
        internal ProjectLabHardwareBase(IF7MeadowDevice device)
        {
        }

        internal abstract MikroBusConnector CreateMikroBus1();
        internal abstract MikroBusConnector CreateMikroBus2();

        public IConnector[] Connectors
        {
            get
            {
                if (_connectors == null)
                {
                    _connectors = new IConnector[2]; // mikroe1, mikroe2, display (485?)
                    _connectors[0] = CreateMikroBus1();
                    _connectors[1] = CreateMikroBus2();
                }

                return _connectors;
            }
        }

        internal virtual void Initialize(IF7MeadowDevice device)
        {
            try
            {
                Logger?.Trace("Instantiating light sensor");
                LightSensor = new Bh1750(
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

            try
            {
                Logger?.Trace("Instantiating environmental sensor");
                EnvironmentalSensor = new Bme688(I2cBus, (byte)Bme688.Addresses.Address_0x76);
                Logger?.Trace("Environmental sensor up");
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Unable to create the BME688 Environmental Sensor: {ex.Message}");
            }

            try
            {
                Logger?.Trace("Instantiating motion sensor");
                MotionSensor = new Bmi270(I2cBus);
                Logger?.Trace("Motion sensor up");
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Unable to create the BMI270 IMU: {ex.Message}");
            }
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