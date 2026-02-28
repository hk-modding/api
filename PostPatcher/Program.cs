using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MethodAttributes = Mono.Cecil.MethodAttributes;

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

            int changes = 0;

            using AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(args[0]);

            ForwardTk2dTypes(assembly);

            assembly.Write(args[1]);

            Console.WriteLine("Changed " + changes + " get/set calls");
        }

        private static void ForwardTk2dTypes(AssemblyDefinition assembly)
        {
            AssemblyNameReference assemblyNameReference = new AssemblyNameReference("TeamCherry.TK2D", null);
            foreach (var typeName in new string[]
                     {
                         "tk2dAnimatedSprite", "tk2dAssetPlatform", "tk2dBaseSprite", "tk2dBatchedSprite", "tk2dButton", "tk2dCamera", "tk2dCameraAnchor",
                         "tk2dCameraResolutionOverride", "tk2dCameraSettings", "tk2dClippedSprite", "tk2dCollider2DData", "tk2dEditorSpriteDataUnloader",
                         "tk2dFont", "tk2dFontChar", "tk2dFontData", "tk2dFontKerning", "Tk2dGlobalEvents", "tk2dLinkedSpriteCollection",
                         "tk2dPixelPerfectHelper", "tk2dResource", "tk2dResourceTocEntry", "tk2dSlicedSprite", "tk2dSprite", "tk2dSpriteAnimation",
                         "tk2dSpriteAnimationClip", "tk2dSpriteAnimationFrame", "tk2dSpriteAnimator", "tk2dSpriteAttachPoint", "tk2dSpriteCollection",
                         "tk2dSpriteCollectionData", "tk2dSpriteCollectionDefault", "tk2dSpriteCollectionDefinition", "tk2dSpriteCollectionFont",
                         "tk2dSpriteCollectionPlatform", "tk2dSpriteCollectionSize", "tk2dSpriteColliderDefinition", "tk2dSpriteColliderIsland",
                         "tk2dSpriteDefinition", "tk2dSpriteFromTexture", "tk2dSpriteGeomGen", "Tk2dSpriteSetKeywords", "tk2dSpriteSheetSource",
                         "tk2dStaticSpriteBatcher", "tk2dSystem", "tk2dTextGeomGen", "tk2dTextMesh", "tk2dTextMeshData", "tk2dTiledSprite", "tk2dTileFlags",
                         "tk2dTileMap", "tk2dTileMapData", "tk2dUpdateManager", "tk2dUtil"
                     })
            {
                var forwardedType = assembly.MainModule.ImportReference
                (
                    assembly.MainModule.AssemblyResolver
                            .Resolve(assemblyNameReference)
                            .MainModule.GetType(typeName)
                );
                assembly.MainModule.ExportedTypes.Add
                (
                    new ExportedType
                    (
                        "",
                        typeName,
                        assembly.MainModule,
                        assembly.Name
                    )
                    {
                        Attributes = TypeAttributes.Public | TypeAttributes.Forwarder,
                        Scope = forwardedType.Scope
                    }
                );
            }
        }
    }
}