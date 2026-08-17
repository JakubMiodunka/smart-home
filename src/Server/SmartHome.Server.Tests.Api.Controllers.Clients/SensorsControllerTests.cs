using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Api.Controllers.Clients;
using SmartHome.Server.Api.Controllers.Clients.Requests;
using SmartHome.Server.Api.Controllers.Clients.Responses;
using SmartHome.Server.Features.Managers.Abstractions;
using SmartHome.Server.Repositories.Abstractions;
using SmartHome.Server.Repositories.Entities;
using SmartHome.Server.Tests.Utilities;
using System.Net;

namespace SmartHome.Server.Tests.Api.Controllers.Clients;

[Category("UnitTest")]
[TestOf(typeof(SensorsController))]
[Author("Jakub Miodunka")]
internal sealed class SensorsControllerTests
{
    #region Constructor
    [Test]
    public void InstantiationPossible()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var sensorsRepositoryMock = new Mock<ISensorsRepository>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var sensorManagerFactoryStub = new Mock<ISensorManagerFactory>();
        var loggerStub = new FakeLogger<SensorsController>();

        Action actionUnderTest = () => new SensorsController(
            httpContextAccessorStub.Object,
            sensorsRepositoryMock.Object,
            stationsRepositoryMock.Object,
            sensorManagerFactoryStub.Object,
            loggerStub);

