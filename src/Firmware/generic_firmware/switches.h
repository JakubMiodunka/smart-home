#ifndef SWITCHES_H
#define SWITCHES_H

#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266WebServer.h>
#include <ESP8266WiFiMulti.h>

#include "requests.h"

/// <summary>
/// Representation of a single electrical switch.
/// </summary>
struct Switch {
  /// <summary>
  /// The identifier of the switch, unique at the station level.
  /// </summary>
  uint8_t localId;

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
/// Configures switch GPIO pin as output.
/// </summary>
/// <param name="switchRef">
/// Switch representation used to initialize the digital pin.
/// </param>
void initializeSwitch(const Switch& switchRef);

/// <summary>
/// Determines the logical state of the provided switch.
/// </summary>
/// <param name="switchRef">
/// The switch representation to be inspected.
/// </param>
/// <param name="desiredState">
/// Logical state of provided switch - <see langword="true"/> 
/// if the switch is closed and current is flowing, <see langword="false"/> otherwise.
/// </param>
bool getSwitchState(const Switch& switchRef);

/// <summary>
/// Changes the state of the provided switch according to its logic.
/// </summary>
/// <remarks>
/// This function updates values within the provided <paramref name="switchRef"/> instance. 
/// along the physical state of the switch GPIO pin.
/// </remarks>
/// <param name="switchRef">
/// The switch representation to be modified.
/// </param>
/// <param name="expectedState">
/// <see langword="true"/> if the switch should be closed to allow current flow, <see langword="false"/> otherwise.
/// </param>
void setSwitchState(Switch& switchRef, const bool expectedState);

/// <summary>
/// Attempts to register a single switch on the remote server.
/// </summary>
/// <remarks>
/// This function changes state of provided <paramref name="switchRef"/> instance
/// according to received server response.
/// </remarks>
/// <param name="wiFiManager">
/// Reference to the WiFi manager responsible for maintaining the network connection.
/// </param>
/// <param name="switchRef">
/// The switch object to be registered.
/// State of GPIO pin will be updated according to received server response.
/// </param>
/// <returns>
/// <see langword="true"/> if the attempt was successful, <see langword="false"/> otherwise.
/// </returns>
bool tryRegisterSwitch(ESP8266WiFiMulti& wiFiManager, Switch& switchRef);

/// <summary>
/// Attempts to update a single switch details on the remote server.
/// </summary>
/// <param name="wiFiManager">
/// Reference to the WiFi manager responsible for maintaining the network connection.
/// </param>
/// <param name="switchRef">
/// The switch object which state shall be updated.
/// </param>
/// <returns>
/// <see langword="true"/> if the attempt was successful, <see langword="false"/> otherwise.
/// </returns>
bool tryUpdateSwitch(ESP8266WiFiMulti& wiFiManager, const Switch& switchRef);

#endif