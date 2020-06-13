namespace Modding
{
    /// <inheritdoc />
    /// <summary>
    ///     Base class that allows other classes to have context specific logging
    /// </summary>
    public abstract class Loggable : ILogger
    {
        internal string ClassName;

        /// <summary>
        ///     Basic setup for Loggable.
        /// </summary>
        protected Loggable()
        {
            ClassName = GetType().Name;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Log at the fine/detailed level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        public void LogFine(string message)
        {
            Logger.LogFine(FormatLogMessage(message));
        }

        /// <inheritdoc />
        /// <summary>
        ///     Log at the fine/detailed level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        public void LogFine(object message)
        {
            Logger.LogFine(FormatLogMessage(message));
        }

        /// <inheritdoc />
        /// <summary>
        ///     Log at the debug level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        public void LogDebug(string message)
        {
            Logger.LogDebug(FormatLogMessage(message));
        }

        /// <inheritdoc />
        /// <summary>
        ///     Log at the debug level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        public void LogDebug(object message)
        {
            Logger.LogDebug(FormatLogMessage(message));
        }

        /// <inheritdoc />
        /// <summary>
        ///     Log at the info level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        public void Log(string message)
        {
            Logger.Log(FormatLogMessage(message));
        }

        /// <inheritdoc />
        /// <summary>
        ///     Log at the info level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        public void Log(object message)
        {
            Logger.Log(FormatLogMessage(message));
        }

        /// <inheritdoc />
        /// <summary>
        ///     Log at the warn level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        public void LogWarn(string message)
        {
            Logger.LogWarn(FormatLogMessage(message));
        }

        /// <inheritdoc />
        /// <summary>
        ///     Log at the warn level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        public void LogWarn(object message)
        {
            Logger.LogWarn(FormatLogMessage(message));
        }

        /// <inheritdoc />
        /// <summary>
        ///     Log at the error level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        public void LogError(string message)
        {
            Logger.LogError(FormatLogMessage(message));
        }

        /// <inheritdoc />
        /// <summary>
        ///     Log at the error level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        public void LogError(object message)
        {
            Logger.LogError(FormatLogMessage(message));
        }

        /// <summary>
        ///     Formats a log message as "[TypeName] - Message"
        /// </summary>
        /// <param name="message">Message to be formatted.</param>
        /// <returns>Formatted Message</returns>
        private string FormatLogMessage(string message)
        {
            return $"[{ClassName}] - {message}".Replace("\n", $"\n[{ClassName}] - ");
        }

        /// <summary>
        ///     Formats a log message as "[TypeName] - Message"
        /// </summary>
        /// <param name="message">Message to be formatted.</param>
        /// <returns>Formatted Message</returns>
        private string FormatLogMessage(object message)
        {
            return FormatLogMessage(message?.ToString() ?? "null");
        }
    }
}