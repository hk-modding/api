using MonoMod;

// ReSharper disable All
#pragma warning disable 1591
#pragma warning disable CS0108, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::HeroControllerStates")]
    public class HeroControllerStates : global::HeroControllerStates
    {
        [MonoModReplace]
        public bool GetState(string stateName)
        {
            return ReflectionHelper.GetField<HeroControllerStates, bool, bool?>(this, stateName, null).GetValueOrDefault();
        }

        [MonoModReplace]
        public void SetState(string stateName, bool value)
        {
            ReflectionHelper.SetFieldSafe(this, stateName, value);
        }
    }
}
