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
            if (ModLoader.LoadState == ModLoader.ModLoadState.NotStarted)
            {
                Logger.APILogger.Log("Main menu loading");
                ModLoader.LoadState = ModLoader.ModLoadState.Started;

                GameObject obj = new GameObject("Mod Loader");
                DontDestroyOnLoad(obj);

                // Preload reflection
                new Thread(ReflectionHelper.PreloadCommonTypes).Start();

                obj.AddComponent<ModLoaderObject>().StartCoroutine(ModLoader.LoadModsInit(obj));
            }
            else
            {
                // Debug log because this is the expected code path
                Logger.APILogger.LogDebug($"OnScreenDebugInfo: Already begun mod loading (state {ModLoader.LoadState})");
            }

            orig_Awake();
        }
    }
}