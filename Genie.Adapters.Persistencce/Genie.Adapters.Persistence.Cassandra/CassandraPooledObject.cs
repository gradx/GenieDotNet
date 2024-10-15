using Cassandra;


namespace Genie.Adapters.Persistence.Cassandra;

public class CassandraPooledObject
{
    public readonly ISession Session;

    public CassandraPooledObject()
    {
        var cluster = Cluster.Builder()
                   .AddContactPoint("localhost")
                   .Build();

        Session = cluster.Connect();

        _ = Session.Execute(@"CREATE KEYSPACE IF NOT EXISTS genie WITH REPLICATION = { 'class' : 'SimpleStrategy', 'replication_factor' : '1' };");

        Session = cluster.Connect("genie");

        _ = Session.Execute(@"CREATE TABLE IF NOT EXISTS genie.test (
            id text PRIMARY KEY,
            json text,
            last_update_timestamp timestamp
            );");
    }
}