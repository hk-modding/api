using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Modding
{
    /// <inheritdoc />
    /// <summary>
    ///     Class to hold GlobalSettings for the Modding API
    /// </summary>
    [PublicAPI]
    [Serializable]
    public class ModHooksGlobalSettings : ModSettings
    {
        /// <summary>
        ///     Lists the known mods that are currently installed and whether or not they've been enabled or disabled via the Mod
        ///     Manager Menu.
        /// </summary>
        [SerializeField] public SerializableBoolDictionary ModEnabledSettings;

        /// <summary>
        ///     Logging Level to use.
        /// </summary>
        public LogLevel LoggingLevel = LogLevel.Info;

        /// <summary>
        ///     Determines if Debug Console (Which displays Messages from Logger) should be shown.
        /// </summary>
        public bool ShowDebugLogInGame;

        /// <summary>
        ///     Determines for the preloading how many different scenes should be loaded at once.
        /// </summary>
        public int PreloadBatchSize = 5;
    }
}