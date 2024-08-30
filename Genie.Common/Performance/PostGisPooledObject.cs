using Npgsql;

namespace Genie.Common.Performance;

public class PostGisPooledObject : GeniePooledObject
{
    public NpgsqlDataSource DataSource { get; init; }

    public PostGisPooledObject()
    {
        var connectionString = "Host=localhost;Username=postgres;Password=genie_in_a_bottle;Database=postgres";
        var dsBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dsBuilder.UseNetTopologySuite();
        DataSource = dsBuilder.Build();
    }
}
