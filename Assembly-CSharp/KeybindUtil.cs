using System;
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
            foreach (BindingSource src in action.Bindings)
            {
                InputHandler.KeyOrMouseBinding ret = src switch
                {
                    KeyBindingSource { Control.IncludeCount: 1 } kbs => new InputHandler.KeyOrMouseBinding(kbs.Control.GetInclude(0)),
                    MouseBindingSource mbs => new InputHandler.KeyOrMouseBinding(mbs.Control),
                    _ => default
                };
                
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

        
        /// <summary>
        /// Adds a controller button binding to the player action based on a <c>InputControlType</c>.
        /// </summary>
        /// <param name="action">The player action</param>
        /// <param name="binding">The binding</param>
        public static void AddInputControlType(this PlayerAction action, InputControlType binding)
        {
            if (binding != InputControlType.None)
            {
                action.AddBinding(new DeviceBindingSource(binding));
            }
        }

        /// <summary>
        /// Parses a InputControlType binding from a string.
        /// </summary>
        /// <param name="src">The source string</param>
        /// <returns></returns>
        public static InputControlType? ParseInputControlTypeBinding(string src)
        {
            if (Enum.TryParse<InputControlType>(src, out var key))
            {
                return key;
            }
            else
            {
                return null;
            }
        }
    }
}