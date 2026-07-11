using Microsoft.Extensions.Logging;
using SmartHome.Server.Api.Clients.Abstractions;
using SmartHome.Server.Features.Managers.Abstractions;
using SmartHome.Server.Features.Managers.Requests;
using SmartHome.Server.Repositories.Entities;
using System.Net;

namespace SmartHome.Server.Features.Managers;

/// <inheritdoc cref="ISwitchManager"/>
internal sealed class SwitchManager : FeatureManager, ISwitchManager
{
    #region Properties
    private readonly ILogger<SwitchManager> _logger;

    // TODO: Move this value to some cinfiguration file.
    protected override TimeSpan HttpClientTimeout => 
        TimeSpan.FromMilliseconds(5000);

    /// <remarks>
    /// This property reflects the most recent state of the switch, 
    /// updated automatically whenever any property is modified by this manager instance.
    /// </remarks>
    public SwitchEntity ManagedSwitch { get; private set; }
    #endregion

    #region Instantiation
    /// <summary>
    /// Creates new instance of <see cref="SwitchManager"/>.
    /// </summary>
    /// <param name="managedSwitch">
    /// Entity of the switch managed by created manager instance.
    /// </param>
    /// <param name="parentStation">
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
        StationEntity parentStation,
        IStationApiClientFactory stationApiClientsFactory,
        ILogger<SwitchManager> logger)
        : base(parentStation, stationApiClientsFactory)
    {
        ArgumentNullException.ThrowIfNull(managedSwitch, nameof(managedSwitch));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        if (managedSwitch.StationId != ParentStation.Id)
        {
            throw new ArgumentException(
                "The station entity shall be the parent station of the switch entity: " +
                $"SwitchStationId=[{managedSwitch.StationId}], StationId=[{parentStation.Id}]",
                nameof(parentStation));
        }

        _logger = logger;
        ManagedSwitch = managedSwitch;
    }
    #endregion

    #region Interacitons
    /// <summary>
    /// Determines the URL of API endpoint which controls the switch.
    /// </summary>
    /// <returns>
    /// Absolute URL of API endpoint which controls the switch,
    /// or <see langword="null"/> if endpoint is considered as unreachable.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when generation of switch URL is not supported switch parent station API version.
    /// </exception>
    private Uri? GetSwitchUrl()
    {
        if (GetStationBaseApiUrl() is not Uri baseStationApiUrl) return null;

        Uri switchApiEndpoint = ParentStation.ApiVersion switch
        {
            1 => new Uri($"switches/{ManagedSwitch.LocalId}", UriKind.Relative),
            _ => throw new NotSupportedException($"Station API version not supported: ApiVersion=[{ParentStation.ApiVersion}]")
        };

        return new Uri(baseStationApiUrl, switchApiEndpoint);
    }

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

        if (GetSwitchUrl() is not Uri endpointUrl)
        {
            _logger.LogWarning(
                "Switch is unreachable: SwitchId=[{SwitchId}], StationId=[{StationId}]",
                ManagedSwitch.Id,
                ParentStation.Id);

            return false;
        }

        var request = new SwitchUpdateRequest(expectedSwitchState);
        IStationApiClient apiClient = StationApiClientsFactory.CreateFor(ParentStation, HttpClientTimeout);
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
