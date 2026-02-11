using System;
using System.Collections.Generic;
using UnityEngine;
using MonoMod;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0414, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::SceneManager")]
    public class SceneManager : global::SceneManager
    {
        [MonoModIgnore]
        private bool gameplayScene;

        [MonoModIgnore]
        private HeroController heroCtrl;

        [MonoModIgnore]
        private bool heroInfoSent;

        // [MonoModIgnore]
        private extern void orig_Update();

        [MonoModIgnore]
        private GameManager gm;

        //Added checks for null and an attempt to fix any missing references
        // [MonoModReplace]
        private void Update()
        {
            if (this.gameplayScene)
            {
                if (!this.heroInfoSent && this.heroCtrl != null && (this.heroCtrl.heroLight == null || this.heroCtrl.heroLight.material == null))
                {
                    this.heroCtrl.SetDarkness(this.darknessLevel);
                    this.heroInfoSent = true;
                }
            }

            orig_Update();
        }

        [MonoModIgnore]
        private Transform borderLeft;

        [MonoModIgnore]
        private Transform borderRight;

        [MonoModIgnore]
        private Transform borderUp;

        [MonoModIgnore]
        private Transform borderDown;

        // [MonoModIgnore]
        private extern void orig_OnCameraAspectChanged(float aspect);

        //add modhook to send the newly created borders to any mods that want them
        // [MonoModReplace]
        private void OnCameraAspectChanged(float aspect)
        {
            orig_OnCameraAspectChanged(aspect);

            List<GameObject> borders = new List<GameObject>();
            if (this.borderLeft != null)
            {
                borders.Add(this.borderLeft.gameObject);
            }
            if (this.borderRight != null)
            {
                borders.Add(this.borderRight.gameObject);
            }
            if (this.borderUp != null)
            {
                borders.Add(this.borderUp.gameObject);
            }
            if (this.borderDown != null)
            {
                borders.Add(this.borderDown.gameObject);
            }
            ModHooks.OnDrawBlackBorders(borders);
        }

        // [MonoModIgnore]
        private extern void orig_Start();

        // [MonoModReplace]
        private void Start()
        {
            try
            {
                orig_Start();
            }
            catch (NullReferenceException) when (!ModLoader.LoadState.HasFlag(ModLoader.ModLoadState.Preloaded))
            { }
        }
    }
}