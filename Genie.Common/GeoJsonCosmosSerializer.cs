﻿using Cysharp.IO;
using Microsoft.Azure.Cosmos;
using Microsoft.IO;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Converters;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Utf8StringInterpolation;

namespace Genie.Common;

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

    private static readonly RecyclableMemoryStreamManager manager = new RecyclableMemoryStreamManager();

    public override Stream ToStream<T>(T input)
    {
        MemoryStream ms = manager.GetStream();
        JsonSerializer.Serialize(ms, input, options);

        return ms;
    }

    public static string ToJson(object geometry)
    {
        var c = new GeoJsonCosmosSerializer();
        var str = (MemoryStream)c.ToStream(geometry);

        str.Position = 0;
        using var sr = new Utf8StreamReader(str);
        var reader = sr.AsTextReader();
        return reader.ReadToEndAsync().Result;
    }

    public static T FromJson<T>(string s)
    {
        var c = new GeoJsonCosmosSerializer();

        using StringReader sw = new(s);
        return c.FromStream<T>(manager.GetStream(Utf8String.Format($"{s}")));
    }
}