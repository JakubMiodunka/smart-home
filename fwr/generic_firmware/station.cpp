#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266HTTPClient.h>
#include <ESP8266WiFiMulti.h>

#include "config.h"
#include "serial_logging.h"
#include "station.h"
#include "requests.h"

void StationRegistrationStationRequest::toJsonDocument(JsonDocument& jsonDocument) const {
  jsonDocument["stationMacAddress"] = this->stationMacAddress;
  jsonDocument["stationApiPort"] = this->stationApiPort;
  jsonDocument["stationApiVersion"] = this->stationApiVersion;
}

/// <summary>
/// Attempts to register the station on the remote server.
/// </summary>
/// <param name="wiFiManager">
/// Reference to the WiFi manager responsible for maintaining the network connection.
/// </param>
/// <param name="macAddress">
/// The MAC address of the station.
/// </param>
/// <returns>
/// <see langword="true"/> if the attempt was successful, <see langword="false"/> otherwise.
/// </returns>
static const bool tryRegisterStationOnRemoteServer(ESP8266WiFiMulti& wiFiManager, const String macAddress) {
  logToSerial(INFO, "Attempting to register station: MAC_ADDRESS=[%s]", macAddress.c_str());

  if (REMOTE_SERVER_API_VERSION != 1) {
    logToSerial(ERROR, "Not supported for specified remote API version: [API_VERSION=%u]", REMOTE_SERVER_API_VERSION);
    return false;
  }

  const String url = getRemoteBaseUrl() + "/stations";
  const HttpMethod httpMethod = PUT;
  StationRegistrationStationRequest request = { macAddress, LOCAL_SERVER_PORT, LOCAL_SERVER_API_VERSION };
  JsonDocument requestJson;
  request.toJsonDocument(requestJson);
  JsonDocument responseJson;
  int httpReturnCode;

  sendHttpRequest(wiFiManager, url, httpMethod, requestJson, responseJson, httpReturnCode);
  bool wasOperationSuccessful = httpReturnCode == HTTP_CODE_NO_CONTENT;

  if (wasOperationSuccessful) {
    logToSerial(INFO, "Station registration successful: MAC_ADDRESS=[%s]", macAddress.c_str());
  }
  else {
    logToSerial(WARNING, "Station registration failed: MAC_ADDRESS=[%s]", macAddress.c_str());
  }

  return wasOperationSuccessful;
}

void registerStationOnRemoteServer(ESP8266WiFiMulti& wiFiManager, const String macAddress) {
  while (!tryRegisterStationOnRemoteServer(wiFiManager, macAddress)) {
    logToSerial(WARNING, "Registration attempt failed. RETRY_INTERVAL=[%lu][ms]", REQUESTS_RETRY_INTERVAL);
    delay(REQUESTS_RETRY_INTERVAL);
  }
}

bool trySendHeartbeatSignal(ESP8266WiFiMulti& wiFiManager) {
  logToSerial(INFO, "Attempting to send heartbeat signal:");

  if (REMOTE_SERVER_API_VERSION != 1) {
    logToSerial(ERROR, "Not supported for specified remote API version: [API_VERSION=%u]", REMOTE_SERVER_API_VERSION);
    return false;
  }

  const String url = getRemoteBaseUrl() + "/stations/heartbeat";
  const HttpMethod httpMethod = PUT;
  JsonDocument requestJson;
  JsonDocument responseJson;
  int httpReturnCode;

  sendHttpRequest(wiFiManager, url, httpMethod, requestJson, responseJson, httpReturnCode);
  bool wasOperationSuccessful = httpReturnCode == HTTP_CODE_NO_CONTENT;

  if (wasOperationSuccessful) {
    logToSerial(INFO, "Heartbeat signal sent successfully:");
  }
  else {
    logToSerial(WARNING, "Failed to sent heartbeat signal:");
  }

  return wasOperationSuccessful;
}
