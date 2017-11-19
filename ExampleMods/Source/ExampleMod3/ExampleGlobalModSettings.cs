using Modding;

namespace ExampleMod3
{
    /// <summary>
    /// Example Mod's global settings.
    /// </summary>
    /// <remarks>These will be saved In the Save Folder as ModName.GlobalSettings.json</remarks>
    public class GlobalModSettings :IModSettings
    {
        //How many hits in between crits?  Defaults to 4 if never set before.
        public int CritCounter { get => GetInt(4); set => SetInt(value); }

        //How hard should crits be
        public float CritMultiplier { get => GetFloat(2f); set => SetFloat(value); }

    }
}
