using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Accelerometers;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Light;
using Meadow.Hardware;
using Meadow.Logging;
using Meadow.Modbus;
using Meadow.Peripherals.Displays;
using System;
using System.Threading;

namespace Meadow.Devices
{
    internal class ProjectLabHardwareV2 : ProjectLabHardwareBase
    {
        protected Mcp23008 mcp1;
        protected Mcp23008 mcp2;
        protected Mcp23008? mcpVersion;

        public ProjectLabHardwareV2(
            IF7FeatherMeadowDevice device,
            ISpiBus spiBus,
            II2cBus i2cBus,
            Mcp23008 mcp1, Mcp23008 mcp2, Mcp23008? mcpVersion
            ) : base (device, spiBus, i2cBus )
        {
            this.mcp1 = mcp1;
            this.mcp2 = mcp2;
            this.mcpVersion = mcpVersion;

            //---- instantiate display
            Logger?.Info("Instantiating display.");
            var chipSelectPort = mcp1.CreateDigitalOutputPort(mcp1.Pins.GP5);
            var dcPort = mcp1.CreateDigitalOutputPort(mcp1.Pins.GP6);
            var resetPort = mcp1.CreateDigitalOutputPort(mcp1.Pins.GP7);
            Thread.Sleep(50);

            base.Display = new St7789(
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
                    if (mcpVersion == null)
                    {
                        revision = $"v2.x";
                    }
                    else
                    {
                        byte rev = mcpVersion.ReadFromPorts(Mcp23xxx.PortBank.A);
                        //mapping? 0 == d2.d?
                        revision = $"v2.{rev}";
                    }
                }
                return revision;
            }
        }
        protected string? revision;

        public ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
        {
            if (Resolver.Device is F7FeatherV2 device)
            {
                var port = device.CreateSerialPort(device.SerialPortNames.Com4, baudRate, dataBits, parity, stopBits);
                port.WriteTimeout = port.ReadTimeout = TimeSpan.FromSeconds(5);
                var serialEnable = mcp2.CreateDigitalOutputPort(mcp2.Pins.GP0, false);

                return new ModbusRtuClient(port, serialEnable);
            }

            // this is v2 instance hardware, so we should never get here
            throw new NotSupportedException();
        }
    }
}