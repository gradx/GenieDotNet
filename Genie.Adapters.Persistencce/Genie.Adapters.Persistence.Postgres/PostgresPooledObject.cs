using Npgsql;

namespace Genie.Adapters.Persistence.Postgres;

public class PostgresPooledObject
{
    public NpgsqlDataSource DataSource { get; init; }
    public NpgsqlConnection Connection { get; init; }

    public PostgresPooledObject()
    {
        var connectionString = "Host=localhost;Username=postgres;Password=genie_in_a_bottle;Database=postgres";
        var dsBuilder = new NpgsqlDataSourceBuilder(connectionString);
        DataSource = dsBuilder.Build();
        Connection = DataSource.CreateConnection();
        Connection.Open();
    }
}
