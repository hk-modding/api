using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Modding
{
    /// <summary>
    /// An interface that signifies that the mod will save data into a save file.
    /// </summary>
    /// <typeparam name="S">The type representing the settings.</typeparam>
    public interface ILocalSettings<S>
    {
        /// <summary>
        /// Called when the mod just loaded the save data.
        /// </summary>
        /// <param name="s">The settings the mod loaded.</param>
        public void OnLoadLocal(S s);
        /// <summary>
        /// Called when the mod needs to save data.
        /// </summary>
        /// <returns>The settings to be stored.</returns>
        public S OnSaveLocal();
    }

    /// <summary>
    /// An interface that signifies that the mod will save global data.
    /// </summary>
    /// <typeparam name="S">The type representing the settings.</typeparam>
    public interface IGlobalSettings<S>
    {
        /// <summary>
        /// Called when the mod just loaded the global settings.
        /// </summary>
        /// <param name="s">The settings the mod loaded.</param>
        public void OnLoadGlobal(S s);
        /// <summary>
        /// Called when the mod needs to save the global settings.
        /// </summary>
        /// <returns>The settings to be stored.</returns>
        public S OnSaveGlobal();
    }

    internal class ModSavegameData
    {
        public Dictionary<string, string> loadedMods;
        public Dictionary<string, JToken> modData = new();
    }
}