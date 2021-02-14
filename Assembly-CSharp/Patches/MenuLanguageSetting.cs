using System.Collections.Generic;
using GlobalEnums;
using MonoMod;
using UnityEngine;

// ReSharper disable All
#pragma warning disable 1591, CS0649, CS0436

namespace Modding.Patches
{
    [MonoModPatch("global::UnityEngine.UI.MenuLanguageSetting")]
    public class MenuLanguageSetting : UnityEngine.UI.MenuLanguageSetting
    {
        [MonoModIgnore]
        private SupportedLanguages[] langs;

        private void orig_RefreshAvailableLanguages() { }

        private void RefreshAvailableLanguages()
        {
            orig_RefreshAvailableLanguages();

            List<SupportedLanguages> finalLangs = new List<SupportedLanguages>();

            foreach (var l in langs)
            {
                if ((TextAsset)Resources.Load("Languages/" + l.ToString() + "_General", typeof(TextAsset)) != null)
                {
                    finalLangs.Add(l);
                }
            }

            langs = finalLangs.ToArray();
        }
    }
}