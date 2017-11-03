using System;
using System.Collections.Generic;
using System.IO;
using GlobalEnums;
using Language;
using UnityEngine;

namespace Modding
{
	// Token: 0x020009BC RID: 2492
	public class ModHooks
	{
		// Token: 0x060032C3 RID: 12995 RVA: 0x00025537 File Offset: 0x00023737
		private ModHooks()
		{
			this.newLogfile = true;
		}

		// Token: 0x1700046C RID: 1132
		// (get) Token: 0x060032C4 RID: 12996 RVA: 0x00025551 File Offset: 0x00023751
		public static ModHooks Instance
		{
			get
			{
				if (ModHooks._instance == null)
				{
					ModHooks._instance = new ModHooks();
				}
				return ModHooks._instance;
			}
		}

		// Token: 0x14000040 RID: 64
		// (add) Token: 0x060032C5 RID: 12997 RVA: 0x00137EF4 File Offset: 0x001360F4
		// (remove) Token: 0x060032C6 RID: 12998 RVA: 0x00137F2C File Offset: 0x0013612C
		[HookInfo("Called after a new Scene has been loaded", "N/A")]
		public event SceneChangedHandler SceneChanged;

		// Token: 0x060032C7 RID: 12999 RVA: 0x00025569 File Offset: 0x00023769
		public void OnSceneChanged(string targetScene)
		{
			if (this.SceneChanged != null)
			{
				this.SceneChanged(targetScene);
			}
		}

		// Token: 0x060032C8 RID: 13000 RVA: 0x0002557F File Offset: 0x0002377F
		public void SetPlayerBool(string target, bool val)
		{
			if (this.SetPlayerBoolHook != null)
			{
				this.SetPlayerBoolHook(target, val);
				return;
			}
			PlayerData.instance.SetBoolInternal(target, val);
		}

		// Token: 0x060032C9 RID: 13001 RVA: 0x00137F64 File Offset: 0x00136164
		public bool GetPlayerBool(string target)
		{
			bool boolInternal = PlayerData.instance.GetBoolInternal(target);
			bool result = boolInternal;
			bool flag = false;
			if (this.GetPlayerBoolHook != null)
			{
				Delegate[] invocationList = this.GetPlayerBoolHook.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					bool flag2 = (bool)invocationList[i].DynamicInvoke(new object[]
					{
						target
					});
					if (flag2 != boolInternal && !flag)
					{
						result = flag2;
						flag = true;
					}
				}
			}
			return result;
		}

		// Token: 0x14000041 RID: 65
		// (add) Token: 0x060032CA RID: 13002 RVA: 0x00137FD0 File Offset: 0x001361D0
		// (remove) Token: 0x060032CB RID: 13003 RVA: 0x00138008 File Offset: 0x00136208
		[HookInfo("Called when anything in the game tries to set a bool in player data", "PlayerData.SetBool")]
		public event SetBoolProxy SetPlayerBoolHook;

		// Token: 0x14000042 RID: 66
		// (add) Token: 0x060032CC RID: 13004 RVA: 0x00138040 File Offset: 0x00136240
		// (remove) Token: 0x060032CD RID: 13005 RVA: 0x00138078 File Offset: 0x00136278
		[HookInfo("Called when anything in the game tries to get a bool from player data", "PlayerData.GetBool")]
		public event GetBoolProxy GetPlayerBoolHook;

		// Token: 0x14000043 RID: 67
		// (add) Token: 0x060032CE RID: 13006 RVA: 0x001380B0 File Offset: 0x001362B0
		// (remove) Token: 0x060032CF RID: 13007 RVA: 0x001380E8 File Offset: 0x001362E8
		[HookInfo("Called directly after a save has been loaded", "GameManager.LoadGame")]
		public event SavegameLoadHandler SavegameLoadHook;

		// Token: 0x14000044 RID: 68
		// (add) Token: 0x060032D0 RID: 13008 RVA: 0x00138120 File Offset: 0x00136320
		// (remove) Token: 0x060032D1 RID: 13009 RVA: 0x00138158 File Offset: 0x00136358
		[HookInfo("Called directly after a save has been saved", "GameManager.SaveGame")]
		public event SavegameSaveHandler SavegameSaveHook;

