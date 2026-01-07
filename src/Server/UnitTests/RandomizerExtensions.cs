using NUnit.Framework.Internal;
using SmartHome.Server.Data.Models.Entities;
using System.Net;
using System.Net.NetworkInformation;

namespace UnitTests;

internal static class RandomizerExtensions
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

    public static StationEntity NextStationEntity(this Randomizer randomizer)
    {
        ArgumentNullException.ThrowIfNull(randomizer, nameof(randomizer));

        long Id = randomizer.NextInt64(1, long.MaxValue);
        PhysicalAddress macAddress = randomizer.NextMacAddress();
        IPAddress ipAddress = randomizer.NextIpAddress();

        return new StationEntity(Id, macAddress, ipAddress);
    }
}
