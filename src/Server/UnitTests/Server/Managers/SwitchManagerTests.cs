using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.ApiClients.StationApi;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Models.Requests;
using SmartHome.Server.Managers;
using System.Net;

namespace SmartHome.UnitTests.Server.Managers;


[Category("UnitTest")]
[TestOf(typeof(SwitchManager))]
[Author("Jakub Miodunka")]
public sealed class SwitchManagerTests
{
    #region Constructor
    [Test]
    public void InstantiationPossible()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();
        StationEntity stationEntity = randomizer.NextOnlineStationEntity() with
        {
            Id = switchEntity.StationId
        };

        var stationApiClientFactory = new Mock<IStationApiClientFactory>();
        var loggerStub = new FakeLogger<SwitchManager>();

        SwitchManager? managerUnderTest = null;
        TestDelegate actionUnderTest = () => managerUnderTest =
            new SwitchManager(
                switchEntity,
                stationEntity,
                stationApiClientFactory.Object,
                loggerStub);

        Assert.DoesNotThrow(actionUnderTest);

        Assert.That(managerUnderTest, Is.Not.Null);
        Assert.That(managerUnderTest.ManagedSwitch, Is.EqualTo(switchEntity));
        Assert.That(managerUnderTest.SwitchParentStation, Is.EqualTo(stationEntity));
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsManagedSwitch()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextOnlineStationEntity();

        var stationApiClientFactory = new Mock<IStationApiClientFactory>();
        var loggerStub = new FakeLogger<SwitchManager>();

