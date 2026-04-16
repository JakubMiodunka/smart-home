using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartHome.Server.Data.Converters.JsonConverters;

/// <summary>
/// Provides custom JSON serialization and deserialization for the <see cref="PhysicalAddress"/> type.
/// </summary>
/// <remarks>
/// This handler serializes MAC addresses to a flat 12-character string format (without separators).
/// </remarks>
public class PhysicalAddressConverter : JsonConverter<PhysicalAddress>
{
    #region Type conversion
    /// <summary>
    /// Reads and converts the JSON <see cref="string"/> to a <see cref="PhysicalAddress"/> object.
    /// </summary>
    /// <param name="reader">
    /// The reader to read from.
    /// </param>
    /// <param name="typeToConvert">
    /// The type to convert.
    /// </param>
    /// <param name="options">
    /// An object that specifies serialization options to use.
    /// </param>
    /// <returns>
    /// A <see cref="PhysicalAddress"/> instance, or <see langword="null"/> if the JSON value is set to <see langword="null">.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    public override PhysicalAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert, nameof(typeToConvert));
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        string? valueAsString = reader.GetString();
        return valueAsString is null ? null : PhysicalAddress.Parse(valueAsString);
    }

    /// <summary>
    /// Writes the <see cref="PhysicalAddress"/> value as a JSON <see cref="string">.
    /// </summary>
    /// <param name="writer">
    /// The writer to write to.
    /// </param>
    /// <param name="value">
    /// The value to convert to JSON.
    /// </param>
    /// <param name="options">
    /// An object that specifies serialization options to use.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    public override void Write(Utf8JsonWriter writer, PhysicalAddress value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer, nameof(writer));
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        writer.WriteStringValue(value.ToString());
    }
    #endregion
}