using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // TODO: convert this to a meadow desktop app

        // TODO: expander 0 and expander 1 are both failing for buttons??
        var expander = FtdiExpanderCollection.Devices[1];

        var projectLab = new ProjectLabFtx(expander);

        //projectLab.UpButton!.Clicked += (s, e) => Console.WriteLine("Up Clicked");
        //projectLab.DownButton!.Clicked += (s, e) => Console.WriteLine("Down Clicked");
        //projectLab.LeftButton!.Clicked += (s, e) => Console.WriteLine("Left Clicked");
        //projectLab.RightButton!.Clicked += (s, e) => Console.WriteLine("Right Clicked");

        while (true)
        {
            var light = await projectLab.LightSensor!.Read();
            var temp = await projectLab.TemperatureSensor!.Read();
            var accel = await projectLab.Accelerometer!.Read();

            Console.WriteLine($"Light: {light.Lux:N0}lux");
            Console.WriteLine($"Temp: {temp.Celsius:N2}°C");
            Console.WriteLine($"Accel: X:{accel.X:N2} Y:{accel.Y:N2} Z:{accel.Z:N2} g");

            await Task.Delay(1000);
        }
    }
}