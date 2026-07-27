#ifndef SENSORS_H
#define SENSORS_H

#include <Arduino.h>
#include <ArduinoJson.h>
#include <ESP8266WebServer.h>
#include <ESP8266WiFiMulti.h>

/// <remarks>
/// Should be in sync with enumeration in remote server codebase.
/// </remarks>
enum MeasurementType {
  Temperature
};

/// <summary>
/// Data transfer object (DTO) representing the request for sensor registration on the remote server.
/// </summary>
struct SensorRegistrationStationRequest {
  /// <summary>
  /// The identifier of the sensor, unique at the station level.
  /// </summary>
  uint8_t sensorLocalId;
  
  /// <summary>
  /// Type of measurements taken by the sensor.
  /// </summary>
  MeasurementType measurementType;

  /// <summary>
  /// Populates the provided JSON document with sensor registration data.
  /// </summary>
  /// <param name="jsonDocument">
  /// The JSON document to be populated.
  /// </param>
  void toJsonDocument(JsonDocument& jsonDocument) const;
};

/// <summary>
/// Data transfer object (DTO) representing the server's response to a sensor registration request.
/// </summary>
struct SensorRegistrationServerResponse {
  /// <summary>
  /// The identifier assigned to the sensor by remote server, unique within the system.
  /// </summary>
  uint32_t sensorId;

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
  static bool tryParseJsonDocument(const JsonDocument& jsonDocument, SensorRegistrationServerResponse& response);
};

/// <summary>
/// Data transfer object (DTO) representing the station to a request for a sensor measurement.
/// </summary>
struct GetMeasurementStationResponse {
  /// <summary>
  /// Value measured by the sensor.
  /// </summary>
  double measurementValue;

  /// <summary>
  /// Populates the provided JSON document with the sensor measurement data.
  /// </summary>
  /// <param name="jsonDocument">
  /// The JSON document to be populated.
  /// </param>
  void toJsonDocument(JsonDocument& jsonDocument) const;
};

/// <summary>
/// Representation of a single sensor capable of taking measurements.
/// </summary>
struct Sensor {
  /// <summary>
  /// The identifier of the sensor, unique within the system.
  /// </summary>
  /// <remarks>
  /// A value of zero indicates that the identifier has not yet been assigned.
  /// </remarks>
  uint32_t id = 0;

  /// <summary>
  /// The identifier of the sensor, unique only at the station level.
  /// </summary>
  /// <remarks>
  /// A value of zero indicates that the identifier has not yet been assigned.
  /// </remarks>
  uint8_t localId = 0;

  /// <summary>
  /// Performs initial configuration of the sensor.
  /// </summary>
  /// <remarks>
  /// Needs to be assigned during structure creation according to particualr sensor hardware.
  /// </remarks>
  void (* const initialize)();

  /// <summary>
  /// Takes a single measurement.
  /// </summary>
  /// <remarks>
  /// Needs to be assigned during structure creation according to particualr sensor hardware.
  /// </remarks>
  /// <returns>
  /// Value of the measurement retrieved from the sensor.
  /// </returns>
  double (* const takeMeasurement)();

  /// <summary>
  /// Attempts to register the sensor on the remote server.
  /// </summary>
  /// <param name="wiFiManager">
  /// Reference to the WiFi manager responsible for maintaining the network connection.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if the attempt was successful, <see langword="false"/> otherwise.
  /// </returns>
  bool tryRegisterOnRemoteServer(ESP8266WiFiMulti& wiFiManager);

  /// <summary>
  /// Registers the sensor on the remote server.
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
  /// Configures a HTTP endpoint on the provided server to allow remote control of the sensor.
  /// </summary>
  /// <param name="server">
  /// Reference to the web server where the endpoint shall be registered.
  /// </param>
  void setupControlEndpoint(ESP8266WebServer& server);
};

#endif