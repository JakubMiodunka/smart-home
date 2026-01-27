#include <Arduino.h>
#include <ArduinoJson.h>

#include "switches.h"
#include "serial_logging.h"

void initializeSwitch(const Switch& switchRef) {
  pinMode(switchRef.pinNumber, OUTPUT);
  digitalWrite(switchRef.pinNumber, switchRef.pinState);
}

void populateSwitchRegistrationRequest(JsonDocument& document, size_t localId, uint8_t pinState) {
  document["localId"] = localId;

  switch (pinState) {
    case HIGH:
      document["isClosed"] = true;
      break;
    case LOW:
      document["isClosed"] = false;
      break;
    default:
      document["isClosed"] = nullptr;
      break;
  }
}