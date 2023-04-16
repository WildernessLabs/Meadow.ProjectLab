<img src="Design/banner.jpg" style="margin-bottom:10px" />

# Meadow.ProjectLab

Project Lab is the most functional IoT prototyping platform on the planet. No more breadboards, complicated wiring, or soldering. Project Lab was built from the ground up using the industry's most powerful, capable, and reliable sensors, components, and connectors.

## Contents
* [Purchasing or Building](#purchasing-or-building)
* [Getting Started](#getting-started)
* [About the Hardware](#about-the-hardware)
  * [Onboard Peripherals](#onboard-peripherals)
  * [Connectivity](#conectivity)
* [Project Lab NuGet](#project-lab-nuget)
* [Pinout Diagram](#pinout-diagram)
  * [Project Lab v1.e](#project-lab-v1e)
  * [Project Lab v2.e](#project-lab-v2e)
* [Hardware Design](#hardware-design)

## Purchasing or Building

You can get a Project Lab from the [Wilderness Labs store](https://store.wildernesslabs.co/collections/frontpage/products/project-lab-board).

It's also designed so that it can be assembled at home for the adventurous. All design files can be found in the [Hardware Design folder](Source/Hardware).

## Getting Started

To make using the hardware even simpler, we've created a Nuget package that instantiates and encapsulates the onboard hardware into a `ProjectLab` class. To use:

1. Add the ProjectLab Nuget package your project: 
    - `dotnet add package Meadow.ProjectLab`, or
    - [Meadow.ProjectLab Nuget Package](https://www.nuget.org/packages/Meadow.ProjectLab)

1. Instantiate the `ProjectLab` class:  
```csharp
public class MeadowApp : App<F7FeatherV2>
{
    IProjectLabHardware projLab;

    public override Task Initialize()
    {
        projLab = ProjectLab.Create();
        ...
```

1. Access the `ProjectLab` peripherals:
```csharp
    if (projLab.EnvironmentalSensor is { } bme688)
    {
        bme688.Updated += Bme688Updated;
        bme688.StartUpdating(TimeSpan.FromSeconds(5));
    }
```

 * [Explore in Fuget.org](https://www.fuget.org/packages/Meadow.ProjectLab/0.1.0/lib/netstandard2.1/ProjectLab.dll/Meadow.Devices/ProjectLab)
 * [Nuget Source](Source/Meadow.ProjectLab)

## Additional Samples

1. **[Setup your Meadow Build Environment](http://developer.wildernesslabs.co/Meadow/Getting_Started/Deploying_Meadow/)** - If you haven't deployed a Meadow app before, you'll need to setup your IDE extension(s), deploy Meadow.OS, etc.
2. **[Run the Demo App](Source/ProjectLab_Demo)** - Deploy the Project Lab demonstration app to see the built in peripherals at work.
3. **[Check out the Project Lab Samples](https://github.com/WildernessLabs/Meadow.ProjectLab.Samples)** - We recommend cloning the [Meadow.ProjectLab.Samples](https://github.com/WildernessLabs/Meadow.ProjectLab.Samples) repo. There you'll find a bunch of awesome samples that you can run right out-of-the box!  
    <a href="https://github.com/WildernessLabs/Meadow.ProjectLab.Samples"><img src="Design/project-lab-samples.png" /></a>
4. **[Add the Project Lab Nuget Package to your own app](https://github.com/WildernessLabs/Meadow.ProjectLab/tree/Demo_App_and_Getting_Started#project-lab-nuget)** - We've created a [Nuget package](https://www.nuget.org/packages/Meadow.ProjectLab) that simplifies using the Project Lab hardware by automatically instiantes the hardware classes for you and makes them available for use in your app. More information on how to use is [below](https://github.com/WildernessLabs/Meadow.ProjectLab/tree/Demo_App_and_Getting_Started#project-lab-nuget).

## About the Hardware

<img src="Design/project-lab-specs.jpg" />

<table>
    <tr>
        <th>Onboard Peripherals</th>
        <th>Connectivity</th>
    </tr>
    <tr>
        <td><strong>ST7789</strong> - SPI 240x240 color display</li></td>
        <td><strong>MikroBUS</strong> - Two sets of MikroBUS pin headers</td>
    </tr>
    <tr>
        <td><strong>BMI270</strong> - I2C motion and acceleration sensor</td>
        <td><strong>Qwiic</strong> - Stemma QT I2C connector</td>
    </tr>
    <tr>
        <td><strong>BH1750</strong> - I2C light sensor</td>
        <td><strong>Grove</strong> - Analog header</td>
    </tr>
    <tr>
        <td><strong>BME688</strong> - I2C atmospheric sensor</td>
        <td><strong>Grove</strong> - GPIO/serial header</td>
    </tr>
    <tr>
        <td><strong>Push Button</strong> - 4 momentary buttons</td>
        <td><strong>RS-485</strong> - Serial</td>
    </tr>
    <tr>
        <td><strong>Magnetic Audio Transducer</strong> - High quality piezo speaker</td>
        <td><strong>Ports</strong> - 3.3V, 5V, ground, one analog and two GPIO ports</td>
    </tr>
</table>

## Pinout Diagram

Check the diagrams below to see what pins on the Meadow are connected to every peripheral on board:
&nbsp;

### Project Lab v1.e

<img src="Design/PinoutV1.jpg" />

### Project Lab v2.e

<img src="Design/PinoutV2.jpg" />

## Hardware Design

You can find the schematics and other design files in the [Hardware folder](Source/Hardware).