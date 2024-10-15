

using Microsoft.Data.SqlClient;

namespace Genie.Adapters.Persistence.SqlServer;

public class SqlServerPooledObject
{
    // Integrated Security=true;TrustServerCertificate=True
    public SqlConnection Connection = new("Server=localhost;Database=genie;User Id=genie_user;Password=password;TrustServerCertificate=True;");

    public SqlServerPooledObject()
    {
        Connection.Open();
    }
}
