using Npgsql;

namespace Genie.Adapters.Persistence.CockroachDB;

public class CockroachPooledObject
{
    public NpgsqlConnection Connection = new("Host=localhost;Port=26257;Username=admin;Password=password;Database=genie");

    public CockroachPooledObject()
    {
        Connection.Open();
    }
}