using Confluent.Kafka;
using Genie.Core.Persistence;
using Microsoft.Extensions.ObjectPool;
using Milvus.Client;
using System.Text.Json;


namespace Genie.Adapters.Persistence.Milvus;

public class MilvusTest(int payload, ObjectPool<MilvusPooledObject> pool) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;


    public MilvusTest() : this(JsonSize,new DefaultObjectPool<MilvusPooledObject>(new DefaultPooledObjectPolicy<MilvusPooledObject>()))
    {

    }

    public MilvusTest(bool create) : this(JsonSize,new DefaultObjectPool<MilvusPooledObject>(new DefaultPooledObjectPolicy<MilvusPooledObject>()))
    {
        if (create)
            CreateDB();
    }


    public override void CreateDB()
    {
        CreateJsonDB();
        CreatePostalDB();
    }

    public override void CreateIndex()
    {
        var lease = pool.Get();

        lease.Postal.CreateIndexAsync("postal_code", IndexType.AutoIndex);

        pool.Return(lease);
    }

    public void CreateJsonDB()
    {
        var lease = pool.Get();

        MilvusCollection collection = lease.Client.GetCollection("json_data");

        var hasCollection = lease.Client.HasCollectionAsync("json_data").GetAwaiter().GetResult();

        if (hasCollection)
        {
            collection.DropAsync().GetAwaiter().GetResult();
            Console.WriteLine("Drop collection {0}", "json_data");
        }

        lease.Json = lease.Client.CreateCollectionAsync(
                   "json_data",
                    [
                        FieldSchema.CreateVarchar("id", 256, isPrimaryKey: true),
                        FieldSchema.CreateJson("json"),
                        FieldSchema.CreateFloatVector("vector", 2),
                    ]
                ).GetAwaiter().GetResult();

        lease.Json.CreateIndexAsync("vector", IndexType.AutoIndex, SimilarityMetricType.L2)
            .GetAwaiter().GetResult();

        lease.Json.LoadAsync().GetAwaiter().GetResult();

        pool.Return(lease);
    }

    public void CreatePostalDB()
    {
        var lease = pool.Get();

        MilvusCollection collection = lease.Client.GetCollection("country_postal");
        var hasCollection = lease.Client.HasCollectionAsync("country_postal").GetAwaiter().GetResult();

        if (hasCollection)
        {
            collection.DropAsync().GetAwaiter().GetResult();
            Console.WriteLine("Drop collection {0}", "country_postal");
        }

        lease.Postal = lease.Client.CreateCollectionAsync(
                   "country_postal",
                    [
                        FieldSchema.Create("id", MilvusDataType.Int64, isPrimaryKey: true),
                        FieldSchema.CreateVarchar("country_code", 256),
                        FieldSchema.CreateVarchar("postal_code", 256),
                        FieldSchema.CreateVarchar("place_name", 256),
                        FieldSchema.CreateVarchar("latitude", 50),
                        FieldSchema.CreateVarchar("longitude", 50),
                        FieldSchema.CreateFloatVector("vector", 2),
                    ]
                ).GetAwaiter().GetResult();

        lease.Postal.CreateIndexAsync("vector", IndexType.AutoIndex, SimilarityMetricType.L2)
            .GetAwaiter().GetResult();

        lease.Postal.LoadAsync().GetAwaiter().GetResult();

        pool.Return(lease);
    }

    public override bool ReadJson(long i)
    {
        bool result = true;
        var lease = pool.Get();

        try
        {
            string expr = $@"id in ['{i}']";

            QueryParameters queryParameters = new();
            queryParameters.OutputFields.Add("json");
            queryParameters.OutputFields.Add("vector");

            var queryResult = lease.Json!.QueryAsync(
                expr,
                queryParameters).GetAwaiter().GetResult();


            var json = queryResult.First(t => t.FieldName == "json") as FieldData<string>;
            var data = json.Data[0];
            var match = JsonSerializer.Deserialize<PersistenceTestModel>(data);

        }
        catch (Exception ex)
        {
            result = false;
        }

        pool.Return(lease);
        return result;
    }

    public override bool WriteJson(long i)
    {
        bool result = true;
        var lease = pool.Get();

        try
        {
            var test = new PersistenceTestModel
            {
                Id = i.ToString(),
                Info = new('-', Payload)
            };

            var json = JsonSerializer.Serialize(test);

            var vectors = new List<ReadOnlyMemory<float>>() { new float[2] { i, 1 } };

            MutationResult response = lease.Json.InsertAsync(
                [
                    FieldData.CreateVarChar("id", [test.Id]),
                FieldData.CreateJson("json", [json]),
                FieldData.CreateFloatVector("vector", vectors)
                ]).GetAwaiter().GetResult();
        }
        catch(Exception ex)
        {
            result = false;
        }

        pool.Return(lease);

        return result;
    }

    public override async Task<bool> WritePostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = pool.Get();

        try
        {
            MutationResult match = await lease.Postal!.InsertAsync(
            [
                FieldData.Create("id", [message.Id]),
                FieldData.CreateVarChar("country_code", [message.CountryCode ?? ""]),
                FieldData.CreateVarChar("postal_code", [message.PostalCode ?? ""]),
                FieldData.CreateVarChar("place_name", [message.PlaceName ?? ""]),
                FieldData.CreateVarChar("latitude", [message.Latitude?.ToString() ?? ""]),
                FieldData.CreateVarChar("longitude", [message.Longitude?.ToString() ?? ""]),
                FieldData.CreateFloatVector("vector", [new float[2] { message.Id, 1 }])
            ]);
        }
        catch (Exception ex)
        {
            result = false;
        }

        pool.Return(lease);
        return result;
    }

    public override async Task<bool> UpdatePostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = pool.Get();

        try
        {
            MutationResult match = await lease.Postal!.UpsertAsync(
            [
                FieldData.Create("id", [message.Id]),
                FieldData.CreateVarChar("country_code", [message.CountryCode ?? ""]),
                FieldData.CreateVarChar("postal_code", [message.PostalCode ?? ""]),
                FieldData.CreateVarChar("place_name", [message.PlaceName ?? ""]),
                FieldData.CreateVarChar("latitude", [message.Latitude?.ToString() ?? ""]),
                FieldData.CreateVarChar("longitude", [message.Longitude?.ToString() ?? ""]),
                FieldData.CreateFloatVector("vector", [new float[2] { message.Id, 1 }])
            ]);
        }
        catch (Exception ex)
        {
            result = false;
        }

        pool.Return(lease);
        return result;
    }

    public override async Task<bool> ReadPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = pool.Get();

        try
        {
            string expr = $@"id in [{(long)message.Id}]";

            QueryParameters queryParameters = new();
            queryParameters.OutputFields.Add("id");
            queryParameters.OutputFields.Add("country_code");
            queryParameters.OutputFields.Add("postal_code");
            queryParameters.OutputFields.Add("place_name");
            queryParameters.OutputFields.Add("latitude");
            queryParameters.OutputFields.Add("longitude");
            queryParameters.OutputFields.Add("vector");

            var queryResult = await lease.Postal!.QueryAsync(
                expr,
                queryParameters);

            var match = Create(queryResult);
        }
        catch (Exception ex)
        {
            result = false;
        }

        pool.Return(lease);
        return result;
    }

    public CountryPostalCode Create(IReadOnlyList<FieldData> queryResult)
    {
        var id = queryResult.First(t => t.FieldName == "id") as FieldData<long>;
        var country = queryResult.First(t => t.FieldName == "country_code") as FieldData<string>;
        var postal = queryResult.First(t => t.FieldName == "postal_code") as FieldData<string>;
        var place = queryResult.First(t => t.FieldName == "place_name") as FieldData<string>;
        var latitude = queryResult.First(t => t.FieldName == "latitude") as FieldData<string>;
        var longitude = queryResult.First(t => t.FieldName == "longitude") as FieldData<string>;

        return new CountryPostalCode
        {
            Id = Convert.ToInt32(id.Data.FirstOrDefault()),
            CountryCode = country.Data.FirstOrDefault(),
            PostalCode = postal.Data.FirstOrDefault(),
            PlaceName = place.Data.FirstOrDefault(),
            Latitude = double.Parse(latitude.Data.FirstOrDefault()),
            Longitude = double.Parse(longitude.Data.FirstOrDefault())
        };
    }

    public override async Task<bool> QueryPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = pool.Get();

        try
        {
            string expr = $@"postal_code in ['{message.PostalCode}']";

            QueryParameters queryParameters = new();
            queryParameters.OutputFields.Add("id");
            queryParameters.OutputFields.Add("country_code");
            queryParameters.OutputFields.Add("postal_code");
            queryParameters.OutputFields.Add("place_name");
            queryParameters.OutputFields.Add("latitude");
            queryParameters.OutputFields.Add("longitude");

            var queryResult = await lease.Postal!.QueryAsync(
                expr,
                queryParameters);

            var match = Create(queryResult);

        }
        catch (Exception ex)
        {
            result = false;
        }

        pool.Return(lease);
        return result;
    }

    public override async Task<bool> SelfJoinPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = pool.Get();

        try
        {
            string expr = $@"id in [{(long)message.Id}]";

            QueryParameters queryParameters = new();
            queryParameters.OutputFields.Add("id");
            queryParameters.OutputFields.Add("country_code");
            queryParameters.OutputFields.Add("postal_code");
            queryParameters.OutputFields.Add("place_name");
            queryParameters.OutputFields.Add("latitude");
            queryParameters.OutputFields.Add("longitude");

            var queryResult = await lease.Postal!.QueryAsync(
                expr,
                queryParameters);


            var match = Create(queryResult);

            string expr2 = $@"postal_code in ['{match.PostalCode}']";

            QueryParameters queryParameters2 = new();
            queryParameters2.OutputFields.Add("id");
            queryParameters2.OutputFields.Add("country_code");
            queryParameters2.OutputFields.Add("place_name");
            queryParameters2.OutputFields.Add("latitude");
            queryParameters2.OutputFields.Add("longitude");

            var queryResult2 = await lease.Postal!.QueryAsync(
                expr2,
                queryParameters2);

            var results = queryResult2.ToList();


            var ids = results.FindAll(x => x.FieldName == "id")[0] as FieldData<long>;
            var countries = results.FindAll(x => x.FieldName == "country_code")[0] as FieldData<string>;
            var places = results.FindAll(x => x.FieldName == "place_name")[0] as FieldData<string>;
            var latitudes = results.FindAll(x => x.FieldName == "latitude")[0] as FieldData<string>;
            var longitudes = results.FindAll(x => x.FieldName == "longitude")[0] as FieldData<string>;

            var rowcount = results[0].RowCount;
            var lookup = new Dictionary<long, CountryPostalCode>();

            for (int i = 0; i < rowcount; i++)
            {
                var cc2 = new CountryPostalCode
                {
                    Id = (int)ids!.Data[i],
                    CountryCode = countries!.Data[i],
                    PostalCode = message.PostalCode,
                    PlaceName = places!.Data[i],
                    Latitude = string.IsNullOrEmpty(latitudes!.Data[i]) ? null : double.Parse(latitudes!.Data[i]),
                    Longitude = string.IsNullOrEmpty(longitudes!.Data[i]) ? null : double.Parse(longitudes!.Data[i])
                };
            }
        }
        catch (Exception ex)
        {
            result = false;
        }

        pool.Return(lease);
        return result;
    }
}