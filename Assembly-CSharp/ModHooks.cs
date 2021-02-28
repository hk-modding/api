using System;
using System.Collections.Generic;
using System.IO;
using GlobalEnums;
using HutongGames.PlayMaker;
using JetBrains.Annotations;
using Modding.Menu;
using Modding.Patches;
using MonoMod;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable PossibleInvalidCastExceptionInForeachLoop
// ReSharper disable SuggestVarOrType_SimpleTypes
#pragma warning disable 1591

namespace Modding
{
    /// <summary>
    ///     Class to hook into various events for the game.
    /// </summary>
    [PublicAPI]
    public class ModHooks
    {
        private const int _modVersion = 60;
        
        internal static bool IsInitialized;

        private static readonly string SettingsPath = Path.Combine(Application.persistentDataPath, "ModdingApi.GlobalSettings.json");

        private static ModHooks _instance;

        /// <summary>
        ///     Currently Loaded Mods
        /// </summary>
        public readonly List<string> LoadedMods = new();

        /// <summary>
        ///     Dictionary of mods and their version #s
        /// </summary>
        public readonly Dictionary<string, string> LoadedModsWithVersions = new ();

        private Console _console;

        private ModHooksGlobalSettings _globalSettings;

        /// <summary>
        ///     The Version of the Modding API
        /// </summary>
        public string ModVersion;

        /// <summary>
        ///     Version of the Game
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public GameVersionData version;

        private ModHooks()
        {
            ModManager _ = new ModManager();
            Logger.SetLogLevel(GlobalSettings.LoggingLevel);
            
            GameVersion gameVersion;
            try
            {
                string[] versionNums = Constants.GAME_VERSION.Split('.');

                gameVersion.major = Convert.ToInt32(versionNums[0]);
                gameVersion.minor = Convert.ToInt32(versionNums[1]);
                gameVersion.revision = Convert.ToInt32(versionNums[2]);
                gameVersion.package = Convert.ToInt32(versionNums[3]);
            }
            catch (Exception e)
            {
                gameVersion.major = 0;
                gameVersion.minor = 0;
                gameVersion.revision = 0;
                gameVersion.package = 0;

                Logger.APILogger.LogError("Failed obtaining game version:\n" + e);
            }

            // ReSharper disable once Unity.IncorrectScriptableObjectInstantiation idk it works
            version = new GameVersionData { gameVersion = gameVersion };

            ModVersion = version.GetGameVersionString() + "-" + _modVersion;

            ApplicationQuitHook += SaveGlobalSettings;
            
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                Logger.APILogger.Log($"Couldn't resolve assembly {args.Name}, with sender {sender}");

                return null;
            };

            IsInitialized = true;
        }

        internal ModHooksGlobalSettings GlobalSettings
        {
            get
            {
                if (_globalSettings != null)
                {
                    return _globalSettings;
                }

                LoadGlobalSettings();

                _globalSettings.ModEnabledSettings ??= new Dictionary<string, bool>();

                return _globalSettings;
            }
        }

