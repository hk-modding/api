using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Modding.Patches;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChanged;
        }

        private void SceneChanged(Scene arg0, Scene arg1)
        {
            if (arg1.name != Constants.MENU_SCENE) return;
            
            Type type = SaveSettings?.GetType();

            if (type == null)
                return;
                
            SaveSettings = (ModSettings) Activator.CreateInstance(type);
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
                            return;

                        Type settingsType = oldSettings.GetType();

                        ModSettings newSettings;

                        try
                        {
                            newSettings = JsonConvert.DeserializeObject(json, settingsType) as ModSettings;
                        }
                        catch (Exception e)
                        {
                            LogError("Failed to load settings using Json.Net, falling back.");
                            LogError(e);
                            
                            newSettings = JsonUtility.FromJson(json, settingsType) as ModSettings;
                        }

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
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (settings is ISerializationCallbackReceiver receiver)
            {
                try
                {
                    receiver.OnBeforeSerialize();
                }
                catch (Exception e)
                {
                    LogError(e);
                }
            }
            else if (settings == null)
            {
                return;
            }

            using FileStream fileStream = File.Create(_globalSettingsPath);

            using var writer = new StreamWriter(fileStream);
            
            try
            {
                try
                {
                    writer.Write(JsonConvert.SerializeObject(settings, Formatting.Indented, new JsonSerializerSettings
                    {
                        ContractResolver = ShouldSerializeContractResolver.Instance
                    }));
                }
                catch (Exception e)
                {
                    LogError("Failed to serialize settings using Json.NET, falling back.");
                    LogError(e);

                    writer.Write(JsonUtility.ToJson(settings, true));
                }
            }
            catch (Exception e)
            {
                LogError(e);
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

                Type saveSettingsType = SaveSettings?.GetType();

                if (saveSettingsType == null)
                    return;

                var saveSettings = (ModSettings) Activator.CreateInstance(saveSettingsType);

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
            ModSettings saveSettings;
            
            try
            {
                saveSettings = SaveSettings;
            }
            catch (Exception e)
            {
                LogError(e);

                return;
            }
            
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (saveSettings is ISerializationCallbackReceiver receiver)
                receiver.OnBeforeSerialize();

            string name = GetType().Name;
            
            Log("Adding Settings to Save file");
            
            if (data.modData == null)
            {
                data.modData = new ModSettingsDictionary();
            }

            data.modData[name] = saveSettings;
        }
    }
}