#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266HTTPClient.h>
#include <ESP8266WiFiMulti.h>

#include "config.h"
#include "serial_logging.h"
#include "switches.h"
#include "requests.h"

/// <summary>
/// Populates the provided JSON document with switch registration data.
/// </summary>
/// <remarks>
/// Used internally - is not exposed in header file.
/// </remarks>
/// <param name="request">
/// The JSON document to be populated.
/// </param>
/// <param name="localId">
/// The identifier of the switch, unique at the station level.
/// </param>
static void populateSwitchRegistrationRequest(JsonDocument& request, byte localId) {
  request["switchLocalId"] = localId;
}

/// <summary>
/// Populates the provided JSON document with switch state update data.
/// </summary>
/// <remarks>
/// Used internally - is not exposed in header file.
/// </remarks>
/// <param name="request">
/// The JSON document to be populated.
/// </param>
/// <param name="localId">
/// The identifier of the switch, unique at the station level.
/// </param>
/// <param name="switchState">
/// Logical state of the switch - <see langword="true"/> 
/// if the switch is closed and current is flowing, <see langword="false"/> otherwise.
/// </param>
static void populateUpdateSwitchStateRequest(JsonDocument& request, byte localId, const bool switchState) {
  request["switchLocalId"] = localId;
  request["switchState"] = switchState;
}

void initializeSwitch(const Switch& switchRef) {
  pinMode(switchRef.pinNumber, OUTPUT);
}

bool getSwitchState(const Switch& switchRef) {
  bool switchState = switchRef.pinState == HIGH;
  switchState = switchRef.reversedLogic ? !switchState : switchState;
  return switchState;
}

void setSwitchState(Switch& switchRef, const bool desiredState) {
  bool pinState = desiredState;
  pinState = switchRef.reversedLogic ? !pinState : pinState;
  switchRef.pinState = pinState ? HIGH : LOW;

  digitalWrite(switchRef.pinNumber, switchRef.pinState);
}

bool tryRegisterSwitch(ESP8266WiFiMulti& wiFiManager, Switch& switchRef, const int localId) {
  if (SERVER_API_VERSION != 1) {
    logToSerial(ERROR, "Switch Registration not supported for specified API version: [SERVER_API_VERSION=%u]", SERVER_API_VERSION);
    return false;
  }
  
  const String url = getBaseUrl() + "/switches/registration";
  const HttpMethod httpMethod = PUT;
  JsonDocument request;
  JsonDocument response;
  int httpReturnCode;
  
  populateSwitchRegistrationRequest(request, localId);
  sendHttpRequest(wiFiManager, url, httpMethod, request, response, httpReturnCode);

  bool expectedSwitchState = response["expectedSwitchState"];
  setSwitchState(switchRef, expectedSwitchState);

  return httpReturnCode == HTTP_CODE_OK;
}

bool tryUpdateSwitchState(ESP8266WiFiMulti& wiFiManager, const Switch& switchRef, const int localId) {
  if (SERVER_API_VERSION != 1) {
    logToSerial(ERROR, "Updating switch state not supported for specified API version: [SERVER_API_VERSION=%u]", SERVER_API_VERSION);
    return false;
  }

  const String url = getBaseUrl() + "/switches/state";
  const HttpMethod httpMethod = PATCH;
  JsonDocument request;
  JsonDocument response;
  int httpReturnCode;
  
  bool switchState = getSwitchState(switchRef);
  populateUpdateSwitchStateRequest(request, localId, switchState);
  sendHttpRequest(wiFiManager, url, httpMethod, request, response, httpReturnCode);

  return httpReturnCode == HTTP_CODE_NO_CONTENT;
}
