using System;
using UnityEngine;

namespace Modding
{
    /// <inheritdoc />
    /// <summary>
    /// Used to represent Mod Data in SaveGameData
    /// </summary>
    [Serializable]
    public class ModSettingsDictionary : SerializableDictionary<string, IModSettings>, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Occurs before serialization
        /// </summary>
        public new void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();

            foreach (IModSettings settings in Values)
            {
                if (settings is ISerializationCallbackReceiver callbackReceiver)
                {
                    callbackReceiver.OnBeforeSerialize();
                }
            }
        }
    }
}
