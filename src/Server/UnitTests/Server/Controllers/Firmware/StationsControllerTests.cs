using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Controllers.Firmware;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Models.Requests;
using SmartHome.Server.Data.Repositories;
using SmartHome.UnitTests;
using System.Net;
using System.Net.NetworkInformation;

namespace UnitTests.Server.Controllers.Firmware;

[Category("UnitTest")]
[TestOf(typeof(StationsController))]
[Author("Jakub Miodunka")]
public sealed class StationsControllerTests
{
    #region Constructor
    [Test]
    public void InstantiationPossible()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var timeProviderStub = new FakeTimeProvider();
        var loggerStub = new FakeLogger<StationsController>();

        TestDelegate actionUnderTest = () => new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object,
            timeProviderStub,
            loggerStub);

        Assert.DoesNotThrow(actionUnderTest);
        stationsRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsHttpContextAccessor()
    {
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var timeProviderStub = new FakeTimeProvider();
        var loggerStub = new FakeLogger<StationsController>();

        TestDelegate actionUnderTest = () => new StationsController(
            null!,
            stationsRepositoryMock.Object,
            timeProviderStub,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
        stationsRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsStationsRepository()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var timeProviderStub = new FakeTimeProvider();
        var loggerStub = new FakeLogger<StationsController>();

        TestDelegate actionUnderTest = () => new StationsController(
            httpContextAccessorStub.Object,
            null!,
            timeProviderStub,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsTimeProvider()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var loggerStub = new FakeLogger<StationsController>();

        TestDelegate actionUnderTest = () => new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object,
            null!,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
        stationsRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsLogger()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var timeProviderStub = new FakeTimeProvider();

        TestDelegate actionUnderTest = () => new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object,
            timeProviderStub,
            null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
        stationsRepositoryMock.AssertNoContentModifications();
    }
    #endregion

    #region Registration
    [Test]
    public async Task RegistrationOfUnknownStationCausesCreationOfNewStationEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var stationEntity = randomizer.NextStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub = 
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(stationEntity.IpAddress);

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        stationsRepositoryMock.Setup(mock => mock
            .CreateStationAsync(
                stationEntity.MacAddress,
                stationEntity.IpAddress,
                stationEntity.ApiPort,
                stationEntity.ApiVersion,
                stationEntity.LastHeartbeat))
            .ReturnsAsync(stationEntity);

        var timeProviderStub = new FakeTimeProvider();
        timeProviderStub.SetUtcNow(stationEntity.LastHeartbeat);

        var loggerMock = new FakeLogger<StationsController>();

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object,
            timeProviderStub,
            loggerMock);

        var request = new StationRegistrationStationRequest(
            stationEntity.MacAddress,
            stationEntity.ApiPort!.Value,
            stationEntity.ApiVersion!.Value);

        IActionResult response = await controllerUnderTest.RegisterStation(request);
        response.AssertNoContentResult();

        stationsRepositoryMock.Verify(mock => mock
            .CreateStationAsync(
                stationEntity.MacAddress,
                stationEntity.IpAddress,
                stationEntity.ApiPort,
                stationEntity.ApiVersion,
                stationEntity.LastHeartbeat),
            Times.Once);

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

    [Test]
    public async Task RegistrationOfKnownStationCausesUpdateOfExistingStationEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntityBeforeUpdate = randomizer.NextStationEntity();

        StationEntity updatedStationEntity = stationEntityBeforeUpdate with
        {
            IpAddress = randomizer.NextIpAddress(),
            /* There is no validation if successive heartbeat timestamps are chronological.
             * In real scenario it is enforced by the usage of TimeProvider instance instead of FakeTimeProvider.
             */
            LastHeartbeat = randomizer.NextDateTimeOffset()
        };

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(updatedStationEntity.IpAddress);

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        stationsRepositoryMock.Setup(mock => mock
            .GetSingleStationAsync(
                filterById: false,
                id: It.IsAny<long?>(),
                filterByIpAddress: false,
                ipAddress: It.IsAny<IPAddress?>(),
                filterByMacAddress: true,
                macAddress: stationEntityBeforeUpdate.MacAddress))
            .ReturnsAsync(stationEntityBeforeUpdate);

        stationsRepositoryMock.Setup(mock => mock
            .UpdateStationAsync(
                updatedStationEntity.Id,
                updateIpAddress: true,
                ipAddress: updatedStationEntity.IpAddress,
                updateApiPort: true,
                apiPort: updatedStationEntity.ApiPort,
                updateApiVersion: true,
                apiVersion: updatedStationEntity.ApiVersion,
                updateLastHeartbeat: true,
                lastHeartbeat: updatedStationEntity.LastHeartbeat))
            .ReturnsAsync(updatedStationEntity);

        var timeProviderStub = new FakeTimeProvider();
        timeProviderStub.SetUtcNow(updatedStationEntity.LastHeartbeat);

        var loggerMock = new FakeLogger<StationsController>();

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object,
            timeProviderStub,
            loggerMock);

        var request = new StationRegistrationStationRequest(
            updatedStationEntity.MacAddress,
            updatedStationEntity.ApiPort!.Value,
            updatedStationEntity.ApiVersion!.Value);

        IActionResult response = await controllerUnderTest.RegisterStation(request);
        response.AssertNoContentResult();

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
                updatedStationEntity.Id, 
                updateIpAddress: true,
                ipAddress: updatedStationEntity.IpAddress,
                updateApiPort: true,
                apiPort: updatedStationEntity.ApiPort,
                updateApiVersion: true,
                apiVersion: updatedStationEntity.ApiVersion,
                updateLastHeartbeat: true,
                lastHeartbeat: updatedStationEntity.LastHeartbeat),
            Times.Once);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public async Task RegistrationReturnsBadRequestIfStationIpAddressCannotBeDetermined()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity() with
        {
            IpAddress = null
        };

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(stationEntity.IpAddress);

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var timeProviderStub = new FakeTimeProvider();
        var loggerMock = new FakeLogger<StationsController>();

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object,
            timeProviderStub,
            loggerMock);

        var request = new StationRegistrationStationRequest(
            stationEntity.MacAddress,
            stationEntity.ApiPort!.Value,
            stationEntity.ApiVersion!.Value);

        IActionResult response = await controllerUnderTest.RegisterStation(request);
        response.AssertBadRequestResult();

        stationsRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }

    [Test]
    public async Task RegistrationReturnsInternalServerErrorIfRepositoryUpdateDuringRegistrationOfKnownStationFails()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var stationEntityBeforeUpdate = randomizer.NextStationEntity();

        StationEntity updatedStationEntity = stationEntityBeforeUpdate with
        {
            IpAddress = randomizer.NextIpAddress(),
            /* There is no validation if successive heartbeat timestamps are chronological.
             * In real scenario it is enforced by the usage of TimeProvider instance instead of FakeTimeProvider.
             */
            LastHeartbeat = randomizer.NextDateTimeOffset()
        };

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(updatedStationEntity.IpAddress);

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        stationsRepositoryMock.Setup(mock => mock
            .GetSingleStationAsync(
                filterById: false,
                id: It.IsAny<long?>(),
                filterByIpAddress: false,
                ipAddress: It.IsAny<IPAddress?>(),
                filterByMacAddress: true,
                macAddress: stationEntityBeforeUpdate.MacAddress))
            .ReturnsAsync(stationEntityBeforeUpdate);

        var timeProviderStub = new FakeTimeProvider();
        timeProviderStub.SetUtcNow(updatedStationEntity.LastHeartbeat);

        var loggerMock = new FakeLogger<StationsController>();

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object,
            timeProviderStub,
            loggerMock);

        var request = new StationRegistrationStationRequest(
            updatedStationEntity.MacAddress,
            updatedStationEntity.ApiPort!.Value,
            updatedStationEntity.ApiVersion!.Value);

        IActionResult response = await controllerUnderTest.RegisterStation(request);
        response.AssertInternalServerError();

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
                updatedStationEntity.Id,
                updateIpAddress: true,
                ipAddress: updatedStationEntity.IpAddress,
                updateApiPort: true,
                apiPort: updatedStationEntity.ApiPort,
                updateApiVersion: true,
                apiVersion: updatedStationEntity.ApiVersion,
                updateLastHeartbeat: true,
                lastHeartbeat: updatedStationEntity.LastHeartbeat),
            Times.Once);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Error));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Error < record.Level));
    }
    #endregion

    #region Heartbeat signal
    [Test]
    public async Task UpdateOfHeartbeatSignalCausesUpdatesOfHeartbeatTimestampIfStationIsRegistered()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var stationEntityBeforeUpdate = randomizer.NextStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(stationEntityBeforeUpdate.IpAddress);

        StationEntity updatedStationEntity = stationEntityBeforeUpdate with
        {
            /* There is no validation if successive heartbeat timestamps are chronological.
             * In real scenario it is enforced by the usage of TimeProvider instance instead of FakeTimeProvider.
             */
            LastHeartbeat = randomizer.NextDateTimeOffset()
        };

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        stationsRepositoryMock.Setup(mock => mock
            .GetSingleStationAsync(
                filterById: It.IsAny<bool>(),
                id: It.IsAny<long?>(),
                filterByIpAddress: true,
                ipAddress: stationEntityBeforeUpdate.IpAddress,
                filterByMacAddress: It.IsAny<bool>(),
                macAddress: It.IsAny<PhysicalAddress?>()))
            .ReturnsAsync(stationEntityBeforeUpdate);

        stationsRepositoryMock.Setup(mock => mock
            .UpdateStationAsync(
                updatedStationEntity.Id,
                updateIpAddress: false,
                ipAddress: null,
                updateApiPort: false,
                apiPort: null,
                updateApiVersion: false,
                apiVersion: null,
                updateLastHeartbeat: true,
                lastHeartbeat: updatedStationEntity.LastHeartbeat))
            .ReturnsAsync(updatedStationEntity);

        var timeProviderStub = new FakeTimeProvider();
        timeProviderStub.SetUtcNow(updatedStationEntity.LastHeartbeat);

        var loggerMock = new FakeLogger<StationsController>();

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object,
            timeProviderStub,
            loggerMock);

        IActionResult response = await controllerUnderTest.ProcessHeartbeatSignal();
        response.AssertNoContentResult();

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
                updatedStationEntity.Id,
                updateIpAddress: false,
                ipAddress: null,
                updateApiPort: false,
                apiPort: null,
                updateApiVersion: false,
                apiVersion: null,
                updateLastHeartbeat: true,
                lastHeartbeat: updatedStationEntity.LastHeartbeat),
            Times.Once);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public async Task UpdateOfHeartbeatSignalReturnsBadRequestIfStationIpAddressCannotBeDetermined()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var stationEntity = randomizer.NextStationEntity() with
        {
            IpAddress = null
        };

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(stationEntity.IpAddress);

        StationEntity updatedStationEntity = stationEntity with
        {
            /* 
             * There is no validation if successive heartbeat timestamps are chronological.
             * In real scenario it is enforced by the usage of TimeProvider instance instead of FakeTimeProvider.
             */
            LastHeartbeat = randomizer.NextDateTimeOffset()
        };

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var timeProviderStub = new FakeTimeProvider();
        var loggerMock = new FakeLogger<StationsController>();

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object,
            timeProviderStub,
            loggerMock);

        IActionResult response = await controllerUnderTest.ProcessHeartbeatSignal();
        response.AssertBadRequestResult();

        stationsRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }

    [Test]
    public async Task UpdateOfHeartbeatSignalReturnsNotFoundIfStationIsUnregistered()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var unregisteredStationEntity = randomizer.NextStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(unregisteredStationEntity.IpAddress);

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var timeProviderStub = new FakeTimeProvider();
        var loggerMock = new FakeLogger<StationsController>();

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object,
            timeProviderStub,
            loggerMock);

        IActionResult updateResult = await controllerUnderTest.ProcessHeartbeatSignal();
        updateResult.AssertNotFoundResult();

        stationsRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }

    [Test]
    public async Task UpdateOfHeartbeatSignalReturnsInternalServerErrorIfRepositoryUpdateFails()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var stationEntityBeforeUpdate = randomizer.NextStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(stationEntityBeforeUpdate.IpAddress);

        StationEntity updatedStationEntity = stationEntityBeforeUpdate with
        {
            /* There is no validation if successive heartbeat timestamps are chronological.
             * In real scenario it is enforced by the usage of TimeProvider instance instead of FakeTimeProvider.
             */
            LastHeartbeat = randomizer.NextDateTimeOffset()
        };

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        stationsRepositoryMock.Setup(mock => mock
            .GetSingleStationAsync(
                filterById: It.IsAny<bool>(),
                id: It.IsAny<long?>(),
                filterByIpAddress: true,
                ipAddress: stationEntityBeforeUpdate.IpAddress,
                filterByMacAddress: It.IsAny<bool>(),
                macAddress: It.IsAny<PhysicalAddress?>()))
            .ReturnsAsync(stationEntityBeforeUpdate);

        var timeProviderStub = new FakeTimeProvider();
        timeProviderStub.SetUtcNow(updatedStationEntity.LastHeartbeat);

        var loggerMock = new FakeLogger<StationsController>();

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object,
            timeProviderStub,
            loggerMock);

        IActionResult response = await controllerUnderTest.ProcessHeartbeatSignal();
        response.AssertInternalServerError();

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
                updatedStationEntity.Id,
                updateIpAddress: false,
                ipAddress: null,
                updateApiPort: false,
                apiPort: null,
                updateApiVersion: false,
                apiVersion: null,
                updateLastHeartbeat: true,
                lastHeartbeat: updatedStationEntity.LastHeartbeat),
            Times.Once);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => record.Level == LogLevel.Error));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Error < record.Level));
    }
    #endregion
}
