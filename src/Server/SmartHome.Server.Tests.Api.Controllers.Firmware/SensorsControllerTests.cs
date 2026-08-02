using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Api.Controllers.Firmware;
using SmartHome.Server.Api.Controllers.Firmware.Requests;
using SmartHome.Server.Api.Controllers.Firmware.Responses;
using SmartHome.Server.Repositories.Abstractions;
using SmartHome.Server.Repositories.Entities;
using SmartHome.Server.Tests.Utilities;
using System.Net;
using System.Net.NetworkInformation;

namespace SmartHome.Server.Tests.Api.Controllers.Firmware;

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
        var loggerStub = new FakeLogger<SensorsController>();

        Action actionUnderTest = () => new SensorsController(
            httpContextAccessorStub.Object,
            sensorsRepositoryMock.Object,
            stationsRepositoryMock.Object,
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
        var loggerStub = new FakeLogger<SensorsController>();

        Action actionUnderTest = () => new SensorsController(
            null!,
            sensorsRepositoryMock.Object,
            stationsRepositoryMock.Object,
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
        var loggerStub = new FakeLogger<SensorsController>();

        Action actionUnderTest = () => new SensorsController(
            httpContextAccessorStub.Object,
            null!,
            stationsRepositoryMock.Object,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);

        stationsRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsStationsRepository()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var sensorsRepositoryMock = new Mock<ISensorsRepository>();
        var loggerStub = new FakeLogger<SensorsController>();

        Action actionUnderTest = () => new SensorsController(
            httpContextAccessorStub.Object,
            sensorsRepositoryMock.Object,
            null!,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);

        sensorsRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsLogger()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var sensorsRepositoryMock = new Mock<ISensorsRepository>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();

        Action actionUnderTest = () => new SensorsController(
            httpContextAccessorStub.Object,
            sensorsRepositoryMock.Object,
            stationsRepositoryMock.Object,
            null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);

        sensorsRepositoryMock.AssertNoContentModifications();
        stationsRepositoryMock.AssertNoContentModifications();
    }
    #endregion

    #region Registration
    [Test]
    public async Task RegistrationOfUnknownSensorCausesCreationOfNewSwitchEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity parentStationEntity = randomizer.NextOnlineStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(parentStationEntity.IpAddress);

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        stationsRepositoryMock.Setup(mock =>
            mock.GetSingleStationAsync(
                filterById: It.IsAny<bool>(),
                id: It.IsAny<long?>(),
                filterByIpAddress: true,
                ipAddress: parentStationEntity.IpAddress,
                filterByMacAddress: It.IsAny<bool>(),
                macAddress: It.IsAny<PhysicalAddress?>()))
            .ReturnsAsync(parentStationEntity);

        SensorEntity sensorEntity = randomizer.NextSensorEntity() with
        {
            StationId = parentStationEntity.Id
        };

        var sensorsRepositoryMock = new Mock<ISensorsRepository>();

        sensorsRepositoryMock.Setup(mock => mock
            .CreateSensorAsync(
                sensorEntity.StationId,
                sensorEntity.LocalId,
                sensorEntity.MeasurementType))
            .ReturnsAsync(sensorEntity);


        var loggerMock = new FakeLogger<SensorsController>();

        var controllerUnderTest = new SensorsController(
            httpContextAccessorStub.Object,
            sensorsRepositoryMock.Object,
            stationsRepositoryMock.Object,
            loggerMock);

        var request = new SensorRegistrationRequest(sensorEntity.LocalId, sensorEntity.MeasurementType);
        IActionResult response = await controllerUnderTest.RegisterSensor(request);

        var expectedResponse = new SensorRegistrationResponse(sensorEntity.Id);
        response.AssertOkObjectResult(expectedValue: expectedResponse);

        sensorsRepositoryMock.Verify(mock => mock
            .CreateSensorAsync(
                sensorEntity.StationId,
                sensorEntity.LocalId,
                sensorEntity.MeasurementType),
            Times.Once);

        stationsRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Information));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public async Task RegistrationOfKnownSwitchCausesReturnOfItsExpectedState() =>
        throw new NotImplementedException();

    [Test]
    public async Task RegistrationReturnsBadRequestIfStationIpAddressCannotBeDetermined() =>
        throw new NotImplementedException();

    [Test]
    public async Task RegistrationReturnsNotFoundIfStationIsNotRegistered() =>
        throw new NotImplementedException();
    #endregion
}
