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
            //---- create our display
            Logger?.Info("Instantiating display.");
            base.Display = new St7789(
                        device: device,
                        spiBus: SpiBus,
                        chipSelectPin: device.Pins.A03,
                        dcPin: device.Pins.A04,
                        resetPin: device.Pins.A05,
                        width: 240, height: 240,
                        colorMode: ColorType.Format16bppRgb565);

            //---- buttons
            Logger?.Info("Instantiating buttons.");
            LeftButton = GetPushButton(device, device.Pins.D10);
            RightButton = GetPushButton(device, device.Pins.D05);
            UpButton = GetPushButton(device, device.Pins.D15);
            DownButton = GetPushButton(device, device.Pins.D02);
            Logger?.Info("Buttons up.");
        }

        public override string RevisionString => revision;

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