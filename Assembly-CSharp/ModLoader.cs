using System;
using System.Collections.Generic;
using System.IO;
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
			ModHooks.ModLog("Trying to load mods");
		    string text = "Modding API: " + ModHooks.Instance.ModVersion + "\n";
			foreach (string text2 in Directory.GetFiles("hollow_knight_Data\\Managed\\Mods"))
			{
				ModHooks.ModLog("Loading assembly: " + text2);
				try
				{
					foreach (Type type in Assembly.LoadFile(text2).GetExportedTypes())
					{
						if (IsSubclassOfRawGeneric(typeof(Mod<>), type))
						{
							ModHooks.ModLog("Trying to instantiate mod<T>: " + type);
							IMod mod = Activator.CreateInstance(type) as IMod;
							LoadedMods.Add((Mod)mod);
							ModHooks.Instance.LoadedMods.Add(type.Name);
						    if (mod == null) continue;

						    mod.Initialize();
						    text = string.Concat(text, type.Name, ": ", mod.GetVersion(), "\n");
						}
						else if (!type.IsGenericType && type.IsClass && type.IsSubclassOf(typeof(Mod)))
						{
							ModHooks.ModLog("Trying to instantiate mod: " + type);
							Mod mod2 = type.GetConstructor(new Type[0])?.Invoke(new object[0]) as Mod;
							LoadedMods.Add(mod2);
							ModHooks.Instance.LoadedMods.Add(type.Name);
						    if (mod2 == null) continue;

						    mod2.Initialize();
						    text = string.Concat(text, type.Name, ": ", mod2.GetVersion(), "\n");
						}
					}
				}
				catch (Exception ex)
				{
					ModHooks.ModLog("Error: " + ex);
				}
			}
			GameObject gameObject = new GameObject();
			gameObject.AddComponent<ModVersionDraw>().drawString = text;
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			Loaded = true;
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
