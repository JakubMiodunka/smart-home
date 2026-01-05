using Dapper;
using System.Data;
using System.Net.NetworkInformation;

namespace SmartHome.Server.Data.Converters.TypeHandlers;

/// <summary>
/// Provides custom mapping between <see cref="PhysicalAddress"/> and database string columns for Dapper.
/// </summary>
/// <remarks>
/// This handler serializes MAC addresses to a flat 12-character string format (without separators).
/// </remarks>
public sealed class PhysicalAddressHandler : SqlMapper.TypeHandler<PhysicalAddress>
{
    #region Type conversion
    /// <summary>
    /// Configures the SQL parameter before executing the command.
    /// </summary>
    /// <param name="parameter">
    /// The database parameter to be configured.
    /// </param>
    /// <param name="value">
    /// The <see cref="PhysicalAddress"/> instance to be stored. 
    /// If <see langword="null"/>, <see cref="DBNull"/> is sent to the database.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    public override void SetValue(IDbDataParameter parameter, PhysicalAddress? value)
    {
        ArgumentNullException.ThrowIfNull(parameter, nameof(parameter));

        parameter.DbType = DbType.AnsiStringFixedLength;
        parameter.Size = 12;
        parameter.Value = value?.ToString() ?? DBNull.Value as object;
    }

    /// <summary>
    /// Parses the database value back into a <see cref="PhysicalAddress"/> object.
    /// </summary>
    /// <param name="value">
    /// The raw value retrieved from the database (expected to be a <see cref="string"/>).
    /// </param>
    /// <returns>
    /// A <see cref="PhysicalAddress"/> instance, or <see langword="null"/> if the database value is <see cref="DBNull"/>.
    /// </returns>
    public override PhysicalAddress? Parse(object? value) =>
        value is string valueAsString ? PhysicalAddress.Parse(valueAsString) : null;
    #endregion
}