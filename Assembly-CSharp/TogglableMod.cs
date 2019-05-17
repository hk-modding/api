namespace Modding
{
    /// <inheritdoc />
    /// <summary>
    ///     Interface which signifies that this mod can be loaded _and_ unloaded while in game. When re-initialized the mod
    ///     will be passed null rather than preloading again.
    /// </summary>
    public interface ITogglableMod : IMod
    {
        /// <summary>
        ///     Called when the Mod is disabled or unloaded.  Ensure you unhook any events that you hooked up in the Initialize
        ///     method.
        /// </summary>
        void Unload();
    }
}