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

        private void Awake()
        {
            if (ModLoader.modLoadState == ModLoader.ModLoadState.NotStarted)
            {
                Logger.APILogger.Log("Main menu loading");
                ModLoader.modLoadState = ModLoader.ModLoadState.Started;

                GameObject obj = new GameObject();
                DontDestroyOnLoad(obj);

                // Preload reflection
                new Thread(ReflectionHelper.PreloadCommonTypes).Start();

                // NonBouncer does absolutely nothing, which makes it a good dummy to run the loader
                obj.AddComponent<NonBouncer>().StartCoroutine(ModLoader.LoadModsInit(obj));
            }
            else
            {
                // Debug log because this is the expected code path
                Logger.APILogger.LogDebug($"OnScreenDebugInfo: Already begun mod loading (state {ModLoader.modLoadState})");
            }

            orig_Awake();
        }
    }
}