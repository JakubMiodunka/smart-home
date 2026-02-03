#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266HTTPClient.h>
#include <ESP8266WiFiMulti.h>

#include "station.h"
#include "requests.h"

void populateStationRegistrationRequest(JsonDocument& request, String macAddress) {
  request["macAddress"] = macAddress;
}

boolean tryRegisterStation(ESP8266WiFiMulti& wiFiManager, const String baseUrl, const String macAddress) {
  const String url = baseUrl + "/stations/registration";
  const HttpMethod httpMethod = PUT;
  JsonDocument request;
  JsonDocument response;
  int httpReturnCode;

  populateStationRegistrationRequest(request, macAddress);
  sendHttpRequest(wiFiManager, url, httpMethod, request, response, httpReturnCode);

  return httpReturnCode == HTTP_CODE_NO_CONTENT;
}