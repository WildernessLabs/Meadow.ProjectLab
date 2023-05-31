using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Modbus;
using Meadow.Peripherals.Leds;
using Meadow.Units;
using System;
using System.Threading;

namespace Meadow.Devices
{
    /// <summary>
    /// Represents Project Lab V3 hardware and exposes its peripherals
    /// </summary>
    public class ProjectLabHardwareV3 : ProjectLabHardwareBase
    {
        /// <summary>
        /// The MCP23008 IO expander connected to internal peripherals
        /// </summary>
        public Mcp23008 Mcp_1 { get; protected set; }

        /// <summary>
        /// The MCP23008 IO expander connected to IO headers and terminals on Project Lab
        /// </summary>
        public Mcp23008? Mcp_2 { get; protected set; }

        /// <summary>
        /// The MCP23008 IO expander that contains the ProjectLab hardware version 
        /// </summary>
        private Mcp23008? Mcp_Version { get; set; }

        /// <summary>
        /// Gets the Ili9341 Display on the Project Lab board
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

        /// <summary>
        /// Display enable port for backlight control
        /// </summary>
        public IDigitalOutputPort DisplayEnablePort { get; protected set; }

        internal ProjectLabHardwareV3(IF7CoreComputeMeadowDevice device, II2cBus i2cBus)
            : base(device)
        {
            I2cBus = i2cBus;

            base.Initialize(device);

            SpiBus = Resolver.Device.CreateSpiBus(
                device.Pins.SCK,
                device.Pins.COPI,
                device.Pins.CIPO,
                new Frequency(48000, Frequency.UnitType.Kilohertz));

            IDigitalInterruptPort? mcp1Interrupt = null;
            IDigitalOutputPort? mcp1Reset = null;

            try
            {
                // MCP the First
                mcp1Interrupt = device.CreateDigitalInterruptPort(device.Pins.A05, InterruptMode.EdgeRising, ResistorMode.InternalPullDown);

                mcp1Reset = device.CreateDigitalOutputPort(device.Pins.D05);

                Mcp_1 = new Mcp23008(i2cBus, address: 0x20, mcp1Interrupt, mcp1Reset);

                Logger?.Trace("Mcp_1 up");
            }
            catch (Exception e)
            {
                Logger?.Trace($"Failed to create MCP1: {e.Message}");
                mcp1Interrupt?.Dispose();
            }

            IDigitalInterruptPort? mcp2Interrupt = null;

            try
            {
                // MCP the Second
                if (device.Pins.D19.Supports<IDigitalChannelInfo>(c => c.InterruptCapable))
                {
                    mcp2Interrupt = device.CreateDigitalInterruptPort(
                        device.Pins.D19, InterruptMode.EdgeRising, ResistorMode.InternalPullDown);
                }

                Mcp_2 = new Mcp23008(I2cBus, address: 0x21, mcp2Interrupt);

                Logger?.Info("Mcp_2 up");
            }
            catch (Exception e)
            {
                Logger?.Trace($"Failed to create MCP2: {e.Message}");
                mcp2Interrupt?.Dispose();
            }

            try
            {
                Mcp_Version = new Mcp23008(I2cBus, address: 0x27);
                Logger?.Info("Mcp_Version up");
            }
            catch (Exception e)
            {
                Logger?.Trace($"ERR creating the MCP that has version information: {e.Message}");
            }

            //---- instantiate display
            Logger?.Trace("Instantiating display");

            DisplayEnablePort = Mcp_1.CreateDigitalOutputPort(Mcp_1.Pins.GP4, true);

            var chipSelectPort = Mcp_1?.CreateDigitalOutputPort(Mcp_1.Pins.GP5);
            var dcPort = Mcp_1?.CreateDigitalOutputPort(Mcp_1.Pins.GP6);
            var resetPort = Mcp_1?.CreateDigitalOutputPort(Mcp_1.Pins.GP7);
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

            //---- led
            RgbLed = new RgbPwmLed(
                redPwmPin: device.Pins.D09,
                greenPwmPin: device.Pins.D10,
                bluePwmPin: device.Pins.D11,
                CommonType.CommonAnode);

            //---- buttons
            Logger?.Trace("Instantiating buttons");
            var leftPort = Mcp_1.CreateDigitalInterruptPort(Mcp_1.Pins.GP2, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            LeftButton = new PushButton(leftPort);
            var rightPort = Mcp_1.CreateDigitalInterruptPort(Mcp_1.Pins.GP1, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            RightButton = new PushButton(rightPort);
            var upPort = Mcp_1.CreateDigitalInterruptPort(Mcp_1.Pins.GP0, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            UpButton = new PushButton(upPort);
            var downPort = Mcp_1.CreateDigitalInterruptPort(Mcp_1.Pins.GP3, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            DownButton = new PushButton(downPort);
            Logger?.Trace("Buttons up");

            try
            {
                Logger?.Trace("Instantiating speaker");
                Speaker = new PiezoSpeaker(device.Pins.D20);
                Logger?.Trace("Speaker up");
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Unable to create the Piezo Speaker: {ex.Message}");
            }

            SetMikroBusPins();
        }

        private void SetMikroBusPins()
        {
            MikroBus1Pins =
                (Resolver.Device.GetPin("PA3"), //A02
                 Resolver.Device.GetPin("Ph10"), //D02
                 Resolver.Device.GetPin("PB12"), //D13
                 Resolver.Device.GetPin("SPI5_SCK"),
                 Resolver.Device.GetPin("SPI5_CIPO"),
                 Resolver.Device.GetPin("SPI5_COPI"),
                 Resolver.Device.GetPin("PB8"), //D03
                 Resolver.Device.GetPin("PC2"),
                 Resolver.Device.GetPin("PB15"), //D13
                 Resolver.Device.GetPin("PB14"), //D12
                 Resolver.Device.GetPin("I2C3_CLK"),
                 Resolver.Device.GetPin("I2C3_DAT"));

            MikroBus2Pins =
                (Resolver.Device.GetPin("PB0"), //A03
                 Mcp_2.Pins.GP1,
                 Mcp_2.Pins.GP2,
                 Resolver.Device.GetPin("SCK"),
                 Resolver.Device.GetPin("CIPO"),
                 Resolver.Device.GetPin("COPI"),
                 Resolver.Device.GetPin("PB9"), //D04
                 Mcp_2.Pins.GP3,
                 Resolver.Device.GetPin("PB15"), //ToDo UART1_A_RX
                 Resolver.Device.GetPin("PB14"), //ToDo UART1_A_TX
                 Resolver.Device.GetPin("I2C1_CLK"),
                 Resolver.Device.GetPin("I2C1_CLK"));
        }

        public override string RevisionString
        {
            get
            {
                if (revision == null)
                {
                    if (Mcp_Version == null)
                    {
                        revision = $"v3.x";
                    }
                    else
                    {
                        byte rev = Mcp_Version.ReadFromPorts(Mcp23xxx.PortBank.A);
                        revision = $"v3.{rev}";
                    }
                }
                return revision;
            }
        }

        private string? revision;

        public override ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
        {
            if (Resolver.Device is F7FeatherBase device)
            {
                var portName = device.PlatformOS.GetSerialPortName("com4");
                var port = device.CreateSerialPort(portName, baudRate, dataBits, parity, stopBits);
                port.WriteTimeout = port.ReadTimeout = TimeSpan.FromSeconds(5);

                return new ModbusRtuClient(port);
            }

            throw new NotSupportedException();
        }
    }
}