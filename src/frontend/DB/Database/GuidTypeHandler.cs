using Dapper;
using System.Data;

namespace DB.Database;

public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.Value = value.ToString(); // Guid → TEXT
    }

    public override Guid Parse(object value)
    {
        return Guid.Parse(value.ToString()); // TEXT → Guid
    }
}