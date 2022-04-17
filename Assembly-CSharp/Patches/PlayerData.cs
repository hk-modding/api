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
            ReflectionHelper.SetFieldSafe(this, boolName, value);
        }

        public bool GetBoolInternal(string boolName)
        {
            return ReflectionHelper.GetField<PlayerData, bool, bool?>(this, boolName) ?? false;
        }

        public void SetIntInternal(string intName, int value)
        {
            ReflectionHelper.SetFieldSafe(this, intName, value);
        }

        public int GetIntInternal(string intName)
        {
            return ReflectionHelper.GetField<PlayerData, int, int?>(this, intName) ?? -9999;
        }

        public void SetFloatInternal(string floatName, float value)
        {
            ReflectionHelper.SetFieldSafe(this, floatName, value);
        }

        public float GetFloatInternal(string floatName)
        {
            return ReflectionHelper.GetField<PlayerData, float, float?>(this, floatName) ?? -9999f;
        }

        public void SetStringInternal(string stringName, string value)
        {
            ReflectionHelper.SetFieldSafe(this, stringName, value);
        }

        public string GetStringInternal(string stringName)
        {
            return ReflectionHelper.GetField<PlayerData, string, string>(this, stringName) ?? " ";
        }

        public void SetVector3Internal(string vector3Name, Vector3 value)
        {
            ReflectionHelper.SetFieldSafe(this, vector3Name, value);
        }

        public Vector3 GetVector3Internal(string vector3Name)
        {
            return ReflectionHelper.GetField<PlayerData, Vector3, Vector3?>(this, vector3Name) ?? Vector3.zero;
        }

        public void SetVariableInternal<T>(string variableName, T value)
        {
            ReflectionHelper.SetFieldSafe(this, variableName, value);
        }

        public T GetVariableInternal<T>(string variableName)
        {
            try
            {
                return ReflectionHelper.GetField<PlayerData, T, T>(this, variableName);
            }
            catch
            {
                return default(T);
            }
        }

        [MonoModReplace]
        public void SetBool(string boolName, bool value)
        {
            ModHooks.SetPlayerBool(boolName, value);
        }

        [MonoModReplace]
        public bool GetBool(string boolName)
        {
            return ModHooks.GetPlayerBool(boolName);
        }

        [MonoModReplace]
        public int GetInt(string intName)
        {
            return ModHooks.GetPlayerInt(intName);
        }

        [MonoModReplace]
        public void SetInt(string intName, int value)
        {
            ModHooks.SetPlayerInt(intName, value);
        }

        [MonoModReplace]
        public void IncrementInt(string intName)
        {
            if (ReflectionHelper.GetFieldInfo(typeof(PlayerData), intName) != null)
            {
                ModHooks.SetPlayerInt(intName, this.GetIntInternal(intName) + 1);
                return;
            }

            Debug.Log("PlayerData: Could not find field named " + intName + ", check variable name exists and FSM variable string is correct.");
        }

        [MonoModReplace]
        public void DecrementInt(string intName)
        {
            if (ReflectionHelper.GetFieldInfo(typeof(PlayerData), intName) != null)
            {
                ModHooks.SetPlayerInt(intName, this.GetIntInternal(intName) - 1);
            }
        }

        [MonoModReplace]
        public void IntAdd(string intName, int amount)
        {
            if (ReflectionHelper.GetFieldInfo(typeof(PlayerData), intName) != null)
            {
                ModHooks.SetPlayerInt(intName, this.GetIntInternal(intName) + amount);
                return;
            }

            Debug.Log("PlayerData: Could not find field named " + intName + ", check variable name exists and FSM variable string is correct.");
        }

        [MonoModReplace]
        public float GetFloat(string floatName)
        {
            return ModHooks.GetPlayerFloat(floatName);
        }

        [MonoModReplace]
        public void SetFloat(string floatName, float value)
        {
            ModHooks.SetPlayerFloat(floatName, value);
        }

        [MonoModReplace]
        public string GetString(string stringName)
        {
            return ModHooks.GetPlayerString(stringName);
        }

        [MonoModReplace]
        public void SetString(string stringName, string value)
        {
            ModHooks.SetPlayerString(stringName, value);
        }

        [MonoModReplace]
        public Vector3 GetVector3(string vector3Name)
        {
            return ModHooks.GetPlayerVector3(vector3Name);
        }

        [MonoModReplace]
        public void SetVector3(string vector3Name, Vector3 value)
        {
            ModHooks.SetPlayerVector3(vector3Name, value);
        }

        [MonoModReplace]
        public T GetVariable<T>(string varName)
        {
            return ModHooks.GetPlayerVariable<T>(varName);
        }

        [MonoModReplace]
        public void SetVariable<T>(string varName, T value)
        {
            ModHooks.SetPlayerVariable<T>(varName, value);
        }

        public extern void orig_TakeHealth(int amount);

        public void TakeHealth(int amount)
        {
            amount = ModHooks.OnTakeHealth(amount);
            orig_TakeHealth(amount);
        }

        public extern void orig_UpdateBlueHealth();

        public void UpdateBlueHealth()
        {
            orig_UpdateBlueHealth();
            SetInt(nameof(healthBlue), GetInt(nameof(healthBlue)) + ModHooks.OnBlueHealth());
        }

        public extern void orig_AddHealth(int amount);

        public void AddHealth(int amount)
        {
            amount = ModHooks.BeforeAddHealth(amount);
            orig_AddHealth(amount);
        }
    }
}