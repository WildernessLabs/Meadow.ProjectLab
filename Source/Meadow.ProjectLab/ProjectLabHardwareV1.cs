﻿using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Modbus;
using Meadow.Peripherals.Leds;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Units;
using System;

namespace Meadow.Devices
{
    /// <summary>
    /// Represents Project Lab V1 hardware and exposes its peripherals
    /// </summary>
    public class ProjectLabHardwareV1 : ProjectLabHardwareBase
    {
        private IF7FeatherMeadowDevice _device;

        private readonly string revision = "v1.x";

        /// <summary>
        /// Gets the ST7789 Display on the Project Lab board
        /// </summary>
        public override IGraphicsDisplay? Display { get; set; }

        /// <summary>
        /// Gets the Up PushButton on the Project Lab board
        /// </summary>
        public override IButton? UpButton { get; }

        /// <summary>
        /// Gets the Down PushButton on the Project Lab board
        /// </summary>
        public override IButton? DownButton { get; }

        /// <summary>
        /// Gets the Left PushButton on the Project Lab board
        /// </summary>
        public override IButton? LeftButton { get; }

        /// <summary>
        /// Gets the Right PushButton on the Project Lab board
        /// </summary>
        public override IButton? RightButton { get; }

        /// <summary>
        /// Gets the Piezo noise maker on the Project Lab board
        /// </summary>
        public override PiezoSpeaker? Speaker { get; }

        /// <summary>
        /// Gets the Piezo noise maker on the Project Lab board
        /// </summary>
        public override RgbPwmLed? RgbLed { get; }

        /// <summary>
        /// Get the ProjectLab pins for mikroBUS header 1
        /// </summary>
        public override (IPin AN, IPin? RST, IPin CS, IPin SCK, IPin CIPO, IPin COPI, IPin PWM, IPin INT, IPin RX, IPin TX, IPin SCL, IPin SCA) MikroBus1Pins { get; protected set; }

        /// <summary>
        /// Get the ProjectLab pins for mikroBUS header 2
        /// </summary>
        public override (IPin AN, IPin? RST, IPin CS, IPin SCK, IPin CIPO, IPin COPI, IPin PWM, IPin INT, IPin RX, IPin TX, IPin SCL, IPin SCA) MikroBus2Pins { get; protected set; }

        internal ProjectLabHardwareV1(IF7FeatherMeadowDevice device, II2cBus i2cBus)
            : base(device)
        {
            _device = device;
            I2cBus = i2cBus;

            base.Initialize(device);

            SpiBus = Resolver.Device.CreateSpiBus(
                device.Pins.SCK,
                device.Pins.COPI,
                device.Pins.CIPO,
                new Frequency(48000, Frequency.UnitType.Kilohertz));

            Logger?.Debug("SPI Bus instantiated");

            Logger?.Trace("Instantiating display");
            Display = new St7789(
                        spiBus: SpiBus,
                        chipSelectPin: device.Pins.A03,
                        dcPin: device.Pins.A04,
                        resetPin: device.Pins.A05,
                        width: 240, height: 240,
                        colorMode: ColorMode.Format16bppRgb565)
            {
                SpiBusMode = SpiClockConfiguration.Mode.Mode3,
                SpiBusSpeed = new Frequency(48000, Frequency.UnitType.Kilohertz)
            };
            ((St7789)Display).SetRotation(RotationType._270Degrees);

            Logger?.Trace("Display up");

            //---- led
            RgbLed = new RgbPwmLed(
                redPwmPin: device.Pins.OnboardLedRed,
                greenPwmPin: device.Pins.OnboardLedGreen,
                bluePwmPin: device.Pins.OnboardLedBlue,
                CommonType.CommonAnode);

            //---- buttons
            Logger?.Trace("Instantiating buttons");
            LeftButton = GetPushButton(device.Pins.D10);
            RightButton = GetPushButton(device.Pins.D05);
            UpButton = GetPushButton(device.Pins.D15);
            DownButton = GetPushButton(device.Pins.D02);
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

        internal override MikroBusConnector CreateMikroBus1()
        {
            Logger?.Trace("Creating MikroBus1 connector");
            return new MikroBusConnector(
                "MikroBus1",
                new PinMapping
                {
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.AN, _device.Pins.A00),
                    // no RST connected
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.CS, _device.Pins.D14),
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.SCK, _device.Pins.SCK),
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.CIPO, _device.Pins.CIPO),
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.COPI, _device.Pins.COPI),
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.PWM, _device.Pins.D04),
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.INT, _device.Pins.D03),
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.RX, _device.Pins.D12),
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.TX, _device.Pins.D13),
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.SCL, _device.Pins.I2C_SCL),
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.SDA, _device.Pins.I2C_SDA),
                });
        }

        internal override MikroBusConnector CreateMikroBus2()
        {
            Logger?.Trace("Creating MikroBus2 connector");
            return new MikroBusConnector(
                "MikroBus2",
                new PinMapping
                {
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.AN, _device.Pins.A01),
                    // no RST connected
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.CS, _device.Pins.A02),
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.SCK, _device.Pins.SCK),
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.CIPO, _device.Pins.CIPO),
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.COPI, _device.Pins.COPI),
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.PWM, _device.Pins.D03),
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.INT, _device.Pins.D04),
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.RX, _device.Pins.D12),
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.TX, _device.Pins.D13),
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.SCL, _device.Pins.I2C_SCL),
                    new PinMapping.PinAlias(MikroBusConnector.PinNames.SDA, _device.Pins.I2C_SDA),
                });
        }

        private void SetMikroBusPins()
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
                 Resolver.Device.GetPin("D08"),
                 Resolver.Device.GetPin("D07"));

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
                 Resolver.Device.GetPin("D08"),
                 Resolver.Device.GetPin("D07"));
        }

        /// <summary>
        /// The hardware revision string
        /// </summary>
        public override string RevisionString => revision;

        private IButton GetPushButton(IPin pin)
        {
            if (pin.Supports<IDigitalChannelInfo>(c => c.InterruptCapable))
            {
                return new PushButton(pin, ResistorMode.InternalPullDown);
            }
            else
            {
                return new PollingPushButton(pin, ResistorMode.InternalPullDown);
            }
        }

        /// <summary>
        /// Get the GetModbus Rtu Client
        /// </summary>
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