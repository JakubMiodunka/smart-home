using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Models.Requests;
using SmartHome.Server.Data.Repositories;
using System.Net;

namespace SmartHome.Server.Managers;

/// <summary>
/// Manager, which is able to control an electrical switch.
/// </summary>
/// TODO: Adjust doc-string.
public interface ISwitchManager
{
    /// <summary>
    /// Changes state of managed electrical switch.
    /// </summary>
    /// <param name="expectedState">
    /// Desired state of electrical switch - <see langword="true"/> if the circuit shall be closed 
    /// and current shall be flowing; <see langword="false"/> otherwise. 
    /// </param>
    /// <see langword="true"/> if operation was successful, <see langword="false"/>otherwise.
    /// </returns>
    public Task<bool> TryChangeState(bool expectedState);
}

/// <summary>
/// Provides logic to communicate with and control an electrical switch via its associated station.
/// </summary>
/// TODO: Add logging.
/// TODO: Adjust doc-string.
public sealed class SwitchManager : ISwitchManager
{
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
    /// Entity of switch which shall be controlled by the manager.
    /// </param>
    /// <param name="httpClientFactory">
    /// Factory used to provide <see cref="HttpClient"/> instances
    /// for HTTP-based communication with station specified in <paramref name="managedSwitch"/>.
    /// </param>
    /// <param name="stationsRepository">
    /// Stations repository which shall be used by this manager.
    /// </param>
    /// <param name="switchesRepository">
    /// 
    /// </param>
    /// <param name="logger">
    /// 
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
    /// Sends a command to station associated with managed electrical switch to change its state.
    /// </summary>
    /// <param name="expectedSwitchState">
    /// Desired state of electrical switch - <see langword="true"/> if the circuit shall be closed 
    /// and current shall be flowing; <see langword="false"/> otherwise. 
    /// </param>
    /// <see langword="true"/> if the command was successfully delivered and acknowledged by the station, 
    /// <see langword="false"/>otherwise.
    /// </returns>
    public async Task<bool> TryChangeState(bool expectedSwitchState)
    {
        _logger.LogInformation("Attempting to change state of switch: SwitchId=[{SwitchId}], ExpectedState=[{ExpectedState}]", ManagedSwitchId, expectedSwitchState);

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
            _logger.LogError("Parent station entity not found: SwitchId=[{SwitchId}], StationId=[{StationId}]", managedSwitch.Id, managedSwitch.StationId);
            return false;
        }

        // TODO: Create predefined ESP8266 client.
        HttpClient httpClient = _httpClientFactory.CreateClient();
        // ESP8266's simple web server only supports only HTTP 1.0.
        httpClient.DefaultRequestVersion = HttpVersion.Version10;
        httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
        httpClient.Timeout = TimeSpan.FromSeconds(1);   // TODO: Maybe store it some configuration variable/file.

        // TODO: Collect information about which protocol station is using and its API version to be more flexible during endpoint URL construinction.
        string url = $"http://{parentStation.IpAddress}:80/api/v1/switches/{managedSwitch.LocalId}";
        
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
            // TODO: Pass cancellation tokent to HttpClient.PatchAsync.
            _logger.LogDebug("Sending request to station: Url=[{Url}], Request=[{Request}]", url, request);
            HttpResponseMessage response = await httpClient.PatchAsync(url, requestJsonContent);
            wasRequestSuccessful = response.StatusCode == HttpStatusCode.NoContent;

            if (wasRequestSuccessful)
            {
                _logger.LogDebug("Response received: StatusCode=[{StatusCode}]", response.StatusCode);
            }
            else
            {
                // TODO: Pass cancellation token to HttpResponseMessage.Content.ReadAsStringAsync.
                _logger.LogWarning(
                    "Response received: StatusCode=[{StatusCode}], Body=[{Body}]",
                    response.StatusCode,
                    await response.Content.ReadAsStringAsync());
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
