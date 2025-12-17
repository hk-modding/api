using System.Collections.Generic;
using Modding;
using TeamCherry.Localization;
using UnityEngine;
using UObject = UnityEngine.Object;
using TLLanguage = TeamCherry.Localization.Language;

namespace Language;

public static class Language
{
    public static void LoadLanguage() => TLLanguage.LoadLanguage();
    public static void LoadAvailableLanguages() => TLLanguage.LoadAvailableLanguages();
    public static string[] GetLanguages() => TLLanguage.GetLanguages();
    public static bool SwitchLanguage(string langCode) => TLLanguage.SwitchLanguage(langCode);
    public static bool SwitchLanguage(LanguageCode code) => TLLanguage.SwitchLanguage(code);
    public static UObject GetAsset(string name) => TLLanguage.GetAsset(name);
    public static LanguageCode CurrentLanguage() => TLLanguage.CurrentLanguage();
    public static string Get(string key) => TLLanguage.Get(key);
    //public static string Get(string key, string sheetTitle) => TLLanguage.Get(key, sheetTitle);
    public static IEnumerable<string> GetSheets() => TLLanguage.GetSheets();
    public static IEnumerable<string> GetKeys(string sheetTitle) => TLLanguage.GetKeys(sheetTitle);
    public static bool Has(string key) => TLLanguage.Has(key);
    public static bool Has(string key, string sheet) => TLLanguage.Has(key, sheet);
    public static bool HasSheet(string sheet) => TLLanguage.HasSheet(sheet);
    public static LanguageCode LanguageNameToCode(SystemLanguage name) => TLLanguage.LanguageNameToCode(name);

    public static string GetInternal(string key, string sheetTitle) => TLLanguage.Get(key, sheetTitle);
    public static string Get(string key, string sheetTitle)
    {
        return ModHooks.LanguageGet(key, sheetTitle);
    }
}
