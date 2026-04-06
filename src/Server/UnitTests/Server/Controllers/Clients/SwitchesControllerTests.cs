using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Controllers.Clients;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Models.Requests;
using SmartHome.Server.Data.Repositories;
using SmartHome.Server.Managers;
using SmartHome.Server.Managers.Factories;
using SmartHome.UnitTests;
using System.Net;

namespace UnitTests.Server.Controllers.Clients;

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
        var switchManagerFactoryStub = new Mock<ISwitchManagerFactory>();
        var loggerStub = new FakeLogger<SwitchesController>();

        TestDelegate actionUnderTest = () => new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            switchManagerFactoryStub.Object,
            loggerStub);

        Assert.DoesNotThrow(actionUnderTest);
        switchesRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsHttpContextAccessor()
    {
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var switchManagerFactoryStub = new Mock<ISwitchManagerFactory>();
        var loggerStub = new FakeLogger<SwitchesController>();

        TestDelegate actionUnderTest = () => new SwitchesController(
            null!,
            switchesRepositoryMock.Object,
            switchManagerFactoryStub.Object,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
        switchesRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsSwitchesRepository()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var switchManagerFactoryStub = new Mock<ISwitchManagerFactory>();
        var loggerStub = new FakeLogger<SwitchesController>();

        TestDelegate actionUnderTest = () => new SwitchesController(
            httpContextAccessorStub.Object,
            null!,
            switchManagerFactoryStub.Object,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsSwitchManagerFactory()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerStub = new FakeLogger<SwitchesController>();

        TestDelegate actionUnderTest = () => new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            null!,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
        switchesRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsLogger()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var switchManagerFactoryStub = new Mock<ISwitchManagerFactory>();

        TestDelegate actionUnderTest = () => new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            switchManagerFactoryStub.Object,
            null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
        switchesRepositoryMock.AssertNoContentModifications();
    }
    #endregion

    #region Switch retrival
    [Test]
    public async Task GettingSingleSwitchPossible()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress clientIpAddress = randomizer.NextIpAddress();
        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(clientIpAddress);

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();

        switchesRepositoryMock.Setup(mock => mock
            .GetSingleSwitchAsync(
                filterById: true,
                id: switchEntity.Id,
                filterByStationId: false,
                stationId: null,
                filterByLocalId: false,
                localId: null))
            .ReturnsAsync(switchEntity);

        var switchManagerFactoryStub = new Mock<ISwitchManagerFactory>();
        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            switchManagerFactoryStub.Object,
            loggerMock);

        IActionResult response = await controllerUnderTest.GetSwitch(switchEntity.Id);
        response.AssertOkObjectResult(expectedValue: switchEntity);

        switchesRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public async Task GettingSingleSwitchReturnsNotFoundIfSwitchDoesNotExist()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress clientIpAddress = randomizer.NextIpAddress();
        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(clientIpAddress);

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();

        switchesRepositoryMock.Setup(mock => mock
            .GetSingleSwitchAsync(
                filterById: true,
                id: switchEntity.Id,
                filterByStationId: false,
                stationId: null,
                filterByLocalId: false,
                localId: null))
            .ReturnsAsync(switchEntity);

        var switchManagerFactoryStub = new Mock<ISwitchManagerFactory>();
        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            switchManagerFactoryStub.Object,
            loggerMock);

        long nonExistingSwitchEntityId = default;
        while (switchEntity.Id == nonExistingSwitchEntityId)
        {
            nonExistingSwitchEntityId = randomizer.NextInt64(1, long.MaxValue);
        }

        IActionResult response = await controllerUnderTest.GetSwitch(nonExistingSwitchEntityId);
        response.AssertNotFoundResult();

        switchesRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }

    [Test]
    public async Task GettingSingleSwitchReturnsBadRequestIfClientIpAddressCannotBeDetermined()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(remoteIpAddress: null);

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();

        switchesRepositoryMock.Setup(mock => mock
            .GetSingleSwitchAsync(
                filterById: true,
                id: switchEntity.Id,
                filterByStationId: false,
                stationId: null,
                filterByLocalId: false,
                localId: null))
            .ReturnsAsync(switchEntity);

        var switchManagerFactoryStub = new Mock<ISwitchManagerFactory>();
        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            switchManagerFactoryStub.Object,
            loggerMock);

        IActionResult response = await controllerUnderTest.GetSwitch(switchEntity.Id);
        response.AssertBadRequestResult();

        switchesRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }

    [TestCase(2)]
    public async Task GettingMultipleSwitchesPossible(int switchesInRepository)
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress clientIpAddress = randomizer.NextIpAddress();
        Mock<IHttpContextAccessor> httpContextAccessorStub = 
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(clientIpAddress);

        SwitchEntity[] repositoryContent = Enumerable.Range(1, switchesInRepository)
            .Select(id => randomizer.NextSwitchEntity() with { Id = id })
            .ToArray();

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();

        switchesRepositoryMock.Setup(mock => mock
            .GetMultipleSwitchesAsync())
            .ReturnsAsync(repositoryContent);

        var switchManagerFactoryStub = new Mock<ISwitchManagerFactory>();
        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            switchManagerFactoryStub.Object,
            loggerMock);

        IActionResult response = await controllerUnderTest.GetSwitches();
        response.AssertOkObjectResult(expectedValue: repositoryContent);

        switchesRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));

    }

    [Test]
    public async Task GettingMultipleSwitchesReturnsBadRequestIfClientIpAddressCannotBeDetermined()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(remoteIpAddress: null);

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var switchManagerFactoryStub = new Mock<ISwitchManagerFactory>();
        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            switchManagerFactoryStub.Object,
            loggerMock);

        IActionResult response = await controllerUnderTest.GetSwitches();
        response.AssertBadRequestResult();

        switchesRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }
    #endregion

    #region Switch update
    // TODO: Finish this section.
    [Test]
    public async Task UpdatePossible()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress clientIpAddress = randomizer.NextIpAddress();
        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(clientIpAddress);

        bool expectedState = randomizer.NextBool();
        bool? actualState = randomizer.NextBool() ? !expectedState : null;
        SwitchEntity switchEntityBeforeUpdate = randomizer.NextSwitchEntity() with
        {
            ExpectedState = expectedState,
            ActualState = actualState
        };

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();

        switchesRepositoryMock.Setup(mock => mock
            .GetSingleSwitchAsync(
                filterById: true,
                id: switchEntityBeforeUpdate.Id,
                filterByStationId: false,
                stationId: null,
                filterByLocalId: false,
                localId: null))
            .ReturnsAsync(switchEntityBeforeUpdate);

        SwitchEntity updatedSwitchEntity = switchEntityBeforeUpdate with
        { 
            ActualState = switchEntityBeforeUpdate.ExpectedState
        };

        var switchManagerMock = new Mock<ISwitchManager>();
        switchManagerMock.SetupGet(mock => mock.ManagedSwitch).Returns(updatedSwitchEntity);    // TODO: Get rid of it.

        switchManagerMock.Setup(mock => 
            mock.TryChangeState(
                switchEntityBeforeUpdate.ExpectedState,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var switchManagerFactoryStub = new Mock<ISwitchManagerFactory>();

        switchManagerFactoryStub.Setup(
                mock => mock.CreateFor(switchEntityBeforeUpdate))
            .Returns(switchManagerMock.Object);

        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            switchManagerFactoryStub.Object,
            loggerMock);

        var request = new SwitchUpdateClientRequest(switchEntityBeforeUpdate.ExpectedState);
        IActionResult response = await controllerUnderTest.UpdateSwitch(switchEntityBeforeUpdate.Id, request, CancellationToken.None);
        response.AssertNoContentResult();

        switchManagerMock.Verify(mock =>
            mock.TryChangeState(
                switchEntityBeforeUpdate.ExpectedState,
                It.IsAny<CancellationToken>()),
            Times.Once);

        switchesRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public async Task UpdateReturnsBadRequestIfClientIpAddressCannotBeDetermined()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(remoteIpAddress: null);

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var switchManagerFactoryStub = new Mock<ISwitchManagerFactory>();
        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            switchManagerFactoryStub.Object,
            loggerMock);

        var request = new SwitchUpdateClientRequest(switchEntity.ExpectedState);
        IActionResult response = await controllerUnderTest.UpdateSwitch(switchEntity.Id, request, CancellationToken.None);
        response.AssertBadRequestResult();

        switchesRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }

    [Test]
    public async Task UpdateReturnsNotFoundIfSwitchDoesNotExist()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress clientIpAddress = randomizer.NextIpAddress();
        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(clientIpAddress);

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var switchManagerStub = new Mock<ISwitchManager>();
        var switchManagerFactoryStub = new Mock<ISwitchManagerFactory>();
        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            switchManagerFactoryStub.Object,
            loggerMock);

        long nonExistingSwitchId = randomizer.NextInt64(1, long.MaxValue);
        bool expectedState = randomizer.NextBool();

        var request = new SwitchUpdateClientRequest(expectedState);
        IActionResult response = await controllerUnderTest.UpdateSwitch(nonExistingSwitchId, request, CancellationToken.None);
        response.AssertNotFoundResult();

        switchesRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }

    [Test]
    public async Task UpdateReturnsServiceUnavailableIfCannotChangeSwitchState()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress clientIpAddress = randomizer.NextIpAddress();
        Mock<IHttpContextAccessor> httpContextAccessorStub =
            FakeDataGenerationUtilities.CreateHttpContextAccessorFake(clientIpAddress);

        bool expectedState = randomizer.NextBool();
        bool? actualState = randomizer.NextBool() ? !expectedState : null;
        SwitchEntity switchEntity = randomizer.NextSwitchEntity() with
        {
            ExpectedState = expectedState,
            ActualState = actualState
        };

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();

        switchesRepositoryMock.Setup(mock => mock
            .GetSingleSwitchAsync(
                filterById: true,
                id: switchEntity.Id,
                filterByStationId: false,
                stationId: null,
                filterByLocalId: false,
                localId: null))
            .ReturnsAsync(switchEntity);

        var switchManagerMock = new Mock<ISwitchManager>();
        switchManagerMock.SetupGet(mock => mock.ManagedSwitch).Returns(switchEntity);    // TODO: Get rid of it.

        var switchManagerFactoryStub = new Mock<ISwitchManagerFactory>();
        
        switchManagerFactoryStub.Setup(
                mock => mock.CreateFor(switchEntity))
            .Returns(switchManagerMock.Object);

        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            switchManagerFactoryStub.Object,
            loggerMock);

        var request = new SwitchUpdateClientRequest(switchEntity.ExpectedState);
        IActionResult response = await controllerUnderTest.UpdateSwitch(switchEntity.Id, request, CancellationToken.None);

        response.AssertStatusCodeResult(StatusCodes.Status503ServiceUnavailable);

        switchManagerMock.Verify(mock =>
            mock.TryChangeState(
                switchEntity.ExpectedState,
                It.IsAny<CancellationToken>()),
            Times.Once);

        switchesRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }
    #endregion
}
