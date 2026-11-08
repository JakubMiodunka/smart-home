#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266WiFi.h>
#include <ESP8266WiFiMulti.h>
#include <ESP8266HTTPClient.h>
#include <ESP8266WebServer.h>
#include <Wire.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_BME280.h>
#include <math.h>

#include <Batteries.h>
#include <Config.h>
#include <Secrets.h>
#include <Sensors.h>
#include <SerialLogging.h>
#include <Station.h>
#include <Requests.h>

ESP8266WiFiMulti WiFiManager;
ESP8266WebServer LocalServer(LOCAL_SERVER_PORT);

Adafruit_BME280 Bme280;
constexpr uint8_t Bme280I2CAddress = 0x76;  // Usually eather 0x76 or 0x77.

constexpr uint8_t WifiConnectionIndicatorPin = 12;
constexpr uint8_t RegistrationIndicatorPin = 13;

constexpr uint8_t BatteryMeasureEnablePin = 14;
constexpr uint8_t BatteryMeasureAnalogPin = A0;

// Features definition:
Sensor Sensors[] = 
  {
    { 
      0,  // Global ID unknown until registration.
      1,
      MeasurementType::BatteryLevel,
      []() {
        logToSerial(DEBUG, "Enabling battery voltage measurement hardware: GPIO_PIN=[%d]", BatteryMeasureEnablePin);

        digitalWrite(BatteryMeasureEnablePin, LOW);
        delay(10); // Time for Q2 transistor to fully open before taking measurement.

        logToSerial(DEBUG, "Battery measurement hardware enabled successfully:");
      },
      []() {
        logToSerial(DEBUG, "Retrieving value from ADC: PIN=[%d]", BatteryMeasureAnalogPin);

        int adcValue = analogRead(BatteryMeasureAnalogPin);

        logToSerial(DEBUG, "Value from ADC retrieved successfully: ADC_VALUE=[%d]", adcValue);
        logToSerial(DEBUG, "Determining battery voltage:");

        // Convert raw ADC reading (0-1023) to voltage and compensate for the R1/R2 voltage divider ratio.
        float batteryVoltage = ((float)adcValue / 1023.0F) * 4.3F;

        /* 
         *Round to one decimal place to filter out minor voltage fluctuations, ensuring stable battery level reading.
         * Tried to also add averaging of the ADC reading to achieve similar effect, but readings were still not stable.
         * This approach offers reasonable accuracy and reading consistency.
         */
        batteryVoltage = roundf(batteryVoltage * 10.0F) / 10.0F;

        logToSerial(DEBUG, "Battery voltage determined successfully: BATT_VOLTAGE=[%.1f V]", batteryVoltage);
        logToSerial(DEBUG, "Determining battery level:");

        float batteryLevel = computeLithiumBatteryChargeLevel(batteryVoltage, 1);

        logToSerial(DEBUG, "Battery level determined successfully: BATT_LEVEL=[%.2f%%]", batteryLevel);

        return batteryLevel;
      },
      []() {
        logToSerial(DEBUG, "Disabling battery voltage measurement hardware: GPIO_PIN=[%d]", BatteryMeasureEnablePin);

        digitalWrite(BatteryMeasureEnablePin, HIGH);

        logToSerial(DEBUG, "Battery measurement hardware disabled successfully:");
      }
    },
    {
      0,
      2,
      MeasurementType::Temperature,
      []() { logToSerial(DEBUG, "No pre-measurement action required."); },
      []() {
          logToSerial(DEBUG, "Retrieving temperature value from BME280:");
          
          float temperatureValue = Bme280.readTemperature();  // Given in Celsius.

          logToSerial(DEBUG, "Temperature value retrieved successfully: VALUE=[%f °C]", temperatureValue);
          logToSerial(DEBUG, "Performing conversion from Celsius to Kelvin:");
          
          float temperatureKelvin = temperatureValue + 273.15F;

          logToSerial(DEBUG, "Conversion successful: VALUE=[%f K]", temperatureKelvin);

          return temperatureKelvin;
      },
      []() { logToSerial(DEBUG, "No post-measurement action required."); }
    },
    { 
      0,
      3,
      MeasurementType::Pressure,
      []() { logToSerial(DEBUG, "No pre-measurement action required:"); },
      []() {
          logToSerial(DEBUG, "Retrieving pressure value from BME280:");
          
          float pressureValue = Bme280.readPressure();  // Given in Pascals.

          logToSerial(DEBUG, "Pressure value retrieved successfully: VALUE=[%f Pa]", pressureValue);

          return pressureValue;
      },
      []() { logToSerial(DEBUG, "No post-measurement action required:"); }
    },
    { 
        0,
        4,
        MeasurementType::RelativeHumidity,
        []() { logToSerial(DEBUG, "No pre-measurement action required:"); },
        []() {
            logToSerial(DEBUG, "Retrieving humidity value from BME280:");
            
            float humidityValue = Bme280.readHumidity();  // Given in %.

            logToSerial(DEBUG, "Humidity value retrieved successfully: VALUE=[%f%%]", humidityValue);

            return humidityValue;
        },
        []() { logToSerial(DEBUG, "No post-measurement action required:"); }
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

  pinMode(WifiConnectionIndicatorPin, OUTPUT);
  digitalWrite(WifiConnectionIndicatorPin, LOW);

  pinMode(RegistrationIndicatorPin, OUTPUT);
  digitalWrite(RegistrationIndicatorPin, LOW);

  logToSerial(INFO, "All status indicators initialized successfully: STATE=[enabled]");
  logToSerial(INFO, "Attempting to initialize sensor: TYPE=[BATT_LEVEL]");

  pinMode(BatteryMeasureEnablePin, OUTPUT);
  digitalWrite(BatteryMeasureEnablePin, LOW);

  logToSerial(INFO, "Sensor initialized successfully: TYPE=[BATT_LEVEL]");
  logToSerial(INFO, "Attempting to initialize sensor: TYPE=[BME280], I2C_ADDRESS=[%d]", Bme280I2CAddress);

  while(!Bme280.begin(Bme280I2CAddress)) {
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
  logToSerial(DEBUG, "Changing state of WiFi connection indicator: STATE=[disabled]");

  digitalWrite(WifiConnectionIndicatorPin, HIGH);

  registerAllFeatures();

  logToSerial(DEBUG, "Changing state of registration indicator: STATE=[disabled]");

  digitalWrite(RegistrationIndicatorPin, HIGH);

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
      logToSerial(DEBUG, "Changing state of registration indicator: STATE=[enabled]");
      digitalWrite(RegistrationIndicatorPin, LOW);

      registerAllFeatures();

      logToSerial(DEBUG, "Changing state of registration indicator: STATE=[disabled]");
      digitalWrite(RegistrationIndicatorPin, HIGH);
    }
    LastHeartbeatTimestamp = currentTimestamp;
  }
}