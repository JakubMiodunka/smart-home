using Dapper;
using System.Data;
using System.Net;

namespace Server.Data.TypeHandlers.Dapper;

internal sealed class IPAddressHandler : SqlMapper.TypeHandler<IPAddress>
{
    public override void SetValue(IDbDataParameter parameter, IPAddress? value)
    {
        parameter.DbType = DbType.AnsiString;
        parameter.Size = 39;    // Max length for IPv6 string representation.
        parameter.Value = value?.ToString() ?? DBNull.Value as object;
    }

    public override IPAddress? Parse(object value)
        => value is string valueAsString ? IPAddress.Parse(valueAsString) : null;
}