using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEngine;
using MonoMod;
using MonoMod.Cil;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0414, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::SceneManager")]
    public class SceneManager : global::SceneManager
    {
        [MonoModIgnore]
        [Attributes.RawIlPatch(nameof(IlPatches.SceneManager_Update))]
        extern private void Update();

        [MonoModIgnore]
        private Transform borderLeft;
        [MonoModIgnore]
        private Transform borderRight;
        [MonoModIgnore]
        private Transform borderUp;
        [MonoModIgnore]
        private Transform borderDown;

        //add modhook to send the newly created borders to any mods that want them
        private extern void orig_OnCameraAspectChanged(float aspect);
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

    public static partial class IlPatches
    {
        [MonoModIgnore]
        public static void SceneManager_Update(ILContext il)
        {
            // add a branch around `this.heroCtrl.heroLight.material.SetColor("_Color", Color.white);`
            ILCursor cursor = new ILCursor(il);

            ILLabel forAfterChecks = cursor.DefineLabel();

            cursor.GotoNext
            (
                MoveType.AfterLabel,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<global::SceneManager>("heroCtrl"),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<global::SceneManager>(nameof(global::SceneManager.darknessLevel)),
                x => x.MatchCallOrCallvirt<global::HeroController>(nameof(global::HeroController.SetDarkness))
            );
            forAfterChecks.Target = cursor.Next;

            cursor.GotoPrev
            (
                MoveType.AfterLabel,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<global::SceneManager>("heroCtrl"),
                x => x.MatchLdfld<global::HeroController>(nameof(global::HeroController.heroLight))
            );
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, ReflectionHelper.GetFieldInfo(typeof(global::SceneManager), "heroCtrl"));
            cursor.Emit(OpCodes.Ldfld, ReflectionHelper.GetFieldInfo(typeof(global::HeroController), nameof(global::HeroController.heroLight)));
            cursor.Emit(OpCodes.Ldnull);
            cursor.Emit(OpCodes.Call, ReflectionHelper.GetMethodInfo(typeof(global::UnityEngine.Object), "op_Equality", false));
            cursor.Emit(OpCodes.Brtrue_S, forAfterChecks);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, ReflectionHelper.GetFieldInfo(typeof(global::SceneManager), "heroCtrl"));
            cursor.Emit(OpCodes.Ldfld, ReflectionHelper.GetFieldInfo(typeof(global::HeroController), nameof(global::HeroController.heroLight)));
            cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetMethodInfo(typeof(global::UnityEngine.SpriteRenderer), "get_material", true));
            cursor.Emit(OpCodes.Ldnull);
            cursor.Emit(OpCodes.Call, ReflectionHelper.GetMethodInfo(typeof(global::UnityEngine.Object), "op_Equality", false));
            cursor.Emit(OpCodes.Brtrue_S, forAfterChecks);
        }
    }
}