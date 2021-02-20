using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Modding.Converters
{
    /// <inheritdoc />
    public class Vector3Converter : JsonConverter<Vector3>
    {
        /// <inheritdoc />
        public override Vector3 ReadJson(Dictionary<string, object> token, object existingValue)
        {
            float x = Convert.ToSingle(token["x"]);
            float y = Convert.ToSingle(token["y"]);
            float z = Convert.ToSingle(token["z"]);
            return new Vector3(x, y, z);
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, Vector3 value)
        {
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
        }
    }
}
