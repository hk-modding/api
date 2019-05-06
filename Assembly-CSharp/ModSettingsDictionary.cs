using System;
using UnityEngine;

namespace Modding
{
    /// <inheritdoc cref="SerializableDictionary{TKey,TValue}" />
    /// <summary>
    /// Used to represent Mod Data in SaveGameData
    /// </summary>
    [Serializable]
    public class ModSettingsDictionary : SerializableDictionary<string, IModSettings>, ISerializationCallbackReceiver
    {
        /// <inheritdoc />
        /// <summary>
        /// Occurs before serialization
        /// </summary>
        public new void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();

            foreach (IModSettings settings in Values)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (settings is ISerializationCallbackReceiver callbackReceiver)
                {
                    callbackReceiver.OnBeforeSerialize();
                }
            }
        }
    }
}
