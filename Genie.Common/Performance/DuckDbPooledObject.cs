using DuckDB.NET.Data;
using Genie.Common.Utils;
using Genie.Core;

namespace Genie.Common.Performance;

public class DuckDbPooledObject : GeniePooledObject
{
    public DuckDBConnection Connection { get; init; }

    public DuckDbPooledObject()
    {
        Connection = DuckDbSupport.GetSpatialDb();
    }
}