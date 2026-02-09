using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Controllers.Firmware;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Models.Requests;
using SmartHome.Server.Data.Models.Responses;
using SmartHome.Server.Data.Repositories;
using SmartHome.UnitTests;

namespace UnitTests.Server.Controllers.Firmware;

// TODO: Add more tests for eadge cases and error handling.
[Category("UnitTest")]
[TestOf(typeof(SwitchesController))]
[Author("Jakub Miodunka")]
public sealed class SwitchesControllerTests
{
    #region Test cases
    [Test]
    public void InstantiationPossible()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var switchesRepositoryStub = new Mock<ISwitchesRepository>();
        var stationsRepositoryStub = new Mock<IStationsRepository>();

        TestDelegate actionUnderTest = () =>
            new SwitchesController(
                httpContextAccessorStub.Object,
                switchesRepositoryStub.Object,
                stationsRepositoryStub.Object);

        Assert.DoesNotThrow(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsAsHttpContextAccessor()
    {
        var switchesRepositoryStub = new Mock<ISwitchesRepository>();
        var stationsRepositoryStub = new Mock<IStationsRepository>();

        TestDelegate actionUnderTest = () =>
            new SwitchesController(
                null!,
                switchesRepositoryStub.Object,
                stationsRepositoryStub.Object);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsSwitchesRepository()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var stationsRepositoryStub = new Mock<IStationsRepository>();

        TestDelegate actionUnderTest = () =>
            new SwitchesController(
                httpContextAccessorStub.Object,
                null!,
                stationsRepositoryStub.Object);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    public void InstantiationImpossibleUsingNullReferenceAsStationsRepository()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var switchesRepositoryStub = new Mock<ISwitchesRepository>();

        TestDelegate actionUnderTest = () =>
            new SwitchesController(
                httpContextAccessorStub.Object,
                switchesRepositoryStub.Object,
                null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public async Task RegistrationCausesCreationOfNewSwitchEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity parentStationEntity = randomizer.NextStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(parentStationEntity.IpAddress);

        var stationsRepositoryStub = new Mock<IStationsRepository>();

        stationsRepositoryStub.Setup(mock =>
            mock.GetSingleStationAsync(
                filterById: It.IsAny<bool>(), id: It.IsAny<long?>(),
                filterByIpAddress: true, ipAddress: parentStationEntity.IpAddress,
                filterByMacAddress: true, macAddress: parentStationEntity.MacAddress))
            .ReturnsAsync(parentStationEntity);

        var newSwitchEntity = randomizer.NextSwitchEntity(stationId: parentStationEntity.Id);

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();

        switchesRepositoryMock.Setup(mock =>
            mock.GetSingleSwitchAsync(
                filterById: It.IsAny<bool>(), id: It.IsAny<long?>(),
                filterByStationId: It.IsAny<bool>(), stationId: It.IsAny<long?>(),
                filterByLocalId: It.IsAny<bool>(), localId: It.IsAny<byte?>()))
            .ReturnsAsync(null as SwitchEntity);

        switchesRepositoryMock.Setup(mock => mock
            .CreateSwitchAsync(
                newSwitchEntity.StationId,
                newSwitchEntity.LocalId,
                newSwitchEntity.ExpectedState,
                newSwitchEntity.ActualState))
            .ReturnsAsync(newSwitchEntity);

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            stationsRepositoryStub.Object);

        var registrationRequest = new SwitchRegistrationRequest(parentStationEntity.MacAddress, newSwitchEntity.LocalId);
        IActionResult registrationResult = await controllerUnderTest.RegisterSwitch(registrationRequest);

        switchesRepositoryMock.Verify(mock => mock
            .CreateSwitchAsync(
                newSwitchEntity.StationId, 
                newSwitchEntity.LocalId,
                expectedState: false,
                actualState: null), Times.Once);

        var expectedResponse = new SwitchRegistrationResponse(newSwitchEntity.ExpectedState);
        registrationResult.AssertOkObjectResult(expectedValue: expectedResponse);
    }

    [Test]
    public async Task RegistrationCausesUpdateOfExistingSwitchEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity parentStationEntity = randomizer.NextStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(parentStationEntity.IpAddress);

        var stationsRepositoryStub = new Mock<IStationsRepository>();

        stationsRepositoryStub.Setup(mock =>
            mock.GetSingleStationAsync(
                filterById: It.IsAny<bool>(), id: It.IsAny<long?>(),
                filterByIpAddress: true, ipAddress: parentStationEntity.IpAddress,
                filterByMacAddress: true, macAddress: parentStationEntity.MacAddress))
            .ReturnsAsync(parentStationEntity);

        var switchEntityBeforeUpdate = randomizer.NextSwitchEntity(stationId: parentStationEntity.Id);

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();

        switchesRepositoryMock.Setup(mock =>
            mock.GetSingleSwitchAsync(
                filterById: It.IsAny<bool>(), id: It.IsAny<long?>(),
                filterByStationId: true, switchEntityBeforeUpdate.StationId,
                filterByLocalId: true, localId: switchEntityBeforeUpdate.LocalId))
            .ReturnsAsync(switchEntityBeforeUpdate);

        bool newSwitchState = randomizer.NextBool();
        while (switchEntityBeforeUpdate.ActualState == newSwitchState)
        {
            newSwitchState = randomizer.NextBool();
        }

        SwitchEntity switchEntityAfterUpdate = switchEntityBeforeUpdate with
        {
            ActualState = newSwitchState
        };

        switchesRepositoryMock.Setup(mock => mock
            .UpdateSwitchAsync(
                switchEntityAfterUpdate.Id,
                updateExpectedState: It.IsAny<bool>(), expectedState: It.IsAny<bool?>(),
                updateActualState: true, actualState: switchEntityAfterUpdate.ActualState))
            .ReturnsAsync(switchEntityAfterUpdate);

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            stationsRepositoryStub.Object);

        var request = new UpdateSwitchStateRequest(
            parentStationEntity.MacAddress,
            switchEntityBeforeUpdate.LocalId,
            switchEntityAfterUpdate.ActualState.Value);

        IActionResult registrationResult = await controllerUnderTest.UpdateSwitchState(request);

        switchesRepositoryMock.Verify(mock => mock
            .UpdateSwitchAsync(
                switchEntityAfterUpdate.Id,
                updateExpectedState: It.IsAny<bool>(), expectedState: It.IsAny<bool?>(),
                updateActualState: true, actualState: switchEntityAfterUpdate.ActualState),
            Times.Once);

        registrationResult.AssertNoContentResult();
    }
    #endregion
}
