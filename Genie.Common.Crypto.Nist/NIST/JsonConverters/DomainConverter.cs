﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NIST.CVP.ACVTS.Libraries.Math.Domain;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Math.JsonConverters
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    /// <summary>
    /// Used to properly serialize/deserialize the <see cref="MathDomain"/> object.
    /// </summary>
    public class DomainConverter : JsonConverter
    {
        private readonly IRandom800_90 _random = new Random800_90();

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        {
            MathDomain md = (MathDomain)value;

            writer.WriteStartArray();

            foreach (var segment in md.DomainSegments)
            {
                var valueSegment = segment as ValueDomainSegment;
                if (valueSegment != null)
                {
                    writer.WriteValue(valueSegment.RangeMinMax.Minimum);
                }
                var rangeSegment = segment as RangeDomainSegment;
                if (rangeSegment != null)
                {
                    var minMax = rangeSegment.RangeMinMax;

                    writer.WriteStartObject();
                    writer.WritePropertyName("min");
                    writer.WriteValue(minMax.Minimum);
                    writer.WritePropertyName("max");
                    writer.WriteValue(minMax.Maximum);
                    writer.WritePropertyName("increment");
                    writer.WriteValue(minMax.Increment);
                    writer.WriteEndObject();
                }
            }

            writer.WriteEndArray();
        }

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        {
            JArray jArray;
            try
            {
                jArray = JArray.Load(reader);
            }
            catch (Exception)
            {
#pragma warning disable CS8603 // Possible null reference return.
                return null;
#pragma warning restore CS8603 // Possible null reference return.
            }

            // Create target object based on JObject
            MathDomain target = Create(jArray);

            return target;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(MathDomain);
        }

        private class RangeDomainModel
        {
            public int Min { get; set; }
            public int Max { get; set; }
            public int Increment { get; set; } = 1;
        }

        private MathDomain Create(JArray jArray)
        {
            MathDomain domain = new MathDomain();

            foreach (var item in jArray)
            {
                if (int.TryParse(item.ToString(), out var parsedValue))
                {
                    domain.AddSegment(new ValueDomainSegment(parsedValue));
                }
                else
                {
                    var model = JsonConvert.DeserializeObject<RangeDomainModel>(
                        item.ToString(),
                        new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }
                    );

#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    domain.AddSegment(new RangeDomainSegment(_random, model.Min, model.Max, model.Increment));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                }
            }

            return domain;
        }
    }
}
