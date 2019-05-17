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
}