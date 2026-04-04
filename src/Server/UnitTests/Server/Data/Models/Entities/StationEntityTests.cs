using NUnit.Framework.Internal;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.UnitTests;
using System.Net;

namespace UnitTests.Server.Data.Models.Entities;

[Category("UnitTest")]
[TestOf(typeof(StationEntity))]
[Author("Jakub Miodunka")]
public sealed class StationEntityTests
{
    #region IsOnline
    [Test]
    public void IsOnlineReturnsTrueIfStationIsOnline()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity() with
        {
            IpAddress = randomizer.NextIpAddress(),
            ApiPort = randomizer.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort + 1),
            ApiVersion = randomizer.NextByte(1, byte.MaxValue)
        };

        bool? isStationOnline = stationEntity.IsOnline();

        Assert.That(isStationOnline, Is.True);
    }

    [Test]
    public void IsOnlineReturnsFalseIfStationIsOffline()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity() with
        {
            IpAddress = null,
            ApiPort = null,
            ApiVersion = null,
        };

        bool? isStationOnline = stationEntity.IsOnline();

        Assert.That(isStationOnline, Is.False);
    }

    [Test]
    public void IsOnlineReturnsNullIfStationStatusIsUnknown()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var stationEntities = new StationEntity[]
        {
            randomizer.NextStationEntity() with
            {
                IpAddress = randomizer.NextIpAddress(),
                ApiPort = null,
                ApiVersion = null,
            },
            randomizer.NextStationEntity() with
            {
                IpAddress = null,
                ApiPort = randomizer.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort + 1),
                ApiVersion = null,
            },
            randomizer.NextStationEntity() with
            {
                IpAddress = null,
                ApiPort = null,
                ApiVersion = randomizer.NextByte(1, byte.MaxValue)
            }
        };

        bool?[] stationStatuses = stationEntities.Select(stationEntity => stationEntity.IsOnline()).ToArray();

        Assert.That(stationStatuses, Has.All.Null);
    }
    #endregion

    #region BaseApiUrl
    [Test]
    public void BaseApiUrlReturnsCorrectUrlForApiVersion1()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity() with
        {
            IpAddress = randomizer.NextIpAddress(),
            ApiPort = randomizer.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort + 1),
            ApiVersion = 1
        };

        Uri expectedBaseApiUrl = new Uri($"http://{stationEntity.IpAddress}:{stationEntity.ApiPort}/api/v{stationEntity.ApiVersion}/");
        Uri? actualBaseApiUrl = stationEntity.BaseApiUrl();

        Assert.That(actualBaseApiUrl, Is.EqualTo(expectedBaseApiUrl));
    }

    [Test]
    public void BaseApiUrlReturnsNullIfStationIsNotOnline()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var stationEntities = new StationEntity[]
        {
            randomizer.NextStationEntity() with
            {
                IpAddress = null,
                ApiPort = null,
                ApiVersion = null,
            },
            randomizer.NextStationEntity() with
            {
                IpAddress = randomizer.NextIpAddress(),
                ApiPort = null,
                ApiVersion = null,
            },
            randomizer.NextStationEntity() with
            {
                IpAddress = null,
                ApiPort = randomizer.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort + 1),
                ApiVersion = null,
            },
            randomizer.NextStationEntity() with
            {
                IpAddress = null,
                ApiPort = null,
                ApiVersion = randomizer.NextByte(1, byte.MaxValue)
            }
        };

        Uri?[] baseApiUrls = stationEntities.Select(stationEntity => stationEntity.BaseApiUrl()).ToArray();

        Assert.That(baseApiUrls, Has.All.Null);
    }
    #endregion
}
