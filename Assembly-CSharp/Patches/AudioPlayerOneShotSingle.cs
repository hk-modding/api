using MonoMod;
using System;
using UnityEngine;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 114, 0414,0162, CS0626, IDE1005, IDE1006

namespace Modding.Patches
{
    // These changes fix NREs that happen in this class when pre-processing scenes without a hero in them
    [MonoModPatch("global::HutongGames.PlayMaker.Actions.AudioPlayerOneShotSingle")]
    public class AudioPlayerOneShotSingle : global::HutongGames.PlayMaker.Actions.AudioPlayerOneShotSingle
    {
        private extern void orig_DoPlayRandomClip();
        private void DoPlayRandomClip()
        {
            try
            {
                orig_DoPlayRandomClip();
            }
            catch (NullReferenceException) when (!ModLoader.Preloaded)
            {}
        }
    }
}
