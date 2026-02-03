#ifndef SWITCHES_H
#define SWITCHES_H

#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266WiFiMulti.h>

#include "requests.h"

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
/// Configures switcg GIOP pin as output.
/// </summary>
/// <param name="switchRef">
/// Switch representation used to initialize the digital pin.
/// </param>
void initializeSwitch(const Switch& switchRef);

/// <summary>
/// Deterines the logical state of provided switch
/// </summary>
/// <param name="switchRef">
/// The switch representation to be inspected.
/// </param>
/// <param name="desiredState">
/// logical state of provided switch - <see langword="true"/> if the switch is be closed and current is flowwing, <see langword="false"/> otherwise.
/// </param>

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
boolean getSwitchState(const Switch& switchRef);

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
/// <param name="desiredState">
/// <see langword="true"/> if the switch should be closed to allow current flow, <see langword="false"/> otherwise.
/// </param>
void setSwitchState(Switch& switchRef, const boolean desiredState);

/// <summary>
/// Populates the provided JSON document with switch registration data.
/// </summary>
/// <param name="request">
/// The JSON document to be populated with registration data.
/// </param>
/// <param name="macAddress">
/// Station MAC address.
/// </param>
/// <param name="localId">
/// The identifier of the switch, unique at the station level.
/// </param>
void populateSwitchRegistrationRequest(JsonDocument& request, const String macAddress, byte localId);

/// <summary>
/// Attempts to register a single switch on the server.
/// </summary>
/// <remarks>
/// This function changes state of provided <paramref name="switchRef"/> instance
/// according to received server response.
/// </remarks>
/// <param name="wiFiManager">
/// Reference to the WiFi manager responsible for maintaining the network connection.
/// </param>
/// <param name="baseUrl">
/// The base URL for the smart home API dedicated to handling firmware requests.
/// </param>
/// <param name="switchRef">
/// The switch object to be registered.
/// State of GPIO pin will be updated according to received server response.
/// </param>
/// <param name="macAddress">
/// Station MAC address.
/// </param>
/// <param name="localId">
/// The local identifier assigned to the switch.
/// </param>
/// <returns>
/// True if the attempt was successful, false otherwise.
/// </returns>
boolean tryRegisterSwitch(ESP8266WiFiMulti& wiFiManager, const String baseUrl, Switch& switchRef, const String macAddress, int localId);

/// <summary>
/// Attempts to update state of a single switch on the server.
/// </summary>
/// <param name="wiFiManager">
/// Reference to the WiFi manager responsible for maintaining the network connection.
/// </param>
/// <param name="baseUrl">
/// The base URL for the smart home API dedicated to handling firmware requests.
/// </param>
/// <param name="switchRef">
/// The switch object which state shall be updated.
/// </param>
/// <param name="macAddress">
/// Station MAC address.
/// </param>
/// <param name="localId">
/// The local identifier assigned to the switch.
/// </param>
/// <returns>
/// True if the attempt was successful, false otherwise.
/// </returns>
boolean tryUpdateSwitchState(ESP8266WiFiMulti& wiFiManager, const String baseUrl, const Switch& switchRef, const String macAddress, int localId);

#endif