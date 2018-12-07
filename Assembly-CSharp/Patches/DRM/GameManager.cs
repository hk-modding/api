using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using MonoMod;
//using MonoMod;
using UnityEngine;
using UnityEngine.SceneManagement;

using MonoModIgnore = MonoMod.MonoModIgnore;
using MonoModReplace = MonoMod.MonoModReplace;
using MonoModOriginalName = MonoMod.MonoModOriginalName;

// ReSharper disable all
//We don't care about XML docs for these as they are being patched into the original code
#pragma warning disable 1591, 0108, 0169, 0649, 114, 0414,0162, IDE1005, IDE1006
namespace Modding.Patches
{
    //These are flat out copied from the game's decompiled source.  We tried doing IL edits, but it was so complicated as to make it not worth it.  If there ever is an easy way to decompile a method, get it as c#, edit, and recompile in monomod, we can remove this.
    public partial class GameManager
    {

        #region SaveGame
        [MonoModIgnore] private GameCameras gameCams;
        [MonoModIgnore] private float sessionPlayTimer;
        [MonoModIgnore] private float sessionStartTime;

        [MonoModIgnore] private extern void UpdateSessionPlayTime();
        [MonoModIgnore] private extern int CheckOldBackups(ref List<string> backupFiles, string backUpSaveSlotPath, bool removeOldest = false);
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
		    			this.playerData.version = "1.4.2.4";
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
        [MonoModIgnore] public event GameManager.UnloadLevel UnloadingLevel;

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


#pragma warning disable 1587
	    ///This allows a mod (or anything else) to "queue" a scene transition
        ///TODO: After some testing without this, see if we should put it back in
#pragma warning restore 1587
        //[MonoModOriginalName( "BeginSceneTransitionRoutine" )]
        //public void orig_BeginSceneTransitionRoutine( GameManager.SceneLoadInfo info ) { }
        //[MonoModReplace]
        //private IEnumerator BeginSceneTransitionRoutine( GameManager.SceneLoadInfo info )
        //{
        //    //this will allow 
        //    while( sceneLoad != null )
        //    {
        //        Debug.LogErrorFormat( this, "Cannot scene transition to {0}, while a scene transition is in progress", new object[]
        //        {
        //             info.SceneName
        //        } );
        //        yield return new WaitForEndOfFrame();
        //    }
        //    orig_BeginSceneTransitionRoutine( info );            
        //    yield break;
        //}
    }
}
