using System;
using System.Collections;
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
        ///     Constructs the mod, assigns the instance and sets the name.
        /// </summary>
        protected Mod(string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = GetType().Name;
            }

            if
            (
                this.GetType()
                    .GetInterfaces()
                    .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IGlobalSettings<>))
                    is Type globalType
            )
            {
                this.globalSettingsType = globalType.GetGenericArguments()[0];
                foreach (var mi in globalType.GetMethods())
                {
                    switch (mi.Name)
                    {
                        case nameof(IGlobalSettings<object>.OnLoadGlobal):
                            this.onLoadGlobalSettings = mi.GetFastDelegate();
                            break;
                        case nameof(IGlobalSettings<object>.OnSaveGlobal):
                            this.onSaveGlobalSettings = mi.GetFastDelegate();
                            break;
                    }
                }
            }

            if
            (
                this.GetType()
                    .GetInterfaces()
                    .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ILocalSettings<>))
                    is Type saveType
            )
            {
                this.saveSettingsType = saveType.GetGenericArguments()[0];

                foreach (var mi in saveType.GetMethods())
                {
                    switch (mi.Name)
                    {
                        case nameof(ILocalSettings<object>.OnLoadLocal):
                            this.onLoadSaveSettings = mi.GetFastDelegate();
                            break;
                        case nameof(ILocalSettings<object>.OnSaveLocal):
                            this.onSaveSaveSettings = mi.GetFastDelegate();
                            break;
                    }
                }
            }

            Name = name;

            Log("Initializing");

            _globalSettingsPath ??= GetGlobalSettingsPath();

            LoadGlobalSettings();
            HookSaveMethods();
        }

        private string GetGlobalSettingsPath()
        {
            string globalSettingsFileName = $"{GetType().Name}.GlobalSettings.json";

            string location = GetType().Assembly.Location;
            string directory = Path.GetDirectoryName(location);
            string globalSettingsOverride = Path.Combine(directory, globalSettingsFileName);

            if (File.Exists(globalSettingsOverride))
            {
                Log("Overriding Global Settings path with Mod directory");
                return globalSettingsOverride;
            }

            return Path.Combine(Application.persistentDataPath, globalSettingsFileName);
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

        /// <summary>
        /// A list of requested scenes to be preloaded and actions to execute on loading of those scenes
        /// </summary>
        /// <returns>List of tuples containg scene names and the respective actions.</returns>
        public virtual (string, Func<IEnumerator>)[] PreloadSceneHooks() => Array.Empty<(string, Func<IEnumerator>)>();

        /// <summary>
        /// This function will be invoked on each gameObject preloaded through the <see cref="GetPreloadNames"/> system.
        /// </summary>
        /// <param name="go">The preloaded gameObject.</param>
        /// <param name="sceneName">The scene the gameObject was preloaded from.</param>
        /// <param name="goName">The path to the preloaded gameObject.</param>
        public virtual void InvokeOnGameObjectPreloaded(GameObject go, string sceneName, string goName) { }

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

        /// <summary>
        ///     If this mod defines a menu via the <see cref="IMenuMod"/> or <see cref="ICustomMenuMod"/> interfaces, override this method to 
        ///     change the text of the button to jump to this mod's menu.
        /// </summary>
        /// <returns></returns>
        public virtual string GetMenuButtonText() => $"{GetName()} {Language.Language.Get("MAIN_OPTIONS", "MainMenu")}";

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

                    if (TryLoadGlobalSettings(_globalSettingsPath, saveType))
                        return;

                    LogError($"Null global settings passed to {GetName()}");

                    string globalSettingsBackup = _globalSettingsPath + ".bak";
                    if (!File.Exists(globalSettingsBackup))
                        return;

                    if (TryLoadGlobalSettings(globalSettingsBackup, saveType))
                    {
                        Log("Successfully loaded global settings from backup");
                        File.Delete(_globalSettingsPath);
                        File.Copy(globalSettingsBackup, _globalSettingsPath);
                    }
                    LogError("Failed to load global settings from backup");

                }
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        /// <summary>
        /// Try to load the global settings from the given path. Returns true if the global settings were successfully loaded.
        /// </summary>
        private bool TryLoadGlobalSettings(string path, Type saveType)
        {
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

            if (obj is null)
            {
                return false;
            }
            this.onLoadGlobalSettings(this, obj);
            return true;
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
                    writer.Write(JsonConvert.SerializeObject(
                        obj,
                        Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            ContractResolver = ShouldSerializeContractResolver.Instance,
                            TypeNameHandling = TypeNameHandling.Auto,
                            Converters = JsonConverterTypes.ConverterTypes
                        }
                    ));
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
                if (this.saveSettingsType is not Type saveType)
                    return;

                if (!data.modData.TryGetValue(this.GetName(), out var obj))
                    return;

                this.onLoadSaveSettings
                (
                    this,
                    obj.ToObject
                    (
                        saveType,
                        JsonSerializer.Create
                        (
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
            catch (Exception e)
            {
                LogError(e);
            }
        }

        private void SaveLocalSettings(ModSavegameData data)
        {
            try
            {
                if (this.saveSettingsType is not Type saveType)
                    return;

                var settings = this.onSaveSaveSettings(this);

                if (settings is null)
                    return;

                data.modData[this.GetName()] = JToken.FromObject
                (
                    settings,
                    JsonSerializer.Create
                    (
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
    }
}