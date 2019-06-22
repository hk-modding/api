using System;

namespace Modding
{
    /// <summary>
    ///     Marks a method for subscription to an event in <see cref="ModHooks" />
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SubscribeEventAttribute : Attribute
    {
        internal string HookName { get; }
        internal Type ModType { get; }

        /// <summary>
        ///     Marks a method for subscription to an event in <see cref="ModHooks" />
        /// </summary>
        /// <param name="hookName">The name of the event to subscribe to</param>
        /// <param name="modType">
        ///     The parent <see cref="Mod" /> of this event. If this is an <see cref="ITogglableMod" />, the
        ///     event will be automatically enabled/disabled.
        /// </param>
        public SubscribeEventAttribute(string hookName, Type modType = null)
        {
            HookName = hookName;
            ModType = modType;
        }
    }
}