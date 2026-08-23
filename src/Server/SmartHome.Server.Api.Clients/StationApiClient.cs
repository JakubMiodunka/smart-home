using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using SmartHome.Server.Api.Clients.Abstractions;
using SmartHome.Server.Repositories.Entities;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SmartHome.Server.Api.Clients;

/// <remarks>
/// While currently station entity hold in client body is used for enriched logging,
/// it's main purpose is to allow configuration of the communication 
/// layer based on station-specific properties.
/// Now configuration is static and applied to all stations, 
/// but in the future some station-specific configuration may be required.
/// </remarks>
/// <inheritdoc cref="IStationApiClient"/>
internal sealed class StationApiClient : IStationApiClient
{
    #region Properties
    public static readonly TimeSpan MinTimeout = TimeSpan.FromMicroseconds(1);
    public static readonly TimeSpan MaxTimeout = TimeSpan.FromTicks(int.MaxValue);

    private readonly StationEntity _station;
    private readonly Lazy<HttpClient> _lazyWrappedHttpClient;
    private readonly ILogger<StationApiClient> _logger;

    /// <remarks>
    /// The <see cref="HttpClient"/> is wrapped in <see cref="Lazy{T}"/> primarily to enhance testability. 
    /// By deferring the initialization and configuration of the client, it is possible to instantiate 
    /// the class in a unit testing context without providing a fully configured 
    /// <see cref="IHttpClientFactory"/> during construction.
    /// </remarks>
    private HttpClient WrappedHttpClient => 
        _lazyWrappedHttpClient.Value;
    #endregion

    #region Instantiation
    /// <summary>
    /// Initializes a new instance of the client for a specific station API.
    /// </summary>
    /// <param name="station">
    /// Station, which shall be associated with created client.
    /// </param>
    /// <param name="httpClientFactory">
    /// Factory which shall be used to obtain instances of <see cref="HttpClient"/> class.
    /// Those will be used to communicate with station associated with the client.
    /// </param>
    /// <param name="timeout">
    /// The maximum time to wait for a station API response.
    /// </param>
    /// <param name="logger">
    /// Logger instance, which shall be used by the created client.
    /// </param>
    public StationApiClient(
        StationEntity station,
        IHttpClientFactory httpClientFactory,
        TimeSpan timeout,
        ILogger<StationApiClient> logger)
    {
        ArgumentNullException.ThrowIfNull(station);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentOutOfRangeException.ThrowIfLessThan(timeout, MinTimeout);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(timeout, MaxTimeout);

        _station = station;
        _logger = logger;
        _lazyWrappedHttpClient = new Lazy<HttpClient>(() => CreateHttpClient(httpClientFactory, timeout));
    }
    #endregion

    #region Utilities
    /// <summary>
    /// Creates HTTP client complementary to communicate with the associated station API.
    /// </summary>
    /// <param name="httpClientFactory">
    /// Factory which shall be used to obtain instance of <see cref="HttpClient"/> class.
    /// </param>
    /// <param name="timeout">
    /// The maximum time to wait for a station API response.
    /// It is assumed, that this value is within valid range - no validation will be performed.
    /// </param>
    /// <returns>
    /// HTTP client complementary to communicate with the associated station API.
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
        
        /*
         * TODO: Collect information about which protocol station is using,
         * so that creation of HTTP client can be more flexible and adapted to specific station types.
         */
        HttpClient httpClient = httpClientFactory.CreateClient();
        httpClient.DefaultRequestVersion = HttpVersion.Version10;   // ESP8266's web server only supports only HTTP 1.0.
        httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
        httpClient.Timeout = timeout;

        /*
         * Methods as HttpClient.GetAsJsonAsync automatically adds 'Accept: application/json' header to the request,
         * but since those methods are not used in this class, the header needs to be added manually.
         * For now all communication with station APIs is expected to be in JSON format, so this header is added here by default.
         */
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

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
    private async Task<HttpContent> AsHttpContentAsync(object? request)
    {
        /*
         * Setting request content to instance of JsonContent class will automatically 
         * add 'Content-Type: application/json; charset=utf-8' header to the request.
         */
        var requestContent = JsonContent.Create(request);

        if (WrappedHttpClient.DefaultRequestVersion <= HttpVersion.Version10)
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

    /// <summary>
    /// Attempts to deserialize the HTTP content into an object of specified type.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object to deserialize into.
    /// </typeparam>
    /// <param name="content">
    /// The HTTP content to deserialize.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// Deserialized object of type <typeparamref name="T"/> if deserialization was successful,
    /// <see langword="null"/> otherwise.
    /// </returns>
    private async Task<T?> FromHttpContentAsync<T>(
        HttpContent content,
        CancellationToken cancellationToken) where T : class
    {
        ArgumentNullException.ThrowIfNull(content);

        _logger.LogDebug(
            "Attempting to deserialize HTTP content: StationId=[{StationId}]",
            _station.Id);

        var jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            RespectRequiredConstructorParameters = true
        };

        try
        {
            T? deserializedContent = await content.ReadFromJsonAsync<T>(
                jsonSerializerOptions,
                cancellationToken);

            _logger.Log(
                deserializedContent is null ? LogLevel.Error : LogLevel.Debug,
                $"HTTP content deserialization {(deserializedContent is null ? "failed" : "successful")}: " +
                "StationId=[{StationId}], Content=[{Content}]",
                _station.Id,
                deserializedContent);

            return deserializedContent;
        }
        catch (JsonException exception)
        {
            _logger.LogError(
                exception,
                "HTTP content deserialization failed: StationId=[{StationId}]",
                _station.Id);

            return null;
        }
        catch (OperationCanceledException exception)
        {
            _logger.LogWarning(
                exception,
                "HTTP content deserialization cancelled: StationId=[{StationId}]",
                _station.Id);

            throw;
        }
    }
    #endregion

