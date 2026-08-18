using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moq;
using NUnit.Framework.Internal;
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
        var responseBody = new GetMeasurementResponse(expectedMeasurementValue);
        using var response = new HttpResponseMessage(expectedStatusCode)
        {
            Content = JsonContent.Create(responseBody)
        };

        var stationApiClientMock = new Mock<IStationApiClient>();
        stationApiClientMock.Setup(mock => mock
            .SendRequestAsync(
                expectedEndpointUrl,
                expectedHttpMethod,
                null,
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
            .SendRequestAsync(
                expectedEndpointUrl,
                expectedHttpMethod,
                null,
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
            .SendRequestAsync(
                expectedEndpointUrl,
                HttpTestUtilities.GetHttpMethodFromName(expectedHttpMethodName),
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }

    [TestCase(1, "GET", HttpStatusCode.OK)]
    public async Task TakingMeasurementFailsIfStationReturnsInvalidStatusCode(
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

        HttpStatusCode invalidStatusCode = randomizer.NextSuccessfulHttpStatusCode();
        while (invalidStatusCode == expectedStatusCode)
        {
            invalidStatusCode = randomizer.NextSuccessfulHttpStatusCode();
        }

        HttpMethod expectedHttpMethod = HttpTestUtilities.GetHttpMethodFromName(expectedHttpMethodName);
        Uri expectedEndpointUrl = GetSensorUrl(sensorEntity, parentStation);

        double expectedMeasurementValue = randomizer.NextDouble();
        var responseBody = new GetMeasurementResponse(expectedMeasurementValue);
        using var response = new HttpResponseMessage(invalidStatusCode)
        {
            Content = JsonContent.Create(responseBody)
        };

        var stationApiClientMock = new Mock<IStationApiClient>();
        stationApiClientMock.Setup(mock => mock
            .SendRequestAsync(
                expectedEndpointUrl,
                expectedHttpMethod,
                null,
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

        Assert.That(actualMeasurementValue, Is.Null);

        stationApiClientMock.Verify(client => client
            .SendRequestAsync(
                expectedEndpointUrl,
                expectedHttpMethod,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Error));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Error < record.Level));
    }

    private static IEnumerable<TestCaseData> InvalidResponseBodyTestCaseParameters()
    {
        yield return new TestCaseData(
            1,
            "GET",
            HttpStatusCode.OK,
            null);

        yield return new TestCaseData(
            1,
            "GET",
            HttpStatusCode.OK,
            JsonContent.Create(new { Message = "This is invalid response body." }));
    }

    [Test]
    [TestCaseSource(nameof(InvalidResponseBodyTestCaseParameters))]
    public async Task SwitchStateChangeFailsIfStationResponseHaveInvalidBody(
        byte stationApiVersion,
        string expectedHttpMethodName,
        HttpStatusCode expectedStatusCode,
        HttpContent? invalidResponseContent)
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

        using var response = new HttpResponseMessage(expectedStatusCode)
        {
            Content = invalidResponseContent
        };

        var stationApiClientMock = new Mock<IStationApiClient>();
        stationApiClientMock.Setup(mock => mock
            .SendRequestAsync(
                expectedEndpointUrl,
                expectedHttpMethod,
                null,
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

        Assert.That(actualMeasurementValue, Is.Null);

        stationApiClientMock.Verify(client => client
            .SendRequestAsync(
                expectedEndpointUrl,
                expectedHttpMethod,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Error));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Error < record.Level));
    }

    [TestCase(1, "GET", HttpStatusCode.OK)]
    public async Task SwitchStateChangeFailsIfStationResponsHaveEmptyBody(
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
        /*
         * By default content of newly created message is set to instance of EmptyContent type,
         * which is private nested class defined in System.Net.Http.HttpResponseMessage.
         * It indicates that the response body is empty.
         * Only way to force the response to have empty body is to not set it,
         * which is the reason why sepparate test case needs to be created for this scenario.
         */
        using var response = new HttpResponseMessage(expectedStatusCode);

        var stationApiClientMock = new Mock<IStationApiClient>();
        stationApiClientMock.Setup(mock => mock
            .SendRequestAsync(
                expectedEndpointUrl,
                expectedHttpMethod,
                null,
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

        Assert.That(actualMeasurementValue, Is.Null);

        stationApiClientMock.Verify(client => client
            .SendRequestAsync(
                expectedEndpointUrl,
                expectedHttpMethod,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Error));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Error < record.Level));
    }
    #endregion
}