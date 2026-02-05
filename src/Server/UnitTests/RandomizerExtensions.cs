using NUnit.Framework.Internal;
using SmartHome.Server.Data.Models.Entities;
using System.Net;
using System.Net.NetworkInformation;

namespace SmartHome.UnitTests;

internal static class RandomizerExtensions
{
    public static bool? NextNullableBool(this Randomizer randomizer)
    {
        ArgumentNullException.ThrowIfNull(randomizer, nameof(randomizer));

        var values = new bool?[] { true, false, null };
        randomizer.Shuffle(values);

        return values.First();
    }

    public static DateTime NextDateTime(this Randomizer randomizer)
    {
        long randomTimestamp = randomizer.NextInt64(DateTime.MinValue.Ticks, DateTime.MaxValue.Ticks);
        return new DateTime(randomTimestamp).ToUniversalTime();
    }

    public static PhysicalAddress NextMacAddress(this Randomizer randomizer)
    {
        ArgumentNullException.ThrowIfNull(randomizer, nameof(randomizer));

        var macAddress = new byte[6];
        randomizer.NextBytes(macAddress);

        return new PhysicalAddress(macAddress);
    }

    public static IPAddress NextIpAddress(this Randomizer randomizer, bool isIpV6 = false)
    {
        ArgumentNullException.ThrowIfNull(randomizer, nameof(randomizer));

        var ipAddress = new byte[isIpV6 ? 16 : 4];
        randomizer.NextBytes(ipAddress);

        return new IPAddress(ipAddress);
    }

    public static StationEntity NextStationEntity(this Randomizer randomizer, DateTime? lastHeartbeat = null)
    {
        ArgumentNullException.ThrowIfNull(randomizer, nameof(randomizer));

        long id = randomizer.NextInt64(1, long.MaxValue);
        PhysicalAddress macAddress = randomizer.NextMacAddress();
        IPAddress ipAddress = randomizer.NextIpAddress();
        lastHeartbeat = lastHeartbeat ?? randomizer.NextDateTime();

        return new StationEntity(id, macAddress, ipAddress, lastHeartbeat.Value);
    }

    public static SwitchEntity NextSwitchEntity(this Randomizer randomizer, long? stationId = null)
    {
        ArgumentNullException.ThrowIfNull(randomizer, nameof(randomizer));

        long id = randomizer.NextInt64(1, long.MaxValue);
        stationId = stationId ?? randomizer.NextInt64(1, long.MaxValue);
        byte localId = randomizer.NextByte();
        bool expectedState = randomizer.NextBool();
        bool? actualState = randomizer.NextNullableBool();

        return new SwitchEntity(id, stationId.Value, localId, expectedState, actualState);
    }
}
