using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using GlobalEnums;
using HutongGames.PlayMaker;
using JetBrains.Annotations;
using Modding.Patches;
using MonoMod;
using Newtonsoft.Json;
using UnityEngine;
using System.Linq;
using Modding.Delegates;
using Object = UnityEngine.Object;

// ReSharper disable PossibleInvalidCastExceptionInForeachLoop
// ReSharper disable SuggestVarOrType_SimpleTypes

namespace Modding
{
    /// <summary>
    ///     Class to hook into various events for the game.
    /// </summary>
    [PublicAPI]
    public class ModHooks
    {
        private const int _modVersion = 69;

        private static readonly string SettingsPath = Path.Combine(Application.persistentDataPath, "ModdingApi.GlobalSettings.json");

        private static ModHooks _instance;

        /// <summary>
        ///     A map of mods to their built menu screens.
        /// </summary>
        public static ReadOnlyDictionary<IMod, MenuScreen> BuiltModMenuScreens => new(ModListMenu.ModScreens);

        /// <summary>
        ///     Dictionary of mods and their version #s
        /// </summary>
        public static readonly Dictionary<string, string> LoadedModsWithVersions = new();

        private static Console _console;

        /// <summary>
        ///     The Version of the Modding API
        /// </summary>
        public static string ModVersion;

        /// <summary>
        ///     Version of the Game
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static GameVersionData version;

        static ModHooks()
        {
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

            // Save global settings only if mods have finished loading
            FinishedLoadingModsHook += () => ApplicationQuitHook += SaveGlobalSettings;
        }

        /// <summary>
        /// The global ModHooks settings.
        /// </summary>
        public static ModHooksGlobalSettings GlobalSettings { get; private set; } = new();

        internal static void LoadGlobalSettings()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                    return;
                Logger.APILogger.Log("Loading Global Settings");
                using FileStream fileStream = File.OpenRead(SettingsPath);
                using var reader = new StreamReader(fileStream);
                string json = reader.ReadToEnd();