    #region Interactions
    /// <summary>
    /// Sends an asynchronous HTTP request to the station associated with this client.
    /// </summary>
    /// <remarks>
    /// <see cref="HttpResponseMessage"/> implements <see cref="IDisposable"/> - it is caller's
    /// responsibility to dispose instance returned by the method,
    /// as when disposed it will be impossible to read the response content.
    /// </remarks>
    /// <param name="endpointUrl">
    /// Absolute URL of the station API endpoint.
    /// </param>
    /// <param name="httpMethod">
    /// HTTP method to be used for the request.
    /// </param>
    /// <param name="requestBody">
    /// The object to be serialized into the HTTP request body,
    /// or <see langword="null"/> if no body is required.
    /// </param>
    /// <param name="expectedResponseStatusCode">
    /// The expected status code of the response.
    /// If the actual status code would not match this value, the request will be considered as failed.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// HTTP response returned by the station API if 
    /// the request was processed successfully, <see langword="null"/> otherwise.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable argument is a <see langword="null"/> reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown, when at least one of provided arguments is invalid.
    /// </exception>
    private async Task<HttpResponseMessage?> SendRawRequestAsync(
        Uri endpointUrl,
        HttpMethod httpMethod,
        object? requestBody,
        HttpStatusCode expectedResponseStatusCode,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(endpointUrl);
        ArgumentNullException.ThrowIfNull(httpMethod);

        if (!endpointUrl.IsAbsoluteUri)
        {
            throw new ArgumentException(
                $"Endpoint URL is not absolute: {endpointUrl}",
                nameof(endpointUrl));
        }

        _logger.LogInformation(
            "Attempting to send station API request: StationId=[{StationId}], " +
            "EndpointUrl=[{Url}], HttpMethod=[{HttpMethod}], RequestBody=[{Request}]",
            _station.Id,
            endpointUrl,
            httpMethod,
            requestBody);

        using var request = new HttpRequestMessage(httpMethod, endpointUrl)
        {
            Content = requestBody is null ? null : await AsHttpContentAsync(requestBody)
        };

        try
        {
            /*
             * Methods as HttpClient.PatchAsJsonAsync are avoided here because they uses chunked transfer encoding,
             * which is not supported by certain station types (e.g. stations based on the ESP8266 
             * chip running a server using the ESP8266WebServer library).
             * 
             * HttpResponseMessage type is disposable - it is caller's responsibility to dispose it,
             * as when disposed it will be impossible to read the response content.
             */
            HttpResponseMessage response = await WrappedHttpClient.SendAsync(request, cancellationToken);

            _logger.Log(
                response.IsSuccessStatusCode ? LogLevel.Information : LogLevel.Warning,
                "Station API response received: StationId=[{StationId}], StatusCode=[{StatusCode}]",
                _station.Id,
                response.StatusCode);

            if (response.StatusCode != expectedResponseStatusCode)
            {
                _logger.LogWarning(
                    "Unexpected response status code: StationId=[{StationId}], " +
                    "ExpectedStatusCode=[{ExpectedStatusCode}], ActualStatusCode=[{ActualStatusCode}]",
                    _station.Id,
                    expectedResponseStatusCode,
                    response.StatusCode);

                return null;
            }

            return response;
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Exception thrown while sending station API request: " +
                "StationId=[{StationId}], ExceptionType=[{ExceptionType}], Message=[{Message}]",
                _station.Id,
                exception.GetType().FullName,
                exception.Message);

            return null;
        }
        catch (OperationCanceledException exception) when (exception.InnerException is TimeoutException)
        {
            _logger.LogWarning(
                    exception,
                    "Station failed to respond within the allowed timeframe: StationId=[{StationId}], Timeout=[{Timeout}]",
                    _station.Id,
                    WrappedHttpClient.Timeout);

            return null;
        }
        catch (OperationCanceledException exception)
        {
            _logger.LogWarning(
                exception,
                "Sending station API request cancelled: StationId=[{StationId}]",
                _station.Id);

            throw;
        }
    }

    /// <inheritdoc cref="IStationApiClient"/>
    public async Task<T?> SendRequestAsync<T>(
        Uri endpointUrl,
        HttpMethod httpMethod,
        object? requestBody,
        HttpStatusCode expectedResponseStatusCode,
        CancellationToken cancellationToken) where T : class
    {
        using HttpResponseMessage? response = await SendRawRequestAsync(
            endpointUrl,
            httpMethod,
            requestBody,
            expectedResponseStatusCode,
            cancellationToken);

        return response is null ? null : await FromHttpContentAsync<T>(response.Content, cancellationToken);
    }

    /// <inheritdoc cref="IStationApiClient"/>
    public async Task<bool> SendRequestAsync(
        Uri endpointUrl,
        HttpMethod httpMethod,
        object? requestBody,
        HttpStatusCode expectedResponseStatusCode,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage? response = await SendRawRequestAsync(
            endpointUrl,
            httpMethod,
            requestBody,
            expectedResponseStatusCode,
            cancellationToken);

        return response is not null;
    }
    #endregion
}
