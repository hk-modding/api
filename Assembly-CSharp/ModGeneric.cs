using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Modding.Patches;
using Newtonsoft.Json;
using UnityEngine;

namespace Modding
{
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
            get => _settings ??= Activator.CreateInstance<TSaveSettings>();
            set => _settings = value;
        }
        
        private bool LoadSettingsDeprecated(Patches.SaveGameData data)
        {
            string name = GetType().Name;
            
            if (data?.modData == null || !data.modData.ContainsKey(name))
                return false;

            Settings = new TSaveSettings();
            Settings.SetSettings(data.modData[name]);

            data.modData.Remove(name);

            // ReSharper disable once SuspiciousTypeConversion.Global
            if (Settings is ISerializationCallbackReceiver callbackReceiver)
            {
                callbackReceiver.OnAfterDeserialize();
            }

            return true;
        }

        /// <summary>
        ///     Loads settings from a save file.
        /// </summary>
        /// <param name="data"></param>
        private void LoadSettings(Patches.SaveGameData data)
        {
            if (LoadSettingsDeprecated(data))
                return;
            
            string name = GetType().Name;
            
            if (data?.PolymorphicModData == null || !data.PolymorphicModData.ContainsKey(name))
                return;
            
            Log("Loading mod settings from save.");

            Settings = JsonConvert.DeserializeObject<TSaveSettings>(data.PolymorphicModData[name], new JsonSerializerSettings()
            {
                ContractResolver = ShouldSerializeContractResolver.Instance,
                TypeNameHandling = TypeNameHandling.Auto,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                Converters = JsonConverterTypes.ConverterTypes
            });

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
            
            Log("Adding settings to save file");
            
            if (data.PolymorphicModData == null)
                data.PolymorphicModData = new Dictionary<string, string>();
            
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (Settings is ISerializationCallbackReceiver receiver)
                receiver.OnBeforeSerialize();

            data.PolymorphicModData[name] = JsonConvert.SerializeObject
            (
                Settings,
                typeof(TSaveSettings),
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
            get => _globalSettings ??= Activator.CreateInstance<TGlobalSettings>();
            set => _globalSettings = value;
        }

        /// <summary>
        ///     Save GlobalSettings to disk. (backs up the current global settings if it exists)
        /// </summary>
        public new void SaveGlobalSettings()
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

            using FileStream fileStream = File.Create(_globalSettingsFilename);
            
            using var writer = new StreamWriter(fileStream);
            
            string json = JsonConvert.SerializeObject(GlobalSettings, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = ShouldSerializeContractResolver.Instance,
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = JsonConverterTypes.ConverterTypes
            });
            
            writer.Write(json);
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

            using FileStream fileStream = File.OpenRead(_globalSettingsFilename);
            using var reader = new StreamReader(fileStream);
            
            string json = reader.ReadToEnd();

            try
            {
                _globalSettings = JsonConvert.DeserializeObject<TGlobalSettings>
                (
                    json,
                    new JsonSerializerSettings
                    {
                        ContractResolver = ShouldSerializeContractResolver.Instance,
                        TypeNameHandling = TypeNameHandling.Auto,
                        ObjectCreationHandling = ObjectCreationHandling.Replace,
                        Converters = JsonConverterTypes.ConverterTypes
                    }
                );
            }
            catch (Exception)
            {
                Logger.LogWarn("Global settings unable to be deserialized by Json.NET, falling back.");
                
                _globalSettings = JsonUtility.FromJson<TGlobalSettings>(json);
            }
        }
    }
}
