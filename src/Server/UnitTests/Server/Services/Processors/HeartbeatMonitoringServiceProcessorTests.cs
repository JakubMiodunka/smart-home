using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Repositories;
using SmartHome.Server.Services.Processors;
using System.Net;

namespace SmartHome.UnitTests.Server.Services;

[Category("UnitTest")]
[TestOf(typeof(HeartbeatMonitoringServiceProcessor))]
[Author("Jakub Miodunka")]
public sealed class HeartbeatMonitoringServiceProcessorTests
{
    #region Constructor
    [Test]
    public void InstantiationPossible()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        TimeSpan maxHeartbeatInterval = randomizer.NextTimeSpan(
            from: TimeSpan.FromMicroseconds(1));

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var timeProviderStub = new FakeTimeProvider();
        var loggerStub = new FakeLogger<HeartbeatMonitoringServiceProcessor>();

        TestDelegate actionUnderTest = () => new HeartbeatMonitoringServiceProcessor(
            stationsRepositoryMock.Object,
            switchesRepositoryMock.Object,
            timeProviderStub,
            maxHeartbeatInterval,
            loggerStub);

        Assert.DoesNotThrow(actionUnderTest);

        stationsRepositoryMock.AssertNoContentModifications();
        switchesRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsStationsRepository()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        TimeSpan maxHeartbeatInterval = randomizer.NextTimeSpan(
            from: TimeSpan.FromMicroseconds(1));

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var timeProviderStub = new FakeTimeProvider();
        var loggerStub = new FakeLogger<HeartbeatMonitoringServiceProcessor>();

        TestDelegate actionUnderTest = () => new HeartbeatMonitoringServiceProcessor(
            null!,
            switchesRepositoryMock.Object,
            timeProviderStub,
            maxHeartbeatInterval,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);

        switchesRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsSwitchesRepository()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        TimeSpan maxHeartbeatInterval = randomizer.NextTimeSpan(
            from: TimeSpan.FromMicroseconds(1));

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var timeProviderStub = new FakeTimeProvider();
        var loggerStub = new FakeLogger<HeartbeatMonitoringServiceProcessor>();

        TestDelegate actionUnderTest = () => new HeartbeatMonitoringServiceProcessor(
            stationsRepositoryMock.Object,
            null!,
            timeProviderStub,
            maxHeartbeatInterval,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);

        stationsRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsTimeProvider()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        TimeSpan maxHeartbeatInterval = randomizer.NextTimeSpan(
            from: TimeSpan.FromMicroseconds(1));

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerStub = new FakeLogger<HeartbeatMonitoringServiceProcessor>();

        TestDelegate actionUnderTest = () => new HeartbeatMonitoringServiceProcessor(
            stationsRepositoryMock.Object,
            switchesRepositoryMock.Object,
            null!,
            maxHeartbeatInterval,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);

        stationsRepositoryMock.AssertNoContentModifications();
        switchesRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsLogger()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        TimeSpan maxHeartbeatInterval = randomizer.NextTimeSpan(
            from: TimeSpan.FromMicroseconds(1));

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var timeProviderStub = new FakeTimeProvider();

        TestDelegate actionUnderTest = () => new HeartbeatMonitoringServiceProcessor(
            stationsRepositoryMock.Object,
            switchesRepositoryMock.Object,
            timeProviderStub,
            maxHeartbeatInterval,
            null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);

        stationsRepositoryMock.AssertNoContentModifications();
        switchesRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingInvalidMaxHeartbeatInterval(
        [Values(-1, 0)] long invalidMaxHeartbeatInterval)   // Given in microseconds.
    {
        var maxHeartbeatInterval = TimeSpan.FromMicroseconds(invalidMaxHeartbeatInterval);

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var timeProviderStub = new FakeTimeProvider();

        TestDelegate actionUnderTest = () => new HeartbeatMonitoringServiceProcessor(
            stationsRepositoryMock.Object,
            switchesRepositoryMock.Object,
            timeProviderStub,
            maxHeartbeatInterval,
            null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);

        stationsRepositoryMock.AssertNoContentModifications();
        switchesRepositoryMock.AssertNoContentModifications();
    }
    #endregion

    #region Service execution
    [Test]
    public async Task MarksStationAsOfflineWhenHeartbeatTimeoutExceeded()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        TimeSpan maxHeartbeatInterval = randomizer.NextTimeSpan(
            from: TimeSpan.FromSeconds(15),
            to: TimeSpan.FromHours(1));

        var timeProviderStub = new FakeTimeProvider();

        StationEntity offlineStation = randomizer.NextOnlineStationEntity() with
        {
            LastHeartbeat = timeProviderStub.GetUtcNow()
        };

        StationEntity onlineStation = randomizer.NextOnlineStationEntity() with
        {
            LastHeartbeat = timeProviderStub.GetUtcNow() + maxHeartbeatInterval
        };

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        
        stationsRepositoryMock.Setup(mock => mock
            .GetMultipleStationsAsync())
            .ReturnsAsync([offlineStation, onlineStation]);

