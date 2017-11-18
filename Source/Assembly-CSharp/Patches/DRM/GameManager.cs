using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using GlobalEnums;
using MonoMod;
using UnityEngine;
using UnityEngine.SceneManagement;

//We don't care about XML docs for these as they are being patched into the original code
#pragma warning disable 1591, 0108, 0169, 0649, 114, 0414,0162, 1005, 1006
namespace Modding.Patches
{
    //These are flat out copied from the game's decompiled source.  We tried doing IL edits, but it was so complicated as to make it not worth it.  If there ever is an easy way to decompile a method, get it as c#, edit, and recompile in monomod, we can remove this.
    public partial class GameManager
    {

        #region SaveGame
        [MonoModIgnore] private GameCameras gameCams;
        [MonoModIgnore] private float sessionTotalPlayTime;
        [MonoModIgnore] private float intervalStartTime;

        [MonoModIgnore] private extern void UpdateSessionPlayTime();
        [MonoModIgnore] private extern int CheckOldBackups(ref List<string> backupFiles, string backUpSaveSlotPath, bool removeOldest = false);

        [MonoModReplace]
        public void SaveGame(int saveSlot)
        {
            if (saveSlot >= 0)
            {
                if (this.gameCams.saveIcon != null)
                {
                    this.gameCams.saveIcon.SendEvent("GAME SAVED");
                }
                else
                {
                    GameObject gameObject = GameObject.FindGameObjectWithTag("Save Icon");
                    if (gameObject != null)
                    {
                        PlayMakerFSM playMakerFSM = FSMUtility.LocateFSM(gameObject, "Checkpoint Control");
                        if (playMakerFSM != null)
                        {
                            playMakerFSM.SendEvent("GAME SAVED");
                        }
                    }
                }
                this.SaveLevelState();
                if (!this.gameConfig.disableSaveGame)
                {
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
                        if (this.gameState != GameState.PAUSED)
                        {
                            this.UpdateSessionPlayTime();
                        }
                        this.playerData.playTime += this.sessionTotalPlayTime;
                        this.sessionTotalPlayTime = 0f;
                        this.intervalStartTime = Time.realtimeSinceStartup;
                        this.playerData.version = "1.2.2.1";
                        this.playerData.profileID = saveSlot;
                        this.playerData.CountGameCompletion();
                    }
                    else
                    {
                        Debug.LogError("Error updating PlayerData before save (PlayerData is null)");
                    }
                    string saveFilename = this.GetSaveFilename(saveSlot);
                    string text = Application.persistentDataPath + saveFilename;
                    string text2 = Application.persistentDataPath + saveFilename + ".bak";
                    int num = 3;
                    string[] files = Directory.GetFiles(Application.persistentDataPath);
                    List<string> list = new List<string>();
                    foreach (string text3 in files)
                    {
                        if (text3.Contains(text2))
                        {
                            list.Add(text3);
                        }
                    }
                    int num2 = this.CheckOldBackups(ref list, text2, false);
                    while (list.Count >= num)
                    {
                        num2 = this.CheckOldBackups(ref list, text2, true);
                    }
                    if (File.Exists(text))
                    {
                        try
                        {
                            if (File.Exists(text2 + num2))
                                File.Delete(text2 + num2);

                            File.Move(text, text2 + num2);
                        }
                        catch (Exception arg)
                        {
                            Debug.LogError("Unable to move save game to backup file: " + arg);
                        }
                    }
                    try
                    {
                        SaveGameData saveGameData = new SaveGameData(this.playerData, this.sceneData);
                        ModHooks.Instance.OnBeforeSaveGameSave(saveGameData);
                        string text4 = JsonUtility.ToJson(saveGameData, !this.gameConfig.useSaveEncryption);
                        ModHooks.Logger.LogFine("[API] - About to Serialize Save Data\n" + text4);
                        string graph = StringEncrypt.EncryptData(text4);
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        FileStream fileStream = File.Create(Application.persistentDataPath + saveFilename);
                        if (this.gameConfig.useSaveEncryption)
                        {
                            binaryFormatter.Serialize(fileStream, graph);
                        }
                        else
                        {
                            binaryFormatter.Serialize(fileStream, text4);
                        }
                        fileStream.Close();
                    }
                    catch (Exception arg2)
                    {
                        Debug.LogError("GM Save - There was an error saving the game: " + arg2);
                    }
                    Modding.ModHooks.Instance.OnSavegameSave(saveSlot);
                }
                else
                {
                    Debug.Log("Saving game disabled. No save file written.");
                }
            }
            else
            {
                Debug.LogError("Save game slot not valid: " + saveSlot);
            }
        }
        #endregion

