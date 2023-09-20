using Meadow.Hardware;
using Meadow.Modbus;
using System.Threading;

namespace Meadow.Devices
{
    /// <summary>
    /// Represents a Modbus RTU client customized for ProjectLab.
    /// </summary>
    public class ProjectLabModbusRtuClient : ModbusRtuClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectLabModbusRtuClient"/> class.
        /// </summary>
        /// <param name="port">The serial port for communication.</param>
        /// <param name="enablePort">The digital output port used for enable control.</param>
        public ProjectLabModbusRtuClient(ISerialPort port, IDigitalOutputPort enablePort)
            : base(port, enablePort)
        {
            // this forces meadow to compile the serial pipeline.  Without it, there's a big delay on sending the first byte
            PostOpenAction = () => { port.Write(new byte[] { 0x00 }); };

            // meadow is not-so-fast, and data will not all get transmitted before the call to the port Write() returns
            PostWriteDelayAction = (m) =>
            {
                var delay = (int)(1d / port.BaudRate * port.DataBits * 1000d * m.Length) + 3; // +3 to add just a little extra for clients who are a little slow to turn off the enable pin
                Thread.Sleep(delay);
            };
        }
    }
}
