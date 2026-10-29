#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266WiFi.h>
#include <ESP8266WiFiMulti.h>
#include <ESP8266HTTPClient.h>
#include <ESP8266WebServer.h>

#include <Config.h>
#include <Secrets.h>
#include <Sensors.h>
#include <SerialLogging.h>
#include <Station.h>
#include <Switches.h>
#include <Requests.h>

// Connectivity:
ESP8266WiFiMulti WiFiManager;
ESP8266WebServer LocalServer(LOCAL_SERVER_PORT);

// Features definition:
Switch Switches[] = 
  { 
    {
      0,  // Global ID unknown until registration.
      1,
      LED_BUILTIN,
      HIGH, 
      true
    }
  };

// Timekeeping:
uint32_t LastHeartbeatTimestamp = 0;    // Given in milliseconds.
uint32_t LastLocalApiPollTimestamp = 0; // Given in milliseconds.

void registerAllFeatures()
{
  logToSerial(INFO, "Attempting to register all station features.");

  String macAddress = WiFi.macAddress();
  macAddress.replace(":", "");

  registerStationOnRemoteServer(WiFiManager, macAddress);

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
  logToSerial(INFO, "All station features registered successfully.");
}

void setup() {
  Serial.begin(SERIAL_PORT_BAUD_RATE);
  Serial.println();

  logToSerial(INFO, "Serial port initialization successful: BAUD_RATE=[%ul]", SERIAL_PORT_BAUD_RATE);
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
  logToSerial(DEBUG, "IP address assigned to station: IP_ADDRESS=[%s]", WiFi.localIP().toString().c_str());
  logToSerial(DEBUG, "WiFi signal strength measured: SIGNAL_STRENGTH=[%d][dBm]", WiFi.RSSI());

  registerAllFeatures();

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
    if (!trySendHeartbeatSignal(WiFiManager)) {
      registerAllFeatures();
    }
    
    LastHeartbeatTimestamp = currentTimestamp;
  }
}