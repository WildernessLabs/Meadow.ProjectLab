using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Modbus;
using System;
using System.Threading;

namespace Meadow.Devices
{
    public class ProjectLabHardwareV2 : ProjectLabHardwareBase
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
        Mcp23008? Mcp_Version { get; set; }

        /// <summary>
        /// Gets the ST7789 Display on the Project Lab board
        /// </summary>
        public override St7789? Display { get; }

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
        /// Get the ProjectLab pins for mikroBUS header 1
        /// </summary>
        public override (IPin AN, IPin RST, IPin CS, IPin SCK, IPin CIPO, IPin COPI, IPin PWM, IPin INT, IPin RX, IPin TX, IPin SCL, IPin SCA) MikroBus1Pins { get; protected set; }

        /// <summary>
        /// Get the ProjectLab pins for mikroBUS header 2
        /// </summary>
        public override (IPin AN, IPin RST, IPin CS, IPin SCK, IPin CIPO, IPin COPI, IPin PWM, IPin INT, IPin RX, IPin TX, IPin SCL, IPin SCA) MikroBus2Pins { get; protected set; }

        internal ProjectLabHardwareV2(
            IF7FeatherMeadowDevice device,
            ISpiBus spiBus,
            II2cBus i2cBus,
            Mcp23008 mcp1
            ) : base(device, spiBus, i2cBus)
        {
            Mcp_1 = mcp1;
            IDigitalInputPort? mcp2_int = null;

            try
            {
                // MCP the Second
                if (device.Pins.D10.Supports<IDigitalChannelInfo>(c => c.InterruptCapable))
                {
                    mcp2_int = device.CreateDigitalInputPort(
                        device.Pins.D10, InterruptMode.EdgeRising, ResistorMode.InternalPullDown);
                }

                Mcp_2 = new Mcp23008(I2cBus, address: 0x21, mcp2_int);

                Logger?.Info("Mcp_2 up");
            }
            catch (Exception e)
            {
                Logger?.Trace($"Failed to create MCP2: {e.Message}");
                mcp2_int?.Dispose();
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
            var chipSelectPort = mcp1.CreateDigitalOutputPort(mcp1.Pins.GP5);
            var dcPort = mcp1.CreateDigitalOutputPort(mcp1.Pins.GP6);
            var resetPort = mcp1.CreateDigitalOutputPort(mcp1.Pins.GP7);
            Thread.Sleep(50);

            Display = new St7789(
                spiBus: SpiBus,
                chipSelectPort: chipSelectPort,
                dataCommandPort: dcPort,
                resetPort: resetPort,
                width: 240, height: 240,
                colorMode: ColorMode.Format16bppRgb565);

            Display.SetRotation(RotationType._270Degrees);

            Logger?.Trace("Display up");

            //---- buttons
            Logger?.Trace("Instantiating buttons");
            var leftPort = mcp1.CreateDigitalInputPort(mcp1.Pins.GP2, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            LeftButton = new PushButton(leftPort);
            var rightPort = mcp1.CreateDigitalInputPort(mcp1.Pins.GP1, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            RightButton = new PushButton(rightPort);
            var upPort = mcp1.CreateDigitalInputPort(mcp1.Pins.GP0, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            UpButton = new PushButton(upPort);
            var downPort = mcp1.CreateDigitalInputPort(mcp1.Pins.GP3, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            DownButton = new PushButton(downPort);
            Logger?.Trace("Buttons up");

            SetMikroBusPins();
        }

        void SetMikroBusPins()
        {
            MikroBus1Pins =
                (Resolver.Device.GetPin("A02"),
                 Mcp_2.Pins.GP4,
                 Mcp_2.Pins.GP5,
                 Resolver.Device.GetPin("SCK"),
                 Resolver.Device.GetPin("CIPO"),
                 Resolver.Device.GetPin("COPI"),
                 Resolver.Device.GetPin("D03"),
                 Mcp_2.Pins.GP6,
                 Resolver.Device.GetPin("D13"),
                 Resolver.Device.GetPin("D12"),
                 Resolver.Device.GetPin("D07"),
                 Resolver.Device.GetPin("D08"));

            MikroBus2Pins =
                (Resolver.Device.GetPin("A03"),
                 Mcp_2.Pins.GP1,
                 Mcp_2.Pins.GP2,
                 Resolver.Device.GetPin("SCK"),
                 Resolver.Device.GetPin("CIPO"),
                 Resolver.Device.GetPin("COPI"),
                 Resolver.Device.GetPin("D04"),
                 Mcp_2.Pins.GP3,
                 Resolver.Device.GetPin("D13"),
                 Resolver.Device.GetPin("D12"),
                 Resolver.Device.GetPin("D07"),
                 Resolver.Device.GetPin("D08"));
        }

        public override string RevisionString
        {
            get
            {
                if (revision == null)
                {
                    if (Mcp_Version == null)
                    {
                        revision = $"v2.x";
                    }
                    else
                    {
                        byte rev = Mcp_Version.ReadFromPorts(Mcp23xxx.PortBank.A);
                        revision = $"v2.{rev}";
                    }
                }
                return revision;
            }
        }
        string? revision;

        public override ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
        {
            if (Resolver.Device is F7FeatherBase device)
            {
                var portName = device.PlatformOS.GetSerialPortName("com4");
                var port = device.CreateSerialPort(portName, baudRate, dataBits, parity, stopBits);
                port.WriteTimeout = port.ReadTimeout = TimeSpan.FromSeconds(5);
                var serialEnable = Mcp_2.CreateDigitalOutputPort(Mcp_2.Pins.GP0, false);

                return new ProjectLabModbusRtuClient(port, serialEnable);
            }

            throw new NotSupportedException();
        }
    }
}