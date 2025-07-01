using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{
    /// <inheritdoc />
    /// <summary>
    ///     Base interface for Mods
    /// </summary>
    public interface IMod : ILogger
    {
        /// <summary>
        ///     Get's the Mod's Name
        /// </summary>
        /// <returns></returns>
        string GetName();

        /// <summary>
        ///     Returns the objects to preload in order for the mod to work.
        /// </summary>
        /// <returns>A List of tuples containing scene name, object name</returns>
        List<(string, string)> GetPreloadNames();
        
        /// <summary>
        /// A list of requested scenes to be preloaded and actions to execute on loading of those scenes
        /// </summary>
        /// <returns>List of tuples containg scene names and the respective actions.</returns>
        (string, Func<IEnumerator>)[] PreloadSceneHooks();

        /// <summary>
        /// This function will be invoked on each gameObject preloaded through the <see cref="GetPreloadNames"/> system.
        /// </summary>
        /// <param name="go">The preloaded gameObject.</param>
        /// <param name="sceneName">The scene the gameObject was preloaded from.</param>
        /// <param name="goName">The path to the preloaded gameObject.</param>
        void InvokeOnGameObjectPreloaded(GameObject go, string sceneName, string goName);


        /// <summary>
        ///     Called after preloading of all mods.
        /// </summary>
        /// <param name="preloadedObjects">The preloaded objects relevant to this <see cref="Mod" /></param>
        void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects);

        /// <summary>
        ///     Returns version of Mod
        /// </summary>
        /// <returns>Mod Version</returns>
        string GetVersion();

        /// <summary>
        ///     Controls when this mod should load compared to other mods.  Defaults to ordered by name.
        /// </summary>
        /// <returns></returns>
        int LoadPriority();

        /// <summary>
        ///     Returns the text that should be displayed on the mod menu button, if there is one.
        /// </summary>
        /// <returns></returns>
        string GetMenuButtonText();
    }

    internal static class IModExtensions
    {
        public static string GetVersionSafe(this IMod mod, string returnOnError)
        {
            try
            {
                return mod.GetVersion();
            }
            catch (Exception ex)
            {
                Logger.APILogger.LogError($"Error determining version for {mod.GetName()}\n" + ex);
                return returnOnError;
            }
        }
    }
}