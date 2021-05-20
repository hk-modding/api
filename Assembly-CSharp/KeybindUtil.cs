using System;
using System.Collections.Generic;
using InControl;

namespace Modding
{
    /// <summary>
    /// Utils for interacting with InControl keybindings.
    /// </summary>
    public static class KeybindUtil
    {
        /// <summary>
        /// Gets a <c>KeyOrMouseBinding</c> from a player action.
        /// </summary>
        /// <param name="action">The player action</param>
        /// <returns></returns>
        public static InputHandler.KeyOrMouseBinding GetKeyOrMouseBinding(this PlayerAction action)
        {
            foreach (var src in action.Bindings)
            {
                InputHandler.KeyOrMouseBinding ret = default;
                if (src is KeyBindingSource kbs && kbs.Control.IncludeCount == 1)
                {
                    ret = new InputHandler.KeyOrMouseBinding(
                        kbs.Control.GetInclude(0)
                    );
                }
                else if (src is MouseBindingSource mbs)
                {
                    ret = new InputHandler.KeyOrMouseBinding(mbs.Control);
                }
                if (!InputHandler.KeyOrMouseBinding.IsNone(ret))
                {
                    return ret;
                }
            }
            return default;
        }

        /// <summary>
        /// Adds a binding to the player action based on a <c>KeyOrMouseBinding</c>.
        /// </summary>
        /// <param name="action">The player action</param>
        /// <param name="binding">The binding</param>
        public static void AddKeyOrMouseBinding(this PlayerAction action, InputHandler.KeyOrMouseBinding binding)
        {
            if (binding.Key != Key.None)
            {
                action.AddBinding(new KeyBindingSource(new KeyCombo(binding.Key)));
            }
            else if (binding.Mouse != Mouse.None)
            {
                action.AddBinding(new MouseBindingSource(binding.Mouse));
            }
        }

        /// <summary>
        /// Parses a key or mouse binding from a string.
        /// </summary>
        /// <param name="src">The source string</param>
        /// <returns></returns>
        public static InputHandler.KeyOrMouseBinding? ParseBinding(string src)
        {
            if (Enum.TryParse<Key>(src, out var key))
            {
                return new InputHandler.KeyOrMouseBinding(key);
            }
            else if (Enum.TryParse<Mouse>(src, out var mouse))
            {
                return new InputHandler.KeyOrMouseBinding(mouse);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a controller button binding for a player action.
        /// </summary>
        /// <param name="ac">The player action.</param>
        /// <returns></returns>
        public static InputControlType GetControllerButtonBinding(this PlayerAction ac)
        {
            foreach (var src in ac.Bindings)
            {
                if (src is DeviceBindingSource dsrc)
                {
                    return dsrc.Control;
                }
            }
            return InputControlType.None;
        }
    }
}