using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Api.Clients;
using SmartHome.Server.Repositories.Entities;
using SmartHome.Server.Tests.Utilities;
using System.Net;
using System.Net.Http.Json;

namespace SmartHome.Server.Tests.Api.Clients;

[Category("UnitTest")]
[TestOf(typeof(StationApiClient))]
[Author("Jakub Miodunka")]
internal sealed class StationApiClientTests
{
    #region Constructor
    [Test]
    public void InstantiationPossible()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;
        
        StationEntity stationEntity = randomizer.NextOnlineStationEntity();
        TimeSpan timeout = randomizer.NextTimeSpan(from: StationApiClient.MinTimeout, to: StationApiClient.MaxTimeout);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var loggerStub = new FakeLogger<StationApiClient>();

        Action actionUnderTest = () => new StationApiClient(
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

        StationEntity stationEntity = randomizer.NextOnlineStationEntity();
        TimeSpan timeout = randomizer.NextTimeSpan(from: StationApiClient.MinTimeout, to: StationApiClient.MaxTimeout);

        var loggerStub = new FakeLogger<StationApiClient>();

        Action actionUnderTest = () => new StationApiClient(
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

        Action actionUnderTest = () => new StationApiClient(
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

        StationEntity stationEntity = randomizer.NextOnlineStationEntity();
        TimeSpan timeout = randomizer.NextTimeSpan(from: StationApiClient.MinTimeout, to: StationApiClient.MaxTimeout);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();

        Action actionUnderTest = () => new StationApiClient(
            stationEntity,
            httpClientFactoryStub.Object,
            timeout,
            null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [TestCase(10)]  // Equal to 1 microsecond.
    [TestCase(int.MaxValue)]
    public void InstantiationPossibleUsingValidTimeout(long timeoutTicks)
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextOnlineStationEntity();
        var timeout = TimeSpan.FromTicks(timeoutTicks);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var loggerStub = new FakeLogger<StationApiClient>();

        Action actionUnderTest = () => new StationApiClient(
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

        StationEntity stationEntity = randomizer.NextOnlineStationEntity();
        var timeout = TimeSpan.FromTicks(timeoutTicks);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var loggerStub = new FakeLogger<StationApiClient>();

        Action actionUnderTest = () => new StationApiClient(
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

        StationEntity stationEntity = randomizer.NextOnlineStationEntity();

        HttpStatusCode expectedResponseStatusCode = randomizer.NextSuccessfulHttpStatusCode();
        GenericHttpRequestBody expectedResponseBody = randomizer.NextHttpRequestBody();
        using var expectedResponse = new HttpResponseMessage(expectedResponseStatusCode)
        {
            Content = JsonContent.Create(expectedResponseBody)
        };

        var httpMessageHandlerMock = new FakeHttpMessageHandler(_ => expectedResponse);
        using var httpClient = new HttpClient(httpMessageHandlerMock);

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
        HttpMethod httpMethod = randomizer.NextHttpMethod();
        GenericHttpRequestBody requestBody = randomizer.NextHttpRequestBody();

        StationApiResponse<GenericHttpRequestBody>? actualResponse = 
            await clientUnderTest.SendRequestAsync<GenericHttpRequestBody>(
                endpointUrl,
                httpMethod,
                requestBody,
                CancellationToken.None);

        Assert.That(actualResponse, Is.Not.Null);
        Assert.That(expectedResponse.StatusCode, Is.EqualTo(actualResponse.StatusCode));
        Assert.That(expectedResponseBody, Is.EqualTo(actualResponse.Body));

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

        StationEntity stationEntity = randomizer.NextOnlineStationEntity();

        HttpStatusCode responseStatusCode = randomizer.NextSuccessfulHttpStatusCode();
        var httpMessageHandlerMock = new FakeHttpMessageHandler(_ => new HttpResponseMessage(responseStatusCode));
        using var httpClient = new HttpClient(httpMessageHandlerMock);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        httpClientFactoryStub.Setup(factory => factory
            .CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var loggerStub = new FakeLogger<StationApiClient>();

        var clientUnderTest = new StationApiClient(
            stationEntity,
            httpClientFactoryStub.Object,
            StationApiClient.MaxTimeout,
            loggerStub);

        HttpMethod httpMethod = randomizer.NextHttpMethod();

        Func<Task> actionUnderTest = async () =>
        {
            StationApiResponse? response = await clientUnderTest.SendRequestAsync(
                null!,
                httpMethod,
                null,
                CancellationToken.None);
        };

        Assert.ThrowsAsync<ArgumentNullException>(actionUnderTest);
        Assert.That(httpMessageHandlerMock.SentRequests, Is.Empty);
    }

    [TestCase]
    public void SendingRequestNotPossibleUsingRelativeEndpointUrl()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextOnlineStationEntity();

        HttpStatusCode responseStatusCode = randomizer.NextSuccessfulHttpStatusCode();
        var httpMessageHandlerMock = new FakeHttpMessageHandler(_ => new HttpResponseMessage(responseStatusCode));
        using var httpClient = new HttpClient(httpMessageHandlerMock);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        httpClientFactoryStub.Setup(factory => factory
            .CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var loggerStub = new FakeLogger<StationApiClient>();

        var clientUnderTest = new StationApiClient(
            stationEntity,
            httpClientFactoryStub.Object,
            StationApiClient.MaxTimeout,
            loggerStub);

        Uri endpointUrl = randomizer.NextHttpUrl(UriKind.Relative);
        HttpMethod httpMethod = randomizer.NextHttpMethod();

        Func<Task> actionUnderTest = async () =>
        {
            StationApiResponse? response = await clientUnderTest.SendRequestAsync(
                endpointUrl,
                httpMethod,
                null,
                CancellationToken.None);
        };

        Assert.ThrowsAsync<ArgumentException>(actionUnderTest);
        Assert.That(httpMessageHandlerMock.SentRequests, Is.Empty);
    }

    [Test]
    public void SendingRequestNotPossibleUsingNullReferenceAsHttpMethod()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextOnlineStationEntity();

        HttpStatusCode responseStatusCode = randomizer.NextSuccessfulHttpStatusCode();
        var httpMessageHandlerMock = new FakeHttpMessageHandler(_ => new HttpResponseMessage(responseStatusCode));
        using var httpClient = new HttpClient(httpMessageHandlerMock);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        httpClientFactoryStub.Setup(factory => factory
            .CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var loggerStub = new FakeLogger<StationApiClient>();

        var clientUnderTest = new StationApiClient(
            stationEntity,
            httpClientFactoryStub.Object,
            StationApiClient.MaxTimeout,
            loggerStub);

        Uri endpointUrl = randomizer.NextHttpUrl();

        Func<Task> actionUnderTest = async () =>
        {
            StationApiResponse? response = await clientUnderTest.SendRequestAsync(
                endpointUrl,
                null!,
                null,
                CancellationToken.None);
        };

        Assert.ThrowsAsync<ArgumentNullException>(actionUnderTest);
        Assert.That(httpMessageHandlerMock.SentRequests, Is.Empty);
    }

    [Test]
    public async Task SendingRequestFailsIfHttpClientThrowsHttpRequestException()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextOnlineStationEntity();

        var httpMessageHandlerMock = new FakeHttpMessageHandler(_ => throw new HttpRequestException());
        using var httpClient = new HttpClient(httpMessageHandlerMock);

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
        HttpMethod httpMethod = randomizer.NextHttpMethod();

        StationApiResponse? response = await clientUnderTest.SendRequestAsync(
            endpointUrl,
            httpMethod,
            null,
            CancellationToken.None);

        Assert.That(response, Is.Null);

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

        StationEntity stationEntity = randomizer.NextOnlineStationEntity();

        var httpMessageHandlerMock = new FakeHttpMessageHandler(_ => 
            throw new OperationCanceledException(null, new TimeoutException()));
        using var httpClient = new HttpClient(httpMessageHandlerMock);
        
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
        HttpMethod httpMethod = randomizer.NextHttpMethod();

        StationApiResponse? response = await clientUnderTest.SendRequestAsync(
            endpointUrl,
            httpMethod,
            null,
            CancellationToken.None);

        Assert.That(response, Is.Null);

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

    private static IEnumerable<HttpContent?> GetInvalidResponseContents()
    {
        yield return null;
        yield return JsonContent.Create(new { Message = "This is invalid response body." });
    }

    [Test]
    public async Task SendingRequestFailsIfResponseHaveInvalidContent(
        [ValueSource(nameof(GetInvalidResponseContents))] HttpContent? invalidResponseContent)
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextOnlineStationEntity();

        HttpStatusCode expectedResponseStatusCode = randomizer.NextSuccessfulHttpStatusCode();
        using var rawResponse = new HttpResponseMessage(expectedResponseStatusCode)
        {
            Content = invalidResponseContent
        };

        var httpMessageHandlerMock = new FakeHttpMessageHandler(_ => rawResponse);
        using var httpClient = new HttpClient(httpMessageHandlerMock);

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
        HttpMethod httpMethod = randomizer.NextHttpMethod();
        GenericHttpRequestBody requestBody = randomizer.NextHttpRequestBody();

        StationApiResponse<GenericHttpRequestBody>? actualResponse =
            await clientUnderTest.SendRequestAsync<GenericHttpRequestBody>(
                endpointUrl,
                httpMethod,
                requestBody,
                CancellationToken.None);

        Assert.That(actualResponse, Is.Null);

        Assert.That(httpMessageHandlerMock.SentRequests, Has.Exactly(1).Items);

        RequestSnapshot request = httpMessageHandlerMock.SentRequests.Single();

        await request.AssertJsonRequest(
            expectedUri: endpointUrl,
            expectedHttpMethod: httpMethod,
            expectedRequestBody: requestBody);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Error));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Error < record.Level));
    }

    [Test]
    public async Task SendingRequestFailsIfResponseHaveEmptyContent()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextOnlineStationEntity();

        HttpStatusCode expectedResponseStatusCode = randomizer.NextSuccessfulHttpStatusCode();
        /*
         * By default content of newly created message is set to instance of EmptyContent type,
         * which is private nested class defined in System.Net.Http.HttpResponseMessage.
         * It indicates that the response body is empty.
         * Only way to force the response to have empty body is to not set it,
         * which is the reason why sepparate test case needs to be created for this scenario.
         */
        using var rawResponse = new HttpResponseMessage(expectedResponseStatusCode);

        var httpMessageHandlerMock = new FakeHttpMessageHandler(_ => rawResponse);
        using var httpClient = new HttpClient(httpMessageHandlerMock);

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
        HttpMethod httpMethod = randomizer.NextHttpMethod();
        GenericHttpRequestBody requestBody = randomizer.NextHttpRequestBody();

        StationApiResponse<GenericHttpRequestBody>? actualResponse =
            await clientUnderTest.SendRequestAsync<GenericHttpRequestBody>(
                endpointUrl,
                httpMethod,
                requestBody,
                CancellationToken.None);

        Assert.That(actualResponse, Is.Null);

        Assert.That(httpMessageHandlerMock.SentRequests, Has.Exactly(1).Items);

        RequestSnapshot request = httpMessageHandlerMock.SentRequests.Single();

        await request.AssertJsonRequest(
            expectedUri: endpointUrl,
            expectedHttpMethod: httpMethod,
            expectedRequestBody: requestBody);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Error));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Error < record.Level));
    }

    [Test]
    public async Task SendingRequestThrowsExceptionWhenOperationIsCancelled()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextOnlineStationEntity();

        var httpMessageHandlerMock = new FakeHttpMessageHandler(_ => throw new OperationCanceledException());
        using var httpClient = new HttpClient(httpMessageHandlerMock);

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
        HttpMethod httpMethod = randomizer.NextHttpMethod();

        Func<Task> actionUnderTest = async () =>
        {
            StationApiResponse? response = await clientUnderTest.SendRequestAsync(
                endpointUrl,
                httpMethod,
                null,
                CancellationToken.None);
        };

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
