using System;

namespace Modding
{
    /// <inheritdoc />
    /// <summary>
    ///     Represents a Dictionary of Strings that can be serialized with Unity's JsonUtil
    /// </summary>
    [Serializable]
    public class SerializableStringDictionary : SerializableDictionary<string, string> { }
}