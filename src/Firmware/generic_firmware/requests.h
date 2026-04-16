#ifndef REQUESTS_H
#define REQUESTS_H

#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266WiFiMulti.h>

#include "switches.h"

/// <summary>
/// Defines the HTTP methods supported in this library.
/// </summary>
enum HttpMethod {
  GET,
  PUT,
  POST,
  PATCH,
  DELETE
};

/// <summary>
/// Generates base URL for endpoints exposed by remote server API.
/// </summary>
/// <returns>
/// The base URL for endpoints exposed by remote server API.
/// </returns>
String getRemoteBaseUrl();

/// <summary>
/// Generates prefix for all endpoints exposed by local server API.
/// </summary>
/// <returns>
/// The base URL for endpoints exposed by local server API.
/// </returns>
String getLocalEndpointPrefix();

/// <summary>
/// Attempts to parse a JSON string into <see cref="JsonDocument"/> instance.
/// </summary>
/// <param name="jsonString">
/// JSON string to be parsed.
/// </param>
/// <param name="jsonDocument">
/// Reference to <see cref="JsonDocument"/> instance where the parsing result shall be stored.
/// </param>
/// <returns>
/// <see langword="true"/> if parsing was successful and all fields are valid, <see langword="false"/> otherwise.
/// </returns>
bool tryParseJsonString(const String jsonString, JsonDocument& jsonDocument);

/// <summary>
/// Sends HTTP request using a specified method and handles the communication lifecycle.
/// </summary>
/// <param name="wiFiManager">
/// Reference to the WiFi manager responsible for maintaining the network connection.
/// </param>
/// <param name="url">
/// The target URL where the request shall be sent.
/// </param>
/// <param name="httpMethod">
/// The <see cref="HttpMethod"/> to be used for the request.
/// </param>
/// <param name="request">
/// The request body to be sent.
/// Ignored when <paramref name="httpMethod"/> is set either to GET or DELETE.
/// </param>
/// <param name="response">
/// An output container for the parsed response body.
/// </param>
/// <param name="httpReturnCode">
/// The HTTP status code or internal error code returned from the remote server.
/// </param>
/// <returns>
/// <see langword="true"/> if operation was successful, <see langword="false"/> otherwise.
///</returns>
bool sendHttpRequest(ESP8266WiFiMulti& wiFiManager, const String url, const HttpMethod httpMethod, const JsonDocument& request, JsonDocument& response, int& httpReturnCode);

#endif