#include <Arduino.h>
#include <ESP8266WiFi.h>
#include <ESP8266WiFiMulti.h>
#include <ESP8266HTTPClient.h>
#include <WiFiClient.h>
#include <ArduinoJson.h>

#include "serial_logging.h"
#include "station.h"
#include "switches.h"

String mac_address;
Switch switches[] = { {LED_BUILTIN, LOW} };
constexpr size_t NUMBER_OF_SWITCHES = sizeof(switches)/sizeof(Switch);

ESP8266WiFiMulti WiFiMulti;

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


void setup() {
  const char* SSID = "";      // Fill up accordingly.
  const char* PASSWORD = "";  // Fill up accordingly.
  
  Serial.begin(115200);
  Serial.println();

  WiFi.mode(WIFI_STA);
  WiFiMulti.addAP(SSID, PASSWORD);
  mac_address = WiFi.macAddress();

  initializeSwitches(switches, NUMBER_OF_SWITCHES);
}

void loop() {
/*
  if (WiFiMulti.run() == WL_CONNECTED) {
    WiFiClient client;
    HTTPClient http;

    // Adres Twojego serwera ASP.NET Core
    if (http.begin(client, "http://192.168.0.199:5236/api/v1/stations")) {
      http.addHeader("Content-Type", "application/json");

      String registration_request = create_station_registration_request();
      int httpCode = http.POST(registration_request);

      if (httpCode > 0) {
        log_to_serial(INFO, "Returned HTTP code: %d", httpCode);
        if (httpCode == HTTP_CODE_OK || httpCode == HTTP_CODE_CREATED) {
          String payload = http.getString();
          log_to_serial(INFO, "Odpowiedz serwera:");
          Serial.println(payload);

          JsonDocument ret;
          deserializeJson(ret, payload);
          log_to_serial(INFO, "Otrzymane ID: %ld", ret["id"].as<long>());
        }
      } else {
        Serial.printf("[HTTP] Blad: %s\n", http.errorToString(httpCode).c_str());
      }

      http.end(); // Zawsze zamykaj polaczenie
    }
  }

  delay(10000); // Wysylaj co 10 sekund
  */
}