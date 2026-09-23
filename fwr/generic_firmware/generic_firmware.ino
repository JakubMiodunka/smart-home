#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266WiFi.h>
#include <ESP8266WiFiMulti.h>
#include <ESP8266HTTPClient.h>
#include <ESP8266WebServer.h>

#include "config.h"
#include "secrets.h"
#include "sensors.h"
#include "serial_logging.h"
#include "station.h"
#include "switches.h"
#include "requests.h"

// Connectivity:
ESP8266WiFiMulti WiFiManager;
ESP8266WebServer LocalServer(LOCAL_SERVER_PORT);

// Peripherals definition:
constexpr uint8_t WifiConnectionIndicatorPinNumber = 12;
constexpr uint8_t RegistrationIndicatorPinNumber = 13;

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

Sensor Sensors[] = 
  { 
    { 
      0,  // Global ID unknown until registration.
      1,
      MeasurementType::Temperature,
      []() {},  // No initializaion required.
      []() { return 21.37; }  // Returning fake measurement.
    } 
  };

// Timekeeping:
uint32_t LastHeartbeatTimestamp = 0;    // Given in milliseconds.
uint32_t LastLocalApiPollTimestamp = 0; // Given in milliseconds.

void registerAllFeatures()
{
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
  logToSerial(INFO, "Attempting to register all sensors on the remote server.");

  for (Sensor& currentSensor : Sensors) {
    currentSensor.registerOnRemoteServer(WiFiManager);
  }

  logToSerial(INFO, "All sensors registered successfully.");
}

void setup() {
  Serial.begin(SERIAL_PORT_BAUD_RATE);
  Serial.println();

  logToSerial(INFO, "Serial port initialization successful: BAUD_RATE=[%ul]", SERIAL_PORT_BAUD_RATE);
  logToSerial(INFO, "Attempting to initialize status indicators:");

  pinMode(WifiConnectionIndicatorPinNumber, OUTPUT);
  digitalWrite(WifiConnectionIndicatorPinNumber, LOW);

  pinMode(RegistrationIndicatorPinNumber, OUTPUT);
  digitalWrite(RegistrationIndicatorPinNumber, LOW);

  logToSerial(INFO, "All status indicators initialized successfully: STATE=[disabled]");
  logToSerial(INFO, "Attempting to initialize all switches: COUNT=[%d]", sizeof(Switches)/sizeof(Switch));

  for (const Switch& currentSwitch : Switches) {
    currentSwitch.initialize();
  }

  logToSerial(INFO, "All switches initialized successfully.");
  logToSerial(INFO, "Attempting to initialize all sensors: COUNT=[%d]", sizeof(Sensors)/sizeof(Sensor));

  for (const Sensor& currentSensor : Sensors) {
    currentSensor.initialize();
  }

  logToSerial(INFO, "All sensors initialized successfully.");
  logToSerial(INFO, "Attempting to connect to WiFi network: WIFI_SSID=[%s]", WIFI_SSID);

  WiFi.mode(WIFI_STA);
  WiFiManager.addAP(WIFI_SSID, WIFI_PASSWORD);
  
  while (WiFiManager.run() != WL_CONNECTED) {
    delay(1000);
  }

  logToSerial(INFO, "Connection established successfully:");
  logToSerial(DEBUG, "IP address assigned to station: IP_ADDRESS=[%s]", WiFi.localIP().toString().c_str());
  logToSerial(DEBUG, "WiFi signal strength measured: SIGNAL_STRENGTH=[%d][dBm]", WiFi.RSSI());
  logToSerial(DEBUG, "Changing state of WiFi connection indicator: STATE=[enabled]");

  digitalWrite(WifiConnectionIndicatorPinNumber, HIGH);

  registerAllFeatures();

  logToSerial(DEBUG, "Changing state of registration indicator: STATE=[enabled]");

  digitalWrite(RegistrationIndicatorPinNumber, HIGH);

  logToSerial(INFO, "Initializing local server API: PORT=[%d]", LOCAL_SERVER_PORT);

  for (Switch& currentSwitch : Switches) {
    currentSwitch.setupControlEndpoint(LocalServer);
  }

  for (Sensor& currentSensor : Sensors) {
    currentSensor.setupControlEndpoint(LocalServer);
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
      logToSerial(DEBUG, "Changing state of registration indicator: STATE=[disabled]");
      digitalWrite(RegistrationIndicatorPinNumber, LOW);

      registerAllFeatures();

      logToSerial(DEBUG, "Changing state of registration indicator: STATE=[enabled]");
      digitalWrite(RegistrationIndicatorPinNumber, HIGH);
    }
    LastHeartbeatTimestamp = currentTimestamp;
  }
}