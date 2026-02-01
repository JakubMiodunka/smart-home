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

void populateSwitchRegistrationRequest(JsonDocument& document, const String macAddress, byte localId) {
  document["stationMacAddress"] = macAddress;
  document["switchLocalId"] = localId;
}