        Assert.DoesNotThrow(actionUnderTest);
        sensorsRepositoryMock.AssertNoContentModifications();
        stationsRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsHttpContextAccessor()
    {
        var sensorsRepositoryMock = new Mock<ISensorsRepository>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var sensorManagerFactoryStub = new Mock<ISensorManagerFactory>();
        var loggerStub = new FakeLogger<SensorsController>();

        Action actionUnderTest = () => new SensorsController(
            null!,
            sensorsRepositoryMock.Object,
            stationsRepositoryMock.Object,
            sensorManagerFactoryStub.Object,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
        sensorsRepositoryMock.AssertNoContentModifications();
        stationsRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsSensorsRepository()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var sensorManagerFactoryStub = new Mock<ISensorManagerFactory>();
        var loggerStub = new FakeLogger<SensorsController>();

        Action actionUnderTest = () => new SensorsController(
            httpContextAccessorStub.Object,
            null!,
            stationsRepositoryMock.Object,
            sensorManagerFactoryStub.Object,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
        stationsRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsStationsRepository()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var sensorsRepositoryMock = new Mock<ISensorsRepository>();
        var sensorManagerFactoryStub = new Mock<ISensorManagerFactory>();
        var loggerStub = new FakeLogger<SensorsController>();

        Action actionUnderTest = () => new SensorsController(
            httpContextAccessorStub.Object,
            sensorsRepositoryMock.Object,
            null!,
            sensorManagerFactoryStub.Object,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
        sensorsRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsSensorManagerFactory()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var sensorsRepositoryMock = new Mock<ISensorsRepository>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var loggerStub = new FakeLogger<SensorsController>();

        Action actionUnderTest = () => new SensorsController(
            httpContextAccessorStub.Object,
            sensorsRepositoryMock.Object,
            stationsRepositoryMock.Object,
            null!,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
        sensorsRepositoryMock.AssertNoContentModifications();
        stationsRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsLogger()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var sensorsRepositoryMock = new Mock<ISensorsRepository>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var sensorManagerFactoryStub = new Mock<ISensorManagerFactory>();

        Action actionUnderTest = () => new SensorsController(
            httpContextAccessorStub.Object,
            sensorsRepositoryMock.Object,
            stationsRepositoryMock.Object,
            sensorManagerFactoryStub.Object,
            null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
        sensorsRepositoryMock.AssertNoContentModifications();
        stationsRepositoryMock.AssertNoContentModifications();
    }
    #endregion

    #region Sensor retrival
    [Test]
    public async Task GettingSingleSensorPossible()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress clientIpAddress = randomizer.NextIpAddress();
        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(clientIpAddress);

        SensorEntity sensorEntity = randomizer.NextSensorEntity();

        var sensorsRepositoryMock = new Mock<ISensorsRepository>();

        sensorsRepositoryMock.Setup(mock => mock
            .GetSingleSensorAsync(
                filterById: true,
                id: sensorEntity.Id,
                filterByStationId: false,
                stationId: null,
                filterByLocalId: false,
                localId: null))
            .ReturnsAsync(sensorEntity);

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var sensorManagerFactoryStub = new Mock<ISensorManagerFactory>();
        var loggerMock = new FakeLogger<SensorsController>();

        var controllerUnderTest = new SensorsController(
            httpContextAccessorStub.Object,
            sensorsRepositoryMock.Object,
            stationsRepositoryMock.Object,
            sensorManagerFactoryStub.Object,
            loggerMock);

        IActionResult response = await controllerUnderTest.GetSensorAsync(sensorEntity.Id);
        response.AssertOkObjectResult(expectedValue: sensorEntity);

        sensorsRepositoryMock.AssertNoContentModifications();
        stationsRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public async Task GettingSingleSensorReturnsNotFoundIfSensorDoesNotExist()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress clientIpAddress = randomizer.NextIpAddress();
        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(clientIpAddress);

        SensorEntity sensorEntity = randomizer.NextSensorEntity();

        var sensorsRepositoryMock = new Mock<ISensorsRepository>();

        sensorsRepositoryMock.Setup(mock => mock
            .GetSingleSensorAsync(
                filterById: true,
                id: sensorEntity.Id,
                filterByStationId: false,
                stationId: null,
                filterByLocalId: false,
                localId: null))
            .ReturnsAsync(sensorEntity);

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var sensorManagerFactoryStub = new Mock<ISensorManagerFactory>();
        var loggerMock = new FakeLogger<SensorsController>();

        var controllerUnderTest = new SensorsController(
            httpContextAccessorStub.Object,
            sensorsRepositoryMock.Object,
            stationsRepositoryMock.Object,
            sensorManagerFactoryStub.Object,
            loggerMock);

        long nonExistingSensorEntityId = randomizer.NextInt64(1, long.MaxValue);
        while (sensorEntity.Id == nonExistingSensorEntityId)
        {
            nonExistingSensorEntityId = randomizer.NextInt64(1, long.MaxValue);
        }

        IActionResult response = await controllerUnderTest.GetSensorAsync(nonExistingSensorEntityId);
        response.AssertNotFoundResult();

        sensorsRepositoryMock.AssertNoContentModifications();
        stationsRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }

    [Test]
    public async Task GettingSingleSensorReturnsBadRequestIfClientIpAddressCannotBeDetermined()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(remoteIpAddress: null);

        SensorEntity sensorEntity = randomizer.NextSensorEntity();

        var sensorsRepositoryMock = new Mock<ISensorsRepository>();

        sensorsRepositoryMock.Setup(mock => mock
            .GetSingleSensorAsync(
                filterById: true,
                id: sensorEntity.Id,
                filterByStationId: false,
                stationId: null,
                filterByLocalId: false,
                localId: null))
            .ReturnsAsync(sensorEntity);

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var sensorManagerFactoryStub = new Mock<ISensorManagerFactory>();
        var loggerMock = new FakeLogger<SensorsController>();

        var controllerUnderTest = new SensorsController(
            httpContextAccessorStub.Object,
            sensorsRepositoryMock.Object,
            stationsRepositoryMock.Object,
            sensorManagerFactoryStub.Object,
            loggerMock);

        IActionResult response = await controllerUnderTest.GetSensorAsync(sensorEntity.Id);
        response.AssertBadRequestResult();

        sensorsRepositoryMock.AssertNoContentModifications();
        stationsRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }

    [TestCase(2)]
    public async Task GettingMultipleSensorsPossible(int sensorsInRepository)
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress clientIpAddress = randomizer.NextIpAddress();
        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(clientIpAddress);

        SensorEntity[] repositoryContent = Enumerable.Range(1, sensorsInRepository)
            .Select(id => randomizer.NextSensorEntity() with { Id = id })
            .ToArray();

        var sensorsRepositoryMock = new Mock<ISensorsRepository>();

        sensorsRepositoryMock.Setup(mock => mock
            .GetMultipleSensorsAsync())
            .ReturnsAsync(repositoryContent);

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var sensorManagerFactoryStub = new Mock<ISensorManagerFactory>();
        var loggerMock = new FakeLogger<SensorsController>();

        var controllerUnderTest = new SensorsController(
            httpContextAccessorStub.Object,
            sensorsRepositoryMock.Object,
            stationsRepositoryMock.Object,
            sensorManagerFactoryStub.Object,
            loggerMock);

        IActionResult response = await controllerUnderTest.GetSensorsAsync();
        response.AssertOkObjectResult(expectedValue: repositoryContent);

        sensorsRepositoryMock.AssertNoContentModifications();
        stationsRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));

    }

