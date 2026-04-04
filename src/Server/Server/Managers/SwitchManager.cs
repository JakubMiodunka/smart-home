using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Models.Requests;
using SmartHome.Server.Data.Repositories;
using System.Net;

namespace SmartHome.Server.Managers;

/// <summary>
/// Manages the state of a specific switch on a remote station.
/// </summary>
public interface ISwitchManager
{
    SwitchEntity ManagedSwitch { get; }

    /// <summary>
    /// Attempts to change state of managed electrical switch.
    /// </summary>
    /// <param name="expectedState">
    /// Desired state of electrical switch - <see langword="true"/> if the circuit shall be closed 
    /// and current shall be flowing, <see langword="false"/> otherwise. 
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if operation was successful, <see langword="false"/>otherwise.
    /// </returns>
    Task<bool> TryChangeState(bool expectedState, CancellationToken cancellationToken);
}

/// <inheritdoc cref="ISwitchManager"/>
public sealed class SwitchManager : FeatureManager, ISwitchManager
{
    #region Properties
    // TODO: Move this value to some cinfiguration file.
    private static readonly TimeSpan s_httpClientTimeout = TimeSpan.FromMilliseconds(5000);

    private readonly IStationsRepository _stationsRepository;
    private readonly ISwitchesRepository _switchesRepository;
    private readonly ILogger<SwitchManager> _logger;

    public SwitchEntity ManagedSwitch { get; private set; }
    #endregion

    #region Instantiation
    /// <summary>
    /// Creates new instance of <see cref="SwitchManager"/>.
    /// </summary>
    /// <param name="managedSwitch">
    /// Entity of the switch managed by created manager instance.
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
        SwitchEntity managedSwitch,
        IHttpClientFactory httpClientFactory,
        IStationsRepository stationsRepository,
        ISwitchesRepository switchesRepository,
        ILogger<SwitchManager> logger) : base(httpClientFactory, s_httpClientTimeout, logger)
    {
        ArgumentNullException.ThrowIfNull(managedSwitch);
        ArgumentNullException.ThrowIfNull(stationsRepository);
        ArgumentNullException.ThrowIfNull(switchesRepository);
        ArgumentNullException.ThrowIfNull(logger);

        _stationsRepository = stationsRepository;
        _switchesRepository = switchesRepository;
        _logger = logger;

        ManagedSwitch = managedSwitch;
    }
    #endregion

    #region Interacitons
    /// <inheritdoc cref="ISwitchManager"/>
    /// <summary>
    /// Sends a command to station associated with managed switch to change its state.
    /// </summary>
    public async Task<bool> TryChangeState(bool expectedSwitchState, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Attempting to change state of switch: SwitchId=[{SwitchId}], ExpectedState=[{ExpectedState}]",
            ManagedSwitch.Id, expectedSwitchState);

        if (ManagedSwitch.ActualState == expectedSwitchState)
        {
            _logger.LogInformation(
                "Switch already in expected state: SwitchId=[{SwitchId}], ExpectedState=[{ExpectedState}], ActualState=[{ActualState}]",
                ManagedSwitch.Id,
                ManagedSwitch.ExpectedState,
                ManagedSwitch.ActualState);

            return true;
        }

        if (await _stationsRepository.GetSingleStationAsync(filterById: true, id: ManagedSwitch.StationId) is not StationEntity parentStation)
        {
            _logger.LogError(
                "Parent station entity not found: SwitchId=[{SwitchId}], StationId=[{StationId}]",
                ManagedSwitch.Id,
                ManagedSwitch.StationId);

            return false;
        }

        if (ManagedSwitch.SwitchUrl(parentStation) is not Uri endpointUrl)
        {
            _logger.LogWarning(
                "Switch endpoint URL was considered as unreachable: SwitchId=[{SwitchId}], StationId=[{StationId}]",
                ManagedSwitch.Id,
                ManagedSwitch.StationId);

            return false;
        }

        var request = new SwitchUpdateServerRequest(expectedSwitchState);
        using HttpContent requestContent = await AsHttpContent(request);

        bool wasRequestSuccessful = false;

        try
        {
            /*
             * HttpClient.PatchAsJsonAsync is avoided here because it uses chunked transfer encoding,
             * which is not supported by certain station types (e.g. stations based on the ESP8266 
             * chip running a server using the ESP8266WebServer library).
             */
            _logger.LogDebug("Sending request to station: Url=[{Url}], Request=[{Request}]", endpointUrl, request);
            HttpResponseMessage response = await _httpClient.PatchAsync(endpointUrl, requestContent, cancellationToken);
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
                    _httpClient.Timeout);

                return false;
            }

            _logger.LogInformation(exception, "Stopping request sending due to cancellation request:");
            return false;
        }

        if (wasRequestSuccessful)
        {
            /*
             * Expected and actual switch state shall be updated at the same time to avoid unintentional state changes.
             * If only the expected state would be changed while the station is offline,
             * the switch would trigger an unintended physical change immediately upon the station's return to online status,
             * specifically in the moment, when station is registering the switch.
             */
            SwitchEntity? updatedSwitchEntity = 
                await _switchesRepository.UpdateSwitchAsync(
                    ManagedSwitch.Id,
                    updateExpectedState: true, 
                    expectedState: expectedSwitchState,
                    updateActualState: true,
                    actualState: expectedSwitchState);

            if (updatedSwitchEntity is null)
            {
                _logger.LogError(
                    "Attempting to change state of switch failed: Message=[{Message}], SwitchId=[{SwitchId}]",
                    "Repository update failed.",
                    ManagedSwitch.Id);

                return false;
            }

            ManagedSwitch = ManagedSwitch with { ActualState = expectedSwitchState };

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
