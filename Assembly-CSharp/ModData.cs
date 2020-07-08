using System;
using System.Collections.Generic;
using JetBrains.Annotations;

// ReSharper disable file UnusedMember.Global

namespace Modding
{
    /// <summary>
    ///     Custom Mod Data
    /// </summary>
    [PublicAPI]
    [Serializable]
    public class ModDta
    {
        /// <summary>
        ///     Collection of all mods' settings
        /// </summary>
        public List<ModSettings> Settings;
    }
}