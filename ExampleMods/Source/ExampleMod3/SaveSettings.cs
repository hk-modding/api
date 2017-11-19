using Modding;

namespace ExampleMod3
{
    /// <summary>
    /// Save Specific Settings
    /// </summary>
    public class SaveSettings : IModSettings
    {
        /// <summary>
        /// How many crits have we had this game?
        /// </summary>
        public int TotalCrits { get => GetInt(); set => SetInt(value); }
    }
}
