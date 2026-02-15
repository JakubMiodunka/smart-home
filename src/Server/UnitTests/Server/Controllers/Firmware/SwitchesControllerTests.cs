using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Controllers.Firmware;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Models.Requests;
using SmartHome.Server.Data.Models.Responses;
using SmartHome.Server.Data.Repositories;
using SmartHome.UnitTests;
using System.Net;
using System.Net.NetworkInformation;

namespace UnitTests.Server.Controllers.Firmware;

[Category("UnitTest")]
[TestOf(typeof(SwitchesController))]
[Author("Jakub Miodunka")]
public sealed class SwitchesControllerTests
{
    #region Constructor
    [Test]
    public void InstantiationPossible()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var switchesRepositoryStub = new Mock<ISwitchesRepository>();
        var stationsRepositoryStub = new Mock<IStationsRepository>();
        var loggerStub = new FakeLogger<SwitchesController>();

        TestDelegate actionUnderTest = () =>
            new SwitchesController(
                httpContextAccessorStub.Object,
                switchesRepositoryStub.Object,
                stationsRepositoryStub.Object,
                loggerStub);

        Assert.DoesNotThrow(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsAsHttpContextAccessor()
    {
        var switchesRepositoryStub = new Mock<ISwitchesRepository>();
        var stationsRepositoryStub = new Mock<IStationsRepository>();
        var loggerStub = new FakeLogger<SwitchesController>();

        TestDelegate actionUnderTest = () =>
            new SwitchesController(
                null!,
                switchesRepositoryStub.Object,
                stationsRepositoryStub.Object,
                loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsSwitchesRepository()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var stationsRepositoryStub = new Mock<IStationsRepository>();
        var loggerStub = new FakeLogger<SwitchesController>();

        TestDelegate actionUnderTest = () =>
            new SwitchesController(
                httpContextAccessorStub.Object,
                null!,
                stationsRepositoryStub.Object,
                loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsStationsRepository()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var switchesRepositoryStub = new Mock<ISwitchesRepository>();
        var loggerStub = new FakeLogger<SwitchesController>();

        TestDelegate actionUnderTest = () =>
            new SwitchesController(
                httpContextAccessorStub.Object,
                switchesRepositoryStub.Object,
                null!,
                loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsLogger()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var switchesRepositoryStub = new Mock<ISwitchesRepository>();
        var stationsRepositoryStub = new Mock<IStationsRepository>();

        TestDelegate actionUnderTest = () =>
            new SwitchesController(
                httpContextAccessorStub.Object,
                switchesRepositoryStub.Object,
                stationsRepositoryStub.Object,
                null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }
    #endregion

    #region Registration
    [Test]
    public async Task RegistrationOfUnknownSwitchCausesCreationOfNewSwitchEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity parentStationEntity = randomizer.NextStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(parentStationEntity.IpAddress);

        var stationsRepositoryStub = new Mock<IStationsRepository>();

        stationsRepositoryStub.Setup(mock =>
            mock.GetSingleStationAsync(
                filterById: It.IsAny<bool>(),
                id: It.IsAny<long?>(),
                filterByIpAddress: true,
                ipAddress: parentStationEntity.IpAddress,
                filterByMacAddress: It.IsAny<bool>(),
                macAddress: It.IsAny<PhysicalAddress?>()))
            .ReturnsAsync(parentStationEntity);

        SwitchEntity newSwitchEntity = randomizer.NextSwitchEntity() with
        {
            StationId = parentStationEntity.Id,
            ExpectedState = false,   // By default, every switch should be open, ensuring that current does not flow.
            ActualState = null  // During entity creation actual state of the switch is unknown.
        };

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();

        switchesRepositoryMock.Setup(mock => mock
            .CreateSwitchAsync(
                newSwitchEntity.StationId,
                newSwitchEntity.LocalId,
                newSwitchEntity.ExpectedState,
                newSwitchEntity.ActualState))
            .ReturnsAsync(newSwitchEntity);

        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            stationsRepositoryStub.Object,
            loggerMock);

        var request = new SwitchRegistrationRequest(newSwitchEntity.LocalId);
        IActionResult response = await controllerUnderTest.RegisterSwitch(request);

        switchesRepositoryMock.Verify(mock => mock
            .CreateSwitchAsync(
                newSwitchEntity.StationId, 
                newSwitchEntity.LocalId,
                expectedState: false,
                actualState: null), Times.Once);

        var expectedResponse = new SwitchRegistrationResponse(newSwitchEntity.ExpectedState);
        response.AssertOkObjectResult(expectedValue: expectedResponse);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public async Task RegistrationOfKnownSwitchCausesUpdateOfExistingSwitchEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity parentStationEntity = randomizer.NextStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(parentStationEntity.IpAddress);

        var stationsRepositoryStub = new Mock<IStationsRepository>();

        stationsRepositoryStub.Setup(mock =>
            mock.GetSingleStationAsync(
                filterById: It.IsAny<bool>(),
                id: It.IsAny<long?>(),
                filterByIpAddress: true,
                ipAddress: parentStationEntity.IpAddress,
                filterByMacAddress: It.IsAny<bool>(),
                macAddress: It.IsAny<PhysicalAddress?>()))
            .ReturnsAsync(parentStationEntity);

        SwitchEntity switchEntityBeforeUpdate = randomizer.NextSwitchEntity() with
        {
            StationId = parentStationEntity.Id
        };

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();

        switchesRepositoryMock.Setup(mock =>
            mock.GetSingleSwitchAsync(
                filterById: It.IsAny<bool>(),
                id: It.IsAny<long?>(),
                filterByStationId: true,
                switchEntityBeforeUpdate.StationId,
                filterByLocalId: true,
                localId: switchEntityBeforeUpdate.LocalId))
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
                updateExpectedState: It.IsAny<bool>(),
                expectedState: It.IsAny<bool?>(),
                updateActualState: true,
                actualState: switchEntityAfterUpdate.ActualState))
            .ReturnsAsync(switchEntityAfterUpdate);

        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            stationsRepositoryStub.Object,
            loggerMock);

        var request = new SwitchUpdateRequest(
            switchEntityBeforeUpdate.LocalId,
            switchEntityAfterUpdate.ActualState.Value);

        IActionResult response = await controllerUnderTest.UpdateSwitch(request);

        switchesRepositoryMock.Verify(mock => mock
            .UpdateSwitchAsync(
                switchEntityAfterUpdate.Id,
                updateExpectedState: It.IsAny<bool>(),
                expectedState: It.IsAny<bool?>(),
                updateActualState: true,
                actualState: switchEntityAfterUpdate.ActualState),
            Times.Once);

        response.AssertNoContentResult();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public async Task RegistrationReturnsBadRequestIfStationIpAddressCannotBeDetermined()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity parentStationEntity = randomizer.NextStationEntity() with
        {
            IpAddress = null
        };

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(parentStationEntity.IpAddress);

        SwitchEntity newSwitchEntity = randomizer.NextSwitchEntity() with
        {
            StationId = parentStationEntity.Id
        };

        var stationsRepositoryStub = new Mock<IStationsRepository>();
        var switchesRepositoryStub = new Mock<ISwitchesRepository>();
        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryStub.Object,
            stationsRepositoryStub.Object,
            loggerMock);

