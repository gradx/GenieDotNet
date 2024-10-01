using Genie.Utils;
using Microsoft.Extensions.ObjectPool;
using Milvus.Client;
using System.Text.Json;


namespace Genie.Adapters.Persistence.Milvus;

public class MilvusTest(int payload, ObjectPool<MilvusPooledObject> pool, ObjectPool<MilvusPooledObject2> pool2) : PersistenceTestBase, IPersistenceTest
{
    public int Payload { get; set; } = payload;


    public override bool ReadJson(long i)
    {
        throw new NotImplementedException();
    }

    public override bool WriteJson(long i)
    {
        bool success = true;
        string id = $@"new{i}";
        var test = new PersistenceTestModel
        {
            Id = id,
            Info = new('-', Payload)
        };

        var json = JsonSerializer.Serialize(test);

        var vectors = new List<ReadOnlyMemory<float>>() { new float[2] { i, 1 } };


        MilvusCollection collection = MilvusPooledObject.Client.GetCollection(MilvusPooledObject.CollectionName);

        MutationResult result = collection.InsertAsync(
            [
                FieldData.Create("test_id", [id]),
                FieldData.CreateJson("json", [json]),
                FieldData.CreateFloatVector("vector", vectors)
            ]).GetAwaiter().GetResult();

        return success;
    }

    public override async Task<bool> WritePostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = pool2.Get();

        try
        {

            MutationResult match = await lease.Collection!.InsertAsync(
            [
                FieldData.Create("id", [(long)message.Id]),
                FieldData.Create("country_code", [message.CountryCode ?? ""]),
                FieldData.Create("postal_code", [message.PostalCode ?? ""]),
                FieldData.Create("place_name", [message.PlaceName ?? ""]),
                FieldData.Create("latitude", [message.Latitude?.ToString() ?? ""]),
                FieldData.Create("longitude", [message.Longitude?.ToString() ?? ""]),
                FieldData.CreateFloatVector("vector", [new float[2] { message.Id, 1 }])
            ]);
        }
        catch (Exception ex)
        {
            result = false;
        }


        pool2.Return(lease);
        return result;
    }

    public override async Task<bool> ReadPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = pool2.Get();

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

            var queryResult = await lease.Collection!.QueryAsync(
                expr,
                queryParameters);


            var id = queryResult.First(t => t.FieldName == "id") as FieldData<long>;
            var country = queryResult.First(t => t.FieldName == "country_code") as FieldData<string>;
            var postal = queryResult.First(t => t.FieldName == "postal_code") as FieldData<string>;
            var place = queryResult.First(t => t.FieldName == "place_name") as FieldData<string>;
            var latitude = queryResult.First(t => t.FieldName == "latitude") as FieldData<string>;
            var longitude = queryResult.First(t => t.FieldName == "longitude") as FieldData<string>;

            var cc = new CountryPostalCode
            {
                Id = Convert.ToInt32(id.Data.FirstOrDefault()),
                CountryCode = country.Data.FirstOrDefault(),
                PostalCode = postal.Data.FirstOrDefault(),
                PlaceName = place.Data.FirstOrDefault(),
                Latitude = double.Parse(latitude.Data.FirstOrDefault()),
                Longitude = double.Parse(longitude.Data.FirstOrDefault())
            };
        }
        catch (Exception ex)
        {
            result = false;
        }

        pool2.Return(lease);
        return result;
    }
    public override async Task<bool> QueryPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = pool2.Get();

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

            var queryResult = await lease.Collection!.QueryAsync(
                expr,
                queryParameters);

            var id = queryResult.First(t => t.FieldName == "id") as FieldData<long>;
            var country = queryResult.First(t => t.FieldName == "country_code") as FieldData<string>;
            var postal = queryResult.First(t => t.FieldName == "postal_code") as FieldData<string>;
            var place = queryResult.First(t => t.FieldName == "place_name") as FieldData<string>;
            var latitude = queryResult.First(t => t.FieldName == "latitude") as FieldData<string>;
            var longitude = queryResult.First(t => t.FieldName == "longitude") as FieldData<string>;

            var cc = new CountryPostalCode
            {
                Id = Convert.ToInt32(id.Data.FirstOrDefault()),
                CountryCode = country.Data.FirstOrDefault(),
                PostalCode = postal.Data.FirstOrDefault(),
                PlaceName = place.Data.FirstOrDefault(),
                Latitude = double.Parse(latitude.Data.FirstOrDefault()),
                Longitude = double.Parse(longitude.Data.FirstOrDefault())
            };

        }
        catch (Exception ex)
        {
            result = false;
        }

        pool2.Return(lease);
        return result;
    }

    public override async Task<bool> SelfJoinPostal(CountryPostalCode message)
    {
        bool result = true;
        var lease = pool2.Get();

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

            var queryResult = await lease.Collection!.QueryAsync(
                expr,
                queryParameters);


            var id = queryResult.First(t => t.FieldName == "id") as FieldData<long>;
            var country = queryResult.First(t => t.FieldName == "country_code") as FieldData<string>;
            var postal = queryResult.First(t => t.FieldName == "postal_code") as FieldData<string>;
            var place = queryResult.First(t => t.FieldName == "place_name") as FieldData<string>;
            var latitude = queryResult.First(t => t.FieldName == "latitude") as FieldData<string>;
            var longitude = queryResult.First(t => t.FieldName == "longitude") as FieldData<string>;

            var cc = new CountryPostalCode
            {
                Id = Convert.ToInt32(id.Data.FirstOrDefault()),
                CountryCode = country.Data.FirstOrDefault(),
                PostalCode = postal.Data.FirstOrDefault(),
                PlaceName = place.Data.FirstOrDefault(),
                Latitude = double.Parse(latitude.Data.FirstOrDefault()),
                Longitude = double.Parse(longitude.Data.FirstOrDefault())
            };

            string expr2 = $@"postal_code in ['{message.PostalCode}']";

            QueryParameters queryParameters2 = new();
            queryParameters2.OutputFields.Add("id");
            queryParameters2.OutputFields.Add("country_code");
            queryParameters2.OutputFields.Add("place_name");
            queryParameters2.OutputFields.Add("latitude");
            queryParameters2.OutputFields.Add("longitude");

            var queryResult2 = await lease.Collection!.QueryAsync(
                expr2,
                queryParameters2);

            var results = queryResult2.ToList();


            var ids = results.FindAll(x => x.FieldName == "id")[0] as FieldData<long>;
            var countrys = results.FindAll(x => x.FieldName == "country_code")[0] as FieldData<string>;
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
                    CountryCode = countrys!.Data[i],
                    PostalCode = message.PostalCode,
                    Latitude = string.IsNullOrEmpty(latitudes!.Data[i]) ? null : double.Parse(latitudes!.Data[i]),
                    Longitude = string.IsNullOrEmpty(longitudes!.Data[i]) ? null : double.Parse(longitudes!.Data[i])
                };
            }
        }
        catch (Exception ex)
        {
            result = false;
        }

        pool2.Return(lease);
        return result;
    }
}