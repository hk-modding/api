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
        ///     - `false`: Load the entire scene unmodified into memory
        ///     - `true`: Preprocess the scenes by filtering to only the objects we care about.
        /// </summary>
        public string PreloadMode = "full-scene";

        /// <summary>
        ///     Maximum number of days to preserve modlogs for.
        /// </summary>
        public int ModlogMaxAge = 7;
    }
}
