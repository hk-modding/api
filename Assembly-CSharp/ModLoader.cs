using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Modding
{
	internal static class ModLoader
	{
		public static void LoadMods()
		{
			if (Loaded)
			{
				return;
			}
			ModHooks.ModLog("Trying to load mods");
			string text = "";
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
		public static bool Loaded;
		public static bool Debug;
		public static List<Mod> LoadedMods = new List<Mod>();
	}
}
