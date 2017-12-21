using System;
using System.IO;
using UnityEngine;

namespace Modding
{
    /// <inheritdoc cref="Loggable" />
    /// <inheritdoc cref="IMod"/>
    /// <summary>
    /// Base mod class.
    /// </summary>
    /// <remarks>Does not provide method to store mod settings in the save file.</remarks>
    public class Mod : Loggable, IMod
    {

        /// <summary>
        /// The Mods Name
        /// </summary>
        public readonly string Name;

        /// <inheritdoc />
        /// <summary>
        /// Constrcuts the mod, assignes the instance and sets the name.
        /// </summary>
        public Mod()
        {
            Name = GetType().Name;
        }
        
        /// <inheritdoc />
        /// <summary>
        /// Called when class is first constructed.
        /// </summary>
        public virtual void Initialize() { }

        /// <inheritdoc />
        /// <summary>
        /// Called during game unload.
        /// </summary>
        public virtual void Unload() { }

        /// <inheritdoc />
        /// <summary>
        /// Returns version of Mod
        /// </summary>
        /// <returns>Mod Version</returns>
        public virtual string GetVersion() => "UNKNOWN";

        /// <inheritdoc />
        /// <summary>
        /// Denotes if the running version is the current version.  Set this with <see cref="T:Modding.GithubVersionHelper" />
        /// </summary>
        /// <returns>If the version is current or not.</returns>
        public virtual bool IsCurrent() => true;

        /// <summary>
        /// Controls when this mod should load compared to other mods.  Defaults to ordered by name.
        /// </summary>
        /// <returns></returns>
        public virtual int LoadPriority() => 1;
    }

    /// <inheritdoc />
    /// <typeparam name="TSaveSettings">A Mod specific implementation of <see cref="IModSettings"/></typeparam>
    /// <remarks>Provides automatic managment of saving mod settings in save file.</remarks>
    public class Mod<TSaveSettings> : Mod where TSaveSettings : IModSettings, new()
	{
	    
	    /// <inheritdoc />
	    /// <summary>
	    /// Instantiates Mod and adds hooks to store and retrieve mod settings during save/load.
	    /// </summary>
		public Mod()
		{
            Log("Instantiating Mod");
			ModHooks.Instance.BeforeSavegameSaveHook += SaveSettings;
			ModHooks.Instance.AfterSavegameLoadHook += LoadSettings;
		}

        /// <summary>
        /// Loads settings from a save file.
        /// </summary>
        /// <param name="data"></param>
		private void LoadSettings(Patches.SaveGameData data)
		{
			string name = GetType().Name;
			Log("Loading Mod Settings from Save.");
			if (data?.modData != null && data.modData.ContainsKey(name))
			{
			    Settings = new TSaveSettings();
                Settings.SetSettings(data.modData[name]);
			}
		}

        /// <summary>
        /// Updates SaveGameData before it's saved to disk.
        /// </summary>
        /// <param name="data"></param>
		private void SaveSettings(Patches.SaveGameData data)
		{
			string name = GetType().Name;
			Log("Adding Settings to Save file");
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
		public TSaveSettings Settings
		{
			get => _settings ?? (_settings = Activator.CreateInstance<TSaveSettings>());
		    set => _settings = value;
		}

		private TSaveSettings _settings;
	}

    /// <inheritdoc />
    /// <summary>
    /// Base class for mods that includes the ability to have both save specific and non-save-specific global settings.
    /// </summary>
    /// <typeparam name="TSaveSettings">A Mod specific implementation of <see cref="IModSettings"/></typeparam>
    /// <typeparam name="TGlobalSettings">A Mod specific implementation of <see cref="IModSettings"/> used for global/non-save-specific settings.</typeparam>
    public class Mod<TSaveSettings, TGlobalSettings> : Mod<TSaveSettings> 
        where TSaveSettings : IModSettings, new()
        where TGlobalSettings : IModSettings, new()
    {
        private readonly string _globalSettingsFilename;

        /// <inheritdoc />
        /// <summary>
        /// Basic Constructor for the Mod.  
        /// </summary>
        public Mod()
        {
            _globalSettingsFilename = Application.persistentDataPath + ModHooks.PathSeperator + GetType().Name + ".GlobalSettings.json";
            LoadGlobalSettings();
        }

        /// <summary>
        /// Save GlobalSettings to disk. (backs up the current global settings if it exists)
        /// </summary>
        public void SaveGlobalSettings()
        {
            Log("Saving Global Settings");
            if (File.Exists(_globalSettingsFilename + ".bak"))
                File.Delete(_globalSettingsFilename + ".bak");

            if (File.Exists(_globalSettingsFilename))
                File.Move(_globalSettingsFilename, _globalSettingsFilename + ".bak");

            using (FileStream fileStream = File.Create(_globalSettingsFilename))
            {
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    string text4 = JsonUtility.ToJson(GlobalSettings, true);
                    writer.Write(text4);
                }
            }
        }

        /// <summary>
        /// Loads global settings from disk (if they exist)
        /// </summary>
        public void LoadGlobalSettings()
        {
            Log("Loading Global Settings");
            if (!File.Exists(_globalSettingsFilename)) return;

            using (FileStream fileStream = File.OpenRead(_globalSettingsFilename))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    string json = reader.ReadToEnd();
                    _globalSettings = JsonUtility.FromJson<TGlobalSettings>(json);
                }
            }
        }

        /// <summary>
        /// Global Settings which are stored independently of saves.
        /// </summary>
        public TGlobalSettings GlobalSettings
        {
            get => _globalSettings ?? (_globalSettings = Activator.CreateInstance<TGlobalSettings>());
            set => _globalSettings = value;
        }

        private TGlobalSettings _globalSettings;
    }
}
