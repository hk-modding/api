using System;
using MonoMod;

// ReSharper disable all
#pragma warning disable 1591, 108, 114

namespace Modding.Patches
{
    [MonoModPatch("global::Platform")]
    public abstract class Platform : global::Platform
    {
        [Obsolete("Please update your mod to the new HK version and use `RoamingSharedData` instead")]
        public ISharedData EncryptedSharedData
        {
            get { return RoamingSharedData; }
        }

        [MonoModReplace]
        public static bool IsSaveSlotIndexValid(int slotIndex) => true;

        // todo: this is the exact same as vanilla???
        // ReSharper disable once UnusedMember.Global
        [MonoModReplace]
        protected string GetSaveSlotFileName(int slotIndex, SaveSlotFileNameUsage usage)
        {
            string text = slotIndex == 0 ? "user.dat" : $"user{slotIndex}.dat";

            string modhook = ModHooks.GetSaveFileName(slotIndex);

            text = string.IsNullOrEmpty(modhook) ? text : modhook;

            switch (usage)
            {
                case SaveSlotFileNameUsage.Backup:
                    text += ".bak";
                    break;
                case SaveSlotFileNameUsage.BackupMarkedForDeletion:
                    text += ".del";
                    break;
            }

            return text;
        }
    }
}