        #region LoadGame

        [MonoModReplace]
        public bool LoadGame(int saveSlot)
        {
            if (saveSlot >= 0)
            {
                string saveFilename = this.GetSaveFilename(saveSlot);
                if (!string.IsNullOrEmpty(saveFilename) && File.Exists(Application.persistentDataPath + saveFilename))
                {
                    try
                    {
                        string toDecrypt = string.Empty;
                        string json = string.Empty;
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        FileStream fileStream = File.Open(Application.persistentDataPath + saveFilename, FileMode.Open);
                        if (this.gameConfig.useSaveEncryption)
                        {
                            toDecrypt = (string)binaryFormatter.Deserialize(fileStream);
                        }
                        else
                        {
                            json = (string)binaryFormatter.Deserialize(fileStream);
                        }
                        fileStream.Close();
                        if (this.gameConfig.useSaveEncryption)
                        {
                            json = StringEncrypt.DecryptData(toDecrypt);
                        }
                        SaveGameData saveGameData = JsonUtility.FromJson<SaveGameData>(json);
                        global::PlayerData instance = saveGameData.playerData;
                        SceneData instance2 = saveGameData.sceneData;
                        ModHooks.Instance.OnAfterSaveGameLoad(saveGameData);
                        global::PlayerData.instance = instance;
                        this.playerData = instance;
                        SceneData.instance = instance2;
                        this.sceneData = instance2;
                        this.profileID = saveSlot;
                        this.inputHandler.RefreshPlayerData();
				        ModHooks.Instance.OnSavegameLoad(saveSlot);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogFormat("Error loading save file for slot {0}: {1}", new object[]
                        {
                            saveSlot,
                            ex
                        });
                        return false;
                    }
                }
                Debug.Log("Save file not found for slot " + saveSlot);
                return false;
            }
            Debug.LogError("Save game slot not valid: " + saveSlot);
            return false;
        }

        #endregion

        #region LoadSceneAdditive

        [MonoModIgnore] private bool tilemapDirty;
        [MonoModIgnore] private bool waitForManualLevelStart;
        [MonoModIgnore] public event GameManager.DestroyPooledObjects DestroyPersonalPools;
        [MonoModIgnore] public event GameManager.UnloadLevel UnloadingLevel;
        [MonoModIgnore] public Scene nextScene { get; private set; }
        [MonoModIgnore] private extern void ManualLevelStart();
        [MonoModIgnore] public event GameManager.LevelReady NextLevelReady;

        [MonoModReplace]
        public IEnumerator LoadSceneAdditive(string destScene)
        {
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
            this.nextScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(destScene);
            AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(destScene, LoadSceneMode.Additive);
            asyncOperation.allowSceneActivation = true;
            yield return asyncOperation;
            UnityEngine.SceneManagement.SceneManager.UnloadScene(exitingScene);
            ModHooks.Instance.OnSceneChanged(destScene);
            this.RefreshTilemapInfo(destScene);
            this.ManualLevelStart();
            if (this.NextLevelReady != null)
            {
                this.NextLevelReady();
            }
            yield break;
            yield break;
        }


        #endregion

        #region LoadFirstScene
        [MonoModReplace]
        public IEnumerator LoadFirstScene()
        {
            yield return new WaitForEndOfFrame();
            this.entryGateName = "top1";
            this.SetState(GameState.PLAYING);
            this.ui.ConfigureMenu();
            this.LoadScene("Tutorial_01");
            ModHooks.Instance.OnNewGame();
            yield break;
            yield break;
        }
        
        #endregion

    }
}
