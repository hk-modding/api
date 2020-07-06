using MonoMod;

// ReSharper disable All
#pragma warning disable 1591, CS0649, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::ChangeFontByLanguage")]
    public class ChangeFontByLanguage
    {
        public extern void orig_SetFont();

        public void SetFont()
        {
            orig_SetFont();
            ModHooks.Instance.OnSetFont();
        }
    }
}
