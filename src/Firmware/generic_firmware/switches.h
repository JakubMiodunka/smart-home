#ifndef SWITCHES_H
#define SWITCHES_H

#include <Arduino.h>
#include <ArduinoJson.h>

/// <summary>
/// Representation of a single electrical switch.
/// </summary>
struct Switch {
  uint8_t pinNumber;
  uint8_t pinState;  // According to Arduino.h, either LOW (0x00) or HIGH (0x01).
};

/// <summary>
/// Configures digital pins according to the provided collection of switch representations.
/// </summary>
/// <param name="switches">
/// The collection of switch representations used to initialize the digital pins.
/// </param>
/// <param name="numberOfSwitches">
/// The total number of switches contained in the provided collection.
/// </param>
void initializeSwitches(Switch* switches, int numberOfSwitches);

/// <summary>
/// Populates the provided JSON document with switch registration data.
/// </summary>
/// <param name="document">
/// The JSON document to be populated with registration data.
/// </param>
/// <param name="localId">
/// The local identifier of the switch.
/// </param>
/// <param name="pinState">
/// The current hardware state of the switch pin.
/// </param>
void populateSwitchRegistrationRequest(JsonDocument& document, size_t localId, uint8_t pinState);

#endif