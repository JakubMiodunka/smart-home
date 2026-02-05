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
  if (SERVER_API_VERSION != 1) {
    logToSerial(ERROR, "Station registration not supported for specified API version: [SERVER_API_VERSION=%u]", SERVER_API_VERSION);
    return false;
  }

  const String url = getBaseUrl() + "/stations/registration";
  const HttpMethod httpMethod = PUT;
  JsonDocument request;
  JsonDocument response;
  int httpReturnCode;

  populateStationRegistrationRequest(request, macAddress);
  sendHttpRequest(wiFiManager, url, httpMethod, request, response, httpReturnCode);

  return httpReturnCode == HTTP_CODE_NO_CONTENT;
}