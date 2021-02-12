using MonoMod;
using System;
using UnityEngine;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 114, 0414,0162, CS0626, IDE1005, IDE1006

namespace Modding.Patches
{
    // These changes fix NREs that happen in this class when pre-processing scenes without a hero in them
    [MonoModPatch("global::PersonalObjectPool")]
    public class PersonalObjectPool : global::PersonalObjectPool
    {
        private extern void orig_OnEnable();
        private void OnEnable()
        {
            try
            {
                orig_OnEnable();
            }
            catch (NullReferenceException) when (!ModLoader.Preloaded)
            {}
        }
    }
}