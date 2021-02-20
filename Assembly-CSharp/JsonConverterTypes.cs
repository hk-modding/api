using System.Collections.Generic;
using Newtonsoft.Json;
using Modding.Converters;

namespace Modding
{
    public static class JsonConverterTypes
    {
        public static List<JsonConverter> ConverterTypes { get; private set; }

        static JsonConverterTypes()
        {
            ConverterTypes = new List<JsonConverter>()
            {
                new Vector2Converter(),
                new Vector3Converter()
            };
        }
    }
}
