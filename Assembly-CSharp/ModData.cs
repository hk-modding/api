using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Modding
{
    /// <summary>
    /// Custom Mod Data
    /// </summary>
    [PublicAPI]
    [Serializable]
    public class ModData
    {
        /// <summary>
        /// Collection of all mods' settings
        /// </summary>
        public List<IModSettings> Settings;
    }
}
