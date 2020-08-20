using MonoMod;
using UnityEngine;

// ReSharper disable All
#pragma warning disable 1591, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::NailSlash")]
    public class NailSlash : global::NailSlash
    {
        private extern void orig_OnTriggerEnter2D(Collider2D otherCollider);

        private void OnTriggerEnter2D(Collider2D otherCollider)
        {
            ModHooks.Instance.OnSlashHit(otherCollider, gameObject);
            orig_OnTriggerEnter2D(otherCollider);
        }
    }
}