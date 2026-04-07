using System.Net;

namespace SmartHome.Server.Managers;

/// <summary>
/// Base class for feature managers that control specific features of stations operating within the system.
/// </summary>
public abstract class FeatureManager
{
    #region Properties
    private readonly Lazy<HttpClient> _lazyHttpClient;
    private readonly ILogger<FeatureManager> _logger;

    protected HttpClient _httpClient =>
        _lazyHttpClient.Value;
    #endregion

    #region Instantiation
    /// <summary>
    /// Creates new instance of <see cref="FeatureManager"/>.
    /// </summary>
    /// <param name="httpClientFactory">
    /// Factory which shall be used to obtain instances of <see cref="HttpClient"/> class.
    /// Those will be used to communicate with station associated with the managed feature entity.
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable argument is a <see langword="null"/> reference.
    /// </exception>
    protected FeatureManager(
        IHttpClientFactory httpClientFactory,
        TimeSpan httpClientTimeout,
        ILogger<FeatureManager> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _lazyHttpClient = new Lazy<HttpClient>(() => CreateHttpClient(httpClientFactory, httpClientTimeout));
    }
    #endregion

    #region Utilities
    /// <summary>
    /// Creates HTTP client complementary to communicate with station associated with the managed feature entity.
    /// </summary>
    /// <returns>
    /// HTTP client complementary to communicate with station associated with the managed feature entity.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable argument is a <see langword="null"/> reference.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value of at least one argument is outside its valid range.
    /// </exception>
    private HttpClient CreateHttpClient(IHttpClientFactory httpClientFactory, TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero);

        /*
         * TODO: Collect information about which protocol station is using,
         * so that creation of HTTP client can be more flexible and adapted to specific station types.
         */

        HttpClient httpClient = httpClientFactory.CreateClient();
        httpClient.DefaultRequestVersion = HttpVersion.Version10;   // ESP8266's web server only supports only HTTP 1.0.
        httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
        httpClient.Timeout = timeout;

        return httpClient;
    }

    /// <summary>
    /// Serializes the request model into an <see cref="HttpContent"/> instance and pre-processes the data.
    /// </summary>
    /// <remarks>
    /// Returns an abstract <see cref="HttpContent"/> to allow for easier migration between serialization formats.
    /// </remarks>
    /// <param name="request">
    /// The object to be serialized into the HTTP request body.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation, returning the serialized <see cref="HttpContent"/>.
    /// </returns>
    protected async Task<HttpContent> AsHttpContent(object? request)
    {
        var requestContent = JsonContent.Create(request);

        if (_httpClient.DefaultRequestVersion <= HttpVersion.Version10)
        {
            /*
             * Forces full serialization of the request body before transmission.
             * This ensures the 'Content-Length' header is sent instead of 'Transfer-Encoding: chunked',
             * which is not supported by the HTTP below version 1.0.
             * Simple server running on ESP8266 does not support chunked transfer encoding, so this is required to ensure compatibility.
             */
            _logger.LogDebug("Buffering request content to calculate Content-Length and prevent chunked transfer:");
            await requestContent.LoadIntoBufferAsync();
        }

        return requestContent;
    }
    #endregion
}
