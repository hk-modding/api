using System.Collections.Generic;
using MonoMod;
using UnityEngine;
// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0414,0626
namespace Modding.Patches
{
    [MonoModPatch("global::ObjectPool")]
    public static class ObjectPool
    {
        private static extern GameObject orig_Spawn(GameObject prefab, Transform parent, Vector3 position,
            Quaternion rotation);

        public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
        {
            GameObject obj = orig_Spawn(prefab, parent, position, rotation);
            return ModHooks.Instance.OnObjectPoolSpawn(obj);
        }
    }

}
