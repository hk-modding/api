using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoMod;
using UnityEngine;

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
