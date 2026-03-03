#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266HTTPClient.h>
#include <ESP8266WiFiMulti.h>
#include <ESP8266WebServer.h>

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
  logToSerial(INFO, "Attempting to initialize switch: LOCAL_ID=[%d]", switchRef.localId);

  pinMode(switchRef.pinNumber, OUTPUT);

  logToSerial(INFO, "Switch initialization successful: LOCAL_ID=[%d]", switchRef.localId);

}

bool getSwitchState(const Switch& switchRef) {
  logToSerial(INFO, "Attempting to determine switch state: LOCAL_ID=[%d]", switchRef.localId);
  
  bool switchState = (switchRef.pinState == HIGH) != switchRef.reversedLogic;

  logToSerial(INFO, "Switch state determined successfuly: LOCAL_ID=[%d], ACTUAL_STATE=[%d]", switchRef.localId, switchState);

  return switchState;
}

void setSwitchState(Switch& switchRef, const bool expectedState) {
  logToSerial(INFO, "Attempting to set switch state: LOCAL_ID=[%d], EXPECTED_STATE=[%d]", switchRef.localId, expectedState);

  bool pinState = (expectedState == HIGH) != switchRef.reversedLogic;
  switchRef.pinState = pinState ? HIGH : LOW;

  digitalWrite(switchRef.pinNumber, switchRef.pinState);

  logToSerial(INFO, "Switch state set successfuly: LOCAL_ID=[%d], ACTUAL_STATE=[%d]", switchRef.localId, switchRef.pinState);
}

bool tryRegisterSwitch(ESP8266WiFiMulti& wiFiManager, Switch& switchRef) {
  logToSerial(INFO, "Attempting to register switch: LOCAL_ID=[%d]", switchRef.localId);
  
  if (REMOTE_SERVER_API_VERSION != 1) {
    logToSerial(ERROR, "Not supported for specified remote API version: [API_VERSION=%u]", REMOTE_SERVER_API_VERSION);
    return false;
  }
  
  const String url = getRemoteBaseUrl() + "/switches/registration";
  const HttpMethod httpMethod = PUT;
  JsonDocument request;
  JsonDocument response;
  int httpReturnCode;
  
  populateSwitchRegistrationRequest(request, switchRef.localId);
  sendHttpRequest(wiFiManager, url, httpMethod, request, response, httpReturnCode);
  bool wasOperationSuccessful = httpReturnCode == HTTP_CODE_OK;

  if (wasOperationSuccessful) {
    bool expectedSwitchState = response["expectedSwitchState"];
    setSwitchState(switchRef, expectedSwitchState);

    logToSerial(INFO, "Switch registration successful: LOCAL_ID=[%d]", switchRef.localId);
  }
  else {
    logToSerial(WARNING, "Switch registration failed: LOCAL_ID=[%d]", switchRef.localId);
  }

  return wasOperationSuccessful;
}

bool tryUpdateSwitch(ESP8266WiFiMulti& wiFiManager, const Switch& switchRef) {
  logToSerial(INFO, "Attempting to update switch: LOCAL_ID=[%d]", switchRef.localId);

  if (REMOTE_SERVER_API_VERSION != 1) {
    logToSerial(ERROR, "Not supported for specified remote API version: [API_VERSION=%u]", REMOTE_SERVER_API_VERSION);
    return false;
  }

  const String url = getRemoteBaseUrl() + "/switches/state";
  const HttpMethod httpMethod = PATCH;
  JsonDocument request;
  JsonDocument response;
  int httpReturnCode;
  
  bool switchState = getSwitchState(switchRef);
  populateUpdateSwitchStateRequest(request, switchRef.localId, switchState);
  sendHttpRequest(wiFiManager, url, httpMethod, request, response, httpReturnCode);
  bool wasOperationSuccessful = httpReturnCode == HTTP_CODE_NO_CONTENT;

  if (wasOperationSuccessful) {
    logToSerial(INFO, "Switch update successful: LOCAL_ID=[%d]", switchRef.localId);
  }
  else {
    logToSerial(WARNING, "Switch update failed: LOCAL_ID=[%d]", switchRef.localId);
  }

  return wasOperationSuccessful;
}
