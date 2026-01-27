#include <Arduino.h>
#include <ESP8266WiFi.h>
#include <ESP8266WiFiMulti.h>
#include <ESP8266HTTPClient.h>
#include <WiFiClient.h>
#include <ArduinoJson.h>

#include "serial_logging.h"
#include "station.h"
#include "switches.h"

ESP8266WiFiMulti WiFiMulti;
String g_macAddress;
Switch g_switches[] = { {LED_BUILTIN, LOW} };
constexpr size_t NUMBER_OF_SWITCHES = sizeof(g_switches)/sizeof(Switch);
constexpr unsigned long RETRY_INTERVAL = 10000;
constexpr const char* SERVER_IP_ADDRESS = "192.168.0.199";

/// <summary>
/// Serializes the provided JSON document into a minified string representation.
/// </summary>
/// <param name="document">
/// A reference to the JSON document to be serialized.
/// </param>
/// <returns>
/// A String containing the serialized JSON data.
/// </returns>
String serializeToString(const JsonDocument& document)
{
  String serialized_document;
  serializeJson(document, serialized_document);
  return serialized_document;
}

/// <summary>
/// Attempts to register the station on the server.
/// </summary>
/// <param name="macAddress">
/// The MAC address of the station.
/// </param>
/// <returns>
/// True if the attempt was successful, false otherwise.
/// </returns>
boolean tryRegisterStation(String macAddress) {
  if (WiFiMulti.run() != WL_CONNECTED) {
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

/// <summary>
/// Attempts to register a single switch on the server.
/// </summary>
/// <param name="switchToRegister">
/// The switch object to be registered.
/// </param>
/// <param name="localId">
/// The local identifier assigned to the switch.
/// </param>
/// <returns>
/// True if the attempt was successful, false otherwise.
/// </returns>
boolean tryRegisterSwitch(const Switch& switchToRegister, int localId) {
  if (WiFiMulti.run() != WL_CONNECTED) {
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

void setup() {
  const char* SSID = "";      // Fill up accordingly.
  const char* PASSWORD = "";  // Fill up accordingly.
  
  Serial.begin(115200);
  Serial.println();

  logToSerial(INFO, "Attempting to connect to %s WiFi network.", SSID);

  WiFi.mode(WIFI_STA);
  WiFiMulti.addAP(SSID, PASSWORD);
  g_macAddress = WiFi.macAddress();

  while (WiFiMulti.run() != WL_CONNECTED) {
    delay(500);
  }

  logToSerial(INFO, "Connection established successfully.");

  logToSerial(DEBUG, "IP addressassigned to station: %s", WiFi.localIP().toString().c_str());
  logToSerial(DEBUG, "Network signal strength: %d dBm", WiFi.RSSI());

  logToSerial(INFO, "Attempting to register station on the server.");
  logToSerial(DEBUG, "Server IP address: %s", SERVER_IP_ADDRESS);
  logToSerial(DEBUG, "MAC address used during registration process: %s", g_macAddress.c_str());

  while (!tryRegisterStation(g_macAddress)) {
    logToSerial(WARNING, "Registration attempt failed. Retrying in %lu ms.", RETRY_INTERVAL);
    delay(RETRY_INTERVAL);
  }

  logToSerial(INFO, "Station registration successful.");

  logToSerial(INFO, "Attempting to register switches on the server.");

  // TODO: Local ID of a switch shall be greather than 0.
  for (int index = 0; index < NUMBER_OF_SWITCHES; index++) {
    const Switch& currentSwitch = g_switches[index];

    logToSerial(INFO, "Attempting to register switch with ID %d.", index + 1);

    while (!tryRegisterSwitch(currentSwitch, index + 1)) {
      logToSerial(WARNING, "Registration attempt failed. Retrying in %lu ms.", RETRY_INTERVAL);
      delay(RETRY_INTERVAL);
    }

    logToSerial(INFO, "Switch registration successful.");
  }

  logToSerial(INFO, "All switches registered successfully.");

/*
  TODO: Initialize switches according to DTOs returned by server.
  for (int index = 0; index < NUMBER_OF_SWITCHES; index++) {
    const Switch& currentSwitch = g_switches[index];
    initializeSwitch(currentSwitch);
  }
*/
}


void loop() {

}