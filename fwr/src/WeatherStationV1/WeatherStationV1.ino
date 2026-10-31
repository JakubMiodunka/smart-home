#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266WiFi.h>
#include <ESP8266WiFiMulti.h>
#include <ESP8266HTTPClient.h>
#include <ESP8266WebServer.h>
#include <Wire.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_BME280.h>

#include <Config.h>
#include <Secrets.h>
#include <Sensors.h>
#include <SerialLogging.h>
#include <Station.h>
#include <Requests.h>

// Connectivity:
ESP8266WiFiMulti WiFiManager;
ESP8266WebServer LocalServer(LOCAL_SERVER_PORT);

// Peripherals definition:
Adafruit_BME280 Bme280;
constexpr uint8_t Bme280I2CAddress = 0x76;  // Usually eather 0x76 or 0x77.

constexpr uint8_t WifiConnectionIndicatorPinNumber = 12;
constexpr uint8_t RegistrationIndicatorPinNumber = 13;

// Features definition:
Sensor Sensors[] = 
  { 
    {
      0,  // Global ID unknown until registration.
      1,
      MeasurementType::Temperature,
      []() {},  // No pre-measurement action required.
      []() { return (double)(Bme280.readTemperature() + 273.15F); },  // Conversion from Celsius to kelvin
      []() {}  // No post-measurement action required.
    },
    { 
      0,  // Global ID unknown until registration.
      2,
      MeasurementType::Pressure,
      []() {},  // No pre-measurement action required.
      []() { return (double)Bme280.readPressure(); },
      []() {}  // No post-measurement action required.
    },
    { 
      0,  // Global ID unknown until registration.
      3,
      MeasurementType::RelativeHumidity,
      []() {},  // No pre-measurement action required.
      []() { return (double)Bme280.readHumidity(); },
      []() {}  // No post-measurement action required.
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
  digitalWrite(WifiConnectionIndicatorPinNumber, HIGH);

  pinMode(RegistrationIndicatorPinNumber, OUTPUT);
  digitalWrite(RegistrationIndicatorPinNumber, HIGH);

  logToSerial(INFO, "All status indicators initialized successfully: STATE=[disabled]");
  logToSerial(INFO, "Attempting to initialize sensor: TYPE=[BME280]");

  while(!Bme280.begin()) {
    logToSerial(ERROR, "Failed to initialize sensor: TYPE=[BME280]");
    delay(1000);
    logToSerial(INFO, "Re-attempting to initialize sensor: TYPE=[BME280]");
  }

  logToSerial(INFO, "Sensor initialized successfully: TYPE=[BME280]");
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

  digitalWrite(WifiConnectionIndicatorPinNumber, LOW);

  registerAllFeatures();

  logToSerial(DEBUG, "Changing state of registration indicator: STATE=[enabled]");

  digitalWrite(RegistrationIndicatorPinNumber, LOW);

  logToSerial(INFO, "Initializing local server API: PORT=[%d]", LOCAL_SERVER_PORT);

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
      digitalWrite(RegistrationIndicatorPinNumber, HIGH);

      registerAllFeatures();

      logToSerial(DEBUG, "Changing state of registration indicator: STATE=[enabled]");
      digitalWrite(RegistrationIndicatorPinNumber, LOW);
    }
    LastHeartbeatTimestamp = currentTimestamp;
  }
}