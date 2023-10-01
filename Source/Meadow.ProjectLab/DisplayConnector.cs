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
        /// Chip Select pin
        /// </summary>
        public const string CS = "CS";
        /// <summary>
        /// Reset pin
        /// </summary>
        public const string RST = "RST";
        /// <summary>
        /// Data/Command pin
        /// </summary>
        public const string DC = "DC";
        /// <summary>
        /// SPI Clock pin
        /// </summary>
        public const string CLK = "CLK";
        /// <summary>
        /// SPI controller ouy, peripheral in pin
        /// </summary>
        public const string COPI = "COPI";
    }

    /// <summary>
    /// Represents the pins definitions for the Display connector on Project Lab
    /// </summary>
    public class DisplayConnectorPinDefinitions : PinDefinitionBase
    {
        private readonly IPin? _cs;
        private readonly IPin? _rst;
        private readonly IPin? _dc;
        private readonly IPin? _clk;
        private readonly IPin? _copi;

        /// <summary>
        /// Chip Select pin
        /// </summary>
        public IPin CS => _cs ?? throw new PlatformNotSupportedException("Pin not connected");
        /// <summary>
        /// Reset pin
        /// </summary>
        public IPin RST => _rst ?? throw new PlatformNotSupportedException("Pin not connected");
        /// <summary>
        /// Data/Command pin
        /// </summary>
        public IPin DC => _dc ?? throw new PlatformNotSupportedException("Pin not connected");
        /// <summary>
        /// SPI Clock pin
        /// </summary>
        public IPin CLK => _clk ?? throw new PlatformNotSupportedException("Pin not connected");
        /// <summary>
        /// SPI controller in, peripheral out pin
        /// </summary>
        public IPin COPI => _copi ?? throw new PlatformNotSupportedException("Pin not connected");

        internal DisplayConnectorPinDefinitions(PinMapping mapping)
        {
            foreach (var m in mapping)
            {
                switch (m.PinName)
                {
                    case PinNames.CS:
                        _cs = m.ConnectsTo;
                        break;
                    case PinNames.RST:
                        _rst = m.ConnectsTo;
                        break;
                    case PinNames.DC:
                        _dc = m.ConnectsTo;
                        break;
                    case PinNames.CLK:
                        _clk = m.ConnectsTo;
                        break;
                    case PinNames.COPI:
                        _copi = m.ConnectsTo;
                        break;
                }
            }
        }
    }

    /// <param name="name">The connector name</param>
    /// <param name="mapping">The mappings to the host controller</param>
    public DisplayConnector(string name, PinMapping mapping)
        : base(name, new DisplayConnectorPinDefinitions(mapping))
    {
    }
}