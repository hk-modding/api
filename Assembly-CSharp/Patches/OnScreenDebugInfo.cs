using MonoMod;
using UnityEngine;

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
            Logger.Log("[API] - Main menu loading");

            GameObject obj = new GameObject();
            DontDestroyOnLoad(obj);

            // NonBouncer does absolutely nothing, which makes it a good dummy to run the loader
            obj.AddComponent<NonBouncer>().StartCoroutine(ModLoader.LoadMods(obj));

            orig_Awake();
        }
    }
}
