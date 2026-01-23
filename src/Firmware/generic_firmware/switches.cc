#include <Arduino.h>
#include <ArduinoJson.h>

#include "switches.h"

void initializeSwitches(Switch* switches, int numberOfSwitches) {
  for (int index = 0; index < numberOfSwitches; index++) {
    const Switch& currentSwitch = switches[index];
    pinMode(currentSwitch.pinNumber, OUTPUT);
    digitalWrite(currentSwitch.pinNumber, currentSwitch.pinState);
  }
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