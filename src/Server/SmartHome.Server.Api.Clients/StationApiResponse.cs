using System.Net;

namespace SmartHome.Server.Api.Clients;

/// <summary>
/// Model of station API response.
/// </summary>
/// <param name="StatusCode">
/// The HTTP status code returned by the API
/// </param>
public sealed record StationApiResponse(HttpStatusCode StatusCode);

/// <inheritdoc cref="StationApiResponse"/>
/// <typeparam name="T">
/// The type of the content object expected in the response body.
/// </typeparam>
/// <param name="Body">
/// The deserialized response body content.
/// </param>
public sealed record StationApiResponse<T>(HttpStatusCode StatusCode, T Body);
