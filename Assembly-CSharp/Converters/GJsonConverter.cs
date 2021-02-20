using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Modding.Converters
{
    public abstract class JsonConverter<Tclass> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Tclass) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (typeof(Tclass) == objectType)
            {
                Dictionary<string, object> token = new Dictionary<string, object>();
                reader.Read();
                while (reader.TokenType == JsonToken.PropertyName)
                {
                    string name = (string) reader.Value;
                    // Value
                    reader.Read();
                    token.Add(name, reader.Value);
                    // JsonToken.PropertyName
                    reader.Read();
                }
                return ReadJson(token, existingValue);
            }
            return serializer.Deserialize(reader);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            WriteJson(writer, (Tclass) value);
            writer.WriteEndObject();

            //serializer.Serialize(writer, value);
        }

        public abstract Tclass ReadJson(Dictionary<string, object> token, object existingValue);
        public abstract void WriteJson(JsonWriter writer, Tclass value);
    }
}
