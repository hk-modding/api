using System.Collections.Generic;
using MonoMod;
using UnityEngine;

// ReSharper disable All
#pragma warning disable 1591, CS0649

namespace Modding.Patches
{
    [MonoModPatch("global::Language.Language")]
    public static class Language
    {
        [MonoModIgnore]
        private static Dictionary<string, Dictionary<string, string>> currentEntrySheets;

        public static string GetInternal(string key, string sheetTitle)
        {
            if (currentEntrySheets == null || !currentEntrySheets.ContainsKey(sheetTitle))
            {
                Debug.LogError($"The sheet with title \"{sheetTitle}\" does not exist!");
                return string.Empty;
            }

            if (currentEntrySheets[sheetTitle].ContainsKey(key))
            {
                return currentEntrySheets[sheetTitle][key];
            }

            return "#!#" + key + "#!#";
        }

        public static string Get(string key, string sheetTitle)
        {
            return ModHooks.Instance.LanguageGet(key, sheetTitle);
        }
    }
}