using System;
using static Meadow.Hardware.IOTerminalConnector;

namespace Meadow.Hardware;

/// <summary>
/// Represents the IO Terminal connector on Project Lab
/// </summary>
public class IOTerminalConnector : Connector<IOTerminalConnectorPinDefinitions>
{
    /// <summary>
    /// The set of IO terminal connector connector pins
    /// </summary>
    public static class PinNames
    {
        /// <summary>
        /// Pin A1
        /// </summary>
        public const string A1 = "A1";
        /// <summary>
        /// Pin D2
        /// </summary>
        public const string D2 = "D2";
        /// <summary>
        /// Pin D3
        /// </summary>
        public const string D3 = "D3";
    }

    /// <summary>
    /// Represents the pins definitions for the IO Terminal connector on Project Lab
    /// </summary>
    public class IOTerminalConnectorPinDefinitions : PinDefinitionBase
    {
        private readonly IPin? _a1;
        private readonly IPin? _d2;
        private readonly IPin? _d3;

        /// <summary>
        /// Pin A1
        /// </summary>
        public IPin A1 => _a1 ?? throw new PlatformNotSupportedException("Pin not connected");
        /// <summary>
        /// Pin D2
        /// </summary>
        public IPin D2 => _d2 ?? throw new PlatformNotSupportedException("Pin not connected");
        /// <summary>
        /// Pin D3
        /// </summary>
        public IPin D3 => _d3 ?? throw new PlatformNotSupportedException("Pin not connected");

        internal IOTerminalConnectorPinDefinitions(PinMapping mapping)
        {
            foreach (var m in mapping)
            {
                switch (m.PinName)
                {
                    case PinNames.A1:
                        _a1 = m.ConnectsTo;
                        break;
                    case PinNames.D2:
                        _d2 = m.ConnectsTo;
                        break;
                    case PinNames.D3:
                        _d3 = m.ConnectsTo;
                        break;
                }
            }
        }
    }

    /// <param name="name">The connector name</param>
    /// <param name="mapping">The mappings to the host controller</param>
    public IOTerminalConnector(string name, PinMapping mapping)
        : base(name, new IOTerminalConnectorPinDefinitions(mapping))
    {
    }
}