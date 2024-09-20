using Couchbase.KeyValue;
using Couchbase;

namespace Genie.Adapters.Persistence.Couchbase;

public class CouchbasePooledObject
{
    public readonly ICluster ICluster;
    public readonly IBucket Bucket;
    public readonly IScope Scope;
    public readonly ICouchbaseCollection Collection;

    public CouchbasePooledObject()
    {
        ICluster = Cluster.ConnectAsync(
        "couchbase://localhost",
        "Administrator",
        "password").GetAwaiter().GetResult();

        Bucket = ICluster.BucketAsync("genie").GetAwaiter().GetResult();

        Scope = Bucket.Scope("genie_scope");
        Collection = Scope.Collection("genie_bench");

    }
}
