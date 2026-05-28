using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace Prepatcher
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Program
    {
        private static void Main(string[] args)
        {
            // --mmhook mode: add type forwarders from one MMHOOK assembly into another
            if (args.Length >= 1 && args[0] == "--mmhook")
            {
                if (args.Length < 3)
                {
                    Console.WriteLine("Usage: PrePatcher.exe --mmhook <target-mmhook.dll> <source-mmhook.dll>");
                    return;
                }
                PatchMMHook(args[1], args[2]);
                return;
            }

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: PrePatcher.exe <Original> <Patched>");
                return;
            }

            int changes = 0;

            using ModuleDefinition module = ModuleDefinition.ReadModule(args[0]);
            
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

            foreach (TypeDefinition type in module.Types.Where(type => type.HasMethods))
            {
                foreach (MethodDefinition method in GetMethodsRecursively(type))
                {
                    if
                    (
                        !method.HasBody
                        || method.DeclaringType == pd
                        && (method.Name == "SetupNewPlayerData" || method.Name == "AddGGPlayerDataOverrides")
                    )
                        continue;

                    ILProcessor il = method.Body.GetILProcessor();
                    
                    // Replace short branches with normal branches, etc.
                    // This ensures that inserting instructions won't cause
                    // short branch offsets to overflow and become null
                    method.Body.SimplifyMacros();

                    bool changesFound = true;

                    while (changesFound)
                    {
                        changesFound = false;

                        foreach (Instruction instr in il.Body.Instructions)
                        {
                            if (instr.Operand is not FieldReference field || field.DeclaringType != pd)
                                continue;

                            if (instr.OpCode == OpCodes.Ldfld)
                            {
                                SwapLdFld
                                (
                                    field,
                                    module,
                                    pdGetBool,
                                    pdGetFloat,
                                    pdGetInt,
                                    pdGetString,
                                    pdGetVector3,
                                    pdGetVariable,
                                    instr,
                                    il
                                );

                                changes++;
                                changesFound = true;

                                break;
                            }

                            // ReSharper disable once InvertIf
                            if (instr.OpCode == OpCodes.Stfld)
                            {
                                SwapStFld
                                (
                                    field,
                                    module,
                                    setBoolSwappedArgs,
                                    setFloatSwappedArgs,
                                    setIntSwappedArgs,
                                    setStringSwappedArgs,
                                    setVector3SwappedArgs,
                                    setVariableSwappedArgs,
                                    instr,
                                    il
                                );

                                changes++;
                                changesFound = true;

                                break;
                            }
                        }
                    }
                    
                    // After inserting instructions, replace branches
                    // with short branches where possible for optimization
                    method.Body.OptimizeMacros();
                }
            }

            // Add type forwarders for types that moved from Assembly-CSharp to TeamCherry.*.dll in Unity 6.
            // This covers TeamCherry.TK2D (tk2d sprite system) and TeamCherry.Cinematics (video player).
            // This allows mods compiled against the old API to load without TypeLoadException.
            string baseDir = Path.GetDirectoryName(Path.GetFullPath(args[0]));
            string[] teamCherryDlls = { "TeamCherry.TK2D.dll", "TeamCherry.Cinematics.dll" };
            foreach (string dllName in teamCherryDlls)
            {
                string dllPath = Path.Combine(baseDir, dllName);
                if (!File.Exists(dllPath))
                {
                    Console.WriteLine($"{dllName} not found, skipping type forwarders");
                    continue;
                }
                using ModuleDefinition tcModule = ModuleDefinition.ReadModule(dllPath);
                var tcRef = new AssemblyNameReference(
                    tcModule.Assembly.Name.Name,
                    tcModule.Assembly.Name.Version
                );
                module.AssemblyReferences.Add(tcRef);

                int forwardersAdded = 0;
                foreach (TypeDefinition type in tcModule.Types)
                {
                    if (!type.IsPublic) continue;
                    if (module.GetType(type.Namespace, type.Name) != null) continue;
                    if (module.ExportedTypes.Any(e => e.Namespace == type.Namespace && e.Name == type.Name)) continue;

                    var exported = new ExportedType(type.Namespace, type.Name, module, tcRef);
                    exported.Attributes = Mono.Cecil.TypeAttributes.Forwarder;
                    module.ExportedTypes.Add(exported);
                    forwardersAdded++;
                }
                Console.WriteLine($"Added {forwardersAdded} type forwarders to {dllName}");
            }

            module.Write(args[1]);

            Console.WriteLine("Changed " + changes + " get/set calls");
        }

        /// <summary>
        /// Yields all methods defined on the given type, or a (recursively) nested type within the given type
        /// </summary>
        private static IEnumerable<MethodDefinition> GetMethodsRecursively(TypeDefinition type)
        {
            if (type.HasMethods)
            {
                foreach (MethodDefinition method in type.Methods)
                {
                    yield return method;
                }
            }

            if (type.HasNestedTypes)
            {
                foreach (TypeDefinition nested in type.NestedTypes)
                {
                    foreach (MethodDefinition method in GetMethodsRecursively(nested))
                    {
                        yield return method;
                    }
                }
            }
        }

        private static void SwapStFld
        (
            FieldReference field,
            ModuleDefinition module,
            MethodReference setBoolSwappedArgs,
            MethodReference setFloatSwappedArgs,
            MethodReference setIntSwappedArgs,
            MethodReference setStringSwappedArgs,
            MethodReference setVector3SwappedArgs,
            MethodReference setVariableSwappedArgs,
            Instruction instr,
            ILProcessor il
        )
        {
            var ldstr = Instruction.Create(OpCodes.Ldstr, field.Name);

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
                var generic = new GenericInstanceMethod(setVariableSwappedArgs);
                generic.GenericArguments.Add(field.FieldType);
                callSet = Instruction.Create(OpCodes.Callvirt, generic);
            }
            
            il.InsertAfter(instr, callSet);

            instr.OpCode = ldstr.OpCode;
            instr.Operand = ldstr.Operand;
        }

        private static void SwapLdFld
        (
            FieldReference field,
            ModuleDefinition module,
            MethodReference pdGetBool,
            MethodReference pdGetFloat,
            MethodReference pdGetInt,
            MethodReference pdGetString,
            MethodReference pdGetVector3,
            MethodReference pdGetVariable,
            Instruction instr,
            ILProcessor il
        )
        {
            var ldstr = Instruction.Create(OpCodes.Ldstr, field.Name);

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
                var generic = new GenericInstanceMethod(pdGetVariable);
                generic.GenericArguments.Add(field.FieldType);
                callGet = Instruction.Create(OpCodes.Callvirt, generic);
            }
            
            il.InsertAfter(instr, callGet);

            instr.OpCode = ldstr.OpCode;
            instr.Operand = ldstr.Operand;
        }

        // Adds type forwarders into targetPath for all public types in sourcePath that don't already exist in target.
        // Used to forward On.tk2dSprite etc. from MMHOOK_Assembly-CSharp → MMHOOK_TeamCherry.TK2D.
        private static void PatchMMHook(string targetPath, string sourcePath)
        {
            try
            {
                string targetFull = Path.GetFullPath(targetPath);
                string sourceFull = Path.GetFullPath(sourcePath);

                if (!File.Exists(sourceFull))
                {
                    Console.WriteLine($"Source MMHOOK not found: {sourceFull}, skipping.");
                    return;
                }
                if (!File.Exists(targetFull))
                {
                    Console.WriteLine($"Target MMHOOK not found: {targetFull}, skipping.");
                    return;
                }

                var resolver = new DefaultAssemblyResolver();
                resolver.AddSearchDirectory(Path.GetDirectoryName(targetFull));
                var readerParams = new ReaderParameters { AssemblyResolver = resolver };

                string tempPath = targetFull + ".tmp";
                int added = 0;

                // Scope the using blocks so files are released before we rename
                using (ModuleDefinition target = ModuleDefinition.ReadModule(targetFull, readerParams))
                using (ModuleDefinition source = ModuleDefinition.ReadModule(sourceFull, readerParams))
                {
                    var sourceRef = new AssemblyNameReference(
                        source.Assembly.Name.Name,
                        source.Assembly.Name.Version
                    );
                    if (!target.AssemblyReferences.Any(r => r.Name == sourceRef.Name))
                        target.AssemblyReferences.Add(sourceRef);

                    foreach (TypeDefinition type in source.Types)
                    {
                        if (!type.IsPublic) continue;
                        if (target.GetType(type.Namespace, type.Name) != null) continue;
                        if (target.ExportedTypes.Any(e => e.Namespace == type.Namespace && e.Name == type.Name)) continue;

                        var exported = new ExportedType(type.Namespace, type.Name, target, sourceRef);
                        exported.Attributes = Mono.Cecil.TypeAttributes.Forwarder;
                        target.ExportedTypes.Add(exported);
                        added++;
                    }

                    target.Write(tempPath);
                }

                // Both modules are now closed; safely replace the target
                File.Delete(targetFull);
                File.Move(tempPath, targetFull);
                Console.WriteLine($"Added {added} type forwarders from {Path.GetFileName(sourceFull)} into {Path.GetFileName(targetFull)}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"PatchMMHook failed: {ex}");
                Environment.Exit(1);
            }
        }

        private static MethodDefinition GenerateSwappedMethod(TypeDefinition methodParent, MethodReference oldMethod)
        {
            MethodDefinition swapped = new
            (
                oldMethod.Name + "SwappedArgs",
                MethodAttributes.Assembly | MethodAttributes.HideBySig,
                methodParent.Module.TypeSystem.Void
            );

            ParameterDefinition[] @params = oldMethod.Parameters.ToArray();

            Debug.Assert(@params.Length == 2);

            swapped.Parameters.Add
            (
                new ParameterDefinition(@params[1].ParameterType)
                {
                    Name = "value"
                }
            );
            swapped.Parameters.Add
            (
                new ParameterDefinition(@params[0].ParameterType)
                {
                    Name = "name"
                }
            );

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
                GenericInstanceMethod generic = new(oldMethod);

                foreach (GenericParameter param in swapped.GenericParameters)
                    generic.GenericArguments.Add(param);

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