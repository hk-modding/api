using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
// ReSharper disable SuggestVarOrType_SimpleTypes

namespace Prepatcher
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: PrePatcher.exe <Original> <Patched>");
                return;
            }

            int changes = 0;

            using (ModuleDefinition module = ModuleDefinition.ReadModule(args[0]))
            {
                TypeDefinition pd = module.GetType("", "PlayerData");

                MethodDefinition pdGetBool = pd.Methods.First(method => method.Name == "GetBool");
                MethodDefinition pdSetBool = pd.Methods.First(method => method.Name == "SetBool");

                MethodDefinition pdGetFloat = pd.Methods.First(method => method.Name == "GetFloat");
                MethodDefinition pdSetFloat = pd.Methods.First(method => method.Name == "SetFloat");

                MethodDefinition pdGetInt = pd.Methods.First(method => method.Name == "GetInt");
                MethodDefinition pdSetInt = pd.Methods.First(method => method.Name == "SetInt");

                MethodDefinition pdGetString = pd.Methods.First(method => method.Name == "GetString");
                MethodDefinition pdSetString = pd.Methods.First(method => method.Name == "SetString");

                MethodDefinition pdGetVector3 = pd.Methods.First(method => method.Name == "GetVector3");
                MethodDefinition pdSetVector3 = pd.Methods.First(method => method.Name == "SetVector3");

                MethodDefinition pdGetVariable = pd.Methods.First(method => method.Name == "GetVariable");
                MethodDefinition pdSetVariable = pd.Methods.First(method => method.Name == "SetVariable");
                
                MethodDefinition setBoolSwappedArgs = GenerateSwappedMethod(pd, pdSetBool);
                MethodDefinition setFloatSwappedArgs = GenerateSwappedMethod(pd, pdSetFloat);
                MethodDefinition setIntSwappedArgs = GenerateSwappedMethod(pd, pdSetInt);
                MethodDefinition setStringSwappedArgs = GenerateSwappedMethod(pd, pdSetString);
                MethodDefinition setVector3SwappedArgs = GenerateSwappedMethod(pd, pdSetVector3);
                MethodDefinition setVariableSwappedArgs = GenerateSwappedMethod(pd, pdSetVariable);

                foreach (TypeDefinition type in module.Types)
                {
                    if (!type.HasMethods)
                    {
                        continue;
                    }

                    foreach (MethodDefinition method in type.Methods)
                    {
                        if (!method.HasBody || (method.DeclaringType == pd && (method.Name == "SetupNewPlayerData" || method.Name == "AddGGPlayerDataOverrides")))
                        {
                            continue;
                        }

                        ILProcessor il = method.Body.GetILProcessor();

                        bool changesFound = true;
                        while (changesFound)
                        {
                            changesFound = false;

                            foreach (Instruction instr in il.Body.Instructions)
                            {
                                if (instr.OpCode == OpCodes.Ldfld)
                                {
                                    FieldReference field = (FieldReference)instr.Operand;
                                    if (field.DeclaringType != pd)
                                    {
                                        continue;
                                    }

                                    Instruction ldstr = Instruction.Create(OpCodes.Ldstr, field.Name);
                                    Instruction callGet;

                                    if (field.FieldType == module.TypeSystem.Boolean)
                                    {
                                        callGet = Instruction.Create(OpCodes.Callvirt, pdGetBool);
                                    }
                                    else if (field.FieldType == module.TypeSystem.Single)
                                    {
                                        callGet = Instruction.Create(OpCodes.Callvirt, pdGetFloat);
                                    }
                                    else if (field.FieldType == module.TypeSystem.Int32)
                                    {
                                        callGet = Instruction.Create(OpCodes.Callvirt, pdGetInt);
                                    }
                                    else if (field.FieldType == module.TypeSystem.String)
                                    {
                                        callGet = Instruction.Create(OpCodes.Callvirt, pdGetString);
                                    }
                                    else if (field.FieldType.Name == "Vector3")
                                    {
                                        callGet = Instruction.Create(OpCodes.Callvirt, pdGetVector3);
                                    }
                                    else
                                    {
                                        GenericInstanceMethod generic = new GenericInstanceMethod(pdGetVariable);
                                        generic.GenericArguments.Add(field.FieldType);
                                        callGet = Instruction.Create(OpCodes.Callvirt, generic);
                                    }

                                    instr.OpCode = callGet.OpCode;
                                    instr.Operand = callGet.Operand;
                                    il.InsertBefore(instr, ldstr);

                                    changes++;
                                    changesFound = true;
                                    break;
                                }
                                else if (instr.OpCode == OpCodes.Stfld)
                                {
                                    FieldReference field = (FieldReference)instr.Operand;
                                    if (field.DeclaringType != pd)
                                    {
                                        continue;
                                    }

                                    Instruction ldstr = Instruction.Create(OpCodes.Ldstr, field.Name);
                                    Instruction callSet;

                                    if (field.FieldType == module.TypeSystem.Boolean)
                                    {
                                        callSet = Instruction.Create(OpCodes.Callvirt, setBoolSwappedArgs);
                                    }
                                    else if (field.FieldType == module.TypeSystem.Single)
                                    {
                                        callSet = Instruction.Create(OpCodes.Callvirt, setFloatSwappedArgs);
                                    }
                                    else if (field.FieldType == module.TypeSystem.Int32)
                                    {
                                        callSet = Instruction.Create(OpCodes.Callvirt, setIntSwappedArgs);
                                    }
                                    else if (field.FieldType == module.TypeSystem.String)
                                    {
                                        callSet = Instruction.Create(OpCodes.Callvirt, setStringSwappedArgs);
                                    }
                                    else if (field.FieldType.Name == "Vector3")
                                    {
                                        callSet = Instruction.Create(OpCodes.Callvirt, setVector3SwappedArgs);
                                    }
                                    else
                                    {
                                        GenericInstanceMethod generic = new GenericInstanceMethod(setVariableSwappedArgs);
                                        generic.GenericArguments.Add(field.FieldType);
                                        callSet = Instruction.Create(OpCodes.Callvirt, generic);
                                    }

                                    instr.OpCode = callSet.OpCode;
                                    instr.Operand = callSet.Operand;
                                    il.InsertBefore(instr, ldstr);

                                    changes++;
                                    changesFound = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                module.Write(args[1]);

                Console.WriteLine("Changed " + changes + " get/set calls");
            }
        }

        private static MethodDefinition GenerateSwappedMethod(TypeDefinition methodParent, MethodDefinition oldMethod)
        {
            MethodDefinition swapped = new MethodDefinition(oldMethod.Name + "SwappedArgs", MethodAttributes.Assembly | MethodAttributes.HideBySig, methodParent.Module.TypeSystem.Void);
            swapped.Parameters.Add(new ParameterDefinition(oldMethod.Parameters.ToArray()[1].ParameterType) { Name = "value" });
            swapped.Parameters.Add(new ParameterDefinition(oldMethod.Parameters.ToArray()[0].ParameterType) { Name = "name" });

            if (oldMethod.HasGenericParameters)
            {
                int paramCount = 0;
                foreach (GenericParameter _ in oldMethod.GenericParameters)
                {
                    swapped.GenericParameters.Add(new GenericParameter(swapped) { Name = "T" + paramCount });
                    paramCount++;
                }
            }

            ILProcessor swappedIL = swapped.Body.GetILProcessor();
            swappedIL.Emit(OpCodes.Ldarg_0);
            swappedIL.Emit(OpCodes.Ldarg_2);
            swappedIL.Emit(OpCodes.Ldarg_1);

            if (oldMethod.ContainsGenericParameter)
            {
                GenericInstanceMethod generic = new GenericInstanceMethod(oldMethod);
                foreach (GenericParameter param in swapped.GenericParameters)
                {
                    generic.GenericArguments.Add(param);
                }

                swappedIL.Emit(OpCodes.Call, generic);
            }
            else
            {
                swappedIL.Emit(OpCodes.Call, oldMethod);
            }

            swappedIL.Emit(OpCodes.Ret);
            
            methodParent.Methods.Add(swapped);

            return swapped;
        }
    }
}
