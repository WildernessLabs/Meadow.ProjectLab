using Meadow.Units;
using System;
using static Meadow.Hardware.DisplayConnector;

namespace Meadow.Hardware;

/// <summary>
/// Represents the display connector on Project Lab
/// </summary>
public class DisplayConnector : Connector<DisplayConnectorPinDefinitions>
{
    /// <summary>
    /// The set of Display connector connector pins
    /// </summary>
    public static class PinNames
    {
        /// <summary>
        /// Display Chip Select pin
        /// </summary>
        public const string DISPLAY_CS = "DISPLAY_CS";
        /// <summary>
        /// Display Reset pin
        /// </summary>
        public const string DISPLAY_RST = "DISPLAY_RST";
        /// <summary>
        /// Display Data/Command pin
        /// </summary>
        public const string DISPLAY_DC = "DISPLAY_DC";
        /// <summary>
        /// Display SPI Clock pin
        /// </summary>
        public const string DISPLAY_CLK = "DISPLAY_CLK";
        /// <summary>
        /// Display SPI controller out, peripheral in pin
        /// </summary>
        public const string DISPLAY_COPI = "DISPLAY_COPI";
        /// <summary>
        /// Display LED (backlight) pin
        /// </summary>
        public const string DISPLAY_LED = "DISPLAY_LED";
        /// <summary>
        /// Touch screen interrupt pin
        /// </summary>
        public const string TOUCH_INT = "TOUCH_INT";
        /// <summary>
        /// Touch Chip Select pin
        /// </summary>
        public const string TOUCH_CS = "TOUCH_CS";
        /// <summary>
        /// Touch SPI Clock pin
        /// </summary>
        public const string TOUCH_CLK = "TOUCH_CLK";
        /// <summary>
        /// Touch SPI controller out, peripheral in pin
        /// </summary>
        public const string TOUCH_COPI = "TOUCH_COPI";
        /// <summary>
        /// Touch SPI controller in, peripheral out pin
        /// </summary>
        public const string TOUCH_CIPO = "TOUCH_CIPO";
    }

    /// <summary>
    /// Represents the pins definitions for the Display connector on Project Lab
    /// </summary>
    public class DisplayConnectorPinDefinitions : PinDefinitionBase
    {
        private readonly IPin? _csDisplay;
        private readonly IPin? _rstDisplay;
        private readonly IPin? _dcDisplay;
        private readonly IPin? _clkDisplay;
        private readonly IPin? _copiDisplay;
        private readonly IPin? _ledDisplay;
        private readonly IPin? _intTouch;
        private readonly IPin? _csTouch;
        private readonly IPin? _clkTouch;
        private readonly IPin? _copiTouch;
        private readonly IPin? _cipoTouch;

        /// <summary>
        /// Display Chip Select pin
        /// </summary>
        public IPin DISPLAY_CS => _csDisplay ?? throw new PlatformNotSupportedException("Pin not connected");
        /// <summary>
        /// Display Reset pin
        /// </summary>
        public IPin DISPLAY_RST => _rstDisplay ?? throw new PlatformNotSupportedException("Pin not connected");
        /// <summary>
        /// Display Data/Command pin
        /// </summary>
        public IPin DISPLAY_DC => _dcDisplay ?? throw new PlatformNotSupportedException("Pin not connected");
        /// <summary>
        /// Display SPI Clock pin
        /// </summary>
        public IPin DISPLAY_CLK => _clkDisplay ?? throw new PlatformNotSupportedException("Pin not connected");
        /// <summary>
        /// Display SPI controller out, peripheral in pin
        /// </summary>
        public IPin DISPLAY_COPI => _copiDisplay ?? throw new PlatformNotSupportedException("Pin not connected");
        /// <summary>
        /// Display LED (backlight) pin
        /// </summary>
        public IPin DISPLAY_LED => _ledDisplay ?? throw new PlatformNotSupportedException("Pin not connected");
        /// <summary>
        /// Touch interrupt pin
        /// </summary>
        public IPin TOUCH_INT => _intTouch ?? throw new PlatformNotSupportedException("Pin not connected");
        /// <summary>
        /// Touch chip select pin
        /// </summary>
        public IPin TOUCH_CS => _csTouch ?? throw new PlatformNotSupportedException("Pin not connected");
        /// <summary>
        /// Touch SPI Clock pin
        /// </summary>
        public IPin TOUCH_CLK => _clkTouch ?? throw new PlatformNotSupportedException("Pin not connected");
        /// <summary>
        /// Touch SPI controller out, peripheral in pin
        /// </summary>
        public IPin TOUCH_COPI => _copiTouch ?? throw new PlatformNotSupportedException("Pin not connected");
        /// <summary>
        /// Touch SPI controller in, peripheral out pin
        /// </summary>
        public IPin TOUCH_CIPO => _cipoTouch ?? throw new PlatformNotSupportedException("Pin not connected");

