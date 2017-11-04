using System;
using System.Collections.Generic;

namespace Modding
{
    /// <summary>
    /// Custom Mod Data
    /// </summary>
	[Serializable]
	public class ModData
	{
        /// <summary>
        /// Collection of all mods' settings
        /// </summary>
		public List<IModSettings> Settings;
	}
}
