using MonoMod;
//We don't care about XML docs for these as they are being patched into the original code
// ReSharper disable All
#pragma warning disable 1591
namespace Modding.Patches
{
    [MonoModPatch("global::OnScreenDebugInfo")]
    public class OnScreenDebugInfo : global::OnScreenDebugInfo
    {
        private void orig_Awake() { }

        private void Awake()
        {
            Logger.Log("GameLoading");
            ModLoader.LoadMods();
            orig_Awake();
        }
    }
}
