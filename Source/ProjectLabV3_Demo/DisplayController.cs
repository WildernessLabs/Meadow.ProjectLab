using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using System;
using System.Collections.Generic;

namespace ProjectLabV3_Demo
{
    internal class DisplayController
    {
        readonly int margin = 5;
        readonly int smallMargin = 3;
        readonly int graphHeight = 106;

        readonly int measureBoxWidth = 82;

        readonly int column1 = 96;
        readonly int column2 = 206;
        readonly int columnWidth = 109;

        readonly int rowHeight = 30;
        readonly int row1 = 135;
        readonly int row2 = 170;
        readonly int row3 = 205;

        readonly int sensorBarHeight = 14;
        readonly int sensorBarInitialWidth = 6;
        readonly int sensorBarX = 184;
        readonly int sensorBarY = 202;
        readonly int sensorBarZ = 220;

        readonly int clockX = 244;
        readonly int clockWidth = 71;

        readonly int dPadSize = 9;

        protected DisplayScreen DisplayScreen { get; set; }

        protected AbsoluteLayout SplashLayout { get; set; }

        protected AbsoluteLayout DataLayout { get; set; }

        public LineChartSeries LineChartSeries { get; set; }

        protected LineChart LineChart { get; set; }

        protected Picture WifiStatus { get; set; }

        protected Label Status { get; set; }

        protected Box TemperatureBox { get; set; }
        protected Label TemperatureLabel { get; set; }
        protected Label TemperatureValue { get; set; }

        protected Box PressureBox { get; set; }
        protected Label PressureLabel { get; set; }
        protected Label PressureValue { get; set; }

        protected Box HumidityBox { get; set; }
        protected Label HumidityLabel { get; set; }
        protected Label HumidityValue { get; set; }

        protected Box LuminanceBox { get; set; }
        protected Label LuminanceLabel { get; set; }
        protected Label LuminanceValue { get; set; }

        protected Label Date { get; set; }
        protected Label Time { get; set; }

        protected Box Up { get; set; }
        protected Box Down { get; set; }
        protected Box Left { get; set; }
        protected Box Right { get; set; }

        protected Box AccelerometerX { get; set; }
        protected Box AccelerometerY { get; set; }
        protected Box AccelerometerZ { get; set; }

        protected Box GyroscopeX { get; set; }
        protected Box GyroscopeY { get; set; }
        protected Box GyroscopeZ { get; set; }

        protected Label ConnectionErrorLabel { get; set; }

        private Color backgroundColor = Color.FromHex("10485E");
        private Color selectedColor = Color.FromHex("C9DB31");
        private Color accentColor = Color.FromHex("EF7D3B");
        private Color ForegroundColor = Color.FromHex("EEEEEE");
        private Font12x20 font12X20 = new Font12x20();
        private Font8x12 font8x12 = new Font8x12();
        private Font8x16 font8x16 = new Font8x16();
        private Font6x8 font6x8 = new Font6x8();

        public DisplayController(IGraphicsDisplay display)
        {
            DisplayScreen = new DisplayScreen(display, RotationType._270Degrees)
            {
                BackgroundColor = backgroundColor
            };

            LoadSplashLayout();

            LoadDataLayout();

            DisplayScreen.Controls.Add(SplashLayout, DataLayout);
        }

