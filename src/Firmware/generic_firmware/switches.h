#ifndef SWITCHES_H
#define SWITCHES_H

#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266WebServer.h>
#include <ESP8266WiFiMulti.h>

#include "requests.h"

/// <summary>
/// Data transfer object (DTO) representing the request for switch registration on the remote server.
/// </summary>
struct SwitchRegistrationStationRequest {
  /// <summary>
  /// The identifier of the switch, unique at the station level.
  /// </summary>
  uint8_t switchLocalId;
  
  /// <summary>
  /// Populates the provided JSON document with switch registration data.
  /// </summary>
  /// <param name="jsonDocument">
  /// The JSON document to be populated.
  /// </param>
  void toJsonDocument(JsonDocument& jsonDocument) const;
};

/// <summary>
/// Data transfer object (DTO) representing the server's response to a switch registration request.
/// </summary>
struct SwitchRegistrationServerResponse {
  /// <summary>
  /// The identifier assigned to the switch by remote server, unique within the system.
  /// </summary>
  uint8_t switchGlobalId;

  /// <summary>
  /// Logical state of the switch to which shall be set - <see langword="true"/> if the switch
  /// shall be closed and current shall flow, <see langword="false"/> otherwise.
  /// </summary>
  bool expectedSwitchState;

  /// <summary>
  /// Attempts to parse a JSON document into DTO instance.
  /// </summary>
  /// <param name="jsonString">
  /// JSON document to be parsed.
  /// </param>
  /// <param name="response">
  /// Reference to the object where the parsing result shall be stored.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if parsing was successful and all fields are valid, <see langword="false"/> otherwise.
  /// </returns>
  static bool tryParseJsonDocument(const JsonDocument& jsonDocument, SwitchRegistrationServerResponse& response);
};

/// <summary>
/// Data transfer object (DTO) for updating switch details on the remote server.
/// </summary>
struct UpdateSwitchStationRequest {
  /// <summary>
  /// Current logical state of the switch - <see langword="true"/> if the switch
  /// is closed and current is flowing, <see langword="false"/> otherwise.
  /// </summary>
  bool actualSwitchState;

  // <summary>
  /// Populates the provided JSON document with switch state update data.
  /// </summary>
  /// <param name="request">
  /// The JSON document to be populated.
  /// </param>
  void toJsonDocument(JsonDocument& jsonDocument) const;
};

/// <summary>
/// Data transfer object (DTO) sent from remote server to control specific switch.
/// </summary>
struct UpdateSwitchServerRequest {
  /// <summary>
  /// Logical state of the switch to which shall be set - <see langword="true"/> if the switch
  /// shall be closed and current shall flow, <see langword="false"/> otherwise.
  /// </summary>
  bool expectedSwitchState;

  /// <summary>
  /// Attempts to parse a JSON document into DTO instance.
  /// </summary>
  /// <param name="jsonDocument">
  /// JSON document to be parsed.
  /// </param>
  /// <param name="request">
  /// Reference to the object where the parsing result shall be stored.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if parsing was successful and all fields are valid, <see langword="false"/> otherwise.
  /// </returns>
  static bool tryParseJsonDocument(const JsonDocument& jsonDocument, UpdateSwitchServerRequest& request);
};

/// <summary>
/// Representation of a single electrical switch.
/// </summary>
struct Switch {
  /// <summary>
  /// The identifier of the switch, unique within the system.
  /// </summary>
  /// <remarks>
  /// A value of zero indicates that the identifier has not yet been assigned.
  /// </remarks>
  uint32_t globalId = 0;

  /// <summary>
  /// The identifier of the switch, unique only at the station level.
  /// </summary>
  /// <remarks>
  /// A value of zero indicates that the identifier has not yet been assigned.
  /// </remarks>
  uint8_t localId = 0;

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
  bool reversedLogic;

  /// <summary>
  /// Configures switch GPIO pin as output.
  /// </summary>
  void initialize() const;

  /// <summary>
  /// Determines the logical state of the switch.
  /// </summary>
  /// <returns>
  /// Logical state of the switch - <see langword="true"/> if the switch is closed
  /// and current is flowing, <see langword="false"/> otherwise.
  /// </returns>
  bool getState() const;

  /// <summary>
  /// Changes the state of the switch according to its logic.
  /// </summary>
  /// <remarks>
  /// This function updates values within the instance
  /// along with the physical state of the GPIO pin.
  /// </remarks>
  /// <param name="expectedState">
  /// <see langword="true"/> if the switch should be closed to allow current flow,
  /// <see langword="false"/> otherwise.
  /// </param>
  void setState(const bool expectedState);

  /// <summary>
  /// Attempts to register the switch on the remote server.
  /// </summary>
  /// <remarks>
  /// This function changes state of switch instance according to received server response.
  /// </remarks>
  /// <param name="wiFiManager">
  /// Reference to the WiFi manager responsible for maintaining the network connection.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if the attempt was successful, <see langword="false"/> otherwise.
  /// </returns>
  bool tryRegisterOnRemoteServer(ESP8266WiFiMulti& wiFiManager);

  /// <summary>
  /// Registers the switch on the remote server.
  /// </summary>
  /// <remarks>
  /// Implements a retry policy, blocking execution until the request 
  /// is successfully acknowledged by the remote server.
  /// </remarks>
  /// <param name="wiFiManager">
  /// Reference to the WiFi manager responsible for maintaining the network connection.
  /// </param>
  void registerOnRemoteServer(ESP8266WiFiMulti& wiFiManager);

  /// <summary>
  /// Attempts to update switch details on the remote server.
  /// </summary>
  /// <param name="wiFiManager">
  /// Reference to the WiFi manager responsible for maintaining the network connection.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if the attempt was successful, <see langword="false"/> otherwise.
  /// </returns>
  bool tryUpdateOnRemoteServer(ESP8266WiFiMulti& wiFiManager) const;

  /// <summary>
  /// Updates switch details on the remote server.
  /// </summary>
  /// <remarks>
  /// Implements a retry policy, blocking execution until the update 
  /// is successfully acknowledged by the remote server.
  /// </remarks>
  /// <param name="wiFiManager">
  /// Reference to the WiFi manager responsible for maintaining the network connection.
  /// </param>
  void updateOnRemoteServer(ESP8266WiFiMulti& wiFiManager) const;

  /// <summary>
  /// Configures a HTTP endpoint on the provided server to allow remote control of the switch.
  /// </summary>
  /// <param name="server">
  /// Reference to the web server where the endpoint shall be registered.
  /// </param>
  void setupControlEndpoint(ESP8266WebServer& server);
};

#endif