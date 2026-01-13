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

        IPAddress stationIpAddress = randomizer.NextIpAddress();
        Mock<IHttpContextAccessor> httpContextAccessorStub = 
            TestDataGenerator.CreateHttpContextAccessorFake(stationIpAddress);

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        
        stationsRepositoryMock.Setup(mock => mock.GetSingleStationAsync(
            filterById: false, id: It.IsAny<long>(),
            filterByMacAddress: true, macAddress: It.IsAny<PhysicalAddress>()))
            .ReturnsAsync(null as StationEntity);

        long stationId = randomizer.NextInt64(1, long.MaxValue);
        PhysicalAddress stationMacAddress = randomizer.NextMacAddress();
        var newStationEntity = new StationEntity(stationId, stationMacAddress, stationIpAddress);

        stationsRepositoryMock.Setup(mock => mock
            .CreateStationAsync(stationMacAddress, stationIpAddress))
            .ReturnsAsync(newStationEntity);

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object);

        var stationDto = new StationDto(stationMacAddress);
        IActionResult registrationResult = await controllerUnderTest.RegisterStation(stationDto);

        stationsRepositoryMock.Verify(mock => mock
            .CreateStationAsync(stationMacAddress, stationIpAddress), Times.Once);

        registrationResult.AssertCreatedAtActionResult(
            expectedActionName: nameof(StationsController.GetStation),
            expectedValue: newStationEntity.ToDto());
    }

    [Test]
    public async Task ControllerUpdatesExistingStationEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress stationNewIpAddress = randomizer.NextIpAddress();
        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(stationNewIpAddress);

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        long stationId = randomizer.NextInt64(1, long.MaxValue);
        PhysicalAddress stationMacAddress = randomizer.NextMacAddress();
        IPAddress stationOldIpAddress = randomizer.NextIpAddress();
        var stationEntityBeforeUpdate = new StationEntity(stationId, stationMacAddress, stationOldIpAddress);

        stationsRepositoryMock.Setup(mock => mock.GetSingleStationAsync(
            filterById: false, id: It.IsAny<long?>(),
            filterByMacAddress: true, macAddress: stationMacAddress))
            .ReturnsAsync(stationEntityBeforeUpdate);

        var updatedStationEntity = new StationEntity(stationId, stationMacAddress, stationNewIpAddress);

        stationsRepositoryMock.Setup(mock => mock
            .UpdateStationAsync(stationId, updateIpAddress: true, ipAddress: stationNewIpAddress))
            .ReturnsAsync(updatedStationEntity);

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object);

        var updatedStationDto = new StationDto(stationMacAddress);
        IActionResult registrationResult = await controllerUnderTest.RegisterStation(updatedStationDto);

        stationsRepositoryMock.Verify(mock => mock
            .UpdateStationAsync(stationId, updateIpAddress: true, ipAddress: stationNewIpAddress), Times.Once);

        registrationResult.AssertOkObjectResult(expectedValue: updatedStationEntity.ToDto());
    }
    
    public async Task ControllerRetrievesExistingStationEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake();

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        StationEntity stationEntity = randomizer.NextStationEntity();

        stationsRepositoryMock.Setup(mock => mock.GetSingleStationAsync(
            filterById: true, id: stationEntity.Id,
            filterByMacAddress: false, macAddress: It.IsAny<PhysicalAddress?>()))
            .ReturnsAsync(stationEntity);

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object);

        IActionResult retrievalResult = await controllerUnderTest.GetStation(stationEntity.Id);

        stationsRepositoryMock.Verify(mock => mock
            .GetSingleStationAsync(filterById: true, id: stationEntity.Id), Times.Once);

        retrievalResult.AssertOkObjectResult(expectedValue: stationEntity.ToDto());
    }
    #endregion
}
