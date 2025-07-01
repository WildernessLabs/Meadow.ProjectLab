using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using Meadow.Modbus;
using System;

namespace Meadow.Devices;

public class ProjectLabRs485Connector : Rs485Connector
{
    private readonly Sc16is752 _expander;

    internal ProjectLabRs485Connector(Sc16is752 expander, SerialPortName portName)
        : base("RS485", portName)
    {
        _expander = expander;
    }

    public override IModbusBusClient CreateModbusBusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
    {
        throw new NotImplementedException();
    }

    public override IModbusServer CreateModbusBusRtuServer(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
    {
        throw new NotImplementedException();
    }
}
