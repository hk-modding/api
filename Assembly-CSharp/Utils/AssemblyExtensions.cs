using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Modding.Utils
{
    /// <summary>
    /// Class containing extensions used by the Modding API for interacting with assemblies.
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Returns a collection containing the types in the provided assembly.
        /// If some types cannot be loaded (e.g. they derive from a type in an uninstalled mod),
        /// then only the successfully loaded types are returned.
        /// </summary>
        public static IEnumerable<Type> GetTypesSafely(this Assembly asm)
        {
            try
            {
                return asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(x => x is not null);
            }
        }

        /// <summary>
        /// Load an image from the assembly's embedded resources, and return a Sprite.
        /// </summary>
        /// <param name="asm">The assembly to load from.</param>
        /// <param name="path">The path to the image.</param>
        /// <param name="pixelsPerUnit">The pixels per unit. Changing this value will scale the size of the sprite accordingly.</param>
        /// <returns>A Sprite object.</returns>
        public static Sprite LoadEmbeddedSprite(this Assembly asm, string path, float pixelsPerUnit = 64f)
        {
            using var stream = asm.GetManifestResourceStream(path);

            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            var tex = new Texture2D(2, 2);

            tex.LoadImage(buffer, true);

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, pixelsPerUnit);
        }
    }
}
