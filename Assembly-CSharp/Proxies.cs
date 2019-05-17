using System;
using UnityEngine;

namespace Modding
{
    /// <summary>
    ///     Called when anything in the game tries to get a bool
    /// </summary>
    /// <param name="originalSet">Value's Name</param>
    /// <returns>The bool value</returns>
    public delegate bool GetBoolProxy(string originalSet);

    /// <summary>
    ///     Called when anything in the game tries to set a bool
    /// </summary>
    /// <param name="originalSet">Name of the Bool</param>
    /// <param name="value">Value to be used</param>
    public delegate void SetBoolProxy(string originalSet, bool value);

    /// <summary>
    ///     Called when anything in the game tries to get an int
    /// </summary>
    /// <param name="intName">Value's Name</param>
    /// <returns>The bool value</returns>
    public delegate int GetIntProxy(string intName);

    /// <summary>
    ///     Called when anything in the game tries to set an int
    /// </summary>
    /// <param name="intName">Name of the Int</param>
    /// <param name="value">Value to be used</param>
    public delegate void SetIntProxy(string intName, int value);

    /// <summary>
    ///     Called when anything in the game tries to get a float
    /// </summary>
    /// <param name="floatName">Name of the float</param>
    /// <returns>The float value</returns>
    public delegate float GetFloatProxy(string floatName);

    /// <summary>
    ///     Called when anything in the game tries to set a float
    /// </summary>
    /// <param name="floatName">Name of the float</param>
    /// <param name="value">Value to be used</param>
    public delegate void SetFloatProxy(string floatName, float value);

    /// <summary>
    ///     Called when anything in the game tries to get a string
    /// </summary>
    /// <param name="stringName">Name of the string</param>
    /// <returns>The string value</returns>
    public delegate string GetStringProxy(string stringName);

    /// <summary>
    ///     Called when anything in the game tries to set a string
    /// </summary>
    /// <param name="stringName">Name of the string</param>
    /// <param name="value">Value to be used</param>
    public delegate void SetStringProxy(string stringName, string value);

    /// <summary>
    ///     Called when anything in the game tries to get a Vector3
    /// </summary>
    /// <param name="vector3Name">Name of the Vector3</param>
    /// <returns>The Vector3 value</returns>
    public delegate Vector3 GetVector3Proxy(string vector3Name);

    /// <summary>
    ///     Called when anything in the game tries to set a Vector3
    /// </summary>
    /// <param name="vector3Name">Name of the Vector3</param>
    /// <param name="value">Value to be used</param>
    public delegate void SetVector3Proxy(string vector3Name, Vector3 value);

    /// <summary>
    ///     Called when anything in the game tries to get a generic variable
    /// </summary>
    /// <param name="type">The type of the variable</param>
    /// <param name="varName">Name of the variable</param>
    /// <param name="orig">Original value</param>
    /// <returns>The variable value</returns>
    public delegate object GetVariableProxy(Type type, string varName, object orig);

    /// <summary>
    ///     Called when anything in the game tries to set a generic variable
    /// </summary>
    /// <param name="type">The type of the variable</param>
    /// <param name="varName">Name of the variable</param>
    /// <param name="value">Value to be used</param>
    public delegate object SetVariableProxy(Type type, string varName, object value);

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

    /// <summary>
    ///     Called when TMP_Text.isRightToLeftText is requested
    /// </summary>
    /// <param name="direction">The currently set text direction</param>
    /// <return>Modified text direction</return>
    public delegate bool TextDirectionProxy(bool direction);
}