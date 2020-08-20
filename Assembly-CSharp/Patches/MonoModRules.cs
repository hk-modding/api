using System;
using JetBrains.Annotations;
using Mono.Cecil;
using MonoMod.InlineRT;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable once CheckNamespace

namespace MonoMod
{
    /// <summary>
    /// Class for controlling some of the adjustments needed for monomod transformations
    /// </summary>
    [UsedImplicitly]
    public static partial class MonoModRules
    {
        static MonoModRules()
        {
            MonoModRule.Modder.ShouldCleanupAttrib = IsUselessAttrib;
        }

        /// <summary>
        /// Supresses MonoMod attributes based on varying rules.  At the very least, we are supressing the "Modding" namespace from getting MonoMod Attributes.
        /// </summary>
        /// <param name="holder"></param>
        /// <param name="attribType"></param>
        /// <returns></returns>
        public static bool IsUselessAttrib(ICustomAttributeProvider holder, TypeReference attribType)
        {
            // If the attribute isn't a MonoMod attribute, it's "useful."
            return attribType.Namespace.StartsWith("MonoMod") && attribType.Name.StartsWith("MonoMod") || attribType.Namespace.StartsWith("Modding.Patches");
        }

        /// <summary>
        /// Returns the get_Instance method for ModHooks.Instance
        /// </summary>
        /// <param name="method">method being worked on</param>
        /// <returns></returns>
        public static MethodDefinition ModHooksInstance(MethodDefinition method)
        {
            //   Console.WriteLine("ModHooksInstance");
            TypeDefinition modHookType = null;

            foreach (TypeDefinition type in method.Module.Types)
            {
                // ReSharper disable once InvertIf
                if (type.Name == "ModHooks")
                {
                    modHookType = type;
                    break;
                }
            }

            if (modHookType == null)
            {
                Console.WriteLine("WARNING - Couldn't find ModHooks type");
                return null;
            }

            foreach (PropertyDefinition property in modHookType.Properties)
            {
                if (property.Name == "Instance")
                {
                    return property.GetMethod;
                }
            }

            Console.WriteLine("WARNING - Couldn't find ModHooks Instance Property");
            return null;
        }
    }
}