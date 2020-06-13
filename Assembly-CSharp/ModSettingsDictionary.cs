using System;
using System.Linq;
using UnityEngine;

namespace Modding
{
    /// <inheritdoc cref="SerializableDictionary{TKey,TValue}" />
    /// <summary>
    ///     Used to represent Mod Data in SaveGameData
    /// </summary>
    [Serializable]
    public class ModSettingsDictionary : SerializableDictionary<string, ModSettings>, ISerializationCallbackReceiver
    {
        /// <inheritdoc />
        /// <summary>
        ///     Occurs before serialization
        /// </summary>
        public new void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();

            foreach (ModSettings settings in Values)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (!(settings is ISerializationCallbackReceiver callbackReceiver))
                {
                    continue;
                }

                try
                {
                    callbackReceiver.OnBeforeSerialize();
                }
                catch (Exception e)
                {
                    Logger.APILogger.LogError("" + e);
                }
            }
        }
    }
}