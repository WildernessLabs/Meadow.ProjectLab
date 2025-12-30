using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Light;
using Meadow.Foundation.Sensors.Motion;
using Meadow.Hardware;
using Meadow.Modbus;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Leds;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Peripherals.Sensors.Environmental;
using Meadow.Peripherals.Sensors.Light;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Peripherals.Speakers;

namespace Meadow.Devices;

public class ProjectLabFtx : IProjectLabHardware
{
    private class PinDefinitions
    {
        public PinDefinitions(FtdiExpander expander)
        {
            BTN1 = expander.Pins.D4; // BC_D4 == UP
            BTN2 = expander.Pins.D5; // BC_D5 == RIGHT
            BTN3 = expander.Pins.D7; // BC_D7 == DOWN
            BTN4 = expander.Pins.D6; // BC_D6 == LEFT
        }

        public IPin BTN1 { get; } // UP
        public IPin BTN2 { get; } // RIGHT
        public IPin BTN3 { get; } // DOWN
        public IPin BTN4 { get; } // LEFT
    }

    private readonly PinDefinitions _pinDefs;
    private readonly FtdiExpander _expander; // do we need 2 expanders here?

    public ProjectLabFtx(FtdiExpander expander)
    {
        _expander = expander;
        _pinDefs = new PinDefinitions(_expander);
    }

    private II2cBus? _i2c1;

    private IButton? _upButton;
    private IButton? _downButton;
    private IButton? _leftButton;
    private IButton? _rightButton;

    private Bh1750? _lightSensor;
    private Bme688? _bme688;
    private Bmi270? _bmi270;

    private II2cBus I2cBus1 => _i2c1 ??= _expander.CreateI2cBus(0);
    private Bme688? Bme688 => _bme688 ??= new Bme688(I2cBus1, 0x76);
    private Bmi270? Bmi270 => _bmi270 ??= new Bmi270(I2cBus1, 0x68);

    public IButton? UpButton => _upButton ??= new PollingPushButton(_pinDefs.BTN1.CreateDigitalInputPort(ResistorMode.ExternalPullUp));
    public IButton? DownButton => _downButton ??= new PollingPushButton(_pinDefs.BTN3.CreateDigitalInputPort(ResistorMode.ExternalPullUp));
    public IButton? LeftButton => _leftButton ??= new PollingPushButton(_pinDefs.BTN4.CreateDigitalInputPort(ResistorMode.ExternalPullUp));
    public IButton? RightButton => _rightButton ??= new PollingPushButton(_pinDefs.BTN2.CreateDigitalInputPort(ResistorMode.ExternalPullUp));

    public ILightSensor? LightSensor => _lightSensor ??= new Bh1750(I2cBus1, 0x23);
    public ISamplingTemperatureSensor? TemperatureSensor => Bme688;
    public IHumiditySensor? HumiditySensor => Bme688;
    public IBarometricPressureSensor? BarometricPressureSensor => Bme688;
    public IGasResistanceSensor? GasResistanceSensor => Bme688;
    public ISamplingTemperatureSensor? TemperatureSensor2 => Bmi270;
    public IGyroscope? Gyroscope => Bmi270;
    public IAccelerometer? Accelerometer => Bmi270;


    public IToneGenerator? Speaker => throw new System.NotImplementedException();
    public IRgbPwmLed? RgbLed => throw new System.NotImplementedException();
    public IPixelDisplay? Display => throw new System.NotImplementedException();
    public string RevisionString => throw new System.NotImplementedException();
    public MikroBusConnector MikroBus1 => throw new System.NotImplementedException();
    public MikroBusConnector MikroBus2 => throw new System.NotImplementedException();
    public GroveDigitalConnector? GroveDigital => throw new System.NotImplementedException();
    public GroveDigitalConnector GroveAnalog => throw new System.NotImplementedException();
    public UartConnector GroveUart => throw new System.NotImplementedException();
    public Rs485Connector Rs485Connector => throw new System.NotImplementedException();
    public I2cConnector Qwiic => throw new System.NotImplementedException();
    public IOTerminalConnector IOTerminal => throw new System.NotImplementedException();
    public DisplayConnector DisplayHeader => throw new System.NotImplementedException();
    public ITouchScreen? Touchscreen => throw new System.NotImplementedException();

    public IMeadowDevice ComputeModule => throw new System.NotImplementedException();

    public ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
    {
        throw new System.NotImplementedException();
    }
}
