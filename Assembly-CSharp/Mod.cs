using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;

// ReSharper disable file UnusedMember.Global

namespace Modding
{
    /// <inheritdoc cref="Loggable" />
    /// <inheritdoc cref="IMod" />
    /// <summary>
    ///     Base mod class.
    /// </summary>
    [PublicAPI]
    public abstract class Mod : Loggable, IMod
    {
        private readonly string _globalSettingsPath;

        /// <summary>
        /// Gets or sets the save settings of this Mod
        /// </summary>
        public virtual ModSettings SaveSettings
        {
            get => null;
            // ReSharper disable once ValueParameterNotUsed overriden by super class
            set { }
        }

        /// <summary>
        /// Gets or sets the global settings of this Mod
        /// </summary>
        public virtual ModSettings GlobalSettings
        {
            get => null;
            // ReSharper disable once ValueParameterNotUsed overriden by super class
            set { }
        }

        /// <summary>
        ///     The Mods Name
        /// </summary>
        public readonly string Name;

        /// <inheritdoc />
        /// <summary>
        ///     Legacy constructor instead of optional argument to not break old mods
        /// </summary>
        protected Mod() : this(null)
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Constructs the mod, assigns the instance and sets the name.
        /// </summary>
        protected Mod(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = GetType().Name;
            }

            Name = name;

            Log("Initializing");

#pragma warning disable 618 // Using obsolete Mod<> for backwards compatibility
            if (ModLoader.IsSubclassOfRawGeneric(typeof(Mod<>), GetType()))
            {
                return;
            }
#pragma warning restore 618

            if (_globalSettingsPath == null)
            {
                _globalSettingsPath = Application.persistentDataPath + ModHooks.PathSeperator + GetType().Name +
                                      ".GlobalSettings.json";
            }

            LoadGlobalSettings();
            HookSaveMethods();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Get's the Mod's Name
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return Name;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Returns the objects to preload in order for the mod to work.
        /// </summary>
        /// <returns>A List of tuples containing scene name, object name</returns>
        public virtual List<(string, string)> GetPreloadNames()
        {
            return null;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Called after preloading of all mods.
        /// </summary>
        /// <param name="preloadedObjects">The preloaded objects relevant to this <see cref="Mod" /></param>
        public virtual void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            // Call the other Initialize to not break older mods
            Initialize();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Returns version of Mod
        /// </summary>
        /// <returns>Mod Version</returns>
        public virtual string GetVersion()
        {
            return "UNKNOWN";
        }

        /// <inheritdoc />
        /// <summary>
        ///     Denotes if the running version is the current version.  Set this with <see cref="T:Modding.GithubVersionHelper" />
        /// </summary>
        /// <returns>If the version is current or not.</returns>
        public virtual bool IsCurrent()
        {
            return true;
        }

        /// <summary>
        ///     Controls when this mod should load compared to other mods.  Defaults to ordered by name.
        /// </summary>
        /// <returns></returns>
        public virtual int LoadPriority()
        {
            return 1;
        }

        /// <summary>
        ///     Called after preloading of all mods.
        /// </summary>
        public virtual void Initialize()
        {
        }

        private void HookSaveMethods()
        {
            ModHooks.Instance.BeforeSavegameSaveHook += SaveSaveSettings;
            ModHooks.Instance.AfterSavegameLoadHook += LoadSaveSettings;
            ModHooks.Instance.ApplicationQuitHook += SaveGlobalSettings;
        }

        private void LoadGlobalSettings()
        {
            Log("Loading Global Settings");
            if (!File.Exists(_globalSettingsPath))
            {
                return;
            }

            using (FileStream fileStream = File.OpenRead(_globalSettingsPath))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    string json = reader.ReadToEnd();
                    try
                    {
                        ModSettings oldSettings = GlobalSettings;
                        if (oldSettings == null)
                        {
                            return;
                        }

                        Type settingsType = oldSettings.GetType();
                        ModSettings newSettings = (ModSettings)JsonUtility.FromJson(json, settingsType);

                        // ReSharper disable once SuspiciousTypeConversion.Global
                        if (newSettings is ISerializationCallbackReceiver receiver)
                        {
                            receiver.OnAfterDeserialize();
                        }

                        GlobalSettings = newSettings;
                    }
                    catch (Exception e)
                    {
                        LogError(e);
                    }
                }
            }
        }

