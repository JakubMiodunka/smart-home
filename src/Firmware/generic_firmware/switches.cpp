#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266HTTPClient.h>
#include <ESP8266WiFiMulti.h>
#include <ESP8266WebServer.h>

#include "config.h"
#include "serial_logging.h"
#include "switches.h"
#include "requests.h"

void SwitchRegistrationRequest::populateRequest(JsonDocument& request) const {
  request["switchLocalId"] = this->switchLocalId;
}

void UpdateSwitchRequest::populateRequest(JsonDocument& request) const {
  request["switchLocalId"] = this->switchLocalId;
  request["actualSwitchState"] = this->actualSwitchState;
}

void Switch::initialize() const {
  logToSerial(INFO, "Attempting to initialize switch: LOCAL_ID=[%d]", this->localId);

  pinMode(this->pinNumber, OUTPUT);

  logToSerial(INFO, "Switch initialization successful: LOCAL_ID=[%d]", this->localId);
}

bool Switch::getState() const {
  logToSerial(INFO, "Attempting to determine switch state: LOCAL_ID=[%d]", this->localId);
  
  bool switchState = (this->pinState == HIGH) != this->reversedLogic;

  logToSerial(INFO, "Switch state determined successfuly: LOCAL_ID=[%d], ACTUAL_STATE=[%d]", this->localId, switchState);

  return switchState;
}

void Switch::setState(const bool expectedState) {
  logToSerial(INFO, "Attempting to set switch state: LOCAL_ID=[%d], EXPECTED_STATE=[%d]", this->localId, expectedState);

  bool pinState = expectedState != this->reversedLogic;
  this->pinState = pinState ? HIGH : LOW;

  digitalWrite(this->pinNumber, this->pinState);

  logToSerial(INFO, "Switch state set successfuly: LOCAL_ID=[%d], ACTUAL_STATE=[%d]", this->localId, this->pinState);
}

bool Switch::tryRegisterOnRemoteServer(ESP8266WiFiMulti& wiFiManager) {
  logToSerial(INFO, "Attempting to register switch: LOCAL_ID=[%d]", this->localId);
  
  if (REMOTE_SERVER_API_VERSION != 1) {
    logToSerial(ERROR, "Not supported for specified remote API version: [API_VERSION=%u]", REMOTE_SERVER_API_VERSION);
    return false;
  }
  
  const String url = getRemoteBaseUrl() + "/switches/registration";
  const HttpMethod httpMethod = PUT;
  SwitchRegistrationRequest requestDto = { this->localId };
  JsonDocument request;
  JsonDocument response;
  int httpReturnCode;
    
  requestDto.populateRequest(request);
  sendHttpRequest(wiFiManager, url, httpMethod, request, response, httpReturnCode);
  bool wasOperationSuccessful = httpReturnCode == HTTP_CODE_OK;

  if (wasOperationSuccessful) {
    bool expectedSwitchState = response["expectedSwitchState"];
    this->setState(expectedSwitchState);

    logToSerial(INFO, "Switch registration successful: LOCAL_ID=[%d]", this->localId);
  }
  else {
    logToSerial(WARNING, "Switch registration failed: LOCAL_ID=[%d]", this->localId);
  }

  return wasOperationSuccessful;
}

void Switch::registerOnRemoteServer(ESP8266WiFiMulti& wiFiManager) {
  while (!this->tryRegisterOnRemoteServer(wiFiManager)) {
    logToSerial(WARNING, "Registration attempt failed. RETRY_INTERVAL=[%lu][ms]", REQUESTS_RETRY_INTERVAL);
    delay(REQUESTS_RETRY_INTERVAL);
  }
}

bool Switch::tryUpdateOnRemoteServer(ESP8266WiFiMulti& wiFiManager) const {
  logToSerial(INFO, "Attempting to update switch: LOCAL_ID=[%d]", this->localId);

  if (REMOTE_SERVER_API_VERSION != 1) {
    logToSerial(ERROR, "Not supported for specified remote API version: [API_VERSION=%u]", REMOTE_SERVER_API_VERSION);
    return false;
  }

  const String url = getRemoteBaseUrl() + "/switches/state";
  const HttpMethod httpMethod = PATCH;
  UpdateSwitchRequest requestDto = { this->localId, this->getState() };
  JsonDocument request;
  JsonDocument response;
  int httpReturnCode;

  requestDto.populateRequest(request);
  sendHttpRequest(wiFiManager, url, httpMethod, request, response, httpReturnCode);
  bool wasOperationSuccessful = httpReturnCode == HTTP_CODE_NO_CONTENT;

  if (wasOperationSuccessful) {
    logToSerial(INFO, "Switch update successful: LOCAL_ID=[%d]", this->localId);
  }
  else {
    logToSerial(WARNING, "Switch update failed: LOCAL_ID=[%d]", this->localId);
  }

  return wasOperationSuccessful;
}

void Switch::updateOnRemoteServer(ESP8266WiFiMulti& wiFiManager) const {
  while (!this->tryUpdateOnRemoteServer(wiFiManager)) {
    logToSerial(WARNING, "Update attempt failed. RETRY_INTERVAL=[%lu][ms]", REQUESTS_RETRY_INTERVAL);
    delay(REQUESTS_RETRY_INTERVAL);
  }
}

void Switch::setupLocalEndpoint(ESP8266WebServer& server) const {
  String endpoint = getLocalBaseUrl() + "/switches/" + String(this->localId);
  // TODO: Implement.
}
