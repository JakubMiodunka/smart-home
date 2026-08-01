namespace SmartHome.Server.Repositories.Enumerations;

/// <summary>
/// Describes the type of a measurement.
/// </summary>
/// <remarks>
/// Also serves as an implicit unit for the recorded value, 
/// as all measurements are expressed in SI units.
/// Must be kept in sync with the corresponding enumeration in the firmware codebase.
/// </remarks>
public enum MeasurementType : byte
{
    Temperature
}