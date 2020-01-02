using System;
using System.Collections.Generic;
using GlobalEnums;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Modding
{
    /// <summary>
    ///     Called just before Application Quits.
    /// </summary>
    public delegate void ApplicationQuitHandler();

    /// <summary>
    ///     Called After SaveGameData is created, but before it is serialized and saved to disk.  Usually not called directly
    ///     in Mods.
    /// </summary>
    /// <param name="data">SaveGameData before serialization</param>
    public delegate void BeforeSavegameSaveHandler(Patches.SaveGameData data);

    /// <summary>
    ///     Called just after the game has been saved to disk
    /// </summary>
    /// <param name="id">Save slot location</param>
    public delegate void SavegameSaveHandler(int id);

    /// <summary>
    ///     Called after the game was loaded from Disk
    /// </summary>
    /// <param name="id">Save slot location</param>
    public delegate void SavegameLoadHandler(int id);

    /// <summary>
    ///     Called after the game was loaded from disk, but before the data is loaded into PlayerData and SceneData.  Populates
    ///     Mod's Settings
    /// </summary>
    /// <param name="data">SaveGameData right after deserialization</param>
    public delegate void AfterSavegameLoadHandler(Patches.SaveGameData data);

    /// <summary>
    ///     Called before game save is cleared.
    /// </summary>
    /// <param name="save">Save slot location</param>
    public delegate void ClearSaveGameHandler(int save);

    /// <summary>
    ///     Called after the game save is cleared.
    /// </summary>
    /// <param name="slot">Save slot Location</param>
    public delegate void AfterClearSaveGameHandler(int slot);

    /// <summary>
    ///     Overrides the save file name.
    /// </summary>
    /// <param name="slot">Save File Slot Id</param>
    /// <returns>Filename to use or null to use vanilla</returns>
    public delegate string GetSaveFileNameHandler(int slot);

    /// <summary>
    ///     Called after a new game save is started.
    /// </summary>
    public delegate void NewGameHandler();

    /// <summary>
    ///     Called after setting up a new PlayerData
    /// </summary>
    /// <param name="data">New PlayerData</param>
    public delegate void NewPlayerDataHandler(PlayerData data);


    /// <summary>
    ///     Called after scene has been loaded.
    /// </summary>
    /// <param name="targetScene">Scene name that was loaded.</param>
    public delegate void SceneChangedHandler(string targetScene);

    //TODO: This says it returns a string, but the event in Modhooks returns void?
    /// <summary>
    ///     Called right before a scene gets loaded, can change which scene gets loaded
    /// </summary>
    /// <param name="sceneName">Scene name to load</param>
    /// <returns>Unknown</returns>
    public delegate string BeforeSceneLoadHandler(string sceneName);

    /// <summary>
    ///     Called whenever localization specific strings are requested
    /// </summary>
    /// <param name="key"></param>
    /// <param name="sheetTitle"></param>
    /// <returns>Localized Value</returns>
    public delegate string LanguageGetHandler(string key, string sheetTitle);

    /// <summary>
    ///     Called whenever the hero updates
    /// </summary>
    public delegate void HeroUpdateHandler();

    /// <summary>
    ///     Called after player values for charms have been set
    /// </summary>
    /// <param name="data">Current PlayerData</param>
    /// <param name="controller">Current HeroController</param>
    public delegate void CharmUpdateHandler(PlayerData data, HeroController controller);

    /// <summary>
    ///     Called when the player attacks
    /// </summary>
    /// <param name="dir">Direction of the attack.</param>
    public delegate void AttackHandler(AttackDirection dir);

    /// <summary>
    ///     Called at the end of the attack function
    /// </summary>
    /// <param name="dir">Direction of the attack.</param>
    public delegate void AfterAttackHandler(AttackDirection dir);

    /// <summary>
    ///     Called at the end of the take damage function
    /// </summary>
    /// <param name="hazardType"></param>
    /// <param name="damageAmount"></param>
    public delegate int AfterTakeDamageHandler(int hazardType, int damageAmount);

    /// <summary>
    ///     Called whenever
    ///     health is updated
    /// </summary>
    /// <returns></returns>
    public delegate int BlueHealthHandler();

    /// <summary>
    ///     Called whenever game tries to show cursor
    /// </summary>
    public delegate void CursorHandler();

    /// <summary>
    ///     Called at the start of the DoAttack function
    /// </summary>
    public delegate void DoAttackHandler();

    /// <summary>
    ///     Called whenever focus cost is calculated
    /// </summary>
    /// <returns></returns>
    public delegate float FocusCostHandler();

    /// <summary>
    ///     Called whenever nail strikes something
    /// </summary>
    /// <param name="otherCollider"></param>
    /// <param name="gameObject"></param>
    public delegate void SlashHitHandler(Collider2D otherCollider, GameObject gameObject);

    /// <summary>
    ///     Called when Hero recovers Soul from hitting enemies
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public delegate int SoulGainHandler(int num);

    /// <summary>
    ///     Called during dash function to change velocity
    /// </summary>
    /// <returns>New vector for velocity</returns>
    public delegate Vector2 DashVelocityHandler(Vector2 change);

    /// <summary>
    ///     Called whenever the dash key is pressed. Overrides normal dash functionalit
    /// </summary>
    public delegate bool DashPressedHandler();

    /// <summary>
    ///     Called whenever a new gameobject is created with a collider and playmaker2d
    /// </summary>
    /// <param name="go"></param>
    public delegate void ColliderCreateHandler(GameObject go);

    /// <summary>
    ///     Handle for events that accept a GameObject and return a GameObject
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public delegate GameObject GameObjectHandler(GameObject go);

    /// <summary>
    ///     Handle for events that accept a GameObject and FSM and return a GameObject
    /// </summary>
    /// <param name="go"></param>
    /// <param name="fsm"></param>
    /// <returns></returns>
    public delegate GameObject GameObjectFsmHandler(GameObject go, Fsm fsm);

    /// <summary>
    ///     Called when the game changes to a new regional font
    /// </summary>
    /// <returns></returns>
    public delegate void SetFontHandler();

    /// <summary>
    ///     Generic Handler with a Void Return
    /// </summary>
    public delegate void VoidHandler();

    /// <summary>
    ///     Called when the player heals
    /// </summary>
    public delegate int BeforeAddHealthHandler(int amount);

    /// <summary>
    ///     Called when a HitInstance is created in TakeDamage. The hit instance returned defines the hit behavior that will
    ///     happen. Overrides default behavior
    /// </summary>
    public delegate HitInstance HitInstanceHandler(Fsm owner, HitInstance hit);

    /// <summary>
    ///     Called when a SceneManager calls DrawBlackBorders and creates boarders for a scene. You may use or modify the
    ///     bounds of an area of the scene with these.
    /// </summary>
    public delegate void DrawBlackBordersHandler(List<GameObject> borders);

    /// <summary>
    ///     Called when an enemy is enabled. Check this isDead flag to see if they're already dead. If you return true, this
    ///     will mark the enemy as already dead on load. Default behavior is to return the value inside "isAlreadyDead".
    /// </summary>
    public delegate bool OnEnableEnemyHandler(GameObject enemy, bool isAlreadyDead);

    /// <summary>
    ///     Called when an enemy recieves a death event. It looks like this event may be called multiple times on an enemy, so
    ///     check "eventAlreadyRecieved" to see if the event has been fired more than once.
    /// </summary>
    [Obsolete("Use " + nameof(OnReceiveDeathEventHandler))]
    public delegate bool OnRecieveDeathEventHandler
    (
        EnemyDeathEffects enemyDeathEffects,
        bool eventAlreadyRecieved,
        ref float? attackDirection,
        ref bool resetDeathEvent,
        ref bool spellBurn,
        ref bool isWatery
    );
    
    /// <summary>
    ///     Called when an enemy recieves a death event. It looks like this event may be called multiple times on an enemy, so
    ///     check "eventAlreadyReceived" to see if the event has been fired more than once.
    /// </summary>
    public delegate void OnReceiveDeathEventHandler
    (
        EnemyDeathEffects enemyDeathEffects,
        bool eventAlreadyReceived,
        ref float? attackDirection,
        ref bool resetDeathEvent,
        ref bool spellBurn,
        ref bool isWatery
    );

    /// <summary>
    ///     Called when an enemy dies and a journal kill is recorded. You may use the "playerDataName" string or one of the
    ///     additional pre-formatted player data strings to look up values in playerData.
    /// </summary>
    [Obsolete("Use " + nameof(RecordKillForJournalHandler))]
    public delegate bool OnRecordKillForJournalHandler
    (
        EnemyDeathEffects enemyDeathEffects,
        string playerDataName,
        string killedBoolPlayerDataLookupKey,
        string killCountIntPlayerDataLookupKey,
        string newDataBoolPlayerDataLookupKey
    );
    
    /// <summary>
    ///     Called when an enemy dies and a journal kill is recorded. You may use the "playerDataName" string or one of the
    ///     additional pre-formatted player data strings to look up values in playerData.
    /// </summary>
    public delegate void RecordKillForJournalHandler
    (
        EnemyDeathEffects enemyDeathEffects,
        string playerDataName,
        string killedBoolPlayerDataLookupKey,
        string killCountIntPlayerDataLookupKey,
        string newDataBoolPlayerDataLookupKey
    );
}