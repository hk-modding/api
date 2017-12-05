using UnityEngine.UI;
using MonoMod;
//We don't care about XML docs for these as they are being patched into the original code
// ReSharper disable All
#pragma warning disable 1591
#pragma warning disable CS0649

namespace Modding.Patches
{
    [MonoModPatch("global::TMPro.TMP_Text")]
    public class TMP_Text : MaskableGraphic
    {
        [MonoModIgnore]
        protected bool m_isRightToLeft;

        public bool isRightToLeftText
        {
            get
            {
                bool dir = ModHooks.Instance.GetTextDirection(m_isRightToLeft);
                isRightToLeftText = dir;
                return dir;
            }
            [MonoModIgnore]
            set { }
        }
    }
}
