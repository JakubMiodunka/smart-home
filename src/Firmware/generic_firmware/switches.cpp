#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266HTTPClient.h>
#include <ESP8266WiFiMulti.h>
#include <ESP8266WebServer.h>

#include "config.h"
#include "serial_logging.h"
#include "switches.h"
#include "requests.h"

void SwitchRegistrationStationRequest::toJsonDocument(JsonDocument& jsonDocument) const {
  jsonDocument["switchLocalId"] = this->switchLocalId;
}

bool SwitchRegistrationServerResponse::tryParseJsonDocument(const JsonDocument& jsonDocument, SwitchRegistrationServerResponse& response) {
  logToSerial(DEBUG, "Attempting to parse JSON document:");

  static constexpr const char* SWITCH_ID_KEY = "switchId";
  JsonVariantConst switchIdVariant = jsonDocument[SWITCH_ID_KEY];

  if (switchIdVariant.isNull()) {
    logToSerial(ERROR, "JSON key not found: JSON_KEY=[%s]", SWITCH_ID_KEY);
    return false;
  }
  
  if (!switchIdVariant.is<uint32_t>()) {
    logToSerial(ERROR, "Type of JSON key invalid: JSON_KEY=[%s], EXPECTED_TYPE=[uint32_t]", SWITCH_ID_KEY);
    return false;
  }

  static constexpr const char* EXPECTED_SWITCH_STATE_KEY = "expectedSwitchState";
  JsonVariantConst expectedSwitchStateVariant = jsonDocument[EXPECTED_SWITCH_STATE_KEY];

  if (expectedSwitchStateVariant.isNull()) {
    logToSerial(ERROR, "JSON key not found: JSON_KEY=[%s]", EXPECTED_SWITCH_STATE_KEY);
    return false;
  }
  
  if (!expectedSwitchStateVariant.is<bool>()) {
    logToSerial(ERROR, "Type of JSON key invalid: JSON_KEY=[%s], EXPECTED_TYPE=[bool]", EXPECTED_SWITCH_STATE_KEY);
    return false;
  }

  response.switchId = switchIdVariant.as<uint32_t>();
  response.expectedSwitchState = expectedSwitchStateVariant.as<bool>();

  logToSerial(DEBUG, "JSON document parsing successful:");
  return true;
}

void UpdateSwitchStationRequest::toJsonDocument(JsonDocument& jsonDocument) const {
  jsonDocument["actualSwitchState"] = this->actualSwitchState;
}

bool UpdateSwitchServerRequest::tryParseJsonDocument(const JsonDocument& jsonDocument, UpdateSwitchServerRequest &request) {
  logToSerial(DEBUG, "Attempting to parse JSON document:");

  static constexpr const char* EXPECTED_SWITCH_STATE_KEY = "expectedSwitchState";
  JsonVariantConst expectedSwitchStateVariant = jsonDocument[EXPECTED_SWITCH_STATE_KEY];

  if (expectedSwitchStateVariant.isNull()) {
    logToSerial(ERROR, "JSON key not found: JSON_KEY=[%s]", EXPECTED_SWITCH_STATE_KEY);
    return false;
  }
  
  if (!expectedSwitchStateVariant.is<bool>()) {
    logToSerial(ERROR, "Type of JSON key invalid: JSON_KEY=[%s], EXPECTED_TYPE=[bool]", EXPECTED_SWITCH_STATE_KEY);
    return false;
  }

  request.expectedSwitchState = expectedSwitchStateVariant.as<bool>();

  logToSerial(DEBUG, "JSON deserialization successful:");
  return true;
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
  
  const String url = getRemoteBaseUrl() + "/switches";
  const HttpMethod httpMethod = PUT;
  SwitchRegistrationStationRequest request = { this->localId };
  JsonDocument requestJson;
  request.toJsonDocument(requestJson);
  JsonDocument responseJson;
  int httpReturnCode;
    
  sendHttpRequest(wiFiManager, url, httpMethod, requestJson, responseJson, httpReturnCode);
  bool wasOperationSuccessful = httpReturnCode == HTTP_CODE_OK;

  if (!wasOperationSuccessful) {
    logToSerial(WARNING, "Switch registration failed: LOCAL_ID=[%d]", this->localId);
    return false;
  }

  SwitchRegistrationServerResponse response;
  if (SwitchRegistrationServerResponse::tryParseJsonDocument(responseJson, response)) {
    this->id = response.switchId;
    this->setState(response.expectedSwitchState);

    logToSerial(INFO, "Switch registration successful: LOCAL_ID=[%d]", this->localId);
    return true;
  }

  logToSerial(ERROR, "Remote server response parsing failed:");
  return false;
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

  const String url = getRemoteBaseUrl() + "/switches/" + String(this->id);
  const HttpMethod httpMethod = PATCH;
  UpdateSwitchStationRequest request = { this->getState() };
  JsonDocument requestJson;
  request.toJsonDocument(requestJson);
  JsonDocument responseJson;
  int httpReturnCode;

  sendHttpRequest(wiFiManager, url, httpMethod, requestJson, responseJson, httpReturnCode);
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

void Switch::setupControlEndpoint(ESP8266WebServer& server) {
  String endpoint = getLocalEndpointPrefix() + "/switches/" + String(this->localId);
  logToSerial(INFO, "Attempting to setup an endpoint: ENDPOINT=[%s]", endpoint.c_str());

  server.on(endpoint, HTTP_PATCH, [this, &server]() {
    String requestBody = server.arg("plain");
    logToSerial(INFO, "Request received: TYPE=[UpdateSwitchServerRequest], BODY=[%s]", requestBody.c_str());
    
    JsonDocument requestJson;
    UpdateSwitchServerRequest request;
    int httpCode;
    if (tryParseJsonString(requestBody, requestJson) && 
        UpdateSwitchServerRequest::tryParseJsonDocument(requestJson, request)) {
      logToSerial(DEBUG, "Request body parsing successful:");

      this->setState(request.expectedSwitchState);
    
      httpCode = HTTP_CODE_NO_CONTENT;
      logToSerial(INFO, "Sending response: HTTP_CODE=[%d], BODY=[]", httpCode);
      server.send(httpCode);

      return;
    }

    logToSerial(ERROR, "Request body parsing failed:");

    httpCode = HTTP_CODE_BAD_REQUEST;
    logToSerial(INFO, "Sending response: HTTP_CODE=[%d], BODY=[]", httpCode);
    server.send(httpCode);
  });

  logToSerial(INFO, "Endpoint setup successful: ENDPOINT=[%s]", endpoint.c_str());
}
