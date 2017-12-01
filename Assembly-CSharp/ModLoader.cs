using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
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
			foreach (string text2 in Directory.GetFiles("hollow_knight_Data\\Managed\\Mods", "*.dll"))
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
							LoadedMods.Add((Mod)mod);
						    if (mod == null) continue;
						    mod.Initialize();

                            ModHooks.Instance.LoadedModsWithVersions.Add(type.Name, mod.GetVersion());
						    ModHooks.Instance.LoadedMods.Add(type.Name);

						    text = string.Concat(text, type.Name, ": ", mod.GetVersion(), mod.IsCurrent() ? "" : " - New Version Available!", "\n");
						}
						else if (!type.IsGenericType && type.IsClass && type.IsSubclassOf(typeof(Mod)))
						{
							Logger.LogDebug("[API] - Trying to instantiate mod: " + type);
							Mod mod2 = type.GetConstructor(new Type[0])?.Invoke(new object[0]) as Mod;
							LoadedMods.Add(mod2);
						    if (mod2 == null) continue;
						    mod2.Initialize();

                            ModHooks.Instance.LoadedModsWithVersions.Add(type.Name, mod2.GetVersion());
						    ModHooks.Instance.LoadedMods.Add(type.Name);

						    text = string.Concat(text, type.Name, ": ", mod2.GetVersion(), mod2.IsCurrent() ? "" : " - New Version Available!", "\n");
						}
					}
				}
				catch (Exception ex)
				{
					Logger.LogError("[API] - Error: " + ex);
				}
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
