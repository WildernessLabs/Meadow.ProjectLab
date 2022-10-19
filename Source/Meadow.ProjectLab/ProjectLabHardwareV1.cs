using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using System;

namespace Meadow.Devices
{
    internal class ProjectLabHardwareV1 : IProjectLabHardware
    {
        private IF7FeatherMeadowDevice _device;
        private ISpiBus _spiBus;
        private St7789? _display;
        private PushButton? _right;
        private string _revision = "v1.x";

        public ProjectLabHardwareV1(IF7FeatherMeadowDevice device, ISpiBus spiBus)
        {
            _device = device;
            _spiBus = spiBus;
        }

        public string GetRevisionString()
        {
            return _revision;
        }

        public St7789 GetDisplay()
        {
            if (_display == null)
            {
                _display = new St7789(
                    device: _device,
                    spiBus: _spiBus,
                    chipSelectPin: _device.Pins.A03,
                    dcPin: _device.Pins.A04,
                    resetPin: _device.Pins.A05,
                    width: 240, height: 240,
                    colorMode: ColorType.Format16bppRgb565);
            }

            return _display;
        }

        public PushButton GetLeftButton()
        {
            if (Resolver.Device is F7FeatherV2)
            {
                // D10 no interrupts
            }
            throw new PlatformNotSupportedException("A hardware bug prevents usage of the Left button on ProjectLab v1 hardware.");
        }

        public PushButton GetRightButton()
        {
            if (_right == null)
            {
                _right = new PushButton(
                    Resolver.Device.CreateDigitalInputPort(
                        _device.Pins.D05,
                        InterruptMode.EdgeBoth,
                        ResistorMode.InternalPullDown));
            }
            return _right;
        }

        public PushButton GetUpButton()
        {
            // D15
            if (Resolver.Device is F7FeatherV2)
            {
                // D15 no interrupts
            }
            throw new PlatformNotSupportedException("A hardware bug prevents usage of the Up button on ProjectLab v1 hardware.");
        }

        public PushButton GetDownButton()
        {
            if (Resolver.Device is F7FeatherV2)
            {
                // D02 no interrupts
            }
            throw new PlatformNotSupportedException("A hardware bug prevents usage of the Down button on ProjectLab v1 hardware.");
        }
    }
}

