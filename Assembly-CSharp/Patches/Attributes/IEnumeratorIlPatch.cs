using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;
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
    [MonoModCustomAttribute("IEnumeratorIlPatch")]
    public class IEnumeratorIlPatch : Attribute
    {
        /// <inheritdoc />
        /// <summary>
        /// Patches a method directly with IL
        /// </summary>
        /// <param name="patcherMethod">Method name that does the IL patch</param>
        public IEnumeratorIlPatch(string patcherMethod) { }
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
        public static void IEnumeratorIlPatch(MethodDefinition method, CustomAttribute attrib)
        {
            CustomAttribute iteratorAttribute = method.CustomAttributes.First
                (x => x.AttributeType.FullName == "System.Runtime.CompilerServices.IteratorStateMachineAttribute");
            TypeReference stateMachineTypeRef = (TypeReference)iteratorAttribute.ConstructorArguments[0].Value;
            TypeDefinition stateMachineTypeDef = stateMachineTypeRef.Resolve();
            MethodDefinition stateMachineMoveNext = stateMachineTypeDef.Methods.First(m => m.Name == "MoveNext");
            var context = new ILContext(stateMachineMoveNext);

            string patcherTypeName = $"Modding.Patches.{nameof(Modding.Patches.IlPatches)}, Assembly-CSharp.mm";
            string patcherMethodName = (string)attrib.ConstructorArguments[0].Value;

            Type patcherType = Type.GetType(patcherTypeName);

            if (patcherType is null)
                throw new InvalidOperationException("Couldn't find patcher type!");

            MethodBase patcherMethod = patcherType?.GetMethod(patcherMethodName, AllBindingFlags/*, null, [typeof(ILContext)], null*/);

            if (patcherMethod is null)
                throw new InvalidOperationException("Couldn't find patcher method!");

            context.Invoke(delegate(ILContext ctx)
            {
                patcherMethod.Invoke(null, [ctx, stateMachineTypeDef]);
            });
        }
    }
}