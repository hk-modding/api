using MonoMod;
using UnityEngine;
//We disable a bunch of warnings here because they don't mean anything.  They all relate to not finding proper stuff for methods/properties/fields that are stubs to make the new methods work.
//We don't care about XML docs for these as they are being patched into the original code
// ReSharper disable All
#pragma warning disable 1591
namespace Modding.Patches
{
    [MonoModPatch("global::NailSlash")]
    public class NailSlash : global::NailSlash
    {
        private void orig_OnTriggerEnter2D(Collider2D otherCollider) { }

        private void OnTriggerEnter2D(Collider2D otherCollider)
        {
            ModHooks.Instance.OnSlashHit(otherCollider, gameObject);
            orig_OnTriggerEnter2D(otherCollider);
        }
    }
}
