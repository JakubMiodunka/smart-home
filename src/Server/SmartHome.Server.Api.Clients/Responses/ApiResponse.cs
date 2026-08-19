using System.Net;

namespace SmartHome.Server.Api.Clients.Responses;

/// <summary>
/// Model of API response.
/// </summary>
/// <param name="StatusCode">
/// The HTTP status code returned by the API
/// </param>
public sealed record ApiResponse(HttpStatusCode StatusCode);

/// <summary>
/// Model of API response.
/// </summary>
/// <typeparam name="T">
/// The type of the content object expected in the response body.
/// </typeparam>
/// <param name="StatusCode">
/// The HTTP status code returned by the API.
/// </param>
/// <param name="Content">
/// The deserialized response body content.
/// </param>
public sealed record ApiResponse<T>(HttpStatusCode StatusCode, T Content);
