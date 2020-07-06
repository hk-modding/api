using MonoMod;

// ReSharper disable all
#pragma warning disable 1591, 108, 114

namespace Modding.Patches
{
    [MonoModPatch("global::Platform")]
    public abstract class Platform : global::Platform
    {
        public static bool IsSaveSlotIndexValid(int slotIndex) => true;

        // ReSharper disable once UnusedMember.Global
        protected string GetSaveSlotFileName(int slotIndex, SaveSlotFileNameUsage usage)
        {
            string text = slotIndex == 0 ? "user.dat" : $"user{slotIndex}.dat";

            string modhook = ModHooks.Instance.GetSaveFileName(slotIndex);

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