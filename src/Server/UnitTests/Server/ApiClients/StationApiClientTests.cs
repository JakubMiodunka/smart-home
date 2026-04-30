using Microsoft.Extensions.Logging.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.ApiClients.StationApi;
using SmartHome.Server.Data.Models.Entities;
using System.Net;

namespace SmartHome.UnitTests.Server.ApiClients;

[Category("UnitTest")]
[TestOf(typeof(StationApiClient))]
[Author("Jakub Miodunka")]
public sealed class StationApiClientTests
{
    #region Test Utilities
    private static HttpMethod[] httpMethods = { HttpMethod.Get, HttpMethod.Put, HttpMethod.Post, HttpMethod.Patch };
    private HttpMethod NextHttpMethod(Random randomizer) =>
        httpMethods[randomizer.Next(httpMethods.Count())];
    #endregion

    #region Constructor
    [Test]
    public void InstantiationPossible()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;
        
        StationEntity stationEntity = randomizer.NextStationEntity();
        TimeSpan timeout = TimeSpan.FromMicroseconds(1);

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
        TimeSpan timeout = TimeSpan.FromMicroseconds(1);

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

        TimeSpan timeout = TimeSpan.FromMicroseconds(1);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var loggerStub = new FakeLogger<StationApiClient>();

        TestDelegate actionUnderTest = () => new StationApiClient(
            null!,
            httpClientFactoryStub.Object,
            timeout,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void InstantiationImpossibleUsingInvalidTimeout(long invalidTimeout)    // Given in microseconds.
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity();
        var timeout = TimeSpan.FromMicroseconds(invalidTimeout);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var loggerStub = new FakeLogger<StationApiClient>();

        TestDelegate actionUnderTest = () => new StationApiClient(
            stationEntity,
            httpClientFactoryStub.Object,
            timeout,
            loggerStub);

        Assert.Throws<ArgumentOutOfRangeException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsLogger()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity();
        TimeSpan timeout = TimeSpan.FromMicroseconds(1);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();

        TestDelegate actionUnderTest = () => new StationApiClient(
            stationEntity,
            httpClientFactoryStub.Object,
            timeout,
            null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }
    #endregion

    #region Request sending
    // TODO: Refine this section.
    [Test]
    public void SendingRequestNotPossibleUsingNullReferenceAsEndpointUrl()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity();
        TimeSpan timeout = TimeSpan.FromMicroseconds(1);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();

        var clientUnderTest = new StationApiClient(
            stationEntity,
            httpClientFactoryStub.Object,
            timeout,
            null!);

        AsyncTestDelegate actionUnderTest = async () => await clientUnderTest.SendRequestAsync(
            null!,
            NextHttpMethod(randomizer),
            CancellationToken.None);

        Assert.ThrowsAsync<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void SendingRequestNotPossibleUsingRelativeEndpointUrl()
    {
        // TODO:
    }

    [Test]
    public void SendingRequestNotPossibleUsingNullReferenceAsHttpMethod()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        StationEntity stationEntity = randomizer.NextStationEntity() with
        {
            IpAddress = randomizer.NextIpAddress(),
            ApiPort = randomizer.NextPort(),
            ApiVersion = randomizer.NextByte(1, byte.MaxValue)
        };

        TimeSpan timeout = TimeSpan.FromMicroseconds(1);

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();

        var clientUnderTest = new StationApiClient(
            stationEntity,
            httpClientFactoryStub.Object,
            timeout,
            null!);

        AsyncTestDelegate actionUnderTest = async () => await clientUnderTest.SendRequestAsync(
            stationEntity.BaseApiUrl()!,
            null!,
            CancellationToken.None);

        Assert.ThrowsAsync<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public async Task SendingRequestFailsIfHttpClientThrowsHttpRequestException()
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
            IpAddress = randomizer.NextIpAddress(),
            ApiPort = randomizer.NextPort(),
            ApiVersion = randomizer.NextByte(1, byte.MaxValue)
        };

        TimeSpan timeout = TimeSpan.FromMicroseconds(1);

        var clientUnderTest = new StationApiClient(
            stationEntity,
            httpClientFactoryStub.Object,
            timeout,
            null!);

        HttpStatusCode? statusCode = await clientUnderTest.SendRequestAsync(
            stationEntity.BaseApiUrl()!,
            NextHttpMethod(randomizer),
            CancellationToken.None);

        Assert.That(statusCode, Is.Null);
    }

    [Test]
    public async Task SendingRequestFailsIfHttpClientThrowsTimeoutException()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var httpMessageHandlerMock = new FakeHttpMessageHandler(_ => throw new TimeoutException());
        var httpClient = new HttpClient(httpMessageHandlerMock);
        var httpClientFactoryStub = new Mock<IHttpClientFactory>();

        httpClientFactoryStub.Setup(factory => factory
            .CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        StationEntity stationEntity = randomizer.NextStationEntity() with
        {
            IpAddress = randomizer.NextIpAddress(),
            ApiPort = randomizer.NextPort(),
            ApiVersion = randomizer.NextByte(1, byte.MaxValue)
        };

        TimeSpan timeout = TimeSpan.FromMicroseconds(1);

        var clientUnderTest = new StationApiClient(
            stationEntity,
            httpClientFactoryStub.Object,
            timeout,
            null!);

        HttpStatusCode? statusCode = await clientUnderTest.SendRequestAsync(
            stationEntity.BaseApiUrl()!,
            NextHttpMethod(randomizer),
            CancellationToken.None);

        Assert.That(statusCode, Is.Null);
    }
    #endregion
}
