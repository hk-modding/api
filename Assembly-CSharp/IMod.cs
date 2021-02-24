using System.Collections.Generic;
using JetBrains.Annotations;
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
        ///     Denotes if the running version is the current version.  Set this with <see cref="GithubVersionHelper" />
        /// </summary>
        /// <returns>If the version is current or not.</returns>
        bool IsCurrent();

        /// <summary>
        ///     Controls when this mod should load compared to other mods.  Defaults to ordered by name.
        /// </summary>
        /// <returns></returns>
        int LoadPriority();
    }
}