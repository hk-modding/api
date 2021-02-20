using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Modding.Converters
{
    public class Vector2Converter : JsonConverter<Vector2>
    {
        public override Vector2 ReadJson(Dictionary<string, object> token, object existingValue)
        {
            float x = Convert.ToSingle(token["x"]);
            float y = Convert.ToSingle(token["y"]);
            return new Vector2(x, y);
        }

        public override void WriteJson(JsonWriter writer, Vector2 value)
        {
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
        }
    }
}
