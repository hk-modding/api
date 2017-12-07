using MonoMod;
using UnityEngine;
using TMPro;
//We don't care about XML docs for these as they are being patched into the original code
// ReSharper disable All
#pragma warning disable 1591
#pragma warning disable CS0649

namespace Modding.Patches
{
    [MonoModPatch("global::DialogueBox")]
    public class DialogueBox : MonoBehaviour
    {
        [MonoModIgnore]
        private TextMeshPro textMesh;

        private void orig_Start() { }

        private void Start()
        {
            orig_Start();
            textMesh.isRightToLeftText = ModHooks.Instance.GetTextDirection(false);
        }
    }
}
