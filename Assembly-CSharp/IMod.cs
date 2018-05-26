namespace Modding
{
    /// <inheritdoc />
    /// <summary>
    /// Base interface for Mods
    /// </summary>
    public interface IMod : ILogger
    {

        /// <summary>
        /// Get's the Mod's Name
        /// </summary>
        /// <returns></returns>
        string GetName();

        /// <summary>
        /// Called when class is first constructed.
        /// </summary>
        void Initialize();

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
        /// Controls when this mod should load compared to other mods.  Defaults to ordered by name.
        /// </summary>
        /// <returns></returns>
        int LoadPriority();
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
