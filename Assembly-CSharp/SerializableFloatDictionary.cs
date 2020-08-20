using System;

namespace Modding
{
    /// <inheritdoc />
    /// <summary>
    ///     Represents a Dictionary of Floats that can be serialized with Unity's JsonUtil
    /// </summary>
    [Serializable]
    public class SerializableFloatDictionary : SerializableDictionary<string, float> { }
}