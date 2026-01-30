using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using Meadow.Units;
using ProjectLabFtx_Demo;

internal class Program
{

    private static async Task Main(string[] args)
    {
        // TODO: convert this to a meadow desktop app
        Console.WriteLine("=== Project Lab FTx Demo ===\n");

        var count = FtdiExpanderCollection.Devices.Count;
        Console.WriteLine($"Found {count} FTDI devices");

        if (count == 0)
        {
            Console.WriteLine("No devices found.");
            return;
        }

        // List all devices
        for (int i = 0; i < count; i++)
        {
            var dev = FtdiExpanderCollection.Devices[i];
            Console.WriteLine($"  Device {i}: {dev.Description} (Serial: {dev.SerialNumber})");
        }
        Console.WriteLine();

        // FT2232H has two channels:
        //   Channel A (expander[0]) - SPI (display) + GPIO
        //   Channel B (expander[1]) - I2C (sensors) + GPIO (buttons)
        
        if (count < 2)
        {
            Console.WriteLine("Warning: Need 2 channels for full functionality. Running with I2C only.");
        }

        var channelA = count >= 2 ? FtdiExpanderCollection.Devices[0] : null;
        var channelB = FtdiExpanderCollection.Devices[count >= 2 ? 1 : 0];

        Console.WriteLine($"Channel A (SPI/Display): {channelA?.Description ?? "Not available"}");
        Console.WriteLine($"Channel B (I2C/Sensors): {channelB.Description}");
        Console.WriteLine();

        // Create ProjectLabFtx with both channels
        var projectLab = new ProjectLabFtx(channelA, channelB);
        Console.WriteLine($"ProjectLab FTx initialized ({projectLab.RevisionString})");
        Console.WriteLine();

        // Initialize display if available
        Console.WriteLine("=== Display ===" );
        Meadow.Foundation.Displays.Ili9341? display = null;
        if (projectLab.Display != null)
        {
            Console.WriteLine("  Display: Available (ILI9341 320x240)");
            display = projectLab.Display as Meadow.Foundation.Displays.Ili9341;
        }
        else
        {
            Console.WriteLine("  Display: NOT AVAILABLE");
        }
        Console.WriteLine();

        // Report sensor availability
        Console.WriteLine("=== Sensor Availability ===");
        Console.WriteLine($"  Temperature (BME688): {(projectLab.TemperatureSensor != null ? "Available" : "NOT DETECTED")}");
        Console.WriteLine($"  Humidity (BME688):    {(projectLab.HumiditySensor != null ? "Available" : "NOT DETECTED")}");
        Console.WriteLine($"  Pressure (BME688):    {(projectLab.BarometricPressureSensor != null ? "Available" : "NOT DETECTED")}");
        Console.WriteLine($"  Accelerometer (BMI270): {(projectLab.Accelerometer != null ? "Available" : "NOT DETECTED")}");
        Console.WriteLine($"  Gyroscope (BMI270):   {(projectLab.Gyroscope != null ? "Available" : "NOT DETECTED")}");
        Console.WriteLine($"  Light (BH1750):       {(projectLab.LightSensor != null ? "Available" : "NOT DETECTED")}");
        Console.WriteLine();

        Console.WriteLine("=== Button Availability ===");
        Console.WriteLine($"  Up:    {(projectLab.UpButton != null ? "Available" : "NOT AVAILABLE")}");
        Console.WriteLine($"  Down:  {(projectLab.DownButton != null ? "Available" : "NOT AVAILABLE")}");
        Console.WriteLine($"  Left:  {(projectLab.LeftButton != null ? "Available" : "NOT AVAILABLE")}");
        Console.WriteLine($"  Right: {(projectLab.RightButton != null ? "Available" : "NOT AVAILABLE")}");
        Console.WriteLine();

        // Set up button event handlers
        if (projectLab.UpButton != null)
        {
            projectLab.UpButton.PressStarted += (s, e) => Console.WriteLine(">>> UP pressed");
        }
        if (projectLab.DownButton != null)
        {
            projectLab.DownButton.PressStarted += (s, e) => Console.WriteLine(">>> DOWN pressed");
        }
        if (projectLab.LeftButton != null)
        {
            projectLab.LeftButton.PressStarted += (s, e) => Console.WriteLine(">>> LEFT pressed");
        }
        if (projectLab.RightButton != null)
        {
            projectLab.RightButton.PressStarted += (s, e) => Console.WriteLine(">>> RIGHT pressed");
        }



        Console.WriteLine("=== Interactive Display Test ===");
        Console.WriteLine("Type a key and press Enter:");
        Console.WriteLine("  r = Fill RED");
        Console.WriteLine("  b = Fill BLUE");
        Console.WriteLine("  k = Fill BLACK");
        Console.WriteLine("  g = Fill GREEN");
        Console.WriteLine("  s = Show() (send buffer to display)");
        Console.WriteLine("  p = Poll GPIO (debug buttons)");
        Console.WriteLine("  q = Exit");
        Console.WriteLine();
        Console.WriteLine("NOTE: Display should flash during init. If it stays white after Show(),");
        Console.WriteLine("      there may be a timing or SPI mode issue. Use a logic analyzer to debug.");
        Console.WriteLine();

        while (true)
        {
            Console.Write("> ");
            Console.Out.Flush();
            var line = Console.ReadLine()?.Trim().ToLower();
            
            if (string.IsNullOrEmpty(line)) continue;
            
            var cmd = line[0];
            
            switch (cmd)
            {
                case 'r':
                    Console.WriteLine(">>> Filling RED...");
                    display?.Fill(Color.Red);
                    Console.WriteLine(">>> RED fill complete (type 's' to Show)");
                    break;
                    
                case 'b':
                    Console.WriteLine(">>> Filling BLUE...");
                    display?.Fill(Color.Blue);
                    Console.WriteLine(">>> BLUE fill complete (type 's' to Show)");
                    break;
                    
                case 'k':
                    Console.WriteLine(">>> Filling BLACK...");
                    display?.Fill(Color.Black);
                    Console.WriteLine(">>> BLACK fill complete (type 's' to Show)");
                    break;
                    
                case 'g':
                    Console.WriteLine(">>> Filling GREEN...");
                    display?.Fill(Color.Green);
                    Console.WriteLine(">>> GREEN fill complete (type 's' to Show)");
                    break;
                    
                case 's':
                    Console.WriteLine(">>> Calling Show()...");
                    display?.Show();
                    Console.WriteLine(">>> Show() complete");
                    break;
                    
                case 'p':
                    // Poll GPIO state by reading digital input ports
                    Console.WriteLine(">>> Polling GPIO state...");
                    Console.WriteLine("  Press and hold a button, then type 'p' again to see state change.");
                    Console.WriteLine();
                    
                    // Channel B - Low Byte D4-D7
                    Console.WriteLine("  Channel B (I2C) - Low Byte D4-D7:");
                    var b_d4 = channelB.CreateDigitalInputPort(channelB.Pins.D4, ResistorMode.ExternalPullUp);
                    var b_d5 = channelB.CreateDigitalInputPort(channelB.Pins.D5, ResistorMode.ExternalPullUp);
                    var b_d6 = channelB.CreateDigitalInputPort(channelB.Pins.D6, ResistorMode.ExternalPullUp);
                    var b_d7 = channelB.CreateDigitalInputPort(channelB.Pins.D7, ResistorMode.ExternalPullUp);
                    Console.WriteLine($"    D4={b_d4.State} D5={b_d5.State} D6={b_d6.State} D7={b_d7.State}");
                    b_d4.Dispose(); b_d5.Dispose(); b_d6.Dispose(); b_d7.Dispose();
                    
                    // Channel B - High Byte C4-C7
                    Console.WriteLine("  Channel B (I2C) - High Byte C4-C7:");
                    var b_c4 = channelB.CreateDigitalInputPort(channelB.Pins.C4, ResistorMode.ExternalPullUp);
                    var b_c5 = channelB.CreateDigitalInputPort(channelB.Pins.C5, ResistorMode.ExternalPullUp);
                    var b_c6 = channelB.CreateDigitalInputPort(channelB.Pins.C6, ResistorMode.ExternalPullUp);
                    var b_c7 = channelB.CreateDigitalInputPort(channelB.Pins.C7, ResistorMode.ExternalPullUp);
                    Console.WriteLine($"    C4={b_c4.State} C5={b_c5.State} C6={b_c6.State} C7={b_c7.State}");
                    b_c4.Dispose(); b_c5.Dispose(); b_c6.Dispose(); b_c7.Dispose();
                    
                    if (channelA != null)
                    {
                        // Channel A - Low Byte D4-D7
                        Console.WriteLine("  Channel A (SPI) - Low Byte D4-D7:");
                        var a_d4 = channelA.CreateDigitalInputPort(channelA.Pins.D4, ResistorMode.ExternalPullUp);
                        var a_d5 = channelA.CreateDigitalInputPort(channelA.Pins.D5, ResistorMode.ExternalPullUp);
                        var a_d6 = channelA.CreateDigitalInputPort(channelA.Pins.D6, ResistorMode.ExternalPullUp);
                        var a_d7 = channelA.CreateDigitalInputPort(channelA.Pins.D7, ResistorMode.ExternalPullUp);
                        Console.WriteLine($"    D4={a_d4.State} D5={a_d5.State} D6={a_d6.State} D7={a_d7.State}");
                        a_d4.Dispose(); a_d5.Dispose(); a_d6.Dispose(); a_d7.Dispose();
                        
                        // Channel A - High Byte C4-C7
                        Console.WriteLine("  Channel A (SPI) - High Byte C4-C7:");
                        var a_c4 = channelA.CreateDigitalInputPort(channelA.Pins.C4, ResistorMode.ExternalPullUp);
                        var a_c5 = channelA.CreateDigitalInputPort(channelA.Pins.C5, ResistorMode.ExternalPullUp);
                        var a_c6 = channelA.CreateDigitalInputPort(channelA.Pins.C6, ResistorMode.ExternalPullUp);
                        var a_c7 = channelA.CreateDigitalInputPort(channelA.Pins.C7, ResistorMode.ExternalPullUp);
                        Console.WriteLine($"    C4={a_c4.State} C5={a_c5.State} C6={a_c6.State} C7={a_c7.State}");
                        a_c4.Dispose(); a_c5.Dispose(); a_c6.Dispose(); a_c7.Dispose();
                    }
                    break;
                    
                case 'q':
                    Console.WriteLine("Exiting...");
                    return;
                    
                default:
                    Console.WriteLine($"Unknown command: {cmd}");
                    break;
            }
        }
    }
}