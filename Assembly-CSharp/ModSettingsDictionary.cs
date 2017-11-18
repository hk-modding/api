using System;

namespace Modding
{
    /// <inheritdoc />
    /// <summary>
    /// Used to represent Mod Data in SaveGameData
    /// </summary>
	[Serializable]
	public class ModSettingsDictionary : SerializableDictionary<string, IModSettings>
	{
	}
}
