
using SingleStoreConnector;

namespace Genie.Adapters.Persistence.SingleStore;

public class SingleStorePooledObject
{
    public SingleStoreConnection Connection = new("host=localhost;port=3306;userid=root;password=password;database=benchmarks;");

    public SingleStorePooledObject()
    {
        Connection.Open();
    }
}
