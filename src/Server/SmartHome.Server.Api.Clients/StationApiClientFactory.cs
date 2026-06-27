using Microsoft.Extensions.Logging;
using SmartHome.Server.Api.Clients.Abstractions;
using SmartHome.Server.Repositories.Entities;

namespace SmartHome.Server.Api.Clients;



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
