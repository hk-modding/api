using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Modding
{
    /// <summary>
    ///     Class to hold GlobalSettings for the Modding API
    /// </summary>
    [PublicAPI]
    public class ModHooksGlobalSettings
    {
        // now used to serialize and deserialize the save data. Not updated until save.
        [JsonProperty]
        internal Dictionary<string, bool> ModEnabledSettings = new();

        /// <summary>
        ///     Logging Level to use.
        /// </summary>
        public LogLevel LoggingLevel = LogLevel.Info;

        /// <summary>
        ///     Determines if the logs should have a short log level instead of the full name.
        /// </summary>
        public bool ShortLoggingLevel;

        /// <summary>
        ///     Determines if the logs should have a timestamp attached to each line of logging.
        /// </summary>
        public bool IncludeTimestamps;

        /// <summary>
        ///     All settings related to the the in game console
        /// </summary>
        public InGameConsoleSettings ConsoleSettings = new();

        /// <summary>
        ///     Determines if Debug Console (Which displays Messages from Logger) should be shown.
        /// </summary>
        public bool ShowDebugLogInGame;

        /// <summary>
        ///     Determines for the preloading how many different scenes should be loaded at once.
        /// </summary>
        public int PreloadBatchSize = 5;

        /// <summary>
        ///     When enabled, listens for filesystem changes to mod DLLs.
        ///     On modification, mods will be unloaded, and the new copy of the assembly gets loaded as well.
        ///     <para><b>Limitations:</b></para>
        ///     <list type="bullet">
        ///         <item><description>
        ///             The old assembly does not get unloaded. If you have created any unity game objects or components,
        ///             make sure to Destroy them in the mod's Unload function.
        ///         </description></item>k
        ///         <item><description>
        ///             Dependencies of mods cannot be hot reloaded. When you modify a dependency DLL, no change will be made to the mod depending on it.
        ///         </description></item>
        ///         <item><description>
        ///             <c>Assembly.location</c> will return an empty string for hot-reloaded mods
        ///         </description></item>
        ///     </list>
        /// </summary>
        public bool EnableHotReload = false;

        /// <summary>
        ///     Maximum number of days to preserve modlogs for.
        /// </summary>
        public int ModlogMaxAge = 7;
    }
}
