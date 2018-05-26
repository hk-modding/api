using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{
    /// <inheritdoc cref="Dictionary{TKey,TValue}" />
    /// <inheritdoc cref="ISerializationCallbackReceiver"/>
    /// <summary>
    /// Represents a Dictionary of &lt;<see cref="!:TKey" />,<see cref="!:TValue" />&gt; that can be serialized with Unity's JsonUtil
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Occurse before something isserialized.
        /// </summary>
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        /// <summary>
        /// Occurs after the object was deserialized
        /// </summary>
        public void OnAfterDeserialize()
        {
            Clear();
            if (keys.Count != values.Count)
            {
                throw new Exception(
                    $"there are {keys.Count} keys and {values.Count} values after deserialization. Make sure that both key and value types are serializable.");
            }
            for (int i = 0; i < keys.Count; i++)
            {
                Add(keys[i], values[i]);
            }
        }

        [SerializeField]
        private List<TKey> keys = new List<TKey>();
        [SerializeField]
        private List<TValue> values = new List<TValue>();
    }
}
