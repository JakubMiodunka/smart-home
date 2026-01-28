#ifndef SWITCHES_H
#define SWITCHES_H

#include <Arduino.h>
#include <ArduinoJson.h>

#include "switches.h"

/// <summary>
/// Representation of a single electrical switch.
/// </summary>
struct Switch {
  /// <summary>
  /// GPIO pin number the switch is connected to.
  /// </summary>
  uint8_t pinNumber;
  
  /// <summary>
  /// Current physical state of the pin - according to Arduino.h,
  /// either LOW (0x00) or HIGH (0x01).
  /// </summary>
  uint8_t pinState;

  /// <summary>
  /// Switch logic indicator.
  /// True if LOW GPIO pin state closes the switch and current is flowing,
  /// false otherwise.
  /// </summary>
  boolean reversedLogic;
};

/// <summary>
/// Configures digital pins according to the provided switch representations.
/// </summary>
/// <param name="switchRef">
/// Switch representations used to initialize the digital pins.
/// </param>
void initializeSwitch(const Switch& switchRef);

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