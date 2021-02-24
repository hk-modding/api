using System;

namespace Modding
{
    /// <summary>
    /// Declares what type of mod an assembly is. Used in determining load order.
    /// </summary>
    // ReSharper disable UnusedMember.Global
    public class AssemblyTypeAttribute : Attribute
    {
        /// <summary>
        /// Assembly type for a library.
        /// </summary>
        public const int LIBRARY = 0;
        /// <summary>
        /// Assembly type for a mod.
        /// </summary>
        public const int MOD = 1;
        /// <summary>
        /// Assembly type for an addon of a mod.
        /// </summary>
        public const int MOD_ADDON = 2;

        /// <summary>
        /// Create a new instance of <see cref="AssemblyTypeAttribute"/>
        /// </summary>
        /// <param name="type">The type of the target assembly. Use provided constants such as <see cref="MOD"/>.</param>
        // ReSharper disable once UnusedParameter.Local
        public AssemblyTypeAttribute(int type) { }
    }
}
