using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Modding.Converters
{
    /// <inheritdoc />
    public abstract class JsonConverter<TClass> : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return typeof(TClass) == objectType;
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (typeof(TClass) == objectType)
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

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            WriteJson(writer, (TClass) value);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Read from token 
        /// </summary>
        /// <param name="token">JSON object</param>
        /// <param name="existingValue">Existing value</param>
        /// <returns></returns>
        [PublicAPI]
        public abstract TClass ReadJson(Dictionary<string, object> token, object existingValue);
        
        /// <summary>
        /// Write value into token
        /// </summary>
        /// <param name="writer">JSON Writer</param>
        /// <param name="value">Value to be written</param>
        [PublicAPI]
        public abstract void WriteJson(JsonWriter writer, TClass value);
    }
}
