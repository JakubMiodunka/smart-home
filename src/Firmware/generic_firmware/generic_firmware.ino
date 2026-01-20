#include <Arduino.h>
#include <ESP8266WiFi.h>
#include <ESP8266WiFiMulti.h>
#include <ESP8266HTTPClient.h>
#include <WiFiClient.h>
#include <ArduinoJson.h>
#include <stdarg.h>

// Nice coding standard: https://users.ece.cmu.edu/~eno/coding/CCodingStandard.html

//--- LOGGING ---
enum LoggingLevel
{
  UNKNOWN,
  DEBUG,
  INFO,
  WARNING,
  ERROR,
  CRITICAL
};

void log_to_serial(LoggingLevel level, const char* format, ...)
{
  const char* level_names[] = {"UNKNOWN", "DEBUG", "INFO", "WARNING", "ERROR", "CRITICAL"};
  const char* level_name = (0  <= level && level <= 5) ? level_names[level] : level_names[0];

  char message_buffer[256];
  va_list args;
  va_start(args, format);
  vsnprintf(message_buffer, sizeof(message_buffer), format, args);
  va_end(args);

  Serial.printf("[%s] %s\n", level_name, message_buffer);
}

//--- JSON ---

String create_station_registration_request()
{
  JsonDocument document;
  document["macAddress"] = WiFi.macAddress();

  String serialized_document;
  serializeJson(document, serialized_document);
  return serialized_document;
}

///---

ESP8266WiFiMulti WiFiMulti;

void setup() {
  Serial.begin(115200);
  Serial.println();

  const char* SSID = "";        // Fill up accordingly.
  const char* PASSWORD = "";  // Fill up accordingly.

  WiFi.mode(WIFI_STA);
  WiFiMulti.addAP(SSID, PASSWORD);
}

void loop() {
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
}