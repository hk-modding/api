using System.Collections.Generic;
using JetBrains.Annotations;

namespace Modding
{
    /// <summary>
    /// Used for Deconstruct when not given by .NET 
    /// </summary>
    [PublicAPI]
    public static class DeconstructUtil
    {
        /// <summary>
        /// Deconstructs a KeyValuePair into key and value
        /// </summary>
        /// <param name="self">The KeyValuePair</param>
        /// <param name="key">Output key</param>
        /// <param name="value">Output value</param>
        /// <typeparam name="TKey">Type of KeyValuePair Key</typeparam>
        /// <typeparam name="TValue">Type of KeyValuePair Value</typeparam>
        // ReSharper disable once UseDeconstructionOnParameter
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> self, out TKey key, out TValue value)
        {
            key = self.Key;
            value = self.Value;
        }
    }
}