using UnityEngine;

namespace Modding
{
    public class PlayerData
    {
        public static PlayerData instance;

        public void SetBoolInternal(string boolName, bool value)
        {
            System.Reflection.FieldInfo field = base.GetType().GetField(boolName);
            if (field != null)
            {
                field.SetValue(PlayerData.instance, value);
                return;
            }
            Debug.Log("PlayerData: Could not find field named " + boolName + ", check variable name exists and FSM variable string is correct.");
        }

        // Token: 0x06002416 RID: 9238 RVA: 0x000D8CE8 File Offset: 0x000D6EE8
        public bool GetBoolInternal(string boolName)
        {
            if (string.IsNullOrEmpty(boolName))
            {
                return false;
            }
            System.Reflection.FieldInfo field = base.GetType().GetField(boolName);
            if (field != null)
            {
                return (bool)field.GetValue(PlayerData.instance);
            }
            Debug.Log("PlayerData: Could not find bool named " + boolName + " in PlayerData");
            return false;
        }

        // Token: 0x06002417 RID: 9239 RVA: 0x000D8D38 File Offset: 0x000D6F38
        public void SetIntInternal(string intName, int value)
        {
            System.Reflection.FieldInfo field = base.GetType().GetField(intName);
            if (field != null)
            {
                field.SetValue(PlayerData.instance, value);
                return;
            }
            Debug.Log("PlayerData: Could not find field named " + intName + ", check variable name exists and FSM variable string is correct.");
        }

        // Token: 0x06002418 RID: 9240 RVA: 0x000D8D7C File Offset: 0x000D6F7C
        public int GetIntInternal(string intName)
        {
            if (string.IsNullOrEmpty(intName))
            {
                Debug.LogError("PlayerData: Int with an EMPTY name requested.");
                return -9999;
            }
            System.Reflection.FieldInfo field = base.GetType().GetField(intName);
            if (field != null)
            {
                return (int)field.GetValue(PlayerData.instance);
            }
            Debug.LogError("PlayerData: Could not find int named " + intName + " in PlayerData");
            return -9999;
        }
    }
}
