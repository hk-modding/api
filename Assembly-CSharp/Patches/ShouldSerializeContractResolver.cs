using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Modding.Patches
{
    /// <inheritdoc />
    public class ShouldSerializeContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// Instance to cache reflection calls.
        /// </summary>
        public static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();
        
        /// <inheritdoc />
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty prop = base.CreateProperty(member, memberSerialization);

            if (member?.DeclaringType?.Assembly.FullName.StartsWith("UnityEngine") ?? false)
                prop.Ignored = true;
            
            return prop;
        }
    }
}