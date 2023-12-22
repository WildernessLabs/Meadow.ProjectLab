using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectLabV3_Demo
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IWiFiNetworkAdapter wifi;

        IProjectLabHardware projectLab;
        DisplayController displayController;

        int currentGraphType = 0;

        List<double> temperatureReadings;
        List<double> pressureReadings;
        List<double> humidityReadings;
        List<double> luminanceReadings;

        public override async Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            temperatureReadings = new List<double>();
            pressureReadings = new List<double>();
            humidityReadings = new List<double>();
            luminanceReadings = new List<double>();

            wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();

            projectLab = ProjectLab.Create();

            displayController = new DisplayController(projectLab.Display);
            displayController.ShowSplashScreen();
            await Task.Delay(3000);
            displayController.ShowDataScreen();

            projectLab.UpButton.PressStarted += (s, e) =>
            {
                currentGraphType = currentGraphType - 1 < 0 ? 3 : currentGraphType - 1;
                UpdateGraph();

                displayController.UpdateDirectionalPad(0, true);
            };
            projectLab.UpButton.PressEnded += (s, e) => displayController.UpdateDirectionalPad(0, false);
            projectLab.DownButton.PressStarted += (s, e) =>
            {
                currentGraphType = currentGraphType + 1 > 3 ? 0 : currentGraphType + 1;
                UpdateGraph();

                displayController.UpdateDirectionalPad(1, true);
            };
            projectLab.DownButton.PressEnded += (s, e) => displayController.UpdateDirectionalPad(1, false);
            projectLab.LeftButton.PressStarted += (s, e) =>
            {
                displayController.UpdateDirectionalPad(2, true);
            };
            projectLab.LeftButton.PressEnded += (s, e) => displayController.UpdateDirectionalPad(2, false);
            projectLab.RightButton.PressStarted += (s, e) =>
            {
                displayController.UpdateDirectionalPad(3, true);
            };
            projectLab.RightButton.PressEnded += (s, e) => displayController.UpdateDirectionalPad(3, false);

            projectLab.EnvironmentalSensor.Updated += EnvironmentalSensorUpdated;
        }

        private void LightSensorUpdated(object sender, IChangeResult<Meadow.Units.Illuminance> e)
        {
            Resolver.Log.Info($"Light sensor: {e.New.Lux}");
        }

        private void EnvironmentalSensorUpdated(object sender, IChangeResult<(Meadow.Units.Temperature? Temperature, Meadow.Units.RelativeHumidity? Humidity, Meadow.Units.Pressure? Pressure, Meadow.Units.Resistance? GasResistance)> e)
        {
            if (temperatureReadings.Count > 10)
            {
                temperatureReadings.RemoveAt(0);
                pressureReadings.RemoveAt(0);
                humidityReadings.RemoveAt(0);
                luminanceReadings.RemoveAt(0);
            }
            temperatureReadings.Add(e.New.Temperature.Value.Celsius);
            pressureReadings.Add(e.New.Pressure.Value.StandardAtmosphere);
            humidityReadings.Add(e.New.Humidity.Value.Percent);
            luminanceReadings.Add(projectLab.LightSensor.Illuminance.Value.Lux);

            displayController.UpdateReadings(
                e.New.Temperature.Value.Celsius,
                e.New.Pressure.Value.StandardAtmosphere,
                e.New.Humidity.Value.Percent,
                projectLab.LightSensor.Illuminance.Value.Lux,
                temperatureReadings);

            UpdateGraph();
        }

        private void UpdateGraph()
        {
            switch (currentGraphType)
            {
                case 0:
                    displayController.UpdateGraph(currentGraphType, temperatureReadings);
                    break;
                case 1:
                    displayController.UpdateGraph(currentGraphType, pressureReadings);
                    break;
                case 2:
                    displayController.UpdateGraph(currentGraphType, humidityReadings);
                    break;
                case 3:
                    displayController.UpdateGraph(currentGraphType, luminanceReadings);
                    break;
            }
        }

        public override async Task Run()
        {
            Resolver.Log.Info("Run...");

            Resolver.Log.Info("Hello, Meadow Core-Compute!");

            projectLab.LightSensor.StartUpdating(TimeSpan.FromSeconds(5));
            projectLab.EnvironmentalSensor.StartUpdating(TimeSpan.FromSeconds(5));

            while (true)
            {
                displayController.UpdateWiFiStatus(wifi.IsConnected);

                displayController.UpdateDateTime();

                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }
    }
}