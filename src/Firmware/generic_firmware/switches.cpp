#include <Arduino.h>
#include <ArduinoJson.h>

#include "switches.h"

void initializeSwitch(const Switch& switchRef) {
  pinMode(switchRef.pinNumber, OUTPUT);
  digitalWrite(switchRef.pinNumber, switchRef.pinState);
}

void changeSwitchState(Switch& switchRef, const boolean isClosed) {
  bool pinState = isClosed;
  pinState = switchRef.reversedLogic ? !pinState : pinState;
  switchRef.pinState = pinState ? HIGH : LOW;
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