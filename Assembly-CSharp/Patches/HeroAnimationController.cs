using System.Collections;
using GlobalEnums;
using MonoMod;
using UnityEngine;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0626, 414, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::HeroAnimationController")]
    public class HeroAnimationController : global::HeroAnimationController
    {
        [MonoModIgnore]
        private HeroControllerStates cState;

        [MonoModIgnore]
        private bool wasFacingRight;

        [MonoModIgnore]
        private extern void UpdateAnimation();

        [MonoModReplace]
        private void Update()
        {
            if (this.controlEnabled)
            {
                this.UpdateAnimation();
            }
            else if (this.cState.facingRight)
            {
                this.wasFacingRight = true;
            }
            else
            {
                this.wasFacingRight = false;
            }
        }
    }
}