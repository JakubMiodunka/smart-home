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
    private readonly ILoggerFactory _loggerFactory;
    #endregion

    #region Instationation
    public SwitchManagerFactory(
        IHttpClientFactory httpClientFactory,
        IStationsRepository stationsRepository,
        ISwitchesRepository switchesRepository,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(stationsRepository);
        ArgumentNullException.ThrowIfNull(switchesRepository);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _httpClientFactory = httpClientFactory;
        _stationsRepository = stationsRepository;
        _switchesRepository = switchesRepository;
        _loggerFactory = loggerFactory;
    }
    #endregion

    #region Interactions
    /// <inheritdoc cref="ISwitchManagerFactory.CreateFor(SwitchEntity)"/>
    public ISwitchManager CreateFor(SwitchEntity switchEntity) =>
        new SwitchManager(
            switchEntity,
            _httpClientFactory,
            _stationsRepository,
            _switchesRepository,
            _loggerFactory.CreateLogger<SwitchManager>());
    #endregion
}
