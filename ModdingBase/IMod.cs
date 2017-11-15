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
    }

    /// <inheritdoc />
    /// <summary>
    /// Generic implementation of Mod which allows for settings
    /// </summary>
    /// <typeparam name="T">Implementation of <see cref="IModSettings"/></typeparam>
    public interface IMod<T> : IMod where T : IModSettings
	{
		T Settings { get; set; }
	}
}
