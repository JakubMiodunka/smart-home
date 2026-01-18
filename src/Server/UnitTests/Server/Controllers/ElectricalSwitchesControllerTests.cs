using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Controllers;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Repositories;
using SmartHome.Server.Managers.Factories;

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
        var switchesRepositoryStub = new Mock<IElectricalSwitchesRepository>();
        var switchesManagerFactoryStub = new Mock<IElectricalSwitchManagerFactory>();

        TestDelegate actionUnderTest = 
            () => new ElectricalSwitchesController(
                switchesRepositoryStub.Object,
                switchesManagerFactoryStub.Object);

        Assert.DoesNotThrow(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsStationsRepository()
    {
        var switchManagerFactoryStub = new Mock<IElectricalSwitchManagerFactory>();

        TestDelegate actionUnderTest = 
            () => new ElectricalSwitchesController(
                switchesRepository: null!,
                switchManagerFactoryStub.Object);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsElectricalSwitchManagerFactory()
    {
        var switchesRepositoryStub = new Mock<IElectricalSwitchesRepository>();

        TestDelegate actionUnderTest =
            () => new ElectricalSwitchesController(
                switchesRepositoryStub.Object,
                switchManagerFactory: null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public async Task ControllerCreatesNewElectricalSwitchEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var newSwitchEntity = randomizer.NextElectricalSwitchEntity();

        var switchesRepositoryMock = new Mock<IElectricalSwitchesRepository>();

        switchesRepositoryMock.Setup(mock => mock.GetSingleElectricalSwitchAsync(
            filterById: false, id: It.IsAny<long?>(),
            filterByStationId: true, stationId: newSwitchEntity.StationId,
            filterByLocalId: true, localId: newSwitchEntity.LocalId))
            .ReturnsAsync(null as ElectricalSwitchEntity);

        switchesRepositoryMock.Setup(mock => mock
            .CreateElectricalSwitchAsync(
                newSwitchEntity.StationId,
                newSwitchEntity.LocalId,
                newSwitchEntity.IsClosed))
            .ReturnsAsync(newSwitchEntity);

        var switchesManagerFactoryStub = new Mock<IElectricalSwitchManagerFactory>();

        var controllerUnderTest = new ElectricalSwitchesController(
                switchesRepositoryMock.Object,
                switchesManagerFactoryStub.Object);

        IActionResult registrationResult =
            await controllerUnderTest.RegisterElectricalSwitch(newSwitchEntity.ToDto());

        switchesRepositoryMock.Verify(mock => mock
            .CreateElectricalSwitchAsync(
                newSwitchEntity.StationId, 
                newSwitchEntity.LocalId,
                newSwitchEntity.IsClosed), Times.Once);

        registrationResult.AssertCreatedAtActionResult(
            expectedActionName: nameof(ElectricalSwitchesController.GetElectricalSwitch),
            expectedValue: newSwitchEntity.ToDto());
    }

    [Test]
    public async Task ControllerUpdatesExistingElectricalSwitchEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var switchEntityBeforeUpdate = randomizer.NextElectricalSwitchEntity();

        var switchesRepositoryMock = new Mock<IElectricalSwitchesRepository>();

        switchesRepositoryMock.Setup(mock => mock.GetSingleElectricalSwitchAsync(
            filterById: false, id: switchEntityBeforeUpdate.Id,
            filterByStationId: false, stationId: It.IsAny<long?>(),
            filterByLocalId: false, localId: It.IsAny<byte?>()))
            .ReturnsAsync(switchEntityBeforeUpdate);

        bool? newSwitchState = randomizer.NextNullableBool();
        while (switchEntityBeforeUpdate.IsClosed == newSwitchState)
        {
            newSwitchState = randomizer.NextNullableBool();
        }

        var updatedSwitchEntity =
            switchEntityBeforeUpdate with { IsClosed = newSwitchState };

        switchesRepositoryMock.Setup(mock => mock
            .UpdateElectricalSwitchAsync(switchEntityBeforeUpdate.Id,
                updateState: true, isClosed: updatedSwitchEntity.IsClosed))
            .ReturnsAsync(updatedSwitchEntity);

        var switchesManagerFactoryStub = new Mock<IElectricalSwitchManagerFactory>();

        var controllerUnderTest = new ElectricalSwitchesController(
            switchesRepositoryMock.Object,
            switchesManagerFactoryStub.Object);

        IActionResult registrationResult = 
            await controllerUnderTest.RegisterElectricalSwitch(updatedSwitchEntity.ToDto());

        switchesRepositoryMock.Verify(mock => mock
            .UpdateElectricalSwitchAsync(updatedSwitchEntity.Id,
            updateState: true, isClosed: updatedSwitchEntity.IsClosed), Times.Once);

        registrationResult.AssertOkObjectResult(expectedValue: updatedSwitchEntity.ToDto());
    }

    [Test]
    public async Task ControllerValidatesBodyOfRegistrationRequest()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var switchesRepositoryMock = new Mock<IElectricalSwitchesRepository>();

        ElectricalSwitchEntity invalidSwitchEntity =
            randomizer.NextElectricalSwitchEntity() with { StationId = 0 };

        var switchesManagerFactoryStub = new Mock<IElectricalSwitchManagerFactory>();

        var controllerUnderTest = new ElectricalSwitchesController(
            switchesRepositoryMock.Object,
            switchesManagerFactoryStub.Object);

        IActionResult registrationResult =
            await controllerUnderTest.RegisterElectricalSwitch(invalidSwitchEntity.ToDto());

        switchesRepositoryMock.Verify(mock => mock
            .CreateElectricalSwitchAsync(It.IsAny<long>(), It.IsAny<byte>(), It.IsAny<bool?>()), Times.Never);

        registrationResult.AssertBadRequestObjectResult();
    }

    [Test]
    public async Task ControllerRetrievesExistingElectricalSwitchEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        ElectricalSwitchEntity switchEntity = randomizer.NextElectricalSwitchEntity();

        var switchesRepositoryMock = new Mock<IElectricalSwitchesRepository>();

        switchesRepositoryMock.Setup(mock => mock.GetSingleElectricalSwitchAsync(
            filterById: true, id: switchEntity.Id,
            filterByStationId: false, stationId: It.IsAny<long?>(),
            filterByLocalId: false, localId: It.IsAny<byte?>()))
            .ReturnsAsync(switchEntity);

        var switchesManagerFactoryStub = new Mock<IElectricalSwitchManagerFactory>();

        var controllerUnderTest = new ElectricalSwitchesController(
            switchesRepositoryMock.Object,
            switchesManagerFactoryStub.Object);

        IActionResult retrievalResult = 
            await controllerUnderTest.GetElectricalSwitch(switchEntity.Id);

        retrievalResult.AssertOkObjectResult(expectedValue: switchEntity.ToDto());
    }
    #endregion
}
