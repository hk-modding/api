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
            orig_Awake();
        }
    }
}