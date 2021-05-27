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
        /// <returns></returns>
        public List<MenuEntry> GetMenuData();

        /// <summary>
        /// A struct representing a menu option.
        /// </summary>
        public struct MenuEntry
        {
            /// <summary>
            /// The name of the setting.
            /// </summary>
            public string name;
            /// <summary>
            /// The description of the setting. May be null.
            /// </summary>
            public string description;
            /// <summary>
            /// The values to display for the setting.
            /// </summary>
            public string[] values;
            /// <summary>
            /// A function to take the current value index and save it.
            /// </summary>
            public Action<int> saver;
            /// <summary>
            /// A function to get the saved data and convert it into a value index.
            /// </summary>
            public Func<int> loader;

            /// <summary>
            /// Creates a new menu entry.
            /// </summary>
            public MenuEntry(string name, string[] values, string description, Action<int> saver, Func<int> loader)
            {
                this.name = name;
                this.description = description;
                this.values = values;
                this.saver = saver;
                this.loader = loader;
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
        /// <returns></returns>
        public MenuScreen GetMenuScreen(MenuScreen modListMenu);
    }
}