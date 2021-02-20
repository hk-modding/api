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
        protected Mod() : this(null) { }

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
                _globalSettingsPath = Application.persistentDataPath + ModHooks.PathSeperator + GetType().Name + ".GlobalSettings.json";
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
        public virtual void Initialize() { }

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
            if (!File.Exists(_globalSettingsPath))
                return;
            
            Log("Loading Global Settings");

            using FileStream fileStream = File.OpenRead(_globalSettingsPath);

            using var reader = new StreamReader(fileStream);
            
            string json = reader.ReadToEnd();

            try
            {
                Type settingsType = GlobalSettings?.GetType();

                if (settingsType == null)
                    return;

                ModSettings settings;

                try
                {
                    settings = JsonConvert.DeserializeObject(
                        json,
                        settingsType,
                        new JsonSerializerSettings
                        {
                            ContractResolver = ShouldSerializeContractResolver.Instance,
                            TypeNameHandling = TypeNameHandling.Auto,
                            ObjectCreationHandling = ObjectCreationHandling.Replace,
                            Converters = JsonConverterTypes.ConverterTypes
                        }
                    ) as ModSettings;
                }
                catch (Exception e)
                {
                    LogError("Failed to load settings using Json.Net, falling back.");
                    LogError(e);

                    settings = JsonUtility.FromJson(json, settingsType) as ModSettings;
                }

                // ReSharper disable once SuspiciousTypeConversion.Global
                if (settings is ISerializationCallbackReceiver receiver)
                {
                    receiver.OnAfterDeserialize();
                }

                GlobalSettings = settings;
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        /// <summary>
        /// Save global settings to saves folder.
        /// </summary>
        protected void SaveGlobalSettings()
        {
            ModSettings settings = GlobalSettings;

            if (settings is null)
                return;

            Log("Saving Global Settings");

            if (File.Exists(_globalSettingsPath + ".bak"))
            {
                File.Delete(_globalSettingsPath + ".bak");
            }

            if (File.Exists(_globalSettingsPath))
            {
                File.Move(_globalSettingsPath, _globalSettingsPath + ".bak");
            }

            // ReSharper disable once SuspiciousTypeConversion.Global
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

            using FileStream fileStream = File.Create(_globalSettingsPath);

            using var writer = new StreamWriter(fileStream);

            try
            {
                writer.Write
                (
                    JsonConvert.SerializeObject
                    (
                        settings,
                        Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            ContractResolver = ShouldSerializeContractResolver.Instance,
                            TypeNameHandling = TypeNameHandling.Auto,
                            Converters = JsonConverterTypes.ConverterTypes
                        }
                    )
                );
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        // All my homies hate backwards compatability
        #pragma warning disable 618
        private bool DeprecatedSaveSettings(Patches.SaveGameData data)
        {
            try
            {
                string name = GetType().Name;

                if (data?.modData == null || !data.modData.ContainsKey(name))
                    return false;

                if (SaveSettings == null)
                {
                    data.modData.Remove(name);
                    
                    return false;
                }

                ModSettings saveSettings = data.modData[name];
                
                data.modData.Remove(name);
                
                switch (saveSettings)
                {
                    case null:
                        return false;

                    case ISerializationCallbackReceiver receiver:
                        receiver.OnAfterDeserialize();
                        break;
                }

                SaveSettings = saveSettings;

                return true;
            }
            catch (Exception e)
            {
                LogError(e);

                return false;
            }
        }
        #pragma warning restore 618


        private void LoadSaveSettings(Patches.SaveGameData data)
        {
            if (DeprecatedSaveSettings(data))
                return;

            try
            {
                string name = GetType().Name;

                if (data?.PolymorphicModData == null || !data.PolymorphicModData.TryGetValue(name, out string settings))
                    return;

                if (SaveSettings == null)
                    return;

                if (string.IsNullOrEmpty(settings))
                    return;

                var saveSettings = (ModSettings) JsonConvert.DeserializeObject
                (
                    settings,
                    SaveSettings.GetType(),
                    new JsonSerializerSettings
                    {
                        ContractResolver = ShouldSerializeContractResolver.Instance,
                        TypeNameHandling = TypeNameHandling.Auto,
                        ObjectCreationHandling = ObjectCreationHandling.Replace,
                        Converters = JsonConverterTypes.ConverterTypes
                    }
                );

                // ReSharper disable once SuspiciousTypeConversion.Global
                if (saveSettings is ISerializationCallbackReceiver receiver)
                    receiver.OnAfterDeserialize();

                Log("Loaded mod settings from save.");

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

            switch (saveSettings)
            {
                // No point in serializing nothing.
                case null:
                    return;

                case ISerializationCallbackReceiver receiver:
                    receiver.OnBeforeSerialize();
                    break;
            }

            string name = GetType().Name;

            Log("Adding settings to save file");

            if (data.PolymorphicModData == null)
                data.PolymorphicModData = new Dictionary<string, string>();

            /*
             * This looks kinda dumb because it kinda is.
             *
             * We do this so if a mod using settings is uninstalled, settings are preseved anyways.
             * Keeping the raw settings isn't viable as we don't have a type with all the fields to load into.
             */
            data.PolymorphicModData[name] = JsonConvert.SerializeObject
            (
                saveSettings,
                saveSettings.GetType(),
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver = ShouldSerializeContractResolver.Instance,
                    TypeNameHandling = TypeNameHandling.Auto,
                    Converters = JsonConverterTypes.ConverterTypes
                }
            );
        }
    }
}