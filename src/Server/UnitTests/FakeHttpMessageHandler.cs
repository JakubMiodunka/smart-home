using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SmartHome.UnitTests;

internal sealed record RequestSnapshot(
    Uri? Url,
    HttpMethod Method,
    HttpRequestHeaders Headers,
    HttpContentHeaders? ContentHeaders,
    string? RawContent);

internal static class RequestSnapshotTestingUtilities
{
    internal static async Task AssertJsonRequest<T>(
        this RequestSnapshot request,
        Uri expectedUri,
        HttpMethod expectedHttpMethod,
        T? expectedRequestBody) where T : class
    {
        Assert.That(request, Is.Not.Null);
        Assert.That(request.Url, Is.Not.Null);
        Assert.That(request.Url, Is.EqualTo(expectedUri));
        Assert.That(request.Method, Is.EqualTo(expectedHttpMethod));

        if (expectedHttpMethod == HttpMethod.Get)
        {
            Assert.That(request.RawContent, Is.Null);

            Assert.That(
                request.Headers.Accept,
                Has.Some.Matches<MediaTypeWithQualityHeaderValue>(headerValue => headerValue.MediaType == "application/json"));

            return;
        }

        Assert.That(request.ContentHeaders?.ContentType?.MediaType, Is.EqualTo("application/json"));
        Assert.That(request.ContentHeaders?.ContentType?.CharSet, Is.EqualTo("utf-8").IgnoreCase);

        Assert.That(request.RawContent, Is.Not.Null);

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        T? requestBody = JsonSerializer.Deserialize<T>(request.RawContent, options);

        Assert.That(requestBody, Is.EqualTo(expectedRequestBody));
    }
}

internal sealed class FakeHttpMessageHandler : DelegatingHandler
{
    #region Properties
    private readonly Func<HttpRequestMessage, HttpResponseMessage>? _requestHandler;
    private readonly Queue<RequestSnapshot> _sentRequests;

    public bool WasAnyRequestSent => 
        0 < _sentRequests.Count();

    public ReadOnlyCollection<RequestSnapshot> SentRequests =>
        _sentRequests.ToList().AsReadOnly();
    #endregion

    #region Instantiation
    internal FakeHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage>? requestHandler = null)
    {
        _requestHandler = requestHandler;
        _sentRequests = new Queue<RequestSnapshot>();
    }
    #endregion

    #region Interactions
    /// <remarks>
    /// This method acts as a mediator in the request data exchange.
    /// Here it allows us to intercept requests sent by the <see cref="HttpClient"/> during test case execution. 
    /// 
    /// We must create a <see cref="RequestSnapshot"/> here because the original <see cref="HttpRequestMessage"/> 
    /// is disposed of automatically further down the pipeline. Capturing a snapshot ensures that 
    /// the request data (especially the Content stream) remains available for assertions even 
    /// after the request cycle is complete, preventing <see cref="ObjectDisposedException"/>.
    /// </remarks>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var snapshot = new RequestSnapshot(
            request.RequestUri,
            request.Method,
            request.Headers,
            request.Content?.Headers,
            request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken));

        _sentRequests.Enqueue(snapshot);

        return _requestHandler is null
            ? throw new NotImplementedException("Request handler unspecified:")
            : _requestHandler.Invoke(request);
    }
    #endregion
}
