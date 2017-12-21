using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Modding
{
    /// <summary>
    /// Handles loading of mods.
    /// </summary>
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
		    string text = "Modding API: " + ModHooks.Instance.ModVersion + (ModHooks.Instance.IsCurrent ? "" : " - New Version Available!") + "\n";
		    string path = string.Empty;
		    if (SystemInfo.operatingSystem.Contains("Windows"))
		        path = Application.dataPath + "\\Managed\\Mods";
            else if (SystemInfo.operatingSystem.Contains("Windows"))
		        path = Application.dataPath + "/Managed/Mods/";
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
				}
			}

		    foreach (Mod mod in LoadedMods.OrderBy(x => x.LoadPriority()))
		    {
		        mod.Initialize();

		        ModHooks.Instance.LoadedModsWithVersions.Add(mod.GetType().Name, mod.GetVersion());
		        ModHooks.Instance.LoadedMods.Add(mod.GetType().Name);

		        text = string.Concat(text, mod.GetType().Name, ": ", mod.GetVersion(), mod.IsCurrent() ? "" : " - New Version Available!", "\n");
            }

			GameObject gameObject = new GameObject();
			gameObject.AddComponent<ModVersionDraw>().drawString = text;
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			Loaded = true;
		    ModHooks.Instance.SaveGlobalSettings();
		}

		static ModLoader()
		{
			Loaded = false;
			Debug = true;
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
		public static List<Mod> LoadedMods = new List<Mod>();

        
	    
    }
}
