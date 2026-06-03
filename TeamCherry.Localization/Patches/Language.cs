using MonoMod;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable all
#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::TeamCherry.Localization.Language")]
    public static class Language
    {
        [MonoModIgnore]
        private static Dictionary<string, Dictionary<string, string>> _currentEntrySheets;

        [MonoModAdded]
        public static string GetInternal(string key, string sheetTitle)
        {
            if (_currentEntrySheets == null || !_currentEntrySheets.ContainsKey(sheetTitle))
            {
                Debug.LogError($"The sheet with title \"{sheetTitle}\" does not exist!");
                return string.Empty;
            }

            if (_currentEntrySheets[sheetTitle].ContainsKey(key))
            {
                return _currentEntrySheets[sheetTitle][key];
            }

            return "#!#" + key + "#!#";
        }

        [MonoModReplace]
        public static string Get(string key, string sheetTitle)
        {
            if (LanguageGet != null)
                return LanguageGet(key, sheetTitle);
            return GetInternal(key, sheetTitle);
        }

        [MonoModAdded]
        public delegate string LanguageGetFunc(string key, string sheetTitle);
        [MonoModAdded]
        public static LanguageGetFunc LanguageGet = null;
    }
}
