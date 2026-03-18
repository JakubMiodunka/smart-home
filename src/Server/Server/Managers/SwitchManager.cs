using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Models.Requests;
using SmartHome.Server.Data.Repositories;
using System.Collections.ObjectModel;
using System.Net;

namespace SmartHome.Server.Managers;

/// <summary>
/// Manages the state of a specific switch on a remote station.
/// </summary>
public interface ISwitchManager
{
    long ManagedSwitchId
    {
        get;
    }

    /// <summary>
    /// Changes state of managed electrical switch.
    /// </summary>
    /// <param name="expectedState">
    /// Desired state of electrical switch - <see langword="true"/> if the circuit shall be closed 
    /// and current shall be flowing, <see langword="false"/> otherwise. 
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the asynchronous operation.
    /// </param>
    /// <see langword="true"/> if operation was successful, <see langword="false"/>otherwise.
    /// </returns>
    Task<bool> TryChangeState(bool expectedState, CancellationToken cancellationToken);
}

/// <inheritdoc cref="ISwitchManager"/>
public sealed class SwitchManager : ISwitchManager
{
    #region Constants
    // TODO: Maybe store configuration in some configuration file.
    private const double RequestTimeout = 5000; // Given in milliseconds.
    // TODO: Use a static dictionary to map API versions to functions that generate relative endpoint URLs for a specific station.
    private static Func<SwitchEntity, Uri> Endpoint => switchEntity => new Uri($"switches/{switchEntity.LocalId}");
    #endregion

    #region Properties
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IStationsRepository _stationsRepository;
    private readonly ISwitchesRepository _switchesRepository;
    private readonly ILogger<SwitchManager> _logger;

    public long ManagedSwitchId { get; init; }
    #endregion

    #region Instantiation
    /// <summary>
    /// Creates new instance of <see cref="SwitchManager"/>.
    /// </summary>
    /// <param name="managedSwitchId">
    /// Identifier of switch which shall be controlled by the manager.
    /// </param>
    /// <param name="httpClientFactory">
    /// Factory which shall be used to obtain instances of <see cref="HttpClient"/> class.
    /// Those will be used to communicate with station associated with the managed switch.
    /// </param>
    /// <param name="stationsRepository">
    /// Stations repository which shall be used by this manager.
    /// </param>
    /// <param name="switchesRepository">
    /// Switches repository which shall be used by this manager.
    /// </param>
    /// <param name="logger">
    /// Logger which shall be used by this manager.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable argument is a <see langword="null"/> reference.
    /// </exception>
    public SwitchManager(
        long managedSwitchId,
        IHttpClientFactory httpClientFactory,
        IStationsRepository stationsRepository,
        ISwitchesRepository switchesRepository,
        ILogger<SwitchManager> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(stationsRepository);
        ArgumentNullException.ThrowIfNull(switchesRepository);
        ArgumentNullException.ThrowIfNull(logger);

        _httpClientFactory = httpClientFactory;
        _stationsRepository = stationsRepository;
        _switchesRepository = switchesRepository;
        _logger = logger;

        ManagedSwitchId = managedSwitchId;
    }
    #endregion

    #region Interacitons
    /// <summary>
    /// Generates endpoint URL specific for controlling managed switch.
    /// </summary>
    /// <param name="managedSwitch">
    /// Managed switch entity.
    /// </param>
    /// <param name="parentStation">
    /// Entity of parent station associated with the managed switch.
    /// </param>
    /// <returns>
    /// Endpoint URL specific for controlling managed switch.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one required reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if base API URL of the parent station cannot be determined.
    /// </exception>
    private Uri GetEndpointUrl(SwitchEntity managedSwitch, StationEntity parentStation)
    {
        ArgumentNullException.ThrowIfNull(managedSwitch);
        ArgumentNullException.ThrowIfNull(parentStation);

        /*
         * TODO:
         * Collect information about which protocol station is using,
         * port number and its API version to be more flexible during endpoint URL constriction.
         */

        if (parentStation.BaseApiUrl() is not Uri baseStationApiUrl)
        {
            throw new InvalidOperationException();
        }

        Uri endpointRelativeUrl = Endpoint(managedSwitch);
        var endpointAbsoluteUrl = new Uri(baseStationApiUrl, endpointRelativeUrl);

        return endpointAbsoluteUrl;
    }

    /// <summary>
    /// Creates HTTP client complementary to communicate with station associated with the managed switch.
    /// </summary>
    /// <returns>
    /// HTTP client complementary to communicate with station associated with the managed switch.
    /// </returns>
    private HttpClient CreateHttpClient()
    {
        /*
         * TODO:
         * Collect information about which protocol station is using,
         * so that creation of HTTP client can be more flexible and adapted to specific station types.
         */

        HttpClient httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestVersion = HttpVersion.Version10;   // ESP8266's simple web server only supports only HTTP 1.0.
        httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
        httpClient.Timeout = TimeSpan.FromMilliseconds(RequestTimeout);

        return httpClient;
    }

