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

        public St7789 Display
        {
            get
            {
                if (display == null)
                {
                    Logger?.Info("Instantiating display.");
                    var chipSelectPort = mcp1.CreateDigitalOutputPort(mcp1.Pins.GP5);
                    var dcPort = mcp1.CreateDigitalOutputPort(mcp1.Pins.GP6);
                    var resetPort = mcp1.CreateDigitalOutputPort(mcp1.Pins.GP7);

                    Thread.Sleep(50);

                    display = new St7789(
                        spiBus: SpiBus,
                        chipSelectPort: chipSelectPort,
                        dataCommandPort: dcPort,
                        resetPort: resetPort,
                        width: 240, height: 240,
                        colorMode: ColorType.Format16bppRgb565);
                    Logger?.Info("Display up.");
                }
                return display;
            }
            set { display = value; }
        }
        protected St7789? display;

        public PushButton LeftButton
        {
            get
            {
                if (leftButton == null)
                {
                    var leftPort = mcp1.CreateDigitalInputPort(mcp1.Pins.GP2, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
                    leftButton = new PushButton(leftPort);
                }
                return leftButton;
            }
            set { throw new Exception("Don't set this."); }
        }
        protected PushButton? leftButton;

        public PushButton RightButton
        {
            get
            {
                if (rightButton == null)
                {
                    var rightPort = mcp1.CreateDigitalInputPort(mcp1.Pins.GP1, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
                    rightButton = new PushButton(rightPort);
                }
                return rightButton;
            }
            set { throw new Exception("Don't set this."); }
        }
        protected PushButton? rightButton;

        public PushButton UpButton
        {
            get
            {
                if (upButton == null)
                {
                    var upPort = mcp1.CreateDigitalInputPort(mcp1.Pins.GP0, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
                    upButton = new PushButton(upPort);
                }
                return upButton;
            }
            set { throw new Exception("Don't set this."); }
        }
        protected PushButton? upButton;

        public PushButton DownButton
        {
            get
            {
                if (downButton == null)
                {
                    var downPort = mcp1.CreateDigitalInputPort(mcp1.Pins.GP3, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
                    downButton = new PushButton(downPort);
                }
                return downButton;
            }
            set { throw new Exception("Don't set this."); }
        }
        protected PushButton? downButton;

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