using MonoMod;
using UnityEngine;
using TMPro;

// ReSharper disable All
#pragma warning disable 1591, CS0649

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