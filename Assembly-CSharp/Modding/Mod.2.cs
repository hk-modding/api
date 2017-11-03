using System;

namespace Modding
{
	// Token: 0x020009C1 RID: 2497
	public class Mod<T> : Mod, IMod where T : IModSettings, new()
	{
		// Token: 0x06003319 RID: 13081
		public Mod()
		{
			string name = base.GetType().Name;
			ModHooks.ModLog(string.Format("[{0}] - Instantiating Mod.", name));
			ModHooks.Instance.BeforeSavegameSaveHook += this.SaveSettings;
			ModHooks.Instance.AfterSavegameLoadHook += this.LoadSettings;
		}

		// Token: 0x0600331A RID: 13082
		private void LoadSettings(SaveGameData data)
		{
			string name = base.GetType().Name;
			ModHooks.ModLog(string.Format("[{0}] - Loading Mod Settings from Save.", name));
			if (data != null && data.modData != null && data.modData.ContainsKey(name))
			{
				this.Settings = data.modData[name];
			}
		}

		// Token: 0x0600331B RID: 13083
		private void SaveSettings(SaveGameData data)
		{
			string name = base.GetType().Name;
			ModHooks.ModLog(string.Format("[{0}] - Adding Settings to Save file", name));
			if (data.modData == null)
			{
				data.modData = new ModSettingsDictionary();
			}
			if (data.modData.ContainsKey(name))
			{
				data.modData[name] = this.Settings;
				return;
			}
			data.modData.Add(name, this.Settings);
		}

		// Token: 0x170004A3 RID: 1187
		// (get) Token: 0x06003638 RID: 13880
		// (set) Token: 0x06003639 RID: 13881
		public IModSettings Settings
		{
			get
			{
				if (this._settings == null)
				{
					this._settings = Activator.CreateInstance<T>();
				}
				return this._settings;
			}
			set
			{
				this._settings = value;
			}
		}

		// Token: 0x04003C7A RID: 15482
		public IModSettings _settings;
	}
}
