using System;
using UnityEngine;

namespace Modding.Delegates
{
    /// <summary>
    ///     Called whenever localization specific strings are requested
    /// </summary>
    /// <param name="key">The key within the sheet</param>
    /// <param name="sheetTitle">The title of the sheet</param>
    /// <param name="orig">The original localized value</param>
    /// <returns>The modified localization, return *current* to keep as-is.</returns>
    public delegate string LanguageGetProxy(string key, string sheetTitle, string orig);

    /// <summary>
    ///     Called when anything in the game tries to get a bool
    /// </summary>
    /// <param name="name">The field being gotten</param>
    /// <param name="orig">The original value of the bool</param>
    /// <returns>The bool, if you are overriding it, otherwise orig.</returns>
    public delegate bool GetBoolProxy(string name, bool orig);

    /// <summary>
    ///     Called when anything in the game tries to set a bool
    /// </summary>
    /// <param name="name">The field being set</param>
    /// <param name="orig">The original value the bool was being set to</param>
    /// <returns>The bool, if overridden, else orig.</returns>
    public delegate bool SetBoolProxy(string name, bool orig);

    /// <summary>
    ///     Called when anything in the game tries to get an int
    /// </summary>
    /// <param name="name">The field being gotten</param>
    /// <param name="orig">The original value of the field</param>
    /// <returns>The int, if overridden, else orig.</returns>
    public delegate int GetIntProxy(string name, int orig);

    /// <summary>
    ///     Called when anything in the game tries to set an int
    /// </summary>
    /// <param name="name">The field which is being set</param>
    /// <param name="orig">The original value</param>
    /// <returns>The int if overrode, else null</returns>
    public delegate int SetIntProxy(string name, int orig);

    /// <summary>
    ///     Called when anything in the game tries to get a float
    /// </summary>
    /// <param name="name">The field being set</param>
    /// <param name="orig">The original value</param>
    /// <returns>The value, if overrode, else null.</returns>
    public delegate float GetFloatProxy(string name, float orig);

    /// <summary>
    ///     Called when anything in the game tries to set a float
    /// </summary>
    /// <param name="name">The field being set</param>
    /// <param name="orig">The original value the float was being set to</param>
    /// <returns>The modified value of the set</returns>
    public delegate float SetFloatProxy(string name, float orig);

    /// <summary>
    ///     Called when anything in the game tries to get a string
    /// </summary>
    /// <param name="name">The name of the field</param>
    /// <param name="res">The original value of the field</param>
    /// <returns>The modified value of the get</returns>
    public delegate string GetStringProxy(string name, string res);

    /// <summary>
    ///     Called when anything in the game tries to set a string
    /// </summary>
    /// <param name="name">The name of the field</param>
    /// <param name="res">The original set value of the string</param>
    /// <returns>The modified value of the set</returns>
    public delegate string SetStringProxy(string name, string res);

    /// <summary>
    ///     Called when anything in the game tries to get a Vector3
    /// </summary>
    /// <param name="name">The name of the Vector3 field</param>
    /// <param name="orig">The original value of the field</param>
    /// <returns>The value to override the vector to</returns>
    public delegate Vector3 GetVector3Proxy(string name, Vector3 orig);

    /// <summary>
    ///     Called when anything in the game tries to set a Vector3
    /// </summary>
    /// <param name="name">The name of the field</param>
    /// <param name="orig">The original value the field was being set to</param>
    /// <returns>The value to override the set to</returns>
    public delegate Vector3 SetVector3Proxy(string name, Vector3 orig);

    /// <summary>
    ///     Called when anything in the game tries to get a generic variable
    /// </summary>
    /// <param name="type">The type of the variable</param>
    /// <param name="name">The field being gotten</param>
    /// <param name="value">The original value of the field</param>
    /// <returns>The modified value</returns>
    public delegate object GetVariableProxy(Type type, string name, object value);

    /// <summary>
    ///     Called when anything in the game tries to set a generic variable
    /// </summary>
    /// <param name="type">The type of the variable</param>
    /// <param name="name">The name of the field being set</param>
    /// <param name="value">The original value the field was being set to</param>
    /// <returns>The new value of the field</returns>
    public delegate object SetVariableProxy(Type type, string name, object value);

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