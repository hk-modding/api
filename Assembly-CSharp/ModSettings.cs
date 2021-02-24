using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

namespace Modding
{
    /// <summary>
    ///     Base class for storing settings for a Mod in the save file.
    /// </summary>
    [Serializable]
    [PublicAPI]
    public abstract class ModSettings { }
}