using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Api.Clients;
using SmartHome.Server.Api.Clients.Abstractions;
using SmartHome.Server.Features.Managers;
using SmartHome.Server.Features.Managers.Responses;
using SmartHome.Server.Repositories.Entities;
using SmartHome.Server.Tests.Utilities;
using System.Net;
using System.Net.Http.Json;

namespace SmartHome.Server.Tests.Features.Managers;


[Category("UnitTest")]
[TestOf(typeof(SensorManager))]
[Author("Jakub Miodunka")]
internal sealed class SensorManagerTests
{
    #region Test utilities
    private static Uri GetSensorUrl(SensorEntity sensorEntity, StationEntity parentStation) =>
        parentStation.ApiVersion switch
        {
            1 => new Uri($"http://{parentStation.IpAddress}:{parentStation.ApiPort}/api/v1/sensors/{sensorEntity.LocalId}"),
            _ => throw new NotSupportedException($"Station API version {parentStation.ApiVersion} is not supported.")
        };
    #endregion

    #region Constructor
    [Test]
    public void InstantiationPossible()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        SensorEntity sensorEntity = randomizer.NextSensorEntity();
        StationEntity stationEntity = randomizer.NextOnlineStationEntity() with
        {
            Id = sensorEntity.StationId
        };

        var stationApiClientFactory = new Mock<IStationApiClientFactory>();
        var loggerStub = new FakeLogger<SensorManager>();

        SensorManager? managerUnderTest = null;
        Action actionUnderTest = () => managerUnderTest =
            new SensorManager(
                sensorEntity,
                stationEntity,
                stationApiClientFactory.Object,
                loggerStub);

        Assert.DoesNotThrow(actionUnderTest);

        Assert.That(managerUnderTest, Is.Not.Null);
        Assert.That(managerUnderTest.ManagedSensor, Is.EqualTo(sensorEntity));
        Assert.That(managerUnderTest.ParentStation, Is.EqualTo(stationEntity));
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsManagedSensor()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextOnlineStationEntity();

        var stationApiClientFactory = new Mock<IStationApiClientFactory>();
        var loggerStub = new FakeLogger<SensorManager>();