        /// <summary>
        ///     Current instance of Modhooks.
        /// </summary>
        public static ModHooks Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = new ModHooks();
                return _instance;
            }
        }

        internal void LogConsole(string message)
        {
            try
            {
                if (!GlobalSettings.ShowDebugLogInGame)
                {
                    return;
                }

                if (_console == null)
                {
                    GameObject go = new GameObject();
                    Object.DontDestroyOnLoad(go);
                    _console = go.AddComponent<Console>();
                }

                _console.AddText(message);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        ///     Called whenever localization specific strings are requested
        /// </summary>
        /// <remarks>N/A</remarks>
        public event LanguageGetHandler LanguageGetHook;

        /// <summary>
        ///     Called whenever localization specific strings are requested
        /// </summary>
        /// <remarks>N/A</remarks>
        internal string LanguageGet(string key, string sheet)
        {
            string @internal = Patches.Language.GetInternal(key, sheet);
            string result = @internal;
            bool gotText = false;

            if (LanguageGetHook == null)
            {
                return result;
            }

            Delegate[] invocationList = LanguageGetHook.GetInvocationList();

            foreach (LanguageGetHandler toInvoke in invocationList)
            {
                try
                {
                    string text = toInvoke.Invoke(key, sheet);
                    if (text == @internal || gotText)
                    {
                        continue;
                    }

                    result = text;
                    gotText = true;
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }

            return result;
        }

        /// <summary>
        ///     Called whenever game tries to show cursor
        /// </summary>
        public event Action CursorHook;

        /// <summary>
        ///     Called whenever game tries to show cursor
        /// </summary>
        internal void OnCursor()
        {
            Cursor.lockState = CursorLockMode.None;

            if (CursorHook != null)
            {
                CursorHook.Invoke();
                return;
            }

            if (GameManager.instance.isPaused)
            {
                Cursor.visible = true;
                return;
            }

            Cursor.visible = false;
        }


        /// <summary>
        ///     Called whenever a new gameobject is created with a collider and playmaker2d
        /// </summary>
        /// <remarks>PlayMakerUnity2DProxy.Start</remarks>
        public event Action<GameObject> ColliderCreateHook;

        /// <summary>
        ///     Called whenever a new gameobject is created with a collider and playmaker2d
        /// </summary>
        /// <remarks>PlayMakerUnity2DProxy.Start</remarks>
        internal void OnColliderCreate(GameObject go)
        {
            Logger.APILogger.LogFine("OnColliderCreate Invoked");

            if (ColliderCreateHook == null)
            {
                return;
            }

            Delegate[] invocationList = ColliderCreateHook.GetInvocationList();

            foreach (Action<GameObject> toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke(go);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }
        }


        /// <summary>
        ///     Called whenever game tries to create a new gameobject.  This happens often, care should be taken.
        /// </summary>
        public event Func<GameObject, GameObject> ObjectPoolSpawnHook;

        /// <summary>
        ///     Called whenever game tries to show cursor
        /// </summary>
        internal GameObject OnObjectPoolSpawn(GameObject go)
        {
            // No log because it's too spammy

            if (ObjectPoolSpawnHook == null)
            {
                return go;
            }

            Delegate[] invocationList = ObjectPoolSpawnHook.GetInvocationList();

            foreach (Func<GameObject, GameObject> toInvoke in invocationList)
            {
                try
                {
                    go = toInvoke.Invoke(go);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }

            return go;
        }

        /// <summary>
        ///     Called when the game is fully closed
        /// </summary>
        /// <remarks>GameManager.OnApplicationQuit</remarks>
        public event Action ApplicationQuitHook;

        /// <summary>
        ///     Called when the game is fully closed
        /// </summary>
        /// <remarks>GameManager.OnApplicationQuit</remarks>
        internal void OnApplicationQuit()
        {
            Logger.APILogger.LogFine("OnApplicationQuit Invoked");

            if (ApplicationQuitHook == null)
            {
                return;
            }

            Delegate[] invocationList = ApplicationQuitHook.GetInvocationList();

            foreach (Action toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }
        }

        /// <summary>
        ///     Save GlobalSettings to disk. (backs up the current global settings if it exists)
        /// </summary>
        internal void SaveGlobalSettings()
        {
            Logger.APILogger.Log("Saving Global Settings");
            
            if (File.Exists(SettingsPath + ".bak"))
            {
                File.Delete(SettingsPath + ".bak");
            }

            if (File.Exists(SettingsPath))
            {
                File.Move(SettingsPath, SettingsPath + ".bak");
            }

            using FileStream fileStream = File.Create(SettingsPath);
            
            using StreamWriter writer = new StreamWriter(fileStream);
            
            try
            {
                string json = JsonConvert.SerializeObject(GlobalSettings, Formatting.Indented, new JsonSerializerSettings
                {
                    ContractResolver = ShouldSerializeContractResolver.Instance,
                    TypeNameHandling = TypeNameHandling.Auto,
                    Converters = JsonConverterTypes.ConverterTypes
                });
                        
                writer.Write(json);

            }
            catch (Exception e)
            {
                Logger.APILogger.LogError("Failed to save global settings using Json.NET.");
                Logger.APILogger.LogError(e);
                        
                string json = JsonUtility.ToJson(GlobalSettings, true);
                        
                writer.Write(json);
            }
        }

        /// <summary>
        ///     Loads global settings from disk (if they exist)
        /// </summary>
        internal void LoadGlobalSettings()
        {
            Logger.APILogger.Log("Loading ModdingApi Global Settings.");

            if (!File.Exists(SettingsPath))
            {
                _globalSettings = new ModHooksGlobalSettings
                {
                    LoggingLevel = LogLevel.Info,
                    ModEnabledSettings = new Dictionary<string, bool>()
                };
                
                return;
            }

            try
            {
                using FileStream fileStream = File.OpenRead(SettingsPath);
                using StreamReader reader = new StreamReader(fileStream);
                
                string json = reader.ReadToEnd();

                try
                {
                    _globalSettings = JsonConvert.DeserializeObject<ModHooksGlobalSettings>
                    (
                        json,
                        new JsonSerializerSettings
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
                    Logger.APILogger.LogError("Failed to deserialize settings using Json.NET, falling back.");
                    Logger.APILogger.LogError(e);

                    _globalSettings = JsonUtility.FromJson<ModHooksGlobalSettings>(json);
                }
            }
            catch (Exception e)
            {
                Logger.APILogger.LogError("Failed to load global settings, creating new settings file:\n" + e);

                if (File.Exists(SettingsPath))
                {
                    File.Move(SettingsPath, SettingsPath + ".error");
                }

                _globalSettings = new ModHooksGlobalSettings
                {
                    LoggingLevel = LogLevel.Info, 
                    ModEnabledSettings = new Dictionary<string, bool>()
                };
            }
        }

        public event HitInstanceHandler HitInstanceHook;

        /// <summary>
        ///     Called whenever a HitInstance is created. Overrides normal functionality
        /// </summary>
        /// <remarks>HutongGames.PlayMaker.Actions.TakeDamage</remarks>
        internal HitInstance OnHitInstanceBeforeHit(Fsm owner, HitInstance hit)
        {
            Logger.APILogger.LogFine("OnHitInstance Invoked");

            if (HitInstanceHook == null)
            {
                return hit;
            }

            Delegate[] invocationList = HitInstanceHook.GetInvocationList();

            foreach (HitInstanceHandler toInvoke in invocationList)
            {
                try
                {
                    hit = toInvoke.Invoke(owner, hit);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }

            return hit;
        }

        /// <summary>
        ///     Called when a SceneManager calls DrawBlackBorders and creates boarders for a scene. You may use or modify the
        ///     bounds of an area of the scene with these.
        /// </summary>
        /// <remarks>SceneManager.DrawBlackBorders</remarks>
        public event Action<List<GameObject>> DrawBlackBordersHook;

        internal void OnDrawBlackBorders(List<GameObject> borders)
        {
            Logger.APILogger.LogFine("OnDrawBlackBorders Invoked");

            if (DrawBlackBordersHook == null)
            {
                return;
            }

            Delegate[] invocationList = DrawBlackBordersHook.GetInvocationList();

            foreach (Action<List<GameObject>> toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke(borders);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }
        }

        /// <summary>
        ///     Called when an enemy is enabled. Check this isDead flag to see if they're already dead. If you return true, this
        ///     will mark the enemy as already dead on load. Default behavior is to return the value inside "isAlreadyDead".
        /// </summary>
        /// <remarks>HealthManager.CheckPersistence</remarks>
        public event OnEnableEnemyHandler OnEnableEnemyHook;

        internal bool OnEnableEnemy(GameObject enemy, bool isAlreadyDead)
        {
            Logger.APILogger.LogFine("OnEnableEnemy Invoked");

            if (OnEnableEnemyHook == null)
            {
                return isAlreadyDead;
            }

            Delegate[] invocationList = OnEnableEnemyHook.GetInvocationList();

            foreach (OnEnableEnemyHandler toInvoke in invocationList)
            {
                try
                {
                    isAlreadyDead = toInvoke.Invoke(enemy, isAlreadyDead);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }

            return isAlreadyDead;
        }

        /// <summary>
        ///     Called when an enemy recieves a death event. It looks like this event may be called multiple times on an enemy, so
        ///     check "eventAlreadyRecieved" to see if the event has been fired more than once.
        /// </summary>
        /// <remarks>EnemyDeathEffects.RecieveDeathEvent</remarks>
        public event OnReceiveDeathEventHandler OnReceiveDeathEventHook;

        internal void OnRecieveDeathEvent
        (
            EnemyDeathEffects enemyDeathEffects,
            bool eventAlreadyRecieved,
            ref float? attackDirection,
            ref bool resetDeathEvent,
            ref bool spellBurn,
            ref bool isWatery
        )
        {
            Logger.APILogger.LogFine("OnRecieveDeathEvent Invoked");

            if (OnReceiveDeathEventHook == null) return;
            
            Delegate[] invocationList = OnReceiveDeathEventHook.GetInvocationList();

            foreach (OnReceiveDeathEventHandler toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke
                    (
                        enemyDeathEffects,
                        eventAlreadyRecieved,
                        ref attackDirection,
                        ref resetDeathEvent,
                        ref spellBurn,
                        ref isWatery
                    );
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }
        }

        /// <summary>
        ///     Called when an enemy dies and a journal kill is recorded. You may use the "playerDataName" string or one of the
        ///     additional pre-formatted player data strings to look up values in playerData.
        /// </summary>
        /// <remarks>EnemyDeathEffects.OnRecordKillForJournal</remarks>
        public event RecordKillForJournalHandler RecordKillForJournalHook;

        internal void OnRecordKillForJournal
        (
            EnemyDeathEffects enemyDeathEffects,
            string playerDataName,
            string killedBoolPlayerDataLookupKey,
            string killCountIntPlayerDataLookupKey,
            string newDataBoolPlayerDataLookupKey
        )
        {
            Logger.APILogger.LogFine("RecordKillForJournal Invoked");

            if (RecordKillForJournalHook == null) return;
            {
                Delegate[] invocationList = RecordKillForJournalHook.GetInvocationList();

                foreach (RecordKillForJournalHandler toInvoke in invocationList)
                {
                    try
                    {
                        toInvoke.Invoke
                        (
                            enemyDeathEffects,
                            playerDataName,
                            killedBoolPlayerDataLookupKey,
                            killCountIntPlayerDataLookupKey,
                            newDataBoolPlayerDataLookupKey
                        );
                    }
                    catch (Exception ex)
                    {
                        Logger.APILogger.LogError("" + ex);
                    }
                }
            }
        }

        #region PlayerManagementHandling

        /// <summary>
        ///     Called when anything in the game tries to set a bool in player data
        /// </summary>
        /// <remarks>PlayerData.SetBool</remarks>
        /// <see cref="SetBoolProxy" />
        public event SetBoolProxy SetPlayerBoolHook;

        /// <summary>
        ///     Called by the game in PlayerData.SetBool
        /// </summary>
        /// <param name="target">Target Field Name</param>
        /// <param name="val">Value to set</param>
        internal void SetPlayerBool(string target, bool val)
        {
            if (SetPlayerBoolHook != null)
            {
                Delegate[] invocationList = SetPlayerBoolHook.GetInvocationList();

                foreach (SetBoolProxy toInvoke in invocationList)
                {
                    try
                    {
                        toInvoke.Invoke(target, val);
                    }
                    catch (Exception ex)
                    {
                        Logger.APILogger.LogError("" + ex);
                    }
                }

                return;
            }

            Patches.PlayerData.instance.SetBoolInternal(target, val);
        }


        /// <summary>
        ///     Called when anything in the game tries to get a bool from player data
        /// </summary>
        /// <remarks>PlayerData.GetBool</remarks>
        public event GetBoolProxy GetPlayerBoolHook;

        /// <summary>
        ///     Called by the game in PlayerData.GetBool
        /// </summary>
        /// <param name="target">Target Field Name</param>
        internal bool GetPlayerBool(string target)
        {
            bool boolInternal = Patches.PlayerData.instance.GetBoolInternal(target);
            bool result = boolInternal;
            bool gotValue = false;
            if (GetPlayerBoolHook == null)
            {
                return result;
            }

            Delegate[] invocationList = GetPlayerBoolHook.GetInvocationList();

            foreach (GetBoolProxy toInvoke in invocationList)
            {
                try
                {
                    bool flag2 = toInvoke.Invoke(target);

                    if (flag2 == boolInternal || gotValue)
                    {
                        continue;
                    }

                    result = flag2;
                    gotValue = true;
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }

            return result;
        }

        /// <summary>
        ///     Called when anything in the game tries to set an int in player data
        /// </summary>
        /// <remarks>PlayerData.SetInt</remarks>
        public event SetIntProxy SetPlayerIntHook;

        /// <summary>
        ///     Called by the game in PlayerData.SetInt
        /// </summary>
        /// <param name="target">Target Field Name</param>
        /// <param name="val">Value to set</param>
        internal void SetPlayerInt(string target, int val)
        {
            if (SetPlayerIntHook != null)
            {
                Delegate[] invocationList = SetPlayerIntHook.GetInvocationList();

                foreach (SetIntProxy toInvoke in invocationList)
                {
                    try
                    {
                        toInvoke.Invoke(target, val);
                    }
                    catch (Exception ex)
                    {
                        Logger.APILogger.LogError("" + ex);
                    }
                }

                return;
            }

            Patches.PlayerData.instance.SetIntInternal(target, val);
        }

        /// <summary>
        ///     Called when anything in the game tries to get an int from player data
        /// </summary>
        /// <remarks>PlayerData.GetInt</remarks>
        public event GetIntProxy GetPlayerIntHook;

        /// <summary>
        ///     Called by the game in PlayerData.GetInt
        /// </summary>
        /// <param name="target">Target Field Name</param>
        internal int GetPlayerInt(string target)
        {
            int intInternal = Patches.PlayerData.instance.GetIntInternal(target);
            int result = intInternal;
            bool gotValue = false;

            if (GetPlayerIntHook == null)
            {
                return result;
            }

            Delegate[] invocationList = GetPlayerIntHook.GetInvocationList();

            foreach (GetIntProxy toInvoke in invocationList)
            {
                try
                {
                    int num = toInvoke.Invoke(target);
                    if (num == intInternal || gotValue)
                    {
                        continue;
                    }

                    result = num;
                    gotValue = true;
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }

            return result;
        }

        /// <summary>
        ///     Called when anything in the game tries to set a float in player data
        /// </summary>
        /// <remarks>PlayerData.SetFloat</remarks>
        public event SetFloatProxy SetPlayerFloatHook;

        /// <summary>
        ///     Called by the game in PlayerData.SetFloat
        /// </summary>
        /// <param name="target">Target Field Name</param>
        /// <param name="val">Value to set</param>
        internal void SetPlayerFloat(string target, float val)
        {
            if (SetPlayerFloatHook != null)
            {
                Delegate[] invocationList = SetPlayerFloatHook.GetInvocationList();

                foreach (SetFloatProxy toInvoke in invocationList)
                {
                    try
                    {
                        toInvoke.Invoke(target, val);
                    }
                    catch (Exception ex)
                    {
                        Logger.APILogger.LogError("" + ex);
                    }
                }

                return;
            }

            Patches.PlayerData.instance.SetFloatInternal(target, val);
        }

        /// <summary>
        ///     Called when anything in the game tries to get a float from player data
        /// </summary>
        /// <remarks>PlayerData.GetFloat</remarks>
        public event GetFloatProxy GetPlayerFloatHook;

        /// <summary>
        ///     Called by the game in PlayerData.GetFloat
        /// </summary>
        /// <param name="target">Target Field Name</param>
        internal float GetPlayerFloat(string target)
        {
            float floatInternal = Patches.PlayerData.instance.GetFloatInternal(target);
            float result = floatInternal;
            bool gotValue = false;

            if (GetPlayerFloatHook == null)
            {
                return result;
            }

            Delegate[] invocationList = GetPlayerFloatHook.GetInvocationList();

            foreach (GetFloatProxy toInvoke in invocationList)
            {
                try
                {
                    float f = toInvoke.Invoke(target);

                    // ReSharper disable once CompareOfFloatsByEqualityOperator 
                    if (f == floatInternal || gotValue)
                    {
                        continue;
                    }

                    result = f;
                    gotValue = true;
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }

            return result;
        }

        /// <summary>
        ///     Called when anything in the game tries to set a string in player data
        /// </summary>
        /// <remarks>PlayerData.SetString</remarks>
        public event SetStringProxy SetPlayerStringHook;

        /// <summary>
        ///     Called by the game in PlayerData.SetString
        /// </summary>
        /// <param name="target">Target Field Name</param>
        /// <param name="val">Value to set</param>
        internal void SetPlayerString(string target, string val)
        {
            if (SetPlayerStringHook != null)
            {
                Delegate[] invocationList = SetPlayerStringHook.GetInvocationList();

                foreach (SetStringProxy toInvoke in invocationList)
                {
                    try
                    {
                        toInvoke.Invoke(target, val);
                    }
                    catch (Exception ex)
                    {
                        Logger.APILogger.LogError("" + ex);
                    }
                }

                return;
            }

            Patches.PlayerData.instance.SetStringInternal(target, val);
        }

        /// <summary>
        ///     Called when anything in the game tries to get a string from player data
        /// </summary>
        /// <remarks>PlayerData.GetString</remarks>
        public event GetStringProxy GetPlayerStringHook;

        /// <summary>
        ///     Called by the game in PlayerData.GetString
        /// </summary>
        /// <param name="target">Target Field Name</param>
        internal string GetPlayerString(string target)
        {
            string stringInternal = Patches.PlayerData.instance.GetStringInternal(target);
            string result = stringInternal;
            bool gotValue = false;
            if (GetPlayerStringHook == null)
            {
                return result;
            }

            Delegate[] invocationList = GetPlayerStringHook.GetInvocationList();

            foreach (GetStringProxy toInvoke in invocationList)
            {
                try
                {
                    string s = toInvoke.Invoke(target);
                    if (s == stringInternal || gotValue)
                    {
                        continue;
                    }

                    result = s;
                    gotValue = true;
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }

            return result;
        }

        /// <summary>
        ///     Called when anything in the game tries to set a Vector3 in player data
        /// </summary>
        /// <remarks>PlayerData.SetVector3</remarks>
        public event SetVector3Proxy SetPlayerVector3Hook;

        /// <summary>
        ///     Called by the game in PlayerData.SetVector3
        /// </summary>
        /// <param name="target">Target Field Name</param>
        /// <param name="val">Value to set</param>
        internal void SetPlayerVector3(string target, Vector3 val)
        {
            if (SetPlayerVector3Hook != null)
            {
                Delegate[] invocationList = SetPlayerVector3Hook.GetInvocationList();

                foreach (SetVector3Proxy toInvoke in invocationList)
                {
                    try
                    {
                        toInvoke.Invoke(target, val);
                    }
                    catch (Exception ex)
                    {
                        Logger.APILogger.LogError("" + ex);
                    }
                }

                return;
            }

            Patches.PlayerData.instance.SetVector3Internal(target, val);
        }

        /// <summary>
        ///     Called when anything in the game tries to get a Vector3 from player data
        /// </summary>
        /// <remarks>PlayerData.GetVector3</remarks>
        public event GetVector3Proxy GetPlayerVector3Hook;

        /// <summary>
        ///     Called by the game in PlayerData.GetVector3
        /// </summary>
        /// <param name="target">Target Field Name</param>
        internal Vector3 GetPlayerVector3(string target)
        {
            Vector3 vecInternal = Patches.PlayerData.instance.GetVector3Internal(target);
            Vector3 result = vecInternal;
            bool gotValue = false;

            if (GetPlayerVector3Hook == null)
            {
                return result;
            }

            Delegate[] invocationList = GetPlayerVector3Hook.GetInvocationList();

            foreach (GetVector3Proxy toInvoke in invocationList)
            {
                try
                {
                    Vector3 vec = toInvoke.Invoke(target);

                    if (vec == vecInternal || gotValue)
                    {
                        continue;
                    }

                    result = vec;
                    gotValue = true;
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }

            return result;
        }

        /// <summary>
        ///     Called when anything in the game tries to set a generic variable in player data
        /// </summary>
        /// <remarks>PlayerData.SetVariable</remarks>
        public event SetVariableProxy SetPlayerVariableHook;

        /// <summary>
        ///     Called by the game in PlayerData.SetVariable
        /// </summary>
        /// <param name="target">Target Field Name</param>
        /// <param name="val">Value to set</param>
        internal void SetPlayerVariable<T>(string target, T val)
        {
            Type t = typeof(T);

            if (t == typeof(bool))
            {
                SetPlayerBool(target, (bool) (object) val);
                return;
            }

            if (t == typeof(int))
            {
                SetPlayerInt(target, (int) (object) val);
                return;
            }

            if (t == typeof(float))
            {
                SetPlayerFloat(target, (float) (object) val);
                return;
            }

            if (t == typeof(string))
            {
                SetPlayerString(target, (string) (object) val);
                return;
            }

            if (t == typeof(Vector3))
            {
                SetPlayerVector3(target, (Vector3) (object) val);
                return;
            }

            if (SetPlayerVariableHook != null)
            {
                bool gotValue = false;
                Delegate[] invocationList = SetPlayerVariableHook.GetInvocationList();

                foreach (SetVariableProxy toInvoke in invocationList)
                {
                    try
                    {
                        T v = (T) toInvoke.Invoke(typeof(T), target, val);
                        if (v.Equals(val) || gotValue)
                        {
                            continue;
                        }

                        val = v;
                        gotValue = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.APILogger.LogError("" + ex);
                    }
                }
            }

            Patches.PlayerData.instance.SetVariableInternal(target, val);
        }

        /// <summary>
        ///     Called when anything in the game tries to get a generic variable from player data
        /// </summary>
        /// <remarks>PlayerData.GetVariable</remarks>
        [PublicAPI]
        public event GetVariableProxy GetPlayerVariableHook;

        /// <summary>
        ///     Called by the game in PlayerData.GetVariable
        /// </summary>
        /// <param name="target">Target Field Name</param>
        internal T GetPlayerVariable<T>(string target)
        {
            Type t = typeof(T);

            if (t == typeof(bool))
            {
                return (T) (object) GetPlayerBool(target);
            }

            if (t == typeof(int))
            {
                return (T) (object) GetPlayerInt(target);
            }

            if (t == typeof(float))
            {
                return (T) (object) GetPlayerFloat(target);
            }

            if (t == typeof(string))
            {
                return (T) (object) GetPlayerString(target);
            }

            if (t == typeof(Vector3))
            {
                return (T) (object) GetPlayerVector3(target);
            }

            T varInternal = Patches.PlayerData.instance.GetVariableInternal<T>(target);
            T result = varInternal;
            bool gotValue = false;

            if (GetPlayerVariableHook == null)
            {
                return result;
            }

            Delegate[] invocationList = GetPlayerVariableHook.GetInvocationList();

            foreach (GetVariableProxy toInvoke in invocationList)
            {
                try
                {
                    T v = (T) toInvoke.Invoke(typeof(T), target, varInternal);
                    if (v.Equals(varInternal) || gotValue)
                    {
                        continue;
                    }

                    result = v;
                    gotValue = true;
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }

            return result;
        }

        /// <summary>
        ///     Called whenever blue health is updated
        /// </summary>
        public event Func<int> BlueHealthHook;

        /// <summary>
        ///     Called whenever blue health is updated
        /// </summary>
        internal int OnBlueHealth()
        {
            Logger.APILogger.LogFine("OnBlueHealth Invoked");

            int result = 0;
            if (BlueHealthHook == null)
            {
                return result;
            }

            Delegate[] invocationList = BlueHealthHook.GetInvocationList();

            foreach (Func<int> toInvoke in invocationList)
            {
                try
                {
                    result = toInvoke.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }

            return result;
        }


        /// <summary>
        ///     Called when health is taken from the player
        /// </summary>
        /// <remarks>HeroController.TakeHealth</remarks>
        public event TakeHealthProxy TakeHealthHook;

        /// <summary>
        ///     Called when health is taken from the player
        /// </summary>
        /// <remarks>HeroController.TakeHealth</remarks>
        internal int OnTakeHealth(int damage)
        {
            Logger.APILogger.LogFine("OnTakeHealth Invoked");

            if (TakeHealthHook == null)
            {
                return damage;
            }

            Delegate[] invocationList = TakeHealthHook.GetInvocationList();

            foreach (TakeHealthProxy toInvoke in invocationList)
            {
                try
                {
                    damage = toInvoke.Invoke(damage);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }

            return damage;
        }

        /// <summary>
        ///     Called when damage is dealt to the player
        /// </summary>
        /// <remarks>HeroController.TakeDamage</remarks>
        public event TakeDamageProxy TakeDamageHook;

        /// <summary>
        ///     Called when damage is dealt to the player
        /// </summary>
        /// <remarks>HeroController.TakeDamage</remarks>
        internal int OnTakeDamage(ref int hazardType, int damage)
        {
            Logger.APILogger.LogFine("OnTakeDamage Invoked");

            if (TakeDamageHook == null)
            {
                return damage;
            }

            Delegate[] invocationList = TakeDamageHook.GetInvocationList();

            foreach (TakeDamageProxy toInvoke in invocationList)
            {
                try
                {
                    damage = toInvoke.Invoke(ref hazardType, damage);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }

            return damage;
        }

        /// <summary>
        ///     Called at the end of the take damage function
        /// </summary>
        public event AfterTakeDamageHandler AfterTakeDamageHook;

        /// <summary>
        ///     Called at the end of the take damage function
        /// </summary>
        internal int AfterTakeDamage(int hazardType, int damageAmount)
        {
            Logger.APILogger.LogFine("AfterTakeDamage Invoked");

            if (AfterTakeDamageHook == null)
            {
                return damageAmount;
            }

            Delegate[] invocationList = AfterTakeDamageHook.GetInvocationList();

            foreach (AfterTakeDamageHandler toInvoke in invocationList)
            {
                try
                {
                    damageAmount = toInvoke.Invoke(hazardType, damageAmount);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }

            return damageAmount;
        }

        /// <summary>
        ///     Called when the player dies
        /// </summary>
        /// <remarks>GameManager.PlayerDead</remarks>
        public event Action BeforePlayerDeadHook;

        /// <summary>
        ///     Called when the player dies (at the beginning of the method)
        /// </summary>
        /// <remarks>GameManager.PlayerDead</remarks>
        internal void OnBeforePlayerDead()
        {
            Logger.APILogger.LogFine("OnBeforePlayerDead Invoked");

            if (BeforePlayerDeadHook == null)
            {
                return;
            }

            Delegate[] invocationList = BeforePlayerDeadHook.GetInvocationList();

            foreach (Action toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }
        }

        /// <summary>
        ///     Called after the player dies
        /// </summary>
        /// <remarks>GameManager.PlayerDead</remarks>
        public event Action AfterPlayerDeadHook;

        /// <summary>
        ///     Called after the player dies (at the end of the method)
        /// </summary>
        /// <remarks>GameManager.PlayerDead</remarks>
        internal void OnAfterPlayerDead()
        {
            Logger.APILogger.LogFine("OnAfterPlayerDead Invoked");

            if (AfterPlayerDeadHook == null)
            {
                return;
            }

            Delegate[] invocationList = AfterPlayerDeadHook.GetInvocationList();

            foreach (Action toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }
        }

        /// <summary>
        ///     Called whenever the player attacks
        /// </summary>
        /// <remarks>HeroController.Attack</remarks>
        public event Action<AttackDirection> AttackHook;

        /// <summary>
        ///     Called whenever the player attacks
        /// </summary>
        /// <remarks>HeroController.Attack</remarks>
        internal void OnAttack(AttackDirection dir)
        {
            Logger.APILogger.LogFine("OnAttack Invoked");

            if (AttackHook == null)
            {
                return;
            }

            Delegate[] invocationList = AttackHook.GetInvocationList();

            foreach (Action<AttackDirection> toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke(dir);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }
        }

        /// <summary>
        ///     Called at the start of the DoAttack function
        /// </summary>
        public event Action DoAttackHook;

        /// <summary>
        ///     Called at the start of the DoAttack function
        /// </summary>
        internal void OnDoAttack()
        {
            Logger.APILogger.LogFine("OnDoAttack Invoked");

            if (DoAttackHook == null)
            {
                return;
            }

            Delegate[] invocationList = DoAttackHook.GetInvocationList();

            foreach (Action toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }
        }


        /// <summary>
        ///     Called at the end of the attack function
        /// </summary>
        /// <remarks>HeroController.Attack</remarks>
        [MonoModPublic]
        public event Action<AttackDirection> AfterAttackHook;

        /// <summary>
        ///     Called at the end of the attack function
        /// </summary>
        /// <remarks>HeroController.Attack</remarks>
        internal void AfterAttack(AttackDirection dir)
        {
            Logger.APILogger.LogFine("AfterAttack Invoked");

            if (AfterAttackHook == null)
            {
                return;
            }

            Delegate[] invocationList = AfterAttackHook.GetInvocationList();

            foreach (Action<AttackDirection> toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke(dir);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }
        }

        /// <summary>
        ///     Called whenever nail strikes something
        /// </summary>
        public event SlashHitHandler SlashHitHook;

        /// <summary>
        ///     Called whenever nail strikes something
        /// </summary>
        internal void OnSlashHit(Collider2D otherCollider, GameObject gameObject)
        {
            Logger.APILogger.LogFine("OnSlashHit Invoked");

            if (otherCollider == null)
            {
                return;
            }

            if (SlashHitHook == null)
            {
                return;
            }

            Delegate[] invocationList = SlashHitHook.GetInvocationList();

            foreach (SlashHitHandler toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke(otherCollider, gameObject);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }
        }

        /// <summary>
        ///     Called after player values for charms have been set
        /// </summary>
        /// <remarks>HeroController.CharmUpdate</remarks>
        public event CharmUpdateHandler CharmUpdateHook;

        /// <summary>
        ///     Called after player values for charms have been set
        /// </summary>
        /// <remarks>HeroController.CharmUpdate</remarks>
        internal void OnCharmUpdate()
        {
            Logger.APILogger.LogFine("OnCharmUpdate Invoked");

            if (CharmUpdateHook == null)
            {
                return;
            }

            Delegate[] invocationList = CharmUpdateHook.GetInvocationList();

            foreach (CharmUpdateHandler toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke(PlayerData.instance, HeroController.instance);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }
        }

        /// <summary>
        ///     Called whenever the hero updates
        /// </summary>
        /// <remarks>HeroController.Update</remarks>
        public event Action HeroUpdateHook;

        /// <summary>
        ///     Called whenever the hero updates
        /// </summary>
        /// <remarks>HeroController.Update</remarks>
        internal void OnHeroUpdate()
        {
            //Logger.APILogger.LogFine("OnHeroUpdate Invoked");

            if (HeroUpdateHook == null)
            {
                return;
            }

            Delegate[] invocationList = HeroUpdateHook.GetInvocationList();

            foreach (Action toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }
        }

        /// <summary>
        ///     Called whenever the player heals
        /// </summary>
        /// <remarks>PlayerData.health</remarks>
        public event Func<int, int> BeforeAddHealthHook;

        /// <summary>
        ///     Called whenever the player heals
        /// </summary>
        /// <remarks>PlayerData.health</remarks>
        internal int BeforeAddHealth(int amount)
        {
            Logger.APILogger.LogFine("BeforeAddHealth Invoked");

            if (BeforeAddHealthHook == null)
            {
                return amount;
            }

            Delegate[] invocationList = BeforeAddHealthHook.GetInvocationList();

            foreach (Func<int, int> toInvoke in invocationList)
            {
                try
                {
                    amount = toInvoke.Invoke(amount);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }

            return amount;
        }

        /// <summary>
        ///     Called whenever focus cost is calculated
        /// </summary>
        public event Func<float> FocusCostHook;

        /// <summary>
        ///     Called whenever focus cost is calculated
        /// </summary>
        internal float OnFocusCost()
        {
            Logger.APILogger.LogFine("OnFocusCost Invoked");

            float result = 1f;

            if (FocusCostHook == null)
            {
                return result;
            }

            Delegate[] invocationList = FocusCostHook.GetInvocationList();

            foreach (Func<float> toInvoke in invocationList)
            {
                try
                {
                    result = toInvoke.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }

            return result;
        }

        /// <summary>
        ///     Called when Hero recovers Soul from hitting enemies
        ///     <returns>The amount of soul to recover</returns>
        /// </summary>
        public event Func<int, int> SoulGainHook;

        /// <summary>
        ///     Called when Hero recovers Soul from hitting enemies
        /// </summary>
        internal int OnSoulGain(int num)
        {
            Logger.APILogger.LogFine("OnSoulGain Invoked");

            if (SoulGainHook == null)
            {
                return num;
            }

            Delegate[] invocationList = SoulGainHook.GetInvocationList();

            foreach (Func<int, int> toInvoke in invocationList)
            {
                try
                {
                    num = toInvoke.Invoke(num);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }

            return num;
        }


        /// <summary>
        ///     Called during dash function to change velocity
        /// </summary>
        /// <remarks>HeroController.Dash</remarks>
        public event Func<Vector2, Vector2> DashVectorHook;

        /// <summary>
        ///     Called during dash function to change velocity
        /// </summary>
        /// <remarks>HeroController.Dash</remarks>
        internal Vector2 DashVelocityChange(Vector2 change)
        {
            Logger.APILogger.LogFine("DashVelocityChange Invoked");

            if (DashVectorHook == null)
            {
                return change;
            }

            Delegate[] invocationList = DashVectorHook.GetInvocationList();

            foreach (Func<Vector2, Vector2> toInvoke in invocationList)
            {
                try
                {
                    change = toInvoke.Invoke(change);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }

            return change;
        }

        /// <summary>
        ///     Called whenever the dash key is pressed. Returns whether or not to override normal dash functionality
        /// </summary>
        /// <remarks>HeroController.LookForQueueInput</remarks>
        public event Func<bool> DashPressedHook;

        /// <summary>
        ///     Called whenever the dash key is pressed. Returns whether or not to override normal dash functionality
        /// </summary>
        /// <remarks>HeroController.LookForQueueInput</remarks>
        internal bool OnDashPressed()
        {
            Logger.APILogger.LogFine("OnDashPressed Invoked");

            if (DashPressedHook == null)
            {
                return false;
            }

            bool ret = false;

            Delegate[] invocationList = DashPressedHook.GetInvocationList();

            foreach (Func<bool> toInvoke in invocationList)
            {
                try
                {
                    ret |= toInvoke.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }

            return ret;
        }

        #endregion


        #region SaveHandling

        /// <summary>
        ///     Called directly after a save has been loaded
        /// </summary>
        /// <remarks>GameManager.LoadGame</remarks>
        public event Action<int> SavegameLoadHook;

        /// <summary>
        ///     Called directly after a save has been loaded
        /// </summary>
        /// <remarks>GameManager.LoadGame</remarks>
        internal void OnSavegameLoad(int id)
        {
            Logger.APILogger.LogFine("OnSavegameLoad Invoked");

            if (SavegameLoadHook == null)
            {
                return;
            }

            Delegate[] invocationList = SavegameLoadHook.GetInvocationList();

            foreach (Action<int> toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke(id);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }
        }

        /// <summary>
        ///     Called directly after a save has been saved
        /// </summary>
        /// <remarks>GameManager.SaveGame</remarks>
        public event Action<int> SavegameSaveHook;
        
        /// <summary>
        ///     Called directly after a save has been saved
        /// </summary>
        /// <remarks>GameManager.SaveGame</remarks>
        internal void OnSavegameSave(int id)
        {
            Logger.APILogger.LogFine("OnSavegameSave Invoked");

            if (SavegameSaveHook == null)
            {
                return;
            }

            Delegate[] invocationList = SavegameSaveHook.GetInvocationList();

            foreach (Action<int> toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke(id);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }
        }

        /// <summary>
        ///     Called whenever a new game is started
        /// </summary>
        /// <remarks>GameManager.LoadFirstScene</remarks>
        public event Action NewGameHook;

        /// <summary>
        ///     Called whenever a new game is started
        /// </summary>
        /// <remarks>GameManager.LoadFirstScene</remarks>
        internal void OnNewGame()
        {
            Logger.APILogger.LogFine("OnNewGame Invoked");

            if (NewGameHook == null)
            {
                return;
            }

            Delegate[] invocationList = NewGameHook.GetInvocationList();

            foreach (Action toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }
        }

        /// <summary>
        ///     Called before a save file is deleted
        /// </summary>
        /// <remarks>GameManager.ClearSaveFile</remarks>
        public event Action<int> SavegameClearHook;

        /// <summary>
        ///     Called before a save file is deleted
        /// </summary>
        /// <remarks>GameManager.ClearSaveFile</remarks>
        internal void OnSavegameClear(int id)
        {
            Logger.APILogger.LogFine("OnSavegameClear Invoked");

            if (SavegameClearHook == null)
            {
                return;
            }

            Delegate[] invocationList = SavegameClearHook.GetInvocationList();

            foreach (Action<int> toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke(id);
                }

                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }
        }

        /// <summary>
        ///     Called directly after a save has been loaded.  Allows for accessing SaveGame instance.
        /// </summary>
        /// <remarks>GameManager.LoadGame</remarks>
        public event Action<Patches.SaveGameData> AfterSavegameLoadHook;

        /// <summary>
        ///     Called directly after a save has been loaded.  Allows for accessing SaveGame instance.
        /// </summary>
        /// <remarks>GameManager.LoadGame</remarks>
        internal void OnAfterSaveGameLoad(Patches.SaveGameData data)
        {
            Logger.APILogger.LogFine("OnAfterSaveGameLoad Invoked");

            if (AfterSavegameLoadHook == null)
            {
                return;
            }

            Delegate[] invocationList = AfterSavegameLoadHook.GetInvocationList();

            foreach (Action<Patches.SaveGameData> toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke(data);
                }

                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }
        }

        /// <summary>
        ///     Called directly before save has been saved to allow for changes to the data before persisted.
        /// </summary>
        /// <remarks>GameManager.SaveGame</remarks>
        public event Action<Patches.SaveGameData> BeforeSavegameSaveHook;

        /// <summary>
        ///     Called directly before save has been saved to allow for changes to the data before persisted.
        /// </summary>
        /// <remarks>GameManager.SaveGame</remarks>
        internal void OnBeforeSaveGameSave(Patches.SaveGameData data)
        {
            Logger.APILogger.LogFine("OnBeforeSaveGameSave Invoked");
            data.LoadedMods = LoadedModsWithVersions;

            if (BeforeSavegameSaveHook == null)
            {
                return;
            }

            Delegate[] invocationList = BeforeSavegameSaveHook.GetInvocationList();

            foreach (Action<Patches.SaveGameData> toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke(data);
                }

                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }
        }

        /// <summary>
        ///     Overrides the filename to load for a given slot.  Return null to use vanilla names.
        /// </summary>
        public event Func<int, string> GetSaveFileNameHook;

        /// <summary>
        ///     Overrides the filename to load for a given slot.  Return null to use vanilla names.
        /// </summary>
        internal string GetSaveFileName(int saveSlot)
        {
            Logger.APILogger.LogFine("GetSaveFileName Invoked");

            if (GetSaveFileNameHook == null)
            {
                return null;
            }

            string ret = null;

            Delegate[] invocationList = GetSaveFileNameHook.GetInvocationList();

            foreach (Func<int, string> toInvoke in invocationList)
            {
                try
                {
                    ret = toInvoke.Invoke(saveSlot);
                }

                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }

            return ret;
        }

        /// <summary>
        ///     Called after a game has been cleared from a slot.
        /// </summary>
        public event Action<int> AfterSaveGameClearHook;

        /// <summary>
        ///     Called after a game has been cleared from a slot.
        /// </summary>
        internal void OnAfterSaveGameClear(int saveSlot)
        {
            Logger.APILogger.LogFine("OnAfterSaveGameClear Invoked");

            if (AfterSaveGameClearHook == null)
            {
                return;
            }

            Delegate[] invocationList = AfterSaveGameClearHook.GetInvocationList();

            foreach (Action<int> toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke(saveSlot);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }
        }

        #endregion

        #region SceneHandling

        /// <summary>
        ///     Called after a new Scene has been loaded
        /// </summary>
        /// <remarks>N/A</remarks>
        public event Action<string> SceneChanged;

        /// <summary>
        ///     Called after a new Scene has been loaded
        /// </summary>
        /// <remarks>N/A</remarks>
        internal void OnSceneChanged(string targetScene)
        {
            Logger.APILogger.LogFine("OnSceneChanged Invoked");

            if (SceneChanged == null)
            {
                return;
            }

            Delegate[] invocationList = SceneChanged.GetInvocationList();

            foreach (Action<string> toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke(targetScene);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }
        }

        /// <summary>
        ///     Called right before a scene gets loaded, can change which scene gets loaded
        /// </summary>
        /// <remarks>N/A</remarks>
        public event Func<string, string> BeforeSceneLoadHook;
        
        /// <summary>
        ///     Called right before a scene gets loaded, can change which scene gets loaded
        /// </summary>
        /// <remarks>N/A</remarks>
        internal string BeforeSceneLoad(string sceneName)
        {
            Logger.APILogger.LogFine("BeforeSceneLoad Invoked");

            if (BeforeSceneLoadHook == null)
            {
                return sceneName;
            }

            Delegate[] invocationList = BeforeSceneLoadHook.GetInvocationList();

            foreach (Func<string, string> toInvoke in invocationList)
            {
                try
                {
                    sceneName = toInvoke.Invoke(sceneName);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("" + ex);
                }
            }

            return sceneName;
        }

        #endregion
    }
}
