using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Accelerometers;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Light;
using Meadow.Gateways.Bluetooth;
using Meadow.Hardware;
using Meadow.Modbus;
using System;

namespace Meadow.Devices
{
    internal class ProjectLabHardwareV1 : ProjectLabHardwareBase
    {
        private string revision = "v1.x";

        public ProjectLabHardwareV1(IF7FeatherMeadowDevice device, ISpiBus spiBus, II2cBus i2cBus)
            : base(device, spiBus, i2cBus)
        {
            
        }

        public override string RevisionString => revision;

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
            set { throw new Exception("Don't set this."); }
        }
        protected St7789 display;

        private PushButton GetPushButton(IF7FeatherMeadowDevice device, IPin pin, InterruptMode? interruptMode = null)
        {
            if (interruptMode == null)
            {
                interruptMode = pin.Supports<IDigitalChannelInfo>(c => c.InterruptCapable) ? InterruptMode.EdgeBoth : InterruptMode.None;
            }

            return new PushButton(
                Resolver.Device.CreateDigitalInputPort(
                    pin,
                    interruptMode.Value,
                    ResistorMode.InternalPullDown));
        }

        public PushButton LeftButton
        {
            get
            {
                if (leftButton == null)
                {
                    leftButton = GetPushButton(device, device.Pins.D10);
                }
                return leftButton;
            }
            set { throw new Exception("Don't set this."); }
        }
        protected PushButton leftButton;

        public PushButton RightButton
        {
            get
            {
                if (rightButton == null)
                {
                    rightButton = GetPushButton(device, device.Pins.D05);
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
                if (upButton == null)
                {
                    upButton = GetPushButton(device, device.Pins.D15);
                }
                return upButton;
            }
            set { throw new Exception("Don't set this."); }
        }
        protected PushButton upButton;

        public PushButton DownButton
        {
            get
            {
                if (downButton == null)
                {
                    downButton = GetPushButton(device, device.Pins.D02);
                }
                return downButton;
            }
            set { throw new Exception("Don't set this."); }
        }
        protected PushButton downButton;

        public ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
        {
            if (Resolver.Device is F7FeatherV1 device)
            {
                var port = device.CreateSerialPort(device.SerialPortNames.Com4, baudRate, dataBits, parity, stopBits);
                port.WriteTimeout = port.ReadTimeout = TimeSpan.FromSeconds(5);
                var serialEnable = device.CreateDigitalOutputPort(device.Pins.D09, false);
                return new ModbusRtuClient(port, serialEnable);
            }

            // this is v1 instance hardware, so we should never get here
            throw new NotSupportedException();
        }
    }
}