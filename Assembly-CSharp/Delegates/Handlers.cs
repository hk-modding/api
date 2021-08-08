using HutongGames.PlayMaker;
using UnityEngine;

namespace Modding.Delegates
{
    /// <summary>
    ///     Called after player values for charms have been set
    /// </summary>
    /// <param name="data">Current PlayerData</param>
    /// <param name="controller">Current HeroController</param>
    public delegate void CharmUpdateHandler(PlayerData data, HeroController controller);

    /// <summary>
    ///     Called at the end of the take damage function
    /// </summary>
    /// <param name="hazardType"></param>
    /// <param name="damageAmount"></param>
    public delegate int AfterTakeDamageHandler(int hazardType, int damageAmount);

    /// <summary>
    ///     Called whenever nail strikes something
    /// </summary>
    /// <param name="otherCollider">What the nail is colliding with</param>
    /// <param name="slash">The NailSlash gameObject</param>
    public delegate void SlashHitHandler(Collider2D otherCollider, GameObject slash);

    /// <summary>
    ///     Called when a HitInstance is created in TakeDamage. The hit instance returned defines the hit behavior that will
    ///     happen. Overrides default behavior
    /// </summary>
    public delegate HitInstance HitInstanceHandler(Fsm owner, HitInstance hit);

    /// <summary>
    ///     Called when an enemy is enabled. Check this isDead flag to see if they're already dead. If you return true, this
    ///     will mark the enemy as already dead on load. Default behavior is to return the value inside "isAlreadyDead".
    /// </summary>
    public delegate bool OnEnableEnemyHandler(GameObject enemy, bool isAlreadyDead);

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
    public delegate void RecordKillForJournalHandler
    (
        EnemyDeathEffects enemyDeathEffects,
        string playerDataName,
        string killedBoolPlayerDataLookupKey,
        string killCountIntPlayerDataLookupKey,
        string newDataBoolPlayerDataLookupKey
    );
}