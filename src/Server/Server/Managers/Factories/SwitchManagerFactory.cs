using SmartHome.Server.ApiClients.StationApi;
using SmartHome.Server.Data.Models.Entities;

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
    /// <param name="parentStation">
    /// Entity of station, that controls the specified switch.
    /// </param>
    /// <returns>
    /// An <see cref="ISwitchManager"/> instance that allows performing operations on the switch.
    /// </returns>
    ISwitchManager CreateFor(SwitchEntity switchEntity, StationEntity parentStation);
}

/// <inheritdoc cref="ISwitchManagerFactory"/>
public sealed class SwitchManagerFactory : ISwitchManagerFactory
{
    #region Properties
    private readonly IStationApiClientsFactory _stationApiClientsFactory;
    private readonly ILoggerFactory _loggerFactory;
    #endregion

    #region Instantiation
    /// <summary>
    /// Creates a new instance of <see cref="SwitchManagerFactory"/>.
    /// </summary>
    /// <param name="stationApiClientsFactory">
    /// Factory, which shall be used to obtain station API clients for created managers.
    /// </param>
    /// <param name="loggerFactory">
    /// Factory, which shall be used to obtain loggers for created managers.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable argument is a <see langword="null"/> reference.
    /// </exception>
    public SwitchManagerFactory(
        IStationApiClientsFactory stationApiClientsFactory,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(stationApiClientsFactory, nameof(stationApiClientsFactory));
        ArgumentNullException.ThrowIfNull(loggerFactory, nameof(loggerFactory));

        _stationApiClientsFactory = stationApiClientsFactory;
        _loggerFactory = loggerFactory;
    }
    #endregion

    #region Interactions
    /// <inheritdoc cref="ISwitchManagerFactory.CreateFor(SwitchEntity)"/>
    public ISwitchManager CreateFor(SwitchEntity switchEntity, StationEntity parentStation) =>
        new SwitchManager(
            switchEntity,
            parentStation,
            _stationApiClientsFactory,
            _loggerFactory.CreateLogger<SwitchManager>());
    #endregion
}
