#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266HTTPClient.h>
#include <ESP8266WiFiMulti.h>

#include "switches.h"
#include "requests.h"

void initializeSwitch(const Switch& switchRef) {
  pinMode(switchRef.pinNumber, OUTPUT);
}

boolean getSwitchState(const Switch& switchRef) {
  boolean switchState = switchRef.pinState == HIGH;
  switchState = switchRef.reversedLogic ? !switchState : switchState;
  return switchState;
}

void setSwitchState(Switch& switchRef, const boolean desiredState) {
  boolean pinState = desiredState;
  pinState = switchRef.reversedLogic ? !pinState : pinState;
  switchRef.pinState = pinState ? HIGH : LOW;

  digitalWrite(switchRef.pinNumber, switchRef.pinState);
}

void populateSwitchRegistrationRequest(JsonDocument& request, const String macAddress, byte localId) {
  request["stationMacAddress"] = macAddress;
  request["switchLocalId"] = localId;
}

boolean tryRegisterSwitch(ESP8266WiFiMulti& wiFiManager, const String baseUrl, Switch& switchRef, const String macAddress, int localId) {
  const String url = baseUrl + "/switches/registration";
  const HttpMethod httpMethod = PUT;
  JsonDocument request;
  JsonDocument response;
  int httpReturnCode;
  
  populateSwitchRegistrationRequest(request, macAddress, localId);
  sendHttpRequest(wiFiManager, url, httpMethod, request, response, httpReturnCode);

  boolean expectedSwitchState = response["expectedSwitchState"];
  setSwitchState(switchRef, expectedSwitchState);

  return httpReturnCode == HTTP_CODE_OK;
}

void populateUpdateSwitchStateRequest(JsonDocument& request, const String macAddress, byte localId, boolean switchState) {
  request["stationMacAddress"] = macAddress;
  request["switchLocalId"] = localId;
  request["switchState"] = switchState;
}

boolean tryUpdateSwitchState(ESP8266WiFiMulti& wiFiManager, const String baseUrl, const Switch& switchRef, const String macAddress, int localId) {
  const String url = baseUrl + "/switches/state";
  const HttpMethod httpMethod = PATCH;
  JsonDocument request;
  JsonDocument response;
  int httpReturnCode;
  
  boolean switchState = getSwitchState(switchRef);
  populateUpdateSwitchStateRequest(request, macAddress, localId, switchState);
  sendHttpRequest(wiFiManager, url, httpMethod, request, response, httpReturnCode);

  return httpReturnCode == HTTP_CODE_NO_CONTENT;
}