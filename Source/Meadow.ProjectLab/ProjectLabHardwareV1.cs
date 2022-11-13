using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Modbus;
using System;

namespace Meadow.Devices
{
    internal class ProjectLabHardwareV1 : IProjectLabHardware
    {
        private IF7FeatherMeadowDevice device;
        private ISpiBus spiBus;
        private St7789? display;
        private PushButton? upButton, downButton, leftButton, rightButton;
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
            if (leftButton == null)
            {
                leftButton = new PushButton(
                    device,
                    device.Pins.D10,
                    ResistorMode.InternalPullDown);
            }
            return leftButton;
        }

        public PushButton GetRightButton()
        {
            if (rightButton == null)
            {
                rightButton = new PushButton(
                    device,
                    device.Pins.D05,
                    ResistorMode.InternalPullDown);
            }
            return rightButton;
        }

        public PushButton GetUpButton()
        {
            if (upButton == null)
            {
                upButton = new PushButton(
                    device,
                    device.Pins.D15,
                    ResistorMode.InternalPullDown);
            }
            return upButton;
        }

        public PushButton GetDownButton()
        {
            if (downButton == null)
            {
                downButton = new PushButton(
                    device,
                    device.Pins.D02,
                    ResistorMode.InternalPullDown);
            }
            return downButton;
        }

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