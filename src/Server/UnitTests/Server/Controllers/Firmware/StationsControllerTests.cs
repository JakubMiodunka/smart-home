using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

// TODO: Add more tests for eadge cases and error handling.
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

        TestDelegate actionUnderTest = () => new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryStub.Object);

        Assert.DoesNotThrow(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsHttpContextAccessor()
    {
        var stationsRepositoryStub = new Mock<IStationsRepository>();

        TestDelegate actionUnderTest = () => new StationsController(null!, stationsRepositoryStub.Object);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsStationsRepository()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();

        TestDelegate actionUnderTest = () => new StationsController(httpContextAccessorStub.Object, null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public async Task ControllerCreatesNewStationEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var newStationEntity = randomizer.NextStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub = 
            TestDataGenerator.CreateHttpContextAccessorFake(newStationEntity.IpAddress);

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        
        stationsRepositoryMock.Setup(mock => 
            mock.GetSingleStationAsync(
                filterById: It.IsAny<bool>(), id: It.IsAny<long?>(),
                filterByIpAddress: It.IsAny<bool>(), ipAddress: It.IsAny<IPAddress?>(),
                filterByMacAddress: It.IsAny<bool>(), macAddress: It.IsAny<PhysicalAddress?>()))
            .ReturnsAsync(null as StationEntity);

        stationsRepositoryMock.Setup(mock => mock
            .CreateStationAsync(newStationEntity.MacAddress, newStationEntity.IpAddress))
            .ReturnsAsync(newStationEntity);

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object);

        var registrationRequest = new StationRegistrationRequest(newStationEntity.MacAddress);
        IActionResult registrationResult = await controllerUnderTest.RegisterStation(registrationRequest);

        stationsRepositoryMock.Verify(mock => mock
            .CreateStationAsync(newStationEntity.MacAddress, newStationEntity.IpAddress), Times.Once);

        registrationResult.AssertNoContentResult();
    }

    [Test]
    public async Task ControllerUpdatesExistingStationEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var stationEntityBeforeUpdate = randomizer.NextStationEntity();

        IPAddress stationNewIpAddress = randomizer.NextIpAddress();

        var updatedStationEntity = new StationEntity(
            stationEntityBeforeUpdate.Id,
            stationEntityBeforeUpdate.MacAddress,
            stationNewIpAddress);

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(stationNewIpAddress);

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        stationsRepositoryMock.Setup(mock => mock.GetSingleStationAsync(
            filterById: false, id: It.IsAny<long?>(),
            filterByIpAddress: false, ipAddress: It.IsAny<IPAddress?>(),
            filterByMacAddress: true, macAddress: stationEntityBeforeUpdate.MacAddress))
            .ReturnsAsync(stationEntityBeforeUpdate);

        stationsRepositoryMock.Setup(mock => mock
            .UpdateStationAsync(stationEntityBeforeUpdate.Id, updateIpAddress: true, ipAddress: stationNewIpAddress))
            .ReturnsAsync(updatedStationEntity);

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object);

        var registrationRequest = new StationRegistrationRequest(updatedStationEntity.MacAddress);
        IActionResult registrationResult = await controllerUnderTest.RegisterStation(registrationRequest);

        stationsRepositoryMock.Verify(mock => mock
            .UpdateStationAsync(updatedStationEntity.Id, updateIpAddress: true, ipAddress: stationNewIpAddress), Times.Once);

        registrationResult.AssertNoContentResult();
    }
    #endregion
}
