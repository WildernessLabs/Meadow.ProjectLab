using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Modbus;
using Meadow.Units;
using System;

namespace Meadow.Devices
{
    public class ProjectLabHardwareV1 : ProjectLabHardwareBase
    {
        private string revision = "v1.x";

        /// <summary>
        /// Gets the ST7789 Display on the Project Lab board
        /// </summary>
        public override IGraphicsDisplay? Display { get; set; }

        /// <summary>
        /// Gets the Up PushButton on the Project Lab board
        /// </summary>
        public override PushButton? UpButton { get; }

        /// <summary>
        /// Gets the Down PushButton on the Project Lab board
        /// </summary>
        public override PushButton? DownButton { get; }

        /// <summary>
        /// Gets the Left PushButton on the Project Lab board
        /// </summary>
        public override PushButton? LeftButton { get; }

        /// <summary>
        /// Gets the Right PushButton on the Project Lab board
        /// </summary>
        public override PushButton? RightButton { get; }

        /// <summary>
        /// Gets the Piezo noise maker on the Project Lab board
        /// </summary>
        public override PiezoSpeaker? Speaker { get; }

        /// <summary>
        /// Get the ProjectLab pins for mikroBUS header 1
        /// </summary>
        public override (IPin AN, IPin RST, IPin CS, IPin SCK, IPin CIPO, IPin COPI, IPin PWM, IPin INT, IPin RX, IPin TX, IPin SCL, IPin SCA) MikroBus1Pins { get; protected set; }

        /// <summary>
        /// Get the ProjectLab pins for mikroBUS header 2
        /// </summary>
        public override (IPin AN, IPin RST, IPin CS, IPin SCK, IPin CIPO, IPin COPI, IPin PWM, IPin INT, IPin RX, IPin TX, IPin SCL, IPin SCA) MikroBus2Pins { get; protected set; }

        internal ProjectLabHardwareV1(IF7FeatherMeadowDevice device, II2cBus i2cBus)
            : base(device)
        {
            I2cBus = i2cBus;

            base.Initialize(device);

            var config = new SpiClockConfiguration(
                new Frequency(48000, Frequency.UnitType.Kilohertz),
            SpiClockConfiguration.Mode.Mode3);

            SpiBus = Resolver.Device.CreateSpiBus(
                device.Pins.SCK,
                device.Pins.COPI,
                device.Pins.CIPO,
                config);

            Logger?.Debug("SPI Bus instantiated");

            Logger?.Trace("Instantiating display");
            Display = new St7789(
                        spiBus: SpiBus,
                        chipSelectPin: device.Pins.A03,
                        dcPin: device.Pins.A04,
                        resetPin: device.Pins.A05,
                        width: 240, height: 240,
                        colorMode: ColorMode.Format16bppRgb565);

            Logger?.Trace("Display up");

            //---- buttons
            Logger?.Trace("Instantiating buttons");
            LeftButton = GetPushButton(device, device.Pins.D10);
            RightButton = GetPushButton(device, device.Pins.D05);
            UpButton = GetPushButton(device, device.Pins.D15);
            DownButton = GetPushButton(device, device.Pins.D02);
            Logger?.Trace("Buttons up");

            try
            {
                Logger?.Trace("Instantiating speaker");
                Speaker = new PiezoSpeaker(device.Pins.D11);
                Logger?.Trace("Speaker up");
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Unable to create the Piezo Speaker: {ex.Message}");
            }

            SetMikroBusPins();
        }

        void SetMikroBusPins()
        {
            MikroBus1Pins =
                (Resolver.Device.GetPin("A00"),
                 null,
                 Resolver.Device.GetPin("D14"),
                 Resolver.Device.GetPin("SCK"),
                 Resolver.Device.GetPin("CIPO"),
                 Resolver.Device.GetPin("COPI"),
                 Resolver.Device.GetPin("D04"),
                 Resolver.Device.GetPin("D03"),
                 Resolver.Device.GetPin("D12"),
                 Resolver.Device.GetPin("D13"),
                 Resolver.Device.GetPin("D07"),
                 Resolver.Device.GetPin("D08"));

            MikroBus2Pins =
                (Resolver.Device.GetPin("A01"),
                 null,
                 Resolver.Device.GetPin("A02"),
                 Resolver.Device.GetPin("SCK"),
                 Resolver.Device.GetPin("CIPO"),
                 Resolver.Device.GetPin("COPI"),
                 Resolver.Device.GetPin("D03"),
                 Resolver.Device.GetPin("D04"),
                 Resolver.Device.GetPin("D12"),
                 Resolver.Device.GetPin("D13"),
                 Resolver.Device.GetPin("D07"),
                 Resolver.Device.GetPin("D08"));
        }

        public override string RevisionString => revision;

        private PushButton GetPushButton(IF7FeatherMeadowDevice device, IPin pin)
             => new PushButton(pin, ResistorMode.InternalPullDown);

        public override ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
        {
            if (Resolver.Device is F7FeatherBase device)
            {
                var portName = device.PlatformOS.GetSerialPortName("com4");
                var port = device.CreateSerialPort(portName, baudRate, dataBits, parity, stopBits);
                port.WriteTimeout = port.ReadTimeout = TimeSpan.FromSeconds(5);
                var serialEnable = device.CreateDigitalOutputPort(device.Pins.D09, false);
                return new ProjectLabModbusRtuClient(port, serialEnable);
            }

            throw new NotSupportedException();
        }
    }
}