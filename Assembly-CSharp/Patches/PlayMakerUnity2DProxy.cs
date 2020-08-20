using MonoMod;
using UnityEngine;

// ReSharper disable All
#pragma warning disable 1591, CS0108

namespace Modding.Patches
{
    [MonoModPatch("global::PlayMakerUnity2DProxy")]
    public class PlayMakerUnity2DProxy : global::PlayMakerUnity2DProxy
    {
        public void Start()
        {
            if (!PlayMakerUnity2d.isAvailable())
            {
                Debug.LogError
                (
                    "PlayMakerUnity2DProxy requires the 'PlayMaker Unity 2D' Prefab in the Scene.\nUse the menu 'PlayMaker/Addons/Unity 2D/Components/Add PlayMakerUnity2D to Scene' to correct the situation",
                    this
                );
                enabled = false;
                return;
            }

            ModHooks.Instance.OnColliderCreate(gameObject);
            RefreshImplementation();
        }
    }
}