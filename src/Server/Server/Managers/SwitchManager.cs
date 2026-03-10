using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Models.Requests;
using SmartHome.Server.Data.Repositories;

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
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable argument is a <see langword="null"/> reference.
    /// </exception>
    public SwitchManager(
        long managedSwitchId,
        IHttpClientFactory httpClientFactory,
        IStationsRepository stationsRepository,
        ISwitchesRepository switchesRepository)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(stationsRepository);
        ArgumentNullException.ThrowIfNull(switchesRepository);

        _httpClientFactory = httpClientFactory;
        _stationsRepository = stationsRepository;
        _switchesRepository = switchesRepository;
        ManagedSwitchId = managedSwitchId;
    }
    #endregion

    #region Interacitons
    /// <summary>
    /// Sends a command to station associated with managed electrical switch to change its state.
    /// </summary>
    /// <param name="expectedState">
    /// Desired state of electrical switch - <see langword="true"/> if the circuit shall be closed 
    /// and current shall be flowing; <see langword="false"/> otherwise. 
    /// </param>
    /// <see langword="true"/> if the command was successfully delivered and acknowledged by the station, 
    /// <see langword="false"/>otherwise.
    /// </returns>
    public async Task<bool> TryChangeState(bool expectedState)
    {
        SwitchEntity? managedSwitch = await _switchesRepository.GetSingleSwitchAsync(filterById: true, id: ManagedSwitchId);

        if (managedSwitch is null)
        {
            throw new InvalidOperationException();
        }

        if (managedSwitch.ActualState == expectedState)
        {
            return true;
        }
        
        StationEntity? parentStation = await _stationsRepository.GetSingleStationAsync(filterById: true, id: managedSwitch.StationId);

        if (parentStation is null)
        {
            throw new InvalidOperationException();
        }

        HttpClient httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(2);

        string url = $"http://{parentStation.IpAddress}:80/api/v1/switches/{managedSwitch.LocalId}";
        var request = new SwitchUpdateStationRequest(expectedState);

        try
        {
            // TODO: Cancellation token can be passed here.
            var response = await httpClient.PutAsJsonAsync(url, request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }
    #endregion
}
