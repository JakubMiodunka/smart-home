#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266WiFi.h>
#include <ESP8266WiFiMulti.h>
#include <ESP8266HTTPClient.h>
#include <WiFiClient.h> // TODO: Not sure if required here.

#include "config.h"
#include "secrets.h"
#include "serial_logging.h"
#include "station.h"
#include "switches.h"
#include "requests.h"

// Constraints:
Switch Switches[] = { {1, LED_BUILTIN, HIGH, true} };
constexpr size_t NUMBER_OF_SWITCHES = sizeof(Switches)/sizeof(Switch);

// Global variables:
ESP8266WiFiMulti WiFiManager;
uint32_t LastHeartbeatTimestamp = 0;  // Given in milliseconds.

// TODO: Add doc-string.
void registerAll() {
  logToSerial(INFO, "Attempting to register station on the server.");

  String macAddress = WiFi.macAddress();
  macAddress.replace(":", "");

  while (!tryRegisterStation(WiFiManager, macAddress)) {
    logToSerial(WARNING, "Registration attempt failed: REQUESTS_RETRY_INTERVAL=[%lu][ms]", REQUESTS_RETRY_INTERVAL);
    delay(REQUESTS_RETRY_INTERVAL);
  }

  logToSerial(INFO, "Station registration successful.");
  logToSerial(INFO, "Attempting to register all switches on the server.");

  for (Switch& currentSwitch : Switches) {
    while (!tryRegisterSwitch(WiFiManager, currentSwitch)) {
      logToSerial(WARNING, "Registration attempt failed. REQUESTS_RETRY_INTERVAL=[%lu][ms]", REQUESTS_RETRY_INTERVAL);
      delay(REQUESTS_RETRY_INTERVAL);
    }
  }

  logToSerial(INFO, "All switches registered successfully.");
  logToSerial(INFO, "Attempting to updatestate of all switches on the server.");

  for (const Switch& currentSwitch : Switches) {
    while (!tryUpdateSwitch(WiFiManager, currentSwitch)) {
      logToSerial(WARNING, "Update attempt failed. REQUESTS_RETRY_INTERVAL=[%lu][ms]", REQUESTS_RETRY_INTERVAL);
      delay(REQUESTS_RETRY_INTERVAL);
    }
  }

  logToSerial(INFO, "State of all switches updated successfully.");
}

void setup() {
  logToSerial(INFO, "Attempting to initialize serial port: BAUD_RATE=[%ul]", SERIAL_PORT_BAUD_RATE);

  Serial.begin(SERIAL_PORT_BAUD_RATE);
  Serial.println();

  logToSerial(INFO, "Serial port to initialization successful:");
  logToSerial(INFO, "Attempting to initialize all switches: COUNT=[%d]", sizeof(Switches)/sizeof(Switch));

  for (const Switch& currentSwitch : Switches) {
    initializeSwitch(currentSwitch);
  }

  logToSerial(INFO, "All switches initialized successfully.");
  logToSerial(INFO, "Attempting to connect to WiFi network: WIFI_SSID=[%s]", WIFI_SSID);

  WiFi.mode(WIFI_STA);
  WiFiManager.addAP(WIFI_SSID, WIFI_PASSWORD);
  
  while (WiFiManager.run() != WL_CONNECTED) {
    delay(500);
  }

  logToSerial(INFO, "Connection established successfully:");
  logToSerial(DEBUG, "DHCP server assigned IP address to station: IP_ADDRESS=[%s]", WiFi.localIP().toString().c_str());
  logToSerial(DEBUG, "Measuring WiFi signal strength: SIGNAL_STRENGTH=[%d][dBm]", WiFi.RSSI());

  registerAll();
}

void loop() {
  uint32_t currentTimestamp = millis();

  if (currentTimestamp - LastHeartbeatTimestamp >= HEARTBEAT_INTERVAL) {
    if (!trySendHeartbeatSignal(WiFiManager)) {
      registerAll();
    }
    LastHeartbeatTimestamp = currentTimestamp;
  }
}