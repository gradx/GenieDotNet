using Aerospike.Client;


namespace Genie.Adapters.Persistence.Aerospike;

public class AerospikePooledObject
{
    public readonly AerospikeClient Client = new("localhost", 3000);

    public AerospikePooledObject()
    {

    }
}