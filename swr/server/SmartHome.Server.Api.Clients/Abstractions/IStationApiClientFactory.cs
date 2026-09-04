using SmartHome.Server.Repositories.Entities;

namespace SmartHome.Server.Api.Clients.Abstractions;

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
    /// <param name="responseTimeout">
    /// The maximum time to wait for a station API response.
    /// </param>
    /// <returns>
    /// An <see cref="IStationApiClient"/> instance configured to communicate with the specified station.
    /// </returns>
    IStationApiClient CreateFor(StationEntity stationEntity, TimeSpan responseTimeout);
}
