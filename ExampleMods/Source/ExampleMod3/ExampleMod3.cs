using System;
using System.Reflection;
using GlobalEnums;
using Modding;

namespace ExampleMod3
{
    /// <summary>
    /// The main mod class
    /// </summary>
    /// <remarks>This configuration has settings that are save specific and global (profile) too.</remarks>
    public class ExampleMod3 : Mod<SaveSettings, GlobalModSettings>
    {

        private int _hitCounter;

        private int _tempNailDamage;

        /// <summary>
        /// Fetches the Mod Version From AssemblyInfo.AssemblyVersion
        /// </summary>
        /// <returns>Mod's Version</returns>
        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// Represents this Mod's instance.
        /// </summary>
        internal static ExampleMod3 Instance;

        /// <summary>
        /// Called after the class has been constructed.
        /// </summary>
        public override void Initialize()
        {
            //Assign the Instance to the instantiated mod.
            Instance = this;

            Log("Initializing");
            
            //Here want to hook into the AfterAttackHook to do something at the end of the attack animation.
            ModHooks.Instance.AttackHook += OnAttack;
            ModHooks.Instance.AfterAttackHook += OnAfterAttack;
            ModHooks.Instance.ApplicationQuitHook += ApplicationQuitHook;
            Log("Initialized");
        }

        /// <summary>
        /// Checks to see if the mod is up to date from a github release, if a new release is in github, return false.  There will be a message in the UI noting that the mod is out of date.
        /// </summary>
        /// <remarks>This is an entirely optional function.  You can not override this method if you don't use github or want to use version checking.</remarks>
        /// <returns></returns>
        public override bool IsCurrent()
        {
            try
            {
                //This should be your repository's name.  
                GithubVersionHelper helper = new GithubVersionHelper("username/ExampleMod3");

                //This assumes you're using Semantic Versioning (ex: 1.2.3.4).  If you use another versioning system, you'll have to implement your own logic to determine if this is new.
                Version currentVersion = new Version(GetVersion());

                Version newVersion = new Version(helper.GetVersion());
                LogDebug($"Comparing Versions: {newVersion} > {currentVersion}");
                return newVersion.CompareTo(currentVersion) < 0;
            }
            catch (Exception ex)
            {
                LogError("Couldn't check version" + ex);
            }

            return true;
        }

        /// <summary>
        /// When the game closes, lets save the Global Settings (in case we changed them)
        /// </summary>
        private void ApplicationQuitHook()
        {
            SaveGlobalSettings();
        }

        /// <summary>
        /// Calculates Crits on attack
        /// </summary>
        /// <remarks>
        /// This checks if this is our 4th attack or not.  If it is, we're going to critical hit by doubling our nail damage for the attack.  We also store the previous nail damage so that we can revert it back after the attack is over.
        /// </remarks>
        /// <param name="dir"></param>
        public void OnAttack(AttackDirection dir)
        {
            LogDebug("Attacking");
            if (_hitCounter >= GlobalSettings.CritCounter)
            {
                Settings.TotalCrits++; // For fun, lets track the total number of crits we have done so far this game.  

                LogDebug("Critical hit!");

                _tempNailDamage = PlayerData.instance.nailDamage; //Store the current nail damage.

                LogDebug("Set _tempNailDamage to " + _tempNailDamage);

                PlayerData.instance.nailDamage = (int)Math.Round(_tempNailDamage * GlobalSettings.CritMultiplier); //Increase Nail Damage by the crit multiplier then round to the nearest int.

                _hitCounter = 0;// reset our hit counter

                return;
            }
            _hitCounter++; //Increase the hit counter
        }

        /// <summary>
        /// Reverts damage
        /// </summary>
        /// <remarks>After the attack is over, we need to reset the nail damage back to what it was.</remarks>
        /// <param name="dir"></param>
        private void OnAfterAttack(AttackDirection dir)
        {
            LogDebug("Attacked!");
            PlayerData.instance.nailDamage = _tempNailDamage; //Attacking is done, we need to set the nail damage back to what it was before we crit.
        }
    }

}
