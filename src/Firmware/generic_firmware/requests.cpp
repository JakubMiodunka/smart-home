#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266WiFiMulti.h>
#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>

#include "requests.h"
#include "switches.h"
#include "station.h"
#include "serial_logging.h"

static const String BASE_URL = "http://192.168.0.199:5236";
static const String STATION_REGISTRATION_ENDPOINT = "/firmware-api/v1/stations";
static const String SWITCHES_REGISTRATION_ENDPOINT = "/firmware-api/v1/switches";

/// <summary>
/// Defines the HTTP methods supported in this library.
/// </summary>
/// <remarks>
/// Used internally - is not exposed in header file.
/// </remarks>
enum HttpMethod {
  GET,
  PUT,
  POST,
  PATCH,
  DELETE
};

/// <summary>
/// Sends HTTP request using a specified method and handles the communication lifecycle.
/// </summary>
/// <remarks>
/// Used internally - is not exposed in header file.
/// </remarks>
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
static bool sendHttpRequest(ESP8266WiFiMulti& wiFiManager, const String url, const HttpMethod httpMethod, const JsonDocument& request, JsonDocument& response, int& httpReturnCode) {
  wl_status_t connectionStatus = wiFiManager.run();
  if (connectionStatus != WL_CONNECTED) {
    logToSerial(ERROR, "WiFi connection failed: STATUS=[%d]", connectionStatus);
    return false;
  }

  WiFiClient wifiClient;
  HTTPClient httpClient;
  
  if (!httpClient.begin(wifiClient, url)) {
    logToSerial(ERROR, "Failed to configure HTTP client: URL=[%s]", url.c_str());
    return false;
  }

  String serializedRequest;
  serializeJson(request, serializedRequest);

  logToSerial(DEBUG, "Sending HTTP POST request: URL=[%s], REQUEST_BODY=[%s]", url.c_str(), serializedRequest.c_str());

  httpClient.addHeader("Content-Type", "application/json");
  switch (httpMethod) {
    case GET:
      httpReturnCode = httpClient.GET();
      break;
    case PUT:
      httpReturnCode = httpClient.PUT(serializedRequest);
      break;
    case POST:
      httpReturnCode = httpClient.POST(serializedRequest);
      break;
    case PATCH:
      httpReturnCode = httpClient.PATCH(serializedRequest);
      break;
    case DELETE:
      httpReturnCode = httpClient.DELETE();
      break;
    default:
      logToSerial(ERROR, "HTTP method not supported: HTTP_METHOD=[%d]", httpMethod);
      return false;
  }
  
  if (httpReturnCode > 0) {
    String serializedResponse = httpClient.getString();
    logToSerial(DEBUG, "Received response: HTTP_RETURN_CODE=[%i], RESPONSE_BODY=[%s]", httpReturnCode, serializedResponse.c_str());

    DeserializationError deserializationError = deserializeJson(response, serializedResponse);

    if (deserializationError) {
      logToSerial(ERROR, "Response deserialization failed: ERROR_MESSAGE=[%s]", deserializationError.c_str());
    }
  }
  else {
    String errorMessage = httpClient.errorToString(httpReturnCode);
    logToSerial(DEBUG, "Received response: HTTP_RETURN_CODE=[%i], ERROR_MESSAGE=[%s]", httpReturnCode, errorMessage.c_str());
  }

  httpClient.end();

  return true;
}

boolean tryRegisterStation(ESP8266WiFiMulti& wiFiManager, String macAddress) {
  const String url = BASE_URL + STATION_REGISTRATION_ENDPOINT;
  const HttpMethod httpMethod = POST;
  JsonDocument request;
  JsonDocument response;
  int httpReturnCode;

  populateStationRegistrationRequest(request, macAddress);
  sendHttpRequest(wiFiManager, url, httpMethod, request, response, httpReturnCode);

  return httpReturnCode == HTTP_CODE_NO_CONTENT;
}

boolean tryRegisterSwitch(ESP8266WiFiMulti& wiFiManager, Switch& switchToRegister, const String macAddress, int localId) {
  const String url = BASE_URL + SWITCHES_REGISTRATION_ENDPOINT;
  const HttpMethod httpMethod = POST;
  JsonDocument request;
  JsonDocument response;
  int httpReturnCode;
  
  populateSwitchRegistrationRequest(request, macAddress, localId);
  sendHttpRequest(wiFiManager, url, httpMethod, request, response, httpReturnCode);

  return httpReturnCode == HTTP_CODE_NO_CONTENT;
}