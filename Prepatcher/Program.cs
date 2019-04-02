using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Prepatcher
{
    class Program
    {
        static void Main(string[] args)
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

                MethodDefinition pdGetBool = pd.Methods.Where(method => method.Name == "GetBool").First();
                MethodDefinition pdSetBool = pd.Methods.Where(method => method.Name == "SetBool").First();

                MethodDefinition pdGetFloat = pd.Methods.Where(method => method.Name == "GetFloat").First();
                MethodDefinition pdSetFloat = pd.Methods.Where(method => method.Name == "SetFloat").First();

                MethodDefinition pdGetInt = pd.Methods.Where(method => method.Name == "GetInt").First();
                MethodDefinition pdSetInt = pd.Methods.Where(method => method.Name == "SetInt").First();

                MethodDefinition pdGetString = pd.Methods.Where(method => method.Name == "GetString").First();
                MethodDefinition pdSetString = pd.Methods.Where(method => method.Name == "SetString").First();

                MethodDefinition pdGetVector3 = pd.Methods.Where(method => method.Name == "GetVector3").First();
                MethodDefinition pdSetVector3 = pd.Methods.Where(method => method.Name == "SetVector3").First();

                MethodDefinition pdGetVariable = pd.Methods.Where(method => method.Name == "GetVariable").First();
                MethodDefinition pdSetVariable = pd.Methods.Where(method => method.Name == "SetVariable").First();
                
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

        public static MethodDefinition GenerateSwappedMethod(TypeDefinition methodParent, MethodDefinition oldMethod)
        {
            MethodDefinition swapped = new MethodDefinition(oldMethod.Name + "SwappedArgs", MethodAttributes.Assembly | MethodAttributes.HideBySig, methodParent.Module.TypeSystem.Void);
            swapped.Parameters.Add(new ParameterDefinition(oldMethod.Parameters.ToArray()[1].ParameterType) { Name = "value" });
            swapped.Parameters.Add(new ParameterDefinition(oldMethod.Parameters.ToArray()[0].ParameterType) { Name = "name" });

            if (oldMethod.HasGenericParameters)
            {
                int paramCount = 0;
                foreach (GenericParameter oldParam in oldMethod.GenericParameters)
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
