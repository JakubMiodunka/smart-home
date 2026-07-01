using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Repositories.Abstractions;
using SmartHome.Server.Services.Processors;
using SmartHome.Server.Tests.Utilities;
using System.Net;
using System.Net.NetworkInformation;

namespace SmartHome.Server.Tests.Services.Processors;

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
        var timeProviderStub = new FakeTimeProvider();
        var loggerStub = new FakeLogger<HeartbeatMonitoringServiceProcessor>();

        TestDelegate actionUnderTest = () => new HeartbeatMonitoringServiceProcessor(
            stationsRepositoryMock.Object,
            timeProviderStub,
            maxHeartbeatInterval,
            loggerStub);

        Assert.DoesNotThrow(actionUnderTest);

        stationsRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsStationsRepository()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        TimeSpan maxHeartbeatInterval = randomizer.NextTimeSpan(
            from: TimeSpan.FromMicroseconds(1));

        var timeProviderStub = new FakeTimeProvider();
        var loggerStub = new FakeLogger<HeartbeatMonitoringServiceProcessor>();

        TestDelegate actionUnderTest = () => new HeartbeatMonitoringServiceProcessor(
            null!,
            timeProviderStub,
            maxHeartbeatInterval,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsTimeProvider()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        TimeSpan maxHeartbeatInterval = randomizer.NextTimeSpan(
            from: TimeSpan.FromMicroseconds(1));

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var loggerStub = new FakeLogger<HeartbeatMonitoringServiceProcessor>();

        TestDelegate actionUnderTest = () => new HeartbeatMonitoringServiceProcessor(
            stationsRepositoryMock.Object,
            null!,
            maxHeartbeatInterval,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);

        stationsRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsLogger()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        TimeSpan maxHeartbeatInterval = randomizer.NextTimeSpan(
            from: TimeSpan.FromMicroseconds(1));

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var timeProviderStub = new FakeTimeProvider();

        TestDelegate actionUnderTest = () => new HeartbeatMonitoringServiceProcessor(
            stationsRepositoryMock.Object,
            timeProviderStub,
            maxHeartbeatInterval,
            null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);

        stationsRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingInvalidMaxHeartbeatInterval(
        [Values(-1, 0)] long invalidMaxHeartbeatInterval)   // Given in microseconds.
    {
        var maxHeartbeatInterval = TimeSpan.FromMicroseconds(invalidMaxHeartbeatInterval);

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var timeProviderStub = new FakeTimeProvider();

        TestDelegate actionUnderTest = () => new HeartbeatMonitoringServiceProcessor(
            stationsRepositoryMock.Object,
            timeProviderStub,
            maxHeartbeatInterval,
            null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);

        stationsRepositoryMock.AssertNoContentModifications();
    }
    #endregion

    #region Service execution
    [Test]
    public async Task ProcessorInvokesSqlProcedure()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var timeProviderStub = new FakeTimeProvider();

        TimeSpan maxHeartbeatInterval = randomizer.NextTimeSpan(
            from: TimeSpan.FromSeconds(15),
            to: TimeSpan.FromHours(1));
             
        DateTimeOffset minHeartbeatTimestamp = timeProviderStub.GetUtcNow() - maxHeartbeatInterval;

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        
        stationsRepositoryMock.Setup(mock => mock
            .MarkOfflineStations(minHeartbeatTimestamp))
            .ReturnsAsync(Array.Empty<long>());

        var loggerMock = new FakeLogger<HeartbeatMonitoringServiceProcessor>();

        var serviceProcessor = new HeartbeatMonitoringServiceProcessor(
            stationsRepositoryMock.Object,
            timeProviderStub,
            maxHeartbeatInterval,
            loggerMock);

        await serviceProcessor.ProcessAsync(CancellationToken.None);

        stationsRepositoryMock.Verify(mock => mock
            .MarkOfflineStations(minHeartbeatTimestamp),
            Times.Once);

        stationsRepositoryMock.Verify(mock => mock
            .CreateStationAsync(
            It.IsAny<PhysicalAddress>(),
            It.IsAny<IPAddress?>(),
            It.IsAny<int?>(),
            It.IsAny<byte?>(),
            It.IsAny<DateTimeOffset>()),
            Times.Never);

        stationsRepositoryMock.Verify(mock => mock
            .UpdateStationAsync(
                It.IsAny<long>(),
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
    #endregion
}
