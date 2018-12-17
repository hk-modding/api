using MonoMod;
using UnityEngine;
//We disable a bunch of warnings here because they don't mean anything.  They all relate to not finding proper stuff for methods/properties/fields that are stubs to make the new methods work.
//We don't care about XML docs for these as they are being patched into the original code
// ReSharper disable All
#pragma warning disable 1591
#pragma warning disable CS0108
namespace Modding.Patches
{
    [MonoModPatch("global::PlayerData")]
    public class PlayerData : global::PlayerData
    {
        [MonoModIgnore]
        public static PlayerData instance { get; set; }

        public void SetBoolInternal(string boolName, bool value)
        {
            ReflectionHelper.SetAttr(this, boolName, value);
        }


        public bool GetBoolInternal(string boolName)
        {
            return ReflectionHelper.GetAttr<bool?>(this, boolName) ?? false;
        }


        public void SetIntInternal(string intName, int value)
        {
            ReflectionHelper.SetAttr(this, intName, value);
        }


        public int GetIntInternal(string intName)
        {
            return ReflectionHelper.GetAttr<int?>(this, intName) ?? -9999;
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
            if (base.GetType().GetField(intName) != null)
            {
                ModHooks.Instance.SetPlayerInt(intName, this.GetIntInternal(intName) + 1);
                return;
            }
            Debug.Log("PlayerData: Could not find field named " + intName + ", check variable name exists and FSM variable string is correct.");
        }

        [MonoModReplace]
        public void DecrementInt(string intName)
        {
            if (GetType().GetField(intName) != null)
            {
                ModHooks.Instance.SetPlayerInt(intName, this.GetIntInternal(intName) - 1);
            }
        }

        [MonoModReplace]
        public void IntAdd(string intName, int amount)
        {
            if (base.GetType().GetField(intName) != null)
            {
                ModHooks.Instance.SetPlayerInt(intName, this.GetIntInternal(intName) + amount);
                return;
            }
            Debug.Log("PlayerData: Could not find field named " + intName + ", check variable name exists and FSM variable string is correct.");
        }

        [MonoModOriginalName("TakeHealth")]
        public void orig_TakeHealth(int amount) { }

        public void TakeHealth(int amount)
        {
            amount = ModHooks.Instance.OnTakeHealth(amount);
            orig_TakeHealth(amount);
        }

        /*
        [MonoModOriginalName("SetupNewPlayerData")]
        public void orig_SetupNewPlayerData() { }

        public bool ModDoHook = false;

        public void SetupNewPlayerData()
        {
            orig_SetupNewPlayerData();
            if (ModDoHook)
                ModHooks.Instance.AfterNewPlayerData(instance);
        }
        */

        public void UpdateBlueHealth()
        {
            healthBlue = ModHooks.Instance.OnBlueHealth();
            if (equippedCharm_8)
            {
                healthBlue += 2;
            }
            if (equippedCharm_9)
            {
                healthBlue += 4;
            }
        }
    
        [MonoModOriginalName("AddHealth")]
        public void orig_AddHealth(int amount) { }
    
        public void AddHealth(int amount)
        {
            amount = ModHooks.Instance.BeforeAddHealth(amount);
            orig_AddHealth(amount);
        }
    }
}
