using System;
using MonoMod;

namespace Modding.Patches
{
    [MonoModCustomMethodAttribute(nameof(MonoMod.MonoModRules.Patch_RH_AddOffset))]
    class PatchRHAddOffsetAttribute : Attribute
    {
    }
}
