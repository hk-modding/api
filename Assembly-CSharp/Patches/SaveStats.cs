using System.Collections.Generic;
using GlobalEnums;
using MonoMod;

namespace Modding.Patches
{
    [MonoModPatch("global::SaveStats")]
    public class SaveStats : global::SaveStats
    {
        
        public Dictionary<string, string> LoadedMods { get; set; }
        public string Name { get; set; }



        [MonoModIgnore]
        public SaveStats(int maxHealth, int geo, MapZone mapZone, float playTime, int maxMPReserve, int permadeathMode, float completionPercentage, bool unlockedCompletionRate) : base(maxHealth, geo, mapZone, playTime, maxMPReserve, permadeathMode, completionPercentage, unlockedCompletionRate)
        {
        }
    }
}
