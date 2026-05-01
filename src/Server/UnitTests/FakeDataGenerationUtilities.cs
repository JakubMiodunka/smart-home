using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Time.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Data.Models.Entities;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;

namespace SmartHome.UnitTests;

/// <seealso cref="FakeDataGenerationUtilities.NextHttpRequestBody(Randomizer)"/>
public sealed record GenericHttpRequestBody(bool? Value1, int? Value2, string? Value3);

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
    public static T PickFrom<T>(this Randomizer randomizer, IEnumerable<T> values)
    {
        ArgumentNullException.ThrowIfNull(randomizer, nameof(randomizer));
        ArgumentNullException.ThrowIfNull(values, nameof(values));
        if (values.Count() == 0) throw new ArgumentException("No values to pick from:", nameof(values));

        int index = randomizer.Next(values.Count());

        return values.ElementAt(index);
    }

    public static bool? NextNullableBool(this Randomizer randomizer)
    {
        ArgumentNullException.ThrowIfNull(randomizer, nameof(randomizer));

        var values = new bool?[] { true, false, null };

        return randomizer.PickFrom(values);
    }

    /// <remarks>
    /// The default minimum value is set to 2000-01-01T00:00:00Z to ensure compatibility with the default
    /// starting point of <see cref="FakeTimeProvider"/>. Using a smaller value would result in
    /// an <see cref="ArgumentOutOfRangeException"/> when initializing provider.
    /// For general testing scenarios, <see cref="DateTimeOffset.MinValue"/>  would typically be used.
    /// </remarks>
    public static DateTimeOffset NextDateTimeOffset(this Randomizer randomizer, DateTimeOffset? from = null, DateTimeOffset? to = null)
    {
        ArgumentNullException.ThrowIfNull(randomizer, nameof(randomizer));

        long fromDefault = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero).Ticks;

        long fromTicks = from?.Ticks ?? fromDefault;
        long toTicks = to?.Ticks ?? DateTimeOffset.MaxValue.Ticks;

        long dateTimeOffsetTicks = randomizer.NextInt64(fromTicks, toTicks);
        
        return new DateTimeOffset(dateTimeOffsetTicks, TimeSpan.Zero);
    }

    public static TimeSpan NextTimeSpan(this Randomizer randomizer, TimeSpan? from = null, TimeSpan? to = null)
    {
        ArgumentNullException.ThrowIfNull(randomizer, nameof(randomizer));

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

    public static int NextPort(this Randomizer randomizer)
    {
        ArgumentNullException.ThrowIfNull(randomizer, nameof(randomizer));

        return randomizer.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort + 1);
    }
    
    /// <remarks>
    /// Convers only basic HTTP methods.
    /// </remarks>
    public static HttpMethod NextHttpMethod(this Randomizer randomizer)
    {
        ArgumentNullException.ThrowIfNull(randomizer, nameof(randomizer));

        var values = new HttpMethod[]
        {
            HttpMethod.Get,
            HttpMethod.Post,
            HttpMethod.Put,
            HttpMethod.Delete,
            HttpMethod.Patch
        };

        return randomizer.PickFrom(values);
    }

    public static HttpStatusCode NextSuccessfulHttpStatusCode(this Randomizer randomizer)
    {
        ArgumentNullException.ThrowIfNull(randomizer, nameof(randomizer));

        var values = new HttpStatusCode[]
        {
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.Accepted,
            HttpStatusCode.NonAuthoritativeInformation,
            HttpStatusCode.NoContent,
            HttpStatusCode.ResetContent,
            HttpStatusCode.PartialContent,
            HttpStatusCode.MultiStatus,
            HttpStatusCode.AlreadyReported,
            HttpStatusCode.IMUsed
        };

        return randomizer.PickFrom(values);
    }

    public static Uri NextHttpUrl(this Randomizer randomizer, UriKind uriKind = UriKind.Absolute)
    {
        ArgumentNullException.ThrowIfNull(randomizer, nameof(randomizer));
        
        if (uriKind == UriKind.Relative)
        {
            return new Uri(randomizer.GetString(), UriKind.Relative);
        }

        if (uriKind == UriKind.Absolute)
        {
            var uriBuilder = new UriBuilder(Uri.UriSchemeHttp, randomizer.NextIpAddress().ToString(), randomizer.NextPort());
            return uriBuilder.Uri;
        }

        throw new NotSupportedException($"Uri kind not supported: UriKind=[{uriKind}]");
    }

    public static GenericHttpRequestBody NextHttpRequestBody(this Randomizer randomizer)
    {
        ArgumentNullException.ThrowIfNull(randomizer, nameof(randomizer));

        return new GenericHttpRequestBody(
            randomizer.NextNullableBool(),
            randomizer.Next(),
            randomizer.GetString());
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

    public static StationEntity NextOfflineStationEntity(this Randomizer randomizer)
    {
        ArgumentNullException.ThrowIfNull(randomizer, nameof(randomizer));

        var allVariantsOfOfflineStationEntity = new StationEntity[]
            {
                randomizer.NextStationEntity() with
                {
                IpAddress = null,
                    ApiPort = null,
                    ApiVersion = null
                },
                randomizer.NextStationEntity() with
                {
                    IpAddress = randomizer.NextIpAddress(),
                    ApiPort = null,
                    ApiVersion = null
                },
                randomizer.NextStationEntity() with
                {
                    IpAddress = null,
                    ApiPort = randomizer.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort + 1),
                    ApiVersion = null
                },
                randomizer.NextStationEntity() with
                {
                    IpAddress = null,
                    ApiPort = null,
                    ApiVersion = randomizer.NextByte(1, byte.MaxValue)
                }
            };

        return randomizer.PickFrom(allVariantsOfOfflineStationEntity);
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
