namespace Modding
{
    public class ModHooksGlobalSettings : IModSettings
    {

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
