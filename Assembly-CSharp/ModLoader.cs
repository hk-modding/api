using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace Modding
{
    /// <summary>
    /// Handles loading of mods.
    /// </summary>
    [SuppressMessage("ReSharper", "SuggestVarOrType_SimpleTypes")]
    [PublicAPI]
    internal static class ModLoader
    {
        /// <summary>
        /// Loads the mod by searching for assemblies in hollow_knight_Data\Managed\Mods\
        /// </summary>
        public static void LoadMods()
        {
            if (Loaded)
            {
                return;
            }

            Logger.Log("[API] - Trying to load mods");
            string path = string.Empty;
            if (SystemInfo.operatingSystem.Contains("Windows"))
                path = Application.dataPath + "\\Managed\\Mods";
            else if (SystemInfo.operatingSystem.Contains("Mac"))
                path = Application.dataPath + "/Resources/Data/Managed/Mods/";
            else if (SystemInfo.operatingSystem.Contains("Linux"))
                path = Application.dataPath + "/Managed/Mods";
            else
                Logger.LogWarn($"Operating system of {SystemInfo.operatingSystem} is not known.  Unable to load mods.");

            if (string.IsNullOrEmpty(path))
            {
                Loaded = true;
                return;
            }

            foreach (string text2 in Directory.GetFiles(path, "*.dll"))
            {
                Logger.LogDebug("[API] - Loading assembly: " + text2);
                try
                {
                    foreach (Type type in Assembly.LoadFile(text2).GetExportedTypes())
                    {
                        if (IsSubclassOfRawGeneric(typeof(Mod<>), type))
                        {
                            Logger.LogDebug("[API] - Trying to instantiate mod<T>: " + type);

                            IMod mod = Activator.CreateInstance(type) as IMod;
                            if (mod == null) continue;
                            LoadedMods.Add((Mod)mod);
                        }
                        else if (!type.IsGenericType && type.IsClass && type.IsSubclassOf(typeof(Mod)))
                        {
                            Logger.LogDebug("[API] - Trying to instantiate mod: " + type);
                            Mod mod2 = type.GetConstructor(new Type[0])?.Invoke(new object[0]) as Mod;
                            if (mod2 == null) continue;
                            LoadedMods.Add(mod2);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("[API] - Error: " + ex);
                    Errors.Add(string.Concat(text2, ": FAILED TO LOAD! Check ModLog.txt."));
                }
            }
            
            ModHooks.Instance.LoadGlobalSettings();
            
            foreach (IMod mod in LoadedMods.OrderBy(x => x.LoadPriority()))
            {
                try
                {
                    LoadMod(mod, false, false);
                }
                catch (Exception ex)
                {
                    Errors.Add(string.Concat(mod.GetName(), ": FAILED TO LOAD! Check ModLog.txt."));
                    Logger.LogError("[API] - Error: " + ex);
                }
            }

            //Clean out the ModEnabledSettings for any mods that don't exist.
            //Calling ToList means we are not working with the dictionary keys directly, preventing an out of sync error
            foreach (string modName in ModHooks.Instance.GlobalSettings.ModEnabledSettings.Keys.ToList())
            {
                if (LoadedMods.All(x => x.GetName() != modName))
                    ModHooks.Instance.GlobalSettings.ModEnabledSettings.Remove(modName);
            }
            
            // Get previously disabled mods and disable them.
            foreach (KeyValuePair<string, bool> modPair in ModHooks.Instance.GlobalSettings.ModEnabledSettings.Where(x => !x.Value))
            {
                IMod mod = LoadedMods.FirstOrDefault(x => x.GetName() == modPair.Key);
                if (!(mod is ITogglableMod togglable)) continue;
                togglable.Unload();
                Logger.LogDebug($"Mod {modPair.Key} was unloaded.");
            }
            

            GameObject gameObject = new GameObject();
            _draw = gameObject.AddComponent<ModVersionDraw>();
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            UpdateModText();
            Loaded = true;
            
            ModHooks.Instance.SaveGlobalSettings();
        }

        private static readonly List<string> Errors = new List<string>();

        static ModLoader()
        {
            Loaded = false;
            Debug = true;
        }

        private static ModVersionDraw _draw;

        private static void UpdateModText()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Modding API: " + ModHooks.Instance.ModVersion + (ModHooks.Instance.IsCurrent ? "" : " - New Version Available!") );
            foreach (string error in Errors)
            {
                builder.AppendLine(error);
            }

            // 56 you made me do this, I hope you're happy
            Dictionary<string, List<IMod>> modsByNamespace = new Dictionary<string, List<IMod>>();

            foreach (IMod mod in LoadedMods)
            {
                try
                {
                    if (!ModHooks.Instance.GlobalSettings.ModEnabledSettings[mod.GetName()])
                    {
                        continue;
                    }

                    if (!ModVersionsCache.ContainsKey(mod.GetName()))
                    {
                        ModVersionsCache.Add(mod.GetName(), mod.GetVersion() + (mod.IsCurrent() ? string.Empty : " - New Version Available!"));
                    }

                    string ns = mod.GetType().Namespace;

                    if (!modsByNamespace.TryGetValue(ns, out List<IMod> nsMods))
                    {
                        nsMods = new List<IMod>();
                        modsByNamespace.Add(ns, nsMods);
                    }

                    nsMods.Add(mod);
                }
                catch (Exception e)
                {
                    Logger.LogError($"[API] - Failed to obtain mod namespace:\n{e}");
                }
            }

            foreach (string ns in modsByNamespace.Keys)
            {
                try
                {
                    List<IMod> nsMods = modsByNamespace[ns];

                    if (nsMods == null || nsMods.Count == 0)
                    {
                        Logger.LogWarn("[API] - Namespace mod list empty, ignoring");
                    }
                    else if (nsMods.Count == 1)
                    {
                        builder.AppendLine($"{nsMods[0].GetName()} : {ModVersionsCache[nsMods[0].GetName()]}");
                    }
                    else
                    {
                        builder.Append($"{ns} : ");
                        for (int i = 0; i < nsMods.Count; i++)
                        {
                            builder.Append(nsMods[i].GetName() + (i == nsMods.Count - 1 ? Environment.NewLine : ", "));
                            if ((i + 1) % 4 == 0 && i < nsMods.Count - 1)
                            {
                                builder.Append(Environment.NewLine + "\t");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError($"[API] - Failed to append mod name text:\n{e}");
                }
            }

            _draw.drawString = builder.ToString();
        }

        internal static void LoadMod(IMod mod, bool updateModText, bool changeSettings = true)
        {
            if(changeSettings || !ModHooks.Instance.GlobalSettings.ModEnabledSettings.ContainsKey(mod.GetName()))
                ModHooks.Instance.GlobalSettings.ModEnabledSettings[mod.GetName()] = true;

            mod.Initialize();


            if (!ModHooks.Instance.LoadedModsWithVersions.ContainsKey(mod.GetType().Name))
                ModHooks.Instance.LoadedModsWithVersions.Add(mod.GetType().Name, mod.GetVersion());
            else
                ModHooks.Instance.LoadedModsWithVersions[mod.GetType().Name] = mod.GetVersion();

            if (ModHooks.Instance.LoadedMods.All(x => x != mod.GetType().Name))
                ModHooks.Instance.LoadedMods.Add(mod.GetType().Name);

            if (updateModText)
                UpdateModText();
        }

        internal static void UnloadMod(ITogglableMod mod) 
        {
            try
            {
                ModHooks.Instance.GlobalSettings.ModEnabledSettings[mod.GetName()] = false;
                ModHooks.Instance.LoadedModsWithVersions.Remove(mod.GetType().Name);
                ModHooks.Instance.LoadedMods.Remove(mod.GetType().Name);

                mod.Unload();
            }
            catch (Exception ex)
            {
                Logger.LogError($"[API] - Failed to unload Mod - {mod.GetName()} - {Environment.NewLine} - {ex} ");
            }

            UpdateModText();
        }

        /// <summary>
        /// Checks to see if a class is a subclass of a generic class.
        /// </summary>
        /// <param name="generic">Generic to compare against.</param>
        /// <param name="toCheck">Type to check</param>
        /// <returns></returns>
        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                Type type = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == type)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
        /// <summary>
        /// Checks if the mod loads are done.
        /// </summary>
        public static bool Loaded;

        /// <summary>
        /// Is Debug Enabled
        /// </summary>
        public static bool Debug;

        /// <summary>
        /// List of loaded mods.
        /// </summary>
        public static List<IMod> LoadedMods = new List<IMod>();

        private static readonly Dictionary<string, string> ModVersionsCache = new Dictionary<string, string>();
        
    }
}
