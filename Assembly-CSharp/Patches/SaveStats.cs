using System.Collections.Generic;
using GlobalEnums;
using MonoMod;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0414

namespace Modding.Patches
{
    [MonoModPatch("global::SaveStats")]
    public class SaveStats : global::SaveStats
    {
        public Dictionary<string, string> LoadedMods { get; set; }
        public string                     Name       { get; set; }

        [MonoModIgnore]
        public SaveStats
        (
            int maxHealth,
            int geo,
            MapZone mapZone,
            float playTime,
            int maxMPReserve,
            int permadeathMode,
            bool bossRushMode,
            float completionPercentage,
            bool unlockedCompletionRate
        ) : base
        (
            maxHealth,
            geo,
            mapZone,
            playTime,
            maxMPReserve,
            permadeathMode,
            bossRushMode,
            completionPercentage,
            unlockedCompletionRate
        ) { }
    }
}