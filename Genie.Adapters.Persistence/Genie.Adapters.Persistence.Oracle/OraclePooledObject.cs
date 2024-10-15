

using Oracle.ManagedDataAccess.Client;

namespace Genie.Adapters.Persistence.Oracle;

public class OraclePooledObject
{
    public OracleConnection Connection = new("Data Source=localhost;User Id=system;Password=Test123;");

    public OraclePooledObject()
    {
        Connection.Open();
    }
}
