using Dapper;
using System.Data;
using System.Net.NetworkInformation;

namespace Server.Data.TypeHandlers.Dapper;

internal sealed class PhysicalAddressHandler : SqlMapper.TypeHandler<PhysicalAddress>
{
    public override void SetValue(IDbDataParameter parameter, PhysicalAddress? value)
    {
        parameter.DbType = DbType.AnsiStringFixedLength;
        parameter.Size = 12;    // Standard length for MAC address without separators.
        parameter.Value = value?.ToString() ?? DBNull.Value as object;
    }

    public override PhysicalAddress? Parse(object value) =>
        value is string valueAsString ? PhysicalAddress.Parse(valueAsString) : null;
}