using MonoMod;
//We don't care about XML docs for these as they are being patched into the original code
// ReSharper disable All
#pragma warning disable 1591
namespace Modding.Patches
{
    [MonoModPatch("global::SaveGameData")]
    public class SaveGameData : global::SaveGameData
    {
        public ModSettingsDictionary modData;

        [MonoModIgnore]
        public SaveGameData(global::PlayerData playerData, SceneData sceneData) : base(playerData, sceneData)
        {
        }

        public SerializableStringDictionary LoadedMods;

        public string Name;
    }
}
