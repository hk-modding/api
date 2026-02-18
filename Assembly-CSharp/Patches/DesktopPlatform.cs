using System;
using MonoMod;

// ReSharper disable all
#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::DesktopPlatform")]
    public class DesktopPlatform : global::DesktopPlatform
    {
        [Obsolete("Please update your mod to the new HK version and use `RoamingSharedData` instead")]
        public ISharedData EncryptedSharedData
        {
            get { return RoamingSharedData; }
        }
    }
}