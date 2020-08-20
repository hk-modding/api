using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Modding
{
    /// <inheritdoc cref="Dictionary{TKey,TValue}" />
    /// <inheritdoc cref="ISerializationCallbackReceiver" />
    /// <summary>
    ///     Represents a Dictionary of &lt;<see cref="!:TKey" />,<see cref="!:TValue" />&gt; that can be serialized with
    ///     Unity's JsonUtil
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        // readonly kills JsonUtility
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        [FormerlySerializedAs("keys")]
        [SerializeField] 
        private List<TKey> _keys = new List<TKey>();

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        [FormerlySerializedAs("values")]
        [SerializeField] 
        private List<TValue> _values = new List<TValue>();
        
        /*
         * Note that OnBeforeSerialize and OnAfterDeserialize should not be needed.
         * I've left them here in the case of Json.NET failing to serialize for the JsonUtility fallback.
         */
        /// <summary>
        ///     Occurse before something is serialized.
        /// </summary>
        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();
            
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                _keys.Add(pair.Key);
                _values.Add(pair.Value);
            }
        }

        /// <summary>
        ///     Occurs after the object was deserialized
        /// </summary>
        public void OnAfterDeserialize()
        {
            Clear();
            
            if (_keys.Count != _values.Count)
            {
                throw new Exception(
                    $"there are {_keys.Count} keys and {_values.Count} values after deserialization. Make sure that both key and value types are serializable.");
            }

            for (int i = 0; i < _keys.Count; i++)
            {
                Add(_keys[i], _values[i]);
            }
        }
    }
}