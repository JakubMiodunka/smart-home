#ifndef STATION_H
#define STATION_H

#include <Arduino.h>
#include <ESP8266WiFiMulti.h>

/// <summary>
/// Data transfer object (DTO) representing the request for station registration on the remote server.
/// </summary>
struct StationRegistrationStationRequest {
  /// <summary>
  /// Station MAC address.
  /// </summary>
  String stationMacAddress;
  
  /// <summary>
  /// Populates the provided JSON document with station registration data.
  /// </summary>
  /// <param name="jsonDocument">
  /// The JSON document to be populated.
  /// </param>
  void toJsonDocument(JsonDocument& jsonDocument) const;
};

struct Station {
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
  /// <see langword="true"/> if the attempt was successful, <see langword="false"/> otherwise.
  /// </returns>
  bool tryRegisterOnRemoteServer(ESP8266WiFiMulti& wiFiManager, const String macAddress) const;

  /// <summary>
  /// Registers the station on the remote server.
  /// </summary>
  /// <remarks>
  /// Implements a retry policy, blocking execution until the request 
  /// is successfully acknowledged by the remote server.
  /// </remarks>
  /// <param name="wiFiManager">
  /// Reference to the WiFi manager responsible for maintaining the network connection.
  /// </param>
  /// <param name="macAddress">
  /// The MAC address of the station.
  /// </param>
  void registerOnRemoteServer(ESP8266WiFiMulti& wiFiManager, const String macAddress) const;

  /// <summary>
  /// Attempts to send heartbaet signal to the remote server.
  /// </summary>
  /// <param name="wiFiManager">
  /// Reference to the WiFi manager responsible for maintaining the network connection.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if the attempt was successful, <see langword="false"/> otherwise.
  /// </returns>
  bool trySendHeartbeatSignal(ESP8266WiFiMulti& wiFiManager) const;
};

#endif