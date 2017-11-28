using System;
using System.Collections.Generic;
using System.IO;
using GlobalEnums;
using MonoMod;
using UnityEngine;


namespace Modding
{
    /// <summary>
    /// Class to hook into various events for the game.
    /// </summary>
	public class ModHooks
    {
        private static int _modVersion = 20;
        

        private static readonly string LogPath = Application.persistentDataPath + "\\ModLog.txt";
        private static readonly string SettingsPath = Application.persistentDataPath + "\\ModdingApi.GlobalSettings.json";
        private static ModHooks _instance;

        private static ModHooksGlobalSettings _globalSettings;

        private static ModHooksGlobalSettings GlobalSettings
        {
            get
            {
                if (_globalSettings != null) return _globalSettings;

                LoadGlobalSettings();
                SaveGlobalSettings();
                return _globalSettings;
            }
        }

        /// <summary>
        /// Provides access to logging system.
        /// </summary>
        public static Logger Logger => _logger ?? (_logger = new Logger(GlobalSettings.LoggingLevel, LogPath));

        private static Logger _logger;

        /// <summary>
        /// Currently Loaded Mods
        /// </summary>
        public List<string> LoadedMods = new List<string>();

        /// <summary>
        /// Dictionary of mods and their version #s
        /// </summary>
        public SerializableStringDictionary LoadedModsWithVersions = new SerializableStringDictionary();

        /// <summary>
        /// The Version of the Modding API
        /// </summary>
        public string ModVersion;


        /// <summary>
        /// Version of the Game
        /// </summary>
        public GameVersionData version;

        private ModHooks()
        {

            GameVersion gameVersion;
            gameVersion.major = 1;
            gameVersion.minor = 2;
            gameVersion.revision = 2;
            gameVersion.package = 1;
            version = new GameVersionData { gameVersion = gameVersion };

            ModVersion = version.GetGameVersionString() + "-" + _modVersion;
            if (File.Exists(LogPath))
                File.Delete(LogPath);

        }


