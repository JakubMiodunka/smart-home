#ifndef STATION_H
#define STATION_H

#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266WiFiMulti.h>

/// <summary>
/// Populates the provided JSON document with station registration data.
/// </summary>
/// <param name="request">
/// The JSON document to be populated with registration data.
/// </param>
/// <param name="macAddress">
/// Station MAC address.
/// </param>
void populateStationRegistrationRequest(JsonDocument& request, String macAddress);

/// <summary>
/// Attempts to register the station on the server.
/// </summary>
/// <param name="wiFiManager">
/// Reference to the WiFi manager responsible for maintaining the network connection.
/// </param>
/// <param name="baseUrl">
/// The base URL for the smart home API dedicated to handling firmware requests.
/// </param>
/// <param name="macAddress">
/// The MAC address of the station.
/// </param>
/// <returns>
/// True if the attempt was successful, false otherwise.
/// </returns>
boolean tryRegisterStation(ESP8266WiFiMulti& wiFiManager, const String baseUrl, const String macAddress);

#endif