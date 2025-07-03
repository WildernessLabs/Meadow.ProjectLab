using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using Meadow.Modbus;
using System;

namespace Meadow.Devices;

public class ProjectLabRs485Connector : Rs485Connector
{
    private readonly Sc16is752 _expander;
    private ISerialPort? _serialPort;
    private ModbusRtuClient? _modbusClient;
    private ModbusRtuServer? _modbusServer;

    internal ProjectLabRs485Connector(Sc16is752 expander, SerialPortName portName)
        : base("RS485", portName)
    {
        Resolver.Log.Info($"Creating RS485 connector on {portName} using SC16IS752 expander", Constants.LogGroup);

        _expander = expander;
    }

    /// <inheritdoc/>
    public override ISerialPort CreateSerialPort(int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, int readBufferSize = 1024)
    {
        _serialPort = _expander.PortB.CreateRs485SerialPort(baudRate, dataBits, parity, stopBits);
        return _serialPort;
    }

    /// <inheritdoc/>
    public override IModbusBusClient CreateModbusBusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
    {
        Resolver.Log.Info("Creating Modbus RTU client...");
        lock (_expander)
        {
            if (_modbusClient == null)
            {
                if (_serialPort == null)
                {
                    _serialPort = _expander.PortB.CreateRs485SerialPort(baudRate, dataBits, parity, stopBits);
                }
                _modbusClient = new ModbusRtuClient(_serialPort);
            }
            else
            {
                if (baudRate != _serialPort!.BaudRate)
                {
                    throw new ArgumentException($"Port is already open at {_serialPort.BaudRate}");
                }
            }
            return _modbusClient;
        }
    }

    /// <inheritdoc/>
    public override IModbusServer CreateModbusBusRtuServer(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
    {
        Resolver.Log.Info("Creating Modbus RTU server...");
        lock (_expander)
        {
            if (_modbusServer == null)
            {
                if (_serialPort == null)
                {
                    _serialPort = _expander.PortB.CreateRs485SerialPort(baudRate, dataBits, parity, stopBits);
                }
                _modbusServer = new ModbusRtuServer(_serialPort);
            }
            else
            {
                if (baudRate != _serialPort!.BaudRate)
                {
                    throw new ArgumentException($"Port is already open at {_serialPort.BaudRate}");
                }
            }
            return _modbusServer;
        }
    }
}
