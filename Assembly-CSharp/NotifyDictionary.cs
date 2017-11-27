using System.Collections.Generic;

namespace Modding
{
    /// <inheritdoc />
    /// <summary>
    /// Dictionary with events to notify when something has been added or removed
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class NotifyDictionary<TKey, TValue>  :Dictionary<TKey, TValue>
    {
        public delegate void OnAddHandler(TKey key, TValue value);

        private event OnAddHandler _OnAddHook;

        /// <summary>
        /// Called After the element was added to the dictionary
        /// </summary>
        public event OnAddHandler OnAddHook
        {
            add
            {
                ModHooks.Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Adding {GetType().Name}.NotifyDictionaryHook");
                _OnAddHook += value;

            }
            remove
            {
                ModHooks.Logger.LogDebug($"[{value.Method.DeclaringType?.Name}] - Removing {GetType().Name}.ApplicationQuitHook");
                _OnAddHook -= value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            _OnAddHook?.Invoke(key, value);
        }
    }
}

