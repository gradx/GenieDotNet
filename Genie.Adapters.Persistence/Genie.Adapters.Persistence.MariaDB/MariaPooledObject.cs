
using MySqlConnector;

namespace Genie.Adapters.Persistence.MariaDB;

public class MariaPooledObject
{
    public readonly MySqlConnection Connection;

    public MariaPooledObject()
    {
        string connectionString = "Server=localhost;Database=benchmark;User=root;Password=;";
        Connection = new MySqlConnection(connectionString);

        Connection.Open();

    }
}