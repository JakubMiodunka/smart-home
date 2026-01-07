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
using UnitTests;

namespace SmartHome.UnitTests.Server.Controllers;

// TODO: Add tests for HTTP responses.
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
    public async Task ControllerCreatesNewStationEntityInRepository()
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
    }

    [Test]
    public async Task ControllerUpdatesExistingStationEntityInRepository()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress stationNewIpAddress = randomizer.NextIpAddress();
        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(stationNewIpAddress);

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        long stationId = randomizer.NextInt64(1, long.MaxValue);
        PhysicalAddress stationMacAddress = randomizer.NextMacAddress();
        IPAddress stationOldIpAddress = randomizer.NextIpAddress();
        var oldStationEntity = new StationEntity(stationId, stationMacAddress, stationOldIpAddress);

        stationsRepositoryMock.Setup(mock => mock.GetSingleStationAsync(
            filterById: false, id: It.IsAny<long?>(),
            filterByMacAddress: true, macAddress: stationMacAddress))
            .ReturnsAsync(oldStationEntity);

        var newStationEntity = new StationEntity(stationId, stationMacAddress, stationNewIpAddress);

        stationsRepositoryMock.Setup(mock => mock
            .UpdateStationAsync(stationMacAddress, updateIpAddress: true, ipAddress: stationNewIpAddress))
            .ReturnsAsync(newStationEntity);

        var controllerUnderTest = new StationsController(
            httpContextAccessorStub.Object,
            stationsRepositoryMock.Object);

        var newStationDto = new StationDto(stationMacAddress);
        await controllerUnderTest.RegisterStation(newStationDto);

        stationsRepositoryMock.Verify(mock => mock
            .UpdateStationAsync(stationMacAddress, updateIpAddress: true, ipAddress: stationNewIpAddress), Times.Once);
    }
    
    public async Task ControllerRetrievesExistingStation()
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
    }
    #endregion
}
