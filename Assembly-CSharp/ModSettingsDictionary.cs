using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{
    /// <summary>
    ///     Used to represent Mod Data in SaveGameData
    /// </summary>
    [Serializable]
    public class ModSettingsDictionary : Dictionary<string, ModSettings>, ISerializationCallbackReceiver
    {
        /// <inheritdoc />
        public void OnBeforeSerialize()
        {
            foreach (ModSettings settings in Values)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (settings is not ISerializationCallbackReceiver callbackReceiver)
                    continue;

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

        /// <inheritdoc />
        public void OnAfterDeserialize()
        {
            foreach (ModSettings settings in Values)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (settings is not ISerializationCallbackReceiver callbackReceiver)
                    continue;

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