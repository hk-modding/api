using MonoMod;

namespace Modding.Patches
{
    [MonoModPatch("global::OnScreenDebugInfo")]
    public class OnScreenDebugInfo : global::OnScreenDebugInfo
    {
        private void orig_Awake() { }

        private void Awake()
        {
            ModLoader.LoadMods();
            orig_Awake();
        }
    }
}
