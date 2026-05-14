using System;
using System.Reflection;
using JetBrains.Annotations;
using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;

namespace Modding.Patches.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// MonoMod attribute for patching a method directly with IL
    /// </summary>
    [UsedImplicitly]
    [MonoModCustomAttribute("RawIlPatch")]
    public class RawIlPatchAttribute : Attribute
    {
        /// <inheritdoc />
        /// <summary>
        /// Patches a method directly with IL
        /// </summary>
        /// <param name="type">Type full name that does the IL patch</param>
        /// <param name="method">Method name that does the IL patch</param>
        public RawIlPatchAttribute(string type, string method) { }
    }
}

namespace MonoMod
{
    public static partial class MonoModRules
    {
        /// <summary>
        /// Remove op 
        /// </summary>
        /// <param name="method">Method to be patched</param>
        /// <param name="attrib">Attribute</param>
        [UsedImplicitly]
        public static void RawIlPatch(MethodDefinition method, CustomAttribute attrib)
        {
            var context = new ILContext(method);

            string patcherTypeName = (string)attrib.ConstructorArguments[0].Value;
            string patcherMethodName = (string)attrib.ConstructorArguments[1].Value;

            Type patcherType = Type.GetType(patcherTypeName);

            if (patcherType is null)
                throw new InvalidOperationException("Couldn't find patcher type!");

            MethodBase patcherMethod = patcherType?.GetMethod(patcherMethodName, AllBindingFlags/*, null, [typeof(ILContext)], null*/);

            if (patcherMethod is null)
                throw new InvalidOperationException("Couldn't find patcher method!");

            patcherMethod.Invoke(null, new[] { context });
        }
    }
}