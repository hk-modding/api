using JetBrains.Annotations;

// ReSharper disable file UnusedMember.Global
// ReSharper disable file UnusedMemberInSuper.Global

namespace Modding
{
    /// <summary>
    ///     Logging Utility
    /// </summary>
    [PublicAPI]
    public interface ILogger
    {
        /// <summary>
        ///     Log at the info level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void Log(string message);

        /// <summary>
        ///     Log at the info level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void Log(object message);

        /// <summary>
        ///     Log at the debug level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogDebug(string message);

        /// <summary>
        ///     Log at the debug level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogDebug(object message);

        /// <summary>
        ///     Log at the error level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogError(string message);

        /// <summary>
        ///     Log at the error level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogError(object message);

        /// <summary>
        ///     Log at the fine level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogFine(string message);

        /// <summary>
        ///     Log at the fine level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogFine(object message);

        /// <summary>
        ///     Log at the warn level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogWarn(string message);

        /// <summary>
        ///     Log at the warn level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogWarn(object message);
    }
}