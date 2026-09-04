using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Api.Clients;
using SmartHome.Server.Api.Clients.Abstractions;
using SmartHome.Server.Repositories.Entities;
using SmartHome.Server.Tests.Utilities;

namespace SmartHome.Server.Tests.Api.Clients;

[Category("UnitTest")]
[TestOf(typeof(StationApiClientFactory))]
[Author("Jakub Miodunka")]
internal sealed class StationApiClientFactoryTests
{
    #region Constructor
    [Test]
    public void InstantiationPossible()
    {
        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var loggerFactoryStub = new Mock<ILoggerFactory>();

        Action actionUnderTest = () => new StationApiClientFactory(
            httpClientFactoryStub.Object,
            loggerFactoryStub.Object);

        Assert.DoesNotThrow(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsHttpClientFactory()
    {
        var loggerFactoryStub = new Mock<ILoggerFactory>();

        Action actionUnderTest = () => new StationApiClientFactory(
            null!,
            loggerFactoryStub.Object);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsLoggerFactory()
    {
        var httpClientFactoryStub = new Mock<IHttpClientFactory>();

        Action actionUnderTest = () => new StationApiClientFactory(
            httpClientFactoryStub.Object,
            null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }
    #endregion

    #region API client creation
    [Test]
    public void ApiClientCreationPossible()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var loggerFactoryStub = new Mock<ILoggerFactory>();

        loggerFactoryStub.Setup(factory => factory
            .CreateLogger(It.IsAny<string>()))
            .Returns((string categoryName) => new FakeLogger(new FakeLogCollector(), categoryName));

        var factoryUnderTest = new StationApiClientFactory(
            httpClientFactoryStub.Object,
            loggerFactoryStub.Object);

        StationEntity stationEntity = randomizer.NextOnlineStationEntity();
        TimeSpan timeout = randomizer.NextTimeSpan(from: StationApiClient.MinTimeout, to: StationApiClient.MaxTimeout);
        IStationApiClient apiClient = factoryUnderTest.CreateFor(stationEntity, timeout);

        Assert.That(apiClient, Is.Not.Null);
        Assert.That(apiClient, Is.InstanceOf<StationApiClient>());
    }
    #endregion
}
