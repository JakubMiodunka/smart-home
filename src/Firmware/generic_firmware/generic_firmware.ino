#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266WiFi.h>
#include <ESP8266WiFiMulti.h>
#include <ESP8266HTTPClient.h>
#include <ESP8266WebServer.h>
#include <WiFiClient.h> // TODO: Not sure if required here.

#include "config.h"
#include "secrets.h"
#include "serial_logging.h"
#include "station.h"
#include "switches.h"
#include "requests.h"

// Connectivity:
ESP8266WiFiMulti WiFiManager;
ESP8266WebServer LocalServer(LOCAL_SERVER_PORT);

// Peripherals definition:
Station ThisStation = {};
Switch Switches[] = { {1, LED_BUILTIN, HIGH, true} };

// Timekeeping:
uint32_t LastHeartbeatTimestamp = 0;    // Given in milliseconds.
uint32_t LastLocalApiPollTimestamp = 0; // Given in milliseconds.

// TODO: Add doc-string.
void registerAll() {
  String macAddress = WiFi.macAddress();
  macAddress.replace(":", "");

  ThisStation.registerOnRemoteServer(WiFiManager, macAddress);

  logToSerial(INFO, "Attempting to register all switches on the remote server.");

  for (Switch& currentSwitch : Switches) {
    currentSwitch.registerOnRemoteServer(WiFiManager);
  }

  logToSerial(INFO, "All switches registered successfully.");
  logToSerial(INFO, "Attempting to update state of all switches on the remote server.");

  for (const Switch& currentSwitch : Switches) {
    currentSwitch.updateOnRemoteServer(WiFiManager);
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
    currentSwitch.initialize();
  }

  logToSerial(INFO, "All switches initialized successfully.");
  logToSerial(INFO, "Attempting to connect to WiFi network: WIFI_SSID=[%s]", WIFI_SSID);

  WiFi.mode(WIFI_STA);
  WiFiManager.addAP(WIFI_SSID, WIFI_PASSWORD);
  
  while (WiFiManager.run() != WL_CONNECTED) {
    delay(1000);
  }

  logToSerial(INFO, "Connection established successfully:");
  logToSerial(DEBUG, "DHCP server assigned IP address to station: IP_ADDRESS=[%s]", WiFi.localIP().toString().c_str());
  logToSerial(DEBUG, "WiFi signal strength measured: SIGNAL_STRENGTH=[%d][dBm]", WiFi.RSSI());

  registerAll();

  logToSerial(INFO, "Initializing local server API: PORT=[%d]", LOCAL_SERVER_PORT);

  for (Switch& currentSwitch : Switches) {
    currentSwitch.setupControlEndpoint(LocalServer);
  }

  LocalServer.begin(LOCAL_SERVER_PORT);
  
  logToSerial(INFO, "Local server API initialized successfully.");
}

void loop() {
  uint32_t currentTimestamp = millis();

  if (currentTimestamp - LastLocalApiPollTimestamp >= LOCAL_SERVER_API_POLL_INTERVAL) {
    LocalServer.handleClient();
    LastLocalApiPollTimestamp = currentTimestamp;
  }

  if (currentTimestamp - LastHeartbeatTimestamp >= HEARTBEAT_INTERVAL) {
    if (!ThisStation.trySendHeartbeatSignal(WiFiManager)) {
      registerAll();
    }
    LastHeartbeatTimestamp = currentTimestamp;
  }
}