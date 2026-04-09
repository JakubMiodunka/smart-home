using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Models.Requests;
using SmartHome.Server.Data.Repositories;
using SmartHome.Server.Managers;
using System.Net;

namespace SmartHome.UnitTests.Server.Managers;

[Category("UnitTest")]
[TestOf(typeof(SwitchManager))]
[Author("Jakub Miodunka")]
public sealed class SwitchManagerTests
{
    #region Constructor
    [Test]
    public void InstantiationPossible()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerStub = new FakeLogger<SwitchManager>();

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();

        TestDelegate actionUnderTest = () => new SwitchManager(
            switchEntity,
            httpClientFactoryStub.Object,
            stationsRepositoryMock.Object,
            switchesRepositoryMock.Object,
            loggerStub);

        Assert.DoesNotThrow(actionUnderTest);

        stationsRepositoryMock.AssertNoContentModifications();
        switchesRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsHttpClientFactory()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerStub = new FakeLogger<SwitchManager>();

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();

        TestDelegate actionUnderTest = () => new SwitchManager(
            switchEntity,
            null!,
            stationsRepositoryMock.Object,
            switchesRepositoryMock.Object,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);

        stationsRepositoryMock.AssertNoContentModifications();
        switchesRepositoryMock.AssertNoContentModifications();
    }

    public void InstantiationImpossibleUsingNullReferenceAsStationsRepository()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerStub = new FakeLogger<SwitchManager>();

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();

        TestDelegate actionUnderTest = () => new SwitchManager(
            switchEntity,
            httpClientFactoryStub.Object,
            null!,
            switchesRepositoryMock.Object,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);

        switchesRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsSwitchesRepository()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var loggerStub = new FakeLogger<SwitchManager>();

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();

        TestDelegate actionUnderTest = () => new SwitchManager(
            switchEntity,
            httpClientFactoryStub.Object,
            stationsRepositoryMock.Object,
            null!,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);

        stationsRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsLogger()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();

        TestDelegate actionUnderTest = () => new SwitchManager(
            switchEntity,
            httpClientFactoryStub.Object,
            stationsRepositoryMock.Object,
            switchesRepositoryMock.Object,
            null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);

        stationsRepositoryMock.AssertNoContentModifications();
        switchesRepositoryMock.AssertNoContentModifications();
    }
    #endregion

    #region Switch state change
    [Test]
    public async Task SwitchStateChangeSuccessfulExpectedStateIsNotEqualToActualState()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var httpMessageHandlerMock = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var httpClient = new HttpClient(httpMessageHandlerMock);
        var httpClientFactoryStub = new Mock<IHttpClientFactory>();

        httpClientFactoryStub.Setup(factory => factory
            .CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        StationEntity stationEntity = randomizer.NextStationEntity() with
        {
            ApiVersion = 1
        };

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        stationsRepositoryMock.Setup(repository => repository
            .GetSingleStationAsync(
                filterById: true,
                id: stationEntity.Id,
                filterByIpAddress: false,
                ipAddress: null,
                filterByMacAddress: false,
                macAddress: null))
            .ReturnsAsync(stationEntity);

        SwitchEntity switchEntityBeforeUpdate = randomizer.NextSwitchEntity();
        switchEntityBeforeUpdate = switchEntityBeforeUpdate with
        {
            StationId = stationEntity.Id,
            ActualState = !switchEntityBeforeUpdate.ExpectedState
        };

        SwitchEntity updatedSwitchEntity = switchEntityBeforeUpdate with
        {
            ActualState = switchEntityBeforeUpdate.ExpectedState
        };

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();

        switchesRepositoryMock.Setup(repository => repository
            .UpdateSwitchAsync(
                    switchEntityBeforeUpdate.Id,
                    updateExpectedState: true,
                    expectedState: updatedSwitchEntity.ExpectedState,
                    updateActualState: true,
                    actualState: updatedSwitchEntity.ActualState))
            .ReturnsAsync(updatedSwitchEntity);

        var loggerMock = new FakeLogger<SwitchManager>();

        var managerUnderTest = new SwitchManager(
            switchEntityBeforeUpdate,
            httpClientFactoryStub.Object,
            stationsRepositoryMock.Object,
            switchesRepositoryMock.Object,
            loggerMock);

        Assert.That(managerUnderTest.ManagedSwitch, Is.EqualTo(switchEntityBeforeUpdate));

        bool wasAttemptSuccessful = await managerUnderTest.TryChangeState(switchEntityBeforeUpdate.ExpectedState, CancellationToken.None);

        Assert.That(wasAttemptSuccessful, Is.True);
        Assert.That(managerUnderTest.ManagedSwitch, Is.EqualTo(updatedSwitchEntity));

        Assert.That(httpMessageHandlerMock.SentRequests, Has.Exactly(1).Items);

        RequestSnapshot request = httpMessageHandlerMock.SentRequests.Single();

        await request.AssertJsonRequest(
            expectedUri: switchEntityBeforeUpdate.SwitchUrl(stationEntity)!,
            expectedHttpMethod: HttpMethod.Patch,
            expectedRequestBody: new SwitchUpdateServerRequest(switchEntityBeforeUpdate.ExpectedState));

        stationsRepositoryMock.AssertNoContentModifications();

        switchesRepositoryMock.Verify(mock =>
            mock.CreateSwitchAsync(
                It.IsAny<long>(),
                It.IsAny<byte>(),
                It.IsAny<bool>(),
                It.IsAny<bool?>()),
            Times.Never);

        switchesRepositoryMock.Verify(repository => repository
            .UpdateSwitchAsync(
                switchEntityBeforeUpdate.Id,
                updateExpectedState: true,
                expectedState: updatedSwitchEntity.ExpectedState,
                updateActualState: true,
                actualState: updatedSwitchEntity.ActualState),
            Times.Once);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public async Task SwitchStateChangeSuccessfulWithoutSendingHttpRequestIfExpectedSwitchStateIsEqualToItsActualState()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var delegatingHandlerMock = new FakeHttpMessageHandler();
        var httpClient = new HttpClient(delegatingHandlerMock);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();

        httpClientFactoryStub.Setup(factory => factory
            .CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerMock = new FakeLogger<SwitchManager>();

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();
        switchEntity = switchEntity with { ActualState = switchEntity.ExpectedState };

        var managerUnderTest = new SwitchManager(
            switchEntity,
            httpClientFactoryStub.Object,
            stationsRepositoryMock.Object,
            switchesRepositoryMock.Object,
            loggerMock);

        bool wasAttemptSuccessful = await managerUnderTest.TryChangeState(switchEntity.ExpectedState, CancellationToken.None);

        Assert.That(wasAttemptSuccessful, Is.True);
        Assert.That(delegatingHandlerMock.WasAnyRequestSent, Is.False);

        stationsRepositoryMock.AssertNoContentModifications();
        switchesRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public async Task SwitchStateChangeFailsIfSwitchParentStationNotFound()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();     
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerMock = new FakeLogger<SwitchManager>();

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();
        switchEntity = switchEntity with
        {
            ActualState = !switchEntity.ExpectedState
        };

        var managerUnderTest = new SwitchManager(
            switchEntity,
            httpClientFactoryStub.Object,
            stationsRepositoryMock.Object,
            switchesRepositoryMock.Object,
            loggerMock);

        Assert.That(managerUnderTest.ManagedSwitch, Is.EqualTo(switchEntity));

        bool wasAttemptSuccessful = await managerUnderTest.TryChangeState(switchEntity.ExpectedState, CancellationToken.None);

        Assert.That(wasAttemptSuccessful, Is.False);
        Assert.That(managerUnderTest.ManagedSwitch, Is.EqualTo(switchEntity));

        stationsRepositoryMock.AssertNoContentModifications();
        switchesRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Error));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Error < record.Level));
    }

    [Test]
    public async Task SwitchStateChangeFailsIfRequestCannotBeSent()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        StationEntity stationEntity = randomizer.NextStationEntity() with
        {
            IpAddress = null,
            ApiPort = null,
            ApiVersion = null
        };

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        stationsRepositoryMock.Setup(repository => repository
            .GetSingleStationAsync(
                filterById: true,
                id: stationEntity.Id,
                filterByIpAddress: false,
                ipAddress: null,
                filterByMacAddress: false,
                macAddress: null))
            .ReturnsAsync(stationEntity);

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();
        switchEntity = switchEntity with
        {
            StationId = stationEntity.Id,
            ActualState = !switchEntity.ExpectedState
        };

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerMock = new FakeLogger<SwitchManager>();

        var managerUnderTest = new SwitchManager(
            switchEntity,
            httpClientFactoryStub.Object,
            stationsRepositoryMock.Object,
            switchesRepositoryMock.Object,
            loggerMock);

        Assert.That(managerUnderTest.ManagedSwitch, Is.EqualTo(switchEntity));

        bool wasAttemptSuccessful = await managerUnderTest.TryChangeState(switchEntity.ExpectedState, CancellationToken.None);

        Assert.That(wasAttemptSuccessful, Is.False);
        Assert.That(managerUnderTest.ManagedSwitch, Is.EqualTo(switchEntity));

        stationsRepositoryMock.AssertNoContentModifications();
        switchesRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }

    [Test]
    public async Task SwitchStateChangeFailsIfReceivedUnexpectedResponse()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        // HTTP 404 NotFound typically indicates that remote is reachable but called endpoint is not found.
        var httpMessageHandlerMock = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var httpClient = new HttpClient(httpMessageHandlerMock);
        var httpClientFactoryStub = new Mock<IHttpClientFactory>();

        httpClientFactoryStub.Setup(factory => factory
            .CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        StationEntity stationEntity = randomizer.NextStationEntity() with
        {
            ApiVersion = 1
        };

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        stationsRepositoryMock.Setup(repository => repository
            .GetSingleStationAsync(
                filterById: true,
                id: stationEntity.Id,
                filterByIpAddress: false,
                ipAddress: null,
                filterByMacAddress: false,
                macAddress: null))
            .ReturnsAsync(stationEntity);

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();
        switchEntity = switchEntity with
        {
            StationId = stationEntity.Id,
            ActualState = !switchEntity.ExpectedState
        };

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerMock = new FakeLogger<SwitchManager>();

        var managerUnderTest = new SwitchManager(
            switchEntity,
            httpClientFactoryStub.Object,
            stationsRepositoryMock.Object,
            switchesRepositoryMock.Object,
            loggerMock);

        Assert.That(managerUnderTest.ManagedSwitch, Is.EqualTo(switchEntity));

        bool wasAttemptSuccessful = await managerUnderTest.TryChangeState(switchEntity.ExpectedState, CancellationToken.None);

        Assert.That(wasAttemptSuccessful, Is.False);
        Assert.That(managerUnderTest.ManagedSwitch, Is.EqualTo(switchEntity));

        Assert.That(httpMessageHandlerMock.SentRequests, Has.Exactly(1).Items);

        stationsRepositoryMock.AssertNoContentModifications();
        switchesRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Error));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Error < record.Level));
    }

    [Test]
    public async Task SwitchStateChangeFailsIfHttpClientThrowsHttpRequestException()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var httpMessageHandlerMock = new FakeHttpMessageHandler(_ => throw new HttpRequestException());
        var httpClient = new HttpClient(httpMessageHandlerMock);
        var httpClientFactoryStub = new Mock<IHttpClientFactory>();

        httpClientFactoryStub.Setup(factory => factory
            .CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        StationEntity stationEntity = randomizer.NextStationEntity() with
        {
            ApiVersion = 1
        };

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        stationsRepositoryMock.Setup(repository => repository
            .GetSingleStationAsync(
                filterById: true,
                id: stationEntity.Id,
                filterByIpAddress: false,
                ipAddress: null,
                filterByMacAddress: false,
                macAddress: null))
            .ReturnsAsync(stationEntity);

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();
        switchEntity = switchEntity with
        {
            StationId = stationEntity.Id,
            ActualState = !switchEntity.ExpectedState
        };

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerMock = new FakeLogger<SwitchManager>();

        var managerUnderTest = new SwitchManager(
            switchEntity,
            httpClientFactoryStub.Object,
            stationsRepositoryMock.Object,
            switchesRepositoryMock.Object,
            loggerMock);

        Assert.That(managerUnderTest.ManagedSwitch, Is.EqualTo(switchEntity));

        bool wasAttemptSuccessful = await managerUnderTest.TryChangeState(switchEntity.ExpectedState, CancellationToken.None);

        Assert.That(wasAttemptSuccessful, Is.False);
        Assert.That(managerUnderTest.ManagedSwitch, Is.EqualTo(switchEntity));

        Assert.That(httpMessageHandlerMock.SentRequests, Has.Exactly(1).Items);

        stationsRepositoryMock.AssertNoContentModifications();
        switchesRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Error));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Error < record.Level));
    }

    [Test]
    public async Task SwitchStateChangeFailsIfHttpClientThrowsTimeoutException()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var httpMessageHandlerMock = new FakeHttpMessageHandler(_ => 
            throw new OperationCanceledException(string.Empty, new TimeoutException()));

        var httpClient = new HttpClient(httpMessageHandlerMock);
        var httpClientFactoryStub = new Mock<IHttpClientFactory>();

        httpClientFactoryStub.Setup(factory => factory
            .CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        StationEntity stationEntity = randomizer.NextStationEntity() with
        {
            ApiVersion = 1
        };

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        stationsRepositoryMock.Setup(repository => repository
            .GetSingleStationAsync(
                filterById: true,
                id: stationEntity.Id,
                filterByIpAddress: false,
                ipAddress: null,
                filterByMacAddress: false,
                macAddress: null))
            .ReturnsAsync(stationEntity);

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();
        switchEntity = switchEntity with
        {
            StationId = stationEntity.Id,
            ActualState = !switchEntity.ExpectedState
        };

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerMock = new FakeLogger<SwitchManager>();

        var managerUnderTest = new SwitchManager(
            switchEntity,
            httpClientFactoryStub.Object,
            stationsRepositoryMock.Object,
            switchesRepositoryMock.Object,
            loggerMock);

        Assert.That(managerUnderTest.ManagedSwitch, Is.EqualTo(switchEntity));

        bool wasAttemptSuccessful = await managerUnderTest.TryChangeState(switchEntity.ExpectedState, CancellationToken.None);

        Assert.That(wasAttemptSuccessful, Is.False);
        Assert.That(managerUnderTest.ManagedSwitch, Is.EqualTo(switchEntity));

        Assert.That(httpMessageHandlerMock.SentRequests, Has.Exactly(1).Items);

        stationsRepositoryMock.AssertNoContentModifications();
        switchesRepositoryMock.AssertNoContentModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }

    [Test]
    public async Task SwitchStateChangeFailsIfSwitchRepositoryUpdateFails()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var httpMessageHandlerStub = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var httpClient = new HttpClient(httpMessageHandlerStub);
        var httpClientFactoryStub = new Mock<IHttpClientFactory>();

        httpClientFactoryStub.Setup(factory => factory
            .CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        StationEntity stationEntity = randomizer.NextStationEntity() with
        {
            ApiVersion = 1
        };

        var stationsRepositoryMock = new Mock<IStationsRepository>();

        stationsRepositoryMock.Setup(repository => repository
            .GetSingleStationAsync(
                filterById: true,
                id: stationEntity.Id,
                filterByIpAddress: false,
                ipAddress: null,
                filterByMacAddress: false,
                macAddress: null))
            .ReturnsAsync(stationEntity);

        SwitchEntity switchEntityBeforeUpdate = randomizer.NextSwitchEntity();
        switchEntityBeforeUpdate = switchEntityBeforeUpdate with
        {
            StationId = stationEntity.Id,
            ActualState = !switchEntityBeforeUpdate.ExpectedState
        };

        SwitchEntity updatedSwitchEntity = switchEntityBeforeUpdate with
        {
            ActualState = switchEntityBeforeUpdate.ExpectedState
        };

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerMock = new FakeLogger<SwitchManager>();

        var managerUnderTest = new SwitchManager(
            switchEntityBeforeUpdate,
            httpClientFactoryStub.Object,
            stationsRepositoryMock.Object,
            switchesRepositoryMock.Object,
            loggerMock);

        Assert.That(managerUnderTest.ManagedSwitch, Is.EqualTo(switchEntityBeforeUpdate));

        bool wasAttemptSuccessful = await managerUnderTest.TryChangeState(switchEntityBeforeUpdate.ExpectedState, CancellationToken.None);

        Assert.That(wasAttemptSuccessful, Is.False);
        Assert.That(managerUnderTest.ManagedSwitch, Is.EqualTo(switchEntityBeforeUpdate));

        stationsRepositoryMock.AssertNoContentModifications();

        switchesRepositoryMock.Verify(mock =>
            mock.CreateSwitchAsync(
                It.IsAny<long>(),
                It.IsAny<byte>(),
                It.IsAny<bool>(),
                It.IsAny<bool?>()),
            Times.Never);

        switchesRepositoryMock.Verify(repository => repository
            .UpdateSwitchAsync(
                switchEntityBeforeUpdate.Id,
                updateExpectedState: true,
                expectedState: updatedSwitchEntity.ExpectedState,
                updateActualState: true,
                actualState: updatedSwitchEntity.ActualState),
            Times.Once);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Error));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Error < record.Level));
    }
    #endregion
}
