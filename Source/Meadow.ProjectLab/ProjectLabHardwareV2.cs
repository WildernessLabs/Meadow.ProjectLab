using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using System.Threading;

namespace Meadow.Devices
{
    internal class ProjectLabHardwareV2 : IProjectLabHardware
    {
        private IF7FeatherMeadowDevice device;
        private ISpiBus spiBus;
        private Mcp23008 mcp1;
        private Mcp23008? mcpVersion;

        private St7789? display;
        private PushButton? leftButton;
        private PushButton? rightButton;
        private PushButton? upButton;
        private PushButton? downButton;
        private string? revision;

        public ProjectLabHardwareV2(Mcp23008 mcp1, Mcp23008? mcpVersion, IF7FeatherMeadowDevice device, ISpiBus spiBus)
        {
            this.device = device;
            this.spiBus = spiBus;
            this.mcp1 = mcp1;
            this.mcpVersion = mcpVersion;
        }

        public string GetRevisionString()
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

        public St7789 GetDisplay()
        {
            if (display == null)
            {
                var chipSelectPort = mcp1.CreateDigitalOutputPort(mcp1.Pins.GP5);
                var dcPort = mcp1.CreateDigitalOutputPort(mcp1.Pins.GP6);
                var resetPort = mcp1.CreateDigitalOutputPort(mcp1.Pins.GP7);

                Thread.Sleep(50);

                display = new St7789(
                    spiBus: spiBus,
                    chipSelectPort: chipSelectPort,
                    dataCommandPort: dcPort,
                    resetPort: resetPort,
                    width: 240, height: 240,
                    colorMode: ColorType.Format16bppRgb565);
            }
            return display;
        }

        public PushButton GetLeftButton()
        {
            if (leftButton == null)
            {
                var leftPort = mcp1.CreateDigitalInputPort(mcp1.Pins.GP2, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
                leftButton = new PushButton(leftPort);
            }
            return leftButton;
        }

        public PushButton GetRightButton()
        {
            if (rightButton == null)
            {
                var rightPort = mcp1.CreateDigitalInputPort(mcp1.Pins.GP1, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
                rightButton = new PushButton(rightPort);
            }
            return rightButton;
        }

        public PushButton GetUpButton()
        {
            if (upButton == null)
            {
                var upPort = mcp1.CreateDigitalInputPort(mcp1.Pins.GP0, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
                upButton = new PushButton(upPort);
            }
            return upButton;
        }

        public PushButton GetDownButton()
        {
            if (downButton == null)
            {
                var downPort = mcp1.CreateDigitalInputPort(mcp1.Pins.GP3, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
                downButton = new PushButton(downPort);
            }
            return downButton;
        }
    }
}