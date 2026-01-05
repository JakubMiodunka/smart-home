using Dapper;
using System.Data;
using System.Net;

namespace SmartHome.Server.Data.Converters.TypeHandlers;

/// <summary>
/// Provides custom mapping between <see cref="IPAddress"/> and database string columns for Dapper.
/// </summary>
/// <remarks>
/// Both IPv4 and IPv6 addresses are supported.
/// </remarks>
public sealed class IPAddressHandler : SqlMapper.TypeHandler<IPAddress>
{
    #region Type conversion
    /// <summary>
    /// Configures the SQL parameter before executing the command.
    /// </summary>
    /// <param name="parameter">
    /// The database parameter to be configured.
    /// </param>
    /// <param name="value">
    /// The <see cref="IPAddress"/> instance to be stored. 
    /// If <see langword="null"/>, <see cref="DBNull"/> is sent to the database.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    public override void SetValue(IDbDataParameter parameter, IPAddress? value)
    {
        ArgumentNullException.ThrowIfNull(parameter, nameof(parameter));

        parameter.DbType = DbType.AnsiString;
        parameter.Size = 39;    // Max length for IPv6 string representation.
        parameter.Value = value?.ToString() ?? DBNull.Value as object;
    }

    /// <summary>
    /// Parses the database value back into an <see cref="IPAddress"/> object.
    /// </summary>
    /// <param name="value">
    /// The raw value retrieved from the database (expected to be a <see cref="string"/>).
    /// </param>
    /// <returns>
    /// An <see cref="IPAddress"/> instance, or <see langword="null"/> reference if the database value is set to <see cref="DBNull"/>.
    /// </returns>
    public override IPAddress? Parse(object? value)
        => value is string valueAsString ? IPAddress.Parse(valueAsString) : null;
    #endregion
}