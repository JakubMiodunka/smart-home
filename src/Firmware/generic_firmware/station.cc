#include <Arduino.h>
#include <ArduinoJson.h>

#include "station.h"

void populateStationRegistrationRequest(JsonDocument& document, String macAddress) {
  document["macAddress"] = macAddress;
}