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
        private IF7FeatherMeadowDevice _device;
        private ISpiBus _spiBus;
        private Mcp23008 _mcp1;
        private Mcp23008? _mcpVersion;

        private St7789? _display;
        private PushButton? _left;
        private PushButton? _right;
        private PushButton? _up;
        private PushButton? _down;
        private string? _revision;

        public ProjectLabHardwareV2(Mcp23008 mcp1, Mcp23008? mcpVersion, IF7FeatherMeadowDevice device, ISpiBus spiBus)
        {
            _device = device;
            _spiBus = spiBus;
            _mcp1 = mcp1;
            _mcpVersion = mcpVersion;
        }

        public string GetRevisionString()
        {
            // TODO: figure this out from MCP3?
            if (_revision == null)
            {
                if (_mcpVersion == null)
                {
                    _revision = $"v2.x";
                }
                else
                {
                    byte rev = _mcpVersion.ReadFromPorts(Mcp23xxx.PortBank.A);
                    //mapping? 0 == d2.d?
                    _revision = $"v2.{rev}";
                }
            }
            return _revision;
        }

        public St7789 GetDisplay()
        {
            if (_display == null)
            {
                var chipSelectPort = _mcp1.CreateDigitalOutputPort(_mcp1.Pins.GP5);
                var dcPort = _mcp1.CreateDigitalOutputPort(_mcp1.Pins.GP6);
                var resetPort = _mcp1.CreateDigitalOutputPort(_mcp1.Pins.GP7);

                Thread.Sleep(50);

                _display = new St7789(
                    spiBus: _spiBus,
                    chipSelectPort: chipSelectPort,
                    dataCommandPort: dcPort,
                    resetPort: resetPort,
                    width: 240, height: 240,
                    colorMode: ColorType.Format16bppRgb565);
            }
            return _display;
        }

        public PushButton GetLeftButton()
        {
            if (_left == null)
            {
                var leftPort = _mcp1.CreateDigitalInputPort(_mcp1.Pins.GP2, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
                _left = new PushButton(leftPort);
            }
            return _left;
        }

        public PushButton GetRightButton()
        {
            if (_right == null)
            {
                var rightPort = _mcp1.CreateDigitalInputPort(_mcp1.Pins.GP1, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
                _right = new PushButton(rightPort);
            }
            return _right;
        }

        public PushButton GetUpButton()
        {
            if (_up == null)
            {
                var upPort = _mcp1.CreateDigitalInputPort(_mcp1.Pins.GP0, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
                _up = new PushButton(upPort);
            }
            return _up;
        }

        public PushButton GetDownButton()
        {
            if (_down == null)
            {
                var downPort = _mcp1.CreateDigitalInputPort(_mcp1.Pins.GP3, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
                _down = new PushButton(downPort);
            }
            return _down;
        }
    }
}

