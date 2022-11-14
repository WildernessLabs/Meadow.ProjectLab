using System;
using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Accelerometers;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Light;
using Meadow.Hardware;

namespace Meadow.Devices
{
    internal class ProjectLabHardwareV1 : IProjectLabHardware
    {
        private IF7FeatherMeadowDevice device;
        public ISpiBus SpiBus { get; set; }
        public II2cBus I2CBus { get; set; }
        public Bh1750? LightSensor { get; set; }
        public Bme688? EnvironmentalSensor { get; set; }
        public Bmi270? MotionSensor { get; set; }
        public PiezoSpeaker Speaker { get; set; }
        private string revision = "v1.x";

        public ProjectLabHardwareV1(IF7FeatherMeadowDevice device, ISpiBus spiBus)
        {
            this.device = device;
            this.SpiBus = spiBus;
        }

        public string RevisionString => revision;

        public St7789 Display
        {
            get
            {
                if (display == null)
                {
                    display = new St7789(
                        device: device,
                        spiBus: SpiBus,
                        chipSelectPin: device.Pins.A03,
                        dcPin: device.Pins.A04,
                        resetPin: device.Pins.A05,
                        width: 240, height: 240,
                        colorMode: ColorType.Format16bppRgb565);
                }

                return display;
            }
            set { display = value; }
        }
        protected St7789 display;

        public PushButton LeftButton
        {
            get
            {
                if (Resolver.Device is F7FeatherV2)
                {
                    // D10 no interrupts
                }
                throw new PlatformNotSupportedException("A hardware bug prevents usage of the Left button on ProjectLab v1 hardware.");
            }
            set { throw new Exception("Don't set this."); }
        }

        public PushButton RightButton
        {
            get
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
            set { throw new Exception("Don't set this."); }
        }
        protected PushButton rightButton;

        public PushButton UpButton
        {
            get
            {
                // D15
                if (Resolver.Device is F7FeatherV2)
                {
                    // D15 no interrupts
                }
                throw new PlatformNotSupportedException("A hardware bug prevents usage of the Up button on ProjectLab v1 hardware.");
            }
            set { throw new Exception("Don't set this."); }
        }

        public PushButton DownButton
        {
            get
            {
                if (Resolver.Device is F7FeatherV2)
                {
                    // D02 no interrupts
                }
                throw new PlatformNotSupportedException("A hardware bug prevents usage of the Down button on ProjectLab v1 hardware.");
            }
            set { throw new Exception("Don't set this."); }
        }
    }
}