		// Token: 0x14000045 RID: 69
		// (add) Token: 0x060032D2 RID: 13010 RVA: 0x00138190 File Offset: 0x00136390
		// (remove) Token: 0x060032D3 RID: 13011 RVA: 0x001381C8 File Offset: 0x001363C8
		[HookInfo("Called whenever a new game is started", "GameManager.LoadFirstScene")]
		public event NewGameHandler NewGameHook;

		// Token: 0x060032D4 RID: 13012 RVA: 0x000255A3 File Offset: 0x000237A3
		public void OnSavegameLoad(int id)
		{
			if (this.SavegameLoadHook != null)
			{
				this.SavegameLoadHook(id);
			}
		}

		// Token: 0x060032D5 RID: 13013 RVA: 0x000255B9 File Offset: 0x000237B9
		public void OnSavegameSave(int id)
		{
			if (this.SavegameSaveHook != null)
			{
				this.SavegameSaveHook(id);
			}
		}

		// Token: 0x060032D6 RID: 13014 RVA: 0x000255CF File Offset: 0x000237CF
		public void OnNewGame()
		{
			if (this.NewGameHook != null)
			{
				this.NewGameHook();
			}
		}

		// Token: 0x060032D7 RID: 13015 RVA: 0x00138200 File Offset: 0x00136400
		public static void ModLog(string info)
		{
			if (ModLoader.debug)
			{
				using (StreamWriter streamWriter = new StreamWriter(Application.persistentDataPath + "\\ModLog.txt", !ModHooks.Instance.newLogfile))
				{
					streamWriter.WriteLine(info);
				}
				if (ModHooks.Instance.newLogfile)
				{
					ModHooks.Instance.newLogfile = false;
				}
			}
		}

		// Token: 0x14000046 RID: 70
		// (add) Token: 0x060032D8 RID: 13016 RVA: 0x00138270 File Offset: 0x00136470
		// (remove) Token: 0x060032D9 RID: 13017 RVA: 0x001382A8 File Offset: 0x001364A8
		[HookInfo("Called whenever localization specific strings are requested", "N/A")]
		public event LanguageGetHandler LanguageGetHook;