    [Test]
    public async Task GettingMultipleSensorsReturnsBadRequestIfClientIpAddressCannotBeDetermined()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(remoteIpAddress: null);

        var sensorsRepositoryMock = new Mock<ISensorsRepository>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var sensorManagerFactoryStub = new Mock<ISensorManagerFactory>();
        var loggerMock = new FakeLogger<SensorsController>();

        var controllerUnderTest = new SensorsController(
            httpContextAccessorStub.Object,
            sensorsRepositoryMock.Object,
            stationsRepositoryMock.Object,
            sensorManagerFactoryStub.Object,
            loggerMock);

        IActionResult response = await controllerUnderTest.GetSensorsAsync();
        response.AssertBadRequestResult();

        sensorsRepositoryMock.AssertNoContentModifications();
        stationsRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }
    #endregion

    #region Taking measurements
    [Test]
    public async Task TakingMeasurementPossible()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress clientIpAddress = randomizer.NextIpAddress();
        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(clientIpAddress);

        SensorEntity sensorEntity = randomizer.NextSensorEntity();

        var sensorsRepositoryMock = new Mock<ISensorsRepository>();

        sensorsRepositoryMock.Setup(mock => mock
            .GetSingleSensorAsync(
                filterById: true,
                id: sensorEntity.Id,
                filterByStationId: false,
                stationId: null,
                filterByLocalId: false,
                localId: null))
            .ReturnsAsync(sensorEntity);

        StationEntity parentStation = randomizer.NextOnlineStationEntity() with
        {
            Id = sensorEntity.StationId
        };

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        stationsRepositoryMock.Setup(mock => mock
            .GetSingleStationAsync(
               filterById: true,
                id: parentStation.Id,
                filterByIpAddress: false,
                ipAddress: null,
                filterByMacAddress: false,
                macAddress: null))
            .ReturnsAsync(parentStation);

        double expectedMeasurementValue = randomizer.NextDouble();
        var sensorManagerMock = new Mock<ISensorManager>();

        sensorManagerMock.Setup(mock =>
            mock.TryGetMeasurementAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedMeasurementValue);

        var sensorManagerFactoryStub = new Mock<ISensorManagerFactory>();

        sensorManagerFactoryStub.Setup(mock => mock
            .CreateFor(sensorEntity, parentStation))
            .Returns(sensorManagerMock.Object);

        var loggerMock = new FakeLogger<SensorsController>();

        var controllerUnderTest = new SensorsController(
            httpContextAccessorStub.Object,
            sensorsRepositoryMock.Object,
            stationsRepositoryMock.Object,
            sensorManagerFactoryStub.Object,
            loggerMock);

        IActionResult response = await controllerUnderTest.GetMeasurementAsync(sensorEntity.Id, CancellationToken.None);

        var expectedResponse = new GetMeasurementResponse(expectedMeasurementValue);
        response.AssertOkObjectResult(expectedValue: expectedResponse);

        sensorsRepositoryMock.AssertNoContentModifications();
        stationsRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Information));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public async Task TakingMeasurementReturnsBadRequestIfClientIpAddressCannotBeDetermined()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(remoteIpAddress: null);

        var sensorsRepositoryMock = new Mock<ISensorsRepository>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var sensorManagerFactoryStub = new Mock<ISensorManagerFactory>();
        var loggerMock = new FakeLogger<SensorsController>();

        var controllerUnderTest = new SensorsController(
            httpContextAccessorStub.Object,
            sensorsRepositoryMock.Object,
            stationsRepositoryMock.Object,
            sensorManagerFactoryStub.Object,
            loggerMock);

        SensorEntity sensorntity = randomizer.NextSensorEntity();

        IActionResult response = await controllerUnderTest.GetMeasurementAsync(sensorntity.Id, CancellationToken.None);
        response.AssertBadRequestResult();

        sensorsRepositoryMock.AssertNoContentModifications();
        stationsRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }

    [Test]
    public async Task TakingMeasurementReturnsNotFoundIfSensorDoesNotExist()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress clientIpAddress = randomizer.NextIpAddress();
        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(clientIpAddress);

        var sensorsRepositoryMock = new Mock<ISensorsRepository>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        double expectedMeasurementValue = randomizer.NextDouble();
        var sensorManagerStub = new Mock<ISensorManager>();
        var sensorManagerFactoryStub = new Mock<ISensorManagerFactory>();
        var loggerMock = new FakeLogger<SensorsController>();

        var controllerUnderTest = new SensorsController(
            httpContextAccessorStub.Object,
            sensorsRepositoryMock.Object,
            stationsRepositoryMock.Object,
            sensorManagerFactoryStub.Object,
            loggerMock);

        SensorEntity nonExistingSensor = randomizer.NextSensorEntity();

        IActionResult response = await controllerUnderTest.GetMeasurementAsync(nonExistingSensor.Id, CancellationToken.None);
        response.AssertNotFoundResult();

        sensorsRepositoryMock.AssertNoContentModifications();
        stationsRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }

    [Test]
    public async Task TakingMeasurementReturnsInternalServerErrorIfParentStationDoesNotExist()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress clientIpAddress = randomizer.NextIpAddress();
        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(clientIpAddress);

        SensorEntity sensorEntity = randomizer.NextSensorEntity();

        var sensorsRepositoryMock = new Mock<ISensorsRepository>();

        sensorsRepositoryMock.Setup(mock => mock
            .GetSingleSensorAsync(
                filterById: true,
                id: sensorEntity.Id,
                filterByStationId: false,
                stationId: null,
                filterByLocalId: false,
                localId: null))
            .ReturnsAsync(sensorEntity);

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var sensorManagerFactoryStub = new Mock<ISensorManagerFactory>();
        var loggerMock = new FakeLogger<SensorsController>();

        var controllerUnderTest = new SensorsController(
            httpContextAccessorStub.Object,
            sensorsRepositoryMock.Object,
            stationsRepositoryMock.Object,
            sensorManagerFactoryStub.Object,
            loggerMock);

        IActionResult response = await controllerUnderTest.GetMeasurementAsync(sensorEntity.Id, CancellationToken.None);
        response.AssertStatusCodeResult(StatusCodes.Status500InternalServerError);

        sensorsRepositoryMock.AssertNoContentModifications();
        stationsRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Error));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Error < record.Level));
    }

    [Test]
    public async Task TakingMeasurementReturnsServiceUnavailableIfCannotTakeMeasurement()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress clientIpAddress = randomizer.NextIpAddress();
        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(clientIpAddress);

        SensorEntity sensorEntity = randomizer.NextSensorEntity();

        var sensorsRepositoryMock = new Mock<ISensorsRepository>();

        sensorsRepositoryMock.Setup(mock => mock
            .GetSingleSensorAsync(
                filterById: true,
                id: sensorEntity.Id,
                filterByStationId: false,
                stationId: null,
                filterByLocalId: false,
                localId: null))
            .ReturnsAsync(sensorEntity);

        StationEntity parentStation = randomizer.NextOnlineStationEntity() with
        {
            Id = sensorEntity.StationId
        };

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        stationsRepositoryMock.Setup(mock => mock
            .GetSingleStationAsync(
               filterById: true,
                id: parentStation.Id,
                filterByIpAddress: false,
                ipAddress: null,
                filterByMacAddress: false,
                macAddress: null))
            .ReturnsAsync(parentStation);

        var sensorManagerMock = new Mock<ISensorManager>();

        sensorManagerMock.Setup(mock =>
            mock.TryGetMeasurementAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as double?);

        var sensorManagerFactoryStub = new Mock<ISensorManagerFactory>();

        sensorManagerFactoryStub.Setup(mock => mock
            .CreateFor(sensorEntity, parentStation))
            .Returns(sensorManagerMock.Object);

        var loggerMock = new FakeLogger<SensorsController>();

        var controllerUnderTest = new SensorsController(
            httpContextAccessorStub.Object,
            sensorsRepositoryMock.Object,
            stationsRepositoryMock.Object,
            sensorManagerFactoryStub.Object,
            loggerMock);

        IActionResult response = await controllerUnderTest.GetMeasurementAsync(sensorEntity.Id, CancellationToken.None);
        response.AssertStatusCodeResult(StatusCodes.Status503ServiceUnavailable);

        sensorsRepositoryMock.AssertNoContentModifications();
        stationsRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Information));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }
    #endregion
}
