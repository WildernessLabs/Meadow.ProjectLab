using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using Meadow.Modbus;

namespace Meadow.Devices;

internal interface IConnectorProvider
{
    ModbusRtuClient GetModbusRtuClient(ProjectLabHardwareBase projLab, int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One);
    MikroBusConnector CreateMikroBus1(IF7CoreComputeMeadowDevice device, Mcp23008 mcp2);
    MikroBusConnector CreateMikroBus2(IF7CoreComputeMeadowDevice device, Mcp23008 mcp2);
}