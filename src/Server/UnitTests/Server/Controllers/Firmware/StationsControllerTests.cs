using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Controllers.Firmware;
using SmartHome.Server.Data;
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
    #region Test cases
    [Test]
    public void InstantiationPossible()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var stationsRepositoryStub = new Mock<IStationsRepository>();
        var timestampProviderStub = new Mock<ITimestampProvider>();

        TestDelegate actionUnderTest = () => new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryStub.Object,
            timestampProviderStub.Object);

        Assert.DoesNotThrow(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsHttpContextAccessor()
    {
        var stationsRepositoryStub = new Mock<IStationsRepository>();
        var timestampProviderStub = new Mock<ITimestampProvider>();

        TestDelegate actionUnderTest = () => new StationsController(
            null!,
            stationsRepositoryStub.Object,
            timestampProviderStub.Object);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsStationsRepository()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var timestampProviderStub = new Mock<ITimestampProvider>();

        TestDelegate actionUnderTest = () => new StationsController(
            httpContextAccessorStub.Object,
            null!,
            timestampProviderStub.Object);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsTimestampProvider()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var stationsRepositoryStub = new Mock<IStationsRepository>();

        TestDelegate actionUnderTest = () => new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryStub.Object,
            null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public async Task RegistrationOfUnknownStationCausesCreationOfNewStationEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var newStationEntity = randomizer.NextStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub = 
            TestDataGenerator.CreateHttpContextAccessorFake(newStationEntity.IpAddress);

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        stationsRepositoryMock.Setup(mock => mock
            .CreateStationAsync(
                newStationEntity.MacAddress,
                newStationEntity.IpAddress,
                newStationEntity.LastHeartbeat))
            .ReturnsAsync(newStationEntity);

        var timestampProviderStub = new Mock<ITimestampProvider>();

        timestampProviderStub.Setup(stub => stub
            .GetUtcNow())
            .Returns(newStationEntity.LastHeartbeat);

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object,
            timestampProviderStub.Object);

        var registrationRequest = new StationRegistrationRequest(newStationEntity.MacAddress);
        IActionResult registrationResult = await controllerUnderTest.RegisterStation(registrationRequest);

        stationsRepositoryMock.Verify(mock => mock
            .CreateStationAsync(
                newStationEntity.MacAddress,
                newStationEntity.IpAddress,
                newStationEntity.LastHeartbeat),
            Times.Once);

        registrationResult.AssertNoContentResult();
    }

    [Test]
    public async Task RegistrationOfKnownStationCausesUpdateOfExistingStationEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var stationEntityBeforeUpdate = randomizer.NextStationEntity();

        StationEntity updatedStationEntity = stationEntityBeforeUpdate with
        {
            IpAddress = randomizer.NextIpAddress(),
            LastHeartbeat = randomizer.NextDateTime()   // Successive heartbeat timestamps do not need to be chronological.
        };

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(updatedStationEntity.IpAddress);

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
                stationEntityBeforeUpdate.Id,
                updateIpAddress: true,
                ipAddress: updatedStationEntity.IpAddress))
            .ReturnsAsync(updatedStationEntity);

        var timestampProviderStub = new Mock<ITimestampProvider>();

        timestampProviderStub.Setup(stub => stub
            .GetUtcNow())
            .Returns(updatedStationEntity.LastHeartbeat);

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object,
            timestampProviderStub.Object);

        var registrationRequest = new StationRegistrationRequest(updatedStationEntity.MacAddress);
        IActionResult registrationResult = await controllerUnderTest.RegisterStation(registrationRequest);

        stationsRepositoryMock.Verify(mock => mock
            .UpdateStationAsync(
                updatedStationEntity.Id, 
                updateIpAddress: true,
                ipAddress: updatedStationEntity.IpAddress,
                updateLastHeartbeat: true,
                lastHeartbeat: updatedStationEntity.LastHeartbeat),
            Times.Once);

        registrationResult.AssertNoContentResult();
    }

    [Test]
    public async Task RegistrationReturnsBadRequestIfStationIpAddressCannotBeDetermined()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity newStationEntity = randomizer.NextStationEntity() with
        {
            IpAddress = null
        };

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(newStationEntity.IpAddress);

        var stationsRepositoryStub = new Mock<IStationsRepository>();
        var timestampProviderStub = new Mock<ITimestampProvider>();

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryStub.Object,
            timestampProviderStub.Object);

        var registrationRequest = new StationRegistrationRequest(newStationEntity.MacAddress);
        IActionResult registrationResult = await controllerUnderTest.RegisterStation(registrationRequest);

        registrationResult.AssertBadRequestObjectResult();
    }

    [Test]
    public async Task UpdateOfHeartbeatSignalCausesUpdatesOfHeartbeatTimestampIfStationIsRegistered()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var stationEntityBeforeUpdate = randomizer.NextStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(stationEntityBeforeUpdate.IpAddress);

        StationEntity updatedStationEntity = stationEntityBeforeUpdate with
        {
            LastHeartbeat = randomizer.NextDateTime()   // Successive heartbeat timestamps do not need to be chronological.
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
                updateLastHeartbeat: true,
                lastHeartbeat: updatedStationEntity.LastHeartbeat))
            .ReturnsAsync(updatedStationEntity);

        var timestampProviderStub = new Mock<ITimestampProvider>();

        timestampProviderStub.Setup(stub => stub
            .GetUtcNow())
            .Returns(updatedStationEntity.LastHeartbeat);

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object,
            timestampProviderStub.Object);

        IActionResult updateResult = await controllerUnderTest.UpdateHeartbeatTimestamp();

        stationsRepositoryMock.Verify(mock => mock
            .UpdateStationAsync(
                updatedStationEntity.Id,
                updateIpAddress: false,
                ipAddress: null,
                updateLastHeartbeat: true,
                lastHeartbeat: updatedStationEntity.LastHeartbeat),
            Times.Once);

        updateResult.AssertNoContentResult();
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
            TestDataGenerator.CreateHttpContextAccessorFake(stationEntity.IpAddress);

        StationEntity updatedStationEntity = stationEntity with
        {
            LastHeartbeat = randomizer.NextDateTime()   // Successive heartbeat timestamps do not need to be chronological.
        };

        var stationsRepositoryStub = new Mock<IStationsRepository>();
        var timestampProviderStub = new Mock<ITimestampProvider>();

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryStub.Object,
            timestampProviderStub.Object);

        IActionResult updateResult = await controllerUnderTest.UpdateHeartbeatTimestamp();

        updateResult.AssertBadRequestObjectResult();
    }

    [Test]
    public async Task UpdateOfHeartbeatSignalReturnsNotFoundIfStationIsUnregistered()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var unregisteredStationEntity = randomizer.NextStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(unregisteredStationEntity.IpAddress);

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        var timestampProviderStub = new Mock<ITimestampProvider>();

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object,
            timestampProviderStub.Object);

        IActionResult updateResult = await controllerUnderTest.UpdateHeartbeatTimestamp();

        updateResult.AssertNotFoundObjectResult();
    }
    #endregion
}
