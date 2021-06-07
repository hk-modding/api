using UnityEngine;
using Modding;
using System.Reflection;

namespace ExampleMods
{
    // Define a new mod named `CustomSaveData` that implements `GlobalSettings`
    // to signify that it will save some data to the saves folder.
    public class CustomSaveData : Mod, IGlobalSettings<CustomGlobalSaveData>, ILocalSettings<CustomLocalSaveData>
    {
        public static CustomSaveData LoadedInstance { get; set; }

        // The global settings for this mod. The settings load will only occur once.
        // so a static field should be used to prevent loss of data.
        // If this mod has not been loaded yet, `OnLoadGlobal` will never be called,
        // so a default value must be provided.
        public static CustomGlobalSaveData GlobalSaveData { get; set; } = new CustomGlobalSaveData();
        // Implement the GlobalSettings interface.
        // This method gets called when the mod loader loads the global settings.
        public void OnLoadGlobal(CustomGlobalSaveData s) => CustomSaveData.GlobalSaveData = s;
        // This method gets called when the mod loader needs to save the global settings.
        public CustomGlobalSaveData OnSaveGlobal() => CustomSaveData.GlobalSaveData;

        // The save data specific to a certain savefile. This setting will be loaded each time a save is opened.
        // If this mod has not been loaded yet on a save, `OnLoadLocal` will never be called,
        // so a default value must be provided.
        public CustomLocalSaveData LocalSaveData { get; set; } = new CustomLocalSaveData();
        // Implement the LocalSettings interface.
        // This method gets called when a save is loaded.
        public void OnLoadLocal(CustomLocalSaveData s) => this.LocalSaveData = s;
        // This method gets called when the player saves their file.
        public CustomLocalSaveData OnSaveLocal() => this.LocalSaveData;

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        // The global settings are loaded before this method is called.
        public override void Initialize()
        {
            if (CustomSaveData.LoadedInstance != null) return;
            CustomSaveData.LoadedInstance = this;
            // Hook into the savegame load hook (which will occur after OnLoadLocal is called).
            ModHooks.SavegameLoadHook += slot =>
            {
                this.LocalSaveData.Loaded += 1;
                CustomSaveData.GlobalSaveData.SavesLoaded += 1;
                this.Log($"Save {slot} has been loaded {this.LocalSaveData.Loaded} times!");
                this.Log($"Saves have been loaded a total of {CustomSaveData.GlobalSaveData.SavesLoaded} times!");
            };
        }
    }

    // These do not have to be classes, the mod loader can serialize and deserialize value types as well.
    // The global data to store that is transient between saves.
    public class CustomGlobalSaveData
    {
        // The number of times the player has loaded into a save.
        public int SavesLoaded;
    }

    // The local data to store that is specific to saves.
    public class CustomLocalSaveData
    {
        // The number of times the player has loaded into this save.
        public int Loaded;
    }
}
