#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266WiFiMulti.h>
#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>

#include "requests.h"
#include "switches.h"
#include "station.h"

/// <summary>
/// Serializes the provided JSON document into a minified string representation.
/// </summary>
/// <remarks>
/// Function used internally - is not exposed in header file.
/// </remarks>
/// <param name="document">
/// A reference to the JSON document to be serialized.
/// </param>
/// <returns>
/// A String containing the serialized JSON data.
/// </returns>
static String serializeToString(const JsonDocument& document)
{
  String serialized_document;
  serializeJson(document, serialized_document);
  return serialized_document;
}

boolean tryRegisterStation(ESP8266WiFiMulti& wiFiManager, String macAddress) {
  if (wiFiManager.run() != WL_CONNECTED) {
    return false;
  }

  WiFiClient wifiClient;
  HTTPClient httpClient;

  if (!httpClient.begin(wifiClient, "http://192.168.0.199:5236/api/v1/stations")) {
    return false;
  }

  JsonDocument request;
  populateStationRegistrationRequest(request, macAddress);

  httpClient.addHeader("Content-Type", "application/json");
  String serializedRequest = serializeToString(request);
  int httpReturnCode = httpClient.POST(serializedRequest);
  
  httpClient.end();

  return httpReturnCode == HTTP_CODE_OK || httpReturnCode == HTTP_CODE_CREATED;
}

boolean tryRegisterSwitch(ESP8266WiFiMulti& wiFiManager, Switch& switchToRegister, int localId) {
  if (wiFiManager.run() != WL_CONNECTED) {
    return false;
  }

  WiFiClient wifiClient;
  HTTPClient httpClient;

  if (!httpClient.begin(wifiClient, "http://192.168.0.199:5236/api/v1/electrical-switches")) {
    return false;
  }

  JsonDocument registrationRequest;
  populateSwitchRegistrationRequest(registrationRequest, localId, switchToRegister.pinState);

  httpClient.addHeader("Content-Type", "application/json");
  int httpReturnCode = httpClient.POST(serializeToString(registrationRequest));

  httpClient.end();

  return httpReturnCode == HTTP_CODE_OK || httpReturnCode == HTTP_CODE_CREATED;
}