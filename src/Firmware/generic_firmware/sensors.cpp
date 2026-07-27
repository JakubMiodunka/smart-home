#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266HTTPClient.h>
#include <ESP8266WiFiMulti.h>
#include <ESP8266WebServer.h>

#include "config.h"
#include "sensors.h"
#include "serial_logging.h"
#include "requests.h"

void SensorRegistrationStationRequest::toJsonDocument(JsonDocument& jsonDocument) const {
  jsonDocument["sensorLocalId"] = this->sensorLocalId;
  jsonDocument["measurementType"] = this->measurementType;
}

void GetMeasurementStationResponse::toJsonDocument(JsonDocument& jsonDocument) const {
  jsonDocument["measurementValue"] = this->measurementValue;
}

bool SensorRegistrationServerResponse::tryParseJsonDocument(const JsonDocument& jsonDocument, SensorRegistrationServerResponse& response) {
  logToSerial(DEBUG, "Attempting to parse JSON document:");

  static constexpr const char* SENSOR_ID_KEY = "sensorId";
  JsonVariantConst sensorIdVariant = jsonDocument[SENSOR_ID_KEY];

  if (sensorIdVariant.isNull()) {
    logToSerial(ERROR, "JSON key not found: JSON_KEY=[%s]", SENSOR_ID_KEY);
    return false;
  }
  
  if (!sensorIdVariant.is<uint32_t>()) {
    logToSerial(ERROR, "Type of JSON key invalid: JSON_KEY=[%s], EXPECTED_TYPE=[uint32_t]", SENSOR_ID_KEY);
    return false;
  }

  response.sensorId = sensorIdVariant.as<uint32_t>();

  logToSerial(DEBUG, "JSON document parsing successful:");
  return true;
}

bool Sensor::tryRegisterOnRemoteServer(ESP8266WiFiMulti& wiFiManager) {
  logToSerial(INFO, "Attempting to register sensor: LOCAL_ID=[%d]", this->localId);
  
  if (REMOTE_SERVER_API_VERSION != 1) {
    logToSerial(ERROR, "Not supported for specified remote API version: [API_VERSION=%u]", REMOTE_SERVER_API_VERSION);
    return false;
  }
  
  const String url = getRemoteBaseUrl() + "/sensors";
  const HttpMethod httpMethod = PUT;
  SensorRegistrationStationRequest request = { this->localId };
  JsonDocument requestJson;
  request.toJsonDocument(requestJson);
  JsonDocument responseJson;
  int httpReturnCode;
    
  sendHttpRequest(wiFiManager, url, httpMethod, requestJson, responseJson, httpReturnCode);
  bool wasOperationSuccessful = httpReturnCode == HTTP_CODE_OK;

  if (!wasOperationSuccessful) {
    logToSerial(WARNING, "Sensor registration failed: LOCAL_ID=[%d]", this->localId);
    return false;
  }

  SensorRegistrationServerResponse response;
  if (SensorRegistrationServerResponse::tryParseJsonDocument(responseJson, response)) {
    this->id = response.sensorId;

    logToSerial(INFO, "Sensor registration successful: LOCAL_ID=[%d]", this->localId);
    return true;
  }

  logToSerial(ERROR, "Remote server response parsing failed:");
  return false;
}

void Sensor::registerOnRemoteServer(ESP8266WiFiMulti& wiFiManager) {
  while (!this->tryRegisterOnRemoteServer(wiFiManager)) {
    logToSerial(WARNING, "Registration attempt failed. RETRY_INTERVAL=[%lu][ms]", REQUESTS_RETRY_INTERVAL);
    delay(REQUESTS_RETRY_INTERVAL);
  }
}

void Sensor::setupControlEndpoint(ESP8266WebServer& server) {
  String endpoint = getLocalEndpointPrefix() + "/sensors/" + String(this->localId);
  logToSerial(INFO, "Attempting to setup an endpoint: ENDPOINT=[%s]", endpoint.c_str());

  server.on(endpoint, HTTP_GET, [this, &server]() {
    logToSerial(INFO, "Request received: TYPE=[GetMeasurementServerRequest], BODY=[]");
    
    int httpCode;

    if (!isRequestAuthorized(server)) {
      httpCode = HTTP_CODE_UNAUTHORIZED;
      logToSerial(INFO, "Sending response: HTTP_CODE=[%d], BODY=[]", httpCode);
      server.send(httpCode);

      return;
    }

    double measurementValue = this->takeMeasurement();
    GetMeasurementStationResponse response = { measurementValue };
    JsonDocument repsonseJson;
    response.toJsonDocument(repsonseJson);

    String serializedResponse;
    serializeJson(repsonseJson, serializedResponse);

    httpCode = HTTP_CODE_OK;
    logToSerial(INFO, "Sending response: HTTP_CODE=[%d], BODY=[%s]", httpCode, serializedResponse.c_str());
    server.send(httpCode, "application/json", serializedResponse);
  });

  logToSerial(INFO, "Endpoint setup successful: ENDPOINT=[%s]", endpoint.c_str());
}