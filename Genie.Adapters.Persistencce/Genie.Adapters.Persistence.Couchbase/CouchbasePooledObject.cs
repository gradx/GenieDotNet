using Couchbase.KeyValue;
using Couchbase;

namespace Genie.Adapters.Persistence.Couchbase;

public class CouchbasePooledObject
{
    public readonly ICluster ICluster;
    public readonly IBucket Bucket;
    public readonly IScope Scope;
    public ICouchbaseCollection Json;
    public ICouchbaseCollection Postal;

    public CouchbasePooledObject()
    {
        // http://localhost:8091/

        ICluster = Cluster.ConnectAsync(
        "couchbase://localhost",
        "Administrator",
        "password").GetAwaiter().GetResult();

        Bucket = ICluster.BucketAsync("genie").GetAwaiter().GetResult();

        Scope = Bucket.Scope("genie_scope");

        Json = Scope.Collection("json_data");
        Postal = Scope.Collection("country_postal");

    }
}
