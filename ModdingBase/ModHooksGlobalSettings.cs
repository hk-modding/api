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
            get => IntValues.ContainsKey(nameof(LoggingLevel)) ? (LogLevel)IntValues[nameof(LoggingLevel)] : LogLevel.Info;
            set
            {
                if (IntValues.ContainsKey(nameof(LoggingLevel)))
                    IntValues[nameof(LoggingLevel)] = (int)value;
                else
                    IntValues.Add(nameof(LoggingLevel), (int)value);
            }
        }
    }
}
