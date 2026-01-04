using NUnit.Framework.Internal;
using System.Net;
using System.Net.NetworkInformation;

namespace UnitTests;

internal static class NUnitExtensions
{
    public static PhysicalAddress NextMacAddress(this Randomizer randomizer)
    {
        ArgumentNullException.ThrowIfNull(randomizer, nameof(randomizer));

        var macAddress = new byte[6];
        randomizer.NextBytes(macAddress);
        return new PhysicalAddress(macAddress);
    }

    public static IPAddress NextIpAddress(this Randomizer randomizer)
    {
        ArgumentNullException.ThrowIfNull(randomizer, nameof(randomizer));

        var ipAddress = new byte[4];
        randomizer.NextBytes(ipAddress);
        return new IPAddress(ipAddress);
    }
}
