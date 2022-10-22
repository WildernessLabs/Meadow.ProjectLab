namespace Meadow.Devices
{
    public static class Extensions
    {
        public static bool IsV1Hardware(this ProjectLab p)
        {
            var t = p.Hardware is ProjectLabHardwareV1;
            return t;
        }

        public static bool IsV2Hardware(this ProjectLab p)
        {
            var t = p.Hardware is ProjectLabHardwareV2;
            return t;
        }
    }
}