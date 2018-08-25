using System;
using System.Collections;
using MonoMod;
//We don't care about XML docs for these as they are being patched into the original code
#pragma warning disable 1591
#pragma warning disable CS0108

namespace Modding.Patches
{
    [MonoModPatch("global::GameManager")]
    public partial class GameManager : global::GameManager
    {

        /*DRMDONE: SaveGame(int)
            Change string text4 = JsonUtility.ToJson(new SaveGameData(this.playerData, this.sceneData), !this.gameConfig.useSaveEncryption);
                    SaveGameData saveGameData = new SaveGameData(this.playerData, this.sceneData);
                    Modding.ModHooks.Instance.OnBeforeSaveGameSave(saveGameData);
                    string text4 = JsonUtility.ToJson(saveGameData, !this.gameConfig.useSaveEncryption);
                    Modding.Logger.LogFine("[API] - About to Serialize Save Data\n" + text4);
            
            Add this right before the return after the try:
                  Modding.ModHooks.Instance.OnSavegameSave(saveSlot);
        */

        /*DRMDONE: LoadGame(int)
           Add this right after SceneData instance2 = saveGameData.sceneData;
                Modding.ModHooks.Instance.OnAfterSaveGameLoad(saveGameData);
           Add this right after this.inputHandler.RefreshPlayerData();
                Modding.ModHooks.Instance.OnSavegameLoad(saveSlot);
        */
        public void orig_OnApplicationQuit() { }

        public void OnApplicationQuit()
        {
            orig_OnApplicationQuit();
            ModHooks.Instance.OnApplicationQuit();
        }

        public void orig_LoadScene(string destScene) { }

        public void LoadScene(string destScene)
        {
            Logger.Log( "Entered LoadScene!" );
            destScene = ModHooks.Instance.BeforeSceneLoad(destScene);
            orig_LoadScene(destScene);
            ModHooks.Instance.OnSceneChanged(destScene);
            Logger.Log( "Finished LoadScene!" );
        }

        public void orig_ClearSaveFile(int saveSlot, Action<bool> callback) { }

        public void ClearSaveFile(int saveSlot, Action<bool> callback)
        {
            ModHooks.Instance.OnSavegameClear(saveSlot);
            orig_ClearSaveFile(saveSlot, callback);
            ModHooks.Instance.OnAfterSaveGameClear(saveSlot);
        }

        public IEnumerator orig_PlayerDead(float waitTime) { yield break; }

        public IEnumerator PlayerDead(float waitTime)
        {
            Logger.Log( "Entered player dead!" );
            ModHooks.Instance.OnBeforePlayerDead();
            yield return orig_PlayerDead(waitTime);
            ModHooks.Instance.OnAfterPlayerDead();
            Logger.Log( "Finished player dead!" );
        }
    }
}
