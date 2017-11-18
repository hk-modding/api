using MonoMod;
using UnityEngine;
//We disable a bunch of warnings here because they don't mean anything.  They all relate to not finding proper stuff for methods/properties/fields that are stubs to make the new methods work.
//We don't care about XML docs for these as they are being patched into the original code
// ReSharper disable All
#pragma warning disable 1591
#pragma warning disable CS0108

namespace Modding.Patches
{
    [MonoModPatch("global::PlayMakerUnity2DProxy")]
    public class PlayMakerUnity2DProxy : global::PlayMakerUnity2DProxy
    {
        public void Start()
        {
            if (!PlayMakerUnity2d.isAvailable())
            {
                Debug.LogError("PlayMakerUnity2DProxy requires the 'PlayMaker Unity 2D' Prefab in the Scene.\nUse the menu 'PlayMaker/Addons/Unity 2D/Components/Add PlayMakerUnity2D to Scene' to correct the situation", this);
                enabled = false;
                return;
            }
            ModHooks.Instance.OnColliderCreate(gameObject);
            RefreshImplementation();
        }
    }
}
