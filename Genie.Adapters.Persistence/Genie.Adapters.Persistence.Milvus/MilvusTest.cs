using Genie.Utils;
using Microsoft.Extensions.ObjectPool;
using Milvus.Client;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;


namespace Genie.Adapters.Persistence.Milvus;

public class MilvusTest(int payload, ObjectPool<MilvusPooledObject> pool) : IPersistenceTest
{
    public int Payload { get; set; } = payload;
    readonly ObjectPool<MilvusPooledObject> Pool = pool;


    public void Write(int i)
    {
        string id = $@"new{i}";
        var test = new PersistenceTest
        {
            Id = id,
            Info = new('-', Payload)
        };

        var json = JsonSerializer.Serialize(test);

        var vectors = new List<ReadOnlyMemory<float>>() { new float[2] { 0, 1 } };

        var lease = Pool.Get();

        MilvusCollection collection = MilvusPooledObject.Client.GetCollection(MilvusPooledObject.CollectionName);

        MutationResult result = collection.InsertAsync(
            [
                FieldData.Create("test_id", [id]),
                FieldData.CreateJson("json", [json]),
                FieldData.CreateFloatVector("vector", vectors)
            ]).GetAwaiter().GetResult();


        Pool.Return(lease);
    }

    public void Read(int i)
    {
        string id = $@"new{i}";

        var lease = Pool.Get();

        string expr = $@"test_id in ['{id}']";

        QueryParameters queryParameters = new();
        queryParameters.OutputFields.Add("test_id");
        queryParameters.OutputFields.Add("json");



        IReadOnlyList<FieldData> queryResult = MilvusPooledObject.Collection!.QueryAsync(
            expr,
            queryParameters).GetAwaiter().GetResult();

        Pool.Return(lease);
    }
}