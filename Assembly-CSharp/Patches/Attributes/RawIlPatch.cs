using System;
using System.Reflection;
using JetBrains.Annotations;
using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace Modding.Patches.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// MonoMod attribute for patching a method directly with IL
    /// </summary>
    [UsedImplicitly]
    [MonoModCustomAttribute("RawIlPatch")]
    public class RawIlPatch : Attribute
    {
        /// <inheritdoc />
        /// <summary>
        /// Patches a method directly with IL
        /// </summary>
        /// <param name="patcherMethod">Method name that does the IL patch</param>
        public RawIlPatch(string patcherMethod) { }
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

            string patcherTypeName = $"Modding.Patches.{nameof(Modding.Patches.IlPatches)}, Assembly-CSharp.mm";
            string patcherMethodName = (string)attrib.ConstructorArguments[0].Value;

            Type patcherType = Type.GetType(patcherTypeName);

            if (patcherType is null)
                throw new InvalidOperationException("Couldn't find patcher type!");

            MethodBase patcherMethod = patcherType?.GetMethod(patcherMethodName, AllBindingFlags/*, null, [typeof(ILContext)], null*/);

            if (patcherMethod is null)
                throw new InvalidOperationException("Couldn't find patcher method!");

            context.Invoke(patcherMethod.CreateDelegate<ILContext.Manipulator>());
        }
    }
}