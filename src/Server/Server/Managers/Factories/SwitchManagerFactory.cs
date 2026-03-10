using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Repositories;

namespace SmartHome.Server.Managers.Factories;

/// <summary>
/// Factory for creating managers that control electrical switches.
/// </summary>
public interface ISwitchManagerFactory
{
    /// <summary>
    /// Creates manager for the electrical switch.
    /// </summary>
    /// <param name="switchEntity">
    /// Entity of switch which shall be controlled by created manager.
    /// </param>
    /// <returns>
    /// An <see cref="SwitchManager"/> instance that allows performing operations on the switch.
    /// </returns>
    ISwitchManager CreateFor(SwitchEntity switchEntity);
}

/// <inheritdoc cref="ISwitchManagerFactory"/>
public sealed class SwitchManagerFactory : ISwitchManagerFactory
{
    #region Properties
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IStationsRepository _stationsRepository;
    private readonly ISwitchesRepository _switchesRepository;
    #endregion

    #region Instationation
    public SwitchManagerFactory(
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
    }
    #endregion

    #region Interactions
    /// <inheritdoc cref="ISwitchManagerFactory"/>
    public ISwitchManager CreateFor(SwitchEntity switchEntity) =>
        new SwitchManager(switchEntity.Id, _httpClientFactory, _stationsRepository, _switchesRepository);
    #endregion
}
