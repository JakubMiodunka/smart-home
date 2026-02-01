#ifndef STATION_H
#define STATION_H

#include <Arduino.h>
#include <ArduinoJson.h>

/// <summary>
/// Populates the provided JSON document with station registration data.
/// </summary>
/// <param name="document">
/// The JSON document to be populated with registration data.
/// </param>
/// <param name="macAddress">
/// Station MAC address.
/// </param>
void populateStationRegistrationRequest(JsonDocument& document, String macAddress);

#endif