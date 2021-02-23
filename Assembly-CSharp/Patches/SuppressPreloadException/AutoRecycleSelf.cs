using MonoMod;
using System;
using UnityEngine;
using System.Collections;
using GlobalEnums;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 114, 0414,0162, CS0626, IDE1005, IDE1006

namespace Modding.Patches
{
    // These changes fix NREs that happen in this class when pre-processing scenes without a hero in them
    [MonoModPatch("global::AutoRecycleSelf")]
    public class AutoRecycleSelf : global::AutoRecycleSelf
    {
        [MonoModIgnore]
        private AudioSource audioSource;
        [MonoModIgnore]
        private bool validAudioSource;
        [MonoModIgnore]
        private extern IEnumerator StartTimer(float wait);
        [MonoModIgnore]
        private extern void RecycleSelf();

        private void OnEnable()
		{
			if (this.afterEvent == AfterEvent.TIME)
            {
                if (this.timeToWait > 0f)
                {
                    base.StartCoroutine(this.StartTimer(this.timeToWait));
                }
            }
            else if (this.afterEvent == AfterEvent.LEVEL_UNLOAD)
            {
                if (GameManager.instance != null)
                {
                    GameManager.instance.DestroyPersonalPools += this.RecycleSelf;
                }
            }
            else if (this.afterEvent == AfterEvent.AUDIO_CLIP_END)
            {
                this.audioSource = base.GetComponent<AudioSource>();
                if (this.audioSource == null)
                {
                    Debug.LogError(base.name + " requires an AudioSource to auto-recycle itself.");
                    this.validAudioSource = false;
                }
                else
                {
                    this.validAudioSource = true;
                }
            }
        }

        private extern void orig_OnDisable();
        private void OnDisable()
        {
            try
            {
                orig_OnDisable();
            }
            catch (NullReferenceException) when (!ModLoader.Preloaded)
            { }
        }
    }
}