using SmartHome.Server.Data.Models.Entities;

namespace SmartHome.Server.ApiClients.StationApi;

/// <summary>
/// Factory for creating station API clients.
/// </summary>
public interface IStationApiClientsFactory
{
    /// <summary>
    /// Creates client dedicated for communication with a specified station API.
    /// </summary>
    /// <param name="stationEntity">
    /// Station, which shall be associated with created client.
    /// </param>
    /// <param name="connectionTimeout">
    /// The maximum time to wait for a station API response.
    /// </param>
    /// <returns>
    /// HTTP client configured for the specified station.
    /// </returns>
    IStationApiClient CreateFor(StationEntity stationEntity, TimeSpan connectionTimeout);
}

/// TODO: Add unit tests.
/// <inheritdoc cref="IStationApiClientsFactory"/>
public sealed class StationApiClientsFactory : IStationApiClientsFactory
{
    #region Properties
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerFactory _loggerFactory;
    #endregion

    #region Instantiation
    /// <summary>
    /// Creates a new factory for station API clients.
    /// </summary>
    /// <param name="httpClientFactory">
    /// HTTP clients factory which shall be passed to created API clients.
    /// </param>
    /// <param name="loggerFactory">
    /// Logger factory, which shall be used to obtain loggers for created API clients.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable argument is a <see langword="null"/> reference.
    /// </exception>
    public StationApiClientsFactory(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _httpClientFactory = httpClientFactory;
        _loggerFactory = loggerFactory;
    }
    #endregion

    #region Interactions
    /// <inheritdoc cref="IStationApiClientsFactory"/>
    public IStationApiClient CreateFor(StationEntity stationEntity, TimeSpan connectionTimeout) =>
        new StationApiClient(
            stationEntity,
            _httpClientFactory,
            connectionTimeout,
            _loggerFactory.CreateLogger<StationApiClient>());
    #endregion
}
