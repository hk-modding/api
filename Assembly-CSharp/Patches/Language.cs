// ReSharper disable All
#pragma warning disable 1591

namespace Modding.Patches
{
    // Language class moved to TeamCherry.Localization.dll in Unity 6.
    // Hooking is now done via MonoMod.RuntimeDetour in ModHooks.InitLanguageHook().
    internal static class Language
    {
        internal static string GetInternal(string key, string sheetTitle)
        {
            return TeamCherry.Localization.Language.Get(key, sheetTitle);
        }
    }
}

// Backward-compatibility shim: mods compiled against Unity 5 API reference
// [Assembly-CSharp]Language.Language. We inject this stub so those mods
// don't get TypeLoadException at runtime.
namespace Language
{
    public static class Language
    {
        public static string Get(string key, string sheetTitle)
            => TeamCherry.Localization.Language.Get(key, sheetTitle);

        public static bool Has(string key, string sheetTitle)
            => TeamCherry.Localization.Language.Has(key, sheetTitle);
    }
}
