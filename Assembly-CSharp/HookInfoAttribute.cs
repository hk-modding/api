using System;
using JetBrains.Annotations;

namespace Modding
{
    /// <inheritdoc />
    /// <summary>
    ///     Attribute to describe hooks programatically.
    /// </summary>
    [PublicAPI]
    public class HookInfoAttribute : Attribute
    {
        /// <summary>
        ///     Hook Description
        /// </summary>
        public readonly string Description;

        /// <summary>
        ///     Location hook is placed in original code.
        /// </summary>
        public readonly string HookLocation;

        /// <inheritdoc />
        /// <summary>
        ///     Creates new Info Attribute
        /// </summary>
        /// <param name="desc">Description</param>
        /// <param name="hookLoc">Location hook is placed in original code.</param>
        public HookInfoAttribute(string desc, string hookLoc)
        {
            Description = desc;
            HookLocation = hookLoc;
        }
    }
}