using MonoMod;
using UnityEngine;

// ReSharper disable All
#pragma warning disable 1591
#pragma warning disable CS0108, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::PlayerData")]
    public class PlayerData : global::PlayerData
    {
        [MonoModIgnore]
        public static PlayerData instance { get; set; }

        public void SetBoolInternal(string boolName, bool value)
        {
            ReflectionHelper.SetAttrSafe(this, boolName, value);
        }

        public bool GetBoolInternal(string boolName)
        {
            return ReflectionHelper.GetAttr<PlayerData, bool, bool?>(this, boolName) ?? false;
        }

        public void SetIntInternal(string intName, int value)
        {
            ReflectionHelper.SetAttrSafe(this, intName, value);
        }

        public int GetIntInternal(string intName)
        {
            return ReflectionHelper.GetAttr<PlayerData, int, int?>(this, intName) ?? -9999;
        }

        public void SetFloatInternal(string floatName, float value)
        {
            ReflectionHelper.SetAttrSafe(this, floatName, value);
        }

        public float GetFloatInternal(string floatName)
        {
            return ReflectionHelper.GetAttr<PlayerData, float, float?>(this, floatName) ?? -9999f;
        }

        public void SetStringInternal(string stringName, string value)
        {
            ReflectionHelper.SetAttrSafe(this, stringName, value);
        }

        public string GetStringInternal(string stringName)
        {
            return ReflectionHelper.GetAttr<PlayerData, string, string>(this, stringName) ?? " ";
        }

        public void SetVector3Internal(string vector3Name, Vector3 value)
        {
            ReflectionHelper.SetAttrSafe(this, vector3Name, value);
        }

        public Vector3 GetVector3Internal(string vector3Name)
        {
            return ReflectionHelper.GetAttr<PlayerData, Vector3, Vector3?>(this, vector3Name) ?? Vector3.zero;
        }

        public void SetVariableInternal<T>(string variableName, T value)
        {
            ReflectionHelper.SetAttrSafe(this, variableName, value);
        }

        public T GetVariableInternal<T>(string variableName)
        {
            try
            {
                return ReflectionHelper.GetAttr<PlayerData, T, T>(this, variableName);
            }
            catch
            {
                return default(T);
            }
        }

        [MonoModReplace]
        public void SetBool(string boolName, bool value)
        {
            ModHooks.Instance.SetPlayerBool(boolName, value);
        }

        [MonoModReplace]
        public bool GetBool(string boolName)
        {
            return ModHooks.Instance.GetPlayerBool(boolName);
        }

        [MonoModReplace]
        public int GetInt(string intName)
        {
            return ModHooks.Instance.GetPlayerInt(intName);
        }

        [MonoModReplace]
        public void SetInt(string intName, int value)
        {
            ModHooks.Instance.SetPlayerInt(intName, value);
        }

        [MonoModReplace]
        public void IncrementInt(string intName)
        {
            if (ReflectionHelper.GetField(typeof(PlayerData), intName) != null)
            {
                ModHooks.Instance.SetPlayerInt(intName, this.GetIntInternal(intName) + 1);
                return;
            }

            Debug.Log("PlayerData: Could not find field named " + intName + ", check variable name exists and FSM variable string is correct.");
        }

        [MonoModReplace]
        public void DecrementInt(string intName)
        {
            if (ReflectionHelper.GetField(typeof(PlayerData), intName) != null)
            {
                ModHooks.Instance.SetPlayerInt(intName, this.GetIntInternal(intName) - 1);
            }
        }

        [MonoModReplace]
        public void IntAdd(string intName, int amount)
        {
            if (ReflectionHelper.GetField(typeof(PlayerData), intName) != null)
            {
                ModHooks.Instance.SetPlayerInt(intName, this.GetIntInternal(intName) + amount);
                return;
            }

            Debug.Log("PlayerData: Could not find field named " + intName + ", check variable name exists and FSM variable string is correct.");
        }

        [MonoModReplace]
        public float GetFloat(string floatName)
        {
            return ModHooks.Instance.GetPlayerFloat(floatName);
        }

        [MonoModReplace]
        public void SetFloat(string floatName, float value)
        {
            ModHooks.Instance.SetPlayerFloat(floatName, value);
        }

        [MonoModReplace]
        public string GetString(string stringName)
        {
            return ModHooks.Instance.GetPlayerString(stringName);
        }

        [MonoModReplace]
        public void SetString(string stringName, string value)
        {
            ModHooks.Instance.SetPlayerString(stringName, value);
        }

        [MonoModReplace]
        public Vector3 GetVector3(string vector3Name)
        {
            return ModHooks.Instance.GetPlayerVector3(vector3Name);
        }

        [MonoModReplace]
        public void SetVector3(string vector3Name, Vector3 value)
        {
            ModHooks.Instance.SetPlayerVector3(vector3Name, value);
        }

        [MonoModReplace]
        public T GetVariable<T>(string varName)
        {
            return ModHooks.Instance.GetPlayerVariable<T>(varName);
        }

        [MonoModReplace]
        public void SetVariable<T>(string varName, T value)
        {
            ModHooks.Instance.SetPlayerVariable<T>(varName, value);
        }

        public extern void orig_TakeHealth(int amount);

        public void TakeHealth(int amount)
        {
            amount = ModHooks.Instance.OnTakeHealth(amount);
            orig_TakeHealth(amount);
        }

        public extern void orig_UpdateBlueHealth();

        public void UpdateBlueHealth()
        {
            orig_UpdateBlueHealth();
            healthBlue += ModHooks.Instance.OnBlueHealth();
        }

        public extern void orig_AddHealth(int amount);

        public void AddHealth(int amount)
        {
            amount = ModHooks.Instance.BeforeAddHealth(amount);
            orig_AddHealth(amount);
        }
    }
}