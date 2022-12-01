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
    internal class ProjectLabHardwareV2 : ProjectLabHardwareBase
    {
        private Mcp23008 mcp_1;
        private Mcp23008 mcp_2;
        private Mcp23008? mcp_Version;

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

        public ProjectLabHardwareV2(
            IF7FeatherMeadowDevice device,
            ISpiBus spiBus,
            II2cBus i2cBus,
            Mcp23008 mcp1
            ) : base(device, spiBus, i2cBus)
        {
            this.mcp_1 = mcp1;
            IDigitalInputPort? mcp2_int = null;

            try
            {
                // MCP the Second
                if (device.Pins.D10.Supports<IDigitalChannelInfo>(c => c.InterruptCapable))
                {
                    mcp2_int = device.CreateDigitalInputPort(
                        device.Pins.D10, InterruptMode.EdgeRising, ResistorMode.InternalPullDown);
                }

                mcp_2 = new Mcp23008(I2cBus, address: 0x21, mcp2_int);

                Logger?.Info("Mcp_2 up");
            }
            catch (Exception e)
            {
                Logger?.Trace($"Failed to create MCP2: {e.Message}");
                mcp2_int?.Dispose();
            }

            try
            {
                mcp_Version = new Mcp23008(I2cBus, address: 0x27);
                Logger?.Info("Mcp_Version up");
            }
            catch (Exception e)
            {
                Logger?.Trace($"ERR creating the MCP that has version information: {e.Message}");
            }

            //---- instantiate display
            Logger?.Info("Instantiating display.");
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
                colorMode: ColorType.Format16bppRgb565);
            Logger?.Info("Display up.");

            //---- buttons
            Logger?.Info("Instantiating buttons.");
            var leftPort = mcp1.CreateDigitalInputPort(mcp1.Pins.GP2, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            LeftButton = new PushButton(leftPort);
            var rightPort = mcp1.CreateDigitalInputPort(mcp1.Pins.GP1, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            RightButton = new PushButton(rightPort);
            var upPort = mcp1.CreateDigitalInputPort(mcp1.Pins.GP0, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            UpButton = new PushButton(upPort);
            var downPort = mcp1.CreateDigitalInputPort(mcp1.Pins.GP3, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            DownButton = new PushButton(downPort);
            Logger?.Info("Buttons up.");
        }

        public override string RevisionString
        {
            get
            {
                // TODO: figure this out from MCP3?
                if (revision == null)
                {
                    if (mcp_Version == null)
                    {
                        revision = $"v2.x";
                    }
                    else
                    {
                        byte rev = mcp_Version.ReadFromPorts(Mcp23xxx.PortBank.A);
                        //mapping? 0 == d2.d?
                        revision = $"v2.{rev}";
                    }
                }
                return revision;
            }
        }
        protected string? revision;

        public override ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
        {
            if (Resolver.Device is F7FeatherV2 device)
            {
                var port = device.CreateSerialPort(device.SerialPortNames.Com4, baudRate, dataBits, parity, stopBits);
                port.WriteTimeout = port.ReadTimeout = TimeSpan.FromSeconds(5);
                var serialEnable = mcp_2.CreateDigitalOutputPort(mcp_2.Pins.GP0, false);

                return new ModbusRtuClient(port, serialEnable);
            }

            // this is v2 instance hardware, so we should never get here
            throw new NotSupportedException();
        }
    }
}