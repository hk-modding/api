using System;
using System.Collections.Generic;
using System.Reflection;
using UObject = UnityEngine.Object;
using USystemLanguage = UnityEngine.SystemLanguage;


namespace Language;

// for backwards compatibility
[MonoMod.MonoModLinkFrom("TeamCherry.Localization.Language")]
public static class Language
{
    public static void LoadLanguage() => COMPAT_LoadLanguage();
    public static void LoadAvailableLanguages() => COMPAT_LoadAvailableLanguages();
    public static string[] GetLanguages() => COMPAT_GetLanguages();
    public static bool SwitchLanguage(string langCode) => COMPAT_SwitchLanguage(langCode);
    public static bool SwitchLanguage(TeamCherry.Localization.LanguageCode code) => COMPAT_SwitchLanguage(code);
    public static UObject GetAsset(string name) => COMPAT_GetAsset(name);
    public static LanguageCode CurrentLanguage() => (LanguageCode) COMPAT_CurrentLanguage();
    public static string Get(string key) => COMPAT_Get(key);
    public static IEnumerable<string> GetSheets() => COMPAT_GetSheets();
    public static IEnumerable<string> GetKeys(string sheetTitle) => COMPAT_GetKeys(sheetTitle);
    public static bool Has(string key) => COMPAT_Has(key);
    public static bool Has(string key, string sheet) => COMPAT_Has(key, sheet);
    public static bool HasSheet(string sheet) => COMPAT_HasSheet(sheet);
    public static LanguageCode LanguageNameToCode(USystemLanguage name) => (LanguageCode) COMPAT_LanguageNameToCode(name);
    public static string GetInternal(string key, string sheetTitle) => COMPAT_Get(key, sheetTitle);
    public static string Get(string key, string sheetTitle) => Modding.ModHooks.LanguageGet(key, sheetTitle);

    // for backwards compatibility
    // KEEP THIS BELOW THE Language CLASS!!!
    [MonoMod.MonoModLinkTo("TeamCherry.Localization.Language", "System.Void LoadLanguage()")]
    [MonoMod.MonoModRemove]
    public static extern void COMPAT_LoadLanguage();
    [MonoMod.MonoModLinkTo("TeamCherry.Localization.Language", "System.Void LoadAvailableLanguages()")]
    [MonoMod.MonoModRemove]
    public static extern void COMPAT_LoadAvailableLanguages();
    [MonoMod.MonoModLinkTo("TeamCherry.Localization.Language", "System.String[] GetLanguages()")]
    [MonoMod.MonoModRemove]
    public static extern string[] COMPAT_GetLanguages();
    [MonoMod.MonoModLinkTo("TeamCherry.Localization.Language", "System.Boolean SwitchLanguage(System.String)")]
    [MonoMod.MonoModRemove]
    public static extern bool COMPAT_SwitchLanguage(string langCode);
    [MonoMod.MonoModLinkTo("TeamCherry.Localization.Language", "System.Boolean SwitchLanguage(TeamCherry.Localization.LanguageCode)")]
    [MonoMod.MonoModRemove]
    public static extern bool COMPAT_SwitchLanguage(TeamCherry.Localization.LanguageCode code);
    [MonoMod.MonoModLinkTo("TeamCherry.Localization.Language", "UnityEngine.Object GetAsset(System.String)")]
    [MonoMod.MonoModRemove]
    public static extern UObject COMPAT_GetAsset(string name);
    [MonoMod.MonoModLinkTo("TeamCherry.Localization.Language", "TeamCherry.Localization.LanguageCode CurrentLanguage()")]
    [MonoMod.MonoModRemove]
    public static extern TeamCherry.Localization.LanguageCode COMPAT_CurrentLanguage();
    [MonoMod.MonoModLinkTo("TeamCherry.Localization.Language", "System.String Get(System.String)")]
    [MonoMod.MonoModRemove]
    public static extern string COMPAT_Get(string key);
    [MonoMod.MonoModLinkTo("TeamCherry.Localization.Language", "System.String Get(System.String,System.String)")]
    [MonoMod.MonoModRemove]
    public static extern string COMPAT_Get(string key, string sheetTitle);
    [MonoMod.MonoModLinkTo("TeamCherry.Localization.Language", "System.Collections.Generic.IEnumerable`1<System.String> GetSheets()")]
    [MonoMod.MonoModRemove]
    public static extern IEnumerable<string> COMPAT_GetSheets();
    [MonoMod.MonoModLinkTo("TeamCherry.Localization.Language", "System.Collections.Generic.IEnumerable`1<System.String> GetKeys(System.String)")]
    [MonoMod.MonoModRemove]
    public static extern IEnumerable<string> COMPAT_GetKeys(string sheetTitle);
    [MonoMod.MonoModLinkTo("TeamCherry.Localization.Language", "System.Boolean Has(System.String)")]
    [MonoMod.MonoModRemove]
    public static extern bool COMPAT_Has(string key);
    [MonoMod.MonoModLinkTo("TeamCherry.Localization.Language", "System.Boolean Has(System.String,System.String)")]
    [MonoMod.MonoModRemove]
    public static extern bool COMPAT_Has(string key, string sheet);
    [MonoMod.MonoModLinkTo("TeamCherry.Localization.Language", "System.Boolean HasSheet(System.String)")]
    [MonoMod.MonoModRemove]
    public static extern bool COMPAT_HasSheet(string sheet);
    [MonoMod.MonoModLinkTo("TeamCherry.Localization.Language", "TeamCherry.Localization.LanguageCode LanguageNameToCode(UnityEngine.SystemLanguage)")]
    [MonoMod.MonoModRemove]
    public static extern TeamCherry.Localization.LanguageCode COMPAT_LanguageNameToCode(USystemLanguage name);
}