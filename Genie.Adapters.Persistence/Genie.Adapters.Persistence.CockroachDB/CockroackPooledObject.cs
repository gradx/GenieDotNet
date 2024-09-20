using Npgsql;

namespace Genie.Adapters.Persistence.CockroachDB;

public class CockroackPooledObject
{
    public NpgsqlDataSource DataSource { get; init; }
    public NpgsqlConnection Connection { get; init; }

    public CockroackPooledObject()
    {
        var connectionString = "Host=localhost;Username=admin;Password=password;Database=genie;Port=26257;";
        var dsBuilder = new NpgsqlDataSourceBuilder(connectionString);
        DataSource = dsBuilder.Build();
        Connection = DataSource.CreateConnection();
        Connection.Open();
    }
}
