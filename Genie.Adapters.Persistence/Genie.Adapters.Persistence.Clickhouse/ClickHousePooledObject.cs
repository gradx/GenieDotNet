


using ClickHouse.Client.ADO;

namespace Genie.Adapters.Persistence.ClickHouse;

public class ClickHousePooledObject
{

    public ClickHouseConnection Connection = new("Host=localhost;Protocol=http;Port=8123;Username=genie;Password=password");

    public ClickHousePooledObject()
    {
        
    }
}