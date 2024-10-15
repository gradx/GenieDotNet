using IBM.Data.Db2;
using Npgsql;

namespace Genie.Adapters.Persistence.DB2;

public class DB2PooledObject
{
    public DB2Connection Connection = new("Server=localhost:50000;Database=testdb;UID=DB2INST1;PWD=genie_in_a_bottle;");

    public DB2PooledObject()
    {
        Connection.Open();
    }
}
