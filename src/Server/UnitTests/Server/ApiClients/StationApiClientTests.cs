using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.ApiClients.StationApi;
using SmartHome.Server.Data.Models.Entities;
using System.Net;
using static SmartHome.UnitTests.FakeDataGenerationUtilities;

namespace SmartHome.UnitTests.Server.ApiClients;

[Category("UnitTest")]
[TestOf(typeof(StationApiClient))]
[Author("Jakub Miodunka")]
public sealed class StationApiClientTests
{
    #region Constructor
    [Test]
    public void InstantiationPossible()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;
        
        StationEntity stationEntity = randomizer.NextStationEntity();
        TimeSpan timeout = randomizer.NextTimeSpan(from: StationApiClient.MinTimeout, to: StationApiClient.MaxTimeout);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var loggerStub = new FakeLogger<StationApiClient>();

        TestDelegate actionUnderTest = () => new StationApiClient(
            stationEntity,
            httpClientFactoryStub.Object,
            timeout,
            loggerStub);

        Assert.DoesNotThrow(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsHttpClientFactory()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity();
        TimeSpan timeout = randomizer.NextTimeSpan(from: StationApiClient.MinTimeout, to: StationApiClient.MaxTimeout);

        var loggerStub = new FakeLogger<StationApiClient>();

        TestDelegate actionUnderTest = () => new StationApiClient(
            stationEntity,
            null!,
            timeout,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsStationEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        TimeSpan timeout = randomizer.NextTimeSpan(from: StationApiClient.MinTimeout, to: StationApiClient.MaxTimeout);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var loggerStub = new FakeLogger<StationApiClient>();

        TestDelegate actionUnderTest = () => new StationApiClient(
            null!,
            httpClientFactoryStub.Object,
            timeout,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsLogger()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity();
        TimeSpan timeout = randomizer.NextTimeSpan(from: StationApiClient.MinTimeout, to: StationApiClient.MaxTimeout);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();

        TestDelegate actionUnderTest = () => new StationApiClient(
            stationEntity,
            httpClientFactoryStub.Object,
            timeout,
            null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [TestCase(10)]  // Equl to 1 microsecond.
    [TestCase(int.MaxValue)]
    public void InstantiationPossibleUsingValidTimeout(long timeoutTicks)
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity();
        var timeout = TimeSpan.FromTicks(timeoutTicks);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var loggerStub = new FakeLogger<StationApiClient>();

        TestDelegate actionUnderTest = () => new StationApiClient(
            stationEntity,
            httpClientFactoryStub.Object,
            timeout,
            loggerStub);

        Assert.DoesNotThrow(actionUnderTest);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(2147483648)] // Equal to: (int.MaxValue + 1)
    public void InstantiationImpossibleUsingInvalidTimeout(long timeoutTicks)
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity();
        var timeout = TimeSpan.FromTicks(timeoutTicks);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var loggerStub = new FakeLogger<StationApiClient>();

        TestDelegate actionUnderTest = () => new StationApiClient(
            stationEntity,
            httpClientFactoryStub.Object,
            timeout,
            loggerStub);

        Assert.Throws<ArgumentOutOfRangeException>(actionUnderTest);
    }
    #endregion

    #region Request sending
    [Test]
    public async Task SendingRequestPossible()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity();

        HttpStatusCode responseStatusCode = randomizer.NextSuccessfulHttpStatusCode();
        var httpMessageHandlerMock = new FakeHttpMessageHandler(_ => new HttpResponseMessage(responseStatusCode));
        var httpClient = new HttpClient(httpMessageHandlerMock);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        httpClientFactoryStub.Setup(factory => factory
            .CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var loggerMock = new FakeLogger<StationApiClient>();

        var clientUnderTest = new StationApiClient(
            stationEntity,
            httpClientFactoryStub.Object,
            StationApiClient.MaxTimeout,
            loggerMock);

        Uri endpointUrl = randomizer.NextHttpUrl();
        var httpMethod = HttpMethod.Put;
        GenericHttpRequestBody requestBody = randomizer.NextHttpRequestBody();

        HttpStatusCode? statusCode = await clientUnderTest.SendRequestAsync(
            endpointUrl,
            httpMethod,
            CancellationToken.None,
            requestBody);

        Assert.That(statusCode, Is.EqualTo(responseStatusCode));

        Assert.That(httpMessageHandlerMock.SentRequests, Has.Exactly(1).Items);

        RequestSnapshot request = httpMessageHandlerMock.SentRequests.Single();

        await request.AssertJsonRequest(
            expectedUri: endpointUrl,
            expectedHttpMethod: httpMethod,
            expectedRequestBody: requestBody);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Information));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    [Test]
    public void SendingRequestNotPossibleUsingNullReferenceAsEndpointUrl()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity();
        TimeSpan timeout = randomizer.NextTimeSpan(from: StationApiClient.MinTimeout, to: StationApiClient.MaxTimeout);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var loggerStub = new FakeLogger<StationApiClient>();

        var clientUnderTest = new StationApiClient(
            stationEntity,
            httpClientFactoryStub.Object,
            timeout,
            loggerStub);

        var httpMethod = HttpMethod.Get;

        AsyncTestDelegate actionUnderTest = async () => await clientUnderTest.SendRequestAsync(
            null!,
            httpMethod,
            CancellationToken.None);

        Assert.ThrowsAsync<ArgumentNullException>(actionUnderTest);
    }

