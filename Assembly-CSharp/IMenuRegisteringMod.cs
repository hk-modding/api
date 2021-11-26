using System;
using System.Collections.Generic;

namespace Modding
{
    /// <summary>
    /// Interface which signifies that this mod will register a menu in the mod list.
    /// </summary>
    public interface IMenuMod : IMod
    {
        /// <summary>
        /// Gets the data for the custom menu.
        /// </summary>
        /// <remarks>
        /// The implementor of this method will need to add the `toggleButtonEntry`
        /// if they want it to appear in their menu. The mod loader will not add it automatically.
        /// </remarks>
        /// <param name="toggleButtonEntry">
        /// An entry representing the mod toggle button.
        /// This will be null if `ToggleButtonInsideMenu` is false or the mod is not an `ITogglableMod`.
        /// </param>
        /// <returns></returns>
        public List<MenuEntry> GetMenuData(MenuEntry? toggleButtonEntry);

        /// <summary>
        /// Will the toggle button (for an ITogglableMod) be inside the returned menu screen.
        /// If this is set, an `ITogglableMod` will not create the toggle entry in the main menu.
        /// </summary>
        public bool ToggleButtonInsideMenu { get; }

        /// <summary>
        /// A struct representing a menu option.
        /// </summary>
        public struct MenuEntry
        {
            /// <summary>
            /// The name of the setting.
            /// </summary>
            public string Name;
            /// <summary>
            /// The description of the setting. May be null.
            /// </summary>
            public string Description;
            /// <summary>
            /// The values to display for the setting.
            /// </summary>
            public string[] Values;
            /// <summary>
            /// A function to take the current value index and save it.
            /// </summary>
            public Action<int> Saver;
            /// <summary>
            /// A function to get the saved data and convert it into a value index.
            /// </summary>
            public Func<int> Loader;

            /// <summary>
            /// Creates a new menu entry.
            /// </summary>
            public MenuEntry(string name, string[] values, string description, Action<int> saver, Func<int> loader)
            {
                this.Name = name;
                this.Description = description;
                this.Values = values;
                this.Saver = saver;
                this.Loader = loader;
            }
        }
    }

    /// <summary>
    /// Interface which signifies that this mod will register a custom menu in the mod list.
    /// </summary>
    public interface ICustomMenuMod : IMod
    {
        /// <summary>
        /// Gets the built menu screen.
        /// </summary>
        /// <param name="modListMenu">The menu screen that is the mod list menu.</param>
        /// <param name="toggleDelegates">
        /// The delegates used for toggling the mod.
        /// This will be null if `ToggleButtonInsideMenu` is false or the mod is not an `ITogglableMod`.
        /// </param>
        /// <remarks>
        /// The implementor of this method will need to add an option using `toggleDelegates`
        /// if they want it to appear in their menu. The mod loader will not add it automatically.
        /// </remarks>
        /// <returns></returns>
        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates);

        /// <summary>
        /// Will the toggle button (for an ITogglableMod) be inside the returned menu screen.
        /// If this is set, an `ITogglableMod` will not create the toggle entry in the main menu.
        /// </summary>
        public bool ToggleButtonInsideMenu { get; }
    }

    /// <summary>
    /// Delegates to load an unload a mod through the menu.
    /// </summary>
    public struct ModToggleDelegates
    {
        /// <summary>
        /// Sets the mod to an enabled or disabled state. This will not be updated until menu is hidden
        /// </summary>
        public Action<bool> SetModEnabled;
        /// <summary>
        /// Gets if the mod is enabled or disabled. This will not be updated until menu is hidden
        /// </summary>
        public Func<bool> GetModEnabled;
        /// <summary>
        /// Left in for backwards compatibility.
        /// </summary>
        [Obsolete("No longer needed to explicitly call this. Using 'SetModEnabled' is enough to toggle the state of the mod")]
        public Action ApplyChange;
    }
}
