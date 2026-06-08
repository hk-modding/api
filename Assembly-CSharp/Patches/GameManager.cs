using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Mono.Cecil;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using Encryption = TeamCherry.SharedUtils.Encryption;

// ReSharper disable all
#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::GameManager")]
    public class GameManager : global::GameManager
    {
        private static string ModdedSavePath(int slot) =>
            Path.Combine
            (
                Application.persistentDataPath,
                $"user{slot}.modded.json"
            );

        private UIManager _uiInstance;

        public UIManager ui
        {
            get
            {
                if (_uiInstance == null) _uiInstance = (UIManager)UIManager.instance;
                return _uiInstance;
            }
            private set => _uiInstance = value;
        }

        private ModSavegameData moddedData;

        [MonoModIgnore]
        [Attributes.RawIlPatch(nameof(IlPatches.GameManager_OnApplicationQuit))]
        extern private void OnApplicationQuit();

        [MonoModIgnore]
        [Attributes.RawIlPatch(nameof(IlPatches.GameManager_LoadScene))]
        extern public void LoadScene(string destScene);

        [MonoModIgnore]
        [Attributes.RawIlPatch(nameof(IlPatches.GameManager_ClearSaveFile))]
        extern public void ClearSaveFile(int saveSlot, Action<bool> callback);

        [MonoModIgnore]
        [Attributes.IEnumeratorIlPatch(nameof(IlPatches.GameManager_PlayerDead))]
        extern public IEnumerator PlayerDead(float waitTime);

        [MonoModIgnore]
        [Attributes.IEnumeratorIlPatch(nameof(IlPatches.GameManager_LoadSceneAdditive))]
        extern public IEnumerator LoadSceneAdditive(string destScene);

        [MonoModIgnore]
        [Attributes.IEnumeratorIlPatch(nameof(IlPatches.GameManager_LoadFirstScene))]
        extern public IEnumerator LoadFirstScene();

        [MonoModIgnore]
        [Attributes.RawIlPatch(nameof(IlPatches.GameManager_OnWillActivateFirstLevel))]
        extern public void OnWillActivateFirstLevel();

        // il patch just dies trying to resolve types for no reason?
        public extern void orig_BeginSceneTransition(global::GameManager.SceneLoadInfo info);

        public void BeginSceneTransition(GameManager.SceneLoadInfo info)
        {
            info.SceneName = ModHooks.BeforeSceneLoad(info.SceneName);
            orig_BeginSceneTransition(info);
        }

        #region SaveGame & LoadGame

        [MonoModIgnore]
        private GameCameras gameCams;

        [MonoModIgnore]
        private float sessionPlayTimer;

        [MonoModIgnore]
        private float sessionStartTime;

        [MonoModIgnore]
        private extern void UpdateSessionPlayTime();

        [MonoModIgnore]
        private extern int CheckOldBackups(ref List<string> backupFiles, string backUpSaveSlotPath, bool removeOldest = false);

        [MonoModIgnore]
        private extern void ResetGameTimer();

        [MonoModIgnore]
        private extern void ShowSaveIcon();

        [MonoModIgnore]
        private extern void HideSaveIcon();

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
                        this.playerData.SetFloat(nameof(PlayerData.playTime), this.playerData.GetFloat(nameof(PlayerData.playTime)) + this.sessionPlayTimer);
                        this.ResetGameTimer();
                        this.playerData.SetString(nameof(PlayerData.version), Constants.GAME_VERSION);
                        this.playerData.SetInt(nameof(PlayerData.profileID), saveSlot);
                        this.playerData.CountGameCompletion();
                    }
                    else
                    {
                        Debug.LogError("Error updating PlayerData before save (PlayerData is null)");
                    }

                    try
                    {
                        SaveGameData obj = new SaveGameData(this.playerData, this.sceneData);

                        ModHooks.OnBeforeSaveGameSave(obj);
                        if (this.moddedData == null)
                        {
                            this.moddedData = new ModSavegameData();
                        }

                        ModHooks.OnSaveLocalSettings(this.moddedData);

                        // save modded data
                        try
                        {
                            var path = ModdedSavePath(saveSlot);
                            string modded = JsonConvert.SerializeObject
                            (
                                this.moddedData,
                                Formatting.Indented,
                                new JsonSerializerSettings
                                {
                                    ContractResolver = ShouldSerializeContractResolver.Instance,
                                    TypeNameHandling = TypeNameHandling.Auto,
                                    Converters = JsonConverterTypes.ConverterTypes
                                }
                            );
                            if (File.Exists(path + ".bak")) File.Delete(path + ".bak");
                            if (File.Exists(path)) File.Move(path, path + ".bak");
                            using FileStream fileStream = File.Create(path);
                            using var writer = new StreamWriter(fileStream);
                            writer.Write(modded);
                        }
                        catch (Exception e)
                        {
                            Logger.APILogger.LogError(e);
                        }

                        string text = null;

                        try
                        {
                            text = JsonConvert.SerializeObject
                            (
                                obj,
                                Formatting.Indented,
                                new JsonSerializerSettings()
                                {
                                    ContractResolver = ShouldSerializeContractResolver.Instance,
                                    TypeNameHandling = TypeNameHandling.Auto,
                                    Converters = JsonConverterTypes.ConverterTypes
                                }
                            );
                        }
                        catch (Exception e)
                        {
                            Logger.LogError("Failed to serialize save using Json.NET, trying fallback.");

                            Logger.APILogger.LogError(e);

                            // If this dies, not much we can do about it.
                            text = JsonUtility.ToJson(obj);
                        }

                        if (this.gameConfig.useSaveEncryption && !Platform.Current.IsFileSystemProtected)
                        {
                            string graph = Encryption.Encrypt(text);
                            BinaryFormatter binaryFormatter = new BinaryFormatter();
                            MemoryStream memoryStream = new MemoryStream();
                            binaryFormatter.Serialize(memoryStream, graph);
                            byte[] binary = memoryStream.ToArray();
                            memoryStream.Close();
                            Platform.Current.WriteSaveSlot
                            (
                                saveSlot,
                                binary,
                                delegate(bool didSave)
                                {
                                    this.HideSaveIcon();
                                    callback(didSave);
                                }
                            );
                        }
                        else
                        {
                            Platform.Current.WriteSaveSlot
                            (
                                saveSlot,
                                Encoding.UTF8.GetBytes(text),
                                delegate(bool didSave)
                                {
                                    this.HideSaveIcon();
                                    if (callback != null)
                                    {
                                        callback(didSave);
                                    }
                                }
                            );
                        }
                    }
                    catch (Exception arg)
                    {
                        Debug.LogError("GM Save - There was an error saving the game: " + arg);
                        this.HideSaveIcon();
                        if (callback != null)
                        {
                            CoreLoop.InvokeNext(delegate { callback(false); });
                        }
                    }

                    ModHooks.OnSavegameSave(saveSlot);
                }
                else
                {
                    Debug.Log("Saving game disabled. No save file written.");
                    if (callback != null)
                    {
                        CoreLoop.InvokeNext(delegate { callback(false); });
                    }
                }
            }
            else
            {
                Debug.LogError("Save game slot not valid: " + saveSlot);
                if (callback != null)
                {
                    CoreLoop.InvokeNext(delegate { callback(false); });
                }
            }
        }

        [MonoModReplace]
        public void LoadGame(int saveSlot, Action<bool> callback)
        {
            if (!Platform.IsSaveSlotIndexValid(saveSlot))
            {
                Debug.LogErrorFormat
                (
                    "Cannot load from invalid save slot index {0}",
                    new object[]
                    {
                        saveSlot
                    }
                );
                if (callback != null)
                {
                    CoreLoop.InvokeNext(delegate { callback(false); });
                }

                return;
            }

            try
            {
                var path = ModdedSavePath(saveSlot);
                if (File.Exists(path))
                {
                    using FileStream fileStream = File.OpenRead(path);
                    using var reader = new StreamReader(fileStream);
                    string json = reader.ReadToEnd();
                    this.moddedData = JsonConvert.DeserializeObject<ModSavegameData>
                    (
                        json,
                        new JsonSerializerSettings()
                        {
                            ContractResolver = ShouldSerializeContractResolver.Instance,
                            TypeNameHandling = TypeNameHandling.Auto,
                            ObjectCreationHandling = ObjectCreationHandling.Replace,
                            Converters = JsonConverterTypes.ConverterTypes
                        }
                    );
                    if (this.moddedData == null)
                    {
                        Logger.APILogger.LogError($"Loaded mod savegame data deserialized to null: {json}");
                        this.moddedData = new ModSavegameData();
                    }
                }
                else
                {
                    this.moddedData = new ModSavegameData();
                }
            }
            catch (Exception e)
            {
                Logger.APILogger.LogError(e);
                this.moddedData = new ModSavegameData();
            }

            ModHooks.OnLoadLocalSettings(this.moddedData);

            Platform.Current.ReadSaveSlot
            (
                saveSlot,
                delegate(byte[] fileBytes)
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
                            string encryptedString = (string)binaryFormatter.Deserialize(serializationStream);
                            json = Encryption.Decrypt(encryptedString);
                        }
                        else
                        {
                            json = Encoding.UTF8.GetString(fileBytes);
                        }

                        SaveGameData saveGameData;

                        try
                        {
                            saveGameData = JsonConvert.DeserializeObject<SaveGameData>
                            (
                                json,
                                new JsonSerializerSettings()
                                {
                                    ContractResolver = ShouldSerializeContractResolver.Instance,
                                    TypeNameHandling = TypeNameHandling.Auto,
                                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                                    Converters = JsonConverterTypes.ConverterTypes
                                }
                            );
                        }
                        catch (Exception e)
                        {
                            Logger.APILogger.LogError("Failed to read save using Json.NET (GameManager::LoadGame), falling back.");
                            Logger.APILogger.LogError(e);

                            saveGameData = JsonUtility.FromJson<SaveGameData>(json);
                        }

                        global::PlayerData instance = saveGameData.playerData;
                        SceneData instance2 = saveGameData.sceneData;
                        global::PlayerData.instance = instance;
                        this.playerData = instance;
                        SceneData.instance = instance2;
                        ModHooks.OnAfterSaveGameLoad(saveGameData);
                        this.sceneData = instance2;
                        this.profileID = saveSlot;
                        this.inputHandler.RefreshPlayerData();
                        ModHooks.OnSavegameLoad(saveSlot);
                        obj = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogFormat
                        (
                            "Error loading save file for slot {0}: {1}",
                            new object[]
                            {
                                saveSlot,
                                ex
                            }
                        );
                        obj = false;
                    }

                    if (callback != null)
                    {
                        callback(obj);
                    }
                }
            );
        }

        #endregion

        extern public void orig_SetupSceneRefs(bool refreshTilemapInfo);
        public void SetupSceneRefs(bool refreshTilemapInfo)
        {
            orig_SetupSceneRefs(refreshTilemapInfo);
            if (IsGameplayScene())
            {
                GameObject go = GameCameras.instance.soulOrbFSM.gameObject.transform.Find("SoulOrb_fill").gameObject;
                GameObject liquid = go.transform.Find("Liquid").gameObject;
                tk2dSpriteAnimator tk2dsa = liquid.GetComponent<tk2dSpriteAnimator>();
                tk2dsa.GetClipByName("Fill").fps = 15 * 1.05f;
                tk2dsa.GetClipByName("Idle").fps = 10 * 1.05f;
                tk2dsa.GetClipByName("Shrink").fps = 15 * 1.05f;
                tk2dsa.GetClipByName("Drain").fps = 30 * 1.05f;
            }
        }

        [MonoModReplace]
        public void GetSaveStatsForSlot(int saveSlot, Action<global::SaveStats> callback)
        {
            if (!Platform.IsSaveSlotIndexValid(saveSlot))
            {
                Debug.LogErrorFormat("Cannot get save stats for invalid slot {0}", new object[] { saveSlot });
                if (callback != null)
                {
                    CoreLoop.InvokeNext(delegate { callback(null); });
                }
                return;
            }
            Platform.Current.ReadSaveSlot(saveSlot, delegate(byte[] fileBytes)
            {
                if (fileBytes == null)
                {
                    if (callback != null)
                    {
                        CoreLoop.InvokeNext(delegate { callback(null); });
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
                    SaveGameData saveGameData;
                    try
                    {
                        saveGameData = JsonConvert.DeserializeObject<SaveGameData>(json, new JsonSerializerSettings()
                        {
                            ContractResolver = ShouldSerializeContractResolver.Instance,
                            TypeNameHandling = TypeNameHandling.Auto,
                            ObjectCreationHandling = ObjectCreationHandling.Replace,
                            Converters = JsonConverterTypes.ConverterTypes
                        });
                    }
                    catch (Exception)
                    {
                        // Not a huge deal, this happens on saves with mod data which haven't been converted yet.
                        Logger.APILogger.LogWarn($"Failed to get save stats for slot {saveSlot} using Json.NET, falling back");
                        saveGameData = JsonUtility.FromJson<SaveGameData>(json);
                    }
                    global::PlayerData playerData = saveGameData.playerData;
                    SaveStats saveStats = new SaveStats
                    (
                        playerData.GetInt(nameof(PlayerData.maxHealthBase)),
                        playerData.GetInt(nameof(PlayerData.geo)),
                        playerData.GetVariable<GlobalEnums.MapZone>(nameof(PlayerData.mapZone)),
                        playerData.GetFloat(nameof(PlayerData.playTime)),
                        playerData.GetInt(nameof(PlayerData.MPReserveMax)),
                        playerData.GetInt(nameof(PlayerData.permadeathMode)),
                        playerData.GetBool(nameof(PlayerData.bossRushMode)),
                        playerData.GetFloat(nameof(PlayerData.completionPercentage)),
                        playerData.GetBool(nameof(PlayerData.unlockedCompletionRate))
                    );
                    if (callback != null)
                    {
                        CoreLoop.InvokeNext(delegate { callback(saveStats); });
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error while loading save file for slot {saveSlot} Exception: {ex}");
                    if (callback != null)
                    {
                        CoreLoop.InvokeNext(delegate { callback(null); });
                    }
                }
            });
        }

        #region PauseToDynamicMenu

        [MonoModIgnore]
        public extern void SetTimeScale(float timescale);

        [MonoModIgnore]
        private extern void SetPausedState(bool value);

        // code has been copied from PauseGameToggle
        public IEnumerator PauseToggleDynamicMenu(MenuScreen screen, bool allowUnpause = false)
        {
            if (this.TimeSlowed)
            {
                yield break;
            }

            if (!this.playerData.GetBool(nameof(PlayerData.disablePause)) && this.gameState == GlobalEnums.GameState.PLAYING)
            {
                this.isPaused = true;
                this.ui.SetState(GlobalEnums.UIState.PAUSED);
                this.SetPausedState(true);
                this.SetState(GlobalEnums.GameState.PAUSED);
                if (HeroController.instance != null)
                {
                    HeroController.instance.Pause();
                }

                this.gameCams.MoveMenuToHUDCamera();
                this.inputHandler.PreventPause();
                this.inputHandler.StopUIInput();
                yield return new WaitForSecondsRealtime(0.3f);
                this.inputHandler.AllowPause();
            }
            else if (allowUnpause && this.gameState == GlobalEnums.GameState.PAUSED)
            {
                this.isPaused = false;
                this.inputHandler.PreventPause();
                this.ui.SetState(GlobalEnums.UIState.PLAYING);
                this.SetPausedState(false);
                this.SetState(GlobalEnums.GameState.PLAYING);
                if (HeroController.instance != null)
                {
                    HeroController.instance.UnPause();
                }

                MenuButtonList.ClearAllLastSelected();
                yield return new WaitForSecondsRealtime(0.3f);
                this.inputHandler.AllowPause();
            }

            yield break;
        }

        #endregion

        [MonoModIgnore]
        private SceneLoad sceneLoad;

        /*
         * This will allow modders to access the scene loader.
         * Note that if there's no transition in progress, it will be null!
         * Example use case: Start a co-routine that checks for an non null
         * sceneLoad then hooks up a callback to the "Finish" delegate to do something when the game has completed loading a scene.
         */
        // [MonoModIgnore]
        public SceneLoad SceneLoad
        {
            get { return sceneLoad; }
        }
    }

    public static partial class IlPatches
    {
        [MonoModIgnore]
        public static void GameManager_OnApplicationQuit(ILContext il)
        {
            // add a `ModHooks.OnApplicationQuit();` at the end of the method
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.AfterLabel, x => x.MatchRet());
            cursor.EmitDelegate(global::Modding.ModHooks.OnApplicationQuit);
        }

        [MonoModIgnore]
        public static void GameManager_LoadScene(ILContext il)
        {
            // add a `destScene = ModHooks.BeforeSceneLoad(destScene);` at the start and a `ModHooks.OnSceneChanged(destScene);` at the end of the method
            ILCursor cursor = new ILCursor(il).Goto(0);

            // Insert a call to your custom method
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate(global::Modding.ModHooks.BeforeSceneLoad);
            cursor.Emit(OpCodes.Starg, 1);

            cursor.GotoNext(MoveType.AfterLabel, x => x.MatchRet());
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate(global::Modding.ModHooks.OnSceneChanged);
        }

        [MonoModIgnore]
        public static void GameManager_ClearSaveFile(ILContext il)
        {
            // add a `ModHooks.OnSavegameClear(saveSlot);` at the start and a `ModHooks.OnAfterSaveGameClear(saveSlot);` at the end of the method
            ILCursor cursor = new ILCursor(il).Goto(0);

            // Insert a call to your custom method
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate(global::Modding.ModHooks.OnSavegameClear);

            // this goes just before both `ret`s
            while (cursor.TryGotoNext(MoveType.AfterLabel, x => x.MatchRet()))
            {
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate(global::Modding.ModHooks.OnAfterSaveGameClear);
                cursor.GotoNext();
            }
        }

        [MonoModIgnore]
        public static void GameManager_PlayerDead(ILContext il, TypeDefinition stateMachineTypeDef)
        {
            // add a `ModHooks.OnSavegameClear(saveSlot);` at the start and a `ModHooks.OnAfterSaveGameClear(saveSlot);` at the end of the method
            ILCursor cursor = new ILCursor(il);

            // Insert a call to your custom method
            cursor.GotoNext(MoveType.AfterLabel, x => x.MatchLdloc(1), x => x.MatchCallOrCallvirt(typeof(global::GameManager), "get_cameraCtrl"));
            cursor.EmitDelegate(global::Modding.ModHooks.OnBeforePlayerDead);

            // this goes just before all the `ret`s
            cursor.GotoNext(MoveType.AfterLabel, x => x.MatchLdcI4(0), x => x.MatchRet());
            cursor.EmitDelegate(global::Modding.ModHooks.OnAfterPlayerDead);
        }

        [MonoModIgnore]
        public static void GameManager_LoadSceneAdditive(ILContext il, TypeDefinition stateMachineTypeDef)
        {
            // add a `destScene = ModHooks.BeforeSceneLoad(destScene);` at the start and a `ModHooks.OnSceneChanged(destScene);` in the middle of the method
            ILCursor cursor = new ILCursor(il);

            // Insert a call to your custom method
            cursor.GotoNext
            (
                MoveType.AfterLabel,
                x => x.MatchLdloc(1),
                x => x.MatchLdcI4(1),
                x => x.MatchStfld<global::GameManager>("tilemapDirty")
            );
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, stateMachineTypeDef.Fields.First(f => f.Name == "destScene"));
            cursor.EmitDelegate(global::Modding.ModHooks.BeforeSceneLoad);
            cursor.Emit(OpCodes.Stfld, stateMachineTypeDef.Fields.First(f => f.Name == "destScene"));

            // somewhere before `this.RefreshTilemapInfo(destScene);`
            cursor.GotoNext
            (
                MoveType.AfterLabel,
                x => x.MatchLdloc(1),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(out _), // destScene field of statemachine type
                x => x.MatchCallOrCallvirt(typeof(global::GameManager), "RefreshTilemapInfo")
            );
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, stateMachineTypeDef.Fields.First(f => f.Name == "destScene"));
            cursor.EmitDelegate(global::Modding.ModHooks.OnSceneChanged);
        }

        [MonoModIgnore]
        public static void GameManager_LoadFirstScene(ILContext il, TypeDefinition stateMachineTypeDef)
        {
            // add a `ModHooks.OnNewGame();` at the end of the method
            ILCursor cursor = new ILCursor(il);

            // Insert a call to your custom method
            cursor.GotoNext(MoveType.AfterLabel, x => x.MatchLdloc(1));
            cursor.GotoNext(MoveType.AfterLabel, x => x.MatchLdcI4(0), x => x.MatchRet());
            cursor.EmitDelegate(global::Modding.ModHooks.OnNewGame);
        }

        [MonoModIgnore]
        public static void GameManager_OnWillActivateFirstLevel(ILContext il)
        {
            // add a `ModHooks.OnNewGame();` at the end of the method
            ILCursor cursor = new ILCursor(il);

            // Insert a call to your custom method
            cursor.GotoNext(MoveType.AfterLabel, x => x.MatchRet());
            cursor.EmitDelegate(global::Modding.ModHooks.OnNewGame);
        }
    }
}