    [TestCase]
    public void SendingRequestNotPossibleUsingRelativeEndpointUrl()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity();
        TimeSpan timeout = randomizer.NextTimeSpan(from: StationApiClient.MinTimeout, to: StationApiClient.MaxTimeout);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var loggerStub = new FakeLogger<StationApiClient>();

        var clientUnderTest = new StationApiClient(
            stationEntity,
            httpClientFactoryStub.Object,
            timeout,
            loggerStub);

        Uri endpointUrl = randomizer.NextHttpUrl(UriKind.Relative);
        var httpMethod = HttpMethod.Get;

        AsyncTestDelegate actionUnderTest = async () => await clientUnderTest.SendRequestAsync(
            endpointUrl,
            httpMethod,
            CancellationToken.None);

        Assert.ThrowsAsync<ArgumentException>(actionUnderTest);
    }

    [Test]
    public void SendingRequestNotPossibleUsingNullReferenceAsHttpMethod()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity();
        TimeSpan timeout = randomizer.NextTimeSpan(from: StationApiClient.MinTimeout, to: StationApiClient.MaxTimeout);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var loggerStub = new FakeLogger<StationApiClient>();

        var clientUnderTest = new StationApiClient(
            stationEntity,
            httpClientFactoryStub.Object,
            timeout,
            loggerStub);

        Uri endpointUrl = randomizer.NextHttpUrl();

        AsyncTestDelegate actionUnderTest = async () => await clientUnderTest.SendRequestAsync(
            endpointUrl,
            null!,
            CancellationToken.None);

        Assert.ThrowsAsync<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public async Task SendingRequestFailsIfHttpClientThrowsHttpRequestException()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity();

        var httpMessageHandlerMock = new FakeHttpMessageHandler(_ => throw new HttpRequestException());
        var httpClient = new HttpClient(httpMessageHandlerMock);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        httpClientFactoryStub.Setup(factory => factory
            .CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var loggerMock = new FakeLogger<StationApiClient>();

        var clientUnderTest = new StationApiClient(
            stationEntity,
            httpClientFactoryStub.Object,
            StationApiClient.MaxTimeout,
            loggerMock);

        Uri endpointUrl = randomizer.NextHttpUrl();
        var httpMethod = HttpMethod.Get;

        HttpStatusCode? statusCode = await clientUnderTest.SendRequestAsync(
            endpointUrl,
            httpMethod,
            CancellationToken.None);

        Assert.That(statusCode, Is.Null);

        Assert.That(httpMessageHandlerMock.SentRequests, Has.Exactly(1).Items);

        RequestSnapshot request = httpMessageHandlerMock.SentRequests.Single();

        await request.AssertJsonRequest(
            expectedUri: endpointUrl,
            expectedHttpMethod: httpMethod,
            expectedRequestBody: null as object);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Error));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Error < record.Level));
    }

    [Test]
    public async Task SendingRequestFailsIfHttpClientThrowsTimeoutException()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity();

        var httpMessageHandlerMock = new FakeHttpMessageHandler(_ => throw new OperationCanceledException(null, new TimeoutException()));
        var httpClient = new HttpClient(httpMessageHandlerMock);
        
        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        httpClientFactoryStub.Setup(factory => factory
            .CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var loggerMock = new FakeLogger<StationApiClient>();

        var clientUnderTest = new StationApiClient(
            stationEntity,
            httpClientFactoryStub.Object,
            StationApiClient.MaxTimeout,
            loggerMock);

        Uri endpointUrl = randomizer.NextHttpUrl();
        var httpMethod = HttpMethod.Get;

        HttpStatusCode? statusCode = await clientUnderTest.SendRequestAsync(
            endpointUrl,
            httpMethod,
            CancellationToken.None);

        Assert.That(statusCode, Is.Null);

        Assert.That(httpMessageHandlerMock.SentRequests, Has.Exactly(1).Items);

        RequestSnapshot request = httpMessageHandlerMock.SentRequests.Single();

        await request.AssertJsonRequest(
            expectedUri: endpointUrl,
            expectedHttpMethod: httpMethod,
            expectedRequestBody: null as object);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }

    [Test]
    public async Task SendingRequestThrowsExceptionWhenOparationIsCancelled()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity();

        var httpMessageHandlerMock = new FakeHttpMessageHandler(_ => throw new OperationCanceledException());
        var httpClient = new HttpClient(httpMessageHandlerMock);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        httpClientFactoryStub.Setup(factory => factory
            .CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var loggerMock = new FakeLogger<StationApiClient>();

        var clientUnderTest = new StationApiClient(
            stationEntity,
            httpClientFactoryStub.Object,
            StationApiClient.MaxTimeout,
            loggerMock);

        Uri endpointUrl = randomizer.NextHttpUrl();
        var httpMethod = HttpMethod.Get;

        AsyncTestDelegate actionUnderTest = async () => await clientUnderTest.SendRequestAsync(
            endpointUrl,
            httpMethod,
            CancellationToken.None);

        Assert.ThrowsAsync<OperationCanceledException>(actionUnderTest);

        Assert.That(httpMessageHandlerMock.SentRequests, Has.Exactly(1).Items);

        RequestSnapshot request = httpMessageHandlerMock.SentRequests.Single();

        await request.AssertJsonRequest(
            expectedUri: endpointUrl,
            expectedHttpMethod: httpMethod,
            expectedRequestBody: null as object);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }
    #endregion
}
