namespace Modding
{
    /// <summary>
    /// Base interface for Mods
    /// </summary>
    public interface IMod
    {
        /// <summary>
        /// Called when class is first constructed.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Called during game unload.
        /// </summary>
        void Unload();

        /// <summary>
        /// Returns version of Mod
        /// </summary>
        /// <returns>Mod Version</returns>
        string GetVersion();

        /// <summary>
        /// Denotes if the running version is the current version.  Set this with <see cref="GithubVersionHelper"/>
        /// </summary>
        /// <returns>If the version is current or not.</returns>
        bool IsCurrent();


        /// <summary>
        /// Log at the info level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void Log(string message);

        /// <summary>
        /// Log at the debug level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogDebug(string message);

        /// <summary>
        /// Log at the error level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogError(string message);

        /// <summary>
        /// Log at the fine level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogFine(string message);

        /// <summary>
        /// Log at the warn level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogWarn(string message);
    }

    /// <inheritdoc />
    /// <summary>
    /// Generic implementation of Mod which allows for settings
    /// </summary>
    /// <typeparam name="T">Implementation of <see cref="IModSettings"/></typeparam>
    public interface IMod<T> : IMod where T : IModSettings
	{
		/// <summary>
		/// Settings For the Mod that would be saved with the save file.
		/// </summary>
		T Settings { get; set; }
	}

    /// <inheritdoc />
    /// <summary>
    /// Generic implementation of Mod which allows for settings
    /// </summary>
    /// <typeparam name="T">Implementation of <see cref="IModSettings"/></typeparam>
    /// <typeparam name="TG">Implementation of <see cref="IModSettings"/></typeparam>
    public interface IMod<T,TG> : IMod where T : IModSettings where TG : IModSettings
    {
        /// <summary>
        /// Settings For the Mod that would be saved with the save file.
        /// </summary>
        T Settings { get; set; }

        /// <summary>
        /// Global Settings which are stored independently of saves.
        /// </summary>
        TG GlobalSettings { get; set; }
    }
}
