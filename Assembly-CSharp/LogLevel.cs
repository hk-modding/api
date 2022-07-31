using JetBrains.Annotations;

namespace Modding
{
    /// <summary>
    ///     What level should logs be done at?
    /// </summary>
    [PublicAPI]
    public enum LogLevel
    {
        /// <summary>
        ///     Finest Level of Logging - Developers Only
        /// </summary>
        Fine,

        /// <summary>
        ///     Debug Level of Logging - Mostly Developers Only
        /// </summary>
        Debug,

        /// <summary>
        ///     Normal Logging Level
        /// </summary>
        Info,

        /// <summary>
        ///     Only Show Warnings and Above
        /// </summary>
        Warn,

        /// <summary>
        ///     Only Show Full Errors
        /// </summary>
        Error,

        /// <summary>
        ///     No Logging at all
        /// </summary>
        [UsedImplicitly]
        Off
    }

    /// <summary>
    ///     Methods for the logging level enum
    /// </summary>
    public static class LogLevelExt
    {
        /// <summary>
        ///     Converts the logging level enum into a short string.
        /// </summary>
        /// <param name="level">The logging level</param>
        /// <returns>A 1 character string of the value of the enum</returns>
        public static string ToShortString(LogLevel level)
        {
            return level switch
            {
                LogLevel.Fine => "F",
                LogLevel.Debug => "D",
                LogLevel.Info => "I",
                LogLevel.Warn => "W",
                LogLevel.Error => "E",
                _ => ""
            };
        }
    }
}