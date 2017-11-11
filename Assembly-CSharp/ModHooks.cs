using System;
using System.Collections.Generic;
using System.IO;
using GlobalEnums;
using UnityEngine;

namespace Modding
{
    /// <summary>
    /// Class to hook into various events for the game.
    /// </summary>
	public class ModHooks
    {
        private static readonly string LogPath = Application.persistentDataPath + "\\ModLog.txt";

        /// <summary>
        /// Provides access to logging system.
        /// </summary>
        public static Logger Logger => _logger ?? (_logger = new Logger(LogLevel.Debug, LogPath));

        private static Logger _logger;

        public List<string> LoadedMods = new List<string>();
        public string ModVersion;

        private static int _modVersion = 4;

        public GameVersionData version;

        private ModHooks()
        {
            GameVersion gameVersion;
            gameVersion.major = 1;
            gameVersion.minor = 2;
            gameVersion.revision = 1;
            gameVersion.package = 4;
            version = new GameVersionData {gameVersion = gameVersion};

            ModVersion = version.GetGameVersionString() + "-" + _modVersion;
            if (File.Exists(LogPath))
                File.Delete(LogPath);
        }

        /// <summary>
        /// Current instance of Modhooks.
        /// </summary>
	    public static ModHooks Instance => _instance ?? (_instance = new ModHooks());

        /// <summary>
        /// Logs the message to ModLog.txt in the save file path.
        /// </summary>
        /// <param name="info">Message To Log</param>
        [Obsolete("This method is obsolete and will be removed in future Mod API Versions. Use ModHooks.Instance.Logger instead.")]
        public static void ModLog(string info)
        {
            Logger.Log(info);
        }

        #region PlayerManagementHandling

        /// <summary>
        /// Called when anything in the game tries to set a bool in player data
        /// </summary>
        /// <remarks>PlayerData.SetBool</remarks>
        [HookInfo("Called when anything in the game tries to set a bool in player data", "PlayerData.SetBool")]
        public event SetBoolProxy SetPlayerBoolHook;

        public void SetPlayerBool(string target, bool val)
        {
            if (SetPlayerBoolHook != null)
            {
                SetPlayerBoolHook(target, val);
                return;
            }
            PlayerData.instance.SetBoolInternal(target, val);
        }


        /// <summary>
        /// Called when anything in the game tries to get a bool from player data
        /// </summary>
        /// <remarks>PlayerData.GetBool</remarks>
        [HookInfo("Called when anything in the game tries to get a bool from player data", "PlayerData.GetBool")]
        public event GetBoolProxy GetPlayerBoolHook;

        public bool GetPlayerBool(string target)
        {
            bool boolInternal = PlayerData.instance.GetBoolInternal(target);
            bool result = boolInternal;
            bool flag = false;
            if (GetPlayerBoolHook == null) return result;

            Delegate[] invocationList = GetPlayerBoolHook.GetInvocationList();
            foreach (Delegate toInvoke in invocationList)
            {
                bool flag2 = (bool)toInvoke.DynamicInvoke(target);
                if (flag2 == boolInternal || flag) continue;

                result = flag2;
                flag = true;
            }
            return result;
        }

        /// <summary>
        /// Called when anything in the game tries to set an int in player data
        /// </summary>
        /// <remarks>PlayerData.SetInt</remarks>
        [HookInfo("Called when anything in the game tries to set an int in player data", "PlayerData.SetInt")]
        public event SetIntProxy SetPlayerIntHook;

        public void SetPlayerInt(string target, int val)
        {
            if (SetPlayerIntHook != null)
            {
                SetPlayerIntHook(target, val);
                return;
            }
            PlayerData.instance.SetIntInternal(target, val);
        }

        /// <summary>
        /// Called when anything in the game tries to get an int from player data
        /// </summary>
        /// <remarks>PlayerData.GetInt</remarks>
        [HookInfo("Called when anything in the game tries to get an int from player data", "PlayerData.GetInt")]
        public event GetIntProxy GetPlayerIntHook;

        public int GetPlayerInt(string target)
        {
            int intInternal = PlayerData.instance.GetIntInternal(target);
            int result = intInternal;
            bool flag = false;
            if (GetPlayerIntHook == null) return result;

            Delegate[] invocationList = GetPlayerIntHook.GetInvocationList();
            foreach (Delegate toInvoke in invocationList)
            {
                int num = (int)toInvoke.DynamicInvoke(target);
                if (num == intInternal || flag) continue;

                result = num;
                flag = true;
            }
            return result;
        }

