using SmartHome.Server.Api.Clients.Abstractions;
using SmartHome.Server.Repositories.Entities;
using System.Net;

namespace SmartHome.Server.Features.Managers.Abstractions;

/// <summary>
/// Set of common utilities for all feature managers,
/// which are responsible for managing specific features of stations within the system.
/// </summary>
internal abstract class FeatureManager
{
    #region Properties
    protected abstract TimeSpan HttpClientTimeout { get; }
    protected IStationApiClientFactory StationApiClientsFactory { get; init; }
    public StationEntity ParentStation { get; init; }
    #endregion

    #region Instatiation
    /// <summary>
    /// Creates new instance of <see cref="FeatureManager"/>.
    /// </summary>
    /// <param name="parentStation">
    /// Parent station of managed feature.
    /// </param>
    /// <param name="stationApiClientsFactory">
    /// Factory of station API clients, which shall be used to obtain clients
    /// capable of communicating with station associated with the managed feature.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable argument is a <see langword="null"/> reference.
    /// </exception>
    protected FeatureManager(StationEntity parentStation, IStationApiClientFactory stationApiClientsFactory)
    {
        ArgumentNullException.ThrowIfNull(parentStation, nameof(parentStation));
        ArgumentNullException.ThrowIfNull(stationApiClientsFactory, nameof(stationApiClientsFactory));
        
        ParentStation = parentStation;
        StationApiClientsFactory = stationApiClientsFactory;
    }
    #endregion

    #region Utilities
    /// <summary>
    /// Determines the base URL for the API exposed by the anaged feature parent station.
    /// </summary>
    /// <returns>
    /// Absolute base URL for the API exposed by the station,
    /// or <see langword="null"/> if the station is unreachable.
    /// </returns>
    protected Uri? GetStationBaseApiUrl()
    {
        if (ParentStation.IpAddress is IPAddress ipAddress
            && ParentStation.ApiPort is int apiPort
            && ParentStation.ApiVersion is byte apiVersion)
        {
            var builder = new UriBuilder(Uri.UriSchemeHttp, ipAddress.ToString(), apiPort, $"api/v{apiVersion}/");
            return builder.Uri;
        }

        return null;
    }
    #endregion
}
