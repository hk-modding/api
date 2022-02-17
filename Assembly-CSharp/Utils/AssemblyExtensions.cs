using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Modding.Utils
{
    /// <summary>
    /// Class containing extensions used by the Modding API for interacting with assemblies.
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Returns a collection containing the types in the provided assembly.
        /// If some types cannot be loaded (e.g. they derive from a type in an uninstalled mod),
        /// then only the successfully loaded types are returned.
        /// </summary>
        public static IEnumerable<Type> GetTypesSafely(this Assembly asm)
        {
            try
            {
                return asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(x => x is not null);
            }
        }
    }
}