        /// <summary>
        /// Called after setting up a new PlayerData
        /// </summary>
        /// <remarks>PlayerData.SetupNewPlayerData</remarks>
        [HookInfo("Called after setting up a new PlayerData", "PlayerData.SetupNewPlayerData")]
        public event NewPlayerDataHandler NewPlayerDataHook;

        public void AfterNewPlayerData()
        {
            NewPlayerDataHook?.Invoke(PlayerData.instance);
        }

        /// <summary>
        /// Called whenever blue health is updated
        /// </summary>
        [HookInfo("Called whenever blue health is updated", "PlayerData.UpdateBlueHealth")]
        public event BlueHealthHandler BlueHealthHook;

        public int OnBlueHealth()
        {
            int result = 0;
            if (BlueHealthHook == null) return result;

            Delegate[] invocationList = BlueHealthHook.GetInvocationList();
            foreach (Delegate toInvoke in invocationList)
            {
                result = (int)toInvoke.DynamicInvoke();
            }
            return result;
        }


        /// <summary>
        /// Called when health is taken from the player
        /// </summary>
        /// <remarks>HeroController.TakeHealth</remarks>
        [HookInfo("Called when health is taken from the player", "PlayerData.TakeHealth")]
        public event TakeHealthProxy TakeHealthHook;

        public int OnTakeHealth(int damage)
        {
            if (TakeHealthHook == null) return damage;

            Delegate[] invocationList = TakeHealthHook.GetInvocationList();
            foreach (Delegate toInvoke in invocationList)
            {
                damage = (int)toInvoke.DynamicInvoke(damage);
            }
            return damage;
        }

        /// <summary>
        /// Called when damage is dealt to the player
        /// </summary>
        /// <remarks>HeroController.TakeDamage</remarks>
        [HookInfo("Called when damage is dealt to the player", "HeroController.TakeDamage")]
        public event TakeDamageProxy TakeDamageHook;

        public int OnTakeDamage(ref int hazardType, int damage)
        {
            if (TakeDamageHook == null) return damage;

            Delegate[] invocationList = TakeDamageHook.GetInvocationList();
            foreach (Delegate toInvoke in invocationList)
            {
                damage = (int)toInvoke.DynamicInvoke(hazardType, damage);
            }
            return damage;
        }

        /// <summary>
        /// Called at the end of the take damage function
        /// </summary>
        [HookInfo("Called at the end of the take damage function", "HeroController.TakeDamage")]
        public event AfterTakeDamageHandler AfterTakeDamageHook;

        public void AfterTakeDamage(int hazardType, int damageAmount)
        {
            AfterTakeDamageHook?.Invoke(hazardType, damageAmount);
        }
        

        /// <summary>
        /// Called whenever the player attacks
        /// </summary>
        /// <remarks>HeroController.Attack</remarks>
        [HookInfo("Called whenever the player attacks", "HeroController.Attack")]
        public event AttackHandler AttackHook;

        public void OnAttack(AttackDirection dir)
        {
            AttackHook?.Invoke(dir);
        }

        /// <summary>
        /// Called at the start of the DoAttack function
        /// </summary>
        [HookInfo("Called at the start of the DoAttack function", "HeroController.DoAttack")]
        public event DoAttackHandler DoAttackHook;

        public void OnDoAttack()
        {
            DoAttackHook?.Invoke();
        }
        

        /// <summary>
        /// Called at the end of the attack function
        /// </summary>
        /// <remarks>HeroController.Attack</remarks>
        [HookInfo("Called at the end of the attack function", "HeroController.Attack")]
        public event AfterAttackHandler AfterAttackHook;

        public void AfterAttack(AttackDirection dir)
        {
            AfterAttackHook?.Invoke(dir);
        }

        /// <summary>
        /// Called whenever nail strikes something
        /// </summary>
        [HookInfo("Called whenever nail strikes something", "NailSlash.OnTriggerEnter2D")]
        public event SlashHitHandler SlashHitHook;

        public void OnSlashHit(Collider2D otherCollider, GameObject gameObject)
        {
            SlashHitHook?.Invoke(otherCollider, gameObject);
        }
        
        /// <summary>
        /// Called after player values for charms have been set
        /// </summary>
        /// <remarks>HeroController.CharmUpdate</remarks>
        [HookInfo("Called after player values for charms have been set", "HeroController.CharmUpdate")]
        public event CharmUpdateHandler CharmUpdateHook;

        public void OnCharmUpdate()
        {
            CharmUpdateHook?.Invoke(PlayerData.instance, HeroController.instance);
        }

