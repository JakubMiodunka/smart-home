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
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var loggerStub = new FakeLogger<SwitchesController>();

        TestDelegate actionUnderTest = () =>
            new SwitchesController(
                httpContextAccessorStub.Object,
                switchesRepositoryMock.Object,
                stationsRepositoryMock.Object,
                loggerStub);

        Assert.DoesNotThrow(actionUnderTest);
        stationsRepositoryMock.AssertThatNoDataModifications();
        switchesRepositoryMock.AssertThatNoDataModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsAsHttpContextAccessor()
    {
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var loggerStub = new FakeLogger<SwitchesController>();

        TestDelegate actionUnderTest = () =>
            new SwitchesController(
                null!,
                switchesRepositoryMock.Object,
                stationsRepositoryMock.Object,
                loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
        stationsRepositoryMock.AssertThatNoDataModifications();
        switchesRepositoryMock.AssertThatNoDataModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsSwitchesRepository()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var loggerStub = new FakeLogger<SwitchesController>();

        TestDelegate actionUnderTest = () =>
            new SwitchesController(
                httpContextAccessorStub.Object,
                null!,
                stationsRepositoryMock.Object,
                loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
        stationsRepositoryMock.AssertThatNoDataModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsStationsRepository()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerStub = new FakeLogger<SwitchesController>();

        TestDelegate actionUnderTest = () =>
            new SwitchesController(
                httpContextAccessorStub.Object,
                switchesRepositoryMock.Object,
                null!,
                loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
        switchesRepositoryMock.AssertThatNoDataModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsLogger()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();

        TestDelegate actionUnderTest = () =>
            new SwitchesController(
                httpContextAccessorStub.Object,
                switchesRepositoryMock.Object,
                stationsRepositoryMock.Object,
                null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
        stationsRepositoryMock.AssertThatNoDataModifications();
        switchesRepositoryMock.AssertThatNoDataModifications();
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

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        stationsRepositoryMock.Setup(mock =>
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
            stationsRepositoryMock.Object,
            loggerMock);

        var request = new SwitchRegistrationStationRequest(newSwitchEntity.LocalId);
        IActionResult response = await controllerUnderTest.RegisterSwitch(request);

        var expectedResponse = new SwitchRegistrationServerResponse(newSwitchEntity.Id, newSwitchEntity.ExpectedState);
        response.AssertOkObjectResult(expectedValue: expectedResponse);

        switchesRepositoryMock.Verify(mock => mock
            .CreateSwitchAsync(
                newSwitchEntity.StationId, 
                newSwitchEntity.LocalId,
                expectedState: false,
                actualState: null),
            Times.Once);

        switchesRepositoryMock.Verify(mock =>
            mock.UpdateSwitchAsync(
                It.IsAny<long>(),
                It.IsAny<bool>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool?>()),
            Times.Never);

        stationsRepositoryMock.AssertThatNoDataModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public async Task RegistrationOfKnownSwitchCausesReturnOfItsExpectedState()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity parentStationEntity = randomizer.NextStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(parentStationEntity.IpAddress);

        var stationsRepositorymock = new Mock<IStationsRepository>();

        stationsRepositorymock.Setup(mock =>
            mock.GetSingleStationAsync(
                filterById: It.IsAny<bool>(),
                id: It.IsAny<long?>(),
                filterByIpAddress: true,
                ipAddress: parentStationEntity.IpAddress,
                filterByMacAddress: It.IsAny<bool>(),
                macAddress: It.IsAny<PhysicalAddress?>()))
            .ReturnsAsync(parentStationEntity);

        SwitchEntity knownSwitchEntity = randomizer.NextSwitchEntity() with
        {
            StationId = parentStationEntity.Id,
            ExpectedState = false,   // By default, every switch should be open, ensuring that current does not flow.
            ActualState = null  // During entity creation actual state of the switch is unknown.
        };

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();

        switchesRepositoryMock.Setup(mock =>
            mock.GetSingleSwitchAsync(
                filterById: false,
                id: null,
                filterByStationId: true,
                stationId: parentStationEntity.Id,
                filterByLocalId: true,
                localId: knownSwitchEntity.LocalId))
            .ReturnsAsync(knownSwitchEntity);

        switchesRepositoryMock.Setup(mock => mock
            .CreateSwitchAsync(
                knownSwitchEntity.StationId,
                knownSwitchEntity.LocalId,
                knownSwitchEntity.ExpectedState,
                knownSwitchEntity.ActualState))
            .ReturnsAsync(knownSwitchEntity);

        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            stationsRepositorymock.Object,
            loggerMock);

        var request = new SwitchRegistrationStationRequest(knownSwitchEntity.LocalId);
        IActionResult response = await controllerUnderTest.RegisterSwitch(request);

        var expectedResponse = new SwitchRegistrationServerResponse(knownSwitchEntity.Id, knownSwitchEntity.ExpectedState);
        response.AssertOkObjectResult(expectedValue: expectedResponse);

        switchesRepositoryMock.AssertThatNoDataModifications();
        stationsRepositorymock.AssertThatNoDataModifications();

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

        SwitchEntity switchEntity = randomizer.NextSwitchEntity() with
        {
            StationId = parentStationEntity.Id
        };

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            stationsRepositoryMock.Object,
            loggerMock);

        var request = new SwitchRegistrationStationRequest(switchEntity.LocalId);
        IActionResult response = await controllerUnderTest.RegisterSwitch(request);
        
        response.AssertBadRequestResult();

        switchesRepositoryMock.AssertThatNoDataModifications();
        stationsRepositoryMock.AssertThatNoDataModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public async Task RegistrationReturnsNotFoundIfStationIsNotRegistered()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(stationEntity.IpAddress);

        SwitchEntity newSwitchEntity = randomizer.NextSwitchEntity() with
        {
            StationId = stationEntity.Id,
            ExpectedState = false,   // By default, every switch should be open, ensuring that current does not flow.
            ActualState = null  // During entity creation actual state of the switch is unknown.
        };

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            stationsRepositoryMock.Object,
            loggerMock);

        var request = new SwitchRegistrationStationRequest(newSwitchEntity.LocalId);
        IActionResult response = await controllerUnderTest.RegisterSwitch(request);

        response.AssertNotFoundResult();

        switchesRepositoryMock.AssertThatNoDataModifications();
        stationsRepositoryMock.AssertThatNoDataModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }
    #endregion

    #region Switch udapte
    [Test]
    public async Task UpdateOfKnownSwitchCausesUpdateOfExistingSwitchEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(stationEntity.IpAddress);

        var stationsRepositoryStub = new Mock<IStationsRepository>();

        stationsRepositoryStub.Setup(mock =>
            mock.GetSingleStationAsync(
                filterById: It.IsAny<bool>(),
                id: It.IsAny<long?>(),
                filterByIpAddress: true,
                ipAddress: stationEntity.IpAddress,
                filterByMacAddress: It.IsAny<bool>(),
                macAddress: It.IsAny<PhysicalAddress?>()))
            .ReturnsAsync(stationEntity);

        SwitchEntity switchEntityBeforeUpdate = randomizer.NextSwitchEntity() with
        {
            StationId = stationEntity.Id
        };

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();

        switchesRepositoryMock.Setup(mock =>
            mock.GetSingleSwitchAsync(
                filterById: true,
                id: switchEntityBeforeUpdate.Id,
                filterByStationId: true,
                switchEntityBeforeUpdate.StationId,
                filterByLocalId: It.IsAny<bool>(),
                localId: It.IsAny<byte?>()))
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

        var request = new SwitchUpdateStationRequest(switchEntityAfterUpdate.ActualState.Value);
        IActionResult response = await controllerUnderTest.UpdateSwitch(switchEntityAfterUpdate.Id, request);

        response.AssertNoContentResult();

        switchesRepositoryMock.Verify(mock =>
            mock.CreateSwitchAsync(
                It.IsAny<long>(),
                It.IsAny<byte>(),
                It.IsAny<bool>(),
                It.IsAny<bool?>()),
            Times.Never);

        switchesRepositoryMock.Verify(mock => mock
            .UpdateSwitchAsync(
                switchEntityAfterUpdate.Id,
                updateExpectedState: It.IsAny<bool>(),
                expectedState: It.IsAny<bool?>(),
                updateActualState: true,
                actualState: switchEntityAfterUpdate.ActualState),
            Times.Once);

        stationsRepositoryStub.AssertThatNoDataModifications();

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

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            stationsRepositoryMock.Object,
            loggerMock);

        var request = new SwitchUpdateStationRequest(switchEntity.ActualState.Value);
        IActionResult response = await controllerUnderTest.UpdateSwitch(switchEntity.Id, request);

        response.AssertBadRequestResult();

        switchesRepositoryMock.AssertThatNoDataModifications();
        stationsRepositoryMock.AssertThatNoDataModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }

    [Test]
    public async Task UpdateReturnsNotFoundIfStationCannotBeFound()
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

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            stationsRepositoryMock.Object,
            loggerMock);

        var request = new SwitchUpdateStationRequest(switchEntity.ActualState.Value);
        IActionResult response = await controllerUnderTest.UpdateSwitch(switchEntity.Id, request);

        response.AssertNotFoundResult();

        switchesRepositoryMock.AssertThatNoDataModifications();
        stationsRepositoryMock.AssertThatNoDataModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }

    [Test]
    public async Task UpdateReturnsNotFoundIfSwitchCannotBeFound()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity parentStationEntity = randomizer.NextStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(parentStationEntity.IpAddress);

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        stationsRepositoryMock.Setup(mock =>
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
            stationsRepositoryMock.Object,
            loggerMock);

        var request = new SwitchUpdateStationRequest(unregisteredSwitchEntity.ActualState.Value);
        IActionResult response = await controllerUnderTest.UpdateSwitch(unregisteredSwitchEntity.Id, request);

        response.AssertNotFoundResult();

        switchesRepositoryMock.AssertThatNoDataModifications();
        stationsRepositoryMock.AssertThatNoDataModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }

    [Test]
    public async Task UpdateReturnsInternalServerErrorIfRepositoryUpdateFails()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity();

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            TestDataGenerator.CreateHttpContextAccessorFake(stationEntity.IpAddress);

        var stationsRepositoryStub = new Mock<IStationsRepository>();

        stationsRepositoryStub.Setup(mock =>
            mock.GetSingleStationAsync(
                filterById: It.IsAny<bool>(),
                id: It.IsAny<long?>(),
                filterByIpAddress: true,
                ipAddress: stationEntity.IpAddress,
                filterByMacAddress: It.IsAny<bool>(),
                macAddress: It.IsAny<PhysicalAddress?>()))
            .ReturnsAsync(stationEntity);

        SwitchEntity switchEntityBeforeUpdate = randomizer.NextSwitchEntity() with
        {
            StationId = stationEntity.Id
        };

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();

        switchesRepositoryMock.Setup(mock =>
            mock.GetSingleSwitchAsync(
                filterById: true,
                id: switchEntityBeforeUpdate.Id,
                filterByStationId: true,
                switchEntityBeforeUpdate.StationId,
                filterByLocalId: It.IsAny<bool>(),
                localId: It.IsAny<byte?>()))
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

        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            stationsRepositoryStub.Object,
            loggerMock);

        var request = new SwitchUpdateStationRequest(switchEntityAfterUpdate.ActualState.Value);
        IActionResult response = await controllerUnderTest.UpdateSwitch(switchEntityAfterUpdate.Id, request);

        response.AssertInternalServerError();

        switchesRepositoryMock.Verify(mock =>
            mock.CreateSwitchAsync(
                It.IsAny<long>(),
                It.IsAny<byte>(),
                It.IsAny<bool>(),
                It.IsAny<bool?>()),
            Times.Never);

        switchesRepositoryMock.Verify(mock => mock
            .UpdateSwitchAsync(
                switchEntityAfterUpdate.Id,
                updateExpectedState: It.IsAny<bool>(),
                expectedState: It.IsAny<bool?>(),
                updateActualState: true,
                actualState: switchEntityAfterUpdate.ActualState),
            Times.Once);

        stationsRepositoryStub.AssertThatNoDataModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }
    #endregion
}
