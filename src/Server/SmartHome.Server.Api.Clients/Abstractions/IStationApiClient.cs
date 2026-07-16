using System.Net;

namespace SmartHome.Server.Api.Clients.Abstractions;

/// <summary>
/// An HTTP client designed for communication with a specific station API.
/// </summary>
/// <remarks>
/// Responsible solely for the transport layer and HTTP client configuration. 
/// It is the caller's responsibility to ensure that the endpoint URL, 
/// HTTP method, and request body are valid and logically correct.
/// </remarks>
public interface IStationApiClient
{
    /// <summary>
    /// Sends an asynchronous HTTP request to the station associated with this client.
    /// </summary>
    /// <param name="endpointUrl">
    /// Absolute URL of the station API endpoint.
    /// </param>
    /// <param name="httpMethod">
    /// HTTP method to be used for the request.
    /// </param>
    /// <param name="requestBody">
    /// The object to be serialized into the HTTP request body,
    /// or <see langword="null"/> if no body is required.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// HTTP status code returned by the station API if 
    /// the request was processed successfully, <see langword="null"/> otherwise.
    /// </returns>
    Task<HttpStatusCode?> SendRequestAsync(
        Uri endpointUrl,
        HttpMethod httpMethod,
        object? requestBody,
        CancellationToken cancellationToken);
}
