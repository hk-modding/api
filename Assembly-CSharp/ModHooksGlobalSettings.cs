using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Modding
{
    /// <inheritdoc />
    /// <summary>
    ///     Class to hold GlobalSettings for the Modding API
    /// </summary>
    [PublicAPI]
    public class ModHooksGlobalSettings
    {
        /// <summary>
        ///     Lists the known mods that are currently installed and whether or not they've been enabled or disabled via the Mod
        ///     Manager Menu.
        /// </summary>
        public Dictionary<string, bool> ModEnabledSettings = new();

        /// <summary>
        ///     Logging Level to use.
        /// </summary>
        public LogLevel LoggingLevel = LogLevel.Info;

        /// <summary>
        ///		Wheter to use colors in the log console.
        /// </summary>
        public bool UseLogColors;

        /// <summary>
        ///		The color to use for Fine logging when UseLogColors is enabled
        /// </summary>
        public string FineColor = "grey";

        /// <summary>
        ///		The color to use for Info logging when UseLogColors is enabled
        /// </summary>
        public string InfoColor = "cyan";

        /// <summary>
        ///		The color to use for Debug logging when UseLogColors is enabled
        /// </summary>
        public string DebugColor = "white";

        /// <summary>
        ///		The color to use for Warning logging when UseLogColors is enabled
        /// </summary>
        public string WarningColor = "yellow";

        /// <summary>
        ///		The color to use for Error logging when UseLogColors is enabled
        /// </summary>
        public string ErrorColor = "red";

        /// <summary>
        ///		The color to use when UseLogColors is disabled
        /// </summary>
        public string DefaultColor = "white";

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