                var de = JsonConvert.DeserializeObject<ModHooksGlobalSettings>(
                    json,
                    new JsonSerializerSettings
                    {
                        ContractResolver = ShouldSerializeContractResolver.Instance,
                        TypeNameHandling = TypeNameHandling.Auto,
                        ObjectCreationHandling = ObjectCreationHandling.Replace,
                        Converters = JsonConverterTypes.ConverterTypes
                    }
                );
                if (de != null)
                {
                    GlobalSettings = de;
                    Logger.SetLogLevel(GlobalSettings.LoggingLevel);
                }
            }
            catch (Exception e)
            {
                Logger.APILogger.LogError(e);
            }
        }

        internal static void SaveGlobalSettings()
        {
            try
            {
                Logger.APILogger.Log("Saving Global Settings");
                
                var settings = GlobalSettings;
                
                if (settings is null)
                    return;
                
                settings.ModEnabledSettings = new Dictionary<string, bool>();
                
                foreach (var x in ModLoader.ModInstances)
                {
                    if (x.Mod is ITogglableMod && x.Error is null) 
                        settings.ModEnabledSettings.Add(x.Name, x.Enabled);
                }
                
                if (File.Exists(SettingsPath + ".bak")) 
                    File.Delete(SettingsPath + ".bak");
                
                if (File.Exists(SettingsPath)) 
                    File.Move(SettingsPath, SettingsPath + ".bak");
                
                using FileStream fileStream = File.Create(SettingsPath);
                using var writer = new StreamWriter(fileStream);
                
                writer.Write(JsonConvert.SerializeObject(
                    settings,
                    Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ContractResolver = ShouldSerializeContractResolver.Instance,
                        TypeNameHandling = TypeNameHandling.Auto,
                        Converters = JsonConverterTypes.ConverterTypes
                    }
                ));
            }
            catch (Exception e)
            {
                Logger.APILogger.LogError(e);
            }
        }

        /// <summary>
        ///     Current instance of Modhooks.
        /// </summary>
        [Obsolete("All members of ModHooks are now static, use the type name instead.")]
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

        internal static void LogConsole(string message, LogLevel level)
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

                _console.AddText(message, level);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        ///     Called whenever localization specific strings are requested
        /// </summary>
        /// <see cref="LanguageGetProxy"/>
        /// <remarks>N/A</remarks>
        public static event LanguageGetProxy LanguageGetHook;

        /// <summary>
        ///     Called whenever localization specific strings are requested
        /// </summary>
        /// <remarks>N/A</remarks>
        internal static string LanguageGet(string key, string sheet)
        {
            string res = Patches.Language.GetInternal(key, sheet);

            if (LanguageGetHook == null)
                return res;

            Delegate[] invocationList = LanguageGetHook.GetInvocationList();

            foreach (LanguageGetProxy toInvoke in invocationList)
            {
                try
                {
                    res = toInvoke(key, sheet, res);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError(ex);
                }
            }

            return res;
        }

        /// <summary>
        ///     Called whenever game tries to show cursor
        /// </summary>
        public static event Action CursorHook;

        /// <summary>
        ///     Called whenever game tries to show cursor
        /// </summary>
        internal static void OnCursor()
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
        public static event Action<GameObject> ColliderCreateHook;

        /// <summary>
        ///     Called whenever a new gameobject is created with a collider and playmaker2d
        /// </summary>
        /// <remarks>PlayMakerUnity2DProxy.Start</remarks>
        internal static void OnColliderCreate(GameObject go)
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
                    Logger.APILogger.LogError(ex);
                }
            }
        }


        /// <summary>
        ///     Called whenever game tries to create a new gameobject.  This happens often, care should be taken.
        /// </summary>
        public static event Func<GameObject, GameObject> ObjectPoolSpawnHook;

        /// <summary>
        ///     Called whenever game tries to show cursor
        /// </summary>
        internal static GameObject OnObjectPoolSpawn(GameObject go)
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
                    Logger.APILogger.LogError(ex);
                }
            }

            return go;
        }

        /// <summary>
        ///     Called when the game is fully closed
        /// </summary>
        /// <remarks>GameManager.OnApplicationQuit</remarks>
        public static event Action ApplicationQuitHook;

        /// <summary>
        ///     Called when the game is fully closed
        /// </summary>
        /// <remarks>GameManager.OnApplicationQuit</remarks>
        internal static void OnApplicationQuit()
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
                    Logger.APILogger.LogError(ex);
                }
            }
        }

        /// <summary>
        /// Called whenever a HitInstance is created. Overrides hit.
        /// </summary>
        /// <see cref="HitInstanceHandler"/>
        public static event HitInstanceHandler HitInstanceHook;

        /// <summary>
        ///     Called whenever a HitInstance is created. Overrides normal functionality
        /// </summary>
        /// <remarks>HutongGames.PlayMaker.Actions.TakeDamage</remarks>
        internal static HitInstance OnHitInstanceBeforeHit(Fsm owner, HitInstance hit)
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
                    Logger.APILogger.LogError(ex);
                }
            }

            return hit;
        }

        /// <summary>
        ///     Called when a SceneManager calls DrawBlackBorders and creates boarders for a scene. You may use or modify the
        ///     bounds of an area of the scene with these.
        /// </summary>
        /// <remarks>SceneManager.DrawBlackBorders</remarks>
        public static event Action<List<GameObject>> DrawBlackBordersHook;

        internal static void OnDrawBlackBorders(List<GameObject> borders)
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
                    Logger.APILogger.LogError(ex);
                }
            }
        }

        /// <summary>
        ///     Called when an enemy is enabled. Check this isDead flag to see if they're already dead. If you return true, this
        ///     will mark the enemy as already dead on load. Default behavior is to return the value inside "isAlreadyDead".
        /// </summary>
        /// <see cref="OnEnableEnemyHandler"/>
        /// <remarks>HealthManager.CheckPersistence</remarks>
        public static event OnEnableEnemyHandler OnEnableEnemyHook;

        internal static bool OnEnableEnemy(GameObject enemy, bool isAlreadyDead)
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
                    Logger.APILogger.LogError(ex);
                }
            }

            return isAlreadyDead;
        }

        /// <summary>
        ///     Called when an enemy recieves a death event. It looks like this event may be called multiple times on an enemy, so
        ///     check "eventAlreadyRecieved" to see if the event has been fired more than once.
        /// </summary>
        /// <see cref="OnReceiveDeathEventHandler"/>
        /// <remarks>EnemyDeathEffects.RecieveDeathEvent</remarks>
        public static event OnReceiveDeathEventHandler OnReceiveDeathEventHook;

        internal static void OnRecieveDeathEvent
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
                    Logger.APILogger.LogError(ex);
                }
            }
        }

        /// <summary>
        ///     Called when an enemy dies and a journal kill is recorded. You may use the "playerDataName" string or one of the
        ///     additional pre-formatted player data strings to look up values in playerData.
        /// </summary>
        /// <see cref="RecordKillForJournalHandler"/>
        /// <remarks>EnemyDeathEffects.OnRecordKillForJournal</remarks>
        public static event RecordKillForJournalHandler RecordKillForJournalHook;

        internal static void OnRecordKillForJournal
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
                        Logger.APILogger.LogError(ex);
                    }
                }
            }
        }

        #region PlayerManagementHandling

        /// <summary>
        ///     Called when anything in the game tries to set a bool in player data
        /// </summary>
        /// <example>
        /// <code>
        /// public int KillCount { get; set; }
        /// 
        /// ModHooks.Instance.SetPlayerBoolHook += SetBool;
        ///
        /// /*
        ///  * This uses the bool set to trigger a death, killing the player
        ///  * as well as preventing them from picking up dash, which could be used
        ///  * in something like a dashless mod.
        ///  *
        ///  * We are also able to use SetBool for counting things, as it is often
        ///  * called every time sometthing happens, regardless of the value
        ///  * this can be seen in our check for "killedMageLord", which counts the
        ///  * number of times the player kills Soul Master with the mod on.
        ///  */
        /// bool SetBool(string name, bool orig) {
        ///     switch (name) {
        ///         case "hasDash":
        ///             var hc = HeroController.instance;
        ///
        ///             // Kill the player
        ///             hc.StartCoroutine(hc.Die());
        ///
        ///             // Prevent dash from being picked up
        ///             return false;
        ///         case "killedMageLord":
        ///             // Just increment the counter.
        ///             KillCount++;
        ///
        ///             // We could also do something like award them geo for each kill
        ///             // And despite being a set, this would trigger on *every* kill
        ///             HeroController.instance.AddGeo(300);
        /// 
        ///             // Not changing the value.
        ///             return orig;
        ///         default:
        ///             return orig;
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <see cref="SetBoolProxy" />
        /// <remarks>PlayerData.SetBool</remarks>
        public static event SetBoolProxy SetPlayerBoolHook;

        /// <summary>
        ///     Called by the game in PlayerData.SetBool
        /// </summary>
        /// <param name="target">Target Field Name</param>
        /// <param name="orig">Value to set</param>
        internal static void SetPlayerBool(string target, bool orig)
        {
            bool value = orig;

            if (SetPlayerBoolHook != null)
            {
                Delegate[] invocationList = SetPlayerBoolHook.GetInvocationList();

                foreach (SetBoolProxy toInvoke in invocationList)
                {
                    try
                    {
                        value = toInvoke(target, value);
                    }
                    catch (Exception ex)
                    {
                        Logger.APILogger.LogError(ex);
                    }
                }
            }

            Patches.PlayerData.instance.SetBoolInternal(target, value);
        }


        /// <summary>
        ///     Called when anything in the game tries to get a bool from player data
        /// </summary>
        /// <example>
        /// <code>
        /// ModHooks.GetPlayerBoolHook += GetBool;
        ///
        /// // In this example, we always give the player dash, and
        /// // leave other bools as-is.
        /// bool? GetBool(string name, bool orig) {
        ///     return name == "canDash" ? true : orig;
        /// }
        /// </code>
        /// </example>
        /// <see cref="GetBoolProxy"/>
        /// <remarks>PlayerData.GetBool</remarks>
        public static event GetBoolProxy GetPlayerBoolHook;

        /// <summary>
        ///     Called by the game in PlayerData.GetBool
        /// </summary>
        /// <param name="target">Target Field Name</param>
        internal static bool GetPlayerBool(string target)
        {
            bool result = Patches.PlayerData.instance.GetBoolInternal(target);

            if (GetPlayerBoolHook == null)
                return result;

            Delegate[] invocationList = GetPlayerBoolHook.GetInvocationList();

            foreach (GetBoolProxy toInvoke in invocationList)
            {
                try
                {
                    result = toInvoke(target, result);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError(ex);
                }
            }

            return result;
        }

        /// <summary>
        ///     Called when anything in the game tries to set an int in player data
        /// </summary>
        /// <example>
        /// <code>
        /// ModHooks.Instance.SetPlayerIntHook += SetInt;
        ///
        /// int? SetInt(string name, int orig) {
        ///      // We could do something every time the player 
        ///      // receives or loses geo.
        ///     if (name == "geo") {
        ///         // Let's give the player soul if they *gain* geo
        ///         if (PlayerData.instance.geo &lt; orig) {
        ///             PlayerData.instance.AddMPChargeSpa(10);
        ///         }
        ///     }
        ///
        ///     // In this case, we aren't changing the value being set
        ///     // at all, so we just leave the value as the original for everything.
        ///     return orig;
        /// }
        /// </code>
        /// </example>
        /// <see cref="SetIntProxy"/>
        /// <remarks>PlayerData.SetInt</remarks>
        public static event SetIntProxy SetPlayerIntHook;

        /// <summary>
        ///     Called by the game in PlayerData.SetInt
        /// </summary>
        /// <param name="target">Target Field Name</param>
        /// <param name="orig">Value to set</param>
        internal static void SetPlayerInt(string target, int orig)
        {
            int value = orig;

            if (SetPlayerIntHook != null)
            {
                Delegate[] invocationList = SetPlayerIntHook.GetInvocationList();

                foreach (SetIntProxy toInvoke in invocationList)
                {
                    try
                    {
                        value = toInvoke(target, value);
                    }
                    catch (Exception ex)
                    {
                        Logger.APILogger.LogError(ex);
                    }
                }
            }

            Patches.PlayerData.instance.SetIntInternal(target, value);
        }

        /// <summary>
        ///     Called when anything in the game tries to get an int from player data
        /// </summary>
        /// <see cref="GetIntProxy"/>
        /// <example>
        /// <code>
        /// ModHooks.GetPlayerIntHook += GetInt;
        ///
        /// // This overrides the number of charm slots we have to 999,
        /// // effectively giving us infinite charm notches.
        /// // We ignore any other GetInt calls.
        /// int? GetInt(string name, int orig) {
        ///     return name == "charmSlots" ? 999 : orig;
        /// }
        /// </code>
        /// </example>
        /// <see cref="GetIntProxy"/>
        /// <remarks>PlayerData.GetInt</remarks>
        public static event GetIntProxy GetPlayerIntHook;

        /// <summary>
        ///     Called by the game in PlayerData.GetInt
        /// </summary>
        /// <param name="target">Target Field Name</param>
        internal static int GetPlayerInt(string target)
        {
            int result = Patches.PlayerData.instance.GetIntInternal(target);

            if (GetPlayerIntHook == null)
                return result;

            Delegate[] invocationList = GetPlayerIntHook.GetInvocationList();

            foreach (GetIntProxy toInvoke in invocationList)
            {
                try
                {
                    result = toInvoke(target, result);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError(ex);
                }
            }

            return result;
        }

        /// <summary>
        ///     Called when anything in the game tries to set a float in player data
        /// </summary>
        /// <see cref="SetFloatProxy"/>
        /// <remarks>PlayerData.SetFloat</remarks>
        public static event SetFloatProxy SetPlayerFloatHook;

        /// <summary>
        ///     Called by the game in PlayerData.SetFloat
        /// </summary>
        /// <param name="target">Target Field Name</param>
        /// <param name="orig">Value to set</param>
        internal static void SetPlayerFloat(string target, float orig)
        {
            float value = orig;

            if (SetPlayerFloatHook != null)
            {
                Delegate[] invocationList = SetPlayerFloatHook.GetInvocationList();

                foreach (SetFloatProxy toInvoke in invocationList)
                {
                    try
                    {
                        value = toInvoke.Invoke(target, value);
                    }
                    catch (Exception ex)
                    {
                        Logger.APILogger.LogError(ex);
                    }
                }
            }

            Patches.PlayerData.instance.SetFloatInternal(target, value);
        }

        /// <summary>
        ///     Called when anything in the game tries to get a float from player data
        /// </summary>
        /// <see cref="GetFloatProxy"/>
        /// <remarks>PlayerData.GetFloat</remarks>
        public static event GetFloatProxy GetPlayerFloatHook;

        /// <summary>
        ///     Called by the game in PlayerData.GetFloat
        /// </summary>
        /// <param name="target">Target Field Name</param>
        internal static float GetPlayerFloat(string target)
        {
            float result = Patches.PlayerData.instance.GetFloatInternal(target);

            if (GetPlayerFloatHook == null)
                return result;

            Delegate[] invocationList = GetPlayerFloatHook.GetInvocationList();

            foreach (GetFloatProxy toInvoke in invocationList)
            {
                try
                {
                    result = toInvoke.Invoke(target, result);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError(ex);
                }
            }

            return result;
        }

        /// <summary>
        ///     Called when anything in the game tries to set a string in player data
        /// </summary>
        /// <see cref="SetStringProxy"/>
        /// <remarks>PlayerData.SetString</remarks>
        public static event SetStringProxy SetPlayerStringHook;

        /// <summary>
        ///     Called by the game in PlayerData.SetString
        /// </summary>
        /// <param name="target">Target Field Name</param>
        /// <param name="orig">Value to set</param>
        internal static void SetPlayerString(string target, string orig)
        {
            string value = orig;

            if (SetPlayerStringHook != null)
            {
                Delegate[] invocationList = SetPlayerStringHook.GetInvocationList();

                foreach (SetStringProxy toInvoke in invocationList)
                {
                    try
                    {
                        value = toInvoke.Invoke(target, value);
                    }
                    catch (Exception ex)
                    {
                        Logger.APILogger.LogError(ex);
                    }
                }
            }

            Patches.PlayerData.instance.SetStringInternal(target, value);
        }

        /// <summary>
        ///     Called when anything in the game tries to get a string from player data
        /// </summary>
        /// <see cref="GetStringProxy"/>
        /// <remarks>PlayerData.GetString</remarks>
        public static event GetStringProxy GetPlayerStringHook;

        /// <summary>
        ///     Called by the game in PlayerData.GetString
        /// </summary>
        /// <param name="target">Target Field Name</param>
        internal static string GetPlayerString(string target)
        {
            string value = Patches.PlayerData.instance.GetStringInternal(target);

            if (GetPlayerStringHook == null)
                return value;

            Delegate[] invocationList = GetPlayerStringHook.GetInvocationList();

            foreach (GetStringProxy toInvoke in invocationList)
            {
                try
                {
                    value = toInvoke(target, value);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError(ex);
                }
            }

            return value;
        }

        /// <summary>
        ///     Called when anything in the game tries to set a Vector3 in player data
        /// </summary>
        /// <see cref="SetVector3Proxy"/>
        /// <remarks>PlayerData.SetVector3</remarks>
        public static event SetVector3Proxy SetPlayerVector3Hook;

        /// <summary>
        ///     Called by the game in PlayerData.SetVector3
        /// </summary>
        /// <param name="target">Target Field Name</param>
        /// <param name="orig">Value to set</param>
        internal static void SetPlayerVector3(string target, Vector3 orig)
        {
            Vector3 value = orig;

            if (SetPlayerVector3Hook != null)
            {
                Delegate[] invocationList = SetPlayerVector3Hook.GetInvocationList();

                foreach (SetVector3Proxy toInvoke in invocationList)
                {
                    try
                    {
                        value = toInvoke.Invoke(target, value);
                    }
                    catch (Exception ex)
                    {
                        Logger.APILogger.LogError(ex);
                    }
                }
            }

            Patches.PlayerData.instance.SetVector3Internal(target, value);
        }

        /// <summary>
        ///     Called when anything in the game tries to get a Vector3 from player data
        /// </summary>
        /// <see cref="GetVector3Proxy"/>
        /// <remarks>PlayerData.GetVector3</remarks>
        public static event GetVector3Proxy GetPlayerVector3Hook;

        /// <summary>
        ///     Called by the game in PlayerData.GetVector3
        /// </summary>
        /// <param name="target">Target Field Name</param>
        internal static Vector3 GetPlayerVector3(string target)
        {
            Vector3 res = Patches.PlayerData.instance.GetVector3Internal(target);

            if (GetPlayerVector3Hook == null)
                return res;

            Delegate[] invocationList = GetPlayerVector3Hook.GetInvocationList();

            foreach (GetVector3Proxy toInvoke in invocationList)
            {
                try
                {
                    res = toInvoke(target, res);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError(ex);
                }
            }

            return res;
        }

        /// <summary>
        ///     Called when anything in the game tries to set a generic variable in player data
        /// </summary>
        /// <see cref="SetVariableProxy"/>
        /// <remarks>PlayerData.SetVariable</remarks>
        public static event SetVariableProxy SetPlayerVariableHook;

        /// <summary>
        ///     Called by the game in PlayerData.SetVariable
        /// </summary>
        /// <param name="target">Target Field Name</param>
        /// <param name="orig">Value to set</param>
        internal static void SetPlayerVariable<T>(string target, T orig)
        {
            Type t = typeof(T);

            if (t == typeof(bool))
            {
                SetPlayerBool(target, (bool)(object)orig);
                return;
            }

            if (t == typeof(int))
            {
                SetPlayerInt(target, (int)(object)orig);
                return;
            }

            if (t == typeof(float))
            {
                SetPlayerFloat(target, (float)(object)orig);
                return;
            }

            if (t == typeof(string))
            {
                SetPlayerString(target, (string)(object)orig);
                return;
            }

            if (t == typeof(Vector3))
            {
                SetPlayerVector3(target, (Vector3)(object)orig);
                return;
            }

            T value = orig;

            if (SetPlayerVariableHook != null)
            {
                Delegate[] invocationList = SetPlayerVariableHook.GetInvocationList();

                foreach (SetVariableProxy toInvoke in invocationList)
                {
                    try
                    {
                        value = (T)toInvoke(t, target, value);
                    }
                    catch (Exception ex)
                    {
                        Logger.APILogger.LogError(ex);
                    }
                }
            }

            Patches.PlayerData.instance.SetVariableInternal(target, value);
        }

        /// <summary>
        ///     Called when anything in the game tries to get a generic variable from player data
        /// </summary>
        /// <see cref="GetVariableProxy"/>
        /// <remarks>PlayerData.GetVariable</remarks>
        [PublicAPI]
        public static event GetVariableProxy GetPlayerVariableHook;

        /// <summary>
        ///     Called by the game in PlayerData.GetVariable
        /// </summary>
        /// <param name="target">Target Field Name</param>
        internal static T GetPlayerVariable<T>(string target)
        {
            Type t = typeof(T);

            if (t == typeof(bool))
            {
                return (T)(object)GetPlayerBool(target);
            }

            if (t == typeof(int))
            {
                return (T)(object)GetPlayerInt(target);
            }

            if (t == typeof(float))
            {
                return (T)(object)GetPlayerFloat(target);
            }

            if (t == typeof(string))
            {
                return (T)(object)GetPlayerString(target);
            }

            if (t == typeof(Vector3))
            {
                return (T)(object)GetPlayerVector3(target);
            }

            T value = Patches.PlayerData.instance.GetVariableInternal<T>(target);

            if (GetPlayerVariableHook == null)
                return value;

            Delegate[] invocationList = GetPlayerVariableHook.GetInvocationList();

            foreach (GetVariableProxy toInvoke in invocationList)
            {
                try
                {
                    value = (T)toInvoke(t, target, value);
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError(ex);
                }
            }

            return value;
        }

        /// <summary>
        ///     Called whenever blue health is updated
        /// </summary>
        public static event Func<int> BlueHealthHook;

        /// <summary>
        ///     Called whenever blue health is updated
        /// </summary>
        internal static int OnBlueHealth()
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
                    Logger.APILogger.LogError(ex);
                }
            }

            return result;
        }


        /// <summary>
        ///     Called when health is taken from the player
        /// </summary>
        /// <remarks>HeroController.TakeHealth</remarks>
        public static event TakeHealthProxy TakeHealthHook;

        /// <summary>
        ///     Called when health is taken from the player
        /// </summary>
        /// <remarks>HeroController.TakeHealth</remarks>
        internal static int OnTakeHealth(int damage)
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
                    Logger.APILogger.LogError(ex);
                }
            }

            return damage;
        }

        /// <summary>
        ///     Called when damage is dealt to the player, at the start of the take damage function.
        /// </summary>
        /// <see cref="TakeDamageProxy"/>
        /// <remarks>HeroController.TakeDamage</remarks>
        public static event TakeDamageProxy TakeDamageHook;

        /// <summary>
        ///     Called when damage is dealt to the player, at the start of the take damage function.
        /// </summary>
        /// <remarks>HeroController.TakeDamage</remarks>
        internal static int OnTakeDamage(ref int hazardType, int damage)
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
                    Logger.APILogger.LogError(ex);
                }
            }

            return damage;
        }

        /// <summary>
        ///     Called in the take damage function, immediately before applying damage (just before checking overcharm)
        /// </summary>
        /// <see cref="AfterTakeDamageHandler"/>
        public static event AfterTakeDamageHandler AfterTakeDamageHook;

        /// <summary>
        ///     Called in the take damage function, immediately before applying damage (just before checking overcharm)
        /// </summary>
        internal static int AfterTakeDamage(int hazardType, int damageAmount)
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
                    Logger.APILogger.LogError(ex);
                }
            }

            return damageAmount;
        }

        /// <summary>
        ///     Called when the player dies
        /// </summary>
        /// <remarks>GameManager.PlayerDead</remarks>
        public static event Action BeforePlayerDeadHook;

        /// <summary>
        ///     Called when the player dies (at the beginning of the method)
        /// </summary>
        /// <remarks>GameManager.PlayerDead</remarks>
        internal static void OnBeforePlayerDead()
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
                    Logger.APILogger.LogError(ex);
                }
            }
        }

        /// <summary>
        ///     Called after the player dies
        /// </summary>
        /// <remarks>GameManager.PlayerDead</remarks>
        public static event Action AfterPlayerDeadHook;

        /// <summary>
        ///     Called after the player dies (at the end of the method)
        /// </summary>
        /// <remarks>GameManager.PlayerDead</remarks>
        internal static void OnAfterPlayerDead()
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
                    Logger.APILogger.LogError(ex);
                }
            }
        }

        /// <summary>
        ///     Called whenever the player attacks
        /// </summary>
        /// <remarks>HeroController.Attack</remarks>
        public static event Action<AttackDirection> AttackHook;

        /// <summary>
        ///     Called whenever the player attacks
        /// </summary>
        /// <remarks>HeroController.Attack</remarks>
        internal static void OnAttack(AttackDirection dir)
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
                    Logger.APILogger.LogError(ex);
                }
            }
        }

        /// <summary>
        ///     Called at the start of the DoAttack function
        /// </summary>
        public static event Action DoAttackHook;

        /// <summary>
        ///     Called at the start of the DoAttack function
        /// </summary>
        internal static void OnDoAttack()
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
                    Logger.APILogger.LogError(ex);
                }
            }
        }


        /// <summary>
        ///     Called at the end of the attack function
        /// </summary>
        /// <remarks>HeroController.Attack</remarks>
        [MonoModPublic]
        public static event Action<AttackDirection> AfterAttackHook;

        /// <summary>
        ///     Called at the end of the attack function
        /// </summary>
        /// <remarks>HeroController.Attack</remarks>
        internal static void AfterAttack(AttackDirection dir)
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
                    Logger.APILogger.LogError(ex);
                }
            }
        }

        /// <summary>
        ///     Called whenever nail strikes something
        /// </summary>
        /// <see cref="SlashHitHandler"/>
        public static event SlashHitHandler SlashHitHook;

        /// <summary>
        ///     Called whenever nail strikes something
        /// </summary>
        internal static void OnSlashHit(Collider2D otherCollider, GameObject gameObject)
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
                    Logger.APILogger.LogError(ex);
                }
            }
        }

        /// <summary>
        ///     Called after player values for charms have been set
        /// </summary>
        /// <see cref="CharmUpdateHandler"/>
        /// <remarks>HeroController.CharmUpdate</remarks>
        public static event CharmUpdateHandler CharmUpdateHook;

        /// <summary>
        ///     Called after player values for charms have been set
        /// </summary>
        /// <remarks>HeroController.CharmUpdate</remarks>
        internal static void OnCharmUpdate()
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
                    Logger.APILogger.LogError(ex);
                }
            }
        }

        /// <summary>
        ///     Called whenever the hero updates
        /// </summary>
        /// <remarks>HeroController.Update</remarks>
        public static event Action HeroUpdateHook;

        /// <summary>
        ///     Called whenever the hero updates
        /// </summary>
        /// <remarks>HeroController.Update</remarks>
        internal static void OnHeroUpdate()
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
                    Logger.APILogger.LogError(ex);
                }
            }
        }

        /// <summary>
        ///     Called whenever the player heals, overrides health added.
        /// </summary>
        /// <remarks>PlayerData.health</remarks>
        public static event Func<int, int> BeforeAddHealthHook;

        /// <summary>
        ///     Called whenever the player heals
        /// </summary>
        /// <remarks>PlayerData.health</remarks>
        internal static int BeforeAddHealth(int amount)
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
                    Logger.APILogger.LogError(ex);
                }
            }

            return amount;
        }

        /// <summary>
        ///     Called whenever focus cost is calculated, allows a focus cost multiplier.
        /// </summary>
        public static event Func<float> FocusCostHook;

        /// <summary>
        ///     Called whenever focus cost is calculated
        /// </summary>
        internal static float OnFocusCost()
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
                    Logger.APILogger.LogError(ex);
                }
            }

            return result;
        }

        /// <summary>
        ///     Called when Hero recovers Soul from hitting enemies
        /// </summary>
        /// <returns>The amount of soul to recover</returns>
        public static event Func<int, int> SoulGainHook;

        /// <summary>
        ///     Called when Hero recovers Soul from hitting enemies
        /// </summary>
        internal static int OnSoulGain(int num)
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
                    Logger.APILogger.LogError(ex);
                }
            }

            return num;
        }


        /// <summary>
        ///     Called during dash function to change velocity
        /// </summary>
        /// <returns>A changed vector.</returns>
        /// <remarks>HeroController.Dash</remarks>
        public static event Func<Vector2, Vector2> DashVectorHook;

        /// <summary>
        ///     Called during dash function to change velocity
        /// </summary>
        /// <remarks>HeroController.Dash</remarks>
        internal static Vector2 DashVelocityChange(Vector2 change)
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
                    Logger.APILogger.LogError(ex);
                }
            }

            return change;
        }

        /// <summary>
        ///     Called whenever the dash key is pressed.
        ///     Returns whether or not to override normal dash functionality - if true, preventing a normal dash
        /// </summary>
        /// <remarks>HeroController.LookForQueueInput</remarks>
        public static event Func<bool> DashPressedHook;

        /// <summary>
        ///     Called whenever the dash key is pressed. Returns whether or not to override normal dash functionality
        /// </summary>
        /// <remarks>HeroController.LookForQueueInput</remarks>
        internal static bool OnDashPressed()
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
                    Logger.APILogger.LogError(ex);
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
        public static event Action<int> SavegameLoadHook;

        /// <summary>
        ///     Called directly after a save has been loaded
        /// </summary>
        /// <remarks>GameManager.LoadGame</remarks>
        internal static void OnSavegameLoad(int id)
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
                    Logger.APILogger.LogError(ex);
                }
            }
        }

        /// <summary>
        ///     Called directly after a save has been saved
        /// </summary>
        /// <remarks>GameManager.SaveGame</remarks>
        public static event Action<int> SavegameSaveHook;

        /// <summary>
        ///     Called directly after a save has been saved
        /// </summary>
        /// <remarks>GameManager.SaveGame</remarks>
        internal static void OnSavegameSave(int id)
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
                    Logger.APILogger.LogError(ex);
                }
            }
        }

        /// <summary>
        ///     Called whenever a new game is started
        /// </summary>
        /// <remarks>GameManager.LoadFirstScene</remarks>
        public static event Action NewGameHook;

        /// <summary>
        ///     Called whenever a new game is started
        /// </summary>
        /// <remarks>GameManager.LoadFirstScene</remarks>
        internal static void OnNewGame()
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
                    Logger.APILogger.LogError(ex);
                }
            }
        }

        /// <summary>
        ///     Called before a save file is deleted
        /// </summary>
        /// <remarks>GameManager.ClearSaveFile</remarks>
        public static event Action<int> SavegameClearHook;

        /// <summary>
        ///     Called before a save file is deleted
        /// </summary>
        /// <remarks>GameManager.ClearSaveFile</remarks>
        internal static void OnSavegameClear(int id)
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
                    Logger.APILogger.LogError(ex);
                }
            }
        }

        /// <summary>
        ///     Called directly after a save has been loaded.  Allows for accessing SaveGame instance.
        /// </summary>
        /// <remarks>GameManager.LoadGame</remarks>
        public static event Action<SaveGameData> AfterSavegameLoadHook;

        /// <summary>
        ///     Called directly after a save has been loaded.  Allows for accessing SaveGame instance.
        /// </summary>
        /// <remarks>GameManager.LoadGame</remarks>
        internal static void OnAfterSaveGameLoad(SaveGameData data)
        {
            Logger.APILogger.LogFine("OnAfterSaveGameLoad Invoked");

            if (AfterSavegameLoadHook == null)
            {
                return;
            }

            Delegate[] invocationList = AfterSavegameLoadHook.GetInvocationList();

            foreach (Action<SaveGameData> toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke(data);
                }

                catch (Exception ex)
                {
                    Logger.APILogger.LogError(ex);
                }
            }
        }

        /// <summary>
        ///     Called directly before save has been saved to allow for changes to the data before persisted.
        /// </summary>
        /// <remarks>GameManager.SaveGame</remarks>
        public static event Action<SaveGameData> BeforeSavegameSaveHook;

        /// <summary>
        ///     Called directly before save has been saved to allow for changes to the data before persisted.
        /// </summary>
        /// <remarks>GameManager.SaveGame</remarks>
        internal static void OnBeforeSaveGameSave(SaveGameData data)
        {
            Logger.APILogger.LogFine("OnBeforeSaveGameSave Invoked");

            if (BeforeSavegameSaveHook == null)
            {
                return;
            }

            Delegate[] invocationList = BeforeSavegameSaveHook.GetInvocationList();

            foreach (Action<SaveGameData> toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke(data);
                }

                catch (Exception ex)
                {
                    Logger.APILogger.LogError(ex);
                }
            }
        }

        /// <summary>
        ///     Overrides the filename to load for a given slot.  Return null to use vanilla names.
        /// </summary>
        public static event Func<int, string> GetSaveFileNameHook;

        /// <summary>
        ///     Overrides the filename to load for a given slot.  Return null to use vanilla names.
        /// </summary>
        internal static string GetSaveFileName(int saveSlot)
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
                    Logger.APILogger.LogError(ex);
                }
            }

            return ret;
        }

        /// <summary>
        ///     Called after a game has been cleared from a slot.
        /// </summary>
        public static event Action<int> AfterSaveGameClearHook;

        /// <summary>
        ///     Called after a game has been cleared from a slot.
        /// </summary>
        internal static void OnAfterSaveGameClear(int saveSlot)
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
                    Logger.APILogger.LogError(ex);
                }
            }
        }

        #endregion

        internal static event Action<ModSavegameData> SaveLocalSettings;
        internal static event Action<ModSavegameData> LoadLocalSettings;

        internal static void OnSaveLocalSettings(ModSavegameData data)
        {
            data.loadedMods = LoadedModsWithVersions;
            SaveLocalSettings?.Invoke(data);
        }

        internal static void OnLoadLocalSettings(ModSavegameData data) => LoadLocalSettings?.Invoke(data);


        #region SceneHandling

        /// <summary>
        ///     Called after a new Scene has been loaded
        /// </summary>
        /// <remarks>N/A</remarks>
        public static event Action<string> SceneChanged;

        /// <summary>
        ///     Called after a new Scene has been loaded
        /// </summary>
        /// <remarks>N/A</remarks>
        internal static void OnSceneChanged(string targetScene)
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
                    Logger.APILogger.LogError(ex);
                }
            }
        }

        /// <summary>
        ///     Called right before a scene gets loaded, can change which scene gets loaded
        /// </summary>
        /// <remarks>N/A</remarks>
        public static event Func<string, string> BeforeSceneLoadHook;

        /// <summary>
        ///     Called right before a scene gets loaded, can change which scene gets loaded
        /// </summary>
        /// <remarks>N/A</remarks>
        internal static string BeforeSceneLoad(string sceneName)
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
                    Logger.APILogger.LogError(ex);
                }
            }

            return sceneName;
        }

        #endregion

        #region mod loading values

        /// <summary>
        /// Gets a mod instance by name.
        /// </summary>
        /// <param name="name">The name of the mod.</param>
        /// <param name="onlyEnabled">Should the method only return the mod if it is enabled.</param>
        /// <param name="allowLoadError">Should the method return the mod even if it had load errors.</param>
        /// <returns></returns>
        public static IMod GetMod(
            string name,
            bool onlyEnabled = false,
            bool allowLoadError = false
        ) => ModLoader.ModInstanceNameMap.TryGetValue(name, out var mod)
            && (!onlyEnabled || mod.Enabled)
            && (allowLoadError || mod.Error is null)
         ? mod.Mod : null;

        /// <summary>
        /// Gets a mod instance by type.
        /// </summary>
        /// <param name="type">The type of the mod.</param>
        /// <param name="onlyEnabled">Should the method only return the mod if it is enabled.</param>
        /// <param name="allowLoadError">Should the method return the mod even if it had load errors.</param>
        /// <returns></returns>
        public static IMod GetMod(
            Type type,
            bool onlyEnabled = false,
            bool allowLoadError = false
        ) => ModLoader.ModInstanceTypeMap.TryGetValue(type, out var mod) 
            && (!onlyEnabled || mod.Enabled) 
            && (allowLoadError || mod.Error is null)
         ? mod.Mod : null;

        /// <summary>
        /// Gets if the mod is currently enabled.
        /// </summary>
        /// <param name="mod">The togglable mod to check.</param>
        /// <returns></returns>
        public static bool ModEnabled(
            ITogglableMod mod
        ) => ModEnabled(mod.GetType());

        /// <summary>
        /// Gets if a mod is currently enabled.
        /// </summary>
        /// <param name="name">The name of the mod to check.</param>
        /// <returns></returns>
        public static bool ModEnabled(
            string name
        ) => ModLoader.ModInstanceNameMap.TryGetValue(name, out var instance) ? instance.Enabled : true;

        /// <summary>
        /// Gets if a mod is currently enabled.
        /// </summary>
        /// <param name="type">The type of the mod to check.</param>
        /// <returns></returns>
        public static bool ModEnabled(
            Type type
        ) => ModLoader.ModInstanceTypeMap.TryGetValue(type, out var instance) ? instance.Enabled : true;

        /// <summary>
        /// Returns an iterator over all mods.
        /// </summary>
        /// <param name="onlyEnabled">Should the iterator only contain enabled mods.</param>
        /// <param name="allowLoadError">Should the iterator contain mods which have load errors.</param>
        /// <returns></returns>
        public static IEnumerable<IMod> GetAllMods(
            bool onlyEnabled = false,
            bool allowLoadError = false
        ) => ModLoader.ModInstances
            .Where(x => (!onlyEnabled || x.Enabled) && (allowLoadError || x.Error is null))
            .Select(x => x.Mod);

        #endregion

        private static event Action _finishedLoadingModsHook;
        
        /// <summary>
        /// Event invoked when mods have finished loading. If modloading has already finished, subscribers will be invoked immediately.
        /// </summary>
        public static event Action FinishedLoadingModsHook
        {
            add
            {
                _finishedLoadingModsHook += value;
                
                if (!ModLoader.LoadState.HasFlag(ModLoader.ModLoadState.Loaded)) 
                    return;
                
                try
                {
                    value.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError(ex);
                }
            }
            remove => _finishedLoadingModsHook -= value;
        }
        
        internal static void OnFinishedLoadingMods()
        {
            if (_finishedLoadingModsHook == null)
            {
                return;
            }

            Delegate[] invocationList = _finishedLoadingModsHook.GetInvocationList();

            foreach (Action toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError(ex);
                }
            }
        }
    }
}