        TestDelegate actionUnderTest = () => new SwitchManager(
            null!,
            stationEntity,
            stationApiClientFactory.Object,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsParentStation()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();

        var stationApiClientFactory = new Mock<IStationApiClientFactory>();
        var loggerStub = new FakeLogger<SwitchManager>();

        TestDelegate actionUnderTest = () => new SwitchManager(
            switchEntity,
            null!,
            stationApiClientFactory.Object,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsStationApiClientFactory()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();
        StationEntity stationEntity = randomizer.NextOnlineStationEntity() with
        {
            Id = switchEntity.StationId
        };

        var loggerStub = new FakeLogger<SwitchManager>();

        TestDelegate actionUnderTest = () => new SwitchManager(
            switchEntity,
            stationEntity,
            null!,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsLogger()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();
        StationEntity stationEntity = randomizer.NextOnlineStationEntity() with
        {
            Id = switchEntity.StationId
        };

        var stationApiClientFactory = new Mock<IStationApiClientFactory>();

        TestDelegate actionUnderTest = () => new SwitchManager(
            switchEntity,
            stationEntity,
            stationApiClientFactory.Object,
            null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingStationWhichIsNotSwitchParentStation()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();
        StationEntity stationEntity = randomizer.NextOnlineStationEntity();
        
        while (stationEntity.Id == switchEntity.StationId)
        {
            stationEntity = stationEntity with
            {
                Id = randomizer.NextInt64(1, long.MaxValue)
            };
        }

        var stationApiClientFactory = new Mock<IStationApiClientFactory>();
        var loggerStub = new FakeLogger<SwitchManager>();

        TestDelegate actionUnderTest = () => new SwitchManager(
            switchEntity,
            stationEntity,
            stationApiClientFactory.Object,
            loggerStub);

        Assert.Throws<ArgumentException>(actionUnderTest);
    }

    #endregion

    #region Switch state change
    [Test]
    public async Task SwitchStateChangeSuccessfulIfExpectedSwitchStateIsNotEqualToItsActualState()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();
        switchEntity = switchEntity with
        {
            ActualState = !switchEntity.ExpectedState
        };

        StationEntity stationEntity = randomizer.NextOnlineStationEntity() with
        {
            Id = switchEntity.StationId,
            ApiVersion = 1
        };

        Uri endpointUrl = switchEntity.SwitchUrl(stationEntity)!;
        var request = new SwitchUpdateServerRequest(switchEntity.ExpectedState);
        
        var stationApiClientMock = new Mock<IStationApiClient>();
        stationApiClientMock.Setup(mock => mock
            .SendRequestAsync(
                endpointUrl,
                HttpMethod.Patch,
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(HttpStatusCode.NoContent);

        var stationApiClientFactoryStub = new Mock<IStationApiClientFactory>();
        stationApiClientFactoryStub.Setup(mock => mock
            .CreateFor(stationEntity, It.IsAny<TimeSpan>()))
            .Returns(stationApiClientMock.Object);

        var loggerMock = new FakeLogger<SwitchManager>();

        var managerUnderTest = new SwitchManager(
            switchEntity,
            stationEntity,
            stationApiClientFactoryStub.Object,
            loggerMock);

        bool wasAttemptSuccessful = 
            await managerUnderTest.TryChangeState(switchEntity.ExpectedState, CancellationToken.None);

        Assert.That(wasAttemptSuccessful, Is.True);

        stationApiClientMock.Verify(client => client
            .SendRequestAsync(
                endpointUrl,
                HttpMethod.Patch,
                request,
                It.IsAny<CancellationToken>()),
            Times.Once);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Information));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public async Task SwitchStateChangeSuccessfulIfExpectedSwitchStateIsEqualToItsActualState()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();
        switchEntity = switchEntity with
        {
            ActualState = switchEntity.ExpectedState
        };

        StationEntity stationEntity = randomizer.NextOnlineStationEntity() with
        {
            Id = switchEntity.StationId,
            ApiVersion = 1
        };

        var stationApiClientMock = new Mock<IStationApiClient>();
        
        var stationApiClientFactoryStub = new Mock<IStationApiClientFactory>();
        stationApiClientFactoryStub.Setup(mock => mock
            .CreateFor(stationEntity, It.IsAny<TimeSpan>()))
            .Returns(stationApiClientMock.Object);

        var loggerMock = new FakeLogger<SwitchManager>();

        var managerUnderTest = new SwitchManager(
            switchEntity,
            stationEntity,
            stationApiClientFactoryStub.Object,
            loggerMock);

        bool wasAttemptSuccessful = await managerUnderTest.TryChangeState(switchEntity.ExpectedState, CancellationToken.None);

        Assert.That(wasAttemptSuccessful, Is.True);

        stationApiClientMock.Verify(client => client
            .SendRequestAsync(
                It.IsAny<Uri>(),
                It.IsAny<HttpMethod>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Information));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public async Task SwitchStateChangeFailsIfParentStationIsOffline()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();
        StationEntity stationEntity = randomizer.NextOfflineStationEntity() with
        {
            Id = switchEntity.StationId
        };

        var stationApiClientMock = new Mock<IStationApiClient>();

        var stationApiClientFactoryStub = new Mock<IStationApiClientFactory>();
        stationApiClientFactoryStub.Setup(mock => mock
            .CreateFor(stationEntity, It.IsAny<TimeSpan>()))
            .Returns(stationApiClientMock.Object);

        var loggerMock = new FakeLogger<SwitchManager>();

        var managerUnderTest = new SwitchManager(
            switchEntity,
            stationEntity,
            stationApiClientFactoryStub.Object,
            loggerMock);

        bool wasAttemptSuccessful = await managerUnderTest.TryChangeState(switchEntity.ExpectedState, CancellationToken.None);

        Assert.That(wasAttemptSuccessful, Is.False);

        stationApiClientMock.Verify(client => client
            .SendRequestAsync(
                It.IsAny<Uri>(),
                It.IsAny<HttpMethod>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }

    [Test]
    public async Task SwitchStateChangeFailsIfRequestSendingFails()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();
        switchEntity = switchEntity with
        {
            ActualState = !switchEntity.ExpectedState
        };

        StationEntity stationEntity = randomizer.NextOnlineStationEntity() with
        {
            Id = switchEntity.StationId,
            ApiVersion = 1
        };

        var stationApiClientMock = new Mock<IStationApiClient>();
        stationApiClientMock.Setup(mock => mock
            .SendRequestAsync(
                It.IsAny<Uri>(),
                It.IsAny<HttpMethod>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as HttpStatusCode?);

        var stationApiClientFactoryStub = new Mock<IStationApiClientFactory>();
        stationApiClientFactoryStub.Setup(mock => mock
            .CreateFor(stationEntity, It.IsAny<TimeSpan>()))
            .Returns(stationApiClientMock.Object);

        var loggerMock = new FakeLogger<SwitchManager>();

        var managerUnderTest = new SwitchManager(
            switchEntity,
            stationEntity,
            stationApiClientFactoryStub.Object,
            loggerMock);

        bool wasAttemptSuccessful = await managerUnderTest.TryChangeState(switchEntity.ExpectedState, CancellationToken.None);

        Assert.That(wasAttemptSuccessful, Is.False);

        Uri endpointUrl = switchEntity.SwitchUrl(stationEntity)!;
        var request = new SwitchUpdateServerRequest(switchEntity.ExpectedState);

        stationApiClientMock.Verify(client => client
            .SendRequestAsync(
                endpointUrl,
                HttpMethod.Patch,
                request,
                It.IsAny<CancellationToken>()),
            Times.Once);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }

    [Test]
    public async Task SwitchStateChangeFailsIfStationReturnsInvalidStatusCode()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();
        switchEntity = switchEntity with
        {
            ActualState = !switchEntity.ExpectedState
        };

        StationEntity stationEntity = randomizer.NextOnlineStationEntity() with
        {
            Id = switchEntity.StationId,
            ApiVersion = 1
        };

        HttpStatusCode invalidStatusCode = randomizer.NextSuccessfulHttpStatusCode();
        while (invalidStatusCode == HttpStatusCode.NoContent)
        {
            invalidStatusCode = randomizer.NextSuccessfulHttpStatusCode();
        }

        Uri endpointUrl = switchEntity.SwitchUrl(stationEntity)!;
        var request = new SwitchUpdateServerRequest(switchEntity.ExpectedState);

        var stationApiClientMock = new Mock<IStationApiClient>();
        stationApiClientMock.Setup(mock => mock
            .SendRequestAsync(
                endpointUrl,
                HttpMethod.Patch,
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(invalidStatusCode);

        var stationApiClientFactoryStub = new Mock<IStationApiClientFactory>();
        stationApiClientFactoryStub.Setup(mock => mock
            .CreateFor(stationEntity, It.IsAny<TimeSpan>()))
            .Returns(stationApiClientMock.Object);

        var loggerMock = new FakeLogger<SwitchManager>();

        var managerUnderTest = new SwitchManager(
            switchEntity,
            stationEntity,
            stationApiClientFactoryStub.Object,
            loggerMock);

        bool wasAttemptSuccessful = await managerUnderTest.TryChangeState(switchEntity.ExpectedState, CancellationToken.None);

        Assert.That(wasAttemptSuccessful, Is.False);

        stationApiClientMock.Verify(client => client
            .SendRequestAsync(
                endpointUrl,
                HttpMethod.Patch,
                request,
                It.IsAny<CancellationToken>()),
            Times.Once);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }
    #endregion
}