using SmartHome.Server.Data.Models.Entities;

namespace SmartHome.Server.ApiClients.StationApi;

/// <summary>
/// Factory for creating station API clients.
/// </summary>
public interface IStationApiClientFactory
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
    /// An <see cref="IStationApiClient"/> instance configured to communicate with the specified station.
    /// </returns>
    IStationApiClient CreateFor(StationEntity stationEntity, TimeSpan connectionTimeout);
}

/// <inheritdoc cref="IStationApiClientFactory"/>
public sealed class StationApiClientFactory : IStationApiClientFactory
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
    public StationApiClientFactory(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory, nameof(httpClientFactory));
        ArgumentNullException.ThrowIfNull(loggerFactory, nameof(loggerFactory));

        _httpClientFactory = httpClientFactory;
        _loggerFactory = loggerFactory;
    }
    #endregion

    #region Interactions
    /// <inheritdoc cref="IStationApiClientFactory"/>
    public IStationApiClient CreateFor(StationEntity stationEntity, TimeSpan connectionTimeout) =>
        new StationApiClient(
            stationEntity,
            _httpClientFactory,
            connectionTimeout,
            _loggerFactory.CreateLogger<StationApiClient>());
    #endregion
}
