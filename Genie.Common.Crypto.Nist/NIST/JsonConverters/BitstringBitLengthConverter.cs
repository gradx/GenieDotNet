using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Math.JsonConverters
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public class BitstringBitLengthConverter : JsonConverter
    {

        private class BitStringModel
        {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
            public string Value { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
            public int BitLength { get; set; }
        }

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        {
            var bs = (BitString)value;

            writer.WriteStartObject();
            writer.WritePropertyName(nameof(BitStringModel.Value));
            writer.WriteValue(bs.ToHex());
            writer.WritePropertyName(nameof(BitStringModel.BitLength));
            writer.WriteValue(bs.BitLength);
            writer.WriteEndObject();
        }

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        {
            if (reader.TokenType != JsonToken.Null)
            {
                // Handles old style BitStrings
                if (reader.TokenType == JsonToken.String)
                {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
                    return new BitString((string)reader.Value);
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                }

                var jObject = JObject.Load(reader);

                var model = JsonConvert.DeserializeObject<BitStringModel>(
                    jObject.ToString(),
                    new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }
                );

#pragma warning disable CS8602 // Dereference of a possibly null reference.
                return new BitString(model.Value, model.BitLength);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }

#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BitString);
        }
    }
}
