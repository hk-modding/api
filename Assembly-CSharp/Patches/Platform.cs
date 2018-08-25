using MonoMod;

#pragma warning disable 1591
namespace Modding.Patches
{
    [MonoModPatch("global::Platform")]
    public abstract class Platform : global::Platform
    {
        protected extern string orig_GetSaveSlotFileName(int slotIndex, SaveSlotFileNameUsage usage);

        public static bool IsSaveSlotIndexValid(int slotIndex) => true;
        
        // ReSharper disable once UnusedMember.Global
        protected string GetSaveSlotFileName(int slotIndex, SaveSlotFileNameUsage usage)
        {
            string saveFileName = ModHooks.Instance.GetSaveFileName(slotIndex);
            ModHooks.ModLog("[API] - " + saveFileName);
            return string.IsNullOrEmpty(saveFileName)
                ? orig_GetSaveSlotFileName(slotIndex, usage) 
                : saveFileName;
        }
    }
}