#ifndef REQUESTS_H
#define REQUESTS_H

#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266WiFiMulti.h>

#include "switches.h"

/// <summary>
/// Attempts to register the station on the server.
/// </summary>
/// <param name="wiFiManager">
/// Reference to the WiFi manager responsible for maintaining the network connection.
/// </param>
/// <param name="macAddress">
/// The MAC address of the station.
/// </param>
/// <returns>
/// True if the attempt was successful, false otherwise.
/// </returns>
boolean tryRegisterStation(ESP8266WiFiMulti& wiFiManager, String macAddress);

/// <summary>
/// Attempts to register a single switch on the server.
/// </summary>
/// <param name="wiFiManager">
/// Reference to the WiFi manager responsible for maintaining the network connection.
/// </param>
/// <param name="switchToRegister">
/// The switch object to be registered.
/// State of GPIO pin will be updated according to received server response.
/// </param>
/// <param name="localId">
/// The local identifier assigned to the switch.
/// </param>
/// <returns>
/// True if the attempt was successful, false otherwise.
/// </returns>
boolean tryRegisterSwitch(ESP8266WiFiMulti& wiFiManager, Switch& switchToRegister, int localId);

#endif