        var request = new SwitchRegistrationRequest(newSwitchEntity.LocalId);
        IActionResult response = await controllerUnderTest.RegisterSwitch(request);

        response.AssertBadRequestResult();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public async Task RegistrationReturnsNotFoundIfStationIsNotRegistered()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity unregisteredSationEntity = randomizer.NextStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(unregisteredSationEntity.IpAddress);

        SwitchEntity newSwitchEntity = randomizer.NextSwitchEntity() with
        {
            StationId = unregisteredSationEntity.Id,
            ExpectedState = false,   // By default, every switch should be open, ensuring that current does not flow.
            ActualState = null  // During entity creation actual state of the switch is unknown.
        };

        var switchesRepositoryStub = new Mock<ISwitchesRepository>();
        var stationsRepositoryStub = new Mock<IStationsRepository>();
        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryStub.Object,
            stationsRepositoryStub.Object,
            loggerMock);

        var request = new SwitchRegistrationRequest(newSwitchEntity.LocalId);
        IActionResult response = await controllerUnderTest.RegisterSwitch(request);

        response.AssertNotFoundResult();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }
    #endregion

    #region Switch udapte
    [Test]
    public async Task UpdateOfKnownSwitchCausesUpdateOfExistingSwitchEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity parentStationEntity = randomizer.NextStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(parentStationEntity.IpAddress);

        var stationsRepositoryStub = new Mock<IStationsRepository>();

        stationsRepositoryStub.Setup(mock =>
            mock.GetSingleStationAsync(
                filterById: It.IsAny<bool>(),
                id: It.IsAny<long?>(),
                filterByIpAddress: true,
                ipAddress: parentStationEntity.IpAddress,
                filterByMacAddress: It.IsAny<bool>(),
                macAddress: It.IsAny<PhysicalAddress?>()))
            .ReturnsAsync(parentStationEntity);

        SwitchEntity switchEntityBeforeUpdate = randomizer.NextSwitchEntity() with
        {
            StationId = parentStationEntity.Id
        };

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();

        switchesRepositoryMock.Setup(mock =>
            mock.GetSingleSwitchAsync(
                filterById: It.IsAny<bool>(),
                id: It.IsAny<long?>(),
                filterByStationId: true,
                switchEntityBeforeUpdate.StationId,
                filterByLocalId: true,
                localId: switchEntityBeforeUpdate.LocalId))
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
                updateExpectedState: It.IsAny<bool>(),
                expectedState: It.IsAny<bool?>(),
                updateActualState: true,
                actualState: switchEntityAfterUpdate.ActualState))
            .ReturnsAsync(switchEntityAfterUpdate);

        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            stationsRepositoryStub.Object,
            loggerMock);

        var request = new SwitchUpdateRequest(
            switchEntityBeforeUpdate.LocalId,
            switchEntityAfterUpdate.ActualState.Value);

        IActionResult response = await controllerUnderTest.UpdateSwitch(request);

        switchesRepositoryMock.Verify(mock => mock
            .UpdateSwitchAsync(
                switchEntityAfterUpdate.Id,
                updateExpectedState: It.IsAny<bool>(),
                expectedState: It.IsAny<bool?>(),
                updateActualState: true,
                actualState: switchEntityAfterUpdate.ActualState),
            Times.Once);

        response.AssertNoContentResult();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public async Task UpdateReturnsBadRequestIfStationIpAddressCannotBeDetermined()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity parentStationEntity = randomizer.NextStationEntity() with
        {
            IpAddress = null
        };

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(parentStationEntity.IpAddress);

        SwitchEntity switchEntity = randomizer.NextSwitchEntity() with
        {
            StationId = parentStationEntity.Id,
            ActualState = randomizer.NextBool() // Actual switch state shall be known.
        };

        var stationsRepositoryStub = new Mock<IStationsRepository>();
        var switchesRepositoryStub = new Mock<ISwitchesRepository>();
        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryStub.Object,
            stationsRepositoryStub.Object,
            loggerMock);

        var request = new SwitchUpdateRequest(switchEntity.LocalId, switchEntity.ActualState.Value);
        IActionResult response = await controllerUnderTest.UpdateSwitch(request);

        response.AssertBadRequestResult();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public async Task UpdateReturnsNotFoundIfStationIsNotRegistered()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity unregisteredStationEntity = randomizer.NextStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(unregisteredStationEntity.IpAddress);

        SwitchEntity switchEntity = randomizer.NextSwitchEntity() with
        {
            StationId = unregisteredStationEntity.Id,
            ActualState = randomizer.NextBool() // Actual switch state shall be known.
        };

        var switchesRepositoryStub = new Mock<ISwitchesRepository>();
        var stationsRepositoryStub = new Mock<IStationsRepository>();
        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryStub.Object,
            stationsRepositoryStub.Object,
            loggerMock);

        var request = new SwitchUpdateRequest(switchEntity.LocalId, switchEntity.ActualState.Value);
        IActionResult response = await controllerUnderTest.UpdateSwitch(request);

        response.AssertNotFoundResult();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public async Task UpdateReturnsNotFoundIfSwitchIsNotRegistered()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity parentStationEntity = randomizer.NextStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(parentStationEntity.IpAddress);

        var stationsRepositoryStub = new Mock<IStationsRepository>();

        stationsRepositoryStub.Setup(mock =>
            mock.GetSingleStationAsync(
                filterById: It.IsAny<bool>(),
                id: It.IsAny<long?>(),
                filterByIpAddress: true,
                ipAddress: parentStationEntity.IpAddress,
                filterByMacAddress: It.IsAny<bool>(),
                macAddress: It.IsAny<PhysicalAddress?>()))
            .ReturnsAsync(parentStationEntity);

        SwitchEntity unregisteredSwitchEntity = randomizer.NextSwitchEntity() with
        {
            StationId = parentStationEntity.Id,
            ActualState = randomizer.NextBool() // Actual switch state shall be known.
        };

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            stationsRepositoryStub.Object,
            loggerMock);

        var request = new SwitchUpdateRequest(
            unregisteredSwitchEntity.LocalId,
            unregisteredSwitchEntity.ActualState.Value);

        IActionResult response = await controllerUnderTest.UpdateSwitch(request);

        response.AssertNotFoundResult();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }
    #endregion
}
