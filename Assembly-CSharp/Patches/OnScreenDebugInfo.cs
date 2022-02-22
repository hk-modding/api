using System.Threading;
using MonoMod;
using UnityEngine;

// ReSharper disable All
#pragma warning disable 1591, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::OnScreenDebugInfo")]
    public class OnScreenDebugInfo : global::OnScreenDebugInfo
    {
        private extern void orig_Awake();

        private static bool StartedModLoading;

        private void Awake()
        {
            if (!StartedModLoading)
            {
                Logger.APILogger.Log("Main menu loading");
                StartedModLoading = true;

                GameObject obj = new GameObject();
                DontDestroyOnLoad(obj);

                // Preload reflection
                new Thread(ReflectionHelper.PreloadCommonTypes).Start();

                // NonBouncer does absolutely nothing, which makes it a good dummy to run the loader
                obj.AddComponent<NonBouncer>().StartCoroutine(ModLoader.LoadModsInit(obj));
            }
            else
            {
                Logger.APILogger.LogDebug("Already begun mod loading");
            }

            orig_Awake();
        }
    }
}