using Microsoft.Extensions.Logging;
using SmartHome.Server.Api.Clients.Abstractions;
using SmartHome.Server.Feature.Managers.Abstractions;
using SmartHome.Server.Feature.Managers.Requests;
using SmartHome.Server.Repositories.Entities;
using System.Net;

namespace SmartHome.Server.Feature.Managers;

/// <inheritdoc cref="ISwitchManager"/>
public sealed class SwitchManager : ISwitchManager
{
    #region Properties
    // TODO: Move this value to some cinfiguration file.
    private static readonly TimeSpan s_httpClientTimeout = TimeSpan.FromMilliseconds(5000);

    private readonly IStationApiClientFactory _stationApiClientsFactory;
    private readonly ILogger<SwitchManager> _logger;

    /// <remarks>
    /// This property reflects the most recent state of the switch, 
    /// updated automatically whenever any property is modified by this manager instance.
    /// </remarks>
    public SwitchEntity ManagedSwitch { get; private set; }
    public StationEntity SwitchParentStation { get; init; }
    #endregion

    #region Instantiation
    /// <summary>
    /// Creates new instance of <see cref="SwitchManager"/>.
    /// </summary>
    /// <param name="managedSwitch">
    /// Entity of the switch managed by created manager instance.
    /// </param>
    /// <param name="switchParentStation">
    /// Parent station of the managed switch.
    /// </param>
    /// <param name="stationApiClientsFactory">
    /// Factory of station API clients, which shall be used to obtain clients
    /// capable of communicating with station associated with the managed switch.
    /// </param>
    /// <param name="logger">
    /// Logger which shall be used by this manager.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable argument is a <see langword="null"/> reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown, when at least one of provided arguments is invalid.
    /// </exception>
    public SwitchManager(
        SwitchEntity managedSwitch,
        StationEntity switchParentStation,
        IStationApiClientFactory stationApiClientsFactory,
        ILogger<SwitchManager> logger)
    {
        ArgumentNullException.ThrowIfNull(managedSwitch, nameof(managedSwitch));
        ArgumentNullException.ThrowIfNull(switchParentStation, nameof(switchParentStation));
        ArgumentNullException.ThrowIfNull(stationApiClientsFactory, nameof(stationApiClientsFactory));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        if (managedSwitch.StationId != switchParentStation.Id)
        {
            throw new ArgumentException(
                "The station entity shall be the parent station of the switch entity: " +
                $"SwitchStationId=[{managedSwitch.StationId}], StationId=[{switchParentStation.Id}]",
                nameof(switchParentStation));
        }

        ManagedSwitch = managedSwitch;
        SwitchParentStation = switchParentStation;
        _stationApiClientsFactory = stationApiClientsFactory;
        _logger = logger;
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

        if (ManagedSwitch.SwitchUrl(SwitchParentStation) is not Uri endpointUrl)
        {
            _logger.LogWarning(
                "Switch is unreachable: SwitchId=[{SwitchId}], StationId=[{StationId}]",
                ManagedSwitch.Id,
                SwitchParentStation.Id);

            return false;
        }

        if (ManagedSwitch.ActualState == expectedSwitchState)
        {
            _logger.LogInformation(
                "Switch already in expected state: SwitchId=[{SwitchId}], ExpectedState=[{ExpectedState}], ActualState=[{ActualState}]",
                ManagedSwitch.Id,
                ManagedSwitch.ExpectedState,
                ManagedSwitch.ActualState);

            return true;
        }

        var request = new SwitchUpdateRequest(expectedSwitchState);
        IStationApiClient apiClient = _stationApiClientsFactory.CreateFor(SwitchParentStation, s_httpClientTimeout);
        HttpStatusCode? responseStatusCode = await apiClient.SendRequestAsync(endpointUrl, HttpMethod.Patch, request, cancellationToken);

        if (responseStatusCode is HttpStatusCode.NoContent)
        {
            _logger.LogInformation("Attempting to change state of switch successful: SwitchId=[{SwitchId}], " +
                "ExpectedState=[{ExpectedState}], ActualState=[{ActualState}]",
                ManagedSwitch.Id,
                ManagedSwitch.ExpectedState,
                ManagedSwitch.ActualState);

            ManagedSwitch = ManagedSwitch with { ExpectedState = expectedSwitchState, ActualState = expectedSwitchState };
            return true;
        }

        _logger.LogWarning(
                "Attempting to change state of switch failed: Message=[{Message}], StatusCode=[{StatusCode}]",
                "Unexpected station response received.",
                responseStatusCode);

        return false;
    }
    #endregion
}
