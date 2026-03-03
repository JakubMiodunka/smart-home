#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266HTTPClient.h>
#include <ESP8266WiFiMulti.h>

#include "config.h"
#include "serial_logging.h"
#include "station.h"
#include "requests.h"

/// <summary>
/// Populates the provided JSON document with station registration data.
/// </summary>
/// <remarks>
/// Used internally - is not exposed in header file.
/// </remarks>
/// <param name="request">
/// The JSON document to be populated with registration data.
/// </param>
/// <param name="macAddress">
/// Station MAC address.
/// </param>
static void populateStationRegistrationRequest(JsonDocument& request, const String macAddress) {
  request["macAddress"] = macAddress;
}

bool tryRegisterStation(ESP8266WiFiMulti& wiFiManager, const String macAddress) {
  logToSerial(INFO, "Attempting to register station: MAC_ADDRESS=[%s]", macAddress.c_str());

  if (REMOTE_SERVER_API_VERSION != 1) {
    logToSerial(ERROR, "Not supported for specified remote API version: [API_VERSION=%u]", REMOTE_SERVER_API_VERSION);
    return false;
  }

  const String url = getRemoteBaseUrl() + "/stations/registration";
  const HttpMethod httpMethod = PUT;
  JsonDocument request;
  JsonDocument response;
  int httpReturnCode;

  populateStationRegistrationRequest(request, macAddress);
  sendHttpRequest(wiFiManager, url, httpMethod, request, response, httpReturnCode);
  bool wasOperationSuccessful = httpReturnCode == HTTP_CODE_NO_CONTENT;

  if (wasOperationSuccessful) {
    logToSerial(INFO, "Station registration successful: MAC_ADDRESS=[%s]", macAddress.c_str());
  }
  else {
    logToSerial(WARNING, "Station registration failed: MAC_ADDRESS=[%s]", macAddress.c_str());
  }

  return wasOperationSuccessful;
}

bool trySendHeartbeatSignal(ESP8266WiFiMulti& wiFiManager) {
  logToSerial(INFO, "Attempting to send heartbeat signal:");

  if (REMOTE_SERVER_API_VERSION != 1) {
    logToSerial(ERROR, "Not supported for specified remote API version: [API_VERSION=%u]", REMOTE_SERVER_API_VERSION);
    return false;
  }

  const String url = getRemoteBaseUrl() + "/stations/heartbeat";
  const HttpMethod httpMethod = PUT;
  JsonDocument request;
  JsonDocument response;
  int httpReturnCode;

  sendHttpRequest(wiFiManager, url, httpMethod, request, response, httpReturnCode);
  bool wasOperationSuccessful = httpReturnCode == HTTP_CODE_NO_CONTENT;

  if (wasOperationSuccessful) {
    logToSerial(INFO, "Heartbeat signal sent successfully:");
  }
  else {
    logToSerial(WARNING, "Failed to sent heartbeat signal:");
  }

  return wasOperationSuccessful;
}
