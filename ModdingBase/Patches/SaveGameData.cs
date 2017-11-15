using MonoMod;

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
    }
}
