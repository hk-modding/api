namespace Modding
{
    /// <inheritdoc />
    /// <summary>
    /// Interface which signifies that this mod can be loaded _and_ unloaded while in game.  Implementing this inerface requires that properly handle tracking every hook you add, game state that you change, so that you can disable it all.
    /// </summary>
    public interface ITogglableMod : IMod
    {
        /// <summary>
        /// Called when the Mod is disabled or unloaded.  Ensure you unhook any events that you hooked up in the Initialize method.
        /// </summary>
        void Unload();
    }
}
