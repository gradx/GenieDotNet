using Marten;

namespace Genie.Adapters.Persistence.Marten;

public class MartenPooledObject
{
    public readonly DocumentStore Store;

    public MartenPooledObject()
    {
        var ops = new StoreOptions();
        ops.Connection("Host=localhost;Username=postgres;Password=genie_in_a_bottle;Database=postgres");

        Store = new DocumentStore(ops);
    }
}