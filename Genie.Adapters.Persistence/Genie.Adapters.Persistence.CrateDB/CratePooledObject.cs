using Npgsql;

namespace Genie.Adapters.Persistence.CrateDB;

public class CratePooledObject
{
    public NpgsqlDataSource DataSource { get; init; }
    public NpgsqlConnection Connection { get; init; }

    public CratePooledObject()
    {
        var connectionString = "Host=localhost;Username=crate;Password=;Database=genie";
        var dsBuilder = new NpgsqlDataSourceBuilder(connectionString);
        DataSource = dsBuilder.Build();
        Connection = DataSource.CreateConnection();
        Connection.Open();
    }
}
