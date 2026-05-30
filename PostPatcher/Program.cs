using System;
using System.Linq;
using Mono.Cecil;

namespace Postpatcher
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: PostPatcher.exe <Original> <Patched>");
                return;
            }

            int forwarders = 0;

            using AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(args[0]);

            // forwarders += ForwardTypes(assembly, "TeamCherry.BuildBot.dll", "TeamCherry.BuildBot", "");
            forwarders += ForwardTypes(assembly, "TeamCherry.Cinematics.dll", "TeamCherry.Cinematics", "");
            forwarders += ForwardTypes(assembly, "TeamCherry.Localization.dll", "HutongGames.PlayMaker.Actions", "HutongGames.PlayMaker.Actions");
            forwarders += ForwardTypes(assembly, "TeamCherry.Localization.dll", "TeamCherry.Localization", "");
            forwarders += ForwardTypes(assembly, "TeamCherry.NestedFadeGroup.dll", "HutongGames.PlayMaker.Actions", "HutongGames.PlayMaker.Actions");
            forwarders += ForwardTypes(assembly, "TeamCherry.NestedFadeGroup.dll", "TeamCherry.NestedFadeGroup", "");
            forwarders += ForwardTypes(assembly, "TeamCherry.SharedUtils.dll", "TeamCherry.SharedUtils", "");
            forwarders += ForwardTypes(assembly, "TeamCherry.TK2D.dll", "", "");
            forwarders += ForwardTypes(assembly, "TeamCherry.TK2D.dll", "tk2dRuntime", "");
            forwarders += ForwardTypes(assembly, "TeamCherry.TK2D.dll", "tk2dRuntime.TileMap", "");

            assembly.Write(args[1]);

            Console.WriteLine("Added " + forwarders + " type forwarders");
        }

        private static int ForwardTypes(AssemblyDefinition outAssembly, string sourcePath, string fromNameSpace, string toNameSpace)
        {
            int forwarders = 0;
            using AssemblyDefinition sourceAssembly = AssemblyDefinition.ReadAssembly(sourcePath);
            AssemblyNameReference nameReference = new AssemblyNameReference(sourceAssembly.Name.Name, sourceAssembly.Name.Version);
            if (outAssembly.MainModule.AssemblyReferences.All(x => x.Name != sourceAssembly.Name.Name))
            {
                outAssembly.MainModule.AssemblyReferences.Add(nameReference);
            }
            foreach (TypeDefinition type in sourceAssembly.MainModule.Types)
            {
                if (!type.IsPublic) continue;
                if (type.Namespace != fromNameSpace) continue;
                if (outAssembly.MainModule.GetType(type.Namespace, type.Name) != null) continue;
                if (outAssembly.MainModule.ExportedTypes.Any(e => e.Namespace == type.Namespace && e.Name == type.Name)) continue;
                var forwardedType = outAssembly.MainModule.ImportReference(type);
                outAssembly.MainModule.ExportedTypes.Add
                (
                    new ExportedType
                    (
                        toNameSpace,
                        type.Name,
                        outAssembly.MainModule,
                        outAssembly.Name
                    )
                    {
                        Attributes = TypeAttributes.Public | TypeAttributes.Forwarder,
                        Scope = forwardedType.Scope
                    }
                );
                forwarders++;
            }

            return forwarders;
        }
    }
}