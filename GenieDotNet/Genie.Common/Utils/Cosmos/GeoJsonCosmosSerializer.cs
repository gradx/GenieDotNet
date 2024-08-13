using Microsoft.Azure.Cosmos;
using NetTopologySuite.IO.Converters;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using NetTopologySuite.IO;

namespace Genie.Common.Utils.Cosmos;

public class GeoJsonCosmosSerializer : CosmosSerializer
{

    private readonly JsonSerializerOptions options;

    public GeoJsonCosmosSerializer()
    {
        options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        options.Converters.Add(new GeoJsonConverterFactory());
    }

    public override T FromStream<T>(Stream stream)
    {
        using (stream)
        {
            if (typeof(Stream).IsAssignableFrom(typeof(T)))
                return (T)(object)stream;

            // Newtonsoft required for NetopologySuite.Feature.AttributeTable CDC equality
            return GeoJsonSerializer.Create().Deserialize<T>(new Newtonsoft.Json.JsonTextReader(new StreamReader(stream)))!;
            //return JsonSerializer.Deserialize<T>(stream, options)!;
        }
    }

    public override Stream ToStream<T>(T input)
    {
        MemoryStream ms = new();
        JsonSerializer.Serialize(ms, input, options);

        return ms;
    }

    public static string ToJson(object geometry)
    {
        var c = new GeoJsonCosmosSerializer();
        var str = (MemoryStream)c.ToStream(geometry);
        return Encoding.UTF8.GetString(str.ToArray());
    }

    public static T FromJson<T>(string s)
    {
        var c = new GeoJsonCosmosSerializer();

        using (StringReader sw = new(s))
            return c.FromStream<T>(new MemoryStream(Encoding.UTF8.GetBytes(s)));
    }
}