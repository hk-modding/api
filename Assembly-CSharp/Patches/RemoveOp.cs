using System;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;

namespace Modding.Patches
{
    /// <inheritdoc />
    /// <summary>
    /// MonoMod attribute for removing op
    /// </summary>
    [UsedImplicitly]
    [MonoModCustomAttribute("RemoveOp")]
    public class RemoveOp : Attribute
    {
        /// <inheritdoc />
        /// <summary>
        /// Replace il op at index ind with nop.
        /// </summary>
        /// <param name="ind">Index of op</param>
        public RemoveOp(int ind) {}
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
        /// <param name="attrib">Attribute RemoveOp</param>
        [UsedImplicitly]
        public static void RemoveOp(MethodDefinition method, CustomAttribute attrib)
        {
            int ind = (int) attrib.ConstructorArguments[0].Value;

            method.Body.Instructions.RemoveAt(ind);
        }
    }
}