using System;
using System.Numerics;
using Newtonsoft.Json;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Math.JsonConverters
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public class BigIntegerConverter : JsonConverter
    {
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        {
            var bigint = (BigInteger)value;

            writer.WriteValue(new BitString(bigint).ToHex());
        }

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        {
            // Use the hex representation constructor
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
            return new BitString((string)reader.Value).ToPositiveBigInteger();
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BigInteger);
        }
    }
}
