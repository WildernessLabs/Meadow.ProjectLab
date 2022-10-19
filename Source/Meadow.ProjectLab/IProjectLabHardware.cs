using Meadow.Foundation.Displays;
using Meadow.Foundation.Sensors.Buttons;

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
    }
}

