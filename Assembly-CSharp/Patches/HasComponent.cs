using MonoMod;
using UnityEngine;

#pragma warning disable 1591

namespace Modding.Patches
{
    // ReSharper disable once UnusedMember.Global
    [MonoModPatch("global::HutongGames.PlayMaker.Actions.HasComponent")]
    public class HasComponent : global::HutongGames.PlayMaker.Actions.HasComponent
    {
        [MonoModIgnore]
        [RemoveOp(23)]
        [ReplaceMethod
            (
                "UnityEngine.GameObject, UnityEngine",
                "GetComponent",
                new[] {"System.Type, mscorlib"},
                "UnityEngine.GameObject, UnityEngine",
                "GetComponent",
                new[] {"System.String, mscorlib"}
            )
        ]
        // ReSharper disable once UnusedMember.Local
        private extern void DoHasComponent(GameObject go);
    }
}