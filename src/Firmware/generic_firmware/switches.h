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
/// Switch representation used to initialize the digital pins.
/// </param>
void initializeSwitch(const Switch& switchRef);

/// <summary>
/// Changes the state of the provided switch according to its logic.
/// </summary>
/// <remarks>
/// This function only updates the values within the provided <paramref name="switchRef"/> instance. 
/// The physical state of the GPIO pin is not modified.
/// </remarks>
/// <param name="switchRef">
/// The switch representation to be modified.
/// </param>
/// <param name="isClosed">
/// <see langword="true"/> if the switch should be closed to allow current flow; otherwise, <see langword="false"/>.
/// </param>
void changeSwitchState(Switch& switchRef, const boolean isClosed);

/// <summary>
/// Populates the provided JSON document with switch registration data.
/// </summary>
/// <param name="document">
/// The JSON document to be populated with registration data.
/// </param>
/// <param name="macAddress">
/// Station MAC address.
/// </param>
/// <param name="localId">
/// The identifier of the switch, unique at the station level.
/// </param>
void populateSwitchRegistrationRequest(JsonDocument& document, const String macAddress, byte localId);

#endif