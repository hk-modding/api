using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Modding
{
	// Token: 0x020009BD RID: 2493
	internal static class ModLoader
	{
		// Token: 0x0600330E RID: 13070
		public static void LoadMods()
		{
			if (ModLoader.loaded)
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
						if (ModLoader.IsSubclassOfRawGeneric(typeof(Mod<>), type))
						{
							ModHooks.ModLog("Trying to instantiate mod<T>: " + type.ToString());
							IMod mod = Activator.CreateInstance(type) as IMod;
							ModLoader.loadedMods.Add((Mod)mod);
							ModHooks.Instance.loadedMods.Add(type.Name);
							mod.Initialize();
							text = string.Concat(new string[]
							{
								text,
								type.Name,
								": ",
								mod.GetVersion(),
								"\n"
							});
						}
						else if (!type.IsGenericType && type.IsClass && type.IsSubclassOf(typeof(Mod)))
						{
							ModHooks.ModLog("Trying to instantiate mod: " + type.ToString());
							Mod mod2 = type.GetConstructor(new Type[0]).Invoke(new object[0]) as Mod;
							ModLoader.loadedMods.Add(mod2);
							ModHooks.Instance.loadedMods.Add(type.Name);
							mod2.Initialize();
							text = string.Concat(new string[]
							{
								text,
								type.Name,
								": ",
								mod2.GetVersion(),
								"\n"
							});
						}
					}
				}
				catch (Exception ex)
				{
					ModHooks.ModLog("Error: " + ex.ToString());
				}
			}
			GameObject gameObject = new GameObject();
			gameObject.AddComponent<ModVersionDraw>().drawString = text;
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			ModLoader.loaded = true;
		}

		// Token: 0x0600330F RID: 13071
		static ModLoader()
		{
			ModLoader.loaded = false;
			ModLoader.debug = true;
		}

		// Token: 0x06003310 RID: 13072
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

		// Token: 0x04003B54 RID: 15188
		public static bool loaded;

		// Token: 0x04003B55 RID: 15189
		public static bool debug;

		// Token: 0x04003B56 RID: 15190
		public static List<Mod> loadedMods = new List<Mod>();
	}
}
