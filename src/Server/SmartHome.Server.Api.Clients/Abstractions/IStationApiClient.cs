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
    /// <param name="expectedResponseStatusCode">
    /// The expected status code of the response.
    /// If the actual status code would not match this value,
    /// the request will be considered as failed.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the request was processed successfully,
    /// <see langword="false"/> otherwise.
    /// </returns>
    Task<bool> TrySendRequestAsync(
        Uri endpointUrl,
        HttpMethod httpMethod,
        object? requestBody,
        HttpStatusCode expectedResponseStatusCode,
        CancellationToken cancellationToken);

    /// <summary>
    /// Sends an asynchronous HTTP request to the station associated with this client.
    /// </summary>
    /// <typeparam name="T">
    /// Type to which response body should be attempted to be deserialized.
    /// </typeparam>
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
    /// <param name="expectedResponseStatusCode">
    /// The expected status code of the response.
    /// If the actual status code would not match this value
    /// the request will be considered as failed.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// Representation of the HTTP response body returned by the station API if 
    /// the request was processed successfully, <see langword="null"/> otherwise.
    /// </returns>
    Task<T?> SendRequestAsync<T>(
        Uri endpointUrl,
        HttpMethod httpMethod,
        object? requestBody,
        HttpStatusCode expectedResponseStatusCode,
        CancellationToken cancellationToken) where T : class;
}