    /// <inheritdoc cref="ISwitchManager.TryChangeState(bool, CancellationToken)"/>
    /// <summary>
    /// Sends a command to station associated with managed switch to change its state.
    /// </summary>
    public async Task<bool> TryChangeState(bool expectedSwitchState, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Attempting to change state of switch: SwitchId=[{SwitchId}], ExpectedState=[{ExpectedState}]",
            ManagedSwitchId, expectedSwitchState);

        if (await _switchesRepository.GetSingleSwitchAsync(filterById: true, id: ManagedSwitchId) is not SwitchEntity managedSwitch)
        {
            _logger.LogWarning("Switch entity not found: SwitchId=[{SwitchId}]", ManagedSwitchId);
            return false;
        }

        if (managedSwitch.ActualState == expectedSwitchState)
        {
            _logger.LogInformation(
                "Switch already in expected state: SwitchId=[{SwitchId}], ExpectedState=[{ExpectedState}], ActualState=[{ActualState}]",
                managedSwitch.Id,
                managedSwitch.ExpectedState,
                managedSwitch.ActualState);

            return true;
        }

        await _switchesRepository.UpdateSwitchAsync(managedSwitch.Id, updateExpectedState: true, expectedState: expectedSwitchState);

        if (await _stationsRepository.GetSingleStationAsync(filterById: true, id: managedSwitch.StationId) is not StationEntity parentStation)
        {
            _logger.LogError(
                "Parent station entity not found: SwitchId=[{SwitchId}], StationId=[{StationId}]",
                managedSwitch.Id,
                managedSwitch.StationId);

            return false;
        }

        HttpClient httpClient = CreateHttpClient();
        Uri url = GetEndpointUrl(managedSwitch, parentStation);

        var request = new SwitchUpdateServerRequest(expectedSwitchState);
        using var requestJsonContent = JsonContent.Create(request);

        if (httpClient.DefaultRequestVersion <= HttpVersion.Version10)
        {
            /*
             * Forces full serialization of the request body before transmission.
             * This ensures the 'Content-Length' header is sent instead of 'Transfer-Encoding: chunked',
             * which is not supported by the HTTP below version 1.0.
             * Simple server running on ESP8266 does not support chunked transfer encoding, so this is required to ensure compatibility.
             */
            _logger.LogDebug("Buffering request content to calculate Content-Length and prevent chunked transfer:");
            await requestJsonContent.LoadIntoBufferAsync();
        }
        
        bool wasRequestSuccessful = false;

        try
        {
            /*
             * HttpClient.PatchAsJsonAsync is avoided here because it uses chunked transfer encoding,
             * which is not supported by certain station types (e.g. stations based on the ESP8266 
             * chip running a server using the ESP8266WebServer library).
             */
            _logger.LogDebug("Sending request to station: Url=[{Url}], Request=[{Request}]", url, request);
            HttpResponseMessage response = await httpClient.PatchAsync(url, requestJsonContent, cancellationToken);
            wasRequestSuccessful = response.StatusCode == HttpStatusCode.NoContent;

            if (wasRequestSuccessful)
            {
                _logger.LogDebug("Response received: StatusCode=[{StatusCode}]", response.StatusCode);
            }
            else
            {
                _logger.LogWarning(
                    "Response received: StatusCode=[{StatusCode}], Body=[{Body}]",
                    response.StatusCode,
                    await response.Content.ReadAsStringAsync(cancellationToken));
            }
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(
                exception,
                "Exception thrown during request sending: ExceptionType=[{ExceptionType}], StatusCode=[{StatusCode}]",
                exception.GetType().FullName,
                exception.StatusCode);

            return false;
        }
        catch (OperationCanceledException exception)
        {
            if (exception.InnerException is TimeoutException)
            {
                _logger.LogWarning(
                    exception,
                    "Station failed to respond within the allowed timeframe: Timeout=[{Timeout}]",
                    httpClient.Timeout);

                return false;
            }

            _logger.LogInformation(exception, "Stopping request sending due to cancellation request:");
            return false;
        }

        if (wasRequestSuccessful)
        {
            await _switchesRepository.UpdateSwitchAsync(
                managedSwitch.Id,
                updateActualState: true,
                actualState: expectedSwitchState);

            _logger.LogInformation("Attempting to change state of switch successful:");
        }
        else
        {
            _logger.LogWarning("Attempting to change state of switch failed:");
        }

        return wasRequestSuccessful;
    }
    #endregion
}
