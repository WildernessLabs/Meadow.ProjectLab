using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using System;

namespace Meadow.Devices
{
    internal class ProjectLabHardwareV1 : IProjectLabHardware
    {
        private IF7FeatherMeadowDevice device;
        private ISpiBus spiBus;
        private St7789? display;
        private PushButton? rightButton;
        private string revision = "v1.x";

        public ProjectLabHardwareV1(IF7FeatherMeadowDevice device, ISpiBus spiBus)
        {
            this.device = device;
            this.spiBus = spiBus;
        }

        public string GetRevisionString()
        {
            return revision;
        }

        public St7789 GetDisplay()
        {
            if (display == null)
            {
                display = new St7789(
                    device: device,
                    spiBus: spiBus,
                    chipSelectPin: device.Pins.A03,
                    dcPin: device.Pins.A04,
                    resetPin: device.Pins.A05,
                    width: 240, height: 240,
                    colorMode: ColorType.Format16bppRgb565);
            }

            return display;
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
            if (rightButton == null)
            {
                rightButton = new PushButton(
                    Resolver.Device.CreateDigitalInputPort(
                        device.Pins.D05,
                        InterruptMode.EdgeBoth,
                        ResistorMode.InternalPullDown));
            }
            return rightButton;
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