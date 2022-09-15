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

        private extern void orig_Update();

        [MonoModIgnore]
        private GameManager gm;

        //Added checks for null and an attempt to fix any missing references
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

        //add modhook to send the newly created borders to any mods that want them
        [MonoModReplace]
        private void DrawBlackBorders()
        {
            List<GameObject> borders = new List<GameObject>();
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.borderPrefab);
            gameObject.transform.SetPosition2D(this.gm.sceneWidth + 10f, this.gm.sceneHeight / 2f);
            gameObject.transform.localScale = new Vector2(20f, this.gm.sceneHeight + 40f);
            borders.Add(gameObject);

            gameObject = UnityEngine.Object.Instantiate<GameObject>(this.borderPrefab);
            gameObject.transform.SetPosition2D(-10f, this.gm.sceneHeight / 2f);
            gameObject.transform.localScale = new Vector2(20f, this.gm.sceneHeight + 40f);
            borders.Add(gameObject);

            gameObject = UnityEngine.Object.Instantiate<GameObject>(this.borderPrefab);
            gameObject.transform.SetPosition2D(this.gm.sceneWidth / 2f, this.gm.sceneHeight + 10f);
            gameObject.transform.localScale = new Vector2(40f + this.gm.sceneWidth, 20f);
            borders.Add(gameObject);

            gameObject = UnityEngine.Object.Instantiate<GameObject>(this.borderPrefab);
            gameObject.transform.SetPosition2D(this.gm.sceneWidth / 2f, -10f);
            gameObject.transform.localScale = new Vector2(40f + this.gm.sceneWidth, 20f);
            borders.Add(gameObject);

            ModHooks.OnDrawBlackBorders(borders);
            
            foreach (var border in borders)
            {
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(border, base.gameObject.scene);
            }
        }

        private extern void orig_Start();
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