namespace Modding
{
    /// <inheritdoc />
    /// <summary>
    /// Class to hold GlobalSettings for the Modding API
    /// </summary>
    public class ModHooksGlobalSettings : IModSettings
    {

        /// <summary>
        /// Logging Level to use.
        /// </summary>
        public LogLevel LoggingLevel
        {
            get => (LogLevel) GetInt((int) LogLevel.Info);
            set => SetInt((int) value);
        }


        /// <summary>
        /// Determines if Debug Console (Which displays Messages from Logger) should be shown.
        /// </summary>
        public bool ShowDebugLogInGame
        {
            get => GetBool(false);
            set => SetBool(value);
        }
    }
}
