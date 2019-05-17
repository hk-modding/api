using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;

namespace Modding.Patches
{
    /// <inheritdoc />
    /// <summary>
    /// MonoMod attribute for replacing a method call
    /// </summary>
    [UsedImplicitly]
    [MonoModCustomAttribute("ReplaceMethod")]
    public class ReplaceMethod : Attribute
    {
        /// <inheritdoc />
        /// <summary>
        /// Replace method call with alternate method call
        /// </summary>
        public ReplaceMethod(string type1, string method, string[] params1, string type2, string method2, string[] params2) {}
    }
}

namespace MonoMod
{
    public static partial class MonoModRules
    {
        private const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        
        /// <summary>
        /// Replace method call with alternate method call
        /// </summary>
        /// <param name="method">Method to be patched</param>
        /// <param name="attrib">Attribute ReplaceWithNop</param>
        [UsedImplicitly]
        public static void ReplaceMethod(MethodDefinition method, CustomAttribute attrib)
        {
            string typeName1 = (string) attrib.ConstructorArguments[0].Value;
            string methodName1 = (string) attrib.ConstructorArguments[1].Value;
            Type[] params1 = ((CustomAttributeArgument[]) attrib.ConstructorArguments[2].Value).Select(x => Type.GetType((string) x.Value)).ToArray();
            
            string typeName2 = (string) attrib.ConstructorArguments[3].Value;
            string methodName2 = (string) attrib.ConstructorArguments[4].Value;
            Type[] params2 = ((CustomAttributeArgument[]) attrib.ConstructorArguments[5].Value).Select(x => Type.GetType((string) x.Value)).ToArray();

            Type t1 = Type.GetType(typeName1);
            Type t2 = Type.GetType(typeName2);
            
            MethodReference from = method.Module.ImportReference(t1.GetMethod(methodName1, all, null, params1, null));
            MethodReference to   = method.Module.ImportReference(t2.GetMethod(methodName2, all, null, params2, null));
            
            Instruction call = method.Body.GetILProcessor().Create(to.HasThis ? OpCodes.Callvirt : OpCodes.Calli, to);
            
            for (int i = method.Body.Instructions.Count - 1; i >= 0; i--)
            {
                Instruction instr = method.Body.Instructions[i];

                if (instr.OpCode != OpCodes.Calli && instr.OpCode != OpCodes.Callvirt && instr.OpCode != OpCodes.Call)
                {
                    continue;
                }

                MethodReference mref = (MethodReference) instr.Operand;

                if (mref.FullName != from.FullName)
                {
                    continue;
                }

                method.Body.Instructions.RemoveAt(i);
                method.Body.Instructions.Insert(i, call);

                break;
            }
        }
    }
}