        /// <summary>
        /// Called whenever the hero updates
        /// </summary>
        /// <remarks>HeroController.Update</remarks>
        [HookInfo("Called whenever the hero updates", "HeroController.Update")]
        public event HeroUpdateHandler HeroUpdateHook;

        public void OnHeroUpdate()
        {
            HeroUpdateHook?.Invoke();
        }

        /// <summary>
        /// Called whenever focus cost is calculated
        /// </summary>
        [HookInfo("Called whenever focus cost is calculated", "HeroController.StartMPDrain")]
        public event FocusCostHandler FocusCostHook;

        public int OnFocusCost()
        {
            int result = 1;
            if (FocusCostHook == null) return result;

            Delegate[] invocationList = FocusCostHook.GetInvocationList();
            foreach (Delegate toInvoke in invocationList)
            {
                result = (int)toInvoke.DynamicInvoke();
            }
            return result;
        }

        /// <summary>
        /// Called when Hero recovers Soul from hitting enemies
        /// </summary>
        [HookInfo("Called when Hero recovers Soul from hitting enemies", "HeroController.SoulGain")]
        public event SoulGainHandler SoulGainHook;

        public int OnSoulGain(int num)
        {
            if (SoulGainHook == null) return num;

            Delegate[] invocationList = SoulGainHook.GetInvocationList();
            foreach (Delegate toInvoke in invocationList)
            {
                num = (int)toInvoke.DynamicInvoke(num);
            }
            return num;
        }


        /// <summary>
        /// Called during dash function to change velocity
        /// </summary>
        /// <remarks>HeroController.Dash</remarks>
        [HookInfo("Called during dash function to change velocity", "HeroController.Dash")]
        public event DashVelocityHandler DashVectorHook;

        public Vector2 DashVelocityChange()
        {
            return DashVectorHook?.Invoke() ?? Vector2.zero;
        }

        /// <summary>
        /// Called whenever the dash key is pressed. Overrides normal dash functionality
        /// </summary>
        /// <remarks>HeroController.LookForQueueInput</remarks>
        [HookInfo("Called whenever the dash key is pressed. Overrides normal dash functionality", "HeroController.LookForQueueInput")]
        public event DashPressedHandler DashPressedHook;

        public bool OnDashPressed()
        {
            if (DashPressedHook == null) return false;

            DashPressedHook();
            return true;
        }

        #endregion


        #region SaveHandling


        /// <summary>
        /// Called directly after a save has been loaded
        /// </summary>
        /// <remarks>GameManager.LoadGame</remarks>
        [HookInfo("Called directly after a save has been loaded", "GameManager.LoadGame")]
        public event SavegameLoadHandler SavegameLoadHook;

        public void OnSavegameLoad(int id)
        {
            SavegameLoadHook?.Invoke(id);
        }

        /// <summary>
        /// Called directly after a save has been saved
        /// </summary>
        /// <remarks>GameManager.SaveGame</remarks>
        [HookInfo("Called directly after a save has been saved", "GameManager.SaveGame")]
        public event SavegameSaveHandler SavegameSaveHook;

        public void OnSavegameSave(int id)
        {
            SavegameSaveHook?.Invoke(id);
        }

        /// <summary>
        /// Called whenever a new game is started
        /// </summary>
        /// <remarks>GameManager.LoadFirstScene</remarks>
        [HookInfo("Called whenever a new game is started", "GameManager.LoadFirstScene")]
        public event NewGameHandler NewGameHook;

        public void OnNewGame()
        {
            NewGameHook?.Invoke();
        }

        /// <summary>
        /// Called before a save file is deleted
        /// </summary>
        /// <remarks>GameManager.ClearSaveFile</remarks>
        [HookInfo("Called whenever a save file is deleted", "GameManager.ClearSaveFile")]
        public event ClearSaveGameHandler SavegameClearHook;

        public void OnSavegameClear(int id)
        {
            SavegameClearHook?.Invoke(id);
        }

        /// <summary>
        /// Called directly after a save has been loaded.  Allows for accessing SaveGame instance.
        /// </summary>
        /// <remarks>GameManager.LoadGame</remarks>
        [HookInfo("Called directly after a save has been loaded.  Allows for accessing SaveGame instance.", "GameManager.LoadGame")]
        public event AfterSavegameLoadHandler AfterSavegameLoadHook;

        public void OnAfterSaveGameLoad(SaveGameData data)
        {
            AfterSavegameLoadHook?.Invoke(data);
        }

