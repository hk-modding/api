using System.Collections.Generic;
using Newtonsoft.Json;
using Modding.Converters;

namespace Modding
{
    /// <summary>
    /// Wrapper over converters used for Unity types with JSON.NET
    /// </summary>
    public static class JsonConverterTypes
    {
        /// <summary>
        /// Converters used for serializing Unity vectors.
        /// </summary>
        public static List<JsonConverter> ConverterTypes { get; }

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