        internal DisplayConnectorPinDefinitions(PinMapping mapping)
        {
            foreach (var m in mapping)
            {
                switch (m.PinName)
                {
                    case PinNames.DISPLAY_CS:
                        _csDisplay = m.ConnectsTo;
                        break;
                    case PinNames.DISPLAY_RST:
                        _rstDisplay = m.ConnectsTo;
                        break;
                    case PinNames.DISPLAY_DC:
                        _dcDisplay = m.ConnectsTo;
                        break;
                    case PinNames.DISPLAY_CLK:
                        _clkDisplay = m.ConnectsTo;
                        break;
                    case PinNames.DISPLAY_COPI:
                        _copiDisplay = m.ConnectsTo;
                        break;
                    case PinNames.DISPLAY_LED:
                        _ledDisplay = m.ConnectsTo;
                        break;
                    case PinNames.TOUCH_INT:
                        _intTouch = m.ConnectsTo;
                        break;
                    case PinNames.TOUCH_CS:
                        _csTouch = m.ConnectsTo;
                        break;
                    case PinNames.TOUCH_CLK:
                        _clkTouch = m.ConnectsTo;
                        break;
                    case PinNames.TOUCH_COPI:
                        _copiTouch = m.ConnectsTo;
                        break;
                    case PinNames.TOUCH_CIPO:
                        _cipoTouch = m.ConnectsTo;
                        break;
                }
            }
        }
    }

    private readonly SpiBusMapping _spiBusMappingDisplay;
    private readonly SpiBusMapping? _spiBusMappingTouch;
    private ISpiBus? _spiDisplay;
    private readonly ISpiBus? _spiTouch;

    /// <param name="name">The connector name</param>
    /// <param name="mapping">The mappings to the host controller</param>
    /// <param name="spiBusMappingDisplay">The mapping for the display connector's SPI bus</param>
    /// <param name="spiBusMappingTouch">The mapping for the touch connector's SPI bus</param>
    public DisplayConnector(string name, PinMapping mapping, SpiBusMapping spiBusMappingDisplay, SpiBusMapping? spiBusMappingTouch = null)
        : base(name, new DisplayConnectorPinDefinitions(mapping))
    {
        _spiBusMappingDisplay = spiBusMappingDisplay;
        _spiBusMappingTouch = spiBusMappingTouch;
    }

    /// <summary>
    /// Gets the display SPI bus
    /// </summary>
    public ISpiBus SpiBusDisplay
        => _spiDisplay ??= _spiBusMappingDisplay.Controller.CreateSpiBus(_spiBusMappingDisplay.Clock, _spiBusMappingDisplay.Copi, _spiBusMappingDisplay.Cipo, new Frequency(1, Frequency.UnitType.Megahertz));

    /// <summary>
    /// Gets the touch screen SPI bus
    /// </summary>
    public ISpiBus? SpiBusTouch
    {
        get
        {
            if (_spiBusMappingTouch == null || _spiTouch == null)
            {
                return null;
            }
            return _spiBusMappingTouch.Controller.CreateSpiBus(_spiBusMappingTouch.Clock, _spiBusMappingTouch.Copi, _spiBusMappingTouch.Cipo, new Frequency(1, Frequency.UnitType.Megahertz));
        }
    }
}