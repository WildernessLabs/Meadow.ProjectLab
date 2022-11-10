using Meadow.Foundation.Displays;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Modbus;

namespace Meadow.Devices
{
    internal interface IProjectLabHardware
    {
        public St7789 GetDisplay();
        public PushButton GetLeftButton();
        public PushButton GetRightButton();
        public PushButton GetUpButton();
        public PushButton GetDownButton();
        public string GetRevisionString();
        public ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One);
    }
}