        /// <summary>
        /// Called directly before save has been saved to allow for changes to the data before persisted.
        /// </summary>
        /// <remarks>GameManager.SaveGame</remarks>
        [HookInfo("Called directly before save has been saved to allow for changes to the data before persisted.", "GameManager.SaveGame")]
        public event BeforeSavegameSaveHandler BeforeSavegameSaveHook;

        public void OnBeforeSaveGameSave(SaveGameData data)
        {
            BeforeSavegameSaveHook?.Invoke(data);
        }

        /// <summary>
        /// Overrides the filename to load for a given slot.  Return null to use vanilla names.
        /// </summary>
        [HookInfo("Overrides the filename for a slot.", "GameManager.SaveGameClear")]
        public event GetSaveFileNameHandler GetSaveFileNameHook;

        public string GetSaveFileName(int saveSlot)
        {
            return GetSaveFileNameHook?.Invoke(saveSlot);
        }

        /// <summary>
        /// Called after a game has been cleared from a slot.
        /// </summary>
        [HookInfo("Called after a savegame has been cleared.", "GameManager.GetSaveFilename")]
        public event AfterClearSaveGameHandler AfterSaveGameClearHook;

        public void OnAfterSaveGameClear(int saveSlot)
        {
            AfterSaveGameClearHook?.Invoke(saveSlot);
        }
        
        #endregion

        /// <summary>
        /// Called whenever localization specific strings are requested
        /// </summary>
        /// <remarks>N/A</remarks>
        [HookInfo("Called whenever localization specific strings are requested", "N/A")]
        public event LanguageGetHandler LanguageGetHook;

        public string LanguageGet(string key, string sheet)
        {
            string @internal = Language.Language.GetInternal(key, sheet);
            string result = @internal;
            bool flag = false;
            if (LanguageGetHook == null) return result;

            Delegate[] invocationList = LanguageGetHook.GetInvocationList();
            foreach (Delegate toInvoke in invocationList)
            {
                string text = (string)toInvoke.DynamicInvoke(key, sheet);
                if (text == @internal || flag) continue;

                result = text;
                flag = true;
            }
            return result;
        }

        #region SceneHandling

        /// <summary>
        /// Called after a new Scene has been loaded
        /// </summary>
        /// <remarks>N/A</remarks>
        [HookInfo("Called after a new Scene has been loaded", "GameManager.LoadScene")]
        public event SceneChangedHandler SceneChanged;

        public void OnSceneChanged(string targetScene)
        {
            SceneChanged?.Invoke(targetScene);
        }

        /// <summary>
        /// Called right before a scene gets loaded, can change which scene gets loaded
        /// </summary>
        /// <remarks>N/A</remarks>
        [HookInfo("Called right before a scene gets loaded, can change which scene gets loaded", "GameManager.LoadScene")]
        public event BeforeSceneLoadHandler BeforeSceneLoadHook;

        public string BeforeSceneLoad(string sceneName)
        {
            if (BeforeSceneLoadHook == null) return sceneName;

            Delegate[] invocationList = BeforeSceneLoadHook.GetInvocationList();
            foreach (Delegate toInvoke in invocationList)
            {
                sceneName = (string)toInvoke.DynamicInvoke(sceneName);
            }
            return sceneName;
        }

        #endregion

        /// <summary>
        /// Called whenever game tries to show cursor
        /// </summary>
        [HookInfo("Called whenever game tries to show cursor", "InputHandler.OnGUI")]
        public event CursorHandler CursorHook;

        public void OnCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            if (CursorHook != null)
            {
                CursorHook();
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
        /// Called whenever a new gameobject is created with a collider and playmaker2d
        /// </summary>
        /// <remarks>PlayMakerUnity2DProxy.Start</remarks>
        [HookInfo("Called whenever a new gameobject is created with a collider and playmaker2d", "PlayMakerUnity2DProxy.Start")]
        public event ColliderCreateHandler ColliderCreateHook;

        public void OnColliderCreate(GameObject go)
        {
            ColliderCreateHook?.Invoke(go);
        }

        /// <summary>
        /// Called when the game is fully closed
        /// </summary>
        /// <remarks>GameManager.OnApplicationQuit</remarks>
        [HookInfo("Called when the game is fully closed", "GameManager.OnApplicationQuit")]
        public event ApplicationQuitHandler ApplicationQuitHook;

        public void OnApplicationQuit()
        {
            ApplicationQuitHook?.Invoke();
        }


        private static ModHooks _instance;
    }
}
