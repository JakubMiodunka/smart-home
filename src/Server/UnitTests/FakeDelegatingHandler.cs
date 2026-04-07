namespace UnitTests;

// TODO: rework
public class FakeDelegatingHandler : DelegatingHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage>? _requestHandler;

    public bool WasDataSent { get; private set; }

    public FakeDelegatingHandler(
        Func<HttpRequestMessage, HttpResponseMessage>? requestHandler = null)
    {
        ArgumentNullException.ThrowIfNull(requestHandler);

        _requestHandler = requestHandler;
        WasDataSent = false;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken _)
    {
        WasDataSent = true;

        return _requestHandler is null
            ? Task.FromException<HttpResponseMessage>(new NotImplementedException("Request handler unspecified:"))
            : Task.FromResult(_requestHandler.Invoke(request));
    }
}