        Action actionUnderTest = () => new SensorManager(
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

        SensorEntity sensorEntity = randomizer.NextSensorEntity();

        var stationApiClientFactory = new Mock<IStationApiClientFactory>();
        var loggerStub = new FakeLogger<SensorManager>();

        Action actionUnderTest = () => new SensorManager(
            sensorEntity,
            null!,
            stationApiClientFactory.Object,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsStationApiClientFactory()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        SensorEntity sensorEntity = randomizer.NextSensorEntity();
        StationEntity stationEntity = randomizer.NextOnlineStationEntity() with
        {
            Id = sensorEntity.StationId
        };

        var loggerStub = new FakeLogger<SensorManager>();

        Action actionUnderTest = () => new SensorManager(
            sensorEntity,
            stationEntity,
            null!,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsLogger()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        SensorEntity sensorEntity = randomizer.NextSensorEntity();
        StationEntity stationEntity = randomizer.NextOnlineStationEntity() with
        {
            Id = sensorEntity.StationId
        };

        var stationApiClientFactory = new Mock<IStationApiClientFactory>();

        Action actionUnderTest = () => new SensorManager(
            sensorEntity,
            stationEntity,
            stationApiClientFactory.Object,
            null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingStationWhichIsNotSensorParentStation()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        SensorEntity sensorEntity = randomizer.NextSensorEntity();
        StationEntity stationEntity = randomizer.NextOnlineStationEntity();
        
        while (stationEntity.Id == sensorEntity.StationId)
        {
            stationEntity = stationEntity with
            {
                Id = randomizer.NextInt64(1, long.MaxValue)
            };
        }

        var stationApiClientFactory = new Mock<IStationApiClientFactory>();
        var loggerStub = new FakeLogger<SensorManager>();

        Action actionUnderTest = () => new SensorManager(
            sensorEntity,
            stationEntity,
            stationApiClientFactory.Object,
            loggerStub);

        Assert.Throws<ArgumentException>(actionUnderTest);
    }
    #endregion

    #region Taking measurement
    [TestCase(1, "GET", HttpStatusCode.OK)]
    public async Task TakingMeasurementPossible(
        byte stationApiVersion,
        string expectedHttpMethodName,
        HttpStatusCode expectedStatusCode)
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        SensorEntity sensorEntity = randomizer.NextSensorEntity();
        StationEntity parentStation = randomizer.NextOnlineStationEntity() with
        {
            Id = sensorEntity.StationId,
            ApiVersion = stationApiVersion
        };

        HttpMethod expectedHttpMethod = HttpTestUtilities.GetHttpMethodFromName(expectedHttpMethodName);
        Uri expectedEndpointUrl = GetSensorUrl(sensorEntity, parentStation);

        double expectedMeasurementValue = randomizer.NextDouble();
        var response = new GetMeasurementResponse(expectedMeasurementValue);

        var stationApiClientMock = new Mock<IStationApiClient>();
        stationApiClientMock.Setup(mock => mock
            .TrySendRequestAsync<GetMeasurementResponse>(
                expectedEndpointUrl,
                expectedHttpMethod,
                null,
                expectedStatusCode,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var stationApiClientFactoryStub = new Mock<IStationApiClientFactory>();
        stationApiClientFactoryStub.Setup(mock => mock
            .CreateFor(parentStation, It.IsAny<TimeSpan>()))
            .Returns(stationApiClientMock.Object);

        var loggerMock = new FakeLogger<SensorManager>();

        var managerUnderTest = new SensorManager(
            sensorEntity,
            parentStation,
            stationApiClientFactoryStub.Object,
            loggerMock);

        double? actualMeasurementValue = await managerUnderTest.TryGetMeasurementAsync(CancellationToken.None);

        Assert.That(actualMeasurementValue, Is.EqualTo(expectedMeasurementValue));

        stationApiClientMock.Verify(client => client
            .TrySendRequestAsync<GetMeasurementResponse>(
                expectedEndpointUrl,
                expectedHttpMethod,
                null,
                expectedStatusCode,
                It.IsAny<CancellationToken>()),
            Times.Once);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Information));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public async Task TakingMeasurementFailsIfParentStationIsOffline()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        SensorEntity sensorEntity = randomizer.NextSensorEntity();
        StationEntity stationEntity = randomizer.NextOfflineStationEntity() with
        {
            Id = sensorEntity.StationId
        };

        var stationApiClientMock = new Mock<IStationApiClient>();

        var stationApiClientFactoryStub = new Mock<IStationApiClientFactory>();
        stationApiClientFactoryStub.Setup(mock => mock
            .CreateFor(stationEntity, It.IsAny<TimeSpan>()))
            .Returns(stationApiClientMock.Object);

        var loggerMock = new FakeLogger<SensorManager>();

        var managerUnderTest = new SensorManager(
            sensorEntity,
            stationEntity,
            stationApiClientFactoryStub.Object,
            loggerMock);

        double? actualMeasurementValue = await managerUnderTest.TryGetMeasurementAsync(CancellationToken.None);

        Assert.That(actualMeasurementValue, Is.Null);

        stationApiClientMock.Verify(client => client
            .TrySendRequestAsync<GetMeasurementResponse>(
                It.IsAny<Uri>(),
                It.IsAny<HttpMethod>(),
                It.IsAny<object?>(),
                It.IsAny<HttpStatusCode>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }

    [TestCase(1, "GET")]
    public async Task TakingMeasurementFailsIfRequestSendingFails(
        byte stationApiVersion,
        string expectedHttpMethodName)
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        SensorEntity sensorEntity = randomizer.NextSensorEntity();
        StationEntity stationEntity = randomizer.NextOnlineStationEntity() with
        {
            Id = sensorEntity.StationId,
            ApiVersion = stationApiVersion
        };

        var stationApiClientMock = new Mock<IStationApiClient>();

        var stationApiClientFactoryStub = new Mock<IStationApiClientFactory>();
        stationApiClientFactoryStub.Setup(mock => mock
            .CreateFor(stationEntity, It.IsAny<TimeSpan>()))
            .Returns(stationApiClientMock.Object);

        var loggerMock = new FakeLogger<SensorManager>();

        var managerUnderTest = new SensorManager(
            sensorEntity,
            stationEntity,
            stationApiClientFactoryStub.Object,
            loggerMock);

        double? actualMeasurementValue = await managerUnderTest.TryGetMeasurementAsync(CancellationToken.None);
        
        Assert.That(actualMeasurementValue, Is.Null);

        Uri expectedEndpointUrl = GetSensorUrl(sensorEntity, stationEntity);

        stationApiClientMock.Verify(client => client
            .TrySendRequestAsync<GetMeasurementResponse>(
                It.IsAny<Uri>(),
                It.IsAny<HttpMethod>(),
                It.IsAny<object?>(),
                It.IsAny<HttpStatusCode>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }
    #endregion
}