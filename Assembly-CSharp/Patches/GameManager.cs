using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using MonoMod;
using UnityEngine;
using UnityEngine.SceneManagement;

//We don't care about XML docs for these as they are being patched into the original code
// ReSharper disable all
#pragma warning disable 1591
#pragma warning disable CS0108

namespace Modding.Patches
{
    [MonoModPatch("global::GameManager")]
    public class GameManager : global::GameManager {
        /*DRMDONE: SaveGame(int)
            Change string text4 = JsonUtility.ToJson(new SaveGameData(this.playerData, this.sceneData), !this.gameConfig.useSaveEncryption);
                    SaveGameData saveGameData = new SaveGameData(this.playerData, this.sceneData);
                    Modding.ModHooks.Instance.OnBeforeSaveGameSave(saveGameData);
                    string text4 = JsonUtility.ToJson(saveGameData, !this.gameConfig.useSaveEncryption);
                    Modding.Logger.APILogger.LogFine("About to Serialize Save Data\n" + text4);
            
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
            destScene = ModHooks.Instance.BeforeSceneLoad(destScene);
            orig_LoadScene(destScene);
            ModHooks.Instance.OnSceneChanged(destScene);
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
            ModHooks.Instance.OnBeforePlayerDead();
            yield return orig_PlayerDead(waitTime);
            ModHooks.Instance.OnAfterPlayerDead();
        }

        #region SaveGame
        [MonoModIgnore] private GameCameras gameCams;
        [MonoModIgnore] private float sessionPlayTimer;
        [MonoModIgnore] private float sessionStartTime;

        [MonoModIgnore] private extern void UpdateSessionPlayTime();
        [MonoModIgnore] private extern int  CheckOldBackups(ref List<string> backupFiles, string backUpSaveSlotPath, bool removeOldest = false);
        [MonoModIgnore] private extern void ResetGameTimer();
        [MonoModIgnore] private extern void ShowSaveIcon();
        [MonoModIgnore] private extern void HideSaveIcon();

        [MonoModReplace]
        public void SaveGame(int saveSlot, Action<bool> callback)
        {
            if (saveSlot >= 0)
            {
                this.SaveLevelState();
                if (!this.gameConfig.disableSaveGame)
                {
                    this.ShowSaveIcon();
                    if (this.achievementHandler != null)
                    {
                        this.achievementHandler.FlushRecordsToDisk();
                    }
                    else
                    {
                        Debug.LogError("Error saving achievements (PlayerAchievements is null)");
                    }
                    if (this.playerData != null)
                    {
                        this.playerData.playTime += this.sessionPlayTimer;
                        this.ResetGameTimer();
                        this.playerData.version = Constants.GAME_VERSION;
                        this.playerData.profileID = saveSlot;
                        this.playerData.CountGameCompletion();
                    }
                    else
                    {
                        Debug.LogError("Error updating PlayerData before save (PlayerData is null)");
                    }
                    try
                    {
                        SaveGameData obj = new SaveGameData(this.playerData, this.sceneData);
                        ModHooks.Instance.OnBeforeSaveGameSave(obj);
                        string text = JsonUtility.ToJson(obj);
                        bool flag = this.gameConfig.useSaveEncryption && !Platform.Current.IsFileSystemProtected;
                        if (flag)
                        {
                            string graph = Encryption.Encrypt(text);
                            BinaryFormatter binaryFormatter = new BinaryFormatter();
                            MemoryStream memoryStream = new MemoryStream();
                            binaryFormatter.Serialize(memoryStream, graph);
                            byte[] binary = memoryStream.ToArray();
                            memoryStream.Close();
                            Platform.Current.WriteSaveSlot(saveSlot, binary, delegate(bool didSave)
                            {
                                this.HideSaveIcon();
                                callback(didSave);
                            });
                        }
                        else
                        {
                            Platform.Current.WriteSaveSlot(saveSlot, Encoding.UTF8.GetBytes(text), delegate(bool didSave)
                            {
                                this.HideSaveIcon();
                                if (callback != null)
                                {
                                    callback(didSave);
                                }
                            });
                        }
                    }
                    catch (Exception arg)
                    {
                        Debug.LogError("GM Save - There was an error saving the game: " + arg);
                        this.HideSaveIcon();
                        if (callback != null)
                        {
                            CoreLoop.InvokeNext(delegate
                            {
                                callback(false);
                            });
                        }
                    }
                    ModHooks.Instance.OnSavegameSave(saveSlot);
                }
                else
                {
                    Debug.Log("Saving game disabled. No save file written.");
                    if (callback != null)
                    {
                        CoreLoop.InvokeNext(delegate
                        {
                            callback(false);
                        });
                    }
                }
            }
            else
            {
                Debug.LogError("Save game slot not valid: " + saveSlot);
                if (callback != null)
                {
                    CoreLoop.InvokeNext(delegate
                    {
                        callback(false);
                    });
                }
            }
        }
        #endregion

        #region LoadGame

        [MonoModReplace]
        public void LoadGame(int saveSlot, Action<bool> callback)
        {
            if (!Platform.IsSaveSlotIndexValid(saveSlot))
            {
                Debug.LogErrorFormat("Cannot load from invalid save slot index {0}", new object[]
                {
                    saveSlot
                });
                if (callback != null)
                {
                    CoreLoop.InvokeNext(delegate { callback(false); });
                }

                return;
            }

            Platform.Current.ReadSaveSlot(saveSlot, delegate(byte[] fileBytes)
            {
                bool obj;
                try
                {
                    bool flag = this.gameConfig.useSaveEncryption && !Platform.Current.IsFileSystemProtected;
                    string json;
                    if (flag)
                    {
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        MemoryStream serializationStream = new MemoryStream(fileBytes);
                        string encryptedString = (string) binaryFormatter.Deserialize(serializationStream);
                        json = Encryption.Decrypt(encryptedString);
                    }
                    else
                    {
                        json = Encoding.UTF8.GetString(fileBytes);
                    }

                    SaveGameData saveGameData = JsonUtility.FromJson<SaveGameData>(json);
                    global::PlayerData instance = saveGameData.playerData;
                    SceneData instance2 = saveGameData.sceneData;
                    global::PlayerData.instance = instance;
                    this.playerData = instance;
                    SceneData.instance = instance2;
                    ModHooks.Instance.OnAfterSaveGameLoad(saveGameData);
                    this.sceneData = instance2;
                    this.profileID = saveSlot;
                    this.inputHandler.RefreshPlayerData();
                    ModHooks.Instance.OnSavegameLoad(saveSlot);
                    obj = true;
                }
                catch (Exception ex)
                {
                    Debug.LogFormat("Error loading save file for slot {0}: {1}", new object[]
                    {
                        saveSlot,
                        ex
                    });
                    obj = false;
                }

                if (callback != null)
                {
                    callback(obj);
                }
            });
        }

        #endregion

        #region GetSaveStatsForSlot
        [MonoModReplace]
        public void GetSaveStatsForSlot(int saveSlot, Action<global::SaveStats> callback)
        {
            if (!Platform.IsSaveSlotIndexValid(saveSlot))
            {
                Debug.LogErrorFormat("Cannot get save stats for invalid slot {0}", new object[]
                {
                    saveSlot
                });
                if (callback != null)
                {
                    CoreLoop.InvokeNext(delegate
                    {
                        callback(null);
                    });
                }
                return;
            }
            Platform.Current.ReadSaveSlot(saveSlot, delegate(byte[] fileBytes)
            {
                if (fileBytes == null)
                {
                    if (callback != null)
                    {
                        CoreLoop.InvokeNext(delegate
                        {
                            callback(null);
                        });
                    }
                    return;
                }
                try
                {
                    bool flag = this.gameConfig.useSaveEncryption && !Platform.Current.IsFileSystemProtected;
                    string json;
                    if (flag)
                    {
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        MemoryStream serializationStream = new MemoryStream(fileBytes);
                        string encryptedString = (string)binaryFormatter.Deserialize(serializationStream);
                        json = Encryption.Decrypt(encryptedString);
                    }
                    else
                    {
                        json = Encoding.UTF8.GetString(fileBytes);
                    }
                    SaveGameData saveGameData = JsonUtility.FromJson<SaveGameData>(json);
                    global::PlayerData playerData = saveGameData.playerData;
                    SaveStats saveStats = new SaveStats(playerData.maxHealthBase, playerData.geo, playerData.mapZone,
                                                        playerData.playTime, playerData.MPReserveMax, playerData.permadeathMode, playerData.bossRushMode,
                                                        playerData.completionPercentage, playerData.unlockedCompletionRate);
                    if (callback != null)
                    {
                        CoreLoop.InvokeNext(delegate
                        {
                            callback(saveStats);
                        });
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(string.Concat(new object[]
                    {
                        "Error while loading save file for slot ",
                        saveSlot,
                        " Exception: ",
                        ex
                    }));
                    if (callback != null)
                    {
                        CoreLoop.InvokeNext(delegate
                        {
                            callback(null);
                        });
                    }
                }
            });

        }
        #endregion

        #region LoadSceneAdditive

        [MonoModIgnore] private bool tilemapDirty;
        [MonoModIgnore] private bool waitForManualLevelStart;
        [MonoModIgnore] public event GameManager.DestroyPooledObjects DestroyPersonalPools;
        [MonoModIgnore] public event GameManager.UnloadLevel          UnloadingLevel;

        [MonoModReplace]
        public IEnumerator LoadSceneAdditive(string destScene)
        {
            Debug.Log( "Loading "+destScene );
            destScene = ModHooks.Instance.BeforeSceneLoad(destScene);
            this.tilemapDirty = true;
            this.startedOnThisScene = false;
            this.nextSceneName = destScene;
            this.waitForManualLevelStart = true;
            if (this.DestroyPersonalPools != null)
            {
                this.DestroyPersonalPools();
            }
            if (this.UnloadingLevel != null)
            {
                this.UnloadingLevel();
            }
            string exitingScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            AsyncOperation loadop = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(destScene, LoadSceneMode.Additive);
            loadop.allowSceneActivation = true;
            yield return loadop;
            yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(exitingScene);
            ModHooks.Instance.OnSceneChanged(destScene);
            this.RefreshTilemapInfo(destScene);
            if( this.IsUnloadAssetsRequired( exitingScene, destScene ) )
            {
                Debug.LogFormat( this, "Unloading assets due to zone transition", new object[ 0 ] );
                yield return Resources.UnloadUnusedAssets();
            }
            GCManager.Collect();
            this.SetupSceneRefs( true );
            this.BeginScene();
            this.OnNextLevelReady();
            this.waitForManualLevelStart = false;
            Debug.Log( "Done Loading " + destScene );
            yield break;
        }
        #endregion

        #region LoadFirstScene
        [MonoModReplace]
        public IEnumerator LoadFirstScene()
        {
            yield return new WaitForEndOfFrame();
            this.OnWillActivateFirstLevel();
            this.LoadScene( "Tutorial_01" );
            ModHooks.Instance.OnNewGame();
            yield break;
        }
        #endregion


        #region OnWillActivateFirstLevel
        [MonoModOriginalName( "OnWillActivateFirstLevel" )]
        public void orig_OnWillActivateFirstLevel() { }
                
        public void OnWillActivateFirstLevel()
        {
            orig_OnWillActivateFirstLevel();
            ModHooks.Instance.OnNewGame();
        }
        #endregion

        
        [MonoModIgnore] private SceneLoad sceneLoad;

        ///This will allow modders to access the scene loader. Note that if there's no transition in progress, it will be null!
        ///Example use case: Start a co-routine that checks for an non null sceneLoad then hooks up a callback to the "Finish" delegate to do something when the game has completed loading a scene.
        [MonoModIgnore] public SceneLoad SceneLoad {
            get {
                return sceneLoad;
            }
        }
    }
}
