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
        public ISpiBus SpiBus { get; set; }
        public II2cBus I2cBus { get; set; }
        //public ModbusRtuClient ModbusRtuClieint { get; set; }
        public ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One);

        public St7789? Display { get; set; }
        public Bh1750? LightSensor { get; set; }
        public Bme688? EnvironmentalSensor { get; set; }
        public Bmi270? MotionSensor { get; set; }
        public PiezoSpeaker Speaker { get; set; }

        public PushButton LeftButton { get; set; }
        public PushButton RightButton { get; set; }
        public PushButton UpButton { get; set; }
        public PushButton DownButton { get; set; }

        public string RevisionString { get; }
    }
}