using Microsoft.Extensions.Logging;
using SmartHome.Server.Api.Clients.Abstractions;
using SmartHome.Server.Feature.Managers.Abstractions;
using SmartHome.Server.Repositories.Entities;

namespace SmartHome.Server.Feature.Managers;

/// <inheritdoc cref="ISwitchManagerFactory"/>
public sealed class SwitchManagerFactory : ISwitchManagerFactory
{
    #region Properties
    private readonly IStationApiClientFactory _stationApiClientsFactory;
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
        IStationApiClientFactory stationApiClientsFactory,
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
