#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266WiFiMulti.h>
#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>

#include "config.h"
#include "serial_logging.h"
#include "requests.h"

/// <summary>
/// Converts the specified HTTP method to its string representation.
/// </summary>
/// <remarks>
/// Used internally - is not exposed in header file.
/// </remarks>
/// <param name="httpMethod">
/// The HTTP method enum value to convert.
/// </param>
/// <returns>
/// Name of provided HTTP method.
/// Returns "UNKNOWN" if provided HTTP method is invalid.
///</returns>
static const char* toHttpMethodName(const HttpMethod httpMethod) {
  const char* names[] = {"GET", "PUT", "POST", "PATCH", "DELETE"};
  return (0  <= httpMethod && httpMethod <= 4) ? names[httpMethod] : "UNKNOWN";
}

String getBaseUrl() {
  return String(PROTOCOL) + "://" + SERVER_IP_ADDRESS + ":" + String(SERVER_PORT) + "/api/firmware/v" + String(SERVER_API_VERSION);
}

bool sendHttpRequest(ESP8266WiFiMulti& wiFiManager, const String url, const HttpMethod httpMethod, const JsonDocument& request, JsonDocument& response, int& httpReturnCode) {
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

  logToSerial(DEBUG, "Sending HTTP %s request: URL=[%s], REQUEST_BODY=[%s]", toHttpMethodName(httpMethod), url.c_str(), serializedRequest.c_str());

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

    if (serializedResponse.length() != 0) {
      DeserializationError deserializationError = deserializeJson(response, serializedResponse);
      
      if (deserializationError) {
        logToSerial(ERROR, "Response deserialization failed: ERROR_MESSAGE=[%s]", deserializationError.c_str());
      }
    }
  }
  else {
    String errorMessage = httpClient.errorToString(httpReturnCode);
    logToSerial(DEBUG, "Received response: HTTP_RETURN_CODE=[%i], ERROR_MESSAGE=[%s]", httpReturnCode, errorMessage.c_str());
  }

  httpClient.end();

  return true;
}
