using System;
using UnityEngine;

namespace Modding
{
    /// <summary>
    ///     Called whenever localization specific strings are requested
    /// </summary>
    /// <param name="key">The key within the sheet</param>
    /// <param name="sheetTitle">The title of the sheet</param>
    /// <param name="orig">The original localized value</param>
    /// <param name="current">The current value, including overrides from other mods.</param>
    /// <returns>Whether or not to use the overriden out param.</returns>
    public delegate bool LanguageGetProxy(string key, string sheetTitle, string orig, string current, out string res);

    /// <summary>
    ///     Called when anything in the game tries to get a bool
    /// </summary>
    /// <example>
    /// <code>
    /// ModHooks.GetPlayerBoolHook += GetBool;
    ///
    /// // In this example, we always give the player dash, and
    /// // leave other bools as-is.
    /// bool? GetBool(string name, bool orig) {
    ///     return name == "canDash" ? true : null;
    /// }
    /// </code>
    /// </example>
    /// <param name="name">The field being gotten</param>
    /// <param name="orig">The original value of the bool</param>
    /// <returns>The bool, if you are overriding it, otherwise null.</returns>
    public delegate bool? GetBoolProxy(string name, bool orig);

    /// <summary>
    ///     Called when anything in the game tries to set a bool
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
    /// bool? SetBool(string name, bool orig) {
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
    ///             return null;
    ///         default:
    ///             return null;
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <param name="name">The field being set</param>
    /// <param name="orig">The original value the bool was being set to</param>
    /// <returns>The bool, if overriden, else null.</returns>
    public delegate bool? SetBoolProxy(string name, bool orig);

    /// <summary>
    ///     Called when anything in the game tries to get an int
    /// </summary>
    /// <example>
    /// <code>
    /// ModHooks.GetPlayerIntHook += GetInt;
    ///
    /// // This overrides the number of charm slots we have to 999,
    /// // effectively giving us infinite charm notches.
    /// // We ignore any other GetInt calls.
    /// int? GetInt(string name, int orig) {
    ///     return name == "charmSlots" ? 999 : nulll;
    /// }
    /// </code>
    /// </example>
    /// <param name="name">The field being gotten</param>
    /// <param name="orig">The original value of the field</param>
    /// <returns>The int if overrode, else null.</returns>
    public delegate int? GetIntProxy(string name, int orig);

    /// <summary>
    ///     Called when anything in the game tries to set an int
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
    ///     // at all, so we just leave the value as null for everything.
    ///     return null;
    /// }
    /// </code>
    /// </example>
    /// <param name="name">The field which is being set</param>
    /// <param name="orig">The original value</param>
    /// <returns>The int if overrode, else null</returns>
    public delegate int? SetIntProxy(string name, int orig);

    /// <summary>
    ///     Called when anything in the game tries to get a float
    /// </summary>
    /// <param name="name">The field being set</param>
    /// <param name="orig">The original value</param>
    /// <returns>The value, if overrode, else null.</returns>
    public delegate float? GetFloatProxy(string name, float orig);

    /// <summary>
    ///     Called when anything in the game tries to set a float
    /// </summary>
    /// <param name="name">The field being set</param>
    /// <param name="orig">The original value the float was being set to</param>
    public delegate float? SetFloatProxy(string name, float orig);

    /// <summary>
    ///     Called when anything in the game tries to get a string
    /// </summary>
    /// <param name="name">The name of the field</param>
    /// <param name="orig">The original value of the string</param>
    /// <param name="res">The value the string will be gotten as if overrode, otherwise ignored, typically null or orig.</param>
    /// <returns>Whether or not the callee is overriding the get</returns>
    public delegate bool GetStringProxy(string name, string orig, out string res);

    /// <summary>
    ///     Called when anything in the game tries to set a string
    /// </summary>
    /// <param name="name">The name of the field</param>
    /// <param name="orig">The original value the string was being set to</param>
    /// <param name="res">The value the string will be set to if overrode, otherwise ignored, typically null or orig.</param>
    /// <returns>Whether or not to override the string set.</returns>
    public delegate bool SetStringProxy(string name, string orig, out string res);

    /// <summary>
    ///     Called when anything in the game tries to get a Vector3
    /// </summary>
    /// <param name="name">The name of the Vector3 field</param>
    /// <param name="orig">The original value of the field</param>
    /// <returns>The value to override the vector to, otherwise null</returns>
    public delegate Vector3? GetVector3Proxy(string name, Vector3 orig);

    /// <summary>
    ///     Called when anything in the game tries to set a Vector3
    /// </summary>
    /// <param name="name">The name of the field</param>
    /// <param name="orig">The original value the field was being set to</param>
    /// <returns>The value to override the set to, otherwise null.</returns>
    public delegate Vector3? SetVector3Proxy(string name, Vector3 orig);

    /// <summary>
    ///     Called when anything in the game tries to get a generic variable
    /// </summary>
    /// <param name="type">The type of the variable</param>
    /// <param name="name">The field being gotten</param>
    /// <param name="orig">The original value of the field</param>
    /// <param name="res">The value to override the get to, typically null or orig when unused.</param>
    /// <returns>Whether or not to override the variable get.</returns>
    public delegate bool GetVariableProxy(Type type, string name, object orig, out object res);

    /// <summary>
    ///     Called when anything in the game tries to set a generic variable
    /// </summary>
    /// <param name="type">The type of the variable</param>
    /// <param name="name">The name of the field being set</param>
    /// <param name="orig">The original value the field was being set to</param>
    /// <param name="res">The value to override the set to, typically null or orig when unused.</param>
    /// <returns>Whether or not to override the set with the out parameter.</returns>
    public delegate bool SetVariableProxy(Type type, string name, object orig, out object res);

    /// <summary>
    ///     Called when damage is dealt to the player
    /// </summary>
    /// <param name="hazardType">The type of hazard that caused the damage.</param>
    /// <param name="damage">Amount of Damage</param>
    /// <returns>Modified Damage</returns>
    public delegate int TakeDamageProxy(ref int hazardType, int damage);

    /// <summary>
    ///     Called when health is taken from the player
    /// </summary>
    /// <param name="damage">Amount of Damage</param>
    /// <returns>Modified Damaged</returns>
    public delegate int TakeHealthProxy(int damage);
}