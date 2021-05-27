using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Modding.Patches;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using MonoMod.Utils;
using System.Linq;
using Newtonsoft.Json.Linq;

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

        private readonly Type globalSettingsType = null;
        private readonly FastReflectionDelegate onLoadGlobalSettings;
        private readonly FastReflectionDelegate onSaveGlobalSettings;
        private readonly Type saveSettingsType = null;
        private readonly FastReflectionDelegate onLoadSaveSettings;
        private readonly FastReflectionDelegate onSaveSaveSettings;

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

            if (this.GetType().GetInterfaces().Where(
                x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(GlobalSettings<>)
            ).FirstOrDefault() is Type globalType)
            {
                this.globalSettingsType = globalType.GetGenericArguments()[0];
                foreach (var mi in globalType.GetMethods())
                {
                    switch (mi.Name)
                    {
                        case nameof(GlobalSettings<object>.OnLoadGlobal):
                            this.onLoadGlobalSettings = mi.GetFastDelegate();
                            break;
                        case nameof(GlobalSettings<object>.OnSaveGlobal):
                            this.onSaveGlobalSettings = mi.GetFastDelegate();
                            break;
                    }
                }
            }
            if (this.GetType().GetInterfaces().Where(
                x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(LocalSettings<>)
            ).FirstOrDefault() is Type saveType)
            {
                this.saveSettingsType = saveType.GetGenericArguments()[0];
                foreach (var mi in saveType.GetMethods())
                {
                    switch (mi.Name)
                    {
                        case nameof(LocalSettings<object>.OnLoadLocal):
                            this.onLoadSaveSettings = mi.GetFastDelegate();
                            break;
                        case nameof(LocalSettings<object>.OnSaveLocal):
                            this.onSaveSaveSettings = mi.GetFastDelegate();
                            break;
                    }
                }
            }

            Name = name;

            Log("Initializing");

            _globalSettingsPath ??= Path.Combine(Application.persistentDataPath, $"{GetType().Name}.GlobalSettings.json");

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
            ModHooks.ApplicationQuitHook += SaveGlobalSettings;
            ModHooks.SaveLocalSettings += SaveLocalSettings;
            ModHooks.LoadLocalSettings += LoadLocalSettings;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChanged;
        }

        private void SceneChanged(Scene arg0, Scene arg1)
        {
            if (arg1.name != Constants.MENU_SCENE) return;

            if (this.saveSettingsType is Type saveType)
            {
                this.onLoadSaveSettings(this, Activator.CreateInstance(saveType));
            }
        }

        private void LoadGlobalSettings()
        {
            try
            {
                // test to see if we can load global settings from this mod
                if (this.globalSettingsType is Type saveType)
                {
                    if (!File.Exists(_globalSettingsPath))
                        return;
                    Log("Loading Global Settings");
                    using FileStream fileStream = File.OpenRead(_globalSettingsPath);
                    using var reader = new StreamReader(fileStream);
                    string json = reader.ReadToEnd();

                    var obj = JsonConvert.DeserializeObject(
                        json,
                        saveType,
                        new JsonSerializerSettings
                        {
                            ContractResolver = ShouldSerializeContractResolver.Instance,
                            TypeNameHandling = TypeNameHandling.Auto,
                            ObjectCreationHandling = ObjectCreationHandling.Replace,
                            Converters = JsonConverterTypes.ConverterTypes
                        }
                    );
                    this.onLoadGlobalSettings(this, obj);
                }
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
            try
            {
                if (this.globalSettingsType is Type saveType)
                {
                    Log("Saving Global Settings");
                    var obj = this.onSaveGlobalSettings(this);
                    if (obj is null)
                        return;
                    if (File.Exists(_globalSettingsPath + ".bak")) File.Delete(_globalSettingsPath + ".bak");
                    if (File.Exists(_globalSettingsPath)) File.Move(_globalSettingsPath, _globalSettingsPath + ".bak");
                    using FileStream fileStream = File.Create(_globalSettingsPath);
                    using var writer = new StreamWriter(fileStream);
                    writer.Write(
                        JsonConvert.SerializeObject
                        (
                            obj,
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
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        private void LoadLocalSettings(ModSavegameData data)
        {
            try
            {
                if (this.saveSettingsType is Type saveType)
                {
                    if (data.modData.TryGetValue(this.GetName(), out var obj))
                    {
                        this.onLoadSaveSettings(
                            this,
                            obj.ToObject(
                                saveType,
                                JsonSerializer.Create(
                                    new JsonSerializerSettings
                                    {
                                        ContractResolver = ShouldSerializeContractResolver.Instance,
                                        TypeNameHandling = TypeNameHandling.Auto,
                                        ObjectCreationHandling = ObjectCreationHandling.Replace,
                                        Converters = JsonConverterTypes.ConverterTypes
                                    }
                                )
                            )
                        );
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        private void SaveLocalSettings(ModSavegameData data)
        {
            try
            {
                if (this.saveSettingsType is Type saveType)
                {
                    var settings = this.onSaveSaveSettings(this);
                    switch (settings)
                    {
                        // No point in serializing nothing.
                        case null:
                            return;
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        case ISerializationCallbackReceiver receiver:
                            receiver.OnBeforeSerialize();
                            break;
                    }
                    data.modData[this.GetName()] = JToken.FromObject(
                        settings,
                        JsonSerializer.Create(
                            new JsonSerializerSettings
                            {
                                ContractResolver = ShouldSerializeContractResolver.Instance,
                                TypeNameHandling = TypeNameHandling.Auto,
                                Converters = JsonConverterTypes.ConverterTypes
                            }
                        )
                    );
                }

            }
            catch (Exception e)
            {
                LogError(e);
            }
        }
    }
}