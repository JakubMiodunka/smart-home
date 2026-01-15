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
[TestOf(typeof(ElectricalSwitchesControllerTests))]
[Author("Jakub Miodunka")]
public sealed class ElectricalSwitchesControllerTests
{
    #region Test cases
    [Test]
    public void InstantiationPossible()
    {
        var electricalSwitchesRepositoryStub = new Mock<IElectricalSwitchesRepository>();

        TestDelegate actionUnderTest = 
            () => new ElectricalSwitchesController(electricalSwitchesRepositoryStub.Object);

        Assert.DoesNotThrow(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsStationsRepository()
    {
        TestDelegate actionUnderTest = 
            () => new ElectricalSwitchesController(null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public async Task ControllerCreatesNewElectricalSwitchEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var newElectricalSwitchEntity = randomizer.NextElectricalSwitchEntity();

        var electricalSwitchesRepositoryMock = new Mock<IElectricalSwitchesRepository>();

        electricalSwitchesRepositoryMock.Setup(mock => mock.GetSingleElectricalSwitchAsync(
            filterById: false, id: It.IsAny<long?>(),
            filterByStationId: true, stationId: newElectricalSwitchEntity.StationId,
            filterByLocalId: true, localId: newElectricalSwitchEntity.LocalId))
            .ReturnsAsync(null as ElectricalSwitchEntity);

        electricalSwitchesRepositoryMock.Setup(mock => mock
            .CreateElectricalSwitchAsync(
                newElectricalSwitchEntity.StationId,
                newElectricalSwitchEntity.LocalId,
                newElectricalSwitchEntity.IsClosed))
            .ReturnsAsync(newElectricalSwitchEntity);

        var controllerUnderTest =
            new ElectricalSwitchesController(electricalSwitchesRepositoryMock.Object);

        IActionResult registrationResult =
            await controllerUnderTest.RegisterElectricalSwitch(newElectricalSwitchEntity.ToDto());

        electricalSwitchesRepositoryMock.Verify(mock => mock
            .CreateElectricalSwitchAsync(
                newElectricalSwitchEntity.StationId, 
                newElectricalSwitchEntity.LocalId,
                newElectricalSwitchEntity.IsClosed), Times.Once);

        registrationResult.AssertCreatedAtActionResult(
            expectedActionName: nameof(ElectricalSwitchesController.GetElectricalSwitch),
            expectedValue: newElectricalSwitchEntity.ToDto());
    }

    [Test]
    public async Task ControllerUpdatesExistingElectricalSwitchEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var electricalSwitchEntityBeforeUpdate = randomizer.NextElectricalSwitchEntity();

        var electricalSwitchesRepositoryMock = new Mock<IElectricalSwitchesRepository>();

        electricalSwitchesRepositoryMock.Setup(mock => mock.GetSingleElectricalSwitchAsync(
            filterById: false, id: electricalSwitchEntityBeforeUpdate.Id,
            filterByStationId: false, stationId: It.IsAny<long?>(),
            filterByLocalId: false, localId: It.IsAny<byte?>()))
            .ReturnsAsync(electricalSwitchEntityBeforeUpdate);

        bool? newSwitchState = randomizer.NextNullableBool();
        while (electricalSwitchEntityBeforeUpdate.IsClosed == newSwitchState)
        {
            newSwitchState = randomizer.NextNullableBool();
        }

        var updatedElectricalSwitchEntity =
            electricalSwitchEntityBeforeUpdate with { IsClosed = newSwitchState };

        electricalSwitchesRepositoryMock.Setup(mock => mock
            .UpdateElectricalSwitchAsync(electricalSwitchEntityBeforeUpdate.Id,
                updateState: true, isClosed: updatedElectricalSwitchEntity.IsClosed))
            .ReturnsAsync(updatedElectricalSwitchEntity);

        var controllerUnderTest =
            new ElectricalSwitchesController(electricalSwitchesRepositoryMock.Object);

        IActionResult registrationResult = 
            await controllerUnderTest.RegisterElectricalSwitch(updatedElectricalSwitchEntity.ToDto());

        electricalSwitchesRepositoryMock.Verify(mock => mock
            .UpdateElectricalSwitchAsync(updatedElectricalSwitchEntity.Id,
            updateState: true, isClosed: updatedElectricalSwitchEntity.IsClosed), Times.Once);

        registrationResult.AssertOkObjectResult(expectedValue: updatedElectricalSwitchEntity.ToDto());
    }

    [Test]
    public async Task ControllerValidatesBodyOfRegistrationRequest()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var electricalSwitchesRepositoryMock = new Mock<IElectricalSwitchesRepository>();

        ElectricalSwitchEntity invalidElectricalSwitchEntity =
            randomizer.NextElectricalSwitchEntity() with { StationId = 0 };

        var controllerUnderTest =
            new ElectricalSwitchesController(electricalSwitchesRepositoryMock.Object);

        IActionResult registrationResult =
            await controllerUnderTest.RegisterElectricalSwitch(invalidElectricalSwitchEntity.ToDto());

        electricalSwitchesRepositoryMock.Verify(mock => mock
            .CreateElectricalSwitchAsync(It.IsAny<long>(), It.IsAny<byte>(), It.IsAny<bool?>()), Times.Never);

        registrationResult.AssertBadRequestObjectResult();
    }

    [Test]
    public async Task ControllerRetrievesExistingElectricalSwitchEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        ElectricalSwitchEntity electricalSwitchEntity = randomizer.NextElectricalSwitchEntity();

        var electricalSwitchesRepositoryMock = new Mock<IElectricalSwitchesRepository>();

        electricalSwitchesRepositoryMock.Setup(mock => mock.GetSingleElectricalSwitchAsync(
            filterById: true, id: electricalSwitchEntity.Id,
            filterByStationId: false, stationId: It.IsAny<long?>(),
            filterByLocalId: false, localId: It.IsAny<byte?>()))
            .ReturnsAsync(electricalSwitchEntity);

        var controllerUnderTest =
            new ElectricalSwitchesController(electricalSwitchesRepositoryMock.Object);

        IActionResult retrievalResult = 
            await controllerUnderTest.GetElectricalSwitch(electricalSwitchEntity.Id);

        retrievalResult.AssertOkObjectResult(expectedValue: electricalSwitchEntity.ToDto());
    }
    #endregion
}
