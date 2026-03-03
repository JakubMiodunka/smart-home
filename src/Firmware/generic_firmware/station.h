#ifndef STATION_H
#define STATION_H

#include <Arduino.h>
#include <ESP8266WiFiMulti.h>

/// <summary>
/// Attempts to register the station on the remote server.
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
bool tryRegisterStation(ESP8266WiFiMulti& wiFiManager, const String macAddress);

/// <summary>
/// Attempts to send heartbaet signal to the remote server.
/// </summary>
/// <param name="wiFiManager">
/// Reference to the WiFi manager responsible for maintaining the network connection.
/// </param>
/// <returns>
/// True if the attempt was successful, false otherwise.
/// </returns>
bool trySendHeartbeatSignal(ESP8266WiFiMulti& wiFiManager);

#endif