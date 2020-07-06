using MonoMod;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0414, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::SceneColorManager")]
    public class SceneColorManager : global::SceneColorManager
    {
        [MonoModIgnore]
        private bool gameplayScene;

        private extern void orig_UpdateScriptParameters();

        // Added checks for null and an attempt to fix any missing references
        private void UpdateScriptParameters()
        {
            if (this.gameplayScene)
            {
                if (HeroController.instance == null || HeroController.instance.heroLight == null)
                {
                    return;
                }
            }

            orig_UpdateScriptParameters();
        }
    }
}