        stationsRepositoryMock.Setup(mock => mock
            .UpdateStationAsync(
                id: offlineStation.Id,
                updateIpAddress: true,
                ipAddress: null,
                updateApiPort:true,
                apiPort: null,
                updateApiVersion: true,
                apiVersion: null,
                updateLastHeartbeat: false,
                lastHeartbeat: null))
            .ReturnsAsync(offlineStation with { IpAddress = null, ApiPort = null, ApiVersion = null });

        var switchesRepositoryStub = new Mock<ISwitchesRepository>();
        var loggerMock = new FakeLogger<HeartbeatMonitoringServiceProcessor>();

        var serviceProcessor = new HeartbeatMonitoringServiceProcessor(
            stationsRepositoryMock.Object,
            switchesRepositoryStub.Object,
            timeProviderStub,
            maxHeartbeatInterval,
            loggerMock);

        timeProviderStub.Advance(maxHeartbeatInterval + TimeSpan.FromSeconds(1));
        await serviceProcessor.ProcessAsync(CancellationToken.None);

        stationsRepositoryMock.Verify (mock => mock
            .UpdateStationAsync(
                id: offlineStation.Id,
                updateIpAddress: true,
                ipAddress: null,
                updateApiPort: true,
                apiPort: null,
                updateApiVersion: true,
                apiVersion: null,
                updateLastHeartbeat: false,
                lastHeartbeat: null),
            Times.Once);

        // Not using named arguments as update method for this station shall not be invoked at all.
        stationsRepositoryMock.Verify(mock => mock
            .UpdateStationAsync(
                onlineStation.Id,
                It.IsAny<bool>(),
                It.IsAny<IPAddress?>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<bool>(),
                It.IsAny<byte?>(),
                It.IsAny<bool>(),
                It.IsAny<DateTimeOffset?>()),
            Times.Never);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public async Task MarksSwitchesAsOfflineWhenParentStationHeartbeatTimeoutExceeded()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        TimeSpan maxHeartbeatInterval = randomizer.NextTimeSpan(
            from: TimeSpan.FromSeconds(15),
            to: TimeSpan.FromHours(1));

        var timeProviderStub = new FakeTimeProvider();

        StationEntity offlineStation = randomizer.NextOnlineStationEntity() with
        {
            LastHeartbeat = timeProviderStub.GetUtcNow()
        };

        bool offlineSwitchState = randomizer.NextBool();

        SwitchEntity offlineSwitch = randomizer.NextSwitchEntity() with
        {
            StationId = offlineStation.Id,
            ExpectedState = offlineSwitchState,
            ActualState = offlineSwitchState
        };

        StationEntity onlineStation = randomizer.NextOnlineStationEntity() with
        {
            LastHeartbeat = timeProviderStub.GetUtcNow() + maxHeartbeatInterval
        };

        bool onlineSwitchState = randomizer.NextBool();

        SwitchEntity onlineSwitch = randomizer.NextSwitchEntity() with
        {
            StationId = onlineStation.Id,
            ExpectedState = onlineSwitchState,
            ActualState = onlineSwitchState
        };

        var stationsRepositoryStub = new Mock<IStationsRepository>();

        stationsRepositoryStub.Setup(mock => mock
            .GetMultipleStationsAsync())
            .ReturnsAsync([offlineStation, onlineStation]);

        stationsRepositoryStub.Setup(mock => mock
            .UpdateStationAsync(
                id: offlineStation.Id,
                updateIpAddress: true,
                ipAddress: null,
                updateApiPort:true,
                apiPort: null,
                updateApiVersion: true,
                apiVersion: null,
                updateLastHeartbeat: false,
                lastHeartbeat: null))
            .ReturnsAsync(offlineStation with { IpAddress = null, ApiPort = null, ApiVersion = null });

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();

        switchesRepositoryMock.Setup(mock => mock
            .GetMultipleSwitchesAsync(
            filterByStationId: true,
            stationId: offlineStation.Id))
        .ReturnsAsync([offlineSwitch]);

        switchesRepositoryMock.Setup(mock => mock
            .UpdateSwitchAsync(
                id: offlineSwitch.Id,
                updateExpectedState: false,
                expectedState: null,
                updateActualState: true,
                actualState: null))
            .ReturnsAsync(offlineSwitch with { ActualState = null });

        var loggerMock = new FakeLogger<HeartbeatMonitoringServiceProcessor>();

        var serviceProcessor = new HeartbeatMonitoringServiceProcessor(
            stationsRepositoryStub.Object,
            switchesRepositoryMock.Object,
            timeProviderStub,
            maxHeartbeatInterval,
            loggerMock);

        timeProviderStub.Advance(maxHeartbeatInterval + TimeSpan.FromSeconds(1));
        await serviceProcessor.ProcessAsync(CancellationToken.None);

        switchesRepositoryMock.Verify(mock => mock
            .UpdateSwitchAsync(
                offlineSwitch.Id,
                updateExpectedState: false,
                expectedState: null,
                updateActualState: true,
                actualState: null),
            Times.Once);

        // Not using named arguments as update method for this station shall not be invoked at all.
        switchesRepositoryMock.Verify(mock => mock
           .UpdateSwitchAsync(
               onlineSwitch.Id,
               It.IsAny<bool>(),
               It.IsAny<bool?>(),
               It.IsAny<bool>(),
               It.IsAny<bool?>()),
           Times.Never);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }
    #endregion
}
