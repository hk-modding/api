using System;
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

        /// <inheritdoc />
        /// <summary>
        ///     Returns the objects to preload in order for the mod to work.
        /// </summary>
        /// <returns>A List of tuples containing asset file name, object name, asset type</returns>
        List<(int, string, Type)> GetPreloadAssetsNames();

        /// <summary>
        ///     Called after preloading of all mods.
        /// </summary>
        /// <param name="preloadedObjects">The preloaded objects relevant to this <see cref="Mod" /></param>
        void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects);

        /// <summary>
        ///     Called after preloading of all mods.
        /// </summary>
        /// <param name="preloadedObjects">The preloaded objects relevant to this <see cref="Mod" /></param>
        /// <param name="preloadedAssets">The preloaded assets relevant to this <see cref="Mod" /></param>
        void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects, Dictionary<int, Dictionary<string, UnityEngine.Object>> preloadedAssets);

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