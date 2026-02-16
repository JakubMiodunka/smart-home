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

ESP8266WiFiMulti WiFiManager;

Switch Switches[] = { {LED_BUILTIN, HIGH, true} };
constexpr size_t NUMBER_OF_SWITCHES = sizeof(Switches)/sizeof(Switch);

void setup() {
  logToSerial(INFO, "Attempting to initialize serial port: BAUD_RATE=[%ul]", SERIAL_PORT_BAUD_RATE);

  Serial.begin(SERIAL_PORT_BAUD_RATE);
  Serial.println();

  logToSerial(INFO, "Serial port to initialization successful:");

  logToSerial(INFO, "Attempting to initialize all switches.");

  for (int index = 0; index < NUMBER_OF_SWITCHES; index++) {
    const Switch& currentSwitch = Switches[index];
    const int localId = index + 1;
    logToSerial(INFO, "Attempting to initialize switch: LOCAL_ID=[%d]", localId);

    initializeSwitch(currentSwitch);

    logToSerial(INFO, "Switch initialization successful: LOCAL_ID=[%d].");
  }

  logToSerial(INFO, "All switches initialized successfully.");

  logToSerial(INFO, "Attempting to connect to WiFi network: WIFI_SSID=[%s]", WIFI_SSID);

  WiFi.mode(WIFI_STA);
  WiFiManager.addAP(WIFI_SSID, WIFI_PASSWORD);
  
  while (WiFiManager.run() != WL_CONNECTED) {
    delay(500);
  }

  logToSerial(INFO, "Connection established successfully.");

  String ipAddress = WiFi.localIP().toString();
  logToSerial(DEBUG, "DHCP server assigned IP address to station: IP_ADDRESS=[%s]", ipAddress.c_str());
  logToSerial(DEBUG, "Measuring WiFi signal strength: SIGNAL_STRENGTH=[%d][dBm]", WiFi.RSSI());

  logToSerial(INFO, "Attempting to register station on the server.");

  String macAddress = WiFi.macAddress();
  macAddress.replace(":", "");

  while (!tryRegisterStation(WiFiManager, macAddress)) {
    logToSerial(WARNING, "Registration attempt failed: REQUESTS_RETRY_INTERVAL=[%lu][ms]", REQUESTS_RETRY_INTERVAL);
    delay(REQUESTS_RETRY_INTERVAL);
  }

  logToSerial(INFO, "Station registration successful.");

  logToSerial(INFO, "Attempting to register all switches on the server.");

  for (int index = 0; index < NUMBER_OF_SWITCHES; index++) {
    Switch& currentSwitch = Switches[index];
    const int localId = index + 1;

    logToSerial(INFO, "Attempting to register switch: LOCAL_ID=[%d]", localId);

    while (!tryRegisterSwitch(WiFiManager, currentSwitch, localId)) {
      logToSerial(WARNING, "Registration attempt failed. REQUESTS_RETRY_INTERVAL=[%lu][ms]", REQUESTS_RETRY_INTERVAL);
      delay(REQUESTS_RETRY_INTERVAL);
    }

    logToSerial(INFO, "Switch registration successful: LOCAL_ID=[%d]", localId);
  }

  logToSerial(INFO, "All switches registered successfully.");

    logToSerial(INFO, "Attempting to updatestate of all switches on the server.");

  for (int index = 0; index < NUMBER_OF_SWITCHES; index++) {
    Switch& currentSwitch = Switches[index];
    const int localId = index + 1;

    logToSerial(INFO, "Attempting to update switch state: LOCAL_ID=[%d]", localId);

    while (!tryUpdateSwitchState(WiFiManager, currentSwitch, localId)) {
      logToSerial(WARNING, "Update attempt failed. REQUESTS_RETRY_INTERVAL=[%lu][ms]", REQUESTS_RETRY_INTERVAL);
      delay(REQUESTS_RETRY_INTERVAL);
    }

    logToSerial(INFO, "Switch state updated successfully: LOCAL_ID=[%d]", localId);
  }

  logToSerial(INFO, "State of all switches updated successfully.");
}

void loop() {

}