using GlobalEnums;
using Modding;
using System.Reflection;

namespace ExampleMods
{
    // Define a new mod named `SimpleHooks` that implements `ITogglableMod`
    // to signify that it can be toggled on or off.
    public class SimpleHooks : Mod, ITogglableMod
    {
        // Store the currently loaded instance of the mod.
        public static SimpleHooks LoadedInstance { get; set; }

        // Fields and properties to store in the mod instance itself
        public int HitCount { get; set; }

        // Override the `GetVersion` method to get the assembly version.
        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        // Code that should run on mod initialization.
        //
        // If your mod implements `ITogglableMod`, this method will be run on loaded the mod up again
        // so make sure to write it in a way that it will not fail if ran multiple times.
        public override void Initialize()
        {
            // Check if there is already a loaded mod instance.
            if (SimpleHooks.LoadedInstance != null) return;
            // Set the mod instance
            SimpleHooks.LoadedInstance = this;
            // Log some message to `ModLog.txt` with info level logging.
            this.Log("Hello Hallownest!");
            // Hook the `LogAttack` method onto the player attack hook.
            ModHooks.AttackHook += this.LogAttack;
        }

        // Code that should be run when the mod is disabled.
        public void Unload()
        {
            // Unhook the methods previously registered so no exceptions will happen.
            ModHooks.AttackHook -= this.LogAttack;
            // "Destroy" the loaded instance of the mod.
            SimpleHooks.LoadedInstance = null;
        }

        // Method to hook onto the player attack hook to log hits.
        private void LogAttack(AttackDirection _)
        {
            this.HitCount += 1;
            this.Log($"You have hit an enemy {this.HitCount} times!");
        }
    }
}
