using GlobalEnums;
using Modding;
using System.Reflection;

namespace ExampleMods
{
    // Define a new mod named `SimpleHooks` that implements `IToggleableMod`
    // to signify that it can be toggled on or off.
    public class SimpleHooks : Mod, ITogglableMod
    {
        // Store a the currently loaded instance of the mod.
        public static SimpleHooks loadedInstance { get; set; }

        // Fields and properties to store in the mod instance itself
        private int storedAttackDamage;
        public int hitCount { get; set; }

        // Override the `GetVersion` method to get the assembly version.
        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        // Code that should run on mod initialization.
        //
        // If your mod implements `IToggleableMod`, this method will be run on loaded the mod up again
        // so make sure to write it in a way that it will not fail if ran multiple times.
        public override void Initialize()
        {
            // Check if there is already a loaded mod instance.
            if (SimpleHooks.loadedInstance != null) return;
            // Set the mod instance
            SimpleHooks.loadedInstance = this;
            // Log some message to `ModLog.txt` with info level logging.
            this.Log("Hello Hallownest! Initializing critical hit example mod.");
            // Hook the `CriticalHit` method onto the player attack hook.
            ModHooks.AttackHook += this.CriticalHit;
            // Hook the `ResetCritDamage` method onto the after player attack hook.
            ModHooks.AfterAttackHook += this.ResetCritDamage;
        }

        // Code that should be run when the mod is disabled.
        public void Unload()
        {
            // Unhook the methods previously registered so no exceptions will happen.
            ModHooks.AttackHook -= this.CriticalHit;
            ModHooks.AfterAttackHook -= this.CriticalHit;
            // "Destroy" the loaded instance of the mod.
            SimpleHooks.loadedInstance = null;
        }

        // Method to Hook onto the player attack hook to handle adding critical hits.
        private void CriticalHit(AttackDirection _)
        {
            if (this.hitCount >= 4)
            {
                this.storedAttackDamage = PlayerData.instance.nailDamage;
                PlayerData.instance.nailDamage *= 2;
                this.hitCount = 0;
                this.Log($"Critical Hit! Nail damage increased to {PlayerData.instance.nailDamage}.");
            }
            this.hitCount += 1;
        }

        // Method to hook onto the after player attack hook to reset the nail damage.
        private void ResetCritDamage(AttackDirection _)
        {
            PlayerData.instance.nailDamage = this.storedAttackDamage;
        }
    }
}
