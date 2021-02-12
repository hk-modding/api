using System;

namespace Modding
{
    /// <summary>
    /// Declares what type of mod an assembly is. Used in determining load order.
    /// </summary>
    public class AssemblyTypeAttribute : Attribute
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public const int LIBRARY = 0;
        public const int MOD = 1;
        public const int MOD_ADDON = 2;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Create a new instance of <see cref="AssemblyTypeAttribute"/>
        /// </summary>
        /// <param name="type">The type of the target assembly. Use provided constants such as <see cref="MOD"/>.</param>
        public AssemblyTypeAttribute(int type) { }
    }
}
