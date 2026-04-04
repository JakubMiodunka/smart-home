using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Data.Models.Entities;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;

namespace SmartHome.UnitTests;

internal static class FakeDataGenerationUtilities
{
    public static Mock<IHttpContextAccessor> CreateHttpContextAccessorFake(IPAddress? remoteIpAddress = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = remoteIpAddress;

        var httpContextAccessorFake = new Mock<IHttpContextAccessor>();
        httpContextAccessorFake.Setup(fake => fake.HttpContext).Returns(httpContext);

        return httpContextAccessorFake;
    }

    public static Utf8JsonReader CreateJsonReader(string readerContent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(readerContent, nameof(readerContent));

        string jsonString = JsonSerializer.Serialize(readerContent);
        byte[] encodedJsonString = Encoding.UTF8.GetBytes(jsonString);
        var jsonReader = new Utf8JsonReader(encodedJsonString);
        jsonReader.Read();

        return jsonReader;
    }

    #region Randomizer extensions
    public static bool? NextNullableBool(this Randomizer randomizer)
    {
        ArgumentNullException.ThrowIfNull(randomizer, nameof(randomizer));

        var values = new bool?[] { true, false, null };
        randomizer.Shuffle(values);

        return values.First();
    }

    /// <remarks>
    /// The default minimum value is set to 2000-01-01T00:00:00Z to ensure compatibility with the default
    /// starting point of <see cref="FakeTimeProvider"/>. Using a smaller value would result in
    /// an <see cref="ArgumentOutOfRangeException"/> when initializing provider.
    /// For general testing scenarios, <see cref="DateTimeOffset.MinValue"/>  would typically be used.
    /// </remarks>
    public static DateTimeOffset NextDateTimeOffset(this Randomizer randomizer, DateTimeOffset? from = null, DateTimeOffset? to = null)
    {
        long fromDefault = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero).Ticks;

        long fromTicks = from?.Ticks ?? fromDefault;
        long toTicks = to?.Ticks ?? DateTimeOffset.MaxValue.Ticks;

        long dateTimeOffsetTicks = randomizer.NextInt64(fromTicks, toTicks);

        return new DateTimeOffset(dateTimeOffsetTicks, TimeSpan.Zero);
    }

    public static TimeSpan NextTimeSpan(this Randomizer randomizer, TimeSpan? from = null, TimeSpan? to = null)
    {
        long fromTicks = from?.Ticks ?? TimeSpan.MinValue.Ticks;
        long toTicks = to?.Ticks ?? TimeSpan.MaxValue.Ticks;

        long timespanTicks = randomizer.NextInt64(fromTicks, toTicks);

        return TimeSpan.FromTicks(timespanTicks);
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

    public static StationEntity NextStationEntity(this Randomizer randomizer)
    {
        ArgumentNullException.ThrowIfNull(randomizer, nameof(randomizer));

        long id = randomizer.NextInt64(1, long.MaxValue);
        PhysicalAddress macAddress = randomizer.NextMacAddress();
        IPAddress ipAddress = randomizer.NextIpAddress();
        int apiPort = randomizer.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort + 1);
        byte apiVersion = randomizer.NextByte(1, byte.MaxValue);
        DateTimeOffset lastHeartbeat = randomizer.NextDateTimeOffset();

        return new StationEntity(id, macAddress, ipAddress, apiPort, apiVersion, lastHeartbeat);
    }

    public static SwitchEntity NextSwitchEntity(this Randomizer randomizer)
    {
        ArgumentNullException.ThrowIfNull(randomizer, nameof(randomizer));

        long id = randomizer.NextInt64(1, long.MaxValue);
        long stationId = randomizer.NextInt64(1, long.MaxValue);
        byte localId = randomizer.NextByte();
        bool expectedState = randomizer.NextBool();
        bool? actualState = randomizer.NextNullableBool();

        return new SwitchEntity(id, stationId, localId, expectedState, actualState);
    }
    #endregion
}
