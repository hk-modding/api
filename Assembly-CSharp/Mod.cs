using System;

namespace Modding
{

    /// <inheritdoc />
    /// <summary>
    /// Base mod class.
    /// </summary>
    /// <remarks>Does not provide method to store mod settings in the save file.</remarks>
    public class Mod : IMod
    {
        public virtual void Initialize()
        {
        }
        public virtual void Unload()
        {
        }
        public virtual string GetVersion() => "UNKNOWN";
    }

    /// <inheritdoc />
    /// <typeparam name="T">A Mod specific implementation of <see cref="IModSettings"/></typeparam>
    /// <remarks>Provides automatic managment of saving mod settings in save file.</remarks>
    public class Mod<T> : Mod where T : IModSettings, new()
	{
        /// <summary>
        /// Instantiates Mod and adds hooks to store and retrieve mod settings during save/load.
        /// </summary>
		public Mod()
		{
			string name = GetType().Name;
			ModHooks.Logger.Log($"[{name}] - Instantiating Mod.");
			ModHooks.Instance.BeforeSavegameSaveHook += SaveSettings;
			ModHooks.Instance.AfterSavegameLoadHook += LoadSettings;
		}

        /// <summary>
        /// Loads settings from a save file.
        /// </summary>
        /// <param name="data"></param>
		private void LoadSettings(SaveGameData data)
		{
			string name = GetType().Name;
			ModHooks.Logger.Log($"[{name}] - Loading Mod Settings from Save.");
			if (data?.modData != null && data.modData.ContainsKey(name))
			{
                Settings = data.modData[name];
			}
		}

        /// <summary>
        /// Updates SaveGameData before it's saved to disk.
        /// </summary>
        /// <param name="data"></param>
		private void SaveSettings(SaveGameData data)
		{
			string name = GetType().Name;
			ModHooks.Logger.Log($"[{name}] - Adding Settings to Save file");
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

        /// <summary>
        /// Mod's Settings
        /// </summary>
		public IModSettings Settings
		{
			get => _settings ?? (_settings = Activator.CreateInstance<T>());
		    set => _settings = value;
		}

		private IModSettings _settings;
	}
}
