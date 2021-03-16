using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;

namespace Modding.Patches
{
    /// <inheritdoc />
    /// <summary>
    /// MonoMod attribute for replacing a method call
    /// </summary>
    [MonoModCustomAttribute("ReplaceMethod")]
    [UsedImplicitly]
    internal class ReplaceMethodAttribute : Attribute
    {
        /// <inheritdoc />
        /// <summary>
        /// Replace method call with alternate method call
        /// </summary>
        public ReplaceMethodAttribute(string type1, string method1, string[] params1, string type2, string method2, string[] params2) { }
    }
}

namespace MonoMod
{
    public static partial class MonoModRules
    {
        private const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

        /// <summary>
        /// Replace method call with alternate method call
        /// </summary>
        /// <param name="method">Method to be patched</param>
        /// <param name="attrib">Attribute ReplaceWithNop</param>
        [UsedImplicitly]
        public static void ReplaceMethod(MethodDefinition method, CustomAttribute attrib)
        {
            MethodBase oldMethod = Type.GetType((string)attrib.ConstructorArguments[0].Value)?.GetMethod
            (
                (string)attrib.ConstructorArguments[1].Value,
                FLAGS,
                null,
                ((CustomAttributeArgument[])attrib.ConstructorArguments[2].Value)
                .Select(t => Type.GetType((string)t.Value)).ToArray(),
                null
            );

            if (oldMethod is null)
                throw new InvalidOperationException("Couldn't find old method!");

            MethodBase newMethod = Type.GetType((string)attrib.ConstructorArguments[3].Value)?.GetMethod
            (
                (string)attrib.ConstructorArguments[4].Value,
                FLAGS,
                null,
                ((CustomAttributeArgument[])attrib.ConstructorArguments[5].Value)
                .Select(t => Type.GetType((string)t.Value)).ToArray(),
                null
            );
            
            if (newMethod is null)
                throw new InvalidOperationException("Couldn't find new method!");

            var il = new ILCursor(new ILContext(method));

            while (il.TryGotoNext(i => i.MatchCallOrCallvirt(oldMethod)))
            {
                il.Remove();
                il.Emit(newMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, newMethod);
            }
        }
    }
}
