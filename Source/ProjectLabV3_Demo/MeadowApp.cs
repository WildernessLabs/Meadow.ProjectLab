using Meadow;
using Meadow.Devices;
using System.Threading.Tasks;

namespace ProjectLabV3_Demo
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IProjectLabHardware projectLab;
        DisplayController displayController;

        public override async Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            projectLab = ProjectLab.Create();

            displayController = new DisplayController(projectLab.Display);
            displayController.ShowSplashScreen();
            await Task.Delay(3000);
            displayController.ShowDataScreen();

            projectLab.UpButton.PressStarted += (s, e) => displayController.UpdateDirectionalPad(0, true);
            projectLab.UpButton.PressEnded += (s, e) => displayController.UpdateDirectionalPad(0, false);
            projectLab.DownButton.PressStarted += (s, e) => displayController.UpdateDirectionalPad(1, true);
            projectLab.DownButton.PressEnded += (s, e) => displayController.UpdateDirectionalPad(1, false);
            projectLab.LeftButton.PressStarted += (s, e) => displayController.UpdateDirectionalPad(2, true);
            projectLab.LeftButton.PressEnded += (s, e) => displayController.UpdateDirectionalPad(2, false);
            projectLab.RightButton.PressStarted += (s, e) => displayController.UpdateDirectionalPad(3, true);
            projectLab.RightButton.PressEnded += (s, e) => displayController.UpdateDirectionalPad(3, false);
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run...");

            Resolver.Log.Info("Hello, Meadow Core-Compute!");

            return base.Run();
        }
    }
}