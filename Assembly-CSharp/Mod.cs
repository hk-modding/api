using System;

namespace Modding
{

    public class Mod
    {
        public virtual void Initialize()
        {
        }
        public virtual void Unload()
        {
        }
        public virtual string GetVersion() => "UNKNOWN";
    }

    public class Mod<T> : Mod, IMod where T : IModSettings, new()
	{
		public Mod()
		{
			string name = GetType().Name;
			ModHooks.ModLog($"[{name}] - Instantiating Mod.");
			ModHooks.Instance.BeforeSavegameSaveHook += SaveSettings;
			ModHooks.Instance.AfterSavegameLoadHook += LoadSettings;
		}
		private void LoadSettings(SaveGameData data)
		{
			string name = GetType().Name;
			ModHooks.ModLog($"[{name}] - Loading Mod Settings from Save.");
			if (data?.modData != null && data.modData.ContainsKey(name))
			{
                Settings = data.modData[name];
			}
		}
		private void SaveSettings(SaveGameData data)
		{
			string name = GetType().Name;
			ModHooks.ModLog($"[{name}] - Adding Settings to Save file");
			if (data.modData == null)
			{
				data.modData = new ModSettingsDictionary();
			}
			if (data.modData.ContainsKey(name))
			{
				data.modData[name] = Settings;
				return;
			}
			data.modData.Add(name, Settings);
		}
		public IModSettings Settings
		{
			get => _settings ?? (_settings = Activator.CreateInstance<T>());
		    set => _settings = value;
		}
		private IModSettings _settings;
	}
}