        private void SaveGlobalSettings()
        {
            Log("Saving Global Settings");
            if (File.Exists(_globalSettingsPath + ".bak"))
            {
                File.Delete(_globalSettingsPath + ".bak");
            }

            if (File.Exists(_globalSettingsPath))
            {
                File.Move(_globalSettingsPath, _globalSettingsPath + ".bak");
            }

            ModSettings settings = GlobalSettings;

            // ReSharper disable once SuspiciousTypeConversion.Global
            if (settings is ISerializationCallbackReceiver receiver)
            {
                receiver.OnBeforeSerialize();
            }
            else if (settings == null)
            {
                return;
            }

            using (FileStream fileStream = File.Create(_globalSettingsPath))
            {
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    try
                    {
                        string text4 = JsonUtility.ToJson(settings, true);
                        writer.Write(text4);
                    }
                    catch (Exception e)
                    {
                        LogError(e);
                    }
                }
            }
        }

        private void LoadSaveSettings(Patches.SaveGameData data)
        {
            try
            {
                string name = GetType().Name;
                Log("Loading Mod Settings from Save.");

                if (data?.modData == null || !data.modData.ContainsKey(name))
                {
                    return;
                }

                ModSettings oldSettings = SaveSettings;
                if (oldSettings == null)
                {
                    return;
                }

                Type saveSettingsType = oldSettings.GetType();

                ModSettings saveSettings = (ModSettings)Activator.CreateInstance(saveSettingsType);

                saveSettings.SetSettings(data.modData[name]);

                // ReSharper disable once SuspiciousTypeConversion.Global
                if (saveSettings is ISerializationCallbackReceiver callbackReceiver)
                {
                    callbackReceiver.OnAfterDeserialize();
                }

                SaveSettings = saveSettings;
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        private void SaveSaveSettings(Patches.SaveGameData data)
        {
            ModSettings saveSettings = null;
            try
            {
                saveSettings = SaveSettings;
            }
            catch (Exception e)
            {
                LogError(e);
            }

            if (saveSettings == null)
            {
                return;
            }

            string name = GetType().Name;
            Log("Adding Settings to Save file");
            if (data.modData == null)
            {
                data.modData = new ModSettingsDictionary();
            }

            if (data.modData.ContainsKey(name))
            {
                data.modData[name] = saveSettings;
                return;
            }

            data.modData.Add(name, saveSettings);
        }
    }

    /// <inheritdoc />
    /// <typeparam name="TSaveSettings">A Mod specific implementation of <see cref="ModSettings" /></typeparam>
    /// <remarks>Provides automatic managment of saving mod settings in save file.</remarks>
    [PublicAPI]
    [Obsolete("Use SaveSettings member on non-generic Mod")]
    public class Mod<TSaveSettings> : Mod where TSaveSettings : ModSettings, new()
    {
        private TSaveSettings _settings;

        /// <inheritdoc />
        /// <summary>
        ///     Legacy constructor instead of optional argument to not break old mods
        /// </summary>
        public Mod() : this(null)
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Instantiates Mod and adds hooks to store and retrieve mod settings during save/load.
        /// </summary>
        public Mod(string name) : base(name)
        {
            Log("Instantiating Mod");
            ModHooks.Instance.BeforeSavegameSaveHook += SaveSettings;
            ModHooks.Instance.AfterSavegameLoadHook += LoadSettings;
        }

        /// <summary>
        ///     Mod's Settings
        /// </summary>
        public TSaveSettings Settings
        {
            get => _settings ?? (_settings = Activator.CreateInstance<TSaveSettings>());
            set => _settings = value;
        }

        /// <summary>
        ///     Loads settings from a save file.
        /// </summary>
        /// <param name="data"></param>
        private void LoadSettings(Patches.SaveGameData data)
        {
            string name = GetType().Name;
            Log("Loading Mod Settings from Save.");

            if (data?.modData == null || !data.modData.ContainsKey(name))
            {
                return;
            }

            Settings = new TSaveSettings();
            Settings.SetSettings(data.modData[name]);

            // ReSharper disable once SuspiciousTypeConversion.Global
            if (Settings is ISerializationCallbackReceiver callbackReceiver)
            {
                callbackReceiver.OnAfterDeserialize();
            }
        }

        /// <summary>
        ///     Updates SaveGameData before it's saved to disk.
        /// </summary>
        /// <param name="data"></param>
        // new isn't redundant idk why it's saying it is
#pragma warning disable 109
        private new void SaveSettings(Patches.SaveGameData data)
#pragma warning restore 109
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
    }

    /// <inheritdoc />
    /// <summary>
    ///     Base class for mods that includes the ability to have both save specific and non-save-specific global settings.
    /// </summary>
    /// <typeparam name="TSaveSettings">A Mod specific implementation of <see cref="ModSettings" /></typeparam>
    /// <typeparam name="TGlobalSettings">
    ///     A Mod specific implementation of <see cref="ModSettings" /> used for
    ///     global/non-save-specific settings.
    /// </typeparam>
    [PublicAPI]
    [Obsolete("Use SaveSettings and GlobalSettings members on non-generic Mod")]
    public class Mod<TSaveSettings, TGlobalSettings> : Mod<TSaveSettings>
        where TSaveSettings : ModSettings, new()
        where TGlobalSettings : ModSettings, new()
    {
        private readonly string _globalSettingsFilename;

        private TGlobalSettings _globalSettings;

        /// <inheritdoc />
        /// <summary>
        ///     Legacy constructor instead of optional argument to not break old mods
        /// </summary>
        public Mod() : this(null)
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Basic Constructor for the Mod.
        /// </summary>
        public Mod(string name) : base(name)
        {
            _globalSettingsFilename = Application.persistentDataPath + ModHooks.PathSeperator + GetType().Name +
                                      ".GlobalSettings.json";
            LoadGlobalSettings();
        }

        /// <summary>
        ///     Global Settings which are stored independently of saves.
        /// </summary>
        public new TGlobalSettings GlobalSettings
        {
            get => _globalSettings ?? (_globalSettings = Activator.CreateInstance<TGlobalSettings>());
            set => _globalSettings = value;
        }

        /// <summary>
        ///     Save GlobalSettings to disk. (backs up the current global settings if it exists)
        /// </summary>
        public void SaveGlobalSettings()
        {
            Log("Saving Global Settings");
            if (File.Exists(_globalSettingsFilename + ".bak"))
            {
                File.Delete(_globalSettingsFilename + ".bak");
            }

            if (File.Exists(_globalSettingsFilename))
            {
                File.Move(_globalSettingsFilename, _globalSettingsFilename + ".bak");
            }

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
        ///     Loads global settings from disk (if they exist)
        /// </summary>
        public void LoadGlobalSettings()
        {
            Log("Loading Global Settings");
            if (!File.Exists(_globalSettingsFilename))
            {
                return;
            }

            using (FileStream fileStream = File.OpenRead(_globalSettingsFilename))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    string json = reader.ReadToEnd();
                    _globalSettings = JsonUtility.FromJson<TGlobalSettings>(json);
                }
            }
        }
    }
}