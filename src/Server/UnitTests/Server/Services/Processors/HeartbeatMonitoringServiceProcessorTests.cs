using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Repositories;
using SmartHome.Server.Services.Processors;

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

        var stationsRepositoryStub = new Mock<IStationsRepository>();
        var switchesRepositoryStub = new Mock<ISwitchesRepository>();
        var timeProviderStub = new FakeTimeProvider();
        var loggerStub = new FakeLogger<HeartbeatMonitoringServiceProcessor>();

        TestDelegate actionUnderTest = () => new HeartbeatMonitoringServiceProcessor(
            stationsRepositoryStub.Object,
            switchesRepositoryStub.Object,
            timeProviderStub,
            maxHeartbeatInterval,
            loggerStub);

        Assert.DoesNotThrow(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsStationsRepository()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        TimeSpan maxHeartbeatInterval = randomizer.NextTimeSpan(
            from: TimeSpan.FromMicroseconds(1));

        var switchesRepositoryStub = new Mock<ISwitchesRepository>();
        var timeProviderStub = new FakeTimeProvider();
        var loggerStub = new FakeLogger<HeartbeatMonitoringServiceProcessor>();

        TestDelegate actionUnderTest = () => new HeartbeatMonitoringServiceProcessor(
            null!,
            switchesRepositoryStub.Object,
            timeProviderStub,
            maxHeartbeatInterval,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsSwitchesRepository()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        TimeSpan maxHeartbeatInterval = randomizer.NextTimeSpan(
            from: TimeSpan.FromMicroseconds(1));

        var stationsRepositoryStub = new Mock<IStationsRepository>();
        var timeProviderStub = new FakeTimeProvider();
        var loggerStub = new FakeLogger<HeartbeatMonitoringServiceProcessor>();

        TestDelegate actionUnderTest = () => new HeartbeatMonitoringServiceProcessor(
            stationsRepositoryStub.Object,
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

        var stationsRepositoryStub = new Mock<IStationsRepository>();
        var switchesRepositoryStub = new Mock<ISwitchesRepository>();
        var loggerStub = new FakeLogger<HeartbeatMonitoringServiceProcessor>();

        TestDelegate actionUnderTest = () => new HeartbeatMonitoringServiceProcessor(
            stationsRepositoryStub.Object,
            switchesRepositoryStub.Object,
            null!,
            maxHeartbeatInterval,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsLogger()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        TimeSpan maxHeartbeatInterval = randomizer.NextTimeSpan(
            from: TimeSpan.FromMicroseconds(1));

        var stationsRepositoryStub = new Mock<IStationsRepository>();
        var switchesRepositoryStub = new Mock<ISwitchesRepository>();
        var timeProviderStub = new FakeTimeProvider();

        TestDelegate actionUnderTest = () => new HeartbeatMonitoringServiceProcessor(
            stationsRepositoryStub.Object,
            switchesRepositoryStub.Object,
            timeProviderStub,
            maxHeartbeatInterval,
            null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingInvalidMaxHeartbeatInterval(
        [Values(-1, 0)] long invalidMaxHeartbeatInterval)   // Given in microseconds.
    {
        var maxHeartbeatInterval = TimeSpan.FromMicroseconds(invalidMaxHeartbeatInterval);

        var stationsRepositoryStub = new Mock<IStationsRepository>();
        var switchesRepositoryStub = new Mock<ISwitchesRepository>();
        var timeProviderStub = new FakeTimeProvider();

        TestDelegate actionUnderTest = () => new HeartbeatMonitoringServiceProcessor(
            stationsRepositoryStub.Object,
            switchesRepositoryStub.Object,
            timeProviderStub,
            maxHeartbeatInterval,
            null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }
    #endregion

    #region Service execution
    // TODO: Add additional test cases.
    #endregion
}
