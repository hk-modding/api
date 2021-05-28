using UnityEngine;
using Modding;
using System.Reflection;

namespace ExampleMods
{
    // Define a new mod named `CustomSaveData` that implements `GlobalSettings`
    // to signify that it will save some data to the saves folder.
    public class CustomSaveData : Mod, GlobalSettings<CustomGlobalSaveData>, LocalSettings<CustomLocalSaveData>
    {
        public static CustomSaveData loadedInstance { get; set; }

        // The global settings for this mod. The settings load will only occur once
        // so a static field should be used to prevent loss of data
        public static CustomGlobalSaveData globalSaveData { get; set; }
        // Implement the GlobalSettings interface.
        // This method gets called when the mod loader loads the global settings.
        public void OnLoadGlobal(CustomGlobalSaveData s) => CustomSaveData.globalSaveData = s;
        // This method gets called when the mod loader needs to save the global settings.
        public CustomGlobalSaveData OnSaveGlobal() => CustomSaveData.globalSaveData;

        // The save data specific to a certain savefile. This setting will be loaded each time a save is opened.
        public CustomLocalSaveData localSaveData { get; set; }
        // Implement the LocalSettings interface.
        // This method gets called when a save is loaded.
        public void OnLoadLocal(CustomLocalSaveData s) => this.localSaveData = s;
        // This method gets called when the player saves their file.
        public CustomLocalSaveData OnSaveLocal() => this.localSaveData;

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        // The global settings are loaded before this method is called.
        public override void Initialize()
        {
            if (CustomSaveData.loadedInstance != null) return;
            CustomSaveData.loadedInstance = this;
            // Hook into the savegame load hook (which will occur after OnLoadLocal is called).
            ModHooks.SavegameLoadHook += slot =>
            {
                this.localSaveData.loaded += 1;
                CustomSaveData.globalSaveData.savesLoaded += 1;
                this.Log($"Save {slot} has been loaded {this.localSaveData.loaded} times!");
                this.Log($"Saves have been loaded a total of {CustomSaveData.globalSaveData.savesLoaded} times!");
            };
        }
    }

    // These do not have to be classes, the mod loader can serialize and deserialize value types as well.
    // The global data to store that is transient between saves.
    public class CustomGlobalSaveData
    {
        // The number of times the player has loaded into a save.
        public int savesLoaded;
    }

    // The local data to store that is specific to saves.
    public class CustomLocalSaveData
    {
        // The number of times the player has loaded into this save.
        public int loaded;
    }
}
