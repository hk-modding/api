using System;
using System.Linq;
using System.Reflection;
using HutongGames.PlayMaker;
using JetBrains.Annotations;
using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;

namespace Modding.Patches
{
    /// <inheritdoc />
    /// <summary>
    /// MonoMod attribute for removing method call
    /// </summary>
    [UsedImplicitly]
    [MonoModCustomAttribute("RemoveMethodCall")]
    public class RemoveMethodCall : Attribute
    {
        /// <inheritdoc />
        /// <summary>
        /// Remove call to method
        /// </summary>
        /// <param name="type">Type full name</param>
        /// <param name="method">Method name</param>
        public RemoveMethodCall(string type, string method) {}
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
        public static void RemoveMethodCall(MethodDefinition method, CustomAttribute attrib)
        {
            var cursor = new ILCursor(new ILContext(method));

            bool did_patch = false;

            string typeName = (string) attrib.ConstructorArguments[0].Value;
            string methodName = (string) attrib.ConstructorArguments[1].Value;
            
            while (cursor.TryGotoNext(x => x.MatchCallOrCallvirt(typeName, methodName)))
            {
                cursor.Remove();

                did_patch = true;
            }

            if (!did_patch)
                throw new MissingMethodException("No method call found in method!");
        }
    }
}