        /// <summary>
        /// Current instance of Modhooks.
        /// </summary>
	    public static ModHooks Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = new ModHooks();
                _instance.ApplicationQuitHook += SaveGlobalSettings;
                return _instance;
            }
        }

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
        public event SetBoolProxy SetPlayerBoolHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding SetPlayerBoolHook");
                _SetPlayerBoolHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing SetPlayerBoolHook");
                _SetPlayerBoolHook -= value;
            }
        }

        private event SetBoolProxy _SetPlayerBoolHook;

        /// <summary>
        /// Called by the game in PlayerData.SetBool 
        /// </summary>
        /// <param name="target">Target Field Name</param>
        /// <param name="val">Value to set</param>
        internal void SetPlayerBool(string target, bool val)
        {
            if (_SetPlayerBoolHook != null)
            {
                _SetPlayerBoolHook(target, val);
                return;
            }
            Patches.PlayerData.instance.SetBoolInternal(target, val);
        }


        /// <summary>
        /// Called when anything in the game tries to get a bool from player data
        /// </summary>
        /// <remarks>PlayerData.GetBool</remarks>
        [HookInfo("Called when anything in the game tries to get a bool from player data", "PlayerData.GetBool")]
        public event GetBoolProxy GetPlayerBoolHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding GetPlayerBoolHook");
                _GetPlayerBoolHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing GetPlayerBoolHook");
                _GetPlayerBoolHook -= value;
            }
        }

        private event GetBoolProxy _GetPlayerBoolHook;

        /// <summary>
        /// Called by the game in PlayerData.GetBool
        /// </summary>
        /// <param name="target">Target Field Name</param>
        internal bool GetPlayerBool(string target)
        {
            //Logger.LogFine("[API] - GetPlayerbool Invoked"); //Probably not going to enable this, even in Fine, Likely going to produce far too much 

            bool boolInternal = Patches.PlayerData.instance.GetBoolInternal(target);
            bool result = boolInternal;
            bool flag = false;
            if (_GetPlayerBoolHook == null) return result;

            Delegate[] invocationList = _GetPlayerBoolHook.GetInvocationList();
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
        public event SetIntProxy SetPlayerIntHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding SetPlayerIntHook");
                _SetPlayerIntHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing SetPlayerIntHook");
                _SetPlayerIntHook -= value;
            }
        }

        private event SetIntProxy _SetPlayerIntHook;

        /// <summary>
        /// Called by the game in PlayerData.SetInt 
        /// </summary>
        /// <param name="target">Target Field Name</param>
        /// <param name="val">Value to set</param>
        internal void SetPlayerInt(string target, int val)
        {
            if (_SetPlayerIntHook != null)
            {
                _SetPlayerIntHook(target, val);
                return;
            }
            Patches.PlayerData.instance.SetIntInternal(target, val);
        }

        /// <summary>
        /// Called when anything in the game tries to get an int from player data
        /// </summary>
        /// <remarks>PlayerData.GetInt</remarks>
        [HookInfo("Called when anything in the game tries to get an int from player data", "PlayerData.GetInt")]
        public event GetIntProxy GetPlayerIntHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding GetPlayerIntHook");
                _GetPlayerIntHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing GetPlayerIntHook");
                _GetPlayerIntHook -= value;
            }
        }

        private event GetIntProxy _GetPlayerIntHook;

        /// <summary>
        /// Called by the game in PlayerData.GetInt 
        /// </summary>
        /// <param name="target">Target Field Name</param>
        internal int GetPlayerInt(string target)
        {
            //Logger.LogFine("[API] - GetPlayerInt Invoked"); //Probably not going to enable this, even in Fine, Likely going to produce far too much 

            int intInternal = Patches.PlayerData.instance.GetIntInternal(target);
            int result = intInternal;
            bool flag = false;
            if (_GetPlayerIntHook == null) return result;

            Delegate[] invocationList = _GetPlayerIntHook.GetInvocationList();
            foreach (Delegate toInvoke in invocationList)
            {
                int num = (int)toInvoke.DynamicInvoke(target);
                if (num == intInternal || flag) continue;

                result = num;
                flag = true;
            }
            return result;
        }

        private event NewPlayerDataHandler _NewPlayerDataHook;

        /// <summary>
        /// Called after setting up a new PlayerData
        /// </summary>
        /// <remarks>PlayerData.SetupNewPlayerData</remarks>
        [HookInfo("Called after setting up a new PlayerData", "PlayerData.SetupNewPlayerData")]
        [Obsolete("Do Not Use - This is called too often due to a bug in the vanilla game's FSM handling.", true)]
        public event NewPlayerDataHandler NewPlayerDataHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding NewPlayerDataHook");
                _NewPlayerDataHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing NewPlayerDataHook");
                _NewPlayerDataHook -= value;
            }
        }

        /// <summary>
        /// Called after setting up a new PlayerData.SetupNewPlayerData
        /// </summary>
        internal void AfterNewPlayerData(PlayerData instance)
        {
            Logger.LogFine("[API] - AfterNewPlayerData Invoked");
            _NewPlayerDataHook?.Invoke(instance);
        }

        /// <summary>
        /// Called whenever blue health is updated
        /// </summary>
        [HookInfo("Called whenever blue health is updated", "PlayerData.UpdateBlueHealth")]
        public event BlueHealthHandler BlueHealthHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding BlueHealthHook");
                _BlueHealthHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing BlueHealthHook");
                _BlueHealthHook -= value;
            }
        }

        private event BlueHealthHandler _BlueHealthHook;

        /// <summary>
        /// Called whenever blue health is updated
        /// </summary>
        internal int OnBlueHealth()
        {
            Logger.LogFine("[API] - OnBlueHealth Invoked");

            int result = 0;
            if (_BlueHealthHook == null) return result;

            Delegate[] invocationList = _BlueHealthHook.GetInvocationList();
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
        public event TakeHealthProxy TakeHealthHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding TakeHealthHook");
                _TakeHealthHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing TakeHealthHook");
                _TakeHealthHook -= value;
            }
        }

        private event TakeHealthProxy _TakeHealthHook;

        /// <summary>
        /// Called when health is taken from the player
        /// </summary>
        /// <remarks>HeroController.TakeHealth</remarks>
        internal int OnTakeHealth(int damage)
        {
            Logger.LogFine("[API] - OnTakeHealth Invoked");

            if (_TakeHealthHook == null) return damage;

            Delegate[] invocationList = _TakeHealthHook.GetInvocationList();
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
        public event TakeDamageProxy TakeDamageHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding TakeDamageHook");
                _TakeDamageHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing TakeDamageHook");
                _TakeDamageHook -= value;
            }
        }

        private event TakeDamageProxy _TakeDamageHook;

        /// <summary>
        /// Called when damage is dealt to the player
        /// </summary>
        /// <remarks>HeroController.TakeDamage</remarks>
        internal int OnTakeDamage(ref int hazardType, int damage)
        {
            Logger.LogFine("[API] - OnTakeDamage Invoked");

            if (_TakeDamageHook == null) return damage;

            Delegate[] invocationList = _TakeDamageHook.GetInvocationList();
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
        public event AfterTakeDamageHandler AfterTakeDamageHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding AfterTakeDamageHook");
                _AfterTakeDamageHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing AfterTakeDamageHook");
                _AfterTakeDamageHook -= value;
            }
        }

        private event AfterTakeDamageHandler _AfterTakeDamageHook;

        /// <summary>
        /// Called at the end of the take damage function
        /// </summary>
        internal int AfterTakeDamage(int hazardType, int damageAmount)
        {
            Logger.LogFine("[API] - AfterTakeDamage Invoked");

            if (_AfterTakeDamageHook == null) return damageAmount;

            Delegate[] invocationList = _AfterTakeDamageHook.GetInvocationList();
            foreach (Delegate toInvoke in invocationList)
            {
                damageAmount = (int)toInvoke.DynamicInvoke(hazardType, damageAmount);
            }
            return damageAmount;
        }


        /// <summary>
        /// Called whenever the player attacks
        /// </summary>
        /// <remarks>HeroController.Attack</remarks>
        [HookInfo("Called whenever the player attacks", "HeroController.Attack")]
        public event AttackHandler AttackHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding AttackHook");
                _AttackHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing AttackHook");
                _AttackHook -= value;
            }
        }

        private event AttackHandler _AttackHook;

        /// <summary>
        /// Called whenever the player attacks
        /// </summary>
        /// <remarks>HeroController.Attack</remarks>
        internal void OnAttack(AttackDirection dir)
        {
            Logger.LogFine("[API] - OnAttack Invoked");

            _AttackHook?.Invoke(dir);
        }

        /// <summary>
        /// Called at the start of the DoAttack function
        /// </summary>
        [HookInfo("Called at the start of the DoAttack function", "HeroController.DoAttack")]
        public event DoAttackHandler DoAttackHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding DoAttackHook");
                _DoAttackHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing DoAttackHook");
                _DoAttackHook -= value;
            }
        }

        private event DoAttackHandler _DoAttackHook;


        /// <summary>
        /// Called at the start of the DoAttack function
        /// </summary>
        internal void OnDoAttack()
        {
            Logger.LogFine("[API] - OnDoAttack Invoked");

            _DoAttackHook?.Invoke();
        }


        /// <summary>
        /// Called at the end of the attack function
        /// </summary>
        /// <remarks>HeroController.Attack</remarks>
        [HookInfo("Called at the end of the attack function", "HeroController.Attack")]
        [MonoModPublic]
        public event AfterAttackHandler AfterAttackHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding AfterAttackHook");
                _AfterAttackHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing AfterAttackHook");
                _AfterAttackHook -= value;
            }
        }

        private event AfterAttackHandler _AfterAttackHook;

        /// <summary>
        /// Called at the end of the attack function
        /// </summary>
        /// <remarks>HeroController.Attack</remarks>
        internal void AfterAttack(AttackDirection dir)
        {
            Logger.LogFine("[API] - AfterAttack Invoked");

            _AfterAttackHook?.Invoke(dir);
        }

        /// <summary>
        /// Called whenever nail strikes something
        /// </summary>
        [HookInfo("Called whenever nail strikes something", "NailSlash.OnTriggerEnter2D")]
        public event SlashHitHandler SlashHitHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding SlashHitHook");
                _SlashHitHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing SlashHitHook");
                _SlashHitHook -= value;
            }
        }

        private event SlashHitHandler _SlashHitHook;

        /// <summary>
        /// Called whenever nail strikes something
        /// </summary>
        internal void OnSlashHit(Collider2D otherCollider, GameObject gameObject)
        {
            Logger.LogFine("[API] - OnSlashHit Invoked");

            if (otherCollider == null) return;

            _SlashHitHook?.Invoke(otherCollider, gameObject);
        }

        /// <summary>
        /// Called after player values for charms have been set
        /// </summary>
        /// <remarks>HeroController.CharmUpdate</remarks>
        [HookInfo("Called after player values for charms have been set", "HeroController.CharmUpdate")]
        public event CharmUpdateHandler CharmUpdateHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding CharmUpdateHook");
                _CharmUpdateHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing CharmUpdateHook");
                _CharmUpdateHook -= value;
            }
        }

        private event CharmUpdateHandler _CharmUpdateHook;


        /// <summary>
        /// Called after player values for charms have been set
        /// </summary>
        /// <remarks>HeroController.CharmUpdate</remarks>
        internal void OnCharmUpdate()
        {
            Logger.LogFine("[API] - OnCharmUpdate Invoked");

            _CharmUpdateHook?.Invoke(PlayerData.instance, HeroController.instance);
        }

        /// <summary>
        /// Called whenever the hero updates
        /// </summary>
        /// <remarks>HeroController.Update</remarks>
        [HookInfo("Called whenever the hero updates", "HeroController.Update")]
        public event HeroUpdateHandler HeroUpdateHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding HeroUpdateHook");
                _HeroUpdateHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing HeroUpdateHook");
                _HeroUpdateHook -= value;
            }
        }

        private event HeroUpdateHandler _HeroUpdateHook;


        /// <summary>
        /// Called whenever the hero updates
        /// </summary>
        /// <remarks>HeroController.Update</remarks>
        internal void OnHeroUpdate()
        {
            //Logger.LogFine("[API] - OnHeroUpdate Invoked");

            _HeroUpdateHook?.Invoke();
        }

        /// <summary>
        /// Called whenever focus cost is calculated
        /// </summary>
        [HookInfo("Called whenever focus cost is calculated", "HeroController.StartMPDrain")]
        public event FocusCostHandler FocusCostHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding FocusCostHook");
                _FocusCostHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing FocusCostHook");
                _FocusCostHook -= value;
            }
        }

        private event FocusCostHandler _FocusCostHook;

        /// <summary>
        /// Called whenever focus cost is calculated
        /// </summary>
        internal float OnFocusCost()
        {
            Logger.LogFine("[API] - OnFocusCost Invoked");

            float result = 1f;
            if (_FocusCostHook == null) return result;

            Delegate[] invocationList = _FocusCostHook.GetInvocationList();
            foreach (Delegate toInvoke in invocationList)
            {
                result = (float)toInvoke.DynamicInvoke();
            }
            return result;
        }

        /// <summary>
        /// Called when Hero recovers Soul from hitting enemies
        /// </summary>
        [HookInfo("Called when Hero recovers Soul from hitting enemies", "HeroController.SoulGain")]
        public event SoulGainHandler SoulGainHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding SoulGainHook");
                _SoulGainHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing SoulGainHook");
                _SoulGainHook -= value;
            }
        }

        private event SoulGainHandler _SoulGainHook;


        /// <summary>
        /// Called when Hero recovers Soul from hitting enemies
        /// </summary>
        internal int OnSoulGain(int num)
        {
            Logger.LogFine("[API] - OnSoulGain Invoked");

            if (_SoulGainHook == null) return num;

            Delegate[] invocationList = _SoulGainHook.GetInvocationList();
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
        public event DashVelocityHandler DashVectorHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding DashVectorHook");
                _DashVectorHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing DashVectorHook");
                _DashVectorHook -= value;
            }
        }

        private event DashVelocityHandler _DashVectorHook;

        /// <summary>
        /// Called during dash function to change velocity
        /// </summary>
        /// <remarks>HeroController.Dash</remarks>
        internal Vector2? DashVelocityChange()
        {
            Logger.LogFine("[API] - DashVelocityChange Invoked");

            return _DashVectorHook?.Invoke();
        }

        /// <summary>
        /// Called whenever the dash key is pressed. Overrides normal dash functionality
        /// </summary>
        /// <remarks>HeroController.LookForQueueInput</remarks>
        [HookInfo("Called whenever the dash key is pressed. Overrides normal dash functionality", "HeroController.LookForQueueInput")]
        public event DashPressedHandler DashPressedHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding DashPressedHook");
                _DashPressedHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing DashPressedHook");
                _DashPressedHook -= value;
            }
        }

        private event DashPressedHandler _DashPressedHook;

        /// <summary>
        /// Called whenever the dash key is pressed. Overrides normal dash functionality
        /// </summary>
        /// <remarks>HeroController.LookForQueueInput</remarks>
        internal bool OnDashPressed()
        {
            Logger.LogFine("[API] - OnDashPressed Invoked");

            if (_DashPressedHook == null) return false;

            _DashPressedHook();
            return true;
        }

        #endregion


        #region SaveHandling


        /// <summary>
        /// Called directly after a save has been loaded
        /// </summary>
        /// <remarks>GameManager.LoadGame</remarks>
        [HookInfo("Called directly after a save has been loaded", "GameManager.LoadGame")]
        public event SavegameLoadHandler SavegameLoadHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding SavegameLoadHook");
                _SavegameLoadHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing SavegameLoadHook");
                _SavegameLoadHook -= value;
            }
        }

        private event SavegameLoadHandler _SavegameLoadHook;


        /// <summary>
        /// Called directly after a save has been loaded
        /// </summary>
        /// <remarks>GameManager.LoadGame</remarks>
        internal void OnSavegameLoad(int id)
        {
            Logger.LogFine("[API] - OnSavegameLoad Invoked");

            _SavegameLoadHook?.Invoke(id);
        }

        /// <summary>
        /// Called directly after a save has been saved
        /// </summary>
        /// <remarks>GameManager.SaveGame</remarks>
        [HookInfo("Called directly after a save has been saved", "GameManager.SaveGame")]
        public event SavegameSaveHandler SavegameSaveHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding SavegameSaveHook");
                _SavegameSaveHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing SavegameSaveHook");
                _SavegameSaveHook -= value;
            }
        }

        private event SavegameSaveHandler _SavegameSaveHook;

        /// <summary>
        /// Called directly after a save has been saved
        /// </summary>
        /// <remarks>GameManager.SaveGame</remarks>
        internal void OnSavegameSave(int id)
        {
            Logger.LogFine("[API] - OnSavegameSave Invoked");

            _SavegameSaveHook?.Invoke(id);
        }

        /// <summary>
        /// Called whenever a new game is started
        /// </summary>
        /// <remarks>GameManager.LoadFirstScene</remarks>
        [HookInfo("Called whenever a new game is started", "GameManager.LoadFirstScene")]
        public event NewGameHandler NewGameHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding NewGameHook");
                _NewGameHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing NewGameHook");
                _NewGameHook -= value;
            }
        }

        private event NewGameHandler _NewGameHook;

        /// <summary>
        /// Called whenever a new game is started
        /// </summary>
        /// <remarks>GameManager.LoadFirstScene</remarks>
        internal void OnNewGame()
        {
            Logger.LogFine("[API] - OnNewGame Invoked");

            _NewGameHook?.Invoke();
        }

        /// <summary>
        /// Called before a save file is deleted
        /// </summary>
        /// <remarks>GameManager.ClearSaveFile</remarks>
        [HookInfo("Called whenever a save file is deleted", "GameManager.ClearSaveFile")]
        public event ClearSaveGameHandler SavegameClearHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding SavegameClearHook");
                _SavegameClearHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing SavegameClearHook");
                _SavegameClearHook -= value;
            }
        }

        private event ClearSaveGameHandler _SavegameClearHook;

        /// <summary>
        /// Called before a save file is deleted
        /// </summary>
        /// <remarks>GameManager.ClearSaveFile</remarks>
        internal void OnSavegameClear(int id)
        {
            Logger.LogFine("[API] - OnSavegameClear Invoked");

            _SavegameClearHook?.Invoke(id);
        }

        /// <summary>
        /// Called directly after a save has been loaded.  Allows for accessing SaveGame instance.
        /// </summary>
        /// <remarks>GameManager.LoadGame</remarks>
        [HookInfo("Called directly after a save has been loaded.  Allows for accessing SaveGame instance.", "GameManager.LoadGame")]
        public event AfterSavegameLoadHandler AfterSavegameLoadHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding AfterSavegameLoadHook");
                _AfterSavegameLoadHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing AfterSavegameLoadHook");
                _AfterSavegameLoadHook -= value;
            }
        }

        private event AfterSavegameLoadHandler _AfterSavegameLoadHook;

        /// <summary>
        /// Called directly after a save has been loaded.  Allows for accessing SaveGame instance.
        /// </summary>
        /// <remarks>GameManager.LoadGame</remarks>
        internal void OnAfterSaveGameLoad(Patches.SaveGameData data)
        {
            Logger.LogFine("[API] - OnAfterSaveGameLoad Invoked");

            _AfterSavegameLoadHook?.Invoke(data);
        }

        /// <summary>
        /// Called directly before save has been saved to allow for changes to the data before persisted.
        /// </summary>
        /// <remarks>GameManager.SaveGame</remarks>
        [HookInfo("Called directly before save has been saved to allow for changes to the data before persisted.", "GameManager.SaveGame")]
        public event BeforeSavegameSaveHandler BeforeSavegameSaveHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding BeforeSavegameSaveHook");
                _BeforeSavegameSaveHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing BeforeSavegameSaveHook");
                _BeforeSavegameSaveHook -= value;
            }
        }

        private event BeforeSavegameSaveHandler _BeforeSavegameSaveHook;

        /// <summary>
        /// Called directly before save has been saved to allow for changes to the data before persisted.
        /// </summary>
        /// <remarks>GameManager.SaveGame</remarks>
        internal void OnBeforeSaveGameSave(Patches.SaveGameData data)
        {
            Logger.LogFine("[API] - OnBeforeSaveGameSave Invoked");
            data.LoadedMods = LoadedModsWithVersions;
            _BeforeSavegameSaveHook?.Invoke(data);
        }

        /// <summary>
        /// Overrides the filename to load for a given slot.  Return null to use vanilla names.
        /// </summary>
        [HookInfo("Overrides the filename for a slot.", "GameManager.SaveGameClear")]
        public event GetSaveFileNameHandler GetSaveFileNameHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding GetSaveFileNameHook");
                _GetSaveFileNameHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing GetSaveFileNameHook");
                _GetSaveFileNameHook -= value;
            }
        }

        private event GetSaveFileNameHandler _GetSaveFileNameHook;

        /// <summary>
        /// Overrides the filename to load for a given slot.  Return null to use vanilla names.
        /// </summary>
        internal string GetSaveFileName(int saveSlot)
        {
            Logger.LogFine("[API] - GetSaveFileName Invoked");

            return _GetSaveFileNameHook?.Invoke(saveSlot);
        }

        /// <summary>
        /// Called after a game has been cleared from a slot.
        /// </summary>
        [HookInfo("Called after a savegame has been cleared.", "GameManager.GetSaveFilename")]
        public event AfterClearSaveGameHandler AfterSaveGameClearHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding AfterSaveGameClearHook");
                _AfterSaveGameClearHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing AfterSaveGameClearHook");
                _AfterSaveGameClearHook -= value;
            }
        }

        private event AfterClearSaveGameHandler _AfterSaveGameClearHook;

        /// <summary>
        /// Called after a game has been cleared from a slot.
        /// </summary>
        internal void OnAfterSaveGameClear(int saveSlot)
        {
            Logger.LogFine("[API] - OnAfterSaveGameClear Invoked");

            _AfterSaveGameClearHook?.Invoke(saveSlot);
        }

        #endregion

        /// <summary>
        /// Called whenever localization specific strings are requested
        /// </summary>
        /// <remarks>N/A</remarks>
        [HookInfo("Called whenever localization specific strings are requested", "N/A")]
        public event LanguageGetHandler LanguageGetHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding LanguageGetHook");
                _LanguageGetHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing LanguageGetHook");
                _LanguageGetHook -= value;
            }
        }

        private event LanguageGetHandler _LanguageGetHook;

        /// <summary>
        /// Called whenever localization specific strings are requested
        /// </summary>
        /// <remarks>N/A</remarks>
        internal string LanguageGet(string key, string sheet)
        {
            string @internal = Patches.Language.GetInternal(key, sheet);
            string result = @internal;
            bool flag = false;
            if (_LanguageGetHook == null) return result;

            Delegate[] invocationList = _LanguageGetHook.GetInvocationList();
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
        public event SceneChangedHandler SceneChanged
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding SceneChanged");
                _SceneChanged += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing SceneChanged");
                _SceneChanged -= value;
            }
        }

        private event SceneChangedHandler _SceneChanged;

        /// <summary>
        /// Called after a new Scene has been loaded
        /// </summary>
        /// <remarks>N/A</remarks>
        internal void OnSceneChanged(string targetScene)
        {
            Logger.LogFine("[API] - OnSceneChanged Invoked");

            _SceneChanged?.Invoke(targetScene);
        }

        /// <summary>
        /// Called right before a scene gets loaded, can change which scene gets loaded
        /// </summary>
        /// <remarks>N/A</remarks>
        [HookInfo("Called right before a scene gets loaded, can change which scene gets loaded", "GameManager.LoadScene")]
        public event BeforeSceneLoadHandler BeforeSceneLoadHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding BeforeSceneLoadHook");
                _BeforeSceneLoadHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing BeforeSceneLoadHook");
                _BeforeSceneLoadHook -= value;
            }
        }

        private event BeforeSceneLoadHandler _BeforeSceneLoadHook;

        /// <summary>
        /// Called right before a scene gets loaded, can change which scene gets loaded
        /// </summary>
        /// <remarks>N/A</remarks>
        internal string BeforeSceneLoad(string sceneName)
        {
            Logger.LogFine("[API] - BeforeSceneLoad Invoked");

            if (_BeforeSceneLoadHook == null) return sceneName;

            Delegate[] invocationList = _BeforeSceneLoadHook.GetInvocationList();
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
        public event CursorHandler CursorHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding CursorHook");
                _CursorHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing CursorHook");
                _CursorHook -= value;
            }
        }

        private event CursorHandler _CursorHook;

        /// <summary>
        /// Called whenever game tries to show cursor
        /// </summary>
        internal void OnCursor()
        {
            //Logger.LogFine("[API] - OnCursor Invoked"); // Too Spammy

            Cursor.lockState = CursorLockMode.None;
            if (_CursorHook != null)
            {
                _CursorHook();
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
        public event ColliderCreateHandler ColliderCreateHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding ColliderCreateHook");
                _ColliderCreateHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing ColliderCreateHook");
                _ColliderCreateHook -= value;
            }
        }

        private event ColliderCreateHandler _ColliderCreateHook;

        /// <summary>
        /// Called whenever a new gameobject is created with a collider and playmaker2d
        /// </summary>
        /// <remarks>PlayMakerUnity2DProxy.Start</remarks>
        internal void OnColliderCreate(GameObject go)
        {
            Logger.LogFine("[API] - OnColliderCreate Invoked");

            _ColliderCreateHook?.Invoke(go);
        }


        /// <summary>
        /// Called whenever game tries to create a new gameobject.  This happens often, care should be taken.
        /// </summary>
        [HookInfo("Called whenever game tries to create a new gameobject.  This happens often, care should be taken.", "ObjectPool.Spawn")]
        public event GameObjectHandler ObjectPoolSpawnHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding ObjectPoolSpawnHook");
                _ObjectPoolSpawnHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing ObjectPoolSpawnHook");
                _ObjectPoolSpawnHook -= value;
            }
        }

        private event GameObjectHandler _ObjectPoolSpawnHook;

        /// <summary>
        /// Called whenever game tries to show cursor
        /// </summary>
        internal GameObject OnObjectPoolSpawn(GameObject go)
        {
            //Logger.LogFine("[API] - OnObjectPool Invoked"); // Too Spammy
            if (_ObjectPoolSpawnHook == null) return go;

            Delegate[] invocationList = _ObjectPoolSpawnHook.GetInvocationList();
            foreach (Delegate toInvoke in invocationList)
            {
                go = (GameObject)toInvoke.DynamicInvoke(go);
            }
            return go;
        }


        /// <summary>
        /// Called whenever game sends GetEventSender. 
        /// </summary>
        /// <remarks>HutongGames.PlayMaker.Actions.GetEventSender</remarks>
        [HookInfo("Called whenever game sends GetEventSender. ", "HutongGames.PlayMaker.Actions.GetEventSender")]
        public event GameObjectHandler OnGetEventSenderHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding OnGetEventSenderHook");
                _OnGetEventSenderHook += value;
            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing OnGetEventSenderHook");
                _OnGetEventSenderHook -= value;
            }
        }

        private event GameObjectHandler _OnGetEventSenderHook;

        /// <summary>
        /// Called whenever the FSM OnGetEvent is ran (only done during attacks/spells right now).  
        /// </summary>
        internal GameObject OnGetEventSender(GameObject go)
        {
            Logger.LogFine("[API] - OnGetEventSendr Invoked"); 
            if (_OnGetEventSenderHook == null) return go;

            Delegate[] invocationList = _OnGetEventSenderHook.GetInvocationList();
            foreach (Delegate toInvoke in invocationList)
            {
                go = (GameObject)toInvoke.DynamicInvoke(go);
            }
            return go;
        }

        /// <summary>
        /// Called when the game is fully closed
        /// </summary>
        /// <remarks>GameManager.OnApplicationQuit</remarks>
        [HookInfo("Called when the game is fully closed", "GameManager.OnApplicationQuit")]
        public event ApplicationQuitHandler ApplicationQuitHook
        {
            add
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding ApplicationQuitHook");
                _ApplicationQuitHook += value;

            }
            remove
            {
                Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing ApplicationQuitHook");
                _ApplicationQuitHook -= value;
            }
        }

        private event ApplicationQuitHandler _ApplicationQuitHook;

        /// <summary>
        /// Called when the game is fully closed
        /// </summary>
        /// <remarks>GameManager.OnApplicationQuit</remarks>
        internal void OnApplicationQuit()
        {
            Logger.LogFine("[API] - OnApplicationQuit Invoked");

            _ApplicationQuitHook?.Invoke();
        }

        /// <summary>
        /// Save GlobalSettings to disk. (backs up the current global settings if it exists)
        /// </summary>
        internal static void SaveGlobalSettings()
        {
            Logger.Log("Saving Global Settings");
            if (File.Exists(SettingsPath + ".bak"))
                File.Delete(SettingsPath + ".bak");

            if (File.Exists(SettingsPath))
                File.Move(SettingsPath, SettingsPath + ".bak");

            using (FileStream fileStream = File.Create(SettingsPath))
            {
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    string text4 = JsonUtility.ToJson(GlobalSettings, true);
                    writer.Write(text4);
                }
            }
        }

        /// <summary>
        /// Loads global settings from disk (if they exist)
        /// </summary>
        internal static void LoadGlobalSettings()
        {
            if (!File.Exists(SettingsPath))
            {
                _globalSettings = new ModHooksGlobalSettings { LoggingLevel = LogLevel.Info };
                return;
            }

            //Logger.Log("[API] - Loading Global Settings");
            using (FileStream fileStream = File.OpenRead(SettingsPath))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    string json = reader.ReadToEnd();
                    _globalSettings = JsonUtility.FromJson<ModHooksGlobalSettings>(json);
                }
            }
        }

    }
}
