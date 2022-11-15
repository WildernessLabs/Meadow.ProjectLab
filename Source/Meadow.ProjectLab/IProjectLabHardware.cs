using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Sensors.Accelerometers;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Light;
using Meadow.Hardware;
using Meadow.Modbus;

namespace Meadow.Devices
{
    public interface IProjectLabHardware
    {
        public ISpiBus SpiBus { get; }
        public II2cBus I2cBus { get; }
        public ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One);

        public St7789? Display { get; }
        public Bh1750? LightSensor { get; }
        public Bme688? EnvironmentalSensor { get; }
        public Bmi270? MotionSensor { get; }
        public PiezoSpeaker? Speaker { get; }

        public PushButton? LeftButton { get; }
        public PushButton? RightButton { get; }
        public PushButton? UpButton { get; }
        public PushButton? DownButton { get; }

        public string RevisionString { get; }
    }
}