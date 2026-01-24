using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Controllers;
using SmartHome.Server.Data.Models.Dtos;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Repositories;
using System.Net;
using System.Net.NetworkInformation;

namespace SmartHome.UnitTests.Server.Controllers;

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
        
        stationsRepositoryMock.Setup(mock => mock.GetSingleStationAsync(
            filterById: false, id: It.IsAny<long?>(),
            filterByIpAddress: false, ipAddress: It.IsAny<IPAddress?>(),
            filterByMacAddress: true, macAddress: It.IsAny<PhysicalAddress?>()))
            .ReturnsAsync(null as StationEntity);

        stationsRepositoryMock.Setup(mock => mock
            .CreateStationAsync(newStationEntity.MacAddress, newStationEntity.IpAddress))
            .ReturnsAsync(newStationEntity);

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object);

        IActionResult registrationResult = await controllerUnderTest.RegisterStation(newStationEntity.ToDto());

        stationsRepositoryMock.Verify(mock => mock
            .CreateStationAsync(newStationEntity.MacAddress, newStationEntity.IpAddress), Times.Once);

        registrationResult.AssertCreatedAtActionResult(
            expectedActionName: nameof(StationsController.GetStation),
            expectedValue: newStationEntity.ToDto());
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

        IActionResult registrationResult = await controllerUnderTest.RegisterStation(updatedStationEntity.ToDto());

        stationsRepositoryMock.Verify(mock => mock
            .UpdateStationAsync(updatedStationEntity.Id, updateIpAddress: true, ipAddress: stationNewIpAddress), Times.Once);

        registrationResult.AssertOkObjectResult(expectedValue: updatedStationEntity.ToDto());
    }

    [Test]
    public async Task ControllerValidatesBodyOfRegistrationRequest()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();

        var invalidStationDto = new StationDto(MacAddress: null!);

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object);

        IActionResult registrationResult = await controllerUnderTest.RegisterStation(invalidStationDto);

        stationsRepositoryMock.Verify(mock => mock
            .CreateStationAsync(It.IsAny<PhysicalAddress>(), It.IsAny<IPAddress?>()), Times.Never);

        registrationResult.AssertBadRequestObjectResult();
    }

    // [Test]
    // public async Task ControllerRetrievesExistingStationEntity()
    // {
    //     Randomizer randomizer = TestContext.CurrentContext.Random;
    // 
    //     StationEntity stationEntity = randomizer.NextStationEntity();
    // 
    //     Mock<IHttpContextAccessor> httpContextAccessorStub =
    //         TestDataGenerator.CreateHttpContextAccessorFake(stationEntity.IpAddress);
    // 
    //     var stationsRepositoryMock = new Mock<IStationsRepository>();
    // 
    //     stationsRepositoryMock.Setup(mock => mock.GetSingleStationAsync(
    //         filterByIpAddress: true, id: stationEntity.Id,
    //         filterByMacAddress: false, macAddress: It.IsAny<PhysicalAddress?>()))
    //         .ReturnsAsync(stationEntity);
    // 
    //     var controllerUnderTest = new StationsController(
    //         httpContextAccessorStub.Object,
    //         stationsRepositoryMock.Object);
    // 
    //     IActionResult retrievalResult = await controllerUnderTest.GetStation(stationEntity.Id);
    // 
    //     retrievalResult.AssertOkObjectResult(expectedValue: stationEntity.ToDto());
    // }
    #endregion
}