		// Token: 0x060032DA RID: 13018 RVA: 0x001382E0 File Offset: 0x001364E0
		public string LanguageGet(string key, string sheet)
		{

            
            string @internal = Language.Language.GetInternal(key, sheet);
			string result = @internal;
			bool flag = false;
			if (this.LanguageGetHook != null)
			{
				Delegate[] invocationList = this.LanguageGetHook.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					string text = (string)invocationList[i].DynamicInvoke(new object[]
					{
						key,
						sheet
					});
					if (text != @internal && !flag)
					{
						result = text;
						flag = true;
					}
				}
			}
			return result;
		}

		// Token: 0x060032DB RID: 13019 RVA: 0x00138350 File Offset: 0x00136550
		public string BeforeSceneLoad(string sceneName)
		{
			if (this.BeforeSceneLoadHook != null)
			{
				Delegate[] invocationList = this.BeforeSceneLoadHook.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					sceneName = (string)invocationList[i].DynamicInvoke(new object[]
					{
						sceneName
					});
				}
			}
			return sceneName;
		}

		// Token: 0x14000047 RID: 71
		// (add) Token: 0x060032DC RID: 13020 RVA: 0x0013839C File Offset: 0x0013659C
		// (remove) Token: 0x060032DD RID: 13021 RVA: 0x001383D4 File Offset: 0x001365D4
		[HookInfo("Called right before a scene gets loaded, can change which scene gets loaded", "N/A")]
		public event BeforeSceneLoadHandler BeforeSceneLoadHook;

		// Token: 0x060032DE RID: 13022 RVA: 0x000255E4 File Offset: 0x000237E4
		public void OnSavegameClear(int id)
		{
			if (this.SavegameClearHook != null)
			{
				this.SavegameClearHook(id);
			}
		}

		// Token: 0x14000048 RID: 72
		// (add) Token: 0x060032DF RID: 13023 RVA: 0x0013840C File Offset: 0x0013660C
		// (remove) Token: 0x060032E0 RID: 13024 RVA: 0x00138444 File Offset: 0x00136644
		[HookInfo("Called whenever a save file is deleted", "GameManager.ClearSaveFile")]
		public event ClearSaveGameHandler SavegameClearHook;

		// Token: 0x060032E1 RID: 13025 RVA: 0x000255FA File Offset: 0x000237FA
		public void SetPlayerInt(string target, int val)
		{
			if (this.SetPlayerIntHook != null)
			{
				this.SetPlayerIntHook(target, val);
				return;
			}
			PlayerData.instance.SetIntInternal(target, val);
		}

		// Token: 0x060032E2 RID: 13026 RVA: 0x0013847C File Offset: 0x0013667C
		public int GetPlayerInt(string target)
		{
			int intInternal = PlayerData.instance.GetIntInternal(target);
			int result = intInternal;
			bool flag = false;
			if (this.GetPlayerIntHook != null)
			{
				Delegate[] invocationList = this.GetPlayerIntHook.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					int num = (int)invocationList[i].DynamicInvoke(new object[]
					{
						target
					});
					if (num != intInternal && !flag)
					{
						result = num;
						flag = true;
					}
				}
			}
			return result;
		}

		// Token: 0x14000049 RID: 73
		// (add) Token: 0x060032E3 RID: 13027 RVA: 0x001384E8 File Offset: 0x001366E8
		// (remove) Token: 0x060032E4 RID: 13028 RVA: 0x00138520 File Offset: 0x00136720
		[HookInfo("Called when anything in the game tries to set an int in player data", "PlayerData.SetInt")]
		public event SetIntProxy SetPlayerIntHook;

		// Token: 0x1400004A RID: 74
		// (add) Token: 0x060032E5 RID: 13029 RVA: 0x00138558 File Offset: 0x00136758
		// (remove) Token: 0x060032E6 RID: 13030 RVA: 0x00138590 File Offset: 0x00136790
		[HookInfo("Called when anything in the game tries to get an int from player data", "PlayerData.GetInt")]
		public event GetIntProxy GetPlayerIntHook;

		// Token: 0x060032E7 RID: 13031 RVA: 0x001385C8 File Offset: 0x001367C8
		public int OnTakeHealth(int damage)
		{
			if (this.TakeHealthHook != null)
			{
				Delegate[] invocationList = this.TakeHealthHook.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					damage = (int)invocationList[i].DynamicInvoke(new object[]
					{
						damage
					});
				}
			}
			return damage;
		}

		// Token: 0x060032E8 RID: 13032 RVA: 0x00138618 File Offset: 0x00136818
		public int OnTakeDamage(ref int HazardType, int damage)
		{
			if (this.TakeDamageHook != null)
			{
				Delegate[] invocationList = this.TakeDamageHook.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					damage = (int)invocationList[i].DynamicInvoke(new object[]
					{
						HazardType,
						damage
					});
				}
			}
			return damage;
		}

		// Token: 0x1400004B RID: 75
		// (add) Token: 0x060032E9 RID: 13033 RVA: 0x00138670 File Offset: 0x00136870
		// (remove) Token: 0x060032EA RID: 13034 RVA: 0x001386A8 File Offset: 0x001368A8
		[HookInfo("Called when health is taken from the player", "HeroController.TakeHealth")]
		public event TakeHealthProxy TakeHealthHook;

		// Token: 0x1400004C RID: 76
		// (add) Token: 0x060032EB RID: 13035 RVA: 0x001386E0 File Offset: 0x001368E0
		// (remove) Token: 0x060032EC RID: 13036 RVA: 0x00138718 File Offset: 0x00136918
		[HookInfo("Called when damage is dealt to the player", "HeroController.TakeDamage")]
		public event TakeDamageProxy TakeDamageHook;

		// Token: 0x060032ED RID: 13037 RVA: 0x0002561E File Offset: 0x0002381E
		public void OnAttack(AttackDirection dir)
		{
			if (this.AttackHook != null)
			{
				this.AttackHook(dir);
			}
		}

		// Token: 0x1400004D RID: 77
		// (add) Token: 0x060032EE RID: 13038 RVA: 0x00138750 File Offset: 0x00136950
		// (remove) Token: 0x060032EF RID: 13039 RVA: 0x00138788 File Offset: 0x00136988
		[HookInfo("Called whenever the player attacks", "HeroController.Attack")]
		public event AttackHandler AttackHook;

		// Token: 0x1400004E RID: 78
		// (add) Token: 0x060032F0 RID: 13040 RVA: 0x001387C0 File Offset: 0x001369C0
		// (remove) Token: 0x060032F1 RID: 13041 RVA: 0x001387F8 File Offset: 0x001369F8
		[HookInfo("Called after setting up a new PlayerData", "PlayerData.SetupNewPlayerData")]
		public event NewPlayerDataHandler NewPlayerDataHook;

		// Token: 0x060032F2 RID: 13042 RVA: 0x00025634 File Offset: 0x00023834
		public void AfterNewPlayerData()
		{
			if (this.NewPlayerDataHook != null)
			{
				this.NewPlayerDataHook(PlayerData.instance);
			}
		}

		// Token: 0x060032F3 RID: 13043 RVA: 0x0002564E File Offset: 0x0002384E
		public void OnCharmUpdate()
		{
			if (this.CharmUpdateHook != null)
			{
				this.CharmUpdateHook(PlayerData.instance, HeroController.instance);
			}
		}

		// Token: 0x1400004F RID: 79
		// (add) Token: 0x060032F4 RID: 13044 RVA: 0x00138830 File Offset: 0x00136A30
		// (remove) Token: 0x060032F5 RID: 13045 RVA: 0x00138868 File Offset: 0x00136A68
		[HookInfo("Called after player values for charms have been set", "HeroController.CharmUpdate")]
		public event CharmUpdateHandler CharmUpdateHook;

		// Token: 0x060032F6 RID: 13046 RVA: 0x0002566D File Offset: 0x0002386D
		public void AfterAttack(AttackDirection dir)
		{
			if (this.AfterAttackHook != null)
			{
				this.AfterAttackHook(dir);
			}
		}

		// Token: 0x14000050 RID: 80
		// (add) Token: 0x060032F7 RID: 13047 RVA: 0x001388A0 File Offset: 0x00136AA0
		// (remove) Token: 0x060032F8 RID: 13048 RVA: 0x001388D8 File Offset: 0x00136AD8
		[HookInfo("Called at the end of the attack function", "HeroController.Attack")]
		public event AfterAttackHandler AfterAttackHook;

		// Token: 0x060032F9 RID: 13049 RVA: 0x00025683 File Offset: 0x00023883
		public void OnHeroUpdate()
		{
			if (this.HeroUpdateHook != null)
			{
				this.HeroUpdateHook();
			}
		}

		// Token: 0x14000051 RID: 81
		// (add) Token: 0x060032FA RID: 13050 RVA: 0x00138910 File Offset: 0x00136B10
		// (remove) Token: 0x060032FB RID: 13051 RVA: 0x00138948 File Offset: 0x00136B48
		[HookInfo("Called whenever the hero updates", "HeroController.Update")]
		public event HeroUpdateHandler HeroUpdateHook;

		// Token: 0x060032FC RID: 13052 RVA: 0x00025698 File Offset: 0x00023898
		public Vector2 DashVelocityChange()
		{
			if (this.DashVectorHook != null)
			{
				return this.DashVectorHook();
			}
			return Vector2.zero;
		}

		// Token: 0x060032FD RID: 13053 RVA: 0x000256B3 File Offset: 0x000238B3
		public bool OnDashPressed()
		{
			if (this.DashPressedHook != null)
			{
				this.DashPressedHook();
				return true;
			}
			return false;
		}

		// Token: 0x14000052 RID: 82
		// (add) Token: 0x060032FE RID: 13054 RVA: 0x00138980 File Offset: 0x00136B80
		// (remove) Token: 0x060032FF RID: 13055 RVA: 0x001389B8 File Offset: 0x00136BB8
		[HookInfo("Called during dash function to change velocity", "HeroController.Dash")]
		public event DashVelocityHandler DashVectorHook;

		// Token: 0x14000053 RID: 83
		// (add) Token: 0x06003300 RID: 13056 RVA: 0x001389F0 File Offset: 0x00136BF0
		// (remove) Token: 0x06003301 RID: 13057 RVA: 0x00138A28 File Offset: 0x00136C28
		[HookInfo("Called whenever the dash key is pressed. Overrides normal dash functionality", "HeroController.LookForQueueInput")]
		public event DashPressedHandler DashPressedHook;

		// Token: 0x06003302 RID: 13058 RVA: 0x000256CB File Offset: 0x000238CB
		public void OnColliderCreate(GameObject go)
		{
			if (this.ColliderCreateHook != null)
			{
				this.ColliderCreateHook(go);
			}
		}

		// Token: 0x14000054 RID: 84
		// (add) Token: 0x06003303 RID: 13059 RVA: 0x00138A60 File Offset: 0x00136C60
		// (remove) Token: 0x06003304 RID: 13060 RVA: 0x00138A98 File Offset: 0x00136C98
		[HookInfo("Called whenever a new gameobject is created with a collider and playmaker2d", "PlayMakerUnity2DProxy.Start")]
		public event ColliderCreateHandler ColliderCreateHook;

		// Token: 0x06003305 RID: 13061 RVA: 0x000256E1 File Offset: 0x000238E1
		public void OnApplicationQuit()
		{
			if (this.ApplicationQuitHook != null)
			{
				this.ApplicationQuitHook();
			}
		}

		// Token: 0x14000055 RID: 85
		// (add) Token: 0x06003306 RID: 13062 RVA: 0x00138AD0 File Offset: 0x00136CD0
		// (remove) Token: 0x06003307 RID: 13063 RVA: 0x00138B08 File Offset: 0x00136D08
		[HookInfo("Called when the game is fully closed", "GameManager.OnApplicationQuit")]
		public event ApplicationQuitHandler ApplicationQuitHook;

		// Token: 0x14000056 RID: 86
		// (add) Token: 0x06003308 RID: 13064 RVA: 0x00138B40 File Offset: 0x00136D40
		// (remove) Token: 0x06003309 RID: 13065 RVA: 0x00138B78 File Offset: 0x00136D78
		[HookInfo("Called directly after a save has been loaded.  Allows for accessing SaveGame instance.", "GameManager.LoadGame")]
		public event AfterSavegameLoadHandler AfterSavegameLoadHook;

		// Token: 0x14000057 RID: 87
		// (add) Token: 0x0600330A RID: 13066 RVA: 0x00138BB0 File Offset: 0x00136DB0
		// (remove) Token: 0x0600330B RID: 13067 RVA: 0x00138BE8 File Offset: 0x00136DE8
		[HookInfo("Called directly before save has been saved to allow for changes to the data before persisted.", "GameManager.SaveGame")]
		public event BeforeSavegameSaveHandler BeforeSavegameSaveHook;

		// Token: 0x0600330C RID: 13068 RVA: 0x000256F6 File Offset: 0x000238F6
		public void OnAfterSaveGameLoad(SaveGameData data)
		{
			if (this.AfterSavegameLoadHook != null)
			{
				this.AfterSavegameLoadHook(data);
			}
		}

		// Token: 0x0600330D RID: 13069 RVA: 0x0002570C File Offset: 0x0002390C
		public void OnBeforeSaveGameSave(SaveGameData data)
		{
			if (this.BeforeSavegameSaveHook != null)
			{
				this.BeforeSavegameSaveHook(data);
			}
		}

		// Token: 0x04003B4F RID: 15183
		private static ModHooks _instance;

		// Token: 0x04003B50 RID: 15184
		private bool newLogfile;

		// Token: 0x04003B51 RID: 15185
		public List<string> loadedMods = new List<string>();
	}
}
