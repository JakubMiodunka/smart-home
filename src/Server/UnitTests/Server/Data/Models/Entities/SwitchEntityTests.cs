using NUnit.Framework.Internal;
using SmartHome.Server.Data.Models.Entities;
using System.Net;

namespace SmartHome.UnitTests.Server.Data.Models.Entities;

[Category("UnitTest")]
[TestOf(typeof(SwitchEntity))]
[Author("Jakub Miodunka")]
public sealed class SwitchEntityTests
{
    #region IsOnline
    [Test]
    public void IsOnlineReturnsTrueIfActualStateIsKnown()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        SwitchEntity switchEntity = randomizer.NextSwitchEntity() with
        {
            ActualState = randomizer.NextBool()
        };

        Assert.That(switchEntity.IsOnline(), Is.True);
    }

    [Test]
    public void IsOnlineReturnsFalseIfActualStateIsUnknown()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        SwitchEntity switchEntity = randomizer.NextSwitchEntity() with
        {
            ActualState = null
        };

        Assert.That(switchEntity.IsOnline(), Is.False);
    }
    #endregion

    #region SwitchUrl
    [Test]
    public void SwitchUrlReturnsCorrectUrlForApiVersion1()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity() with
        {
            IpAddress = randomizer.NextIpAddress(),
            ApiPort = randomizer.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort + 1),
            ApiVersion = 1
        };

        SwitchEntity switchEntity = randomizer.NextSwitchEntity() with
        {
            StationId = stationEntity.Id
        };

        Uri? baseStationApiUrl = stationEntity.BaseApiUrl();
        Assert.That(baseStationApiUrl, Is.Not.Null);

        Uri expectedSwitchUrl = new Uri(baseStationApiUrl, $"switches/{switchEntity.LocalId}");
        Uri? actualSwitchUrl = switchEntity.SwitchUrl(stationEntity);

        Assert.That(actualSwitchUrl, Is.EqualTo(expectedSwitchUrl));
    }

    [Test]
    public void SwitchUrlThrowsArgumentNullExceptionIfParentStationIsNull()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();

        TestDelegate actionUnderTest = () => switchEntity.SwitchUrl(null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void SwitchUrlThrowsArgumentOutOfRangeExceptionIfSpecifiedStationIsNotParentStation()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;


        SwitchEntity switchEntity = randomizer.NextSwitchEntity();

        long stationId = randomizer.NextInt64(1, long.MaxValue);
        while (stationId == switchEntity.StationId)
        {
            stationId = randomizer.NextInt64(1, long.MaxValue);
        }

        StationEntity stationEntity = randomizer.NextStationEntity() with
        {
            Id = stationId
        };

        TestDelegate actionUnderTest = () => switchEntity.SwitchUrl(stationEntity);

        Assert.Throws<ArgumentOutOfRangeException>(actionUnderTest);
    }

    [Test]
    public void SwitchUrlReturnsNullIfStationIsNotOnline()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity offlineStationEntity = randomizer.NextOfflineStationEntity();

        SwitchEntity switchEntity = randomizer.NextSwitchEntity() with
        {
            StationId = offlineStationEntity.Id
        };

        Uri? actualSwitchUrl = switchEntity.SwitchUrl(offlineStationEntity);

        Assert.That(actualSwitchUrl, Is.Null);
    }

    [Test]
    public void SwitchUrlThrowsNotSupportedExceptionIfApiVersionNotSupported()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity() with
        {
            IpAddress = randomizer.NextIpAddress(),
            ApiPort = randomizer.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort + 1),
            ApiVersion = byte.MaxValue  // It is very unlikely that such a high API version will be supported.
        };

        SwitchEntity switchEntity = randomizer.NextSwitchEntity() with
        {
            StationId = stationEntity.Id
        };

        TestDelegate actionUnderTest = () => switchEntity.SwitchUrl(stationEntity);

        Assert.Throws<NotSupportedException>(actionUnderTest);
    }

    #endregion
}
