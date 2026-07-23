using SmartHome.Server.Repositories.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace SmartHome.Server.Api.Controllers.Firmware.Requests;

/// <summary>
/// Data transfer object (DTO) representing a request to register a sensor within the system.
/// </summary>
/// <remarks>
/// Used for data exchange between the server and station firmware.
/// </remarks>
/// <param name="SensorLocalId">
/// The identifier of the sensor, unique only at the station level.
/// </param>
/// <param name="MeasurementType">
/// Type of measurements taken by the sensor.
/// </param>
public sealed record SensorRegistrationRequest(
    [Range(1, byte.MaxValue)]
    byte SensorLocalId,

    [EnumDataType(typeof(MeasurementType))]
    MeasurementType MeasurementType);
