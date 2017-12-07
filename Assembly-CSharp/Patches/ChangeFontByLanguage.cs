using MonoMod;
//We don't care about XML docs for these as they are being patched into the original code
// ReSharper disable All
#pragma warning disable 1591
#pragma warning disable CS0649

namespace Modding.Patches
{
    [MonoModPatch("global::ChangeFontByLanguage")]
    public class ChangeFontByLanguage
    {
        public void orig_SetFont() { }

        public void SetFont()
        {
            orig_SetFont();
            ModHooks.Instance.OnSetFont();
        }
    }
}
