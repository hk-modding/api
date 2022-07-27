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
    }
}