        private void LoadSplashLayout()
        {
            SplashLayout = new AbsoluteLayout(DisplayScreen, 0, 0, DisplayScreen.Width, DisplayScreen.Height)
            {
                BackgroundColor = Color.FromHex("#14607F"),
                IsVisible = false
            };

            var image = Image.LoadFromResource("ProjectLabV3_Demo.Resources.img_meadow.bmp");
            var displayImage = new Picture(0, 0, DisplayScreen.Width, DisplayScreen.Height, image)
            {
                BackColor = Color.FromHex("#14607F"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            SplashLayout.Controls.Add(displayImage);
        }

        private void LoadDataLayout()
        {
            DataLayout = new AbsoluteLayout(DisplayScreen, 0, 0, DisplayScreen.Width, DisplayScreen.Height)
            {
                BackgroundColor = backgroundColor,
                IsVisible = false
            };

            DataLayout.Controls.Add(new Label(
                margin,
                margin,
                DisplayScreen.Width / 2,
                font8x16.Height)
            {
                Text = $"Project Lab v3",
                TextColor = Color.White,
                Font = font8x16
            });

            var wifiImage = Image.LoadFromResource("ProjectLabV3_Demo.Resources.img_wifi_connecting.bmp");
            WifiStatus = new Picture(
                DisplayScreen.Width - wifiImage.Width - margin,
                margin,
                wifiImage.Width,
                font8x16.Height,
                wifiImage)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            DataLayout.Controls.Add(WifiStatus);

            LineChart = new LineChart(
                margin,
                margin + font8x16.Height + smallMargin,
                DisplayScreen.Width - margin * 2,
                graphHeight)
            {
                BackgroundColor = Color.FromHex("082936"),
                AxisColor = ForegroundColor,
                ShowYAxisLabels = true,
                IsVisible = false,
                AlwaysShowYOrigin = false,
            };
            LineChartSeries = new LineChartSeries()
            {
                LineColor = selectedColor,
                PointColor = selectedColor,
                LineStroke = 1,
                PointSize = 2,
                ShowLines = true,
                ShowPoints = true,
            };
            LineChart.Series.Add(LineChartSeries);
            DataLayout.Controls.Add(LineChart);

            // TEMPERATURE
            TemperatureBox = new Box(margin, row1, measureBoxWidth, rowHeight)
            {
                ForeColor = selectedColor
            };
            DataLayout.Controls.Add(TemperatureBox);
            TemperatureLabel = new Label(
                margin + smallMargin,
                row1 + smallMargin,
                measureBoxWidth - smallMargin * 2,
                font6x8.Height)
            {
                Text = $"TEMPERATURE",
                TextColor = backgroundColor,
                Font = font6x8
            };
            DataLayout.Controls.Add(TemperatureLabel);
            TemperatureValue = new Label(
                margin + smallMargin,
                row1 + font6x8.Height + smallMargin * 2,
                measureBoxWidth - smallMargin * 2,
                font6x8.Height * 2)
            {
                Text = $"-.-C",
                TextColor = backgroundColor,
                Font = font6x8,
                ScaleFactor = ScaleFactor.X2
            };
            DataLayout.Controls.Add(TemperatureValue);

            // PRESSURE
            PressureBox = new Box(
                margin,
                row2,
                measureBoxWidth,
                rowHeight)
            {
                ForeColor = backgroundColor
            };
            DataLayout.Controls.Add(PressureBox);
            PressureLabel = new Label(
                margin + smallMargin,
                row2 + smallMargin,
                measureBoxWidth - smallMargin * 2,
                font6x8.Height)
            {
                Text = $"PRESSURE",
                TextColor = ForegroundColor,
                Font = font6x8
            };
            DataLayout.Controls.Add(PressureLabel);
            PressureValue = new Label(
                margin + smallMargin,
                row2 + font6x8.Height + smallMargin * 2,
                measureBoxWidth - smallMargin * 2,
                font6x8.Height * 2)
            {
                Text = $"-.-atm",
                TextColor = ForegroundColor,
                Font = font6x8,
                ScaleFactor = ScaleFactor.X2
            };
            DataLayout.Controls.Add(PressureValue);

            // HUMIDITY
            HumidityBox = new Box(
                margin,
                row3,
                measureBoxWidth,
                rowHeight)
            {
                ForeColor = backgroundColor
            };
            DataLayout.Controls.Add(HumidityBox);
            HumidityLabel = new Label(
                margin + smallMargin,
                row3 + smallMargin,
                measureBoxWidth - smallMargin * 2,
                font6x8.Height)
            {
                Text = $"HUMIDITY",
                TextColor = ForegroundColor,
                Font = font6x8
            };
            DataLayout.Controls.Add(HumidityLabel);
            HumidityValue = new Label(
                margin + smallMargin,
                row3 + font6x8.Height + smallMargin * 2,
                columnWidth - smallMargin * 2,
                font6x8.Height * 2)
            {
                Text = $"-.-%",
                TextColor = ForegroundColor,
                Font = font6x8,
                ScaleFactor = ScaleFactor.X2
            };
            DataLayout.Controls.Add(HumidityValue);

            // LUMINANCE
            LuminanceBox = new Box(
                column1,
                row1,
                columnWidth,
                rowHeight)
            {
                ForeColor = backgroundColor
            };
            DataLayout.Controls.Add(LuminanceBox);
            LuminanceLabel = new Label(
                column1 + smallMargin,
                row1 + smallMargin,
                columnWidth - smallMargin * 2,
                font6x8.Height)
            {
                Text = $"LUMINANCE",
                TextColor = ForegroundColor,
                Font = font6x8
            };
            DataLayout.Controls.Add(LuminanceLabel);
            LuminanceValue = new Label(
                column1 + smallMargin,
                row1 + font6x8.Height + smallMargin * 2,
                columnWidth - smallMargin * 2,
                font6x8.Height * 2)
            {
                Text = $"0Lux",
                TextColor = ForegroundColor,
                Font = font6x8,
                ScaleFactor = ScaleFactor.X2
            };
            DataLayout.Controls.Add(LuminanceValue);

            // ACCELEROMETER
            DataLayout.Controls.Add(new Label(
                column1 + smallMargin,
                row2 + smallMargin,
                columnWidth - smallMargin * 2,
                font6x8.Height)
            {
                Text = $"ACCELEROMETER (g)",
                TextColor = Color.White,
                Font = font6x8
            });

            DataLayout.Controls.Add(new Label(
                column1 + smallMargin,
                sensorBarX,
                font6x8.Width * 2,
                font6x8.Height * 2)
            {
                Text = $"X",
                TextColor = Color.White,
                Font = font6x8,
                ScaleFactor = ScaleFactor.X2
            });
            AccelerometerX = new Box(
                column1 + font6x8.Width * 2 + margin,
                sensorBarX,
                sensorBarInitialWidth,
                sensorBarHeight)
            {
                ForeColor = Color.FromHex("98A645")
            };
            DataLayout.Controls.Add(AccelerometerX);

            DataLayout.Controls.Add(new Label(
                column1 + smallMargin,
                sensorBarY,
                font6x8.Width * 2,
                font6x8.Height * 2)
            {
                Text = $"Y",
                TextColor = Color.White,
                Font = font6x8,
                ScaleFactor = ScaleFactor.X2
            });
            AccelerometerY = new Box(
                column1 + font6x8.Width * 2 + margin,
                sensorBarY,
                sensorBarInitialWidth,
                sensorBarHeight)
            {
                ForeColor = Color.FromHex("C9DB31")
            };
            DataLayout.Controls.Add(AccelerometerY);

            DataLayout.Controls.Add(new Label(
                column1 + smallMargin,
                sensorBarZ,
                font6x8.Width * 2,
                font6x8.Height * 2)
            {
                Text = $"Z",
                TextColor = Color.White,
                Font = font6x8,
                ScaleFactor = ScaleFactor.X2
            });
            AccelerometerZ = new Box(
                column1 + font6x8.Width * 2 + margin,
                sensorBarZ,
                sensorBarInitialWidth,
                sensorBarHeight)
            {
                ForeColor = Color.FromHex("E1EB8B")
            };
            DataLayout.Controls.Add(AccelerometerZ);

            // GYROSCOPE
            DataLayout.Controls.Add(new Label(
                column2 + smallMargin,
                row2 + smallMargin,
                columnWidth - smallMargin * 2,
                font6x8.Height)
            {
                Text = $"GYROSCOPE (rpm)",
                TextColor = Color.White,
                Font = font6x8
            });

            DataLayout.Controls.Add(new Label(
                column2 + smallMargin,
                sensorBarX,
                font6x8.Width * 2,
                font6x8.Height * 2)
            {
                Text = $"X",
                TextColor = Color.White,
                Font = font6x8,
                ScaleFactor = ScaleFactor.X2
            });
            GyroscopeX = new Box(
                column2 + font6x8.Width * 2 + margin,
                sensorBarX,
                sensorBarInitialWidth,
                sensorBarHeight)
            {
                ForeColor = Color.FromHex("98A645")
            };
            DataLayout.Controls.Add(GyroscopeX);

            DataLayout.Controls.Add(new Label(
                column2 + smallMargin,
                sensorBarY,
                font6x8.Width * 2,
                font6x8.Height * 2)
            {
                Text = $"Y",
                TextColor = Color.White,
                Font = font6x8,
                ScaleFactor = ScaleFactor.X2
            });
            GyroscopeY = new Box(
                column2 + font6x8.Width * 2 + margin,
                sensorBarY,
                sensorBarInitialWidth,
                sensorBarHeight)
            {
                ForeColor = Color.FromHex("C9DB31")
            };
            DataLayout.Controls.Add(GyroscopeY);

            DataLayout.Controls.Add(new Label(
                column2 + smallMargin,
                sensorBarZ,
                font6x8.Width * 2,
                font6x8.Height * 2)
            {
                Text = $"Z",
                TextColor = Color.White,
                Font = font6x8,
                ScaleFactor = ScaleFactor.X2
            });
            GyroscopeZ = new Box(
                column2 + font6x8.Width * 2 + margin,
                sensorBarZ,
                sensorBarInitialWidth,
                sensorBarHeight)
            {
                ForeColor = Color.FromHex("E1EB8B")
            };
            DataLayout.Controls.Add(GyroscopeZ);

            // CLOCK
            DataLayout.Controls.Add(new Box(
                clockX,
                row1,
                clockWidth,
                rowHeight)
            {
                ForeColor = ForegroundColor
            });
            Date = new Label(
                clockX,
                row1 + smallMargin,
                clockWidth,
                font6x8.Height)
            {
                Text = $"----/--/--",
                TextColor = backgroundColor,
                HorizontalAlignment = HorizontalAlignment.Center,
                Font = font6x8
            };
            DataLayout.Controls.Add(Date);
            Time = new Label(
                clockX,
                row1 + font6x8.Height + smallMargin * 2,
                clockWidth,
                font6x8.Height * 2)
            {
                Text = $"--:--",
                TextColor = backgroundColor,
                HorizontalAlignment = HorizontalAlignment.Center,
                Font = font6x8,
                ScaleFactor = ScaleFactor.X2
            };
            DataLayout.Controls.Add(Time);

            // D-Pad
            Up = new Box(
                218,
                136,
                dPadSize,
                dPadSize)
            {
                ForeColor = ForegroundColor
            };
            DataLayout.Controls.Add(Up);
            Down = new Box(
                218,
                156,
                dPadSize,
                dPadSize)
            {
                ForeColor = ForegroundColor
            };
            DataLayout.Controls.Add(Down);
            Left = new Box(
                208,
                146,
                dPadSize,
                dPadSize)
            {
                ForeColor = ForegroundColor
            };
            DataLayout.Controls.Add(Left);
            Right = new Box(
                228,
                146,
                dPadSize,
                dPadSize)
            {
                ForeColor = ForegroundColor
            };
            DataLayout.Controls.Add(Right);
        }

        public void ShowSplashScreen()
        {
            DataLayout.IsVisible = false;
            SplashLayout.IsVisible = true;
        }

        public void ShowDataScreen()
        {
            SplashLayout.IsVisible = false;
            DataLayout.IsVisible = true;
        }

        public void UpdateStatus(string status)
        {
            Status.Text = status;
        }

        public void UpdateWiFiStatus(bool isConnected)
        {
            var imageWiFi = isConnected
                ? Image.LoadFromResource("ProjectLabV3_Demo.Resources.img_wifi_connected.bmp")
                : Image.LoadFromResource("ProjectLabV3_Demo.Resources.img_wifi_connecting.bmp");
            WifiStatus.Image = imageWiFi;
        }

        public void UpdateReadings(double temperature, double pressure, double humidity, double luminance, List<double> readings)
        {
            DisplayScreen.BeginUpdate();

            TemperatureValue.Text = $"{temperature:N1}C";
            PressureValue.Text = $"{pressure:N1}atm";
            HumidityValue.Text = $"{humidity:N1}%";
            LuminanceValue.Text = $"{luminance:N0}Lx";

            DisplayScreen.EndUpdate();
        }

        public void UpdateGraph(int graphType, List<double> readings)
        {
            DisplayScreen.BeginUpdate();

            UpdateSelectReading(graphType);

            LineChartSeries.Points.Clear();

            for (var p = 0; p < readings.Count; p++)
            {
                LineChartSeries.Points.Add(p * 2, readings[p]);
            }

            DisplayScreen.EndUpdate();
        }

        public void UpdateDateTime()
        {
            DisplayScreen.BeginUpdate();

            int TIMEZONE_OFFSET = -8;
            var today = DateTime.Now.AddHours(TIMEZONE_OFFSET);
            Date.Text = today.ToString("yyyy/MM/dd");
            Time.Text = today.ToString("HH:mm");

            DisplayScreen.EndUpdate();
        }

        public void UpdateDirectionalPad(int direction, bool pressed)
        {
            //DisplayScreen.BeginUpdate();

            switch (direction)
            {
                case 0: Up.ForeColor = pressed ? accentColor : ForegroundColor; break;
                case 1: Down.ForeColor = pressed ? accentColor : ForegroundColor; break;
                case 2: Left.ForeColor = pressed ? accentColor : ForegroundColor; break;
                case 3: Right.ForeColor = pressed ? accentColor : ForegroundColor; break;
            }

            //DisplayScreen.EndUpdate();
        }

        protected void UpdateSelectReading(int reading)
        {
            TemperatureBox.ForeColor = PressureBox.ForeColor = HumidityBox.ForeColor = LuminanceBox.ForeColor = backgroundColor;
            TemperatureLabel.TextColor = PressureLabel.TextColor = HumidityLabel.TextColor = LuminanceLabel.TextColor = ForegroundColor;
            TemperatureValue.TextColor = PressureValue.TextColor = HumidityValue.TextColor = LuminanceValue.TextColor = ForegroundColor;

            switch (reading)
            {
                case 0:
                    TemperatureBox.ForeColor = selectedColor;
                    TemperatureLabel.TextColor = backgroundColor;
                    TemperatureValue.TextColor = backgroundColor;
                    break;
                case 1:
                    PressureBox.ForeColor = selectedColor;
                    PressureLabel.TextColor = backgroundColor;
                    PressureValue.TextColor = backgroundColor;
                    break;
                case 2:
                    HumidityBox.ForeColor = selectedColor;
                    HumidityLabel.TextColor = backgroundColor;
                    HumidityValue.TextColor = backgroundColor;
                    break;
                case 3:
                    LuminanceBox.ForeColor = selectedColor;
                    LuminanceLabel.TextColor = backgroundColor;
                    LuminanceValue.TextColor = backgroundColor;
                    break;
            }
        }

        public void UpdateAccelerometerReading(double x, double y, double z)
        {
            DisplayScreen.BeginUpdate();
            AccelerometerX.Width = (int)x * 10 + 5;
            AccelerometerY.Width = (int)y * 10 + 5;
            AccelerometerZ.Width = (int)z * 10 + 5;
            DisplayScreen.EndUpdate();
        }

        public void UpdateGyroscopeReading(double x, double y, double z)
        {
            DisplayScreen.BeginUpdate();
            GyroscopeX.Width = (int)x * 10 + 5;
            GyroscopeY.Width = (int)y * 10 + 5;
            GyroscopeZ.Width = (int)z * 10 + 5;
            DisplayScreen.EndUpdate();
        }
    }
}