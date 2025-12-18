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
    static Language()
    {
        TllType = Type.GetType("TeamCherry.Localization.Language, TeamCherry.Localization, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        TllLoadLanguage = TllType.GetMethod("LoadLanguage", BindingFlags.Public | BindingFlags.Static, null, Array.Empty<Type>(), Array.Empty<ParameterModifier>());
        TllLoadAvailableLanguages = TllType.GetMethod("LoadAvailableLanguages", BindingFlags.Public | BindingFlags.Static, null, Array.Empty<Type>(), Array.Empty<ParameterModifier>());
        TllGetLanguages = TllType.GetMethod("GetLanguages", BindingFlags.Public | BindingFlags.Static, null, Array.Empty<Type>(), Array.Empty<ParameterModifier>());
        TllSwitchLanguageStr = TllType.GetMethod("SwitchLanguage", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, Array.Empty<ParameterModifier>());
        TllSwitchLanguageLc = TllType.GetMethod("SwitchLanguage", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(TeamCherry.Localization.LanguageCode) }, Array.Empty<ParameterModifier>());
        TllGetAsset = TllType.GetMethod("GetAsset", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, Array.Empty<ParameterModifier>());
        TllCurrentLanguage = TllType.GetMethod("CurrentLanguage", BindingFlags.Public | BindingFlags.Static, null, Array.Empty<Type>(), Array.Empty<ParameterModifier>());
        TllGet1 = TllType.GetMethod("Get", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, Array.Empty<ParameterModifier>());
        TllGetSheets = TllType.GetMethod("GetSheets", BindingFlags.Public | BindingFlags.Static, null, Array.Empty<Type>(), Array.Empty<ParameterModifier>());
        TllGetKeys = TllType.GetMethod("GetKeys", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, Array.Empty<ParameterModifier>());
        TllHas1 = TllType.GetMethod("Has", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, Array.Empty<ParameterModifier>());
        TllHas2 = TllType.GetMethod("Has", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string), typeof(string) }, Array.Empty<ParameterModifier>());
        TllHasSheet = TllType.GetMethod("HasSheet", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, Array.Empty<ParameterModifier>());
        TllLanguageNameToCode = TllType.GetMethod("LanguageNameToCode", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(USystemLanguage) }, Array.Empty<ParameterModifier>());
        TllGet2 = TllType.GetMethod("Get", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string), typeof(string) }, Array.Empty<ParameterModifier>());
    }
    private static readonly Type TllType;

    private static readonly MethodInfo TllLoadLanguage;
    public static void LoadLanguage() => TllLoadLanguage.Invoke(null, Array.Empty<object>());

    private static readonly MethodInfo TllLoadAvailableLanguages;
    public static void LoadAvailableLanguages() => TllLoadAvailableLanguages.Invoke(null, Array.Empty<object>());

    private static readonly MethodInfo TllGetLanguages;
    public static string[] GetLanguages() => (string[])(TllGetLanguages.Invoke(null, Array.Empty<object>()));

    private static readonly MethodInfo TllSwitchLanguageStr;
    public static bool SwitchLanguage(string langCode) => (bool)(TllSwitchLanguageStr.Invoke(null, new object[] { langCode }));

    private static readonly MethodInfo TllSwitchLanguageLc;
    public static bool SwitchLanguage(TeamCherry.Localization.LanguageCode code) => (bool)(TllSwitchLanguageLc.Invoke(null, new object[] { code }));

    private static readonly MethodInfo TllGetAsset;
    public static UObject GetAsset(string name) => (UObject)(TllGetAsset.Invoke(null, new object[] { name }));

    private static readonly MethodInfo TllCurrentLanguage;
    public static TeamCherry.Localization.LanguageCode CurrentLanguage() => (TeamCherry.Localization.LanguageCode)(TllCurrentLanguage.Invoke(null, Array.Empty<object>()));

    private static readonly MethodInfo TllGet1;
    public static string Get(string key) => (string)(TllGet1.Invoke(null, new object[] { key }));

    private static readonly MethodInfo TllGetSheets;
    public static IEnumerable<string> GetSheets() => (IEnumerable<string>)(TllGetSheets.Invoke(null, Array.Empty<object>()));

    private static readonly MethodInfo TllGetKeys;
    public static IEnumerable<string> GetKeys(string sheetTitle) => (IEnumerable<string>)(TllGetKeys.Invoke(null, new object[] { sheetTitle }));

    private static readonly MethodInfo TllHas1;
    public static bool Has(string key) => (bool)(TllHas1.Invoke(null, new object[] { key }));

    private static readonly MethodInfo TllHas2;
    public static bool Has(string key, string sheet) => (bool)(TllHas2.Invoke(null, new object[] { key, sheet }));

    private static readonly MethodInfo TllHasSheet;
    public static bool HasSheet(string sheet) => (bool)(TllHasSheet.Invoke(null, new object[] { sheet }));

    private static readonly MethodInfo TllLanguageNameToCode;
    public static TeamCherry.Localization.LanguageCode LanguageNameToCode(USystemLanguage name) => (TeamCherry.Localization.LanguageCode)(TllLanguageNameToCode.Invoke(null, new object[] { name }));

    private static readonly MethodInfo TllGet2;
    public static string GetInternal(string key, string sheetTitle) => (string)(TllGet2.Invoke(null, new object[] { key, sheetTitle }));

    public static string Get(string key, string sheetTitle) => Modding.ModHooks.LanguageGet(key, sheetTitle);
}