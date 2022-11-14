﻿using System;
using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Sensors.Accelerometers;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Light;
using Meadow.Hardware;
using Meadow.Logging;
using Meadow.Modbus;

namespace Meadow.Devices
{
    /// <summary>
    /// Contains common elements of Project Lab Hardware
    /// </summary>
    public class ProjectLabHardwareBase : IProjectLabHardware
    {
        protected Logger? Logger { get; } = Resolver.Log;
        protected IF7FeatherMeadowDevice device;

        //==== properties

        /// <summary>
        /// Gets the SPI Bus
        /// </summary>
        public ISpiBus SpiBus { get; set; }
        /// <summary>
        /// Gets the I2C Bus
        /// </summary>
        public II2cBus I2cBus { get; set; }
        /// <summary>
        /// Gets the BH1750 Light Sensor on the Project Lab board
        /// </summary>
        public Bh1750? LightSensor { get; set; }
        /// <summary>
        /// Gets the BME688 environmental sensor  on the Project Lab board
        /// </summary>
        public Bme688? EnvironmentalSensor { get; set; }
        /// <summary>
        /// Gets the Piezo noise maker on the Project Lab board
        /// </summary>
        public PiezoSpeaker? Speaker { get; set; }
        /// <summary>
        /// Gets the BMI inertial movement unit (IMU) on the Project Lab board
        /// </summary>
        public Bmi270? MotionSensor { get; set; }
        /// <summary>
        /// Gets the ST7789 Display on the Project Lab board
        /// </summary>
        public St7789? Display { get; set; }
        /// <summary>
        /// Gets the Up PushButton on the Project Lab board
        /// </summary>
        public PushButton? UpButton { get; set; }
        /// <summary>
        /// Gets the Down PushButton on the Project Lab board
        /// </summary>
        public PushButton? DownButton { get; set; }
        /// <summary>
        /// Gets the Left PushButton on the Project Lab board
        /// </summary>
        public PushButton? LeftButton { get; set; }
        /// <summary>
        /// Gets the Right PushButton on the Project Lab board
        /// </summary>
        public PushButton? RightButton { get; set; }
        /// <summary>
        /// Gets the ProjectLab board hardware revision
        /// </summary>
        public virtual string RevisionString { get; set; } = "unknown";

        public ProjectLabHardwareBase(IF7FeatherMeadowDevice device, ISpiBus spiBus, II2cBus i2cBus)
        {
            this.device = device;
            this.SpiBus = spiBus;
            this.I2cBus = i2cBus;

            //==== Initialize the shared/common stuff
            try
            {
                Logger?.Info("Instantiating light sensor.");
                LightSensor = new Bh1750(
                    i2cBus: I2cBus,
                    measuringMode: Bh1750.MeasuringModes.ContinuouslyHighResolutionMode, // the various modes take differing amounts of time.
                    lightTransmittance: 0.5, // lower this to increase sensitivity, for instance, if it's behind a semi opaque window
                    address: (byte)Bh1750.Addresses.Address_0x23);
                Logger?.Info("Light sensor up.");
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Unable to create the BH1750 Light Sensor: {ex.Message}");
            }

            try
            {
                Logger?.Info("Instantiating environmental sensor.");
                EnvironmentalSensor = new Bme688(I2cBus, (byte)Bme688.Addresses.Address_0x76);
                Logger?.Info("Environmental sensor up.");
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Unable to create the BME688 Environmental Sensor: {ex.Message}");
            }

            try
            {
                Logger?.Info("Instantiating speaker.");
                Speaker = new PiezoSpeaker(device, device.Pins.D11);
                Logger?.Info("Speaker up.");
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Unable to create the Piezo Speaker: {ex.Message}");
            }

            try
            {
                Logger?.Info("Instantiating motion sensor.");
                MotionSensor = new Bmi270(I2cBus);
                Logger?.Info("Motion sensor up.");
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Unable to create the BMI270 IMU: {ex.Message}");
            }

        }

        /// <summary>
        /// Gets a ModbusRtuClient for the on-baord RS485 connector
        /// </summary>
        /// <param name="baudRate"></param>
        /// <param name="dataBits"></param>
        /// <param name="parity"></param>
        /// <param name="stopBits"></param>
        /// <returns></returns>
        public ModbusRtuClient GetModbusRtuClient(int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
        {
            //return GetModbusRtuClient(baudRate, dataBits, parity, stopBits);
            throw new Exception("Deal with this.");
        }

        /// <summary>
        /// Gets the pin definitions for the Project Lab board
        /// </summary>
        public static (
            IPin MB1_CS,
            IPin MB1_INT,
            IPin MB1_PWM,
            IPin MB1_AN,
            IPin MB1_SO,
            IPin MB1_SI,
            IPin MB1_SCK,
            IPin MB1_SCL,
            IPin MB1_SDA,

            IPin MB2_CS,
            IPin MB2_INT,
            IPin MB2_PWM,
            IPin MB2_AN,
            IPin MB2_SO,
            IPin MB2_SI,
            IPin MB2_SCK,
            IPin MB2_SCL,
            IPin MB2_SDA,

            IPin A0,
            IPin D03,
            IPin D04
            ) Pins = (
            Resolver.Device.GetPin("D14"),
            Resolver.Device.GetPin("D03"),
            Resolver.Device.GetPin("D04"),
            Resolver.Device.GetPin("A00"),
            Resolver.Device.GetPin("CIPO"),
            Resolver.Device.GetPin("COPI"),
            Resolver.Device.GetPin("SCK"),
            Resolver.Device.GetPin("D08"),
            Resolver.Device.GetPin("D07"),

            Resolver.Device.GetPin("A02"),
            Resolver.Device.GetPin("D04"),
            Resolver.Device.GetPin("D03"),
            Resolver.Device.GetPin("A01"),
            Resolver.Device.GetPin("CIPO"),
            Resolver.Device.GetPin("COPI"),
            Resolver.Device.GetPin("SCK"),
            Resolver.Device.GetPin("D08"),
            Resolver.Device.GetPin("D07"),

            Resolver.Device.GetPin("A00"),
            Resolver.Device.GetPin("D03"),
            Resolver.Device.GetPin("D04